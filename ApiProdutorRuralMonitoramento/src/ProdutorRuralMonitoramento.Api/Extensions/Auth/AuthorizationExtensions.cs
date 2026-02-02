using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace ProdutorRuralMonitoramento.Api.Extensions.Auth
{
        [ExcludeFromCodeCoverage]
        public static class AuthorizationExtensions
        {
            public static IServiceCollection AddAuthorizationExtension(this IServiceCollection services, IConfiguration configure)
            {
                var jwtSettings = configure.GetSection("JwtSettings");
                var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]);

                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidAudience = jwtSettings["Audience"],
                        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key)
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            return Task.CompletedTask;
                        }
                    };
                });

                services.AddAuthorization();

                return services;
            }
        }
    }

