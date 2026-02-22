namespace Escale.API.DTOs.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string message = "Success")
        => new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null)
        => new() { Success = false, Message = message, Errors = errors };
}

public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string>? Errors { get; set; }

    public static ApiResponse SuccessResponse(string message = "Success")
        => new() { Success = true, Message = message };

    public static ApiResponse ErrorResponse(string message, List<string>? errors = null)
        => new() { Success = false, Message = message, Errors = errors };
}
