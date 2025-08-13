// --------------------------------------------------------------------------------------
// LoanWise.Api/Controllers/NotificationsController.cs
// Author: Faz Ahmed
// Purpose: Authenticated endpoints for listing and acknowledging (mark-as-read) notifications.
// Notes:
//  - Thin controller; all business logic lives in MediatR handlers (CQRS).
//  - Unified ApiResponse<T> envelope for all actions.
//  - No caching (user-specific + frequently changing).
// --------------------------------------------------------------------------------------

using LoanWise.Application.DTOs.Notifications;
using LoanWise.Application.Features.Notifications.Commands.MarkNotificationAsRead;
using LoanWise.Application.Features.Notifications.Queries.GetNotifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Api.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    [Produces("application/json")]
    [Tags("Notifications")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public sealed class NotificationsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(IMediator mediator, ILogger<NotificationsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// (Authored by Faz Ahmed) Returns the current user's notifications (most recent first).
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<NotificationDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyNotifications(CancellationToken ct = default)
        {
            _logger.LogDebug("Fetching notifications for current user.");
            var result = await _mediator.Send(new GetNotificationsQuery(), ct);
            return Ok(result); // Handler returns ApiResponse<List<NotificationDto>>
        }

        /// <summary>
        /// (Authored by Faz Ahmed) Marks a notification as read for the current user.
        /// </summary>
        /// <param name="id">Notification ID.</param>
        /// <param name="ct">Cancellation token.</param>
        [HttpPut("{id:guid}/read")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken ct = default)
        {
            if (id == Guid.Empty)
            {
                var bad = ApiResponse<bool>.FailureResult("Notification ID cannot be empty.");
                return BadRequest(bad);
            }

            _logger.LogInformation("Marking notification {NotificationId} as read.", id);
            var result = await _mediator.Send(new MarkNotificationAsReadCommand(id), ct);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
