using System.ComponentModel.DataAnnotations;
using LoanWise.Domain.Entities;

namespace LoanWise.Application.DTOs.Users
{
    /// <summary>
    /// Represents user-provided input when registering for a new account.
    /// </summary>
    public class UserRegistrationDto
    {
        /// <summary>
        /// Full name of the user.
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string FullName { get; set; } = default!;

        /// <summary>
        /// Email address to be used for login.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        /// <summary>
        /// Password for securing the account.
        /// </summary>
        [Required]
        [MinLength(6)]
        public string Password { get; set; } = default!;

        /// <summary>
        /// The role the user is registering as (Borrower or Lender).
        /// </summary>
        [Required]
        [EnumDataType(typeof(UserRole))]
        public UserRole Role { get; set; }
    }
}
