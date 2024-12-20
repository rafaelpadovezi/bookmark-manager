﻿using BookmarkManager.Domain.Services;
using BookmarkManager.Infrastructure.DbContexts;
using BookmarkManager.Infrastructure.RabbitMQ;
using BookmarkManager.Utils;
using CliFx;
using CliFx.Attributes;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BookmarkManager.Commands
{
    [Command("api")]
    public class ApiCommand : ICommand
    {
        public async ValueTask ExecuteAsync(IConsole console)
        {
            await Program.CreateHostBuilder(Array.Empty<string>())
                .ConfigureLogging(builder =>
                {
                    builder.AddSimpleConsole(options =>
                    {
                        options.IncludeScopes = true;
                    });
                })
                .ConfigureServices(services =>
                {
                    services.AddHostedService<OutboxSendingService>();
                })
                .Build()
                .RunAsync();
        }

        public class Startup
        {
            public IConfiguration Configuration { get; }

            public Startup(IConfiguration configuration)
            {
                Configuration = configuration;
            }

            public void ConfigureServices(IServiceCollection services)
            {
                // api
                services
                    .AddControllers(options =>
                        options.Filters.Add(new ModelStateFilter()))
                    .AddFluentValidation(options =>
                        options.RegisterValidatorsFromAssemblyContaining<Startup>());
                // infra
                services
                    .AddDbContext<BookmarkManagerContext>(options =>
                        options.UseSqlServer(Configuration.GetConnectionString("BookmarkManagerContext")))
                    .AddRabbitMQConnection(Configuration.GetSection("RabbitMQ"))
                    .AddScoped<IQueueProducer, RabbitMqClient>();
                // application core
                services
                    .AddScoped<IBookmarkService, BookmarkService>();
            }

            public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
            {
                app.UseRouting();

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapGet("/", async context =>
                    {
                        await context.Response.WriteAsync("BookmarkManager Api");
                    });

                    endpoints.MapControllers();
                });

                // if (env.EnvironmentName != "Testing")
                //     EnsureDbCreated(app);
            }

            private static void EnsureDbCreated(IApplicationBuilder app)
            {
                using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
                using var context = serviceScope.ServiceProvider.GetRequiredService<BookmarkManagerContext>();
                context.Database.EnsureCreated();
            }
        }
    }
}