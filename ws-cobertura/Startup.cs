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
using System.Reflection;
using System.IO;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Examples;
using ws_cobertura.httpapi.Model.Request;

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

            //services.AddSwaggerGen();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "API cobertura acceso a internet",
                    Description = "API cobertura acceso a internet"
                });
               /* c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Token Bearer requerido, ejemplo: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
               */
                c.TagActionsBy(api => new[] { api.GroupName });
                c.DocInclusionPredicate((name, api) => true);

                APIConfiguration oAPIConfiguration = Configuration.GetSection("API").Get<APIConfiguration>();

                List<string> sTokenSwagger = new List<string>();

                if (oAPIConfiguration != null && !string.IsNullOrEmpty(oAPIConfiguration.tokenOpenData))
                {
                    sTokenSwagger.Add(oAPIConfiguration.tokenOpenData);
                }

                /*c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                  {
                    {
                      new OpenApiSecurityScheme
                      {
                        Reference = new OpenApiReference
                          {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                          },
                          Scheme = "oauth2",
                          Name = "Bearer",
                          In = ParameterLocation.Header
                        },
                        sTokenSwagger
                      }
                    });*/

                c.SchemaFilter<PostDataRequestExample>();

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                c.IncludeXmlComments(xmlPath);
            });

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
            /*var basePath = "/v1";
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger(c => 
                {
                    c.SerializeAsV2 = true;
                    c.RouteTemplate = "swagger/{documentName}/swagger.json";
                    c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                    {
                        swaggerDoc.Servers = new List<OpenApiServer> { new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}{basePath}" } };
                    });
                }
            );*/

            // Enable middleware to serve generated Swagger as a JSON endpoint.

            APIConfiguration oAPIConfiguration = Configuration.GetSection("API").Get<APIConfiguration>();

            string sBasePathSwagger = string.Empty;

            string sRutaSwagger = "/swagger/v1/swagger.json";

            string sUrlBaseSwagger = "http://localhost:5000/";

            if (oAPIConfiguration != null && !string.IsNullOrEmpty(oAPIConfiguration.basePathSwagger))
            {
                sBasePathSwagger = oAPIConfiguration.basePathSwagger;
                sRutaSwagger = sBasePathSwagger + "swagger/v1/swagger.json";
            }

            if (oAPIConfiguration != null && !string.IsNullOrEmpty(oAPIConfiguration.urlBaseSwagger))
            {
                sUrlBaseSwagger = oAPIConfiguration.urlBaseSwagger;
            }

            //app.UseSwagger();
            app.UseSwagger(c =>
            {
                c.RouteTemplate = "swagger/{documentName}/swagger.json";
                c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                {
                    swaggerDoc.Servers = new List<OpenApiServer> { new OpenApiServer { Url = sUrlBaseSwagger } };
                });
            });

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.)

            //app.UseSwaggerUI(c=> { c.SwaggerEndpoint(rutaSwagger, "API Cobertura"); c.RoutePrefix = "swagger/ui"; });
            app.UseSwaggerUI(c => { c.SwaggerEndpoint(sRutaSwagger, "API Cobertura"); });


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "apicobertura",
                    pattern: "apicobertura",
                    defaults: new { controller = "httpapi.APICoberturaController", action = "404" });
                endpoints.MapSwagger();
            });
        }
    }
}
