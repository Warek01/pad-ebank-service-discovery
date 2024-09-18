from pydantic import HttpUrl

from app.dtos.base_dto import BaseDto


class RegisterDto(BaseDto):
    class Healthcheck(BaseDto):
        url: HttpUrl
        interval: int

    service_name: str
    service_id: str
    url: HttpUrl
    healthcheck: Healthcheck
