using Messaging;
using Messaging.ElasticSearch;
using Messaging.Logic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using MQTTnet.Diagnostics.PacketInspection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ws_cobertura.ElasticSearch;
using ws_cobertura.httpapi.Model.Configuration;
using ws_cobertura.httpapi.Model.General;
using ws_cobertura.RabbitMQ;

namespace ws_cobertura
{
    class Program
    {
        protected static Dictionary<Type, long> configChanged_lastTimestamp = new Dictionary<Type, long>();

        static void OnConfigChanged<T>(IConfigurationSection configSection, long currentTimestamp, Action<T> then)
            where T : new()
        {
            configSection.GetReloadToken().RegisterChangeCallback((o) => OnConfigChanged<T>(configSection, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), then), DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            Type configType = typeof(T);

            long lastTimestamp = (configChanged_lastTimestamp.ContainsKey(configType) ? configChanged_lastTimestamp[configType] : 0);
            configChanged_lastTimestamp[configType] = currentTimestamp;
            if (currentTimestamp - lastTimestamp < 2)
            {
                return;
            }

            T newConfigSection = configSection.Get<T>();
            if (then != null)
            {
                then(newConfigSection);
            }
        }
        static void OnConfigChanged<CoberturaRabbitMQConfiguration>(HostBuilderContext hostContext)
        {
            Console.WriteLine("CoberturaRabbitMQConfiguration changed");
            var rabbitMqConfig_SMSMT = hostContext.Configuration.GetSection("RabbitMq-cobertura");
            Console.WriteLine("CoberturaRabbitMQConfiguration JSON = {0}", System.Text.Json.JsonSerializer.Serialize(rabbitMqConfig_SMSMT.Get<CoberturaRabbitMQConfiguration>()));

            rabbitMqConfig_SMSMT.GetReloadToken().RegisterChangeCallback((o) => OnConfigChanged<CoberturaRabbitMQConfiguration>(hostContext), null);
        }

        protected static IServiceCollection _services;
        static async Task Main(string[] args)
        {
            await Task.Delay(10000); //esperamos 10 segundos para inciar la aplicación, ya que el rabbit puede tardar un poco en iniciarse por completo

            var webHost = Host.CreateDefaultBuilder(args)
                .ConfigureLogging((hostContext, loggingBuilder) =>
                {
                })
               .ConfigureServices((hostContext, services) =>
               {
                   _services = services;

                   IConfiguration configuration = hostContext.Configuration;

                   // config api
                   var apiConfiguration = configuration.GetSection("API");

                   //services.Configure<APIConfiguration>(apiConfiguration); // ya configurado en startup para authorization
                   APIConfiguration oApiConfiguration = apiConfiguration.Get<APIConfiguration>();
                   services.AddSingleton(oApiConfiguration);

                   // config retry consumer rabbitmq
                   IConfigurationRoot coberturaRetryRulesConfiguration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("coberturaRetryRules.json", optional: false).Build();
                   BaseRetryRuleSet<CoberturaRetryRule> coberturaRetryRules = coberturaRetryRulesConfiguration.Get<BaseRetryRuleSet<CoberturaRetryRule>>();
                   services.AddSingleton<BaseRetryRuleSet<CoberturaRetryRule>>(coberturaRetryRules);

                   // config rabbitmq
                   var coberturaRabbitMQConfiguration = configuration.GetSection("RabbitMq-cobertura");
                   services.Configure<CoberturaRabbitMQConfiguration>(coberturaRabbitMQConfiguration);
                   coberturaRabbitMQConfiguration.GetReloadToken().RegisterChangeCallback((o) => OnConfigChanged<CoberturaRabbitMQConfiguration>(hostContext), null);
                   CoberturaRabbitMQConfiguration oCoberturaRabbitMQConfiguration = coberturaRabbitMQConfiguration.Get<CoberturaRabbitMQConfiguration>();

                   // config elastic search
                   CoberturaESConfiguration esConfiguration = configuration.GetSection("ElasticSearch").Get<CoberturaESConfiguration>();
                   services.AddSingleton(esConfiguration);
                   IndexMappingsConfiguration indexMappingsConfiguration = configuration.GetSection("ElasticSearchIndexMappings").Get<IndexMappingsConfiguration>();
                   services.AddSingleton(indexMappingsConfiguration);

                   services.AddSingleton<CoberturaReportRepository>();

                   services.AddSingleton<CoberturaESHelper>(f => CoberturaESHelper.CreateInstance(esConfiguration, indexMappingsConfiguration, (coberturaESHelper, esClient) =>
                   {
                       esClient.MapIndex<CoberturaReport>(coberturaESHelper.GetIndexNameForType("CoberturaReport"));
                   }));

                   
                   // config cron agrupar elastic search
                   var coberturaESCronAgruparConfiguration = configuration.GetSection("ElasticSearchCronAgrupar");
                   services.Configure<CoberturaESCronConfiguration>(coberturaESCronAgruparConfiguration);
                   coberturaESCronAgruparConfiguration.GetReloadToken().RegisterChangeCallback((o) => OnConfigChanged<CoberturaESCronConfiguration>(hostContext), null);
                   CoberturaESCronConfiguration esCronConfiguration = coberturaESCronAgruparConfiguration.Get<CoberturaESCronConfiguration>();
                   services.AddSingleton(esCronConfiguration);

                   // servicio cron agrupar elastic search
                   services.AddSingleton<CoberturaESServicioAgrupado>();
                   services.AddHostedService<CoberturaESServicioAgrupado>(provider => provider.GetRequiredService<CoberturaESServicioAgrupado>());

                   // helper api
                   services.AddSingleton<APIHelper>();

                   // rabbit mq memoryqueue
                   InMemoryMQQueue<dynamic> queue = new InMemoryMQQueue<dynamic>("json");
                   services.AddSingleton(queue);

                   // rabbitmq sender
                   services.AddSingleton<CoberturaSender>();
                   // rabbitmq consumer y worker
                   services.AddSingleton<IHostedService>(provider => new CoberturaConsumer(queue, oCoberturaRabbitMQConfiguration));
                   services.AddSingleton<CoberturaRabbitMQWorker>();
                   services.AddHostedService<CoberturaRabbitMQWorker>(provider => provider.GetRequiredService<CoberturaRabbitMQWorker>());
               })
               .UseSystemd()
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("sharedsettings.json", optional: true)
                    .AddJsonFile("appsettings.json", optional: false)
                    .Build();

                   ChangeToken.OnChange(() => configuration.GetReloadToken(), () => Console.WriteLine("configuration changed"));

                   webBuilder
                        .UseStartup<Startup>()
                        .UseConfiguration(configuration)
                        //.UseIISIntegration() //para publicarlo en IIS
                        .UseKestrel((hostContext, options) =>
                        {
                            options.Limits.MaxConcurrentConnections = 200;
                            Console.WriteLine("MaxConcurrentConnections = " + options.Limits.MaxConcurrentConnections);
                            options.Limits.MinResponseDataRate = null;
                        });
               })
               .Build();

            using (IServiceScope serviceScope = webHost.Services.CreateScope())
            {
                /*CoberturaSender coberturaSender = serviceScope.ServiceProvider.GetRequiredService<CoberturaSender>();
                _coberturaRabbitMQConfiguration.GetReloadToken().RegisterChangeCallback((o) => OnConfigChanged<CoberturaRabbitMQConfiguration>(_coberturaRabbitMQConfiguration, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), (c) => coberturaSender.OnConfigChanged(c)), DateTimeOffset.UtcNow.ToUnixTimeSeconds());
   
                CentralizedEventListener centralizedEventListener = serviceScope.ServiceProvider.GetRequiredService<CentralizedEventListener>();

                CacheManager manager = (CacheManager)serviceScope.ServiceProvider.GetRequiredService<CacheManager>();
                await manager.Initialize(serviceScope);

   
                Console.WriteLine("Cache done");*/
            }

            Console.WriteLine("RUNNING");

            await webHost.RunAsync();
        }

        public class MqttPacketInspector : IMqttPacketInspector
        {
            public void ProcessMqttPacket(ProcessMqttPacketContext context)
            {
                Console.WriteLine(context.Direction + " | " + System.Text.UTF8Encoding.UTF8.GetString(context.Buffer));

            }
        }
    }
}
