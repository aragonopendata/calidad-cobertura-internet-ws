using System.Text.Json.Serialization;

namespace ws_cobertura.httpapi.Model.Request
{
    public class ObtenerMunicipioPorCoordenadasRequest
    {
        [JsonPropertyName("latitud")]
        public string latitud { get; set; }  // Latitud de la posición GPS de la que queremos conocer su municipio.
        [JsonPropertyName("longitud")]
        public string longitud { get; set; } // Longitud de la posición GPS de la que queremos conocer su municipio.
    }
}
