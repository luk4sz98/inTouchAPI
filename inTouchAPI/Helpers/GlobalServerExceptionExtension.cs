using Microsoft.AspNetCore.Diagnostics;
using System.Net;

namespace inTouchAPI.Helpers;

public static class GlobalServerExceptionExtension
{
    public static void ConfigureExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(appError =>
        {
            appError.Run(async context =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";
                var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (contextFeature != null)
                {
                    var error = new { StatusCode = (int)HttpStatusCode.InternalServerError, ErrorDescription = "Internal Server Error" };
                    await context.Response.WriteAsync($"StatusCode: {error.StatusCode}\nMessage: {error.ErrorDescription}.");
                }
            });
        });
    }
}
