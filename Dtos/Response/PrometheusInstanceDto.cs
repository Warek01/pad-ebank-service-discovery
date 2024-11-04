namespace ServiceDiscovery.Dtos.Response;

public class PrometheusInstanceDto {
   public List<string> Targets { get; set; }

   public Dictionary<string, string> Labels { get; set; }
}
