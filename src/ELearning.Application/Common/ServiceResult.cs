namespace ELearning.Application.Common;

public sealed class ServiceResult<T>
{
    public bool Succeeded { get; init; }

    public int StatusCode { get; init; }

    public string? ErrorMessage { get; init; }

    public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; init; }

    public T? Data { get; init; }

    public static ServiceResult<T> Ok(T data, int statusCode = 200) => new()
    {
        Succeeded = true,
        StatusCode = statusCode,
        Data = data
    };

    public static ServiceResult<T> Fail(
        string errorMessage,
        int statusCode = 400,
        IReadOnlyDictionary<string, string[]>? validationErrors = null) => new()
    {
        Succeeded = false,
        StatusCode = statusCode,
        ErrorMessage = errorMessage,
        ValidationErrors = validationErrors
    };
}

