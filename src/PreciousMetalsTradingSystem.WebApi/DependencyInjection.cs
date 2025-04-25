using PreciousMetalsTradingSystem.WebApi.AppConfigSetup;
using PreciousMetalsTradingSystem.WebApi.Filters;
using PreciousMetalsTradingSystem.WebApi.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Serilog;
using System.Text.Json.Serialization;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using WebMotions.Fake.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Throw;
using PreciousMetalsTradingSystem.Application.AMark.Services;
using PreciousMetalsTradingSystem.Infrastructure.Services;
using PreciousMetalsTradingSystem.WebApi.Common.Authorization.Services;
using PreciousMetalsTradingSystem.Infrastructure.SignalR.Hubs;
using PreciousMetalsTradingSystem.Application.Common.Options;

namespace PreciousMetalsTradingSystem.WebApi
{
    public static class DependencyInjection
    {
        private const string CORS_POLICY_NAME = "DefaultCorsPolicy";

        public static IServiceCollection AddWebServices(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddControllers()
                .AddCors(configuration)
                .AddServices()
                .AddAuthentication(configuration)
                .AddAuthorization()
                .AddOpenAPI()
                .AddAppConfig()
                .AddMiddlewares()
                .AddAMarkTradingService(configuration)
                .AddHttpContextAccessor();
        }

        public static void AddSerilog(this WebApplicationBuilder builder)
        {
            builder.Host.UseSerilog((context, configuration) =>
            configuration.ReadFrom.Configuration(context.Configuration));
        }

        public static void UseWebServices(this WebApplication app)
        {
            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            //    app.UseSwagger();
            //    app.UseSwaggerUI();
            //}

            app.UseCors(CORS_POLICY_NAME);
            app.UseOpenAPI();
            app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
            app.UseMiddleware<ContextEnrichmentMiddleware>();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<PermissionAuthorizationMiddleware>();
            app.MapControllers();
            app.MapHub<ActivityHub>("/activityhub");
            app.MapHub<ProductsHub>("/productshub");
            app.MapHub<InventoryHub>("/inventoryhub");
            app.MapHub<HedgingHub>("/hedginghub");
            app.MapHub<FinancialsHub>("/financialshub");

        }

        #region Private

        private static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddSingleton<IRolePermissionService, RolePermissionService>();

            return services;
        }

        private static IServiceCollection AddMiddlewares(this IServiceCollection services)
        {
            services.AddTransient<GlobalExceptionHandlingMiddleware>();
            services.AddTransient<PermissionAuthorizationMiddleware>();
            services.AddTransient<HttpClientLoggingMiddleware>();
            
            return services;
        }

        private static IServiceCollection AddAppConfig(this IServiceCollection services)
        {
            services.ConfigureOptions<DatabaseOptionsSetup>();
            services.ConfigureOptions<HostOptionsSetup>();
            services.ConfigureOptions<AMarkOptionsSetup>();
            services.ConfigureOptions<ApiSettingsOptionsSetup>();
            services.ConfigureOptions<HangfireOptionsSetup>();
            services.ConfigureOptions<TradeConfirmationEmailOptionsSetup>();
            services.ConfigureOptions<MailGunOptionsSetup>();

            return services;
        }

        // TODO: Move auth conif to Infrastructure
        private static IServiceCollection AddAuthentication(
            this IServiceCollection services, IConfiguration configuration)
        {
            var hostOptions = new Options.HostOptions();
            configuration.GetSection(HostOptionsSetup.SectionName).Bind(hostOptions);

            if (hostOptions.UseMockAuthentication)
            {
                // NOTE: Here we can configure specifics for our mock auth
                //       Like token, claims, roles, permissions
                services.AddAuthentication(FakeJwtBearerDefaults.AuthenticationScheme)
                    .AddFakeJwtBearer(options =>
                    {
                        options.IncludeErrorDetails = true;
                        options.BearerValueType = FakeJwtBearerBearerValueType.Jwt;
                    });

                services.Configure<FakeJwtBearerOptions>(FakeJwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.Events = new WebMotions.Fake.Authentication.JwtBearer.Events.JwtBearerEvents
                    {
                        OnChallenge = context =>
                        {
                            // Handle the challenge when user is not authenticated or the token is invalid
                            context.HandleResponse();  // Prevent default challenge response
                            throw new UnauthorizedAccessException("Token is invalid or user is not authenticated.");
                        },
                        // We have to hook the OnMessageReceived event in order to
                        // allow the JWT authentication handler to read the access
                        // token from the query string when a WebSocket or 
                        // Server-Sent Events request comes in.

                        // Sending the access token in the query string is required when using WebSockets or ServerSentEvents
                        // due to a limitation in Browser APIs. We restrict it to only calls to the
                        // SignalR hub in this code.
                        // See https://docs.microsoft.com/aspnet/core/signalr/security#access-token-logging
                        // for more information about security considerations when using
                        // the query string to transmit the access token.
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];

                            // If the request is for our hub...
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken)
                                && path.HasValue
                                && path.Value!.Contains("hub"))
                            {
                                // Read the token out of the query string
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });
            }
            else
            {
                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme) //OpenIdConnectDefaults.AuthenticationScheme
                        .AddMicrosoftIdentityWebApi(configuration.GetSection("AzureAd"));

                services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.Events = new JwtBearerEvents
                    {
                        // TODO: Revise config below, create more appropiate messages, maybe log exceptions...
                        OnAuthenticationFailed = context =>
                        {
                            throw new UnauthorizedAccessException("Invalid authentication token.");
                        },
                        OnChallenge = context =>
                        {
                            context.HandleResponse();  // Prevent default challenge response
                            throw new UnauthorizedAccessException("User is not authenticated.");
                        },
                        // We have to hook the OnMessageReceived event in order to
                        // allow the JWT authentication handler to read the access
                        // token from the query string when a WebSocket or 
                        // Server-Sent Events request comes in.

                        // Sending the access token in the query string is required when using WebSockets or ServerSentEvents
                        // due to a limitation in Browser APIs. We restrict it to only calls to the
                        // SignalR hub in this code.
                        // See https://docs.microsoft.com/aspnet/core/signalr/security#access-token-logging
                        // for more information about security considerations when using
                        // the query string to transmit the access token.
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];

                            // If the request is for our hub...
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) 
                                && path.HasValue 
                                && path.Value!.Contains("hub"))
                            {
                                // Read the token out of the query string
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });
            }

            return services;
        }

        private static IServiceCollection AddControllers(this IServiceCollection services)
        {
            services.AddControllers(options =>
            {
                options.Filters.Add<ModelBindingValidationFilter>();
                options.Filters.Add<RouteToRequestPropertyMappingFilter>();
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            return services;
        }

        private static IServiceCollection AddOpenAPI(this IServiceCollection services)
        {
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Header,
                    Description = "Please insert JWT with Bearer into field"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                options.SchemaFilter<OpenApiExcludeSchemaFilter>();
                options.OperationFilter<OpenApiExcludeOperationFilter>();
            });
            return services;
        }

        private static IApplicationBuilder UseOpenAPI(this IApplicationBuilder app)
        {
            var hostOptions = app.ApplicationServices.GetRequiredService<IOptions<Options.HostOptions>>().Value;

            if (hostOptions.UseOpenApi)
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            return app;
        }

        private static IServiceCollection AddCors(this IServiceCollection services, IConfiguration configuration)
        {
            var hostOptions = new Options.HostOptions();
            configuration.GetSection(HostOptionsSetup.SectionName).Bind(hostOptions);

            var allowedOrigins = hostOptions
                .AllowedOrigins
                .ThrowIfNull()
                .Value
                .Split(";")
                .ToArray();

            services.AddCors(corsOptions =>
            {
                corsOptions.AddPolicy(CORS_POLICY_NAME, builder =>
                {
                    builder
                        .WithOrigins(allowedOrigins)
                        .AllowCredentials()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            return services;
        }

        private static IServiceCollection AddAMarkTradingService(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var apiSettingsOptions = new ApiSettingsOptions();
            configuration.GetSection(ApiSettingsOptionsSetup.SectionName).Bind(apiSettingsOptions);

            if (apiSettingsOptions.UseMockAMarkTradingService)
            {
                services.AddScoped<IAMarkTradingService, AMarkTradingServiceMock>();
            }
            else
            {
                services.AddHttpClient<IAMarkTradingService, AMarkTradingService>()
                   .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                   {
                       // ServerCertificateCustomValidationCallback: By setting this callback to always return true,
                       // you bypass SSL certificate validation for all requests made by this HttpClient.

                       // Security Note: This setup should only be used in development environments or controlled
                       // testing scenarios.For production, you should address SSL certificate validation properly
                       // to avoid security vulnerabilities.
                       ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
                   })
                   .AddHttpMessageHandler<HttpClientLoggingMiddleware>();
            }
           
            return services;
        }

        #endregion
    }
}
