
using FarmXpert.Data;
using FarmXpert.Models;
using FarmXpert.Services;
using FarmXpert.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Text;
using static System.Net.WebRequestMethods;


namespace FarmXpert
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<FarmDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
               .AddJwtBearer(options =>
               {
                   options.TokenValidationParameters = new TokenValidationParameters
                   {
                       ValidateIssuer = true,
                       ValidateAudience = true,
                       ValidateLifetime = true,
                       ValidateIssuerSigningKey = true,
                       ValidIssuer = builder.Configuration["Jwt:Issuer"],
                       ValidAudience = builder.Configuration["Jwt:Audience"],
                       IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))

                   };
               });

            builder.Services.AddAuthorization();
            builder.Services.AddSignalR();
            // Œœ„«  «·ÿﬁ” +  ‰»ÌÂ« 
            builder.Services.AddHttpClient<IWeatherService, OpenWeatherService>();
            builder.Services.AddScoped<IAlertService, AlertService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });
            ////////////////////////////////////
            builder.Services.AddControllers()
               .AddJsonOptions(options =>
               {
                 options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
               });
            /////////////////////////////////

            // Swagger + JWT Support
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "FarmXpert API", Version = "v1" });

                // JWT Bearer token configuration in Swagger
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = " Bearer YOUR_TOKEN_HERE"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                        Array.Empty<string>()
                    }
                });
            });
            //HTTPS
            //builder.Services.AddHttpsRedirection(options =>
            //{
                //options.HttpsPort = 5001;
            //});
            ///////////////////////////////////////////////////

            var app = builder.Build();

            
            if (app.Environment.IsDevelopment() || true)
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //app.UseHttpsRedirection();   //HTTPS
            app.UseStaticFiles();
            app.UseCors("AllowAll");

            app.UseAuthentication();
            app.UseMiddleware<TokenRevocationMiddleware>();

            app.UseAuthorization();
            //  ”ÃÌ· «·‹ Hub ›Ì pipeline
            app.MapHub<NotificationHub>("/notificationHub");
            app.MapControllers();
           


            app.Run();
        }
    }
}
