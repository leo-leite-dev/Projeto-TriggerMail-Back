namespace TriggerMail.Core.Application.Abstractions.Result;

public sealed record Error(string Code, string Message, int? StatusCode = null)
{
    public static Error BadRequest(string msg) => new("bad_request", msg, 400);
    public static Error Unauthorized(string msg) => new("unauthorized", msg, 401);
    public static Error Forbidden(string msg) => new("forbidden", msg, 403);
    public static Error NotFound(string msg) => new("not_found", msg, 404);
    public static Error Conflict(string msg) => new("conflict", msg, 409);
    public static Error Failure(string msg) => new("failure", msg, 500);
}