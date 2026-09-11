using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace PAS.AspNetCore.Endpoints;

public class FluentValidationFilterFactory {

    /// <summary>
    /// A filter factory that automatically detects and applies FluentValidation validators 
    /// to Minimal API endpoints based on their request DTO types.
    /// </summary>
    public static EndpointFilterDelegate CreateFactory(
        EndpointFilterFactoryContext filterContext,
        EndpointFilterDelegate next
    ) {
        var parameters = filterContext.MethodInfo.GetParameters();

        for (int i = 0; i < parameters.Length; i++) {
            var parameterType = parameters[i].ParameterType;
            var validatorType = typeof(IValidator<>).MakeGenericType(parameterType);

            return async (invocationContext) => {
                if (invocationContext.HttpContext.RequestServices.GetService(validatorType) is IValidator validator) {
                    var argument = invocationContext.Arguments[i]!;
                    var validationContext = new ValidationContext<object>(argument);
                    var validationResult = await validator!.ValidateAsync(validationContext, invocationContext.HttpContext.RequestAborted);

                    if (!validationResult.IsValid) {
                        return Results.ValidationProblem(validationResult.ToDictionary());
                    }
                }

                return await next(invocationContext);
            };
        }

        return next;
    }
}
