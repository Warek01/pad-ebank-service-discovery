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

   public RegistryService(
      IHttpClientFactory httpClientFactory,
      ILogger<RegistryService> logger,
      IConnectionMultiplexer connectionMultiplexer
   ) {
      _redis = connectionMultiplexer.GetDatabase();
      _logger = logger;
      _httpClient = httpClientFactory.CreateClient();

      int timeout = int.Parse(Environment.GetEnvironmentVariable("HEALTHCHECK_REQUEST_TIMEOUT")!);
      _httpClient.Timeout = TimeSpan.FromSeconds(timeout);
   }

   public void Dispose() {
      _httpClient.Dispose();
      GC.SuppressFinalize(this);
   }

   public bool HasEntry(RegistryEntry entry) {
      lock (_registryLock) {
         return _registry.Contains(entry);
      }
   }

   public void Add(RegistryEntry entry) {
      lock (_registryLock) {
         _registry.Add(entry);
      }

      _ = ScheduleHealthChecks(entry);
   }

   public List<RegistryEntry> GetByName(string name) {
      lock (_registryLock) {
         return _registry.FindAll(entry => entry.Name == name);
      }
   }

   public RegistryEntry? GetById(string id) {
      lock (_registryLock) {
         return _registry.FirstOrDefault(entry => entry.Id == id);
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

   private async Task ScheduleHealthChecks(RegistryEntry entry) {
      int retries = int.Parse(Environment.GetEnvironmentVariable("HEALTHCHECK_REQUEST_RETRIES")!);
      int retryDelay = int.Parse(Environment.GetEnvironmentVariable("HEALTHCHECK_REQUEST_RETRY_DELAY")!);
      int retiresLeft = retries;
      bool previouslyFailed = false;

      while (retiresLeft >= 0 && !_cts.Token.IsCancellationRequested) {
         TimeSpan waitFor = TimeSpan.FromSeconds(previouslyFailed ? retryDelay : entry.HealthCheckInterval);
         await Task.Delay(waitFor, _cts.Token);

         if (!HasEntry(entry)) {
            return;
         }

         try {
            HttpResponseMessage res = await _httpClient.GetAsync(entry.HealthCheckUri, _cts.Token);
            res.EnsureSuccessStatusCode();
            previouslyFailed = false;
            retiresLeft = retries;
         }
         catch (HttpRequestException e) {
            _logger.LogInformation($"Health check error for {entry}; Retries left: {retiresLeft}; Reason: {e.Message}");
            retiresLeft--;
            previouslyFailed = true;
         }
      }

      await Remove(entry);
      _logger.LogInformation($"Service {entry} removed from registry due to failed health checks");
   }

   public bool HasServiceByName(string name) {
      lock (_registryLock) {
         return _registry.Any(entry => entry.Name == name);
      }
   }
   
   public bool HasServiceById(string id) {
      lock (_registryLock) {
         return _registry.Any(entry => entry.Id == id);
      }
   }
}
