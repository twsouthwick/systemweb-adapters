using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using ClassLibrary;

namespace MvcApp.Controllers
{
    [RoutePrefix("api/test")]
    public class TestController : ApiController
    {
        [Route("request/info")]
        [HttpGet]
        public Task GetData() => RequestInfo.WriteRequestInfo(false);

        [Route("request/cookie")]
        [HttpGet]
        public Task TestRequestCookie() => CookieTests.RequestCookies(HttpContext.Current);

        [Route("response/cookie")]
        [HttpGet]
        public Task TestResponseCookie(bool shareable = false)
        {
            HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.Public);
            return CookieTests.ResponseCookies(HttpContext.Current, shareable);
        }
    }
}
