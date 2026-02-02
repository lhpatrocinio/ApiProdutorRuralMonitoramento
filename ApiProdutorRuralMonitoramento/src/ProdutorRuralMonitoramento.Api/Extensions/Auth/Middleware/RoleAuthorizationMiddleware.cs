using Microsoft.AspNetCore.Authorization;

namespace ProdutorRuralMonitoramento.Api.Extensions.Auth.Middleware
{
    public class RoleAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;

        public RoleAuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();

            if (endpoint != null)
            {
                // Check if the endpoint allows anonymous access
                var allowAnonymousAttribute = endpoint.Metadata.GetMetadata<AllowAnonymousAttribute>();
                if (allowAnonymousAttribute != null)
                {
                    // Skip authorization for anonymous endpoints
                    await _next(context);
                    return;
                }

                var authorizeAttribute = endpoint.Metadata.GetMetadata<AuthorizeAttribute>();

                if (authorizeAttribute != null)
                {
                    if (!context.User.Identity?.IsAuthenticated ?? false)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("Token não autenticado");
                        return;
                    }

                    if (!string.IsNullOrEmpty(authorizeAttribute.Roles))
                    {
                        var allowedRoles = authorizeAttribute.Roles.Split(',');

                        var userHasRole = allowedRoles.Any(role => context.User.IsInRole(role.Trim()));

                        if (!userHasRole)
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            await context.Response.WriteAsync("Acesso negado. Permissão insuficiente.");
                            return;
                        }
                    }
                }
            }

            await _next(context);
        }
    }

}

