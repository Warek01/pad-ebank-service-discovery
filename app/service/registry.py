import json
from typing import Optional

from app.redis import get_redis_client


class ServiceRegistry(dict[str, list[dict]]):
    pass


_registry: Optional[ServiceRegistry] = None

REGISTRY_CACHE_FIELD = 'service-discovery'


def get_registry() -> ServiceRegistry:
    global _registry

    if not _registry:
        cache = get_redis_client()
        cached_data = cache.get(REGISTRY_CACHE_FIELD)
        _registry = json.loads(cached_data) if cached_data else ServiceRegistry()

    return _registry
