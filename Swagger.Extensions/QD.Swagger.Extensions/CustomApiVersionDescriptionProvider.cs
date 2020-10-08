using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using static Microsoft.Extensions.DependencyInjection.ServiceDescriptor;
using static System.Globalization.CultureInfo;
using static Microsoft.AspNetCore.Mvc.Versioning.ApiVersionMapping;

namespace QD.Swagger.Extensions
{
    internal class CustomApiVersionDescriptionProvider : IApiVersionDescriptionProvider
    {
        readonly Lazy<IReadOnlyList<ApiVersionDescription>> apiVersionDescriptions;
        readonly IOptions<ApiExplorerOptions> options;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DefaultApiVersionDescriptionProvider" /> class.
        /// </summary>
        /// <param name="actionDescriptorCollectionProvider">
        ///     The <see cref="IActionDescriptorCollectionProvider">provider</see>
        ///     used to enumerate the actions within an application.
        /// </param>
        /// <param name="apiExplorerOptions">
        ///     The <see cref="IOptions{TOptions}">container</see> of configured
        ///     <see cref="ApiExplorerOptions">API explorer options</see>.
        /// </param>
        public CustomApiVersionDescriptionProvider(IActionDescriptorCollectionProvider actionDescriptorCollectionProvider, IOptions<ApiExplorerOptions> apiExplorerOptions)
        {
            apiVersionDescriptions = LazyApiVersionDescriptions.Create(this, actionDescriptorCollectionProvider);
            options = apiExplorerOptions;
        }

        /// <summary>
        ///     Gets a read-only list of discovered API version descriptions.
        /// </summary>
        /// <value>
        ///     A <see cref="IReadOnlyList{T}">read-only list</see> of
        ///     <see cref="ApiVersionDescription">API version descriptions</see>.
        /// </value>
        public IReadOnlyList<ApiVersionDescription> ApiVersionDescriptions => apiVersionDescriptions.Value;

        /// <summary>
        ///     Gets the options associated with the API explorer.
        /// </summary>
        /// <value>The current <see cref="ApiExplorerOptions">API explorer options</see>.</value>
        protected ApiExplorerOptions Options => options.Value;

        /// <summary>
        ///     Determines whether the specified action is deprecated for the provided API version.
        /// </summary>
        /// <param name="actionDescriptor">The <see cref="ActionDescriptor">action</see> to evaluate.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to evaluate.</param>
        /// <returns>
        ///     True if the specified <paramref name="actionDescriptor">action</paramref> is deprecated for the
        ///     <paramref name="apiVersion">API version</paramref>; otherwise, false.
        /// </returns>
        public virtual bool IsDeprecated(ActionDescriptor actionDescriptor, ApiVersion apiVersion)
        {
            var model = actionDescriptor.GetApiVersionModel();
            return !model.IsApiVersionNeutral && model.DeprecatedApiVersions.Contains(apiVersion);
        }

        /// <summary>
        ///     Enumerates all API versions within an application.
        /// </summary>
        /// <param name="actionDescriptorCollectionProvider">
        ///     The <see cref="IActionDescriptorCollectionProvider">provider</see>
        ///     used to enumerate the actions within an application.
        /// </param>
        /// <returns>
        ///     A <see cref="IReadOnlyList{T}">read-only list</see> of
        ///     <see cref="ApiVersionDescription">API version descriptions</see>.
        /// </returns>
        protected virtual IReadOnlyList<ApiVersionDescription> EnumerateApiVersions(IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
        {
            if (actionDescriptorCollectionProvider == null)
                throw new ArgumentNullException(nameof(actionDescriptorCollectionProvider));

            var supported = new HashSet<CustomApiVersion>();
            var deprecated = new HashSet<CustomApiVersion>();
            var descriptions = new List<ApiVersionDescription>();

            BucketizeApiVersions(actionDescriptorCollectionProvider.ActionDescriptors.Items, supported, deprecated);
            AppendDescriptions(descriptions, supported, false);
            AppendDescriptions(descriptions, deprecated, true);

            return descriptions.OrderBy(d => d.ApiVersion).ToArray();
        }

        internal static void AddApiExplorerServices(IServiceCollection services, Action<ApiExplorerOptions> setupAction)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddMvcCore().AddApiExplorer();
            services.TryAdd(Singleton<IOptionsFactory<ApiExplorerOptions>, ApiExplorerOptionsFactory<ApiExplorerOptions>>());
            services.TryAddSingleton<IApiVersionDescriptionProvider, CustomApiVersionDescriptionProvider>();
            services.TryAddEnumerable(Transient<IApiDescriptionProvider, VersionedApiDescriptionProvider>());
            services.Configure(setupAction);
        }

        void AppendDescriptions(ICollection<ApiVersionDescription> descriptions, IEnumerable<CustomApiVersion> versions, bool deprecated)
        {
            foreach (var version in versions)
            {
                var groupName = version.ToString(version.GroupName ?? Options.GroupNameFormat, CurrentCulture);
                descriptions.Add(new ApiVersionDescription(version, groupName, deprecated));
            }
        }

        void BucketizeApiVersions(IReadOnlyList<ActionDescriptor> actions, ISet<CustomApiVersion> supported, ISet<CustomApiVersion> deprecated)
        {
            var declared = new HashSet<CustomApiVersion>();
            var advertisedSupported = new HashSet<CustomApiVersion>();
            var advertisedDeprecated = new HashSet<CustomApiVersion>();

            foreach (var action in actions)
            {
                var model = action.GetApiVersionModel(Explicit | Implicit);
                var groupName = (action.EndpointMetadata.FirstOrDefault(x => x.GetType() == typeof(ApiExplorerSettingsAttribute)) as ApiExplorerSettingsAttribute)?.GroupName;

                foreach (var version in model.DeclaredApiVersions.Select(x => new CustomApiVersion(x, groupName)))
                    declared.Add(version);

                foreach (var version in model.SupportedApiVersions.Select(x => new CustomApiVersion(x, groupName)))
                {
                    supported.Add(version);
                    advertisedSupported.Add(version);
                }

                foreach (var version in model.DeprecatedApiVersions.Select(x => new CustomApiVersion(x, groupName)))
                {
                    deprecated.Add(version);
                    advertisedDeprecated.Add(version);
                }
            }

            advertisedSupported.ExceptWith(declared);
            advertisedDeprecated.ExceptWith(declared);
            supported.ExceptWith(advertisedSupported);
            deprecated.ExceptWith(supported.Concat(advertisedDeprecated));

            if (supported.Count == 0 && deprecated.Count == 0)
                supported.Add(new CustomApiVersion(Options.DefaultApiVersion, null));
        }

        sealed class LazyApiVersionDescriptions : Lazy<IReadOnlyList<ApiVersionDescription>>
        {
            readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;
            readonly CustomApiVersionDescriptionProvider _apiVersionDescriptionProvider;

            LazyApiVersionDescriptions(CustomApiVersionDescriptionProvider apiVersionDescriptionProvider, IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
            {
                _apiVersionDescriptionProvider = apiVersionDescriptionProvider;
                _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
            }

            internal static Lazy<IReadOnlyList<ApiVersionDescription>> Create(CustomApiVersionDescriptionProvider apiVersionDescriptionProvider, IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
            {
                var descriptions = new LazyApiVersionDescriptions(apiVersionDescriptionProvider, actionDescriptorCollectionProvider);
                return new Lazy<IReadOnlyList<ApiVersionDescription>>(descriptions.EnumerateApiVersions);
            }

            IReadOnlyList<ApiVersionDescription> EnumerateApiVersions()
            {
                return _apiVersionDescriptionProvider.EnumerateApiVersions(_actionDescriptorCollectionProvider);
            }
        }
    }
}
