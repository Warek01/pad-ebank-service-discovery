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
