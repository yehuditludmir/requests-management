using System.ComponentModel.DataAnnotations;
using RequestsManagement.DTO;
using RequestsManagement.DTO.Enums;
using RequestsManagement.BL.Services;
using Microsoft.AspNetCore.Mvc;

namespace RequestsManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestsController : ControllerBase
    {
        private readonly IRequestService _requestService;

        public RequestsController(IRequestService requestService)
        {
            _requestService = requestService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<RequestDto>>> Get(
            [FromQuery] RequestQueryDto query, CancellationToken cancellationToken)
        {
            var result = await _requestService.GetPageAsync(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("stats")]
        public async Task<ActionResult<RequestStatsDto>> GetStats(
            [FromQuery] RequestFilterDto filter, CancellationToken cancellationToken)
        {
            var stats = await _requestService.GetStatsAsync(filter, cancellationToken);
            return Ok(stats);
        }

        // מעדכן סטטוס של פנייה אחת. 404 אם לא קיימת, 400 במעבר אסור, 409 בהתנגשות
        [HttpPatch("{id:int}/status")]
        public async Task<ActionResult<RequestDto>> UpdateStatus(
            int id,
            UpdateStatusDto update,
            [FromHeader(Name = "X-User"), Required(ErrorMessage = "The X-User header is required."), MaxLength(100)] string changedBy,
            CancellationToken cancellationToken)
        {
            var result = await _requestService.UpdateStatusAsync(id, update, changedBy, cancellationToken);

            switch (result.Outcome)
            {
                case UpdateStatusOutcome.NotFound:
                    return NotFound();

                case UpdateStatusOutcome.InvalidTransition:
                    ModelState.AddModelError(nameof(UpdateStatusDto.Status), result.Error!);
                    return ValidationProblem();

                case UpdateStatusOutcome.Conflict:
                    return Problem(statusCode: StatusCodes.Status409Conflict, title: result.Error);

                default:
                    return Ok(result.Request);
            }
        }

        // מעדכן סטטוס של כמה פניות. מחזיר אילו הצליחו ואילו נכשלו (Partial Success)
        [HttpPatch("status")]
        public async Task<ActionResult<BulkUpdateStatusResultDto>> BulkUpdateStatus(
            BulkUpdateStatusDto update,
            [FromHeader(Name = "X-User"), Required(ErrorMessage = "The X-User header is required."), MaxLength(100)] string changedBy,
            CancellationToken cancellationToken)
        {
            var result = await _requestService.BulkUpdateStatusAsync(update, changedBy, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:int}/history")]
        public async Task<ActionResult<IReadOnlyList<RequestStatusChangeDto>>> GetHistory(int id, CancellationToken cancellationToken)
        {
            var history = await _requestService.GetHistoryAsync(id, cancellationToken);
            return history is null ? NotFound() : Ok(history);
        }
    }
}
