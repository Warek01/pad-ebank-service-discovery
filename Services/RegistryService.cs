using ServiceDiscovery.Models;

namespace ServiceDiscovery.Services;

public class RegistryService {
  private readonly List<RegistryEntry> _registry = [];
  private readonly object _registryLock = new();

  public bool Has(string id) {
    lock (_registryLock) {
      return _registry.Any(entry => entry.Id == id);
    }
  }

  public bool Has(RegistryEntry entry) {
    lock (_registryLock) {
      return _registry.Contains(entry);
    }
  }

  public void Add(RegistryEntry entry) {
    lock (_registryLock) {
      _registry.Add(entry);
    }
  }

  public List<RegistryEntry> GetByName(string name) {
    lock (_registryLock) {
      return _registry.FindAll(entry => entry.Name == name);
    }
  }

  public RegistryEntry? GetById(string id) {
    lock (_registryLock) {
      return _registry.FirstOrDefault(entry => entry.Id == id);
    }
  }

  public void Remove(RegistryEntry entry) {
    lock (_registryLock) {
      _registry.Remove(entry);
    }
  }

  public List<RegistryEntry> GetAll() {
    lock (_registryLock) {
      return [.._registry];
    }
  }
}
