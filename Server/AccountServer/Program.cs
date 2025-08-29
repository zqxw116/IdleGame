
using AccountDB;
using AccountServer.Services;

namespace AccountServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<AccountDbContext>(); 
            builder.Services.AddSingleton<FacebookService>(); 
            builder.Services.AddSingleton<GoogleService>(); 
            builder.Services.AddScoped<AccountService>();// dbcontext라서 싱글톤 사용하면 에러라 scoped
                                                         // 모든 IP에서 포트 7777로 접속 허용
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(7777);
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            // 개발용이라면 주석/삭제
            // app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();
            
            app.MapGet("/ping", () => "pong");


            app.Run();
        }
    }
}
