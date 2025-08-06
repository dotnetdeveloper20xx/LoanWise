using System;

namespace LoanWise.Domain.Entities
{
    /// <summary>
    /// Represents a system-generated event linked to user or platform activity.
    /// </summary>
    public class SystemEvent
    {
        /// <summary>
        /// Unique identifier for the event.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Optional foreign key linking to the user this event is related to.
        /// </summary>
        public Guid? UserId { get; private set; }

        /// <summary>
        /// Navigation property to the associated user (if any).
        /// </summary>
        public User? User { get; private set; }

        /// <summary>
        /// The type/category of the event.
        /// </summary>
        public EventType Type { get; private set; }

        /// <summary>
        /// Descriptive message for what occurred (e.g., “Loan Approved”).
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Optional metadata or structured context (e.g., loan ID, repayment ID) in JSON or raw string.
        /// </summary>
        public string? Metadata { get; private set; }

        /// <summary>
        /// Timestamp when the event occurred (UTC).
        /// </summary>
        public DateTime CreatedAtUtc { get; private set; }

        /// <summary>
        /// Required by EF Core.
        /// </summary>
        private SystemEvent() { }

        /// <summary>
        /// Creates a new system event log entry.
        /// </summary>
        /// <param name="id">Unique ID for the event.</param>
        /// <param name="type">Event type.</param>
        /// <param name="message">Description of the event.</param>
        /// <param name="createdAtUtc">Timestamp of event occurrence.</param>
        /// <param name="userId">User affected (optional).</param>
        /// <param name="metadata">Structured context (optional).</param>
        public SystemEvent(
            Guid id,
            EventType type,
            string message,
            DateTime createdAtUtc,
            Guid? userId = null,
            string? metadata = null)
        {
            Id = id;
            Type = type;
            Message = message;
            CreatedAtUtc = createdAtUtc;
            UserId = userId;
            Metadata = metadata;
        }
    }

    /// <summary>
    /// Defines the type or source of system events.
    /// </summary>
    public enum EventType
    {
        LoanCreated = 0,
        LoanApproved = 1,
        LoanRejected = 2,
        LoanFunded = 3,
        RepaymentMade = 4,
        VerificationUploaded = 5,
        NotificationSent = 6,
        AdminAction = 7,
        ErrorLogged = 8,
        Other = 99
    }
}
