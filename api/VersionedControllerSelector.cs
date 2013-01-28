using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;
using api.Controllers.v2;

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
                            typeof (StudentsController))
                    },
                    {
                        "v2.StudentsController", new HttpControllerDescriptor(
                            configuration,
                            "Students",
                            typeof (StudentsController))
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
            MediaTypeWithQualityHeaderValue[] versionHeaders = request.Headers.Accept
                                                                      .Where(
                                                                          header => header.MediaType.Contains("api.v"))
                                                                      .ToArray();
            IHttpRouteData routeData = request.GetRouteData();
            object controllerName;

            string version = versionHeaders.Any()
                                 ? "v" + versionHeaders.Select(header => header.MediaType.Substring(
                                     header.MediaType.IndexOf("api.",
                                                              StringComparison.InvariantCultureIgnoreCase) + 4,
                                     2)).Max(v => int.Parse(v[v.Length - 1].ToString(CultureInfo.InvariantCulture)))
                                 : "default";

            routeData.Values.TryGetValue("controller", out controllerName);

            string controllerKey = string.Format(ControllerKeyFormat, version, controllerName);

            return controllerTypeCache[controllerKey];
        }

        public IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
        {
            return controllerTypeCache;
        }
    }
}