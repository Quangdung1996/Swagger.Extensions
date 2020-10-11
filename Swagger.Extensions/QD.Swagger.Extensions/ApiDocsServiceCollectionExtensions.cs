using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;

namespace QD.Swagger.Extensions
{
    public static class ApiDocsServiceCollectionExtensions
    {
        public static void AddSwaggerForApiDocs(this IServiceCollection services, string groupNameFormat, Action<SwaggerGenOptions> additionalOptions)
        {
            CustomApiVersionDescriptionProvider.AddApiExplorerServices(
                services,
                options =>
                {
                    options.GroupNameFormat = groupNameFormat;
                    options.SubstituteApiVersionInUrl = true;
                });

            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>(
                    s =>
                        new ConfigureSwaggerOptions(s.GetRequiredService<IApiVersionDescriptionProvider>(), s.GetRequiredService<ApplicationPartManager>(), additionalOptions))
                .AddSwaggerGen()
                .AddSwaggerGenNewtonsoftSupport();

            services.AddApiVersioning(o =>
                {
                    o.AssumeDefaultVersionWhenUnspecified = true;
                    o.DefaultApiVersion = new ApiVersion(1, 0);
                });

            services.AddRouting(options => options.LowercaseUrls = true);

            services.Configure<MvcOptions>(options => { });

            services.AddControllers()
                  .AddNewtonsoftJson();
        }
    }
}