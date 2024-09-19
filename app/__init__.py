import os
from flask import Flask, send_from_directory
from dotenv import load_dotenv
from flask_swagger_ui import get_swaggerui_blueprint

from .routes import bp
from .service import services_bp


def create_app():
    load_dotenv('../.env')

    app = Flask(
        __name__,
        instance_relative_config=True,
    )

    app.config.from_mapping(SECRET_KEY='dev')
    app.config.from_pyfile('config.py', silent=True)
    app.register_blueprint(bp)
    app.register_blueprint(services_bp, url_prefix='/services')

    swaggerui_blueprint = get_swaggerui_blueprint('/docs', '/static/swagger.json')
    app.register_blueprint(swaggerui_blueprint, url_prefix='/docs')

    @app.route('/static/<path:path>')
    def send_static(path):
        return send_from_directory('static', path)

    try:
        os.makedirs(app.instance_path)
    except OSError:
        pass

    return app
