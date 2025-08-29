
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
            builder.Services.AddScoped<AccountService>();// dbcontext�� �̱��� ����ϸ� ������ scoped
                                                         // ��� IP���� ��Ʈ 7777�� ���� ���
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
            // ���߿��̶�� �ּ�/����
            // app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();
            
            app.MapGet("/ping", () => "pong");


            app.Run();
        }
    }
}
