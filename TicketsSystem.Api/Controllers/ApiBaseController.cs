using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TicketsSystem.Core.Errors;

namespace TicketsSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiBaseController : ControllerBase
    {
        protected IActionResult ProcessResult(Result result)
        {
            if (result.IsSuccess)
                return Ok();

            if (result.HasError<NotFoundError>())
                return NotFound(new { error = result.Errors.First().Message });

            if (result.HasError<ForbiddenError>())
                return Forbid();

            if (result.HasError<UnauthorizedError>())
                return Unauthorized(new { error = result.Errors.First().Message });

            return BadRequest(new { errors = result.Errors.Select(e => e.Message) });
        }

        protected IActionResult ProcessResult<T>(Result<T> result)
        {
            if (result.IsFailed)
                return BadRequest(new { error = result.Errors.First().Message });

            return Ok(result.Value);
        }
    }
}
