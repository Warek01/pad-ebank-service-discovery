from flask import request, jsonify, current_app
from pydantic import ValidationError

from app.service.dtos.request.register_dto import RegisterDto
from app.redis import get_redis_client
from app.service.registry import registry
from app.service import services_bp
from app.responses import RESPONSE_SUCCESS, RESPONSE_NOT_FOUND


@services_bp.route('/register', methods=['POST'])
def register():
    client = get_redis_client()
    data = request.get_json()
    dto: RegisterDto

    try:
        dto = RegisterDto(**data)
    except ValidationError as e:
        return jsonify({ "error": e.errors() }), 400

    if dto.service_name not in registry:
        registry[dto.service_name] = []

    registry[dto.service_name].append(dto)
    client.hset('service-discovery', dto.service_name, dto.model_dump_json())

    return RESPONSE_SUCCESS


@services_bp.route('/<string:service_name>')
def get_instances(service_name: str):
    if service_name not in registry:
        return RESPONSE_NOT_FOUND

    response = { 'instances': [] }

    for entry in registry[service_name]:
        response['instances'].append({ 'id': entry.service_id, 'url': str(entry.url) })

    return jsonify(response)
