namespace StarterKit.Application.Results;

public enum ErrorCode
{
    ValidationFailed,
    InvalidCredentials,
    EmailAlreadyRegistered,
    InvalidRefreshToken,
    UserNotFound
}

public sealed record AppError(ErrorCode Code, string Message);

public sealed class Result
{
    private Result(bool isSuccess, AppError? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public AppError? Error { get; }

    public static Result Success() => new(true, null);
    public static Result Failure(AppError error) => new(false, error);
}

public sealed class Result<T>
{
    private Result(bool isSuccess, T? value, AppError? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public bool IsSuccess { get; }
    public T? Value { get; }
    public AppError? Error { get; }

    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(AppError error) => new(false, default, error);
}
