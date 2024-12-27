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
using System;
using MCSMqttBus.Producer;
using MQTTnet;
using System.IO;
using System.Net.Http;
using Domain.Models;
using Models;

namespace Presentation
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            // GUID için BSON Serializer ayarı
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console() // Konsola yazdırmak için
            .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day) // Dosyaya yazdırmak için
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

            // Servis Bağımlılıklarını Kaydet
            services.AddSingleton<ISnmpService, SnmpService>();
            services.AddTransient<WebSocketHandlerSnmp>();
            services.AddTransient<WebSocketHandlerTcp>();
            Log.Information("Web Socket preparing...");
            

            // MongoDB Bağlantısı

            Log.Information("MongoDB conneciton preparing is started");
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
            Log.Information("Program started");

            services.AddScoped<DeviceService>();
            services.AddScoped<SnmpParserService>();
            services.AddScoped<TcpService>();
            services.AddScoped<UserService>();
            services.AddScoped<LoginService>();

            // JWT Authentication
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

            // Controller ve CORS Desteği
            services.AddControllers();
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigins",
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:3000","http://10.0.20.69:3000") // İzin verilen kaynak
                            .AllowAnyHeader() // Herhangi bir başlığa izin ver
                            .AllowAnyMethod() // Herhangi bir HTTP metoduna izin ver
                            .AllowCredentials();
                    });
            });

            // Swagger Ayarları
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Snmp Communication",
                    Version = "v1",
                    Description = "A simple example ASP.NET Core Web API"
                });

                // Swagger için JWT Authentication tanımı
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

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, WebSocketHandlerSnmp webSocketHandler)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                // Swagger arayüzünü etkinleştir
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Snmp Communication");
                    c.RoutePrefix = string.Empty;
                });
            }

            // Statik Dosyalar (İsteğe Bağlı)
            app.UseStaticFiles();

            // WebSocket Middleware
            app.UseWebSockets();

            // Routing Middleware
            app.UseRouting();

            // CORS Middleware
            app.UseCors("AllowSpecificOrigins");

            // Authentication ve Authorization Middleware'leri
            app.UseAuthentication();
            app.UseAuthorization();

            // Endpoint Middleware
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.Map("/ws", async context =>
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        await webSocketHandler.HandleAsync(context, webSocket);
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
