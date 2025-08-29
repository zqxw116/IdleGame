using System;
using System.Linq;
using System.Threading.Tasks;
using AccountDB;
using AccountServer.Models;
using Microsoft.Extensions.Logging;

namespace AccountServer.Services
{
    public class AccountService
    {
        private readonly AccountDbContext _dbContext;
        private readonly FacebookService _facebook;
        private readonly GoogleService _google;
        private readonly ILogger<AccountService> _logger;

        public AccountService(
            AccountDbContext context,
            FacebookService facebook,
            GoogleService google,
            ILogger<AccountService> logger)
        {
            _dbContext = context;
            _facebook = facebook;
            _google = google;
            _logger = logger;
        }

        public async Task<LoginAccountPacketRes> LoginGoogleAccount(string token)
        {
            var res = new LoginAccountPacketRes();
            try
            {
                _logger.LogInformation("LoginGoogleAccount start. token(20): {tok}",
                    Trunc(token, 20));

                var tokenData = await _google.GetUserTokenData(token);

                if (tokenData == null)
                {
                    _logger.LogWarning("GoogleTokenData is null");
                    return res;
                }
                if (string.IsNullOrEmpty(tokenData.sub))
                {
                    _logger.LogWarning("Google token has empty sub (invalid token). email={email}", tokenData.email);
                    return res;
                }

                _logger.LogInformation("Google token OK. sub={sub}, email={email}", tokenData.sub, tokenData.email);

                var accountDb = _dbContext.Accounts
                    .FirstOrDefault(a => a.LoginProviderUserId == tokenData.sub
                                      && a.LoginProviderType == ProviderType.Google);

                if (accountDb == null)
                {
                    _logger.LogInformation("로그인 한적 없음. No account found. Creating new Google account. sub={sub}", tokenData.sub);

                    accountDb = new AccountDb
                    {
                        LoginProviderUserId = tokenData.sub,
                        LoginProviderType = ProviderType.Google
                    };

                    _dbContext.Accounts.Add(accountDb);
                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation("Account created. AccountDbId={id}", accountDb.AccountDbId);
                }
                else
                {
                    _logger.LogInformation("로그인 존재. Existing account login. AccountDbId={id}", accountDb.AccountDbId);
                }

                res.success = true;
                res.accountDbId = accountDb.AccountDbId;
                res.providerType = ProviderType.Google;

                _logger.LogInformation("LoginGoogleAccount done. success={success}, accountDbId={id}",
                    res.success, res.accountDbId);

                return res;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LoginGoogleAccount error");
                return res;
            }
        }

        public async Task<LoginAccountPacketRes> LoginFacebookAccount(string token)
        {
            var res = new LoginAccountPacketRes();
            try
            {
                _logger.LogInformation("LoginFacebookAccount start. token(20): {tok}", Trunc(token, 20));

                var tokenData = await _facebook.GetUserTokenData(token);
                if (tokenData == null || tokenData.is_valid == false)
                {
                    _logger.LogWarning("Facebook token invalid or null");
                    return res;
                }

                var accountDb = _dbContext.Accounts
                    .FirstOrDefault(a => a.LoginProviderUserId == tokenData.user_id
                                      && a.LoginProviderType == ProviderType.Facebook);

                if (accountDb == null)
                {
                    _logger.LogInformation("No FB account. Creating. user_id={uid}", tokenData.user_id);

                    accountDb = new AccountDb
                    {
                        LoginProviderUserId = tokenData.user_id,
                        LoginProviderType = ProviderType.Facebook
                    };

                    _dbContext.Accounts.Add(accountDb);
                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation("FB account created. AccountDbId={id}", accountDb.AccountDbId);
                }
                else
                {
                    _logger.LogInformation("Existing FB account. AccountDbId={id}", accountDb.AccountDbId);
                }

                res.success = true;
                res.accountDbId = accountDb.AccountDbId;
                _logger.LogInformation("LoginFacebookAccount done. success={success}, accountDbId={id}",
                    res.success, res.accountDbId);

                return res;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LoginFacebookAccount error");
                return res;
            }
        }

        public async Task<LoginAccountPacketRes> LoginGuestAccount(string userID)
        {
            var res = new LoginAccountPacketRes();
            try
            {
                _logger.LogInformation("LoginGuestAccount start. userID={uid}", userID);

                var accountDb = _dbContext.Accounts
                    .FirstOrDefault(a => a.LoginProviderUserId == userID
                                      && a.LoginProviderType == ProviderType.Guest);

                if (accountDb == null)
                {
                    _logger.LogInformation("No Guest account. Creating. userID={uid}", userID);

                    accountDb = new AccountDb
                    {
                        LoginProviderUserId = userID,
                        LoginProviderType = ProviderType.Guest
                    };

                    _dbContext.Accounts.Add(accountDb);
                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation("Guest account created. AccountDbId={id}", accountDb.AccountDbId);
                }
                else
                {
                    _logger.LogInformation("Existing Guest account. AccountDbId={id}", accountDb.AccountDbId);
                }

                res.success = true;
                res.accountDbId = accountDb.AccountDbId;

                _logger.LogInformation("LoginGuestAccount done. success={success}, accountDbId={id}",
                    res.success, res.accountDbId);

                return res;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LoginGuestAccount error");
                return res;
            }
        }

        private static string Trunc(string s, int n)
            => string.IsNullOrEmpty(s) ? "<null>" : (s.Length <= n ? s : s.Substring(0, n));
    }
}
