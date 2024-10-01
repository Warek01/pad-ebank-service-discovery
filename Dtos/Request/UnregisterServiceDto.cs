using System.ComponentModel;

namespace ServiceDiscovery.Dtos.Request;

public class UnregisterServiceDto {
  [DefaultValue("bafae5fc-aefe-462b-a57c-e3ce21cf2fe5")]
  public string Id { get; set; } = null!;
}
