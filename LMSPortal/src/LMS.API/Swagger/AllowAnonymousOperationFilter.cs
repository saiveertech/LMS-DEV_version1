using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LMS.API.Swagger;

// The global security requirement in Program.cs applies to every operation
// by default. This clears it for actions/controllers marked [AllowAnonymous]
// so Swagger UI doesn't prompt for a token on endpoints that don't need one.
public class AllowAnonymousOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var isAnonymous =
            context.MethodInfo.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any() ||
            (context.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any() ?? false);

        if (isAnonymous)
        {
            operation.Security = new List<OpenApiSecurityRequirement>();
        }
    }
}
