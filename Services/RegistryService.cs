using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using ServiceDiscovery.Helpers;
using ServiceDiscovery.Models;
using StackExchange.Redis;

namespace ServiceDiscovery.Services;

public class RegistryService : IDisposable {
  private readonly IDatabase _redis;
  private readonly List<RegistryEntry> _registry = [];
  private readonly object _registryLock = new();
  private readonly CancellationTokenSource _cts = new();
  private readonly ILogger _logger;
  private readonly HttpClient _httpClient;
  private readonly JsonOptions _jsonOptions;

  public RegistryService(
    IHttpClientFactory httpClientFactory,
    ILogger<RegistryService> logger,
    IOptions<JsonOptions> jsonOptions,
    IConnectionMultiplexer connectionMultiplexer,
    IHostApplicationLifetime applicationLifetime
  ) {
    _redis = connectionMultiplexer.GetDatabase();
    _logger = logger;
    _jsonOptions = jsonOptions.Value;
    _httpClient = httpClientFactory.CreateClient();

    int timeout = int.Parse(Environment.GetEnvironmentVariable("HEALTHCHECK_REQUEST_TIMEOUT")!);
    _httpClient.Timeout = TimeSpan.FromSeconds(timeout);

    // TODO: Fix not firing in docker
    applicationLifetime.ApplicationStopping.Register(() => {
      _logger.LogInformation("Shutting down registry service");
      _cts.Cancel();
      _redis.KeyExpire(CacheKeys.Registry, TimeSpan.FromSeconds(60));
    });
  }

  public void Dispose() {
    _httpClient.Dispose();
    GC.SuppressFinalize(this);
  }

  public bool Has(string url) {
    lock (_registryLock) {
      return _registry.Any(entry => entry.Url == url);
    }
  }

  public bool Has(RegistryEntry entry) {
    lock (_registryLock) {
      return _registry.Contains(entry);
    }
  }

  public async Task Add(RegistryEntry entry) {
    string cached;

    lock (_registryLock) {
      _registry.Add(entry);
      cached = JsonSerializer.Serialize(_registry, _jsonOptions.SerializerOptions);
    }

    _ = ScheduleHealthChecks(entry);
    await _redis.StringSetAsync(CacheKeys.Registry, cached);
  }

  public List<RegistryEntry> GetByName(string name) {
    lock (_registryLock) {
      return _registry.FindAll(entry => entry.Name == name);
    }
  }

  public RegistryEntry? GetByUrl(string url) {
    lock (_registryLock) {
      return _registry.FirstOrDefault(entry => entry.Url == url);
    }
  }

  public async Task Remove(RegistryEntry entry) {
    lock (_registryLock) {
      _registry.Remove(entry);
    }

    await _redis.KeyDeleteAsync(CacheKeys.DashboardInstanceDtos);
  }

  public List<RegistryEntry> GetAll() {
    lock (_registryLock) {
      return [.._registry];
    }
  }

  public async Task StartFromCache() {
    string? registryStr = await _redis.StringGetAsync(CacheKeys.Registry);

    if (registryStr is null) {
      return;
    }

    byte[] bytes = Encoding.UTF8.GetBytes(registryStr);
    using var stream = new MemoryStream(bytes);

    List<RegistryEntry> data =
      await JsonSerializer.DeserializeAsync<List<RegistryEntry>>(stream, _jsonOptions.SerializerOptions) ?? [];

    foreach (RegistryEntry entry in data) {
      lock (_registryLock) {
        _registry.Add(entry);
      }

      _ = ScheduleHealthChecks(entry);
    }
  }

  private async Task ScheduleHealthChecks(RegistryEntry entry) {
    string name = $"{entry.Url} ({entry.Name})";
    int retries = int.Parse(Environment.GetEnvironmentVariable("HEALTHCHECK_REQUEST_RETRIES")!);
    int retryDelay = int.Parse(Environment.GetEnvironmentVariable("HEALTHCHECK_REQUEST_RETRY_DELAY")!);
    int retiresLeft = retries;
    bool fail = false;

    while (retiresLeft >= 0 && !_cts.Token.IsCancellationRequested) {
      TimeSpan waitFor = TimeSpan.FromSeconds(fail ? retryDelay : entry.HealthCheck.Interval);
      await Task.Delay(waitFor, _cts.Token);

      if (!Has(entry)) {
        return;
      }

      try {
        HttpResponseMessage res = await _httpClient.GetAsync(new Uri(entry.HealthCheck.Url), _cts.Token);
        res.EnsureSuccessStatusCode();
        fail = false;
        retiresLeft = retries;
      }
      catch (HttpRequestException e) {
        _logger.LogInformation($"Health check error for {name}; Retries left: {retiresLeft}; Reason: {e.Message}");
        retiresLeft--;
        fail = true;
      }
    }

    await Remove(entry);
    _logger.LogInformation($"Service {name} removed from registry due to failed health checks");
  }
}
