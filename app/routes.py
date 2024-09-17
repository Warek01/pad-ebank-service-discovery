from flask import Blueprint, app

bp = Blueprint('main', __name__)


@bp.route('/')
def get():
    return 'get'
