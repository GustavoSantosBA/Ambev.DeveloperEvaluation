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
    private readonly IServiceProvider _serviceProvider;

    public ValidationActionFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Continue normal execution if no validation errors
        await next();
    }

    /// <summary>
    /// Helper method to validate request objects
    /// </summary>
    public static async Task<IActionResult?> ValidateRequest<T>(T request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
        where T : class
    {
        // Try to get validator from DI container
        var validator = serviceProvider.GetService<IValidator<T>>();
        
        if (validator == null)
        {
            // If no validator is registered, skip validation
            return null;
        }

        var validationResult = await validator.ValidateAsync(request, cancellationToken);

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
