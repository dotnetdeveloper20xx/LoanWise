using LoanWise.Application.Features.Admin.Commands.ApproveLoan;
using LoanWise.Application.Features.Admin.Commands.RejectLoan;
using LoanWise.Application.Features.Repayments.Commands.CheckOverdueRepayments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreBoost.Application.Common.Models;

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
        /// Approves a pending loan (Admin only).
        /// </summary>
        [HttpPost("loans/{loanId:guid}/approve")]
        [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ApproveLoan(Guid loanId, CancellationToken ct)
        {
            ApiResponse<Guid> res = await _mediator.Send(new ApproveLoanCommand(loanId), ct);
            return res.Success ? Ok(res) : BadRequest(res);
        }

        /// <summary>
        /// Rejects a pending loan with an optional reason (Admin only).
        /// </summary>
        [HttpPost("loans/{loanId:guid}/reject")]
        [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RejectLoan(Guid loanId, [FromBody] string Reason, CancellationToken ct)
        {
            ApiResponse<Guid> res = await _mediator.Send(new RejectLoanCommand(loanId, Reason), ct);
            return res.Success ? Ok(res) : BadRequest(res);
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
