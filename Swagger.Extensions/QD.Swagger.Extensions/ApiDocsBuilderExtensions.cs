using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace QD.Swagger.Extensions
{
    public static class ApiDocsBuilderExtensions
    {
        public static IApplicationBuilder UseSwaggerForApiDocs(this IApplicationBuilder app, string documentTitle, bool useAuthorized = true)
        {
            if (useAuthorized)
            {
                //app.UseSwaggerAuthorized();
                app.UseAuthorization();
                app.UseAuthentication();
            }


            app.UseSwagger();

            app.UseSwaggerUI(
                options =>
                {
                    options.DocumentTitle = documentTitle;

                    options.IndexStream = () => typeof(ApiDocsBuilderExtensions).GetTypeInfo().Assembly.GetManifestResourceStream("QD.Swagger.Extensions.index.html");

                    foreach (var description in app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>().ApiVersionDescriptions)
                        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                });

            return app;
        }

        //internal static IApplicationBuilder UseSwaggerAuthorized(this IApplicationBuilder builder)
        //{
        //    return builder.UseMiddleware<SwaggerAuthorizedMiddleware>();
        //}
    }
}
