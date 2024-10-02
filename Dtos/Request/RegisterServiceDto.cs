using System.ComponentModel;

namespace ServiceDiscovery.Dtos.Request;

public class RegisterServiceDto {
  [DefaultValue("TestService")]
  public string Name { get; set; } = null!;

  [DefaultValue("example.com")]
  public string Url { get; set; } = null!;

  [DefaultValue("example.com")]
  public string HealthCheckUrl { get; set; } = null!;

  [DefaultValue(30)]
  public int HealthCheckInterval { get; set; }
}
