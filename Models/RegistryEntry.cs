namespace ServiceDiscovery.Models;

public class RegistryEntry {
  public string HealthCheckUrl { get; set; } = null!;
  public int HealthCheckInterval { get; set; }
  public string Name { get; set; } = null!;
  public string Scheme { get; set; } = null!;
  public string Host { get; set; } = null!;
  public string Port { get; set; } = null!;

  public override string ToString() {
    return $"RegistryEntry {Name} {Scheme}://{Host}:{Port}";
  }
}
