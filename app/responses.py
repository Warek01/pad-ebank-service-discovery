from json import dumps

RESPONSE_SUCCESS = (dumps({ 'message': 'success' }), 200)
RESPONSE_CLIENT_ERROR = (dumps({ 'message': 'client error' }), 400)
RESPONSE_UNAUTHORIZED = (dumps({ 'message': 'unauthorized' }), 401)
RESPONSE_NOT_FOUND = (dumps({ 'message': 'not found' }), 404)
