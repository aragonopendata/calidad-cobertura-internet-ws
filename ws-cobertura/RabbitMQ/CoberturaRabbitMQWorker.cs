using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Messaging.Logic;
using Messaging.MOAPI;
using Microsoft.Extensions.Hosting;
using System.Threading;
using Messaging.RabbitMQ;
using Microsoft.Extensions.Options;
using ws_cobertura.ElasticSearch;
using ws_cobertura.httpapi.Model.General;
using Messaging.RabbitMQ;
using EasyNetQ;
using ws_cobertura.httpapi.Model.Response;
using System.Globalization;

namespace ws_cobertura.RabbitMQ
{
    public class CoberturaRabbitMQWorker : BackgroundService
    {
        private readonly ILogger<CoberturaRabbitMQWorker> _logger;
        private readonly CoberturaRabbitMQConfiguration _coberturaRabbitMQConfiguration;
        private readonly BaseRetryRuleSet<CoberturaRetryRule> _coberturaRetryRule;
        protected RabbitMQAdvancedConsumer<Messaging.IMessage> _consumer;
        private readonly CoberturaReportRepository _coberturaReportRepository;
        private readonly APIHelper _apiHelper;
        public CoberturaRabbitMQWorker(ILogger<CoberturaRabbitMQWorker> logger, IOptions<CoberturaRabbitMQConfiguration> coberturaRabbitMQConfiguration, BaseRetryRuleSet<CoberturaRetryRule> coberturaRetryRule, CoberturaReportRepository coberturaReportRepository, APIHelper apiHelper)
        {
            this._logger = logger;
            this._coberturaRabbitMQConfiguration = coberturaRabbitMQConfiguration.Value;
            this._coberturaRetryRule = coberturaRetryRule;
            this._coberturaReportRepository = coberturaReportRepository;
            this._apiHelper = apiHelper;
        }

        /////////////////////////////////
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using CancellationTokenSource cancelKeyPressToken = new CancellationTokenSource();
            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
            {
                e.Cancel = true;
                CancellationTokenSource.CreateLinkedTokenSource(stoppingToken).Cancel();
            };


            if (String.IsNullOrEmpty(_coberturaRabbitMQConfiguration.QueueName))
            {
                _logger.LogInformation("[coberturaMQ] NO CONSUMER CONFIGURED");
            }
            else
            {
                _logger.LogInformation("[coberturaMQ] Starting RabbitMQ consumer on queue " + _coberturaRabbitMQConfiguration.QueueName + "...");
                _consumer = new Messaging.RabbitMQ.RabbitMQAdvancedConsumer<Messaging.IMessage>(_coberturaRabbitMQConfiguration, ReceiveDelegateMQ);
                await _consumer.Start();
            }

            while (!stoppingToken.IsCancellationRequested && !cancelKeyPressToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(1000, stoppingToken);
                }
                catch (Exception ex)
                {
                    Helper.VolcarAConsola("CoberturaRabbitMQWorker", "ExecuteAsync", ex.Message, true);
                }
            }

            _logger.LogCritical("Critical - EXIT", DateTimeOffset.Now);
        }
        public async Task<Messaging.RabbitMQ.RabbitMQAdvancedConsumerResult> ReceiveDelegateMQ(IMessage<Messaging.IMessage> message, MessageReceivedInfo receivedInfo)
        {
            GenericResult oResult = GenericResult.Error(CoberturaESResultCode.DEFAULT_ERROR);

            if (!(message.Body is CoberturaMessage))
            {
                Console.WriteLine("No es un CoberturaMessage");
                return new RabbitMQAdvancedConsumerResult()
                {
                    delay = 0,
                    reject = true
                };
            }

            try
            {
                CoberturaReport oReport = null;

                try
                {
                    CoberturaMessage oCoberturaMessage = (CoberturaMessage)message.Body;

                    oReport = new CoberturaReport().FillFromCoberturaMessage(oCoberturaMessage);

                    if ((!oReport.message.coordenadax.HasValue || !oReport.message.coordenaday.HasValue) && string.IsNullOrEmpty(oReport.message.municipio))
                    {
                        oResult = GenericResult.Error(CoberturaESResultCode.DEFAULT_ERROR, "ES ERROR");
                        return GenerateConsumerResult(message, oResult, _coberturaRetryRule);
                    }

                    if (!oReport.message.coordenadax.HasValue || !oReport.message.coordenadax.HasValue)
                    {
                        try
                        {
                            ObtenerCoordenadasPorMunicipioResponse oObtenerCoordenadasPorMunicipioResponse = await _apiHelper.ObtenerCoordenadasPorMunicipio(oReport.message.municipio);

                            if (oObtenerCoordenadasPorMunicipioResponse.estadoRespuesta == "1" && !string.IsNullOrEmpty(oObtenerCoordenadasPorMunicipioResponse.coordenadax) && !string.IsNullOrEmpty(oObtenerCoordenadasPorMunicipioResponse.coordenaday))
                            {
                                oReport.message.coordenadax = Double.Parse(oObtenerCoordenadasPorMunicipioResponse.coordenadax, CultureInfo.InvariantCulture);
                                oReport.message.coordenaday = Double.Parse(oObtenerCoordenadasPorMunicipioResponse.coordenaday, CultureInfo.InvariantCulture);

                                AnonimizarCoordenadasUTMResponse oCoordenadasAnonimizadas = _apiHelper.AnonimizarCoordenadasUTM(oReport.message.coordenadax.Value, oReport.message.coordenaday.Value);

                                oReport.message.coordenadax = oCoordenadasAnonimizadas.coordenadax;
                                oReport.message.coordenaday = oCoordenadasAnonimizadas.coordenaday;
                            }
                            else
                            {
                                oResult = GenericResult.Error(CoberturaESResultCode.DEFAULT_ERROR, "ES ERROR");
                                return GenerateConsumerResult(message, oResult, _coberturaRetryRule);
                            }
                        }
                        catch (Exception ex) {
                            Helper.VolcarAConsola("CoberturaRabbitMQWorker", "ReceiveDelegateMQ", "ObtenerCoordenadasPorMunicipio:" + ex.Message, true);
                            oResult = GenericResult.Error(CoberturaESResultCode.DEFAULT_ERROR, "ES ERROR");
                            return GenerateConsumerResult(message, oResult, _coberturaRetryRule);
                        }
                    }

                    oReport.message.agrupado_cuadricula_tecnologia = ((int)oReport.message.coordenadax).ToString() + "|" + ((int)oReport.message.coordenaday).ToString() + "|" + oReport.message.tipoRed.ToString();
                    oReport.message.agrupado_ine_tecnologia = (oReport.message.ine + "|" + oReport.message.tipoRed.ToString());
                    oReport.message.agrupado_municipio_tecnologia = (oReport.message.municipio + "|" + oReport.message.tipoRed.ToString());

                    oReport.message.location = new CoberturaMessageLocation();

                    Location oLocation = new Location();

                    oLocation.InverseReproject((double) oReport.message.coordenadax, (double) oReport.message.coordenaday);

                    oReport.message.location.lat = oLocation.geo_x;
                    oReport.message.location.lon = oLocation.geo_y;
                }
                catch (Exception ex)
                {
                    Helper.VolcarAConsola("CoberturaRabbitMQWorker", "ReceiveDelegateMQ", "PREPARING MESSAGE:" + ex.Message, true);

                    oReport = null;
                }
               
                if (oReport == null)
                {
                    return GenerateConsumerResult(message, oResult, _coberturaRetryRule);
                }

                try
                {
                    bool esresult = await _coberturaReportRepository.AddCoberturaReport(oReport);

                    if (esresult)
                    {
                        oResult = GenericResult.Success();
                    }
                    else
                    {
                        oResult = GenericResult.Error(CoberturaESResultCode.DEFAULT_ERROR, "ES ERROR");
                    }
                }
                catch (Exception ex)
                {
                    Helper.VolcarAConsola("CoberturaRabbitMQWorker", "ReceiveDelegateMQ", "SENDING MESSAGE:" + ex.Message, true);

                    oResult = GenericResult.Error(CoberturaESResultCode.EXCEPTION, "ES EXCEPTION " + ex.Message);
                }

                return GenerateConsumerResult(message, oResult, _coberturaRetryRule);
            }
            catch (Exception ex)
            {
                Helper.VolcarAConsola("CoberturaRabbitMQWorker", "ReceiveDelegateMQ", ex.Message, true);

                _logger.LogError("ReceiveDelegateSMS GLOBAL EXCEPCION " + ex.Message); 

                return GenerateConsumerResult(message, null, _coberturaRetryRule);
            }
        }

        protected RabbitMQAdvancedConsumerResult GenerateConsumerResult(IMessage<Messaging.IMessage> message, GenericResult oResult, BaseRetryRuleSet<CoberturaRetryRule> ruleSet) {
            IRetryRule rule = default(IRetryRule);

            if (oResult == null)
            {
                rule = ruleSet.GetRuleForCode(GenericResultCode.DEFAULT_ERROR);
            }
            else if (oResult != null && oResult.IsError)
            {
                rule = ruleSet.GetRuleForResult(oResult);

                message.AddHeader("status-code", String.Format("{0}", oResult.Code));
                message.AddHeader("status-message", String.Format("CoberturaESError: {0}", oResult.Description));
            }
            else
            {
                return new RabbitMQAdvancedConsumerResult()
                {
                    delay = 0,
                    reject = false
                };
            }

            int currentRetryCount = message.GetRetryCount();
            int nextRetryCount = currentRetryCount + 1;
            if (rule.MustRetry(nextRetryCount))
            {
                return new RabbitMQAdvancedConsumerResult()
                {
                    delay = rule.GetNextDelay(nextRetryCount),
                    reject = false
                };
            }
            else
            {
                return new RabbitMQAdvancedConsumerResult()
                {
                    delay = 0,
                    reject = true
                };
            }
        }

        public async Task<GenericResult> Send(CoberturaMessage coberturaMessage)
        {
            HttpResponseMessage response = null;

            try
            {
                Console.WriteLine("Se va a enviar a Cobertura");
            }
            catch (TaskCanceledException ex)
            {
                return GenericResult.Error(MOAPIResultCode.NET_TIMEOUT, ex.Message);
            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                return GenericResult.Error(MOAPIResultCode.HTTP_UNKNOWN_HOST, ex.Message);
            }
            catch (Exception ex)
            {
                Helper.VolcarAConsola("CoberturaRabbitMQWorker", "Send", ex.Message, true);

                return GenericResult.Error(MOAPIResultCode.HTTP_EXCEPTION, ex.Message);
            }

            if ((int)response.StatusCode == 200)
            {
                return GenericResult.Success();
            }
            else
            {
                return GenericResult.Error(String.Format(MOAPIResultCode.HTTP_ERROR_CODE_XX, (int)response.StatusCode));
            }
        }
    }
}
