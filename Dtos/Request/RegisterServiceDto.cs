using System.ComponentModel;

namespace ServiceDiscovery.Dtos.Request;

public class RegisterServiceDto {
  [DefaultValue("bafae5fc-aefe-462b-a57c-e3ce21cf2fe5")]
  public string Id { get; set; } = null!;

  [DefaultValue("TestService")]
  public string Name { get; set; } = null!;

  [DefaultValue("example.com")]
  public string Url { get; set; } = null!;

  [DefaultValue("example.com")]
  public string HealthCheckUrl { get; set; } = null!;

  [DefaultValue(30)]
  public int HealthCheckInterval { get; set; }
}
