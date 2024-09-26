import os
from typing import Optional
from redis import Redis

from app.config.settings import REDIS_PORT, REDIS_DB, REDIS_HOST, REDIS_USERNAME, REDIS_PASSWORD

redis_client = Redis(
    host=REDIS_HOST,
    port=REDIS_PORT,
    db=REDIS_DB,
    username=REDIS_USERNAME,
    password=REDIS_PASSWORD,
    decode_responses=True,
)

# _client: Optional[Redis] = None
#
#
# def get_redis_client() -> Redis:
#     global _client
#
#     if not _client:
#         _client = Redis(
#             host=os.getenv('REDIS_HOST'),
#             port=int(os.getenv('REDIS_PORT')),
#             db=int(os.getenv('REDIS_DB')),
#             username=os.getenv('REDIS_USER'),
#             password=os.getenv('REDIS_PASSWORD'),
#             decode_responses=True,
#         )
#
#     return _client
