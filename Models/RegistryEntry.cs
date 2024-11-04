namespace ServiceDiscovery.Models;

public class RegistryEntry {
  public string Name { get; set; } = null!;
  public string Id { get; set; } = null!;
  public int HealthCheckInterval { get; set; }
  public Uri HealthPingUri { get; set; } = null!;
  public Uri HealthCheckUri { get; set; } = null!;
  public Uri? HttpUri { get; set; } = null!;
  public Uri? GrpcUri { get; set; } = null!;
}
