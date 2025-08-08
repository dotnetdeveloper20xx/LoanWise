using LoanWise.Application.DTOs.Loans;
using LoanWise.Application.Features.Dashboard.Queries.GetAdminLoanStats;
using LoanWise.Application.Features.Dashboard.Queries.GetBorrowerDashboard;
using LoanWise.Application.Features.Loans.Commands.ApplyLoan;
using LoanWise.Application.Features.Loans.Commands.DisburseLoan;
using LoanWise.Application.Features.Loans.Queries.GetBorrowerLoanHistory;
using LoanWise.Application.Features.Loans.Queries.GetLoansByBorrower;
using LoanWise.Application.Features.Loans.Queries.GetOpenLoans;
using LoanWise.Application.Features.Repayments.Queries.GetRepaymentsByLoanId;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/loans")]
    public class LoanController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LoanController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("apply")]
        [Authorize(Roles = "Borrower")]
        public async Task<IActionResult> ApplyLoan([FromBody] ApplyLoanCommand command)
        {
            ApiResponse<Guid> result = await _mediator.Send(command);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("open")]
        [Authorize(Roles = "Lender")]
        public async Task<IActionResult> GetOpenLoans()
        {
            var response = await _mediator.Send(new GetOpenLoansQuery());
            return response.Success ? Ok(response) : BadRequest(response);
        }

     
        [HttpGet("my")]
        [Authorize(Roles = "Borrower")]
        public async Task<IActionResult> GetLoansByBorrower()
        {
            var response = await _mediator.Send(new GetLoansByBorrowerQuery());
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPost("{loanId}/disburse")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DisburseLoan(Guid loanId)
        {
            var response = await _mediator.Send(new DisburseLoanCommand(loanId));
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpGet("{loanId}/repayments")]
        [Authorize(Roles = "Borrower")]
        public async Task<IActionResult> GetRepaymentsByLoanId(Guid loanId)
        {
            var result = await _mediator.Send(new GetRepaymentsByLoanIdQuery(loanId));
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("borrowers/dashboard")]
        [Authorize(Roles = "Borrower")]
        public async Task<IActionResult> GetBorrowerDashboard()
        {
            var response = await _mediator.Send(new GetBorrowerDashboardQuery());
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpGet("loans/stats")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetLoanStats()
        {
            var result = await _mediator.Send(new GetAdminLoanStatsQuery());
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("borrowers/history")]
        [Authorize(Roles = "Borrower")]
        [ProducesResponseType(typeof(ApiResponse<List<BorrowerLoanHistoryDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBorrowerLoanHistory([FromQuery] GetBorrowerLoanHistoryQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }


    }
}
