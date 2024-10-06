namespace ServiceDiscovery.Dtos.Response;

public class InstanceDto {
  public string Scheme { get; set; } = null!;
  public string Host { get; set; } = null!;
  public string Port { get; set; } = null!;
}
