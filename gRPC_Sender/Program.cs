using gRPC_Sender.Interceptor.gRPC_Sender.Service;
using gRPC_Sender.Job;
using gRPC_Sender.Mapper;
using gRPC_Sender.Repository;
using gRPC_Sender.Service;
using gRPC_Sender.Service.gRPC_Sender.Service;
using Quartz;

namespace gRPC_Sender
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddAutoMapper(typeof(EntityMappingProfile));
            builder.Services.AddSingleton<IDataReader>(new DataReader("your_connection_string"));
            builder.Services.AddSingleton<EntityReader>();
            builder.Services.AddSingleton<SenderService>();
            // Регистрация интерсептора
            builder.Services.AddSingleton<LoggingInterceptor>();

            // Регистрация gRPC сервиса
            builder.Services.AddGrpc(options =>
            {
                options.Interceptors.Add<LoggingInterceptor>(); // Добавляем интерсептор
            });
            builder.Services.AddGrpc();
            builder.Services.AddSingleton<IJob, EntityReaderJob>();
            // Настройка Quartz.NET
            builder.Services.AddQuartz(q =>
            {
                q.UseDefaultThreadPool(x=>x.MaxConcurrency=1);
                q.AddJob<EntityReaderJob>(opts => opts.WithIdentity("EntityReaderJob"));
                q.AddTrigger(opts => opts
                    .ForJob("EntityReaderJob")
                    .WithIdentity("EntityReaderTrigger")
                    .WithCronSchedule("0 0/1 * * * ?")); // Запуск каждые 1 минут
            });

            builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                // Регистрация вашего gRPC-сервиса
                endpoints.MapGrpcService<SenderService>();
            });
            app.Run();
        }
    }
}
