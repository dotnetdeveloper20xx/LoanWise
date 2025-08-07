namespace LoanWise.Application.Common.Exceptions
{
    /// <summary>
    /// Thrown when a requested resource cannot be found.
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }
}
