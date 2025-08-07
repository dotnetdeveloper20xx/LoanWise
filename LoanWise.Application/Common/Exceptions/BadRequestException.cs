namespace LoanWise.Application.Common.Exceptions
{
    /// <summary>
    /// Thrown when an operation fails due to invalid or unacceptable input.
    /// </summary>
    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message) { }
    }
}
