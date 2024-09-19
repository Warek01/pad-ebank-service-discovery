from datetime import datetime
from threading import Thread
from time import time, sleep

import requests

from .registry import get_registry, registry_lock, cache_registry


def request_handler(service: dict):
    try:
        response = requests.get(service['healthcheck']['url'], timeout=10)
        response.raise_for_status()
    except:
        current_time = datetime.now().strftime("%H:%M:%S")
        print(f'[{current_time}] Healthcheck error {service["service_name"]} - {service["service_id"]}', flush=True)

        with registry_lock:
            registry = get_registry()
            if service['service_name'] in registry:
                registry[service['service_name']].remove(service)
                cache_registry()
        return

    with registry_lock:
        cache_registry()


def healthcheck():
    print('Started healthcheck task', flush=True)

    while True:
        with registry_lock:
            registry = get_registry()
            for service_name in registry:
                services = registry[service_name]

                for service in services:
                    now = int(time())

                    if now - service['last_checked_at'] < service['healthcheck']['check_interval']:
                        break

                    thread = Thread(target=request_handler, args=(service,))
                    thread.start()

                    service['last_checked_at'] = now

        sleep(1)


healthcheck_scheduler_thread = Thread(target=healthcheck)
