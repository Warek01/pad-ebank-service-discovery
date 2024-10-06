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
        HealthCheckInterval = e.HealthCheckInterval,
        HealthCheckUrl = e.HealthCheckUrl,
        Name = e.Name,
        Scheme = e.Scheme,
        Host = e.Host,
        Port = e.Port,
      })
      .ToList();

    cache = JsonSerializer.Serialize(res, jsonOptions.Value.JsonSerializerOptions);
    await _redis.StringSetAsync(CacheKeys.DashboardInstanceDtos, cache, TimeSpan.FromSeconds(60));

    return Ok(res);
  }

  [HttpGet("{name}")]
  public ActionResult<List<InstanceDto>> GetInstances([DefaultValue("TestService")] string name) {
    List<InstanceDto> res = registryService.GetByName(name)
      .Select(e => new InstanceDto {
        Host = e.Host,
        Port = e.Port,
        Scheme = e.Scheme,
      })
      .ToList();

    return Ok(res);
  }

  [HttpPost]
  public async Task<ActionResult> Register(RegisterServiceDto dto) {
    if (registryService.Has(dto.Host)) {
      return Created();
    }

    var entry = new RegistryEntry {
      HealthCheckUrl = dto.HealthCheckUrl,
      HealthCheckInterval = dto.HealthCheckInterval,
      Name = dto.Name,
      Scheme = dto.Scheme,
      Host = dto.Host,
      Port = dto.Port,
    };

    await registryService.Add(entry);
    await _redis.KeyDeleteAsync(CacheKeys.DashboardInstanceDtos);

    return Created();
  }

  [HttpDelete("{id}")]
  public async Task<ActionResult> Unregister(string url) {
    if (!registryService.Has(url)) {
      return NotFound();
    }

    RegistryEntry entry = registryService.GetByHost(url)!;
    await registryService.Remove(entry);

    return Ok();
  }
}
