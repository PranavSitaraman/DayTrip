using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using UserManagement.DataAccess;
using LinqToDB.AspNet;
using LinqToDB.AspNet.Logging;
using LinqToDB.Configuration;
using DayTrip.Backend.Hubs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.FileProviders;

namespace DayTrip.Backend
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();
            services.AddControllers();

            services.AddLinqToDbContext<UserDataConnection>((provider, options) =>
            {
                options
                    .UsePostgreSQL(Configuration.GetConnectionString("Default"))
                    .UseDefaultLogging(provider);
            });

            services.AddLinqToDbContext<AppDataConnection>((provider, options) =>
            {
                options
                    .UsePostgreSQL(Configuration.GetConnectionString("Default"))
                    .UseDefaultLogging(provider);
            });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(o =>
                {
                    o.Cookie.HttpOnly = true;
                    o.LoginPath = string.Empty;
                    o.AccessDeniedPath = string.Empty;
                    o.Events.OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    };
                });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("UserOnly", policy => policy.RequireClaim(ClaimTypes.Sid));
                options.DefaultPolicy = options.GetPolicy("UserOnly")!;
            });

            services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });
            services.AddSwaggerGen();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();
            app.UseSwaggerUI();

            string staticDirectory = Path.Combine(Directory.GetCurrentDirectory(), "StaticFiles");
            Directory.CreateDirectory(staticDirectory);
            Directory.CreateDirectory(Path.Combine(staticDirectory, "Images"));

            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(staticDirectory),
                RequestPath = "/StaticFiles"
            });
            
            Console.WriteLine("Static files stored at " + Path.Combine(Directory.GetCurrentDirectory(), "StaticFiles"));


            app.UseResponseCompression();

            app.UseCookiePolicy(new()
            {
                MinimumSameSitePolicy = SameSiteMode.Lax,
            });


            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<PinHub>("/PinHub");
                endpoints.MapHub<CommentHub>("/CommentHub");
                endpoints.MapHub<BlazorChatSampleHub>("/chat");
            });
        }
    }
}