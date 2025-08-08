using LoanWise.Application.DTOs.Notifications;
using LoanWise.Application.Features.Notifications.Commands.MarkNotificationAsRead;
using LoanWise.Application.Features.Notifications.Queries.GetNotifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Api.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public NotificationsController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<NotificationDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyNotifications()
        {
            var result = await _mediator.Send(new GetNotificationsQuery());
            return Ok(result);
        }

        [HttpPut("{id}/read")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var result = await _mediator.Send(new MarkNotificationAsReadCommand(id));
            return Ok(result);
        }
    }
}
