﻿using System.Text.Json.Serialization;

namespace ws_cobertura.httpapi.Model.Response
{
    public class ObtenerMunicipioPorCoordenadasResponse
    {
        [JsonPropertyName("nombreMunicipio")]
        public string nombreMunicipio { get; set; } //Nombre del municipio que se encuentra en la posición GPS.
        [JsonPropertyName("ineMunicipio")]
        public string ineMunicipio { get; set; } //INE del municipio que se encuentra en la posición GPS.
        [JsonPropertyName("provincia")]
        public string provincia { get; set; } //nombre de la provincia que se encuentra en la posición GPS.
        [JsonPropertyName("coordenadax")]
        public string coordenadax { get; set; } //Coordenada X de la posición geográfica en el estándar EPSG:25830.
        [JsonPropertyName("coordenaday")]
        public string coordenaday { get; set; } //Coordenada Y de la posición geográfica en el estándar EPSG:25830.
        [JsonPropertyName("estadoRespuesta")]
        public string estadoRespuesta { get; set; } //Estado de la respuesta. 1 = Correcto.Se ha obtenido el municipio correctamente. 0 = Error.No se ha obtenido el municipio correctamente.
        [JsonPropertyName("mensajeRespuesta")]
        public string mensajeRespuesta { get; set; } //Si el campo “estadoRespuesta” viene a 0 en este campo vendrá el texto de error que explica por qué ha fallado el servicio web
    }
}
