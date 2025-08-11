// LoanWise.Api/Controllers/BorrowersRiskController.cs
using LoanWise.Application.DTOs.Admin;
using LoanWise.Application.DTOs.Borrowers;
using LoanWise.Application.Features.Admin.Borrowers.Queries.ListByKycStatus;
using LoanWise.Application.Features.Borrowers.Commands.VerifyKyc;
using LoanWise.Application.Features.Borrowers.Queries.GetRiskSummary;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SendGrid.Helpers.Mail;
using StoreBoost.Application.Common.Models;
using System.Globalization;

namespace LoanWise.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class BorrowersRiskController : ControllerBase
{
    private readonly IMediator _mediator;
    public BorrowersRiskController(IMediator mediator) => _mediator = mediator;

    // Borrower or Admin can view
    [HttpGet("borrowers/{borrowerId:guid}/risk-summary")]
    [Authorize(Roles = "Borrower,Lender,Admin")]
    public async Task<ActionResult<ApiResponse<BorrowerRiskSummaryDto>>> GetRiskSummary(Guid borrowerId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetBorrowerRiskSummaryQuery(borrowerId), ct);
        return Ok(result);
    }

    // Admin triggers KYC
    [HttpPost("admin/kyc/{borrowerId:guid}/verify")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<string>>> VerifyKyc(Guid borrowerId, CancellationToken ct)
        => await _mediator.Send(new VerifyKycCommand(borrowerId), ct);

    //    Multi-status(comma) :
    ///api/admin/borrowers/by-kyc? status = Verified, Pending&sortBy=scoreAt&sortDir=desc&page=1&pageSize=25

    //Repeated keys:
    ///api/admin/borrowers/by-kyc? status = Verified & status = Pending & sortBy = name & sortDir = asc & search = ali
    //    [HttpGet("by-kyc")]
    public async Task<ActionResult<ApiResponse<BorrowerKycListResult>>> ListByKyc(
    [FromQuery] string[] status,          // e.g. status=Verified,Pending or repeated keys
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 25,
    [FromQuery] string? search = null,
    [FromQuery] string? sortBy = null,    // score|verifiedAt|name|risk|status|scoreAt|lastScoreAt
    [FromQuery] string sortDir = "desc",
    CancellationToken ct = default)
    {
        var statusesCsv = string.Join(',', status ?? Array.Empty<string>());
        var res = await _mediator.Send(
            new ListBorrowersByKycStatusQuery(statusesCsv, page, pageSize, search, sortBy, sortDir), ct);
        return Ok(res);
    }
}
