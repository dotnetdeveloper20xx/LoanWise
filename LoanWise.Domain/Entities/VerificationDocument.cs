using System;

namespace LoanWise.Domain.Entities
{
    /// <summary>
    /// Represents a document uploaded by a user for identity or income verification.
    /// </summary>
    public class VerificationDocument
    {
        /// <summary>
        /// Unique identifier for the document.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Foreign key referencing the uploading user.
        /// </summary>
        public Guid UserId { get; private set; }

        /// <summary>
        /// Navigation property to the user who uploaded this document.
        /// </summary>
        public User User { get; private set; }

        /// <summary>
        /// Type of document uploaded (e.g., Payslip, Passport, BankStatement).
        /// </summary>
        public DocumentType Type { get; private set; }

        /// <summary>
        /// Name of the file in blob storage (or full URL/identifier).
        /// </summary>
        public string BlobReference { get; private set; }

        /// <summary>
        /// UTC timestamp when the document was uploaded.
        /// </summary>
        public DateTime UploadedAtUtc { get; private set; }

        /// <summary>
        /// Optional textual note (e.g., “Feb Payslip”, “Gov ID”).
        /// </summary>
        public string? Description { get; private set; }

        /// <summary>
        /// Required by EF Core.
        /// </summary>
        private VerificationDocument() { }

        /// <summary>
        /// Creates a new verification document.
        /// </summary>
        /// <param name="id">Unique document ID.</param>
        /// <param name="userId">Uploader's user ID.</param>
        /// <param name="type">Document type.</param>
        /// <param name="blobReference">Blob storage reference.</param>
        /// <param name="uploadedAtUtc">Timestamp of upload.</param>
        /// <param name="description">Optional description.</param>
        public VerificationDocument(
            Guid id,
            Guid userId,
            DocumentType type,
            string blobReference,
            DateTime uploadedAtUtc,
            string? description = null)
        {
            if (string.IsNullOrWhiteSpace(blobReference))
                throw new ArgumentException("Blob reference cannot be null or empty.", nameof(blobReference));

            Id = id;
            UserId = userId;
            Type = type;
            BlobReference = blobReference;
            UploadedAtUtc = uploadedAtUtc;
            Description = description;
        }
    }

    /// <summary>
    /// Supported document types for verification.
    /// </summary>
    public enum DocumentType
    {
        Payslip = 0,
        ID = 1,
        Passport = 2,
        BankStatement = 3,
        UtilityBill = 4,
        Other = 5
    }
}
