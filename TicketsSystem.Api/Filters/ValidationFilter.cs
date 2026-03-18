using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TicketsSystem.Api.Filters
{
    public class ValidationFilter : IAsyncActionFilter
    {
        private readonly IServiceProvider _serviceProvider;

        public ValidationFilter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var errors = new List<string>();

            foreach (var argument in context.ActionArguments.Values)
            {
                if (argument == null)
                    continue;

                var argumentType = argument.GetType();
                var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);
                var validator = _serviceProvider.GetService(validatorType);

                if (validator == null)
                    continue;

                var validateMethod = validatorType.GetMethod("ValidateAsync",
                    new[] { argumentType, typeof(CancellationToken) });

                if (validateMethod == null)
                    continue;

                var validationResult = await (Task<FluentValidation.Results.ValidationResult>)
                    validateMethod.Invoke(validator, new[] { argument, CancellationToken.None })!;

                if (!validationResult.IsValid)
                {
                    errors.AddRange(validationResult.Errors.Select(e => e.ErrorMessage));
                }
            }

            if (errors.Count > 0)
            {
                context.Result = new BadRequestObjectResult(new { errors });
                return;
            }

            await next();
        }
    }
}
