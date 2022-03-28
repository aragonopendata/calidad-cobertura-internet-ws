using System.Text.Json.Serialization;

namespace ws_cobertura.httpapi.Model.Request
{
    public class ObtenerCoordenadasPorMunicipioRequest
    {
        [JsonPropertyName("municipio")]
        public string municipio { get; set; } // Nombre del municipio a buscar.Es el texto que mete el usuario en la caja de texto de la pantalla.
    }
}
