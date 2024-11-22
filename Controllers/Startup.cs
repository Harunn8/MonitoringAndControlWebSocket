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
using System.ComponentModel.DataAnnotations;
using Services;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using Interfaces;
using Models;

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
            // Servis Baðýmlýlýklarýný Kaydet
            services.AddSingleton<ISnmpService, SnmpService>(); // SNMP servisi
            services.AddTransient<WebSocketHandler>(); // WebSocketHandler

            services.AddSingleton<IUserRepository, UserRepository>();
            services.AddSingleton<IUserService, UserService>();

            // MongoDB Baðlantýsý
            var mongoClient = new MongoClient(Configuration.GetConnectionString("MongoDb"));
            var database = mongoClient.GetDatabase("DeviceDB");

            services.AddSingleton(database);
            services.AddScoped<DeviceService>();

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

            // Controller ve CORS Desteði
            services.AddControllers();
            services.AddCors();

            // Swagger Ayarlarý
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Snmp Communication",
                    Version = "v1",
                    Description = "A simple example ASP.NET Core Web API"
                });

                // Swagger için JWT Authentication tanýmý
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

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, WebSocketHandler webSocketHandler)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                // Swagger arayüzünü etkinleþtir
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Snmp Communication");
                    c.RoutePrefix = string.Empty;
                });
            }

            // HTTPS Zorunluluðu
            app.UseHttpsRedirection();

            // Statik Dosyalar (Ýsteðe Baðlý)
            app.UseStaticFiles();

            // WebSocket Middleware
            app.UseWebSockets();

            // Routing Middleware
            app.UseRouting();

            // CORS Middleware
            app.UseCors(builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyHeader()
                       .AllowAnyMethod();
            });

            // Authentication ve Authorization Middleware'leri
            app.UseAuthentication(); // Authentication önce gelir
            app.UseAuthorization();  // Authorization sonra gelir

            // Endpoint Middleware
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers(); // API Controller'larýný haritalandýr
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
