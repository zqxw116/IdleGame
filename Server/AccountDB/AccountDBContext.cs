using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountDB
{
    public class AccountDbContext : DbContext
    {
        public DbSet<AccountDb> Accounts { get; set; } // 메모리에 저장

        static readonly ILoggerFactory _logger = LoggerFactory.Create(builder => { builder.AddConsole(); }); // 로그용

        public static string ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=AccountDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

        public AccountDbContext()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options
                .UseLoggerFactory(_logger)
                .UseSqlServer(ConnectionString); //SQL 서버 연동해주세요
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<AccountDb>()
                .HasIndex(t => t.LoginProviderUserId) // LoginProviderUserId 를 유니크하게 변경해줌
                .IsUnique();
        }
    }
}
