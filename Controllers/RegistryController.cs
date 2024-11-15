using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using ServiceDiscovery.Dtos.Response;
using ServiceDiscovery.Models;
using ServiceDiscovery.Services;

namespace ServiceDiscovery.Controllers;

[ApiController]
[Route("Api/v{v:apiVersion}/Registry")]
public class RegistryController(
   RegistryService registryService
) : ControllerBase {
   [HttpGet]
   public ActionResult<RegistryEntry> GetAllInstances() {
      return Ok(registryService.GetAll());
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
   public ActionResult Register(RegistryEntry entry) {
      if (registryService.HasServiceById(entry.Id)) {
         return Created();
      }

      registryService.Add(entry);

      return Created();
   }

   [HttpDelete("{id}")]
   public ActionResult Unregister(string id) {
      if (!registryService.HasServiceById(id)) {
         return NotFound();
      }

      RegistryEntry entry = registryService.GetById(id)!;
      registryService.Remove(entry);

      return Ok();
   }
}
