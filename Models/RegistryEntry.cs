namespace ServiceDiscovery.Models;

public class RegistryEntry {
  public class HealthCheckEntry {
    public string Url { get; set; } = null!;
    public int Interval { get; set; }
  }

  public HealthCheckEntry HealthCheck { get; set; } = null!;
  public string Name { get; set; } = null!;
  public string Url { get; set; } = null!;
}
