using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using ws_cobertura.httpapi.Model.Configuration;
using ws_cobertura.RabbitMQ;
using ws_cobertura.httpapi.Model.Request;
using ws_cobertura.httpapi.Model.Response;

namespace ws_cobertura.httpapi.Model.General
{
    public class APIHelper
    {
        protected readonly APIConfiguration _apiConfiguration;
        protected readonly CoberturaSender _coberturaSender;

        public JsonSerializerOptions oJsonSerializerOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        };

        public APIHelper(APIConfiguration apiConfiguration, CoberturaSender coberturaSender)
        {
            _apiConfiguration = apiConfiguration;
            _coberturaSender = coberturaSender;
        }

        public async Task<RegistrarDatosCoberturaResponse> RegistrarDatosCobertura(RegistrarDatosCoberturaRequest oRequest) {
            RegistrarDatosCoberturaResponse oResponse = new RegistrarDatosCoberturaResponse();

            string sTimeStamp = oRequest.timestamp;
            string sCoordenadax = oRequest.coordenadax;
            string sCoordenaday = oRequest.coordenaday;
            string sMunicipio = oRequest.municipio;
            string sIne = oRequest.ine;
            string sModelo = oRequest.modelo;
            string sSO = oRequest.so;
            string sTipoRed = oRequest.tipoRed;
            string sOperador = oRequest.operador;
            string sValorIntensidadSenial = oRequest.valorIntensidadSenial;
            string sRangoIntensidadSenial = oRequest.rangoIntensidadSenial;
            string sVelocidadBajada = oRequest.velocidadBajada;
            string sVelocidadSubida = oRequest.velocidadSubida;
            string sLatencia = oRequest.latencia;

            if (string.IsNullOrEmpty(sTipoRed)) {
                oResponse.estadoRespuesta = "0";
                oResponse.mensajeRespuesta = "Faltan datos";

                return oResponse;
            }

            if (string.IsNullOrEmpty(sIne)) {
                sIne = Constantes.CAMPO_SIN_DATOS;
            }

            if (string.IsNullOrEmpty(sMunicipio)) {
                sMunicipio = Constantes.CAMPO_SIN_DATOS;
            }

            CoberturaMessage oCoberturaMessage = new CoberturaMessage();
            oCoberturaMessage.timestamp = sTimeStamp;

            if (!string.IsNullOrEmpty(sCoordenadax)) {
                oCoberturaMessage.coordenadax = Double.Parse(sCoordenadax, CultureInfo.InvariantCulture);
            }
            if (!string.IsNullOrEmpty(sCoordenaday)) {
                oCoberturaMessage.coordenaday = Double.Parse(sCoordenaday, CultureInfo.InvariantCulture);
            }

            oCoberturaMessage.municipio = sMunicipio;
            oCoberturaMessage.ine = sIne;
            oCoberturaMessage.modelo = sModelo;
            oCoberturaMessage.so = sSO;
            oCoberturaMessage.tipoRed = sTipoRed;
            oCoberturaMessage.operador = sOperador;

            if (!string.IsNullOrEmpty(sVelocidadBajada)) {
                oCoberturaMessage.velocidadBajada = Decimal.Parse(sVelocidadBajada, CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(sVelocidadSubida))
            {
                oCoberturaMessage.velocidadSubida = Decimal.Parse(sVelocidadSubida, CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(sLatencia))
            {
                oCoberturaMessage.latencia = Decimal.Parse(sLatencia, CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(oCoberturaMessage.timestamp)) {
                DateTime dTimeParsed = DateTime.Parse(oCoberturaMessage.timestamp, CultureInfo.InvariantCulture);

                oCoberturaMessage.timestamp_seconds = Helper.DateTimeToEpochSeconds(dTimeParsed);
            }

            if (!string.IsNullOrEmpty(sValorIntensidadSenial))
            {
                oCoberturaMessage.valorIntensidadSenial = Decimal.Parse(sValorIntensidadSenial, CultureInfo.InvariantCulture);
            }

            if (string.IsNullOrEmpty(sRangoIntensidadSenial))
            {
                if (!string.IsNullOrEmpty(sValorIntensidadSenial))
                {
                    EnumRango.Intensidad oIntensidad = EnumRango.calcularRangoIntensidad(oCoberturaMessage.valorIntensidadSenial);
                    oCoberturaMessage.rangoIntensidadSenial = (int) oIntensidad;
                }
                else {
                    EnumRango.Intensidad oIntensidad = EnumRango.calcularRangoIntensidad(null);
                    oCoberturaMessage.rangoIntensidadSenial = (int) oIntensidad;
                }
            }
            else {
                oCoberturaMessage.rangoIntensidadSenial = int.Parse(sRangoIntensidadSenial, CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(sLatencia))
            {
                decimal? dRangoLatencia = null;

                try
                {
                    dRangoLatencia = decimal.Parse(sLatencia);
                }
                catch (Exception exx) { 
                
                }

                EnumRango.Latencia oLatencia = EnumRango.calcularRangoLatencia(dRangoLatencia);
                oCoberturaMessage.rangoLatencia = (int)oLatencia;
            }
            else
            {
                EnumRango.Latencia oLatencia = EnumRango.calcularRangoLatencia(null);
                oCoberturaMessage.rangoLatencia = (int)oLatencia;
            }

            if (!string.IsNullOrEmpty(sVelocidadBajada))
            {
                decimal? dRangoVelocidadBajada = null;

                try
                {
                    dRangoVelocidadBajada = decimal.Parse(sVelocidadBajada);
                }
                catch (Exception exx)
                {

                }

                EnumRango.VelocidadBajada oVelocidadBajada = EnumRango.calcularRangoVelocidadBajada(dRangoVelocidadBajada);
                oCoberturaMessage.rangoVelocidadBajada = (int)oVelocidadBajada;
            }
            else
            {
                EnumRango.VelocidadBajada oVelocidadBajada = EnumRango.calcularRangoVelocidadBajada(null);
                oCoberturaMessage.rangoVelocidadBajada = (int)oVelocidadBajada;
            }

            if (!string.IsNullOrEmpty(sVelocidadSubida))
            {
                decimal? dRangoVelocidadSubida = null;

                try
                {
                    dRangoVelocidadSubida = decimal.Parse(sVelocidadSubida);
                }
                catch (Exception exx)
                {

                }

                EnumRango.VelocidadSubida oVelocidadSubida = EnumRango.calcularRangoVelocidadSubida(dRangoVelocidadSubida);
                oCoberturaMessage.rangoVelocidadSubida = (int)oVelocidadSubida;
            }
            else
            {
                EnumRango.VelocidadSubida oVelocidadSubida = EnumRango.calcularRangoVelocidadSubida(null);
                oCoberturaMessage.rangoVelocidadSubida = (int)oVelocidadSubida;
            }

            if (!string.IsNullOrEmpty(sLatencia))
            {
                decimal? dRangoLatencia = null;

                try
                {
                    dRangoLatencia = decimal.Parse(sLatencia);
                }
                catch (Exception exx)
                {

                }

                EnumRango.Latencia oLatencia = EnumRango.calcularRangoLatencia(dRangoLatencia);
                oCoberturaMessage.rangoLatencia = (int)oLatencia;
            }
            else
            {
                EnumRango.Latencia oLatencia = EnumRango.calcularRangoLatencia(null);
                oCoberturaMessage.rangoLatencia = (int)oLatencia;
            }

            int iResult = 0;

            try
            {
                iResult = await _coberturaSender.SendMessageNetQ(oCoberturaMessage);
            }
            catch (Exception ex) {
                Helper.VolcarAConsola("APIHelper", "RegistrarDatosCobertura.SendMessageNetQ", ex.Message, true);
                iResult = 0;
            }

            if (iResult == 1)
            {
                oResponse.estadoRespuesta = "1";
                oResponse.mensajeRespuesta = string.Empty;
            }
            else {
                oResponse.estadoRespuesta = "0";
                oResponse.mensajeRespuesta = "No se pudo guardar la información";
            }

            return oResponse;
        }

        public async Task<ObtenerMunicipioPorCoordenadasResponse> ObtenerMunicipioPorCoordenadas(string sLatitudGPS, string sLongitudGPS) {
            ObtenerMunicipioPorCoordenadasResponse oResponse = new ObtenerMunicipioPorCoordenadasResponse();

            Location oLocation = new Location();

            double dLatitudGPS = double.Parse(sLatitudGPS, CultureInfo.InvariantCulture);
            double dLongitudGPS = double.Parse(sLongitudGPS, CultureInfo.InvariantCulture);
            double dCoordenadaXUTM = 0;
            double dCoordenadaYUTM = 0;

            //convertimos de latitud, longitud a coordenadas utm
            oLocation.Reproject(dLatitudGPS, dLongitudGPS);

            dCoordenadaXUTM = oLocation.utm_X;
            dCoordenadaYUTM = oLocation.utm_Y;

            string sUrlEndpointBusqueda = _apiConfiguration.UrlObtenerMunicipioPorCoordenadas;

            string sCoordenadaXUTM = dCoordenadaXUTM.ToString("G", CultureInfo.InvariantCulture);
            string sCoordenadaYUTM = dCoordenadaYUTM.ToString("G", CultureInfo.InvariantCulture);

            sUrlEndpointBusqueda = sUrlEndpointBusqueda.Replace("{coordenadaX}", sCoordenadaXUTM);
            sUrlEndpointBusqueda = sUrlEndpointBusqueda.Replace("{coordenadaY}", sCoordenadaYUTM);

            HttpResponseMessage oClientResponse = await new HttpClient().GetAsync(sUrlEndpointBusqueda);

            string sResponseContent = await oClientResponse.Content.ReadAsStringAsync();
            
            var requestXmlBody = new XmlDocument();
            requestXmlBody.LoadXml(sResponseContent);

            string sCodigoMunicipio = string.Empty;
            string sNombreMunicipio = string.Empty;
            string sProvinciaMunicipio = string.Empty;

            XmlNodeList oListNodesCodigoMunicipio = requestXmlBody.GetElementsByTagName("VISOR2D:c_muni_ine");
            XmlNodeList oListNodesNombreMunicipio = requestXmlBody.GetElementsByTagName("VISOR2D:d_muni_ine");
            XmlNodeList oListNodesProvinciaMunicipio = requestXmlBody.GetElementsByTagName("VISOR2D:provincia");

            if (oListNodesCodigoMunicipio.Count > 0)
            {
                sCodigoMunicipio = oListNodesCodigoMunicipio[0].InnerText;
            }
            if (oListNodesNombreMunicipio.Count > 0)
            {
                sNombreMunicipio = oListNodesNombreMunicipio[0].InnerText;
            }
            if (oListNodesProvinciaMunicipio.Count > 0)
            {
                sProvinciaMunicipio = oListNodesProvinciaMunicipio[0].InnerText;
            }

            if (!string.IsNullOrEmpty(sCodigoMunicipio) && !string.IsNullOrEmpty(sNombreMunicipio) && !string.IsNullOrEmpty(sProvinciaMunicipio))
            {
                oResponse.estadoRespuesta = "1";
                oResponse.mensajeRespuesta = string.Empty;

                oResponse.ineMunicipio = sCodigoMunicipio;
                oResponse.nombreMunicipio = sNombreMunicipio;
                oResponse.provincia = sProvinciaMunicipio;

                string sMultiploRedondeo = _apiConfiguration.MultiploRedondeoAnonimizarUTM;

                int iMultiploRedondeo = 1;

                if (!string.IsNullOrEmpty(sMultiploRedondeo))
                {
                    int.TryParse(sMultiploRedondeo, out iMultiploRedondeo);
                }

                int iCoordenadaXUTMAnonmizada = ((int)(dCoordenadaXUTM / iMultiploRedondeo)) * iMultiploRedondeo;
                int iCoordenadaYUTMAnonmizada = ((int)(dCoordenadaYUTM / iMultiploRedondeo)) * iMultiploRedondeo;

                string sCoordenadaXUTMAnonimizada = iCoordenadaXUTMAnonmizada.ToString("G", CultureInfo.InvariantCulture);
                string sCoordenadaYUTMAnonimizada = iCoordenadaYUTMAnonmizada.ToString("G", CultureInfo.InvariantCulture);

                oResponse.coordenadax = sCoordenadaXUTMAnonimizada;
                oResponse.coordenaday = sCoordenadaYUTMAnonimizada;
            }
            else
            {
                oResponse.estadoRespuesta = "0";
                oResponse.mensajeRespuesta = "No se encontraron conincidencias";
            }

            return oResponse;
        }


        public async Task<ObtenerCoordenadasPorMunicipioResponse> ObtenerCoordenadasPorMunicipio(string sMunicipioBuscar)
        {
            ObtenerCoordenadasPorMunicipioResponse oResponse = new ObtenerCoordenadasPorMunicipioResponse();

            string sUrlEndpointBusqueda = _apiConfiguration.UrlObtenerCoordenadasPorMunicipio;

            sUrlEndpointBusqueda = sUrlEndpointBusqueda.Replace("{textoBuscar}", sMunicipioBuscar);

            HttpResponseMessage oClientResponse = await new HttpClient().GetAsync(sUrlEndpointBusqueda);

            string sResponseContent = await oClientResponse.Content.ReadAsStringAsync();

            var requestXmlBody = new XmlDocument();
            requestXmlBody.LoadXml(sResponseContent);

            XmlNodeList oListNodes = requestXmlBody.GetElementsByTagName("SearchResult");

            List<string> sListElementosRespuestaSinTratar = new List<string>();

            foreach (XmlNode oNode in oListNodes)
            {
                string sElementoRespuestaSinTratar = oNode.SelectSingleNode("List").InnerText;

                if (!string.IsNullOrEmpty(sElementoRespuestaSinTratar))
                {
                    sListElementosRespuestaSinTratar.Add(sElementoRespuestaSinTratar);
                }
            }

            string[] separadorFilas = new string[] { "\n" };
            string[] separadorColumnas = new string[] { "#" };
            string[] separadorCoordenadas = new string[] { ":" };

            List<string> sListElementosRespuestaTratados = new List<string>();

            foreach (string sElemento in sListElementosRespuestaSinTratar)
            {
                string[] sElementosSeparados = sElemento.Split(separadorFilas, StringSplitOptions.None);

                sElementosSeparados.ToList().ForEach(x => sListElementosRespuestaTratados.Add(x));
            }

            string sCoordenadasEncontradas = string.Empty;

            foreach (string sElemento in sListElementosRespuestaTratados)
            {
                if (!string.IsNullOrEmpty(sElemento))
                {
                    string[] sCamposSeparados = sElemento.Split(separadorColumnas, StringSplitOptions.None);

                    if (sCamposSeparados.Length >= 2)
                    {

                        if (sCamposSeparados[0] == sCamposSeparados[1])
                        {
                            if (!string.IsNullOrEmpty(sCamposSeparados[2]) && sCamposSeparados[2] != "0.0:0.0")
                            {
                                sCoordenadasEncontradas = sCamposSeparados[2];
                                break;
                            }
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(sCoordenadasEncontradas))
            {
                string[] sSplitCoordenadas = sCoordenadasEncontradas.Split(separadorCoordenadas, StringSplitOptions.None);

                oResponse.estadoRespuesta = "1";
                oResponse.mensajeRespuesta = string.Empty;

                oResponse.coordenadax = sSplitCoordenadas[0];
                oResponse.coordenaday = sSplitCoordenadas[1];
            }
            else
            {
                oResponse.estadoRespuesta = "0";
                oResponse.mensajeRespuesta = "No se encontraron conincidencias";
            }

            return oResponse;
        }

        public AnonimizarCoordenadasUTMResponse AnonimizarCoordenadasUTM(double coordenadaXUTM, double coordenadaYUTM) {
            AnonimizarCoordenadasUTMResponse oResponse = new AnonimizarCoordenadasUTMResponse();

            string sMultiploRedondeo = _apiConfiguration.MultiploRedondeoAnonimizarUTM;

            int iMultiploRedondeo = 1;

            if (!string.IsNullOrEmpty(sMultiploRedondeo))
            {
                int.TryParse(sMultiploRedondeo, out iMultiploRedondeo);
            }

            int iCoordenadaXUTMAnonmizada = ((int)(coordenadaXUTM / iMultiploRedondeo)) * iMultiploRedondeo;
            int iCoordenadaYUTMAnonmizada = ((int)(coordenadaYUTM / iMultiploRedondeo)) * iMultiploRedondeo;

            oResponse.coordenadax = iCoordenadaXUTMAnonmizada;
            oResponse.coordenaday = iCoordenadaYUTMAnonmizada;

            return oResponse;
        }
        
    }
}
