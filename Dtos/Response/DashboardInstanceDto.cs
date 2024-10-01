namespace ServiceDiscovery.Dtos.Response;

public class DashboardInstanceDto {
  public string Id { get; set; } = null!;
  public string Name { get; set; } = null!;
  public string Url { get; set; } = null!;
  public string HealthCheckUrl { get; set; } = null!;
  public int HealthCheckInterval { get; set; }
}
