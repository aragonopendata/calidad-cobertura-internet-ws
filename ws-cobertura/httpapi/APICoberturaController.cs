using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ws_cobertura.httpapi.Model.General;
using ws_cobertura.httpapi.Model.Request;
using ws_cobertura.httpapi.Model.Response;

namespace ws_cobertura.httpapi
{
    public class APICoberturaController : ControllerBase
    {
        APIHelper _apiHelper;

        public APICoberturaController(APIHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        // recibimos los datos de coberturas para guardarlos en rabbitmq
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
    }
}
