using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebServer.Services;

namespace WebServer.Controllers
{
    // RESTful 서버 통신규약
    // POST: TestController/test

    // ip:port/test
    [ApiController]
    [Route("test")]
    public class TestController : ControllerBase
    {
        AccountService _service;
        public TestController(AccountService service)
        {
            _service = service;
        }
        // ip:port/test/hello
        [HttpPost]
        [Route("hello")]
        public TestPackRes TestPost([FromBody] TestPackReq value)
        {
            TestPackRes result = new TestPackRes();
            result.success = true;

            int id = _service.GenerateAccountID();
            return result;

        }
    }
}
