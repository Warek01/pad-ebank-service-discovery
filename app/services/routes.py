from flask import request, jsonify
from . import services_bp


@services_bp.route('/test', methods=['GET', 'POST'])
def test():
    data = request.get_json()
    print(data['name'])
    return jsonify(data)
