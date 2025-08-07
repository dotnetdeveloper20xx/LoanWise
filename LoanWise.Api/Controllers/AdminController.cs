using LoanWise.Application.Features.Repayments.Commands.CheckOverdueRepayments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanWise.Api.Controllers
{
    [Authorize(Roles = "Admin")] // Only admins can access this controller
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Triggers overdue check for all repayments in all loans. Admin-only.
        /// </summary>
        [HttpPost("repayments/check-overdue")]
        public async Task<IActionResult> CheckOverdueRepayments()
        {
            var response = await _mediator.Send(new CheckOverdueRepaymentsCommand());
            return response.Success ? Ok(response) : BadRequest(response);
        }
    }
}
