using ServiceDiscovery.Models;

namespace ServiceDiscovery.Services;

public class HealthCheckService(
  RegistryService registryService,
  IHttpClientFactory httpClientFactory,
  ILogger<HealthCheckService> logger
) {
  public async Task ScheduleHealthChecks(RegistryEntry entry) {
    HttpClient httpClient = httpClientFactory.CreateClient();
    var timeout = int.Parse(Environment.GetEnvironmentVariable("HEALTHCHECK_REQUEST_TIMEOUT")!);
    httpClient.Timeout = TimeSpan.FromSeconds(timeout);

    var name = $"{entry.Id} ({entry.Name})";
    var retries = int.Parse(Environment.GetEnvironmentVariable("HEALTHCHECK_REQUEST_RETRIES")!);
    var retryDelay = int.Parse(Environment.GetEnvironmentVariable("HEALTHCHECK_REQUEST_RETRY_DELAY")!);
    var retiresLeft = retries;
    var fail = false;

    while (retiresLeft >= 0) {
      var waitFor = TimeSpan.FromSeconds(fail ? retryDelay : entry.HealthCheck.Interval);
      await Task.Delay(waitFor);

      if (!registryService.Has(entry)) {
        return;
      }

      try {
        HttpResponseMessage res = await httpClient.GetAsync(new Uri(entry.HealthCheck.Url));
        res.EnsureSuccessStatusCode();
        fail = false;
        retiresLeft = retries;
      }
      catch (HttpRequestException e) {
        logger.LogInformation($"Health check error for {name}; Retries left: {retiresLeft}; Reason: {e.Message}");
        retiresLeft--;
        fail = true;
      }
    }

    registryService.Remove(entry);
    logger.LogInformation($"Service {name} removed from registry due to failed health checks");
  }
}
