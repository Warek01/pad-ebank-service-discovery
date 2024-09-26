import os
import sys
import threading
from signal import signal, SIGTERM, SIGINT
from threading import Thread, Event
from types import FrameType
from flask import Flask
from flask_swagger_ui import get_swaggerui_blueprint

from .config.settings import REDIS_USERNAME, REDIS_PASSWORD, REDIS_HOST, REDIS_PORT, REDIS_DB
from .routes import base_bp
from .service import init_service_module
from .service.healthcheck_scheduler import healthcheck_job
from .service.routes import init_service_routes

app_termination_event: Event
app: Flask


def create_app() -> Flask:
    global app, app_termination_event

    app = Flask(
        __name__,
        instance_relative_config=True,
        static_folder='static',
        static_url_path='',
    )

    app.url_map.strict_slashes = False

    app.register_blueprint(base_bp)
    app.register_blueprint(get_swaggerui_blueprint('/docs', '/swagger.json'), url_prefix='/docs')
    init_service_module(app)

    try:
        os.makedirs(app.instance_path)
    except OSError:
        pass

    app_termination_event = Event()
    scheduler_thread = Thread(target=healthcheck_job, args=(app_termination_event, app), daemon=True)
    scheduler_thread.start()

    return app


def shutdown_handler(signum: int, frame: FrameType | None):
    global app, app_termination_event

    app.logger.info('Terminating ...')

    app_termination_event.set()
    all_threads_shut_down = False
    app_threads = threading.enumerate()

    # give time to gracefully stop
    app.logger.info(f'Waiting for {len(app_threads)} threads to terminate')
    while not all_threads_shut_down:
        all_threads_shut_down = True

        for thread in app_threads:
            if thread.is_alive():
                all_threads_shut_down = False

    sys.exit(0)


signal(SIGTERM, shutdown_handler)
signal(SIGINT, shutdown_handler)
