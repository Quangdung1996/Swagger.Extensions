using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace QD.Swagger.Extensions
{
    internal class SwaggerDefaultValues : IOperationFilter
    {
        /// <summary>
        ///     Applies the filter to the specified operation using the given context.
        /// </summary>
        /// <param name="operation">The operation to apply the filter to.</param>
        /// <param name="context">The current operation filter context.</param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var apiDescription = context.ApiDescription;

            operation.Deprecated |= apiDescription.IsDeprecated();

            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            foreach (var parameter in operation.Parameters)
            {
                var description = apiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);

                if (parameter.Description == null)
                    parameter.Description = description.ModelMetadata?.Description;

                if (parameter.Schema.Default == null && description.DefaultValue != null)
                    parameter.Schema.Default = new OpenApiString(description.DefaultValue.ToString());

                parameter.Required |= description.IsRequired;
            }

            if (operation.Responses.ContainsKey("401"))
            {
                if (operation.Responses["401"].Content?.Any(x => x.Value.Schema.Reference.Id == "ProblemDetails") == true)
                {
                    operation.Responses["401"].Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Schema = context.SchemaGenerator.GenerateSchema(typeof(Error401Response), context.SchemaRepository)
                        }
                    };
                }
            }

            if (operation.Responses.ContainsKey("403"))
            {
                if (operation.Responses["403"].Content?.Any(x => x.Value.Schema.Reference.Id == "ProblemDetails") == true)
                {
                    operation.Responses["403"].Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Schema = context.SchemaGenerator.GenerateSchema(typeof(Error403Response), context.SchemaRepository)
                        }
                    };
                }
            }
        }
    }
}