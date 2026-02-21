using Microsoft.AspNetCore.Mvc;
using StarterKit.Application.Results;

namespace StarterKit.Api.Errors;

public static class ApplicationErrorMapper
{
    public static ActionResult Map(ControllerBase controller, AppError? error)
    {
        if (error is null)
            return controller.Problem("Unknown application error.");

        return error.Code switch
        {
            ErrorCode.ValidationFailed => controller.BadRequest(error.Message),
            ErrorCode.EmailAlreadyRegistered => controller.Conflict(error.Message),
            ErrorCode.InvalidCredentials => controller.Unauthorized(error.Message),
            ErrorCode.InvalidRefreshToken => controller.Unauthorized(error.Message),
            ErrorCode.UserNotFound => controller.Unauthorized(),
            _ => controller.Problem(error.Message)
        };
    }

    public static ActionResult<T> Map<T>(ControllerBase controller, AppError? error)
    {
        return Map(controller, error);
    }
}
