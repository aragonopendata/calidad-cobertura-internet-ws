using System.Text.Json.Serialization;

namespace ws_cobertura.httpapi.Model.Response
{
    public class ObtenerCoordenadasPorMunicipioResponse
    {
        [JsonPropertyName("coordenadax")]
        public string coordenadax { get; set; } // Coordenada X en el estándar EPSG:25830 de donde se encuentra el municipio que se ha buscado.
        [JsonPropertyName("coordenaday")]
        public string coordenaday { get; set; } // Coordenada Y en el estándar EPSG:25830 de donde se encuentra el municipio que se ha buscado.
        [JsonPropertyName("estadoRespuesta")]
        public string estadoRespuesta { get; set; } // Estado de la respuesta. 1 = Correcto.Se han obtenido las coordenadas correctamente. 0 = Error.No se han obtenido las coordenadas correctamente.
        [JsonPropertyName("mensajeRespuesta")]
        public string mensajeRespuesta { get; set; } // Si el campo “estadoRespuesta” viene a 0 en este campo vendrá el texto de error que explica por qué ha fallado el servicio web.
    }
}
