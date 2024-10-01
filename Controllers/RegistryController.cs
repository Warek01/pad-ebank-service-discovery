using System.ComponentModel;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using ServiceDiscovery.Dtos.Request;
using ServiceDiscovery.Dtos.Response;
using ServiceDiscovery.Models;
using ServiceDiscovery.Services;

namespace ServiceDiscovery.Controllers;

[ApiController]
[ApiVersion(1)]
[Route("Api/v{v:apiVersion}/[controller]")]
public class RegistryController(
  RegistryService registryService,
  HealthCheckService healthCheckService
) : ControllerBase {
  [HttpGet]
  public ActionResult<DashboardInstanceDto> GetAllInstances() {
    List<DashboardInstanceDto> res = registryService.GetAll()
      .Select(e => new DashboardInstanceDto {
        Id = e.Id,
        Url = e.Url,
        Name = e.Name,
        HealthCheckInterval = e.HealthCheck.Interval,
        HealthCheckUrl = e.HealthCheck.Url,
      })
      .ToList();

    return Ok(res);
  }

  [HttpGet("Instances/{name}")]
  public ActionResult<List<InstanceDto>> GetInstances([DefaultValue("TestService")] string name) {
    List<InstanceDto> res = registryService.GetByName(name)
      .Select(e => new InstanceDto {
        Id = e.Id,
        Url = e.Url,
      })
      .ToList();

    return Ok(res);
  }

  [HttpPost]
  public ActionResult Register(RegisterServiceDto dto) {
    if (registryService.Has(dto.Id)) {
      return Created();
    }

    var entry = new RegistryEntry {
      Id = dto.Id,
      Name = dto.Name,
      Url = dto.Url,
      HealthCheck = new RegistryEntry.HealthCheckEntry {
        Url = dto.HealthCheckUrl,
        Interval = dto.HealthCheckInterval,
      },
    };

    registryService.Add(entry);
    _ = healthCheckService.ScheduleHealthChecks(entry);

    return Created();
  }

  [HttpDelete("{id}")]
  public ActionResult Unregister([DefaultValue("bafae5fc-aefe-462b-a57c-e3ce21cf2fe5")] string id) {
    if (!registryService.Has(id)) {
      return NotFound();
    }

    RegistryEntry entry = registryService.GetById(id)!;
    registryService.Remove(entry);

    return Ok();
  }
}
