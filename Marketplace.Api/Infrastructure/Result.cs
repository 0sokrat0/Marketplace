using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Api.Infrastructure;

public enum ResultStatus
{
    /// <summary>200 OK, в ответ уходит Data.</summary>
    Success,

    /// <summary>204 No Content, ответ без тела.</summary>
    NoContent,

    /// <summary>400 Bad Request, в ответ уходит Message.</summary>
    InvalidData,

    /// <summary>401 Unauthorized, в ответ уходит Message.</summary>
    Unauthorized,

    /// <summary>404 Not Found, в ответ уходит Message.</summary>
    NotFound,

    /// <summary>409 Conflict, в ответ уходит Message.</summary>
    Conflict
}

/// <summary>Ошибка операции, неявно превращается в Result и в Result&lt;T&gt;.</summary>
public readonly record struct Failure(ResultStatus Status, string Message);

/// <summary>Итог операции без данных. Статический вход для создания ошибок: Result.NotFound("...").</summary>
public class Result
{
    public ResultStatus Status { get; init; }

    public string? Message { get; init; }

    public bool IsSuccess => Status is ResultStatus.Success or ResultStatus.NoContent;

    protected virtual object? Payload => null;

    public static Result NoContent() => new() { Status = ResultStatus.NoContent };

    public static Failure InvalidData(string message) => new(ResultStatus.InvalidData, message);

    public static Failure Unauthorized(string message) => new(ResultStatus.Unauthorized, message);

    public static Failure NotFound(string message) => new(ResultStatus.NotFound, message);

    public static Failure Conflict(string message) => new(ResultStatus.Conflict, message);

    public static implicit operator Result(Failure failure) => new() { Status = failure.Status, Message = failure.Message };

    /// <summary>Переносит ошибку в результат с другим типом данных.</summary>
    public Failure ToFailure() => new(Status, Message ?? string.Empty);

    /// <summary>Ошибки уходят простой строкой, успех данными.</summary>
    public IActionResult ToActionResult() => Status switch
    {
        ResultStatus.Success => new OkObjectResult(Payload),
        ResultStatus.NoContent => new NoContentResult(),
        _ => new ContentResult
        {
            StatusCode = StatusCodeOf(Status),
            Content = Message,
            ContentType = "text/plain; charset=utf-8"
        }
    };

    private static int StatusCodeOf(ResultStatus status) => status switch
    {
        ResultStatus.InvalidData => StatusCodes.Status400BadRequest,
        ResultStatus.Unauthorized => StatusCodes.Status401Unauthorized,
        ResultStatus.NotFound => StatusCodes.Status404NotFound,
        ResultStatus.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError
    };
}

/// <summary>Итог операции с данными. Из данных и из Failure получается неявно: return dto; return Result.NotFound("...").</summary>
public class Result<T> : Result
{
    public T? Data { get; init; }

    protected override object? Payload => Data;

    public static implicit operator Result<T>(T data) => new() { Status = ResultStatus.Success, Data = data };

    public static implicit operator Result<T>(Failure failure) =>
        new() { Status = failure.Status, Message = failure.Message };
}
