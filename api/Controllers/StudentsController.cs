using System.Collections.Generic;
using System.Web.Http;

namespace api.Controllers
{
    public class StudentsController : ApiController
    {
        // GET api/values
        public IEnumerable<string> Get()
        {
            return new[] { "default" };
        }
    }
}