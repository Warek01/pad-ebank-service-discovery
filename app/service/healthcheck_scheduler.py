from threading import Thread, Event
from time import time, sleep
import requests
from flask import current_app, Flask

from .registry import registry, registry_lock, cache_registry


def healthcheck_request_job(service: dict, app: Flask):
    with app.app_context():
        try:
            response = requests.get(service['healthcheck']['url'], timeout=5)
            response.raise_for_status()
        except Exception:
            current_app.logger.info(f'Healthcheck error {service["service_name"]}-{service["service_id"]}')

            with registry_lock:
                if service['service_name'] in registry:
                    registry[service['service_name']].remove(service)

                    if not registry[service['service_name']]:
                        del registry[service['service_name']]

                    cache_registry()
            return

        with registry_lock:
            cache_registry()


def healthcheck_job(stop_event: Event, app: Flask):
    sleep(5)

    with app.app_context():
        current_app.logger.info('Started healthcheck task')

        while not stop_event.is_set():
            with registry_lock:
                for service_name in registry:
                    services = registry[service_name]

                    if not services:
                        del registry[service_name]
                        cache_registry()
                    else:
                        for service in services:
                            now = int(time())

                            if now - service['last_checked_at'] < service['healthcheck']['check_interval']:
                                break

                            thread = Thread(target=healthcheck_request_job, args=(service, app))
                            thread.start()

                            service['last_checked_at'] = now

            sleep(1)
