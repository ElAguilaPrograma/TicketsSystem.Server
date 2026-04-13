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
            {
                var success = result.Successes.OfType<AppSuccess>().FirstOrDefault();
                if (success != null && success.Metadata.TryGetValue("SuccessCode", out var code))
                {
                    return code switch
                    {
                        201 => StatusCode(201, new { message = success.Message }),
                        202 => StatusCode(202, new { message = success.Message }),
                        204 => NoContent(),
                        _ => Ok(new { message = success.Message })
                    };
                }
                return Ok();
            }

            if (result.HasError<NotFoundError>())
                return NotFound(new { error = result.Errors.First().Message });

            if (result.HasError<ForbiddenError>())
                return StatusCode(403, new { error = result.Errors.First().Message });

            if (result.HasError<UnauthorizedError>())
                return Unauthorized(new { error = result.Errors.First().Message });

            return BadRequest(new { errors = result.Errors.Select(e => e.Message) });
        }

        protected IActionResult ProcessResult<T>(Result<T> result)
        {
            if (result.IsFailed)
            {
                if (result.HasError<NotFoundError>())
                    return NotFound(new { error = result.Errors.First().Message });

                if (result.HasError<ForbiddenError>())
                    return StatusCode(403, new { error = result.Errors.First().Message });


                if (result.HasError<UnauthorizedError>())
                    return Unauthorized(new { error = result.Errors.First().Message });

                return BadRequest(new { errors = result.Errors.Select(e => e.Message) });
            }

            return Ok(result.Value);
        }
    }
}
