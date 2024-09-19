from app.dtos.base_dto import BaseDto


class ShutdownDto(BaseDto):
    service_id: str
    service_name: str
    reason: str
