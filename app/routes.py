from flask import Blueprint, render_template, current_app

from app.service.registry import registry

base_bp = Blueprint('main', __name__)


@base_bp.route('/')
def serve_index():
    current_app.logger.info(registry)
    return render_template('index.jinja', registry=registry)
