using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using devbuddy.Desktop.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace devbuddy.Desktop
{
    internal static class DependencyInjection
    {
        public static void ConfigureDesktopServices(this IServiceCollection services)
        {
            services.AddWpfBlazorWebView();

#if DEBUG
            services.AddBlazorWebViewDeveloperTools();
#endif
        }

        public static void ConfigureServerServices(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();

            services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.WriteIndented = true;
                options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });

            services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("fixed", opt =>
                {
                    opt.PermitLimit = 100;
                    opt.Window = TimeSpan.FromMinutes(1);
                });
            });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigins",
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:3000")
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
            });

#if DEBUG
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "DevBuddy API",
                    Version = "v1",
                    Description = "API per DevBuddy Desktop"
                });
            });
#endif
        }

        public static void ConfigureDesktopConfiguraton(this WebApplication app)
        {
#if DEBUG
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "DevBuddy API V1");
            });
#endif
            app.UseCors("AllowSpecificOrigins");
            app.UseRateLimiter();
            // Error handling globale
            app.UseExceptionHandler(exceptionHandlerApp =>
            {
                exceptionHandlerApp.Run(async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "Si è verificato un errore interno.",
                        timestamp = DateTime.UtcNow
                    });
                });
            });

            APIs.ConfigureApiEndpoints(app);
        }
    }
}
