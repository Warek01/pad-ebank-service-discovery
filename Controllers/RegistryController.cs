using System.ComponentModel;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ServiceDiscovery.Dtos.Response;
using ServiceDiscovery.Helpers;
using ServiceDiscovery.Models;
using ServiceDiscovery.Services;
using StackExchange.Redis;

namespace ServiceDiscovery.Controllers;

[ApiController]
[Route("Api/v{v:apiVersion}/Registry")]
public class RegistryController(
   RegistryService registryService,
   IConnectionMultiplexer connectionMultiplexer,
   IOptions<JsonOptions> jsonOptions
) : ControllerBase {
   private readonly IDatabase _redis = connectionMultiplexer.GetDatabase();

   [HttpGet]
   public async Task<ActionResult<RegistryEntry>> GetAllInstances() {
      string? cache = await _redis.StringGetAsync(CacheKeys.DashboardInstanceDtos);

      if (cache is not null) {
         return Ok(cache);
      }

      List<RegistryEntry> res = registryService.GetAll();

      cache = JsonSerializer.Serialize(res, jsonOptions.Value.JsonSerializerOptions);
      await _redis.StringSetAsync(CacheKeys.DashboardInstanceDtos, cache, TimeSpan.FromSeconds(60));

      return Ok(res);
   }

   [HttpGet("{name}")]
   public ActionResult GetInstances(
      [DefaultValue("TestService")] string name,
      [FromQuery] string format = "default"
   ) {
      switch (format.ToLower()) {
         case "default": {
            return Ok(registryService.GetByName(name));
         }
         case "prometheus": {
            if (!registryService.HasServiceByName(name)) {
               return NotFound();
            }
            
            Dictionary<string, PrometheusInstanceDto> dict = new Dictionary<string, PrometheusInstanceDto>();
            List<RegistryEntry> entries = registryService.GetByName(name);

            foreach (RegistryEntry entry in entries) {
               if (!dict.TryGetValue(entry.Name, out PrometheusInstanceDto? value)) {
                  value = new PrometheusInstanceDto {
                     Targets = [],
                     Labels = new Dictionary<string, string> {
                        { "job", entry.Name },
                     },
                  };
                  dict[entry.Name] = value;
               }

               value.Targets.Add($"{entry.HttpUri!.Host}:{entry.HttpUri!.Port}");
            }

            return Ok(dict.Values);
         }
         default:
            return BadRequest("Unknown format");
      }
   }

   [HttpPost]
   public async Task<ActionResult> Register(RegistryEntry entry) {
      if (registryService.HasServiceById(entry.Id)) {
         return Created();
      }

      registryService.Add(entry);
      await _redis.KeyDeleteAsync(CacheKeys.DashboardInstanceDtos);

      return Created();
   }

   [HttpDelete("{id}")]
   public async Task<ActionResult> Unregister(string id) {
      if (!registryService.HasServiceById(id)) {
         return NotFound();
      }

      RegistryEntry entry = registryService.GetById(id)!;
      await registryService.Remove(entry);

      return Ok();
   }
}
