using gRPC_Sender.Interceptor;
using gRPC_Sender.Interceptor.gRPC_Sender.Service;
using gRPC_Sender.Job;
using gRPC_Sender.Mapper;
using gRPC_Sender.Repository;
using gRPC_Sender.Service;
using gRPC_Sender.Service.gRPC_Sender.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using System.Text;

namespace gRPC_Sender
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddAutoMapper(typeof(EntityMappingProfile));
            builder.Services.AddSingleton<IDataReader>(new DataReader("your_connection_string"));
            builder.Services.AddSingleton<IEntityReader,EntityReader>();
            builder.Services.AddSingleton<SenderService>();
            // ����������� ������������
            builder.Services.AddSingleton<LoggingInterceptor>();

            // ����������� gRPC �������
            builder.Services.AddGrpc(options =>
            {
                options.Interceptors.Add<LoggingInterceptor>(); // ��������� �����������
                options.Interceptors.Add<AuthorizationInterceptor>();
            });
            builder.Services.AddGrpc();

            var key = Encoding.ASCII.GetBytes("This is my test private keyThis is my test private key");
            builder.Services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(x =>
                {
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });


            builder.Services.AddSingleton<IJob, EntityReaderJob>();
            // ��������� Quartz.NET
            /*            builder.Services.AddQuartz(q =>
                        {
                            q.UseDefaultThreadPool(x=>x.MaxConcurrency=1);
                            q.AddJob<EntityReaderJob>(opts => opts.WithIdentity("EntityReaderJob"));
                            q.AddTrigger(opts => opts
                                .ForJob("EntityReaderJob")
                                .WithIdentity("EntityReaderTrigger")
                                .WithCronSchedule("0 0/1 * * * ?")); // ������ ������ 1 �����
                        });*/
            builder.Services.AddQuartz(q =>
            {
                // ��������� ���� ������� � ������������ ������������ 1
                q.UseDefaultThreadPool(tp =>
                {
                    tp.MaxConcurrency = 1;
                });

                // ����������� � ����������� ������ (Job)
                var jobKey = new JobKey("EntityReaderJob");
                q.AddJob<EntityReaderJob>(opts => opts.WithIdentity(jobKey));

                // ����������� ��������, ������� ��������� ������ ������ 10 ������
                q.AddTrigger(opts => opts
                    .ForJob(jobKey)
                    .WithIdentity("EntityReaderTrigger")
                    .StartNow() // ����������� �����
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(10) // �������� 10 ������
                        .RepeatForever())); // ����������� ����������
            });

            builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

            // Add services to the container.
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("GrpcAccessPolicy", policy =>
                {
                    policy.RequireAuthenticatedUser();  // ��������� ��������������
                    policy.RequireRole("Admin");         // �������� ������ ����������, ��������, ���� Admin
                });
            });

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
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                // ����������� ������ gRPC-�������
                endpoints.MapGrpcService<SenderService>();
            });
            app.Run();
        }
    }
}
