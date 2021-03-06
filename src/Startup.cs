using BookmarkManager.Infrastructure;
using BookmarkManager.Infrastructure.Queue;
using BookmarkManager.Services;
using BookmarkManager.Utils;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BookmarkerManager
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
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
                .AddScoped<IQueue<Bookmark>, BookmarkInsertedQueue>();
            // application core
            services
                .AddScoped<IBookmarkService, BookmarkService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("BookmarkManager Api");
                });

                endpoints.MapControllers();
            });

            if (env.EnvironmentName != "Testing")
                EnsureDbCreated(app);
        }

        private static void EnsureDbCreated(IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            using var context = serviceScope.ServiceProvider.GetService<BookmarkManagerContext>();
            context.Database.EnsureCreated();
        }
    }
}
