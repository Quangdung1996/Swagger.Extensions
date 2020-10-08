using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using TwentyFive.Core.Entities;

namespace QD.Swagger.Extensions
{
    /// <summary>
    ///     Configures the Swagger generation options.
    /// </summary>
    /// <remarks>
    ///     This allows API versioning to define a Swagger document per API version after the
    ///     <see cref="IApiVersionDescriptionProvider" /> service has been resolved from the service container.
    /// </remarks>
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        readonly Action<SwaggerGenOptions> _additionalOptions;
        readonly ApplicationPartManager _partManager;
        readonly IApiVersionDescriptionProvider _provider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConfigureSwaggerOptions" /> class.
        /// </summary>
        /// <param name="provider">
        ///     The <see cref="IApiVersionDescriptionProvider">provider</see> used to generate Swagger
        ///     documents.
        /// </param>
        /// <param name="partManager"></param>
        /// <param name="additionalOptions"></param>
        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider, ApplicationPartManager partManager, Action<SwaggerGenOptions> additionalOptions)
        {
            _provider = provider;
            _partManager = partManager;
            _additionalOptions = additionalOptions;
        }

        /// <inheritdoc />
        public void Configure(SwaggerGenOptions options)
        {
            //// add a swagger document for each discovered API version
            //// note: you might choose to skip or document deprecated API versions differently
            foreach (var description in _provider.ApiVersionDescriptions)
                options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));

            options.DocInclusionPredicate(
                (documentName, apiDescription) =>
                {
                    var actionApiVersionModel = apiDescription.ActionDescriptor
                        .GetApiVersionModel(ApiVersionMapping.Explicit | ApiVersionMapping.Implicit);

                    var groupName = (apiDescription.ActionDescriptor.EndpointMetadata.FirstOrDefault(x => x.GetType() == typeof(ApiExplorerSettingsAttribute)) as ApiExplorerSettingsAttribute)?.GroupName;

                    if (groupName == null)
                        return documentName.StartsWith("Central");

                    if (actionApiVersionModel.DeclaredApiVersions.Any())
                        return actionApiVersionModel.DeclaredApiVersions.Any(
                            v => v.ToString(groupName, CultureInfo.CurrentCulture) == documentName);

                    return actionApiVersionModel.ImplementedApiVersions.Any(v => v.ToString(groupName, CultureInfo.CurrentCulture) == documentName);
                });

            IncludeXml(options, "QD.Swagger.Extensions");


            foreach (var applicationPart in _partManager.ApplicationParts.Select(x => x.Name).Distinct())
            {
                IncludeXml(options, applicationPart);
            }

            options.AddSecurityDefinition(
                "Bearer",
                new OpenApiSecurityScheme
                {
                    Description =
                        "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.<br><br>Example: 'Bearer 12345abcdef'<br>",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

            options.AddSecurityRequirement(
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference {Type = ReferenceType.SecurityScheme, Id = "Bearer"},
                            In = ParameterLocation.Header,
                            Name = "Bearer"
                        },
                        new List<string>()
                    }
                });

            options.EnableAnnotations();

            options.OperationFilter<SwaggerDefaultValues>();

            options.MapType<LocateString>(
                () => new OpenApiSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, OpenApiSchema>
                    {
                        ["vi"] = new OpenApiSchema { Type = "string" },
                        ["en"] = new OpenApiSchema { Type = "string" }
                    },
                    Description = "LocateString",
                    Required = new SortedSet<string> { "vi" }
                });

            _additionalOptions?.Invoke(options);
        }

        static void IncludeXml(SwaggerGenOptions options, string applicationPart)
        {
            var xmlFile = $"{applicationPart}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath);
        }

        static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
        {
            var info = new OpenApiInfo
            {
                Title = description.GroupName,
                Version = description.ApiVersion.ToString(),
                Contact = new OpenApiContact { Name = "Dung Bui", Email = "quangdung199697@gmail.com" }
            };

            if (description.IsDeprecated)
                info.Description += " This API version has been deprecated.";

            return info;
        }
    }
}
