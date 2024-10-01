using System.ComponentModel;

namespace ServiceDiscovery.Dtos.Request;

public class GetInstancesDto {
  [DefaultValue("TestService")]
  public string ServiceName { get; set; } = null!;
}
