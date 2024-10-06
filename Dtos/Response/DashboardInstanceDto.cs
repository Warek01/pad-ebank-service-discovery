namespace ServiceDiscovery.Dtos.Response;

public class DashboardInstanceDto : InstanceDto {
  public string Name { get; set; } = null!;
  public string HealthCheckUrl { get; set; } = null!;
  public int HealthCheckInterval { get; set; }
}
