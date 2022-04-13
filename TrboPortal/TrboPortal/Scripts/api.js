// Base URL for API. If we ever going to make a v2 we'll just move it then.
const apiBase = '/api/v1'

window.getApiBaseUrl = function getApiBaseUrl() {
    return apiBase;
}

/**
 * Generic method for performing an api call
 *
 * @param request:      The request url used (not prepended with base url).
 * @param method:       The method to use (GET, POST, PUT, DELETE)
 * @param data:         The data to send
 * @param onSuccess:    The callback to call on success
 * @param onFailed:     The callback to call on error
 * @param onFinished:   (Optional) callback to call after request finished, always called.
 */
export function performApiCall(request_url, method, data, onSuccess, onFailed, onFinished = null) {
    const request = new Request(`${apiBase}/${request_url}`);
    internalPerformApiCall(request, method, true, data, onSuccess, onFailed, onFinished);
}

export function getValidationErrors(response) {
    var errors = [];
    try {
        var modelState = response.ModelState;
        for (var key in modelState) {
            if (modelState.hasOwnProperty(key)) {
                for (var i = 0; i < modelState[key].length; i++) {
                    var errorString = modelState[key][i];
                    console.log(errorString);
                    errors.push(errorString);
                }
            }
        }
    } catch (e) {
        console.log(e);
        errors.push("Unknown error!");
    }

    return errors;
}


function internalPerformApiCall(request, method, asJson, data, onSuccess, onFailed, onFinished = null) {
  // This function implements the actual api call
  const headers = new Headers();
  if (asJson) {
    headers.append('Content-Type', 'application/json');
  }

  fetch(request, {
    method: method,
    mode: 'same-origin', // Do not send CSRF token to another domain.
    headers: headers,
    body: data == null ? null : asJson ? JSON.stringify(data) : data
  }).then(function (response) {
    if (response.status == 200 || response.status == 204) {
      // 200 ok, delete
      onSuccess(response);
    } else if (response.status == 400) {
      onFailed(response);
    } else if (response.status == 401 || response.status == 403) {
      // 401 unauthorized, go to login page
      // 403 forbidden, logged in with wrong account, go to login page
      console.log(`Unauthorized! (${response.status})`);
      onFailed(response);
    } else if (response.status > 400) {
      // something unexpected happened. Although onFailed proably wouln't be able to parse response message wouldn't b
      console.log(`Some unexpected error happened! (${response.status})`);
      onFailed(response);
    } else {
      console.log(`Some unexpected non-error happened! (${response.status})`);
    }
    if (onFinished != null) {
      onFinished(response);
    }
  });
}
