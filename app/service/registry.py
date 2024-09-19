import json
import threading
from typing import Optional

from app.redis import get_redis_client


class ServiceRegistry(dict[str, list[dict]]):
    pass


_registry: Optional[ServiceRegistry] = None

REGISTRY_CACHE_FIELD = 'service-discovery'

registry_lock = threading.Lock()


def get_registry() -> ServiceRegistry:
    global _registry

    if not _registry:
        cache = get_redis_client()
        cached_data = cache.get(REGISTRY_CACHE_FIELD)
        _registry = json.loads(cached_data) if cached_data else ServiceRegistry()

    return _registry


def cache_registry():
    global _registry
    cache = get_redis_client()

    if not _registry:
        cache.delete(REGISTRY_CACHE_FIELD)
    else:
        cache.set(REGISTRY_CACHE_FIELD, json.dumps(_registry))
