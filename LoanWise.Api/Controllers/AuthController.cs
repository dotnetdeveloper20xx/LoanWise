// --------------------------------------------------------------------------------------
// LoanWise.Api - AuthController
// Author: Faz Ahmed
// Purpose: Authentication endpoints for registration, login, and token refresh.
// Notes:
//  - Clean Architecture + CQRS (MediatR).
//  - Unified ApiResponse<T> contract across all actions.
//  - Includes XML docs, explicit content types, and observability (ILogger).
// --------------------------------------------------------------------------------------

using LoanWise.Application.DTOs.Auth;
using LoanWise.Application.Features.Auth.Commands.LoginUser;
using LoanWise.Application.Features.Auth.Commands.RefreshToken;
using LoanWise.Application.Features.Auth.Commands.RegisterUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StoreBoost.Application.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace LoanWise.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [AllowAnonymous] // All actions below are public; secure others explicitly if needed
    [Produces("application/json")]
    [Tags("Auth")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public sealed class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IMediator mediator, ILogger<AuthController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new user (Borrower, Lender, or Admin).
        /// </summary>
        /// <param name="command">Registration details.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>ApiResponse with the created user summary or validation errors.</returns>
        /// <response code="200">User registered successfully.</response>
        /// <response code="400">Validation errors or user already exists.</response>
        [HttpPost("register")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody, Required] RegisterUserCommand command, CancellationToken ct = default)
        {
            _logger.LogInformation("Registration attempt for email '{Email}' by Faz Ahmed's API.", command.Registration.Email);

            var result = await _mediator.Send(command, ct);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Authenticates a user and returns a JWT access token.
        /// </summary>
        /// <param name="command">Login credentials (email + password).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>ApiResponse containing JWT and expiry on success.</returns>
        /// <response code="200">Authentication successful.</response>
        /// <response code="401">Invalid credentials.</response>
        [HttpPost("login")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody, Required] LoginUserCommand command, CancellationToken ct = default)
        {
            _logger.LogInformation("Login attempt for email '{Email}' from IP {IP} via Faz Ahmed's API.",
                command
                .Login.Email, HttpContext.Connection.RemoteIpAddress?.ToString());

            var result = await _mediator.Send(command, ct);
            // Treat authentication failures as 401 for clarity to clients
            return result.Success ? Ok(result) : Unauthorized(result);
        }

        /// <summary>
        /// Exchanges a valid refresh token for a new access token.
        /// </summary>
        /// <param name="request">Refresh token request.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>ApiResponse with new access token and (optionally) a rotated refresh token.</returns>
        /// <response code="200">Refresh successful.</response>
        /// <response code="401">Refresh token invalid or expired.</response>
        [HttpPost("refresh")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<RefreshTokenResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<RefreshTokenResponse>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh([FromBody, Required] RefreshTokenRequest request, CancellationToken ct = default)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            _logger.LogInformation("Token refresh attempt from IP {IP} via Faz Ahmed's API.", ip);

            var result = await _mediator.Send(new RefreshTokenCommand(request, ip), ct);
            return result.Success ? Ok(result) : Unauthorized(result);
        }
    }
}
