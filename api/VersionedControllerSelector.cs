using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;

namespace api
{
    //TODO: Use the container for registration & resolution instead of custom "type cache"
    public class VersionedControllerSelector : IHttpControllerSelector
    {
        private const string ControllerKeyFormat = "{0}.{1}Controller";

        private readonly Dictionary<string, HttpControllerDescriptor> controllerTypeCache;

        public VersionedControllerSelector(HttpConfiguration configuration)
        {
            controllerTypeCache = new Dictionary<string, HttpControllerDescriptor>
                {
                    {
                        "v1.StudentsController", new HttpControllerDescriptor(
                            configuration,
                            "Students",
                            typeof (Controllers.v1.StudentsController))
                    },
                    {
                        "v2.StudentsController", new HttpControllerDescriptor(
                            configuration,
                            "Students",
                            typeof (Controllers.v2.StudentsController))
                    },
                    {
                        "default.StudentsController", new HttpControllerDescriptor(
                            configuration,
                            "Students",
                            typeof (Controllers.StudentsController))
                    }
                };
        }

        public HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            var versionHeaders = request.Headers.Accept
                                    .Where(header => header.MediaType.Contains("api.v"))
                                    .ToArray();
            var routeData = request.GetRouteData();
            object controllerName;

            var version = versionHeaders.Any() ? 
                "v" + versionHeaders.Select(ExtractApiVersion).Max() : 
                "default";

            routeData.Values.TryGetValue("controller", out controllerName);

            var controllerKey = string.Format(ControllerKeyFormat, version, controllerName);

            if (!controllerTypeCache.ContainsKey(controllerKey))
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            return controllerTypeCache[controllerKey];
        }

        private int ExtractApiVersion(MediaTypeWithQualityHeaderValue header)
        {
            var apiVersionIndex = header.MediaType.IndexOf("api.v", StringComparison.InvariantCultureIgnoreCase) + 5;
            var formatSpecificationIndex = header.MediaType.IndexOf("+", StringComparison.InvariantCultureIgnoreCase);
            var versionTextLength = formatSpecificationIndex - apiVersionIndex;

            return int.Parse(header.MediaType.Substring(apiVersionIndex, versionTextLength));
        }

        public IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
        {
            return controllerTypeCache;
        }
    }
}