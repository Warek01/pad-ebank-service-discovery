using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using ServiceDiscovery.Models;

namespace ServiceDiscovery.Services;

public class LoadBalancingService(
   RegistryService registryService,
   IOptions<JsonOptions> jsonOptions,
   IHttpClientFactory httpClientFactory,
   ILogger<LoadBalancingService> logger
) {
   private readonly HttpClient _httpClient = httpClientFactory.CreateClient();

   public async Task<RegistryEntry?> GetServiceInstance(string name) {
      List<RegistryEntry> entries = registryService.GetByName(name);

      if (entries.Count == 0) {
         return null;
      }

      List<Task<RegistryEntry>> requests = [];

      foreach (RegistryEntry entry in entries) {
         async Task<RegistryEntry> RequestCallback() {
            await _httpClient.GetAsync(entry.HealthPingUrl);
            return entry;
         }
         
         Task<RegistryEntry> task = RequestCallback();
         requests.Add(task);
      }

      Task<RegistryEntry> firstResponse = await Task.WhenAny(requests);
      return await firstResponse;
   }
}
