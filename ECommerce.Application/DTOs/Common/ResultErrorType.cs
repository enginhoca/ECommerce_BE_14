using System;

namespace ECommerce.Application.DTOs.Common;

public enum ResultErrorType
{
    None=0, // 200OK
    NotFound=1, //404NotFound
    Validation=2, //400BadRequest
    Conflict=3, //409Conflict
    Unauthorized=4, //401Unauthorized
    Forbidden=5, //403Forbidden
    Unexpected=6 // 500InternalServerError
}
