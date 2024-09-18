import os
from flask import Flask
from dotenv import load_dotenv

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

    try:
        os.makedirs(app.instance_path)
    except OSError:
        pass

    return app
