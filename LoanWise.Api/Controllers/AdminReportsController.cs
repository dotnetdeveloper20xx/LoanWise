using LoanWise.Application.DTOs.Reports;
using LoanWise.Application.Features.Admin.Queries.GetFundingReport;
using LoanWise.Application.Features.Admin.Queries.GetLoanReport;
using LoanWise.Application.Features.Admin.Queries.GetRepaymentReport;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Api.Controllers
{
    [ApiController]
    [Route("api/admin/reports")]
    [Authorize(Roles = "Admin")]
    public class AdminReportsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AdminReportsController(IMediator mediator) => _mediator = mediator;

        [HttpGet("loans")]
        [ProducesResponseType(typeof(ApiResponse<List<LoanReportDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLoansReport()
        {
            var result = await _mediator.Send(new GetLoanReportQuery());
            return Ok(result);
        }

        [HttpGet("repayments")]
        [ProducesResponseType(typeof(ApiResponse<List<RepaymentReportDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRepaymentsReport()
        {
            var result = await _mediator.Send(new GetRepaymentReportQuery());
            return Ok(result);
        }

        [HttpGet("fundings")]
        [ProducesResponseType(typeof(ApiResponse<List<FundingReportDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFundingsReport()
        {
            var result = await _mediator.Send(new GetFundingReportQuery());
            return Ok(result);
        }


    }
}
