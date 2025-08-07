namespace StoreBoost.Application.Common.Models
{
    /// <summary>
    /// Standard API response wrapper to enforce consistent return format.
    /// </summary>
    public sealed class ApiResponse<T>
    {
        public bool Success { get; }
        public string? Message { get; }
        public T? Data { get; }

        private ApiResponse(bool success, T? data = default, string? message = null)
        {
            Success = success;
            Data = data;
            Message = message;
        }

        /// <summary>
        /// Creates a successful response with optional data and message.
        /// </summary>
        public static ApiResponse<T> SuccessResult(T data, string? message = null) =>
            new(success: true, data: data, message: message);

        /// <summary>
        /// Creates a successful response with no data.
        /// </summary>
        public static ApiResponse<T> SuccessResult(string? message = null) =>
            new(success: true, data: default, message: message);

        /// <summary>
        /// Creates a failed response with an error message.
        /// </summary>
        public static ApiResponse<T> FailureResult(string message) =>
            new(success: false, data: default, message: message);

        public override string ToString()
        {
            return Success
                ? $"Success: {Message ?? "OK"}"
                : $"Failure: {Message ?? "Unknown Error"}";
        }
    }
}
