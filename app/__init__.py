import os
from flask import Flask, request
from .routes import bp
from .services import services_bp


def create_app():
    app = Flask(
        __name__,
        instance_relative_config=True,
    )
    app.config.from_mapping(
        SECRET_KEY='dev',
        DATABASE=os.path.join(app.instance_path, 'flaskr.sqlite'),
    )
    app.config.from_pyfile('config.py', silent=True)
    app.register_blueprint(bp)
    app.register_blueprint(services_bp, url_prefix='/services')

    try:
        os.makedirs(app.instance_path)
    except OSError:
        pass

    return app
