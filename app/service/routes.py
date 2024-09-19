from flask import request, jsonify, current_app
from pydantic import ValidationError

from app.service.dtos.request.register_dto import RegisterDto
from app.service.dtos.request.shutdown_dto import ShutdownDto
from app.service.registry import get_registry, cache_registry, registry_lock
from app.service import services_bp
from app.responses import RESPONSE_SUCCESS, RESPONSE_NOT_FOUND


@services_bp.route('/register', methods=['POST'])
def register():
    data = request.get_json()
    dto: RegisterDto

    try:
        dto = RegisterDto(**data)
    except ValidationError as e:
        return jsonify({ "error": e.errors() }), 400

    with registry_lock:
        registry = get_registry()
        if dto.service_name not in registry:
            registry[dto.service_name] = []

        entry = dto.model_dump()
        entry['last_checked_at'] = 0
        registry[dto.service_name].append(entry)
        cache_registry()

    current_app.logger.info(f'Registered service {dto.service_name} - {dto.service_id}')

    return RESPONSE_SUCCESS


@services_bp.route('/<string:service_name>')
def get_instances(service_name: str):
    with registry_lock:
        registry = get_registry()
        if service_name not in registry:
            return RESPONSE_NOT_FOUND

        response = { 'instances': [] }

        for entry in registry[service_name]:
            response['instances'].append(
                { 'id': entry['service_id'], 'url': entry['url'] }
            )

    return jsonify(response)


@services_bp.route('/shutdown', methods=['POST'])
def shutdown_service():
    data = request.get_json()
    dto: ShutdownDto

    try:
        dto = ShutdownDto(**data)
    except ValidationError as e:
        return jsonify({ "error": e.errors() }), 400

    with registry_lock:
        registry = get_registry()
        if dto.service_name not in registry:
            return RESPONSE_NOT_FOUND

        registry[dto.service_name] = [instance for instance in registry[dto.service_name] if
                                      instance['service_id'] != dto.service_id]

        if not registry[dto.service_name]:
            del registry[dto.service_name]

        cache_registry()
    current_app.logger.info(f'Deleted service {dto.service_name} - {dto.service_id}')

    return RESPONSE_SUCCESS
