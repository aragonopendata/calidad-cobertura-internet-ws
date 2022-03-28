using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using ws_cobertura.httpapi.Model.Configuration;
using ws_cobertura.httpapi.Model.General;

namespace ws_cobertura
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
            services.Configure<KestrelServerOptions>(Configuration.GetSection("Kestrel"));
            services.Configure<APIConfiguration>(Configuration.GetSection("API"));

            //services.AddControllers();
            services.AddCors();
            services.AddConnections();
            services.AddHttpClient();
            services.AddControllers();

            services.AddAuthentication(CustomAuthorizationOptions.DefaultScemeName)
                .AddScheme<CustomAuthorizationOptions, CustomAuthorizationHandler>(
                    CustomAuthorizationOptions.DefaultScemeName,
                    opts => {
                    }
                );

            services.AddSingleton<IServiceCollection>(services); // Última entrada siempre
            services.AddSingleton<ServiceProvider>(services.BuildServiceProvider()); // Última entrada siempre
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            //loggerFactory.AddConsole();
            //loggerFactory.AddDebug();

            app.UseRouting();

            app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true) // allow any origin
                .AllowCredentials());

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "apicobertura",
                    pattern: "apicobertura",
                    defaults: new { controller = "httpapi.APICoberturaController", action = "404" });
            });
        }
    }
}
