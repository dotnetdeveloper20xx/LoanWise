using LoanWise.Application.DTOs.Metadata;
using LoanWise.Application.Features.Metadata.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Api.Controllers
{
    [ApiController]
    [Route("api/metadata")]
    [AllowAnonymous] // Anyone (even not logged in) can get enum lists
    public class MetadataController : ControllerBase
    {
        private readonly IMediator _mediator;
        public MetadataController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Gets all loan statuses.
        /// </summary>
        [HttpGet("loan-statuses")]
        [ProducesResponseType(typeof(ApiResponse<List<EnumItemDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLoanStatuses()
        {
            var result = await _mediator.Send(new GetLoanStatusesQuery());
            return Ok(result);
        }

        /// <summary>
        /// Gets all loan purposes.
        /// </summary>
        [HttpGet("loan-purposes")]
        [ProducesResponseType(typeof(ApiResponse<List<EnumItemDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLoanPurposes()
        {
            var result = await _mediator.Send(new GetLoanPurposesQuery());
            return Ok(result);
        }

        /// <summary>
        /// Gets all risk levels.
        /// </summary>
        [HttpGet("risk-levels")]
        [ProducesResponseType(typeof(ApiResponse<List<EnumItemDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRiskLevels()
        {
            var result = await _mediator.Send(new GetRiskLevelsQuery());
            return Ok(result);
        }
    }
}
