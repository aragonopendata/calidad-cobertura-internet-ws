﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ws_cobertura.ElasticSearch;
using ws_cobertura.httpapi.Model.General;
using ws_cobertura.httpapi.Model.Request;
using ws_cobertura.httpapi.Model.Response;

namespace ws_cobertura.httpapi
{
    public class APICoberturaController : ControllerBase
    {
        APIHelper _apiHelper;
        CoberturaReportRepository _coberturaReportRepository;

        public APICoberturaController(APIHelper apiHelper, CoberturaReportRepository coberturaReportRepository)
        {
            _apiHelper = apiHelper;
            _coberturaReportRepository = coberturaReportRepository;
        }

        //RECIBIMOS LOS DATOS DE COBERTURA PARA INSERTARLOS EN RABBITMQ
        [Authorize]
        [HttpPost]
        [Route("api/registrarDatosCobertura")]
        public async Task<IActionResult> RegistrarDatosCobertura()
        {
            try
            {
                RegistrarDatosCoberturaResponse oResponse = new RegistrarDatosCoberturaResponse();
                string jsonResponse;

                string bodyData = await Helper.GetRawBodyAsync(HttpContext.Request);

                RegistrarDatosCoberturaRequest oRequest = JsonSerializer.Deserialize<RegistrarDatosCoberturaRequest>(bodyData);

                oResponse = await _apiHelper.RegistrarDatosCobertura(oRequest);

                jsonResponse = JsonSerializer.Serialize(oResponse, _apiHelper.oJsonSerializerOptions);

                return Ok(jsonResponse);
            }
            catch (Exception ex)
            {
                Helper.VolcarAConsola("APICoberturaController", "RegistrarDatosCobertura", ex.Message, true);

                return Conflict("");
            }
        }


        //RECIBIMOS LATITUD Y LONGITUD EN FORMATO GPS Y LAS TRANSFORMAMOS A 'EPSG:25830' (UTM) PARA ENVIARLAS A IGEAR Y RECIBIR DATOS DE MUNICIPIO LOS CUALES DEVOLVEMOS
        [Authorize]
        [HttpPost]
        [Route("api/obtenerMunicipioPorCoordenadas")]
        public async Task<IActionResult> ObtenerMunicipioPorCoordenadas()
        {
            try
            {
                ObtenerMunicipioPorCoordenadasResponse oResponse = new ObtenerMunicipioPorCoordenadasResponse();
                string jsonResponse;

                string bodyData = await Helper.GetRawBodyAsync(HttpContext.Request);

                ObtenerMunicipioPorCoordenadasRequest oRequest = JsonSerializer.Deserialize<ObtenerMunicipioPorCoordenadasRequest>(bodyData);

                if (oRequest == null || string.IsNullOrEmpty(oRequest.latitud) || string.IsNullOrEmpty(oRequest.longitud))
                {
                    return BadRequest();
                }

                string sLatitudGPS = oRequest.latitud;
                string sLongitudGPS = oRequest.longitud;

                oResponse = await _apiHelper.ObtenerMunicipioPorCoordenadas(sLatitudGPS, sLongitudGPS);

                jsonResponse = JsonSerializer.Serialize(oResponse, _apiHelper.oJsonSerializerOptions);

                return Ok(jsonResponse);
            }
            catch (Exception ex)
            {
                Helper.VolcarAConsola("APICoberturaController", "obtenerMunicipioPorCoordenadas", ex.Message, true);

                return Conflict("");
            }
        }

        //RECIBIMOS TEXTO CON NOMBRE MUNICIPIO PARA ENVIARLO A IGEAR Y RECIBIR LAS COORDENADAS EN FORMATO 'EPSG:25830' (UTM) LAS CUALES DEVOLVEMOS
        [Authorize]
        [HttpPost]
        [Route("api/obtenerCoordenadasPorMunicipio")]
        public async Task<IActionResult> ObtenerCoordenadasPorMunicipio()
        {

            try
            {
                ObtenerCoordenadasPorMunicipioResponse oResponse = new ObtenerCoordenadasPorMunicipioResponse();
                string jsonResponse;

                string bodyData = await Helper.GetRawBodyAsync(HttpContext.Request);

                ObtenerCoordenadasPorMunicipioRequest oRequest = JsonSerializer.Deserialize<ObtenerCoordenadasPorMunicipioRequest>(bodyData);

                if (oRequest == null || string.IsNullOrEmpty(oRequest.municipio))
                {
                    return BadRequest();
                }

                string sMunicipioBuscar = oRequest.municipio;

                oResponse = await _apiHelper.ObtenerCoordenadasPorMunicipio(sMunicipioBuscar);

                jsonResponse = JsonSerializer.Serialize(oResponse, _apiHelper.oJsonSerializerOptions);

                return Ok(jsonResponse);
            }
            catch (Exception ex)
            {
                Helper.VolcarAConsola("APICoberturaController", "ObtenerCoordenadasPorMunicipio", ex.Message, true);

                return Conflict("");
            }
        }

        //TEST VELOCIDAD SUBIDA FICHEROS
        [Authorize]
        [HttpPost]
        [Route("api/testVelocidadSubida")]
        public IActionResult PruebaVelocidadSubida()
        {

            try
            {
                PruebaVelocidadSubidaResponse oResponse = new PruebaVelocidadSubidaResponse();
                string jsonResponse;

                /* if (HttpContext.Request.Form.Files.Count > 0) // HttpContext.Request.Body.Length > 0 //HttpContext.Request.ContentLength.HasValue && HttpContext.Request.ContentLength.Value > 0
                 {
                     oResponse.estadoRespuesta = "1";
                     oResponse.mensajeRespuesta = string.Empty;
                 }
                 else {
                     oResponse.estadoRespuesta = "0";
                     oResponse.mensajeRespuesta = "No se pudo calcular la velocidad de subida.";
                 }*/
                oResponse.estadoRespuesta = "1";
                oResponse.mensajeRespuesta = string.Empty;

                jsonResponse = JsonSerializer.Serialize(oResponse, _apiHelper.oJsonSerializerOptions);

                return Ok(jsonResponse);
            }
            catch (Exception ex)
            {
                Helper.VolcarAConsola("APICoberturaController", "PruebaVelocidadSubida", ex.Message, true);

                return Conflict("");
            }
        }

        //SERVICIO SUBIDA DATOS BULK
        [Authorize]
        [HttpPost]
        [Route("api/cargarFicheroCobertura")]
        public async Task<IActionResult> CargarFicheroCobertura(List<IFormFile> files)
        {
            try
            {
                if (string.IsNullOrEmpty(HttpContext.Request.ContentType) || !HttpContext.Request.ContentType.ToLower().Contains("multipart")) {
                    return Conflict("Incorrect Content-Type");
                }

                if (files.Count <= 0)
                {
                    return Conflict("File not found");
                }

                IFormFile oFile = files.FirstOrDefault();

                if (oFile.ContentType != "text/csv") {
                    return Conflict("File not allowed");
                }

                var headers = HttpContext.Request.Headers;

                string nombreFichero = oFile.FileName;

                string sCredencialesCargaFichero = string.Empty;
                sCredencialesCargaFichero = headers.FirstOrDefault(x => x.Key == "creds").Value.FirstOrDefault();

                if (_apiHelper.comprobarCredencialesCarga(sCredencialesCargaFichero) == false) {
                    return Unauthorized();
                }

                StreamReader reader = null;

                MemoryStream tempStream = new MemoryStream();

                if (string.IsNullOrWhiteSpace(nombreFichero))
                {
                    return Conflict("Missing file name");
                }

                using (Stream fileContents = oFile.OpenReadStream())
                {
                    fileContents.CopyTo(tempStream);

                    tempStream.Seek(0, SeekOrigin.End);
                    StreamWriter writer = new StreamWriter(tempStream);
                    writer.WriteLine();
                    writer.Flush();
                    tempStream.Position = 0;
                }

                /*using (tempStream)
                {
                    oFile.CopyTo(tempStream);

                    tempStream.Seek(0, SeekOrigin.End);
                    StreamWriter writer = new StreamWriter(tempStream);
                    writer.WriteLine();
                    writer.Flush();
                    tempStream.Position = 0;

                }*/

                /*foreach (var file in files)
                {
                    var fileName = file.Name;

                    if (string.IsNullOrWhiteSpace(fileName))
                    {
                        continue;
                    }

                    using (tempStream)
                    {
                        file.CopyTo(tempStream);

                        tempStream.Seek(0, SeekOrigin.End);
                        StreamWriter writer = new StreamWriter(tempStream);
                        writer.WriteLine();
                        writer.Flush();
                        tempStream.Position = 0;
                    }
                }*/

                reader = new StreamReader(tempStream);

                //reader = new StreamReader(tempStream, Encoding.GetEncoding(1252));//1252 es ANSI encoding

                Helper.VolcarAConsola("APICoberturaController", "CargarFicheroCobertura", "Recibido fichero " + nombreFichero + ", procesando...", false);

                new Task(() => { iniciarCargaDatosCobertura(nombreFichero, reader); }).Start();
                
                return Ok();
            }
            catch (Exception ex)
            {
                Helper.VolcarAConsola("APICoberturaController", "CargarFicheroCobertura", ex.Message, true);

                return Conflict("");
            }
        }

        //SERVICIO EJECUCION AGRUPADO
        [Authorize]
        [HttpPost]
        [Route("api/lanzarAgrupado")]
        public async Task<IActionResult> LanzarAgrupado() {

            try {
                var headers = HttpContext.Request.Headers;

                string sCredencialesCargaFichero = string.Empty;
                sCredencialesCargaFichero = headers.FirstOrDefault(x => x.Key == "creds").Value.FirstOrDefault();

                if (_apiHelper.comprobarCredencialesCarga(sCredencialesCargaFichero) == false)
                {
                    return Unauthorized();
                }

                new Task(() => { _coberturaReportRepository.GroupCoberturaReports(); }).Start();

                //CoberturaReportRepository.GroupCoberturaReports
                return Ok();
            }
            catch (Exception ex)
            {
                Helper.VolcarAConsola("APICoberturaController", "LanzarAgrupado", ex.Message, true);

                return Conflict("");
            }
        }

        //OBTENER REPORTES RAW PAGINADOS PARA ACCESIBILIDAD
        [HttpPost]
        [Route("api/obtenerReportesAccesibilidad")]
        public async Task<IActionResult> ObtenerReportesAccesibilidad()
        {
           
            try
            {
                string jsonResponse = string.Empty;

                string bodyData = await Helper.GetRawBodyAsync(HttpContext.Request);

                ReportesAccesibilidadRequest oRequest = JsonSerializer.Deserialize<ReportesAccesibilidadRequest>(bodyData);

                if (oRequest == null || !oRequest.tipo.HasValue || !oRequest.skip.HasValue || !oRequest.take.HasValue)
                {
                    return BadRequest();
                }

                if (oRequest.take > 500) {
                    return BadRequest();
                }

                string sIndexMapping = string.Empty;

                if (oRequest.tipo == 0)
                {
                    sIndexMapping = "CoberturaReport";
                }
                /*else if (oRequest.tipo == 1)
                {
                    sIndexMapping = "CoberturaReportAgrupadoCuadricula500m";
                }
                else if (oRequest.tipo == 2)
                {
                    sIndexMapping = "CoberturaReportAgrupadoCuadricula5000m";
                }
                else if (oRequest.tipo == 3)
                {
                    sIndexMapping = "CoberturaReportAgrupadoCuadricula20000m";
                }*/
                else {
                    return Conflict();
                }

                List<string> sListMunicipios = new List<string>();
                List<string> sListCalidades = new List<string>();
                List<string> sListCategorias = new List<string>();
                string sLastDocumentId = string.Empty;

                if (oRequest.fmunicipios != null && oRequest.fmunicipios.Length > 0) {
                    sListMunicipios = oRequest.fmunicipios.ToList();
                }

                if (oRequest.fcategorias != null && oRequest.fcategorias.Length > 0)
                {
                    sListCategorias = oRequest.fcategorias.ToList();
                }

                if (oRequest.fcalidades != null && oRequest.fcalidades.Length > 0)
                {
                    sListCalidades = oRequest.fcalidades.ToList();
                }

                /*if (!string.IsNullOrEmpty(oRequest.last)) {
                    sLastDocumentId = oRequest.last;
                }*/

                //CoberturaPaginatorHelper oCoberturaPaginatorHelper = oRequest.paginator;

                ReportesAccesibilidadResponse oResponse = await _coberturaReportRepository.ObtenerReportesPaginados(sIndexMapping, oRequest.skip.Value, oRequest.take.Value, oRequest.sortBy, oRequest.sortOrder, sListMunicipios, sListCategorias, sListCalidades);

                if (oResponse.documents == null) {
                    oResponse.documents = new List<DatosAccesibilidad>();
                }

                jsonResponse = JsonSerializer.Serialize(oResponse, _apiHelper.oJsonSerializerOptions);

                return Ok(jsonResponse);
            }
            catch (Exception ex)
            {
                Helper.VolcarAConsola("APICoberturaController", "ObtenerReportesAccesibilidad", ex.Message, true);

                return Conflict("");
            }
        }

        private async Task<IActionResult> iniciarCargaDatosCobertura(string nombreFichero, StreamReader reader)
        {
            try
            {
                string sLinea;

                int iContador = 0;

                while ((sLinea = reader.ReadLine()) != null)
                {
                    try { 

                        iContador++;
                        if (iContador == 1) //cabecera
                        {
                            continue;
                        }

                        if (sLinea == null || sLinea == "")
                        {
                            continue;
                        }

                        string[] columns = sLinea.Split((char)0x3b); //char = ';'

                        int cantidadColumnas = columns.Length;

                        if (cantidadColumnas < 7)
                        {
                            continue;
                        }

                        if (string.IsNullOrEmpty(columns[0]))
                        {
                            continue;
                        }

                        if (string.IsNullOrEmpty(columns[1]))
                        {
                            continue;
                        }

                        if (string.IsNullOrEmpty(columns[2]))
                        {
                            continue;
                        }

                        if (string.IsNullOrEmpty(columns[3]))
                        {
                            continue;
                        }

                        if (string.IsNullOrEmpty(columns[4]))
                        {
                            continue;
                        }

                        string sOperador = columns[0];
                        string sTipoRed = columns[1];
                        string sTimestamp = columns[2];
                        string sLatitud = columns[3];
                        string sLongitud = columns[4];
                        string sIntensidad = columns[5]; //RED_MOVIL
                        string sLatencia = columns[5]; //RED_CABLEADA
                        string sVelocidadBajada = columns[6];
                        string sVelocidadSubida = columns[7];

                        string sCategoria = string.Empty;

                        try
                        {
                            sCategoria = columns[8];
                        }
                        catch (Exception exx) { 
                    
                        }

                        if (string.IsNullOrEmpty(sCategoria))
                        {
                            sCategoria = Constantes.RED_MOVIL;
                        }
                        else {
                            if (sCategoria != Constantes.RED_MOVIL && sCategoria != Constantes.RED_CABLEADA) {
                                sCategoria = Constantes.RED_MOVIL;
                            }
                        }

                        RegistrarDatosCoberturaRequest oRequest = new RegistrarDatosCoberturaRequest();

                        oRequest.operador = sOperador;
                        oRequest.tipoRed = sTipoRed;
                        oRequest.timestamp = sTimestamp;

                        string sLatitudGPS = sLatitud;
                        string sLongitudGPS = sLongitud;

                        if (!string.IsNullOrEmpty(sVelocidadBajada)) {
                            await Task.Delay(100);

                            ObtenerMunicipioPorCoordenadasResponse oResponseMunicipio = await _apiHelper.ObtenerMunicipioPorCoordenadas(sLatitudGPS, sLongitudGPS);

                            if (oResponseMunicipio.estadoRespuesta == "1")
                            {
                                oRequest.coordenadax = oResponseMunicipio.coordenadax;
                                oRequest.coordenaday = oResponseMunicipio.coordenaday;

                                oRequest.coordenadax5000 = oResponseMunicipio.coordenadax5000;
                                oRequest.coordenaday5000 = oResponseMunicipio.coordenaday5000;
                                oRequest.coordenadax20000 = oResponseMunicipio.coordenadax20000;
                                oRequest.coordenaday20000 = oResponseMunicipio.coordenaday20000;

                                oRequest.municipio = oResponseMunicipio.nombreMunicipio;
                                oRequest.ine = oResponseMunicipio.ineMunicipio;
             
                                if (sCategoria == Constantes.RED_CABLEADA)
                                {
                                    oRequest.latencia = sLatencia;
                                }
                                else
                                {
                                    oRequest.valorIntensidadSenial = sIntensidad;
                                }

                                oRequest.velocidadBajada = sVelocidadBajada;
                                oRequest.velocidadSubida = sVelocidadSubida;
                                oRequest.categoria = sCategoria;

                                RegistrarDatosCoberturaResponse oResponse = await _apiHelper.RegistrarDatosCobertura(oRequest);

                                if (oResponse.estadoRespuesta == "1")
                                {
                                    //Helper.VolcarAConsola("APICoberturaController", "iniciarCargaDatosCobertura", "Registro " + iContador.ToString() + " insertado con respuesta OK", false);
                                }
                                else
                                {
                                    Helper.VolcarAConsola("APICoberturaController", "iniciarCargaDatosCobertura", "Registro " + iContador.ToString() + " insertado con respuesta KO y mensaje: " + oResponse.mensajeRespuesta, false);
                                }
                            }
                            else
                            {
                                Helper.VolcarAConsola("APICoberturaController", "iniciarCargaDatosCobertura", "Registro " + iContador.ToString() + " no se pudieron obtener datos del municipio", false);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Helper.VolcarAConsola("APICoberturaController", "iniciarCargaDatosCobertura", "No se pudo insertar registro " + iContador.ToString() +": " + ex.Message, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.VolcarAConsola("APICoberturaController", "iniciarCargaDatosCobertura", ex.Message, true);
            }

            return null;
        }
    }
}
