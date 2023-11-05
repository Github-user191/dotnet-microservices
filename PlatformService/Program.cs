using Microsoft.EntityFrameworkCore;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.SyncDataServices.Grpc;
using PlatformService.SyncDataServices.Http; 

namespace PlatformService {
    public class Program {
        public static void Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();

            builder.Services.AddHttpClient<ICommandDataClient, HttpCommandDataClient>();

            builder.Services.AddScoped<IPlatformRepository, PlatformRepository>();
            // Singleton because we want to keep the same instance of the Message Bus at all times
            builder.Services.AddSingleton<IMessageBusClient, MessageBusClient>();

            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies()); // Register AutoMapper DTO
            builder.Services.AddGrpc();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            if (builder.Environment.IsDevelopment()) {
                Console.WriteLine("Using InMem Database");
                builder.Services.AddDbContext<AppDBContext>(options => options.UseInMemoryDatabase("InMem"));
            }
            else {
                Console.WriteLine("Using SQLServer Database");
                builder.Services.AddDbContext<AppDBContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("PlatformConnection")
                ));
            }

            Console.WriteLine($"--> Connection String: {builder.Configuration.GetConnectionString("PlatformConnection")}");

            Console.WriteLine($"--> Command Service Endpoint: {builder.Configuration["CommandService"]}");

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if(app.Environment.IsDevelopment()) {
                app.UseSwagger();
                app.UseSwaggerUI();
            }


            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapGet("/protos/platforms.proto", async context => {
                await context.Response.WriteAsync(File.ReadAllText("Protos/platforms.proto"));
            });
            app.MapGrpcService<GrpcPlatformService>();
            app.MapControllers();

            // Run DB code on app start
            PrepDB.PrepPopulation(app, builder.Environment.IsProduction());

            app.Run();
        }
    }
}