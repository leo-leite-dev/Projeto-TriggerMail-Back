namespace TriggerMail.Core.Application.Abstractions.Result;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public Error? Error { get; }

    private Result(bool ok, T? value, Error? error)
    {
        IsSuccess = ok;
        Value = value;
        Error = error;
    }

    public static Result<T> Ok(T value) => new(true, value, null);
    public static Result<T> Fail(Error error) => new(false, default, error);

    public static Result<T> NotFound(string msg) => Fail(Error.NotFound(msg));
    public static Result<T> BadRequest(string msg) => Fail(Error.BadRequest(msg));
    public static Result<T> Forbidden(string msg) => Fail(Error.Forbidden(msg));
    public static Result<T> Unauthorized(string msg) => Fail(Error.Unauthorized(msg));
    public static Result<T> Failure(string msg) => Fail(Error.Failure(msg));
}