from app.dtos.base_dto import BaseDto


class RegisterDto(BaseDto):
    class Healthcheck(BaseDto):
        url: str
        check_interval: int

    service_name: str
    service_id: str
    url: str
    healthcheck: Healthcheck
