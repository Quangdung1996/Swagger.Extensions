using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace QD.Swagger.Extensions
{
    internal class SwaggerUploadFileFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (!(operation?.RequestBody?.Content?.Any(x => x.Key.ToLower() == "multipart/form-data") ?? false)) return;

            var uploadFiles = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
                .Union(context.MethodInfo.GetCustomAttributes(true))
                .OfType<SwaggerUploadFile>();

            if (uploadFiles.Count() == 0) return;

            var uploadFile = uploadFiles.First();

            operation.RequestBody.Content["multipart/form-data"].Schema.Properties =
                new Dictionary<string, OpenApiSchema>
                {
                    [uploadFile.Parameter] = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary",
                        Description = uploadFile.Description
                    }
                };

            if (!string.IsNullOrEmpty(uploadFile.Example))
            {
                operation.RequestBody.Content["multipart/form-data"].Schema.Example = new OpenApiString(uploadFile.Example);
                operation.RequestBody.Content["multipart/form-data"].Schema.Description = uploadFile.Example;
            }
        }

        private class SwaggerUploadFile
        {
            public string Parameter { get; set; }

            public string Description { get; set; }

            public string Example { get; set; }
        }
    }
}