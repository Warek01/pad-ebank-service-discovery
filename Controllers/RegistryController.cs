using System.ComponentModel;
using System.Text.Json;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ServiceDiscovery.Dtos.Request;
using ServiceDiscovery.Dtos.Response;
using ServiceDiscovery.Helpers;
using ServiceDiscovery.Models;
using ServiceDiscovery.Services;
using StackExchange.Redis;

namespace ServiceDiscovery.Controllers;

[ApiController]
[ApiVersion(1)]
[Route("Api/v{v:apiVersion}/[controller]")]
public class RegistryController(
  RegistryService registryService,
  IConnectionMultiplexer connectionMultiplexer,
  IOptions<JsonOptions> jsonOptions
) : ControllerBase {
  private readonly IDatabase _redis = connectionMultiplexer.GetDatabase();

  [HttpGet]
  public async Task<ActionResult<DashboardInstanceDto>> GetAllInstances() {
    string? cache = await _redis.StringGetAsync(CacheKeys.DashboardInstanceDtos);

    if (cache is not null) {
      return Ok(cache);
    }

    List<DashboardInstanceDto> res = registryService.GetAll()
      .Select(e => new DashboardInstanceDto {
        Url = e.Url,
        Name = e.Name,
        HealthCheckInterval = e.HealthCheck.Interval,
        HealthCheckUrl = e.HealthCheck.Url,
      })
      .ToList();

    cache = JsonSerializer.Serialize(res, jsonOptions.Value.JsonSerializerOptions);
    await _redis.StringSetAsync(CacheKeys.DashboardInstanceDtos, cache, TimeSpan.FromSeconds(60));

    return Ok(res);
  }

  [HttpGet("Instances/{name}")]
  public ActionResult<List<InstanceDto>> GetInstances([DefaultValue("TestService")] string name) {
    List<InstanceDto> res = registryService.GetByName(name)
      .Select(e => new InstanceDto {
        Url = e.Url,
      })
      .ToList();

    return Ok(res);
  }

  [HttpPost]
  public async Task<ActionResult> Register(RegisterServiceDto dto) {
    if (registryService.Has(dto.Id)) {
      return Created();
    }

    var entry = new RegistryEntry {
      Url = dto.Url,
      Name = dto.Name,
      HealthCheck = new RegistryEntry.HealthCheckEntry {
        Url = dto.HealthCheckUrl,
        Interval = dto.HealthCheckInterval,
      },
    };

    await registryService.Add(entry);
    await _redis.KeyDeleteAsync(CacheKeys.DashboardInstanceDtos);

    return Created();
  }

  [HttpDelete("{id}")]
  public async Task<ActionResult> Unregister([DefaultValue("bafae5fc-aefe-462b-a57c-e3ce21cf2fe5")] string id) {
    if (!registryService.Has(id)) {
      return NotFound();
    }

    RegistryEntry entry = registryService.GetById(id)!;
    await registryService.Remove(entry);

    return Ok();
  }
}
