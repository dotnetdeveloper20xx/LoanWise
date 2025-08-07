namespace LoanWise.Application.Common.Exceptions
{
    /// <summary>
    /// Thrown when the user is not authorized to perform the requested action.
    /// </summary>
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message = "Unauthorized") : base(message) { }
    }
}
