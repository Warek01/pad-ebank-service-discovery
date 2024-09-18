from app.service.dtos.request.register_dto import RegisterDto


class ServiceRegistry(dict[str, list[RegisterDto]]):
    pass


registry = ServiceRegistry()
