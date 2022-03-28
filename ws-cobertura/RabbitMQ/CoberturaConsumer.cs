using Messaging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ws_cobertura.httpapi.Model.Configuration;
using static ws_cobertura.Program;

namespace ws_cobertura.RabbitMQ
{
    public class CoberturaConsumer : BackgroundService, IObserver<object>
    {
        public static System.Text.Json.JsonSerializerOptions jsonSerializerOptions = new System.Text.Json.JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Default,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        private readonly CoberturaRabbitMQConfiguration _coberturaRabbitMQConfiguration;

        protected InMemoryMQQueue<dynamic> _memoryQueue;

        protected String consumerId = Guid.NewGuid().ToString();

        public CoberturaConsumer(InMemoryMQQueue<dynamic> memoryQueue, CoberturaRabbitMQConfiguration coberturaRabbitMQConfiguration)
        {
            _coberturaRabbitMQConfiguration = coberturaRabbitMQConfiguration;
            _memoryQueue = memoryQueue;
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
            Helper.VolcarAConsola("coberturaConsumer", "OnNext", "CONSUMER " + consumerId + " - " + "OnError " + error.Message, false);

        }

        public void OnNext(dynamic device)
        {
            Helper.VolcarAConsola("coberturaConsumer", "OnNext", "CONSUMER " + consumerId + " - " + "Processing device data...", false);

            Task.Run(async () =>
            {
                IMqttClient mqttClient = null;

                try
                {
                    mqttClient = new MqttFactory().CreateMqttClient();

                    String device_user = device.bs_mac;

                    var mqttClienteOptions = new MqttClientOptionsBuilder().WithTcpServer(_coberturaRabbitMQConfiguration.Hostname, _coberturaRabbitMQConfiguration.Port)
                        .WithCredentials(device_user, (string)null)
                        .WithPacketInspector(new MqttPacketInspector())
                        .WithCommunicationTimeout(TimeSpan.FromSeconds(5))
                        .WithCleanSession().Build();

                    Helper.VolcarAConsola("coberturaConsumer", "OnNext", "CONSUMER " + consumerId + " - " + "Connecting to " + device_user + "@" + _coberturaRabbitMQConfiguration.Hostname + ":" + _coberturaRabbitMQConfiguration.Port + "...", false);


                    await mqttClient.ConnectAsync(mqttClienteOptions, CancellationToken.None);

                    string newpayload = JsonSerializer.Serialize(device.payload, jsonSerializerOptions);

                    Console.WriteLine();

                    Helper.VolcarAConsola("coberturaConsumer", "OnNext", "CONSUMER " + consumerId + " - " + "Published", false);
                    Helper.VolcarAConsola("coberturaConsumer", "OnNext", "CONSUMER " + consumerId + " - " + "Disconnecting...", false);

                }
                catch (Exception ex)
                {
                    Helper.VolcarAConsola("CoberturaConsumer", "OnNext.SendMessageNetQ", "CONSUMER " + consumerId + " - " + "EXCEPTION " + ex.Message, true);

                }
                finally
                {
                    await mqttClient?.DisconnectAsync();
                }

            });
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Helper.VolcarAConsola("coberturaConsumer", "ExecuteAsync", "CONSUMER " + consumerId + " - " + "Subscribing...", false);

            await _memoryQueue.Subscribe(this, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            Helper.VolcarAConsola("coberturaConsumer", "StopAsync", "CONSUMER " + consumerId + " - " + "Stopping...", false);

            Helper.VolcarAConsola("coberturaConsumer", "StopAsync", "CONSUMER " + consumerId + " - " + "Stopped", false);

            await base.StopAsync(stoppingToken);
        }

        public override void Dispose()
        {
            Helper.VolcarAConsola("coberturaConsumer", "Dispose", "CONSUMER " + consumerId + " - " + "Disposed", false);

            base.Dispose();
        }

    }
}
