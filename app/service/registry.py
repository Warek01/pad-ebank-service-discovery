import json
import threading
from typing import Optional

from app.redis import redis_client


class ServiceRegistry(dict[str, list[dict]]):
    pass


REGISTRY_CACHE_FIELD = 'service-discovery'

registry_lock = threading.Lock()

_cached_data: Optional[str] = redis_client.get(REGISTRY_CACHE_FIELD)
registry: ServiceRegistry = json.loads(_cached_data) if _cached_data else ServiceRegistry()


def cache_registry() -> None:
    global registry

    if not registry:
        redis_client.delete(REGISTRY_CACHE_FIELD)
    else:
        redis_client.set(REGISTRY_CACHE_FIELD, json.dumps(registry))
