
using LoanWise.Application.Features.Dashboard.Queries.GetBorrowerDashboard;
using LoanWise.Application.Features.Loans.Commands.ApplyLoan;
using LoanWise.Application.Features.Loans.Commands.DisburseLoan;
using LoanWise.Application.Features.Loans.Queries.GetLoansByBorrower;
using LoanWise.Application.Features.Loans.Queries.GetOpenLoans;
using LoanWise.Application.Features.Repayments.Queries.GetRepaymentsByLoanId;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Api.Controllers
{
    [ApiController]
    [Route("api/loans")]
    public class LoanController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LoanController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Submits a loan application by a borrower.
        /// </summary>
        /// <remarks>
        /// Accepts borrower-submitted loan details and creates a new loan application
        /// in the system. The loan will initially be in a pending state until reviewed.
        /// </remarks>
        /// <param name="command">The loan application command containing amount, purpose, duration, etc.</param>
        /// <returns>
        /// A unique loan ID wrapped in an <see cref="ApiResponse{T}"/>.
        /// </returns>
        /// <response code="200">Returns the ID of the newly created loan.</response>
        /// <response code="400">Returns validation or processing errors.</response>
        [HttpPost("apply")]
        public async Task<IActionResult> ApplyLoan([FromBody] ApplyLoanCommand command)
        {
            ApiResponse<Guid> result = await _mediator.Send(command);

            return result.Success
                ? Ok(result)
                : BadRequest(result);
        }


        /// <summary>
        /// Retrieves all open loans that are available for funding.
        /// </summary>
        /// <remarks>
        /// This endpoint is intended for lenders to view loans that have been approved
        /// and are not yet fully funded or disbursed. Results include loan amount,
        /// amount already funded, duration, and purpose.
        /// </remarks>
        /// <returns>
        /// A list of open loans wrapped in an <see cref="ApiResponse{T}"/>.
        /// </returns>
        /// <response code="200">Returns the list of open loans.</response>
        /// <response code="400">Returns an error if the query fails.</response>
        [HttpGet("open")]
        public async Task<IActionResult> GetOpenLoans()
        {
            var response = await _mediator.Send(new GetOpenLoansQuery());
            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Retrieves all loans submitted by the specified borrower.
        /// </summary>
        /// <param name="borrowerId">The ID of the borrower.</param>
        /// <returns>A list of the borrower's loans with status and funded amount.</returns>
        /// <response code="200">Returns the borrower's loans.</response>
        /// <response code="400">If borrowerId is invalid or not found.</response>
        [HttpGet("my")]
        public async Task<IActionResult> GetLoansByBorrower([FromQuery] Guid borrowerId)
        {
            var response = await _mediator.Send(new GetLoansByBorrowerQuery(borrowerId));
            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Disburses a fully funded loan. Admin only.
        /// </summary>
        /// <param name="loanId">The ID of the loan to disburse.</param>
        /// <returns>
        /// Returns the loan ID if disbursement is successful.
        /// </returns>
        /// <response code="200">Loan disbursed successfully.</response>
        /// <response code="400">Loan not found or not eligible for disbursement.</response>
        [HttpPost("{loanId}/disburse")]
        public async Task<IActionResult> DisburseLoan(Guid loanId)
        {
            var response = await _mediator.Send(new DisburseLoanCommand(loanId));
            return response.Success ? Ok(response) : BadRequest(response);
        }


        /// <summary>
        /// Retrieves the repayment schedule for a specific loan.
        /// </summary>
        /// <param name="loanId">The loan ID.</param>
        /// <returns>List of repayment installments.</returns>
        /// <response code="200">Repayments found</response>
        /// <response code="400">Loan not found</response>
        [HttpGet("{loanId}/repayments")]
        public async Task<IActionResult> GetRepaymentsByLoanId(Guid loanId)
        {
            var result = await _mediator.Send(new GetRepaymentsByLoanIdQuery(loanId));
            return result.Success ? Ok(result) : BadRequest(result);
        }


        /// <summary>
        /// Returns borrower dashboard metrics such as loan counts and upcoming repayment.
        /// </summary>
        /// <param name="borrowerId">Borrower user ID</param>
        /// <returns>BorrowerDashboardDto</returns>
        /// <response code="200">Success</response>
        /// <response code="400">Invalid or not found</response>
        [HttpGet("borrowers/{borrowerId}/dashboard")]
        public async Task<IActionResult> GetBorrowerDashboard(Guid borrowerId)
        {
            var response = await _mediator.Send(new GetBorrowerDashboardQuery(borrowerId));
            return response.Success ? Ok(response) : BadRequest(response);
        }

    }
}
