using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using api.Controllers;

namespace api.tests
{
    public class VersionedControllerSelectorTests
    {
        [TestClass]
        public class GetMappingsTests
        {
            [TestMethod]
            public void Has_Default_Mapping()
            {
                var selector = new VersionedControllerSelector(new HttpConfiguration());

                var mappings = selector.GetControllerMapping();

                Assert.IsNotNull(mappings.Keys.Where(key => key.StartsWith("default")));
            }

            [TestMethod]
            public void Has_V1_Mapping()
            {
                var selector = new VersionedControllerSelector(new HttpConfiguration());

                var mappings = selector.GetControllerMapping();

                Assert.IsNotNull(mappings.Keys.Where(key => key.StartsWith("v1")));
            }

            [TestMethod]
            public void Has_V2_Mapping()
            {
                var selector = new VersionedControllerSelector(new HttpConfiguration());

                var mappings = selector.GetControllerMapping();

                Assert.IsNotNull(mappings.Keys.Where(key => key.StartsWith("v2")));
            }
        }

        [TestClass]
        public class SelectionTests
        {
            [TestMethod]
            public void Selects_Default_Controller_When_No_Header_Present()
            {
                var config = new HttpConfiguration();
                config.Routes.MapHttpRoute(
                    "Default", 
                    "api/{controller}/{action}",
                    new { controller = "Students", action = "Get" }
                );
                var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/");
                var routeData = config.Routes.GetRouteData(request);
                request.Properties[HttpPropertyKeys.HttpRouteDataKey] = routeData;

                var selector = new VersionedControllerSelector(config);

                var descriptor = selector.SelectController(request);

                Assert.AreEqual(descriptor.ControllerType, typeof(StudentsController));
            }

            [TestMethod]
            public void Selects_v1_Controller_When_V1_Accept_Specified()
            {
                var config = new HttpConfiguration();
                config.Routes.MapHttpRoute(
                    "Default",
                    "api/{controller}/{action}",
                    new { controller = "Students", action = "Get" }
                );
                var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/");
                var routeData = config.Routes.GetRouteData(request);
                request.Properties[HttpPropertyKeys.HttpRouteDataKey] = routeData;
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/api.v1+json"));

                var selector = new VersionedControllerSelector(config);

                var descriptor = selector.SelectController(request);

                Assert.AreEqual(descriptor.ControllerType, typeof(Controllers.v1.StudentsController));
            }

            [TestMethod]
            public void Selects_v2_Controller_When_V1_And_V2_Accept_Specified()
            {
                var config = new HttpConfiguration();
                config.Routes.MapHttpRoute(
                    "Default",
                    "api/{controller}/{action}",
                    new { controller = "Students", action = "Get" }
                );
                var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/");
                var routeData = config.Routes.GetRouteData(request);
                request.Properties[HttpPropertyKeys.HttpRouteDataKey] = routeData;
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/api.v1+json"));
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/api.v2+json"));

                var selector = new VersionedControllerSelector(config);

                var descriptor = selector.SelectController(request);

                Assert.AreEqual(descriptor.ControllerType, typeof(Controllers.v2.StudentsController));
            }
        }
    }
}
