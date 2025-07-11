using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GameDB
{
    public class GameDbContext : DbContext
    {
        public DbSet<TestDb> Tests { get; set; }
        static readonly ILoggerFactory _logger = LoggerFactory.Create(builder => {builder.AddConsole(); });
        public static string ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=GameDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

        public GameDbContext()
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.
                UseLoggerFactory(_logger).     // 로깅을 해줘라. 콘솔에다가 로깅을 찍어달라.
                UseSqlServer(ConnectionString);// sql 서버를 사용해서 이곳으로 연결해달라
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // TODO
            builder.Entity<TestDb>()
                .HasIndex(t => t.Name)
                .IsUnique();
        }
    }
}
