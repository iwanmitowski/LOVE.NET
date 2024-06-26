﻿namespace LOVE.NET.Web
{
    using System;
    using System.Reflection;
    using System.Text;

    using CloudinaryDotNet;

    using LOVE.NET.Data;
    using LOVE.NET.Data.Common;
    using LOVE.NET.Data.Common.Repositories;
    using LOVE.NET.Data.Models;
    using LOVE.NET.Data.Repositories;
    using LOVE.NET.Data.Repositories.Chat;
    using LOVE.NET.Data.Repositories.Countries;
    using LOVE.NET.Data.Repositories.Users;
    using LOVE.NET.Data.Seeding;
    using LOVE.NET.Services.Chats;
    using LOVE.NET.Services.Countries;
    using LOVE.NET.Services.Dashboard;
    using LOVE.NET.Services.Dating;
    using LOVE.NET.Services.Email;
    using LOVE.NET.Services.Genders;
    using LOVE.NET.Services.Identity;
    using LOVE.NET.Services.Images;
    using LOVE.NET.Services.Mapping;
    using LOVE.NET.Services.Messaging;
    using LOVE.NET.Web.ViewModels.Chat;
    using LOVE.NET.Web.ViewModels.Identity;

    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.OpenApi.Models;

    using static LOVE.NET.Common.GlobalConstants;
    using static LOVE.NET.Common.GlobalConstants.JWTSecurityScheme;

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            ConfigureServices(builder.Services, builder.Configuration);
            var app = builder.Build();
            Configure(app, builder.Configuration);
            app.Run();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddDefaultIdentity<ApplicationUser>(IdentityOptionsProvider.GetIdentityOptions)
                .AddRoles<ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddSwaggerGen();
            services.AddRazorPages();
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddSingleton(configuration);

            Account cloudinaryAccount = new Account(
                configuration[CloudinaryCloudName],
                configuration[CloudinaryKey],
                configuration[CloudinarySecret]);

            Cloudinary cloudinary = new Cloudinary(cloudinaryAccount);
            cloudinary.Api.Secure = true;

            services.AddSingleton(cloudinary);

            services.AddAuthentication(auth =>
            {
                auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = configuration[AuthSettingsAudience],
                    ValidIssuer = configuration[AuthSettingsIssuer],
                    RequireExpirationTime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            configuration[AuthSettingsKey])),
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                };
            });

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "LOVE.NET.API",
                    Description = "An ASP.NET Core Web Dating App API",
                });

                var jwtSecurityScheme = new OpenApiSecurityScheme
                {
                    Scheme = JWTScheme,
                    BearerFormat = JWT,
                    Name = JWTName,
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Description = JWTDescription,
                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme,
                    },
                };

                options.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        jwtSecurityScheme, Array.Empty<string>()
                    },
                });
            });

            // Data repositories
            services.AddScoped(typeof(IDeletableEntityRepository<>), typeof(EfDeletableEntityRepository<>));
            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
            services.AddScoped(typeof(IRepository<Gender>), typeof(EfRepository<Gender>));
            services.AddScoped<IDbQueryRunner, DbQueryRunner>();
            services.AddScoped<IUsersRepository, UsersRepository>();
            services.AddScoped<ICountriesRepository, CountriesRepository>();
            services.AddScoped<IChatRepository, ChatRepository>();
            services.AddScoped<IChatroomRepository, ChatroomRepository>();

            // Application services
            services.AddSignalR();
            services.AddTransient<IEmailSender>(x =>
                new SendGridEmailSender(configuration[SendGridApiKey]));
            services.AddTransient<IIdentityService, IdentityService>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<ICountriesService, CountriesService>();
            services.AddSingleton<IImagesService, ImagesService>();
            services.AddTransient<IGendersService, GendersService>();
            services.AddTransient<IDatingService, DatingService>();
            services.AddTransient<IDashboardService, DashboardService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddSingleton<IUsersGroupService, UsersGroupService>();
            services.AddCors(options =>
            {
                options.AddPolicy("DockerOrigin",
                    builder => builder.WithOrigins(configuration[UrlBase])
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials());
            });
        }

        private static void Configure(WebApplication app, IConfiguration configuration)
        {
            AutoMapperConfig.RegisterMappings(
                typeof(BaseCredentialsModel).GetTypeInfo().Assembly,
                typeof(MessageDto).GetTypeInfo().Assembly);

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                options.RoutePrefix = string.Empty;
            });

            if (app.Environment.IsDevelopment())
            {
                // Seed data on application startup
                using (var serviceScope = app.Services.CreateScope())
                {
                    var dbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // dbContext.Database.EnsureDeleted();
                    dbContext.Database.Migrate();
                    new ApplicationDbContextSeeder().SeedAsync(dbContext, serviceScope.ServiceProvider).GetAwaiter().GetResult();
                }

                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            app.UseCors("DockerOrigin");

            // app.UseHttpsRedirection(); // Do not redirect to https when in dev docker
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapHub<ChatHub>("/chat");
            app.MapControllerRoute("areaRoute", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
            app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();
        }
    }
}
