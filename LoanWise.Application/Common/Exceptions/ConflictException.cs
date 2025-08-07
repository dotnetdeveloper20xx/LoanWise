namespace LoanWise.Application.Common.Exceptions
{
    /// <summary>
    /// Thrown when the operation would cause a business conflict (e.g., duplicate resource).
    /// </summary>
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
    }
}
