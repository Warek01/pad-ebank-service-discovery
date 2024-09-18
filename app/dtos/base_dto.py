from pydantic import BaseModel, ConfigDict
from pydantic.alias_generators import to_camel


class BaseDto(BaseModel):
    model_config = ConfigDict(
        alias_generator=to_camel,
        strict=True,
        cache_strings=True,
        extra='ignore',
    )
