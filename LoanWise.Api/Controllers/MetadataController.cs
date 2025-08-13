// --------------------------------------------------------------------------------------
// LoanWise.Api/Controllers/MetadataController.cs
// Author: Faz Ahmed
// Purpose: Public (anonymous) metadata endpoints for enum-backed dropdowns (loan statuses,
//          purposes, risk levels). Thin controller delegating to MediatR queries.
// Notes:
//  - Adds CancellationToken support.
//  - Explicit Produces + response types for Swagger.
//  - Safe, short-term response caching (tweak duration as needed).
// --------------------------------------------------------------------------------------

using LoanWise.Application.DTOs.Metadata;
using LoanWise.Application.Features.Metadata.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Api.Controllers
{
    [ApiController]
    [Route("api/metadata")]
    [AllowAnonymous] // Publicly accessible — used by pre-login forms
    [Produces("application/json")]
    
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any, NoStore = false)]
    public sealed class MetadataController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<MetadataController> _logger;

        public MetadataController(IMediator mediator, ILogger<MetadataController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// (Authored by Faz Ahmed) Gets all loan statuses.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        [HttpGet("loan-statuses")]
        [ProducesResponseType(typeof(ApiResponse<List<EnumItemDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLoanStatuses(CancellationToken ct = default)
        {
            _logger.LogDebug("Fetching metadata: loan statuses");
            var result = await _mediator.Send(new GetLoanStatusesQuery(), ct);
            return Ok(result); // Handlers return ApiResponse<T>
        }

        /// <summary>
        /// (Authored by Faz Ahmed) Gets all loan purposes.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        [HttpGet("loan-purposes")]
        [ProducesResponseType(typeof(ApiResponse<List<EnumItemDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLoanPurposes(CancellationToken ct = default)
        {
            _logger.LogDebug("Fetching metadata: loan purposes");
            var result = await _mediator.Send(new GetLoanPurposesQuery(), ct);
            return Ok(result);
        }

        /// <summary>
        /// (Authored by Faz Ahmed) Gets all risk levels.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        [HttpGet("risk-levels")]
        [ProducesResponseType(typeof(ApiResponse<List<EnumItemDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRiskLevels(CancellationToken ct = default)
        {
            _logger.LogDebug("Fetching metadata: risk levels");
            var result = await _mediator.Send(new GetRiskLevelsQuery(), ct);
            return Ok(result);
        }
    }
}
