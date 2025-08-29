using AccountServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AccountServer.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AccountService _account;
        private readonly ILogger<AccountController> _logger;

        public AccountController(AccountService account, ILogger<AccountController> logger)
        {
            _account = account;
            _logger = logger;
        }

        [HttpPost("login/google")]
        public async Task<LoginAccountPacketRes> LoginGoogleAccount([FromBody] LoginAccountPacketReq req)
        {
            _logger.LogInformation("Google login 요청 수신. userId={uid}, tokenLen={len}",
                req?.userId, req?.token?.Length ?? -1);

            var res = await _account.LoginGoogleAccount(req.token);

            _logger.LogInformation("Google login 결과. success={success}, accountDbId={id}",
                res.success, res.accountDbId);

            return res;
        }

        [HttpPost("login/facebook")]
        public async Task<LoginAccountPacketRes> LoginFacebook([FromBody] LoginAccountPacketReq req)
        {
            _logger.LogInformation("Facebook login 요청 수신. tokenLen={len}",
                req?.token?.Length ?? -1);

            var res = await _account.LoginFacebookAccount(req.token);

            _logger.LogInformation("Facebook login 결과. success={success}, accountDbId={id}",
                res.success, res.accountDbId);

            return res;
        }

        [HttpPost("login/guest")]
        public async Task<LoginAccountPacketRes> LoginGuest([FromBody] LoginAccountPacketReq req)
        {
            _logger.LogInformation("Guest login 요청 수신. userId={uid}", req?.userId);

            var res = await _account.LoginGuestAccount(req.userId);

            _logger.LogInformation("Guest login 결과. success={success}, accountDbId={id}",
                res.success, res.accountDbId);

            return res;
        }
    }
}
