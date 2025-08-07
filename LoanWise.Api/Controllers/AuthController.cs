using LoanWise.Application.Features.Auth.Commands.LoginUser;
using LoanWise.Application.Features.Auth.Commands.RegisterUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanWise.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Registers a new user (Borrower, Lender, or Admin).
        /// </summary>
        /// <param name="command">User registration data.</param>
        /// <returns>Success or validation errors.</returns>
        /// <response code="200">User registered successfully.</response>
        /// <response code="400">Validation errors or existing user.</response>
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// </summary>
        /// <param name="command">Login credentials (email + password).</param>
        /// <returns>JWT token if successful.</returns>
        /// <response code="200">Authentication successful.</response>
        /// <response code="401">Invalid credentials.</response>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
