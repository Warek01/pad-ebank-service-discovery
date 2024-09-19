import json

from flask import request, jsonify, current_app
from pydantic import ValidationError

from app.service.dtos.request.register_dto import RegisterDto
from app.redis import get_redis_client
from app.service.dtos.request.shutdown_dto import ShutdownDto
from app.service.registry import get_registry, REGISTRY_CACHE_FIELD
from app.service import services_bp
from app.responses import RESPONSE_SUCCESS, RESPONSE_NOT_FOUND


@services_bp.route('/register', methods=['POST'])
def register():
    registry = get_registry()
    cache = get_redis_client()
    data = request.get_json()
    dto: RegisterDto

    try:
        dto = RegisterDto(**data)
    except ValidationError as e:
        return jsonify({ "error": e.errors() }), 400

    if dto.service_name not in registry:
        registry[dto.service_name] = []

    registry[dto.service_name].append(dto.model_dump())
    cache.set(REGISTRY_CACHE_FIELD, json.dumps(registry))
    current_app.logger.info(f'Registered service {dto.service_name} - {dto.service_id}')

    return RESPONSE_SUCCESS


@services_bp.route('/<string:service_name>')
def get_instances(service_name: str):
    registry = get_registry()

    if service_name not in registry:
        return RESPONSE_NOT_FOUND

    response = { 'instances': [] }

    for entry in registry[service_name]:
        response['instances'].append({ 'id': entry['service_id'], 'url': entry['url'] })

    return jsonify(response)


@services_bp.route('/shutdown', methods=['POST'])
def shutdown_service():
    registry = get_registry()
    cache = get_redis_client()
    data = request.get_json()
    dto: ShutdownDto

    try:
        dto = ShutdownDto(**data)
    except ValidationError as e:
        return jsonify({ "error": e.errors() }), 400

    if dto.service_name not in registry:
        return RESPONSE_NOT_FOUND

    registry[dto.service_name] = [instance for instance in registry[dto.service_name] if
                                  instance['service_id'] != dto.service_id]

    if not registry[dto.service_name]:
        del registry[dto.service_name]

    if not registry:
        cache.delete(REGISTRY_CACHE_FIELD)
    else:
        cache.set(REGISTRY_CACHE_FIELD, json.dumps(registry))

    current_app.logger.info(f'Deleted service {dto.service_name} - {dto.service_id}')

    return RESPONSE_SUCCESS
