using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using FluentValidation;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.Common.Validation; 

namespace Ambev.DeveloperEvaluation.WebApi.Filters;

/// <summary>
/// Action filter to handle validation automatically and reduce code duplication
/// </summary>
public class ValidationActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Continue normal execution if no validation errors
        await next();
    }

    /// <summary>
    /// Helper method to validate request objects
    /// </summary>
    public static async Task<IActionResult?> ValidateRequest<T>(T request, CancellationToken cancellationToken)
        where T : class
    {
        var validatorType = typeof(IValidator<>).MakeGenericType(typeof(T));

        if (Activator.CreateInstance(validatorType) is not IValidator validator)
        {
            return null;
        }

        var validationContext = new ValidationContext<T>(request);
        var validationResult = await validator.ValidateAsync(validationContext, cancellationToken);

        if (!validationResult.IsValid)
        {
            return new BadRequestObjectResult(new ApiResponse
            {
                Success = false,
                Message = "Validation Failed",
                Errors = validationResult.Errors.Select(e => new ValidationErrorDetail
                {
                    Error = e.PropertyName,
                    Detail = e.ErrorMessage
                })
            });
        }

        return null;
    }
}
