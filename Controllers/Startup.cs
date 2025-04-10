using System;
using Application.Interfaces;
using Presentation.Controllers;
using Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MongoDB.Driver;
using Services;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using Controllers.Controllers;
using Serilog;
using MQTTnet.Client.Options;
using MQTTnet.Client;
using MCSMqttBus.Connection.Base;
using MCSMqttBus.Connection;
using MCSMqttBus.Producer;
using MQTTnet;
using Models;
using Services.AlarmService.Services;
using AutoMapper;

namespace Presentation
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

            Log.Information("Logger is configured and started");

            Log.Information("Mqtt Connection preparing...");

            var mqttOptions = new MqttClientOptionsBuilder()
                .WithClientId("telemetry")
                .WithTcpServer("127.0.0.1",1883)
                .WithCleanSession()
                .Build();

            services.AddSingleton<IMqttClient>(_ => new MqttFactory().CreateMqttClient());
            services.AddSingleton<IMqttConnection>(_ => new MqttConneciton(mqttOptions, new MqttFactory().CreateMqttClient()));
            services.AddSingleton<MqttProducer>();

            services.AddScoped<ISnmpService, SnmpService>();
            services.AddTransient<WebSocketHandlerSnmp>();
            services.AddTransient<WebSocketHandlerTcp>();
            Log.Information("Web Socket preparing...");
            
            Log.Information("MongoDB conneciton preparing is started");
            var mongoDbSettings = Configuration.GetSection("MongoDbSettings").Get<MongoDBSettings>();
            services.AddSingleton<IMongoClient>(sp =>
            {
                var settings = Configuration["ConnectionStrings:MongoDb"];
                return new MongoClient(settings);
            });
            services.AddSingleton(sp =>
            {
                var mongoClient = sp.GetRequiredService<IMongoClient>();
                return mongoClient.GetDatabase(mongoDbSettings.DatabaseName);
            });
            Log.Information("MongoDB connection was establish");
            services.AddScoped<ContextSeedService>();
            Log.Information("Program started");

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddScoped<DeviceService>();
            services.AddScoped<SnmpParserService>();
            services.AddScoped<TcpService>();
            services.AddScoped<UserService>();
            services.AddScoped<LoginService>();
            services.AddScoped<DeviceDataService>();
            services.AddScoped<AlarmManagerService>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"])),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            services.AddControllers();
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyHeader()
                               .AllowAnyMethod();
                    });
            });


            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Snmp Communication",
                    Version = "v1",
                    Description = "A simple example ASP.NET Core Web API"
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header. Example: \"Bearer {token}\""
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, WebSocketHandlerSnmp webSocketHandlerSnmp, WebSocketHandlerTcp webSocketHandlerTcp)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Snmp Communication");
                    c.RoutePrefix = string.Empty;
                });
            }

            app.UseStaticFiles();

            app.UseWebSockets();

            app.UseRouting();

            app.UseCors("AllowAllOrigins");

            app.UseAuthentication();
            app.UseAuthorization();

            using(var scope = app.ApplicationServices.CreateScope()) 
            {
                var seedService = scope.ServiceProvider.GetRequiredService<ContextSeedService>();
                seedService.UserSeedAsync().Wait();
                seedService.DeviceSeedAsync().Wait();                 
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.Map("/ws/snmp", async context =>
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        await webSocketHandlerSnmp.HandleAsyncSnmp(context, webSocket);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                });
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.Map("/ws/tcp", async context =>
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        await webSocketHandlerTcp.HandleAsyncTcp(context, webSocket);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                });
            });
        }
    }
}