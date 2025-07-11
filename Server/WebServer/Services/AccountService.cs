using GameDB;
using Microsoft.EntityFrameworkCore;

namespace WebServer.Services
{
    public class AccountService
    {
        GameDbContext _dbContext;
        public AccountService(GameDbContext dbContext) 
        {
            _dbContext = dbContext;
        }


        int _idGenerator = 1;
        public int GenerateAccountID()
        {
            _idGenerator++;
            return _idGenerator;
        }
    }
}
