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
using System.IO;
using Newtonsoft.Json.Linq;

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
            string sCoordenadax5000 = oRequest.coordenadax5000;
            string sCoordenaday5000 = oRequest.coordenaday5000;
            string sCoordenadax20000 = oRequest.coordenadax20000;
            string sCoordenaday20000 = oRequest.coordenaday20000;
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

            string sCategoria = oRequest.categoria;

            if (string.IsNullOrEmpty(sTipoRed)) {
                oResponse.estadoRespuesta = "0";
                oResponse.mensajeRespuesta = "Faltan datos";

                return oResponse;
            }

            sTipoRed = normalizarTipoRed(sTipoRed);

            CoberturaMessage oCoberturaMessage = new CoberturaMessage();

            oCoberturaMessage.propuesto_para_revision = requiereRevision(oRequest);

            oCoberturaMessage.timestamp = sTimeStamp;

            if (!string.IsNullOrEmpty(sCoordenadax)) {
                oCoberturaMessage.coordenadax = Double.Parse(sCoordenadax, CultureInfo.InvariantCulture);
            }
            if (!string.IsNullOrEmpty(sCoordenaday)) {
                oCoberturaMessage.coordenaday = Double.Parse(sCoordenaday, CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(sCoordenadax5000))
            {
                oCoberturaMessage.coordenadax5000 = Double.Parse(sCoordenadax5000, CultureInfo.InvariantCulture);
            }
            if (!string.IsNullOrEmpty(sCoordenaday5000))
            {
                oCoberturaMessage.coordenaday5000 = Double.Parse(sCoordenaday5000, CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(sCoordenadax20000))
            {
                oCoberturaMessage.coordenadax20000 = Double.Parse(sCoordenadax20000, CultureInfo.InvariantCulture);
            }
            if (!string.IsNullOrEmpty(sCoordenaday20000))
            {
                oCoberturaMessage.coordenaday20000 = Double.Parse(sCoordenaday20000, CultureInfo.InvariantCulture);
            }

            if (string.IsNullOrEmpty(sIne))
            {
                sIne = Constantes.CAMPO_SIN_DATOS;
            }

            List<string> sListMunicipios = new List<string>();

            if (string.IsNullOrEmpty(sMunicipio))
            {
                sMunicipio = Constantes.CAMPO_SIN_DATOS;
            }
            else {
                sListMunicipios.Add(sMunicipio);
            }

            oCoberturaMessage.municipio = sMunicipio;
            oCoberturaMessage.arrMunicipios = sListMunicipios.ToArray();

            oCoberturaMessage.ine = sIne;
            oCoberturaMessage.modelo = sModelo;
            oCoberturaMessage.so = sSO;

            List<string> sListTipoRed = new List<string>();
            sListTipoRed.Add(sTipoRed);

            oCoberturaMessage.tipoRed = sTipoRed;
            oCoberturaMessage.arrTipoRed = sListTipoRed.ToArray();

            oCoberturaMessage.operador = sOperador;

            List<string> sListCategorias = new List<string>();

            if (string.IsNullOrEmpty(sCategoria) || (sCategoria != Constantes.RED_CABLEADA && sCategoria != Constantes.RED_MOVIL))
            {
                oCoberturaMessage.categoria = obtenerCategoria(oCoberturaMessage.so, oCoberturaMessage.modelo, oCoberturaMessage.tipoRed);
            }
            else
            {
                oCoberturaMessage.categoria = sCategoria;
            }

            sListCategorias.Add(oCoberturaMessage.categoria);

            oCoberturaMessage.arrCategorias = sListCategorias.ToArray();

            if (string.IsNullOrEmpty(sVelocidadBajada))
            {
                sVelocidadBajada = "0";
            }

            if (string.IsNullOrEmpty(sVelocidadSubida))
            {
                sVelocidadSubida = "0";
            }

            oCoberturaMessage.velocidadBajada = Decimal.Parse(sVelocidadBajada, CultureInfo.InvariantCulture);

            oCoberturaMessage.velocidadSubida = Decimal.Parse(sVelocidadSubida, CultureInfo.InvariantCulture);

            /*if (!string.IsNullOrEmpty(sVelocidadBajada)) {
                oCoberturaMessage.velocidadBajada = Decimal.Parse(sVelocidadBajada, CultureInfo.InvariantCulture);

                if (oCoberturaMessage.velocidadBajada == 0)
                {
                    oCoberturaMessage.velocidadBajada = null;
                }
            }

            if (!string.IsNullOrEmpty(sVelocidadSubida))
            {
                oCoberturaMessage.velocidadSubida = Decimal.Parse(sVelocidadSubida, CultureInfo.InvariantCulture);

                if (oCoberturaMessage.velocidadSubida == 0)
                {
                    oCoberturaMessage.velocidadSubida = null;
                }
            }*/

            if (!string.IsNullOrEmpty(sLatencia))
            {
                oCoberturaMessage.latencia = Decimal.Parse(sLatencia, CultureInfo.InvariantCulture);

                if (oCoberturaMessage.latencia == 0)
                {
                    oCoberturaMessage.latencia = null;
                }
            }

            if (!string.IsNullOrEmpty(oCoberturaMessage.timestamp)) {
                DateTime dTimeParsed = DateTime.Parse(oCoberturaMessage.timestamp, CultureInfo.InvariantCulture);

                oCoberturaMessage.timestamp_seconds = Helper.DateTimeToEpochSeconds(dTimeParsed);
            }

            if (!string.IsNullOrEmpty(sValorIntensidadSenial))
            {
                oCoberturaMessage.valorIntensidadSenial = Decimal.Parse(sValorIntensidadSenial, CultureInfo.InvariantCulture);

                if (oCoberturaMessage.valorIntensidadSenial == 0) {
                    oCoberturaMessage.valorIntensidadSenial = null;
                }                
            }


            /*if (string.IsNullOrEmpty(sRangoIntensidadSenial))
            {
                if (!string.IsNullOrEmpty(sValorIntensidadSenial))
                {
                    EnumRango.Intensidad oIntensidad = EnumRango.calcularRangoIntensidad(oCoberturaMessage.valorIntensidadSenial, oCoberturaMessage.tipoRed);
                    oCoberturaMessage.rangoIntensidadSenial = (int) oIntensidad;
                }
                else {
                    EnumRango.Intensidad oIntensidad = EnumRango.calcularRangoIntensidad(null, oCoberturaMessage.tipoRed);
                    oCoberturaMessage.rangoIntensidadSenial = (int) oIntensidad;
                }
            }
            else {
                oCoberturaMessage.rangoIntensidadSenial = int.Parse(sRangoIntensidadSenial, CultureInfo.InvariantCulture);
            }*/

            if (!string.IsNullOrEmpty(sValorIntensidadSenial))
            {
                EnumRango.Intensidad oIntensidad = EnumRango.calcularRangoIntensidad(oCoberturaMessage.valorIntensidadSenial, oCoberturaMessage.tipoRed);
                oCoberturaMessage.rangoIntensidadSenial = (int)oIntensidad;
            }
            else
            {
                EnumRango.Intensidad oIntensidad = EnumRango.calcularRangoIntensidad(null, oCoberturaMessage.tipoRed);
                oCoberturaMessage.rangoIntensidadSenial = (int)oIntensidad;
            }

            if (!string.IsNullOrEmpty(sLatencia))
            {
                decimal? dRangoLatencia = null;

                try
                {
                    dRangoLatencia = Decimal.Parse(sLatencia, CultureInfo.InvariantCulture);
                }
                catch (Exception exx) { 
                
                }

                EnumRango.Latencia oLatencia = EnumRango.calcularRangoLatencia(dRangoLatencia, oCoberturaMessage.categoria);
                oCoberturaMessage.rangoLatencia = (int)oLatencia;
            }
            else
            {
                EnumRango.Latencia oLatencia = EnumRango.calcularRangoLatencia(null, oCoberturaMessage.categoria);
                oCoberturaMessage.rangoLatencia = (int)oLatencia;
            }

            decimal? dVelocidadBajada = 0;

            try
            {
                dVelocidadBajada = Decimal.Parse(sVelocidadBajada, CultureInfo.InvariantCulture);
            }
            catch (Exception exx)
            {

            }

            EnumRango.VelocidadBajada oVelocidadBajada = EnumRango.calcularRangoVelocidadBajada(dVelocidadBajada, oCoberturaMessage.categoria);
            oCoberturaMessage.rangoVelocidadBajada = (int)oVelocidadBajada;

            decimal? dVelocidadSubida = 0;

            try
            {
                dVelocidadSubida = Decimal.Parse(sVelocidadSubida, CultureInfo.InvariantCulture);
            }
            catch (Exception exx)
            {

            }

            EnumRango.VelocidadSubida oVelocidadSubida = EnumRango.calcularRangoVelocidadSubida(dVelocidadSubida, oCoberturaMessage.categoria);
            oCoberturaMessage.rangoVelocidadSubida = (int)oVelocidadSubida;
            /*if (!string.IsNullOrEmpty(sVelocidadBajada))
            {
                decimal? dRangoVelocidadBajada = null;

                try
                {
                    dRangoVelocidadBajada = Decimal.Parse(sVelocidadBajada, CultureInfo.InvariantCulture);
                }
                catch (Exception exx)
                {

                }

                EnumRango.VelocidadBajada oVelocidadBajada = EnumRango.calcularRangoVelocidadBajada(dRangoVelocidadBajada, oCoberturaMessage.categoria);
                oCoberturaMessage.rangoVelocidadBajada = (int)oVelocidadBajada;
            }
            else
            {
                EnumRango.VelocidadBajada oVelocidadBajada = EnumRango.calcularRangoVelocidadBajada(null, oCoberturaMessage.categoria);
                oCoberturaMessage.rangoVelocidadBajada = (int)oVelocidadBajada;
            }*/

            /*if (!string.IsNullOrEmpty(sVelocidadSubida))
            {
                decimal? dRangoVelocidadSubida = null;

                try
                {
                    dRangoVelocidadSubida = Decimal.Parse(sVelocidadSubida, CultureInfo.InvariantCulture);
                }
                catch (Exception exx)
                {

                }

                EnumRango.VelocidadSubida oVelocidadSubida = EnumRango.calcularRangoVelocidadSubida(dRangoVelocidadSubida, oCoberturaMessage.categoria);
                oCoberturaMessage.rangoVelocidadSubida = (int)oVelocidadSubida;
            }
            else
            {
                EnumRango.VelocidadSubida oVelocidadSubida = EnumRango.calcularRangoVelocidadSubida(null, oCoberturaMessage.categoria);
                oCoberturaMessage.rangoVelocidadSubida = (int)oVelocidadSubida;
            }*/

            if (!string.IsNullOrEmpty(sLatencia))
            {
                decimal? dRangoLatencia = null;

                try
                {
                    dRangoLatencia = Decimal.Parse(sLatencia, CultureInfo.InvariantCulture);
                }
                catch (Exception exx)
                {

                }

                EnumRango.Latencia oLatencia = EnumRango.calcularRangoLatencia(dRangoLatencia, oCoberturaMessage.categoria);
                oCoberturaMessage.rangoLatencia = (int)oLatencia;
            }
            else
            {
                EnumRango.Latencia oLatencia = EnumRango.calcularRangoLatencia(null, oCoberturaMessage.categoria);
                oCoberturaMessage.rangoLatencia = (int)oLatencia;
            }

            int iResult = 0;

            if (oCoberturaMessage.velocidadBajada.HasValue)
            {
                try
                {
                    iResult = await _coberturaSender.SendMessageNetQ(oCoberturaMessage);
                }
                catch (Exception ex)
                {
                    Helper.VolcarAConsola("APIHelper", "RegistrarDatosCobertura.SendMessageNetQ", ex.Message, true);
                    iResult = 0;
                }
            }
            else {
                iResult = 1;
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

                AnonimizarCoordenadasUTMResponse pAnonimizarCoordenadasUTMResponse500 = AnonimizarCoordenadasUTM(dCoordenadaXUTM, dCoordenadaYUTM, 500, 250);

                // se usa como base el anonimizado pAnonimizarCoordenadasUTMResponse500
                //AnonimizarCoordenadasUTMResponse pAnonimizarCoordenadasUTMResponse5000 = AnonimizarCoordenadasUTM(pAnonimizarCoordenadasUTMResponse500.coordenadax, pAnonimizarCoordenadasUTMResponse500.coordenaday, 5000, 2500);
                //AnonimizarCoordenadasUTMResponse pAnonimizarCoordenadasUTMResponse20000 = AnonimizarCoordenadasUTM(pAnonimizarCoordenadasUTMResponse500.coordenadax, pAnonimizarCoordenadasUTMResponse500.coordenaday, 20000, 10000);

                AnonimizarCoordenadasUTMResponse pAnonimizarCoordenadasUTMResponse5000 = AnonimizarCoordenadasUTM(dCoordenadaXUTM, dCoordenadaYUTM, 5000, 2500);
                AnonimizarCoordenadasUTMResponse pAnonimizarCoordenadasUTMResponse20000 = AnonimizarCoordenadasUTM(dCoordenadaXUTM, dCoordenadaYUTM, 20000, 10000);

                oResponse.coordenadax = pAnonimizarCoordenadasUTMResponse500.coordenadax.ToString("G", CultureInfo.InvariantCulture);
                oResponse.coordenaday = pAnonimizarCoordenadasUTMResponse500.coordenaday.ToString("G", CultureInfo.InvariantCulture);

                oResponse.coordenadax5000 = pAnonimizarCoordenadasUTMResponse5000.coordenadax.ToString("G", CultureInfo.InvariantCulture);
                oResponse.coordenaday5000 = pAnonimizarCoordenadasUTMResponse5000.coordenaday.ToString("G", CultureInfo.InvariantCulture);
                oResponse.coordenadax20000 = pAnonimizarCoordenadasUTMResponse20000.coordenadax.ToString("G", CultureInfo.InvariantCulture);
                oResponse.coordenaday20000 = pAnonimizarCoordenadasUTMResponse20000.coordenaday.ToString("G", CultureInfo.InvariantCulture);
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

        public AnonimizarCoordenadasUTMResponse AnonimizarCoordenadasUTM(double coordenadaXUTM, double coordenadaYUTM, int iMultiploRedondeo, int iSumaRedondeo) {
            AnonimizarCoordenadasUTMResponse oResponse = new AnonimizarCoordenadasUTMResponse();

            /*string sMultiploRedondeo = _apiConfiguration.MultiploRedondeoAnonimizarUTM;

            int iMultiploRedondeo = 1;

            if (!string.IsNullOrEmpty(sMultiploRedondeo))
            {
                int.TryParse(sMultiploRedondeo, out iMultiploRedondeo);
            }*/

            int iCoordenadaXUTMAnonimizada = ((int)(coordenadaXUTM / iMultiploRedondeo)) * iMultiploRedondeo;
            int iCoordenadaYUTMAnonimizada = ((int)(coordenadaYUTM / iMultiploRedondeo)) * iMultiploRedondeo;

            /*string sSumaRedondeo = _apiConfiguration.SumaRedondeoAnonimizarUTM; // Se suman las unidades al redondeo en el caso de que se requiera un punto diferente al calculado. Al dividir entre 500 y parsear a integer, se obtiene la esquina de una cuadrícula de 500x500, al sumarle 250 al resultado se obtiene el centro.

            int iSumaRedondeo = 0;

            if (!string.IsNullOrEmpty(sSumaRedondeo)) 
            { 
                int.TryParse(sSumaRedondeo, out iSumaRedondeo);
            }*/

            iCoordenadaXUTMAnonimizada = iCoordenadaXUTMAnonimizada + iSumaRedondeo;
            iCoordenadaYUTMAnonimizada = iCoordenadaYUTMAnonimizada + iSumaRedondeo;

            oResponse.coordenadax = iCoordenadaXUTMAnonimizada;
            oResponse.coordenaday = iCoordenadaYUTMAnonimizada;

            return oResponse;
        }

        public MemoryStream generarFicheroXLSX(List<DatosCobertura> oListDatosCobertura)
        {
            MemoryStream mStreamFichero = new MemoryStream();

            try
            {
                string sRutaFichero = string.Empty;

                using (DocumentFormat.OpenXml.Packaging.SpreadsheetDocument document = DocumentFormat.OpenXml.Packaging.SpreadsheetDocument.Create(mStreamFichero, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook))
                {
                    DocumentFormat.OpenXml.Packaging.WorkbookPart workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();

                    DocumentFormat.OpenXml.Packaging.WorksheetPart workSheet = workbookPart.AddNewPart<DocumentFormat.OpenXml.Packaging.WorksheetPart>();

                    var sheetData = new DocumentFormat.OpenXml.Spreadsheet.SheetData();
                    workSheet.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet(sheetData);

                    DocumentFormat.OpenXml.Spreadsheet.Sheets sheets = workbookPart.Workbook.AppendChild(new DocumentFormat.OpenXml.Spreadsheet.Sheets());

                    DocumentFormat.OpenXml.Spreadsheet.Sheet sheetCobertura = new DocumentFormat.OpenXml.Spreadsheet.Sheet() { Id = workbookPart.GetIdOfPart(workSheet), SheetId = 1, Name = "Cobertura" };

                    sheets.Append(sheetCobertura);

                    DocumentFormat.OpenXml.Spreadsheet.Row headerRows = new DocumentFormat.OpenXml.Spreadsheet.Row();
                    List<String> columns = new List<string>();
                    JObject parsedJsonHeaderReporte = JObject.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(oListDatosCobertura[0]));

                    foreach (JProperty property in parsedJsonHeaderReporte.Properties())
                    {
                        columns.Add(property.Name.ToString());
                        DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                        cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                        cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(property.Name.ToString());
                        headerRows.AppendChild(cell);
                    }

                    sheetData.AppendChild(headerRows);

                    foreach (DatosCobertura oDatosCobertura in oListDatosCobertura)
                    {
                        //serializamos y deserializamos a JSON para obener el orden correcto de los campos según el orden de serialización               

                        JObject parsedJson = JObject.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(oDatosCobertura));

                        DocumentFormat.OpenXml.Spreadsheet.Row newRow = new DocumentFormat.OpenXml.Spreadsheet.Row();

                        foreach (JProperty property in parsedJson.Properties())
                        {
                            DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                            cell.DataType = DatosCobertura.getOpenXMLPropertyDataType(property.Name.ToString());
                            cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(property.Value.ToString());
                            newRow.AppendChild(cell);
                        }

                        sheetData.AppendChild(newRow);
                    }

                    workbookPart.Workbook.Save();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (mStreamFichero.Length > 0)
            {
                mStreamFichero.Seek(0, SeekOrigin.Begin);
            }

            return mStreamFichero;
        }

        private string normalizarTipoRed(string sTipoRed) {


            if (sTipoRed.ToLower() == Constantes.CONEXION_WIMAX.ToLower()) {
                sTipoRed = Constantes.CONEXION_WIMAX;
            } 
            else if (sTipoRed.ToLower() == Constantes.CONEXION_WIFI.ToLower())
            {
                sTipoRed = Constantes.CONEXION_WIFI;
            }
            else if (sTipoRed.ToLower() == Constantes.CONEXION_ETH.ToLower())
            {
                sTipoRed = Constantes.CONEXION_ETH;
            }
            else if (sTipoRed.ToLower() == Constantes.CONEXION_MOBILE.ToLower())
            {
                sTipoRed = Constantes.CONEXION_MOBILE;
            }
            else if (sTipoRed.ToLower() == Constantes.CONEXION_CELLULAR.ToLower())
            {
                sTipoRed = Constantes.CONEXION_MOBILE;
            }
            else if (sTipoRed.ToLower() == Constantes.CONEXION_2G.ToLower() || sTipoRed.ToLower() == Constantes.CONEXION_GSM.ToLower() || sTipoRed.ToLower() == Constantes.CONEXION_GPRS.ToLower() || sTipoRed.ToLower() == Constantes.CONEXION_EDGE.ToLower())
            {
                sTipoRed = Constantes.CONEXION_2G;
            }
            else if (sTipoRed.ToLower() == Constantes.CONEXION_3G.ToLower() || sTipoRed.ToLower() == Constantes.CONEXION_CDMA.ToLower() || sTipoRed.ToLower() == Constantes.CONEXION_UMTS.ToLower() || sTipoRed.ToLower() == Constantes.CONEXION_HSPA.ToLower()
                     || sTipoRed.ToLower() == Constantes.CONEXION_HSUPA.ToLower() || sTipoRed.ToLower() == Constantes.CONEXION_HSDPA.ToLower() || sTipoRed.ToLower() == Constantes.CONEXION_1XRTT.ToLower() || sTipoRed.ToLower() == Constantes.CONEXION_EHRPD.ToLower())
            {
                sTipoRed = Constantes.CONEXION_3G;
            }
            else if (sTipoRed.ToLower() == Constantes.CONEXION_4G.ToLower() || sTipoRed.ToLower() == Constantes.CONEXION_LTE.ToLower() || sTipoRed.ToLower() == Constantes.CONEXION_UMB.ToLower() || sTipoRed.ToLower() == Constantes.CONEXION_HSPA_PLUS.ToLower())
            {
                sTipoRed = Constantes.CONEXION_4G;
            }
            else if (sTipoRed.ToLower() == Constantes.CONEXION_5G.ToLower())
            {
                sTipoRed = Constantes.CONEXION_5G;
            }
            
            return sTipoRed;
        }

        private string obtenerCategoria(string sSO, string sModelo, string sTipoRed)
        {
            string sCategoria = Constantes.RED_MOVIL;

            if (!string.IsNullOrEmpty(sSO)) {
                sSO = sSO.ToLower().Trim();
            }

            if (!string.IsNullOrEmpty(sModelo))
            {
                sModelo = sModelo.ToLower().Trim();
            }

            if (sTipoRed == Constantes.CONEXION_WIFI) {
                sCategoria = Constantes.RED_CABLEADA;
            }
            else if (sSO == Constantes.SO_ANDROID.ToLower())
            {
                sCategoria = Constantes.RED_MOVIL;
            }
            else if (sSO == Constantes.SO_IOS.ToLower() || sModelo == Constantes.MODELO_IPHONE.ToLower())
            {
                sCategoria = Constantes.RED_MOVIL;
            }
            else if (sSO == Constantes.SO_OSX.ToLower() || sModelo == Constantes.MODELO_MAC.ToLower())
            {
                sCategoria = Constantes.RED_CABLEADA;
            }
            else if (sSO == Constantes.SO_LINUX.ToLower() || sModelo == Constantes.MODELO_LINUX.ToLower())
            {
                sCategoria = Constantes.RED_CABLEADA;
            }
            else if (sSO == Constantes.SO_WINDOWS.ToLower() || sModelo == Constantes.MODELO_PC.ToLower())
            {
                sCategoria = Constantes.RED_CABLEADA;
            }
            else {
                if (sTipoRed == Constantes.CONEXION_2G || sTipoRed == Constantes.CONEXION_3G || sTipoRed == Constantes.CONEXION_4G || sTipoRed == Constantes.CONEXION_5G || sTipoRed == Constantes.CONEXION_MOBILE || sTipoRed == Constantes.CONEXION_CELLULAR) {
                    sCategoria = Constantes.RED_MOVIL;
                } else if (sTipoRed == Constantes.CONEXION_ETH) {
                    sCategoria = Constantes.RED_CABLEADA;
                }
            }

            return sCategoria;
        }

        public bool comprobarCredencialesCarga(string sToken) {

            if (string.IsNullOrEmpty(sToken) || _apiConfiguration.tokenCarga != sToken)
            {
                return false;
            }

            return true;
        }

        public bool comprobarCredencialesAccesibilidad(string sToken)
        {

            if (string.IsNullOrEmpty(sToken) || _apiConfiguration.tokenAccesibilidad != sToken)
            {
                return false;
            }

            return true;
        }

        public bool comprobarCredencialesOpenData(string sToken)
        {

            if (string.IsNullOrEmpty(_apiConfiguration.tokenOpenData)) { // si token en config vacio, no hace falta autenticacion
                return true;
            }

            if (string.IsNullOrEmpty(sToken)) {
                return false;
            }

            sToken = sToken.Replace("Bearer ", "");

            if (_apiConfiguration.tokenOpenData != sToken)
            {
                return false;
            }

            return true;
        }

        public bool? requiereRevision(RegistrarDatosCoberturaRequest oRequest) {

            bool? bRequiereRevision = null;


            /*if (!string.IsNullOrEmpty(oRequest.ubicacionManual)) {
                if (oRequest.ubicacionManual.ToLower() == "true" || oRequest.ubicacionManual.ToLower() == "1") {
                    bRequiereRevision = true;
                }
            }*/
            if (oRequest.ubicacionManual.HasValue && oRequest.ubicacionManual.Value == true)
            {
                bRequiereRevision = true;
            }

            return bRequiereRevision;

        }   

    }
}
