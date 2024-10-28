using Microsoft.AspNetCore.Mvc;
using ServiceDiscovery.Dtos.Response;
using ServiceDiscovery.Models;
using ServiceDiscovery.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace ServiceDiscovery.Controllers;

[ApiController]
[Route("Api/v{v:apiVersion}/Load-Balancing")]
[SwaggerTag("Load Balancing")]
public class LoadBalancingController(
   LoadBalancingService loadBalancingService
) : ControllerBase {
   [HttpGet("{name}")]
   [SwaggerOperation("Get the least loaded instance of a service")]
   [SwaggerResponse(StatusCodes.Status200OK, "The instance", typeof(InstanceDto))]
   [SwaggerResponse(StatusCodes.Status404NotFound, "Instance is not present in registry")]
   public async Task<ActionResult<InstanceDto>> GetServiceInstance(string name) {
      RegistryEntry? service = await loadBalancingService.GetServiceInstance(name);

      if (service is null) {
         return NotFound();
      }

      var dto = new InstanceDto {
         Host = service.Host,
         Port = service.Port,
         Scheme = service.Scheme,
      };
      
      return Ok(dto);
   }
}
