namespace LoanWise.Api.Models
{
    /// <summary>
    /// Represents a typed API response with result data.
    /// </summary>
    /// <typeparam name="T">The type of the data returned.</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Indicates if the operation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Optional message to return with the response.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// The result data of the operation.
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Creates a success response with data and optional message.
        /// </summary>
        public static ApiResponse<T> SuccessResponse(T data, string? message = null)
            => new() { Success = true, Message = message, Data = data };

        /// <summary>
        /// Creates a failed response with a required message.
        /// </summary>
        public static ApiResponse<T> Fail(string message)
            => new() { Success = false, Message = message };
    }
}
