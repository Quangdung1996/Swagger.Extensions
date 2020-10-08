using Microsoft.AspNetCore.Mvc;
using System;

#pragma warning disable IDE1006 // Naming Styles

namespace QD.Swagger.Extensions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class ProducesDataNotFoundAttribute : ProducesResponseTypeAttribute
    {
        public ProducesDataNotFoundAttribute() : base(typeof(ErrorDataNotFoundResponse), 400)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class ProducesDataInvalidAttribute : ProducesResponseTypeAttribute
    {
        public ProducesDataInvalidAttribute() : base(typeof(ErrorInvalidDataResponse), 400)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class ProducesSuccessAttribute : ProducesResponseTypeAttribute
    {
        public ProducesSuccessAttribute(Type type) : base(typeof(SuccessResponse<>).MakeGenericType(type), 200)
        {
        }

        public ProducesSuccessAttribute() : base(typeof(SuccessResponse), 200)
        {
        }
    }

    public class SuccessResponse
    {
        /// <example>true</example>
        public bool successful { get; set; }
    }

    public class SuccessResponse<T> where T : class, new()
    {
        public SuccessResponse()
        {
        }

        public SuccessResponse(T example)
        {
            successful = true;
            data = example;
        }

        /// <example>true</example>
        public bool successful { get; set; }

        public T data { get; set; }
    }

    public class ErrorInvalidDataResponse
    {
        /// <example>false</example>
        public bool successful { get; set; } = false;

        /// <example>1003</example>
        public int errorCode { get; set; }

        /// <example>INVALID_DATA</example>
        public string errorDescription { get; set; }
    }

    public class Error401Response
    {
        /// <example>false</example>
        public bool successful { get; set; } = false;

        /// <example>401</example>
        public int errorCode { get; set; }

        /// <example>Unauthorized</example>
        public string errorDescription { get; set; }
    }

    public class Error403Response
    {
        /// <example>false</example>
        public bool successful { get; set; } = false;

        /// <example>403</example>
        public int errorCode { get; set; }

        /// <example>Forbidden</example>
        public string errorDescription { get; set; }
    }

    public class ErrorDataNotFoundResponse
    {
        /// <example>false</example>
        public bool successful { get; set; } = false;

        /// <example>2001</example>
        public int errorCode { get; set; }

        /// <example>ENTITY_DATA_NOT_FOUND</example>
        public string errorDescription { get; set; }
    }
}

#pragma warning restore IDE1006 // Naming Styles