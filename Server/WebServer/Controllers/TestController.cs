using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Controllers
{
    // RESTful 서버 통신규약
    // POST: TestController/test

    // ip:port/test
    [ApiController]
    [Route("test")]
    public class TestController : ControllerBase
    {
        // ip:port/test/hello
        [HttpPost]
        [Route("hello")]
        public TestPackRes TestPost([FromBody] TestPackReq value)
        {
            TestPackRes result = new TestPackRes();
            result.success = true;
            return result;

        }
    }
}
