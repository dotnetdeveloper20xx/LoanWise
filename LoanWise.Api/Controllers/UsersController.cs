// --------------------------------------------------------------------------------------
// LoanWise.Api/Controllers/UsersController.cs
// Author: Faz Ahmed
// Purpose: Authenticated user endpoints (e.g., /api/users/me).
// Notes:
//  - Thin controller; business logic handled in MediatR handlers.
//  - Uses IUserContext to resolve the current user's ID (avoids brittle claim parsing).
//  - Returns unified ApiResponse<T> envelope from handlers with predictable HTTP codes.
// --------------------------------------------------------------------------------------

using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.Features.Users.Queries.GetCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/users")]
    [Produces("application/json")]
    [Tags("Users")]
    public sealed class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IUserContext _userContext;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IMediator mediator, IUserContext userContext, ILogger<UsersController> logger)
        {
            _mediator = mediator;
            _userContext = userContext;
            _logger = logger;
        }

        /// <summary>
        /// (Authored by Faz Ahmed) Gets the currently authenticated user's profile.
        /// </summary>
        /// <remarks>
        /// The user ID is taken from the authenticated context (JWT), not from the client.
        /// </remarks>
        /// <response code="200">User details returned.</response>
        /// <response code="401">Missing or invalid identity.</response>
        /// <response code="400">Request failed (see ApiResponse message).</response>
        [HttpGet("me")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentUser(CancellationToken ct = default)
        {
            var userId = _userContext.UserId;
            if (!userId.HasValue || userId.Value == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized access to /api/users/me: missing user ID in context.");
                return Unauthorized(ApiResponse<object> .FailureResult("Invalid or missing user identity."));
            }

            var result = await _mediator.Send(new GetCurrentUserQuery(userId.Value), ct);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
