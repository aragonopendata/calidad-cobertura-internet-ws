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

                    if (!oReport.message.coordenadax.HasValue || !oReport.message.coordenaday.HasValue)
                    {
                        try
                        {
                            ObtenerCoordenadasPorMunicipioResponse oObtenerCoordenadasPorMunicipioResponse = await _apiHelper.ObtenerCoordenadasPorMunicipio(oReport.message.municipio);

                            if (oObtenerCoordenadasPorMunicipioResponse.estadoRespuesta == "1" && !string.IsNullOrEmpty(oObtenerCoordenadasPorMunicipioResponse.coordenadax) && !string.IsNullOrEmpty(oObtenerCoordenadasPorMunicipioResponse.coordenaday))
                            {
                                oReport.message.coordenadax = Double.Parse(oObtenerCoordenadasPorMunicipioResponse.coordenadax, CultureInfo.InvariantCulture);
                                oReport.message.coordenaday = Double.Parse(oObtenerCoordenadasPorMunicipioResponse.coordenaday, CultureInfo.InvariantCulture);

                                AnonimizarCoordenadasUTMResponse oCoordenadasAnonimizadas500 = _apiHelper.AnonimizarCoordenadasUTM(oReport.message.coordenadax.Value, oReport.message.coordenaday.Value, 500, 250);

                                //se generan desde la anonimizada por 500
                                AnonimizarCoordenadasUTMResponse oCoordenadasAnonimizadas5000 = _apiHelper.AnonimizarCoordenadasUTM(oCoordenadasAnonimizadas500.coordenadax, oCoordenadasAnonimizadas500.coordenaday, 5000, 2500);
                                AnonimizarCoordenadasUTMResponse oCoordenadasAnonimizadas20000 = _apiHelper.AnonimizarCoordenadasUTM(oCoordenadasAnonimizadas500.coordenadax, oCoordenadasAnonimizadas500.coordenaday, 20000, 10000);

                                oReport.message.coordenadax = oCoordenadasAnonimizadas500.coordenadax;
                                oReport.message.coordenaday = oCoordenadasAnonimizadas500.coordenaday;

                                oReport.message.coordenadax5000 = oCoordenadasAnonimizadas5000.coordenadax;
                                oReport.message.coordenaday5000 = oCoordenadasAnonimizadas5000.coordenaday;

                                oReport.message.coordenadax20000 = oCoordenadasAnonimizadas20000.coordenadax;
                                oReport.message.coordenaday20000 = oCoordenadasAnonimizadas20000.coordenaday;
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

                    /*oReport.message.agrupado_cuadricula_tecnologia = ((int)oReport.message.coordenadax).ToString() + "|" + ((int)oReport.message.coordenaday).ToString() + "|" + oReport.message.tipoRed.ToString();
                    oReport.message.agrupado_ine_tecnologia = (oReport.message.ine + "|" + oReport.message.tipoRed.ToString());
                    oReport.message.agrupado_municipio_tecnologia = (oReport.message.municipio + "|" + oReport.message.tipoRed.ToString());*/
                    oReport.message.agrupado_cuadricula_500 = ((int)oReport.message.coordenadax).ToString() + "|" + ((int)oReport.message.coordenaday).ToString();
                    oReport.message.agrupado_cuadricula_5000 = ((int)oReport.message.coordenadax5000).ToString() + "|" + ((int)oReport.message.coordenaday5000).ToString();
                    oReport.message.agrupado_cuadricula_20000 = ((int)oReport.message.coordenadax20000).ToString() + "|" + ((int)oReport.message.coordenaday20000).ToString();

                    oReport.message.agrupado_cuadricula_500_categoria = ((int)oReport.message.coordenadax).ToString() + "|" + ((int)oReport.message.coordenaday).ToString() + "|" + oReport.message.categoria;
                    oReport.message.agrupado_cuadricula_5000_categoria = ((int)oReport.message.coordenadax5000).ToString() + "|" + ((int)oReport.message.coordenaday5000).ToString() + "|" + oReport.message.categoria;
                    oReport.message.agrupado_cuadricula_20000_categoria = ((int)oReport.message.coordenadax20000).ToString() + "|" + ((int)oReport.message.coordenaday20000).ToString() + "|" + oReport.message.categoria;

                    oReport.message.location = new CoberturaMessageLocation();

                    Location oLocation = new Location();

                    oLocation.InverseReproject((double) oReport.message.coordenadax, (double) oReport.message.coordenaday);

                    oReport.message.location.lat = oLocation.geo_x;
                    oReport.message.geoLat = oLocation.geo_x;
                    oReport.message.location.lon = oLocation.geo_y;
                    oReport.message.geoLon = oLocation.geo_y;

                    //para datos tooltip formateados
                    oReport.Categoria = oReport.message.categoria;

                    oReport.Fecha = Helper.EpochSecondsToDateTime(long.Parse(oReport.message.timestamp_seconds)).ToString("dd/MM/yyyy");

                    oReport.Municipio = oReport.message.municipio;
                    
                    EnumRango.Intensidad oRangoIntensidad = EnumRango.calcularRangoIntensidad(oReport.message.valorIntensidadSenial, oReport.message.tipoRed);
                    string textoRangoIntensidad = EnumRango.getStringValue(oRangoIntensidad);
                    string textoIntensidad = oReport.message.valorIntensidadSenial.HasValue ? textoRangoIntensidad + " (" + Decimal.Round(oReport.message.valorIntensidadSenial.Value, 0, MidpointRounding.AwayFromZero).ToString() + " dBm)" : Constantes.CAMPO_SIN_DATOS;

                    oReport.Intensidad = textoIntensidad;

                    EnumRango.Latencia oRangoLatencia = EnumRango.calcularRangoLatencia(oReport.message.latencia, oReport.message.categoria);
                    string tenxtoRangoLatencia = EnumRango.getStringValue(oRangoLatencia);
                    string textoLatencia = oReport.message.latencia.HasValue ? tenxtoRangoLatencia + " (" + Decimal.Round(oReport.message.latencia.Value, 0, MidpointRounding.AwayFromZero).ToString() + " ms)" : Constantes.CAMPO_SIN_DATOS;

                    oReport.Latencia = textoLatencia;

                    EnumRango.VelocidadBajada oRangoVelocidadBajada = EnumRango.calcularRangoVelocidadBajada(oReport.message.velocidadBajada, oReport.message.categoria);
                    string textoRangoVelocidadBajada = EnumRango.getStringValue(oRangoVelocidadBajada);
                    string textoVelocidadBajada = oReport.message.velocidadBajada.HasValue ? textoRangoVelocidadBajada + " (" + Decimal.Round(oReport.message.velocidadBajada.Value, 2, MidpointRounding.AwayFromZero).ToString() + " Mbps)" : Constantes.CAMPO_SIN_DATOS;

                    oReport.message.textoRangoVelocidadBajada = ((int)oRangoVelocidadBajada).ToString() + " - " + textoRangoVelocidadBajada;

                    oReport.Calidad = oReport.message.textoRangoVelocidadBajada;

                    oReport.Velocidad__Bajada = textoVelocidadBajada;

                    EnumRango.VelocidadSubida oRangoVelocidadSubida = EnumRango.calcularRangoVelocidadSubida(oReport.message.velocidadSubida, oReport.message.categoria);
                    string textoRangoVelocidadSubida = EnumRango.getStringValue(oRangoVelocidadSubida);
                    string textoVelocidadSubida = oReport.message.velocidadSubida.HasValue ? textoRangoVelocidadSubida + " (" + Decimal.Round(oReport.message.velocidadSubida.Value, 2, MidpointRounding.AwayFromZero).ToString() + " Mbps)" : Constantes.CAMPO_SIN_DATOS;

                    oReport.Velocidad__Subida = textoVelocidadSubida;

                    oReport.Tipo__Red = !string.IsNullOrEmpty(oReport.message.tipoRed) ? oReport.message.tipoRed : Constantes.CAMPO_SIN_DATOS;
                    oReport.Operador = !string.IsNullOrEmpty(oReport.message.operador) ? oReport.message.operador : Constantes.CAMPO_SIN_DATOS;
                    oReport.Tipo__Dispositivo = !string.IsNullOrEmpty(oReport.message.modelo) ? oReport.message.modelo : Constantes.CAMPO_SIN_DATOS;
                    oReport.SO__Dispositivo = !string.IsNullOrEmpty(oReport.message.so) ? oReport.message.so : Constantes.CAMPO_SIN_DATOS;

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
