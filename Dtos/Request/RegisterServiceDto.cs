using System.ComponentModel;

namespace ServiceDiscovery.Dtos.Request;

public class RegisterServiceDto {
  [DefaultValue("ExampleService")]
  public string Name { get; set; } = null!;

  [DefaultValue("https")]
  public string Scheme { get; set; } = null!;

  [DefaultValue("example.com")]
  public string Host { get; set; } = null!;

  [DefaultValue("443")]
  public string Port { get; set; } = null!;

  [DefaultValue("example.com/healthz")]
  public string HealthCheckUrl { get; set; } = null!;

  [DefaultValue(30)]
  public int HealthCheckInterval { get; set; }
}
