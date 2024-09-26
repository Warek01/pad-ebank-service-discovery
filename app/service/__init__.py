from flask import Blueprint, Flask

from .routes import init_service_routes


def init_service_module(app: Flask) -> None:
    services_bp = Blueprint('services', __name__, url_prefix='/api/service')
    init_service_routes(services_bp)
    app.register_blueprint(services_bp)
