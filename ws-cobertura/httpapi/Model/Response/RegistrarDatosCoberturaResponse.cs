using System.Text.Json.Serialization;

namespace ws_cobertura.httpapi.Model.Response
{
    public class RegistrarDatosCoberturaResponse
    {
        [JsonPropertyName("estadoRespuesta")]
        public string estadoRespuesta { get; set; } // Estado de la respuesta. 1 = Correcto.Se ha subido el reporte correctamente. 0 = Error.No se ha subido el reporte correctamente.
        [JsonPropertyName("mensajeRespuesta")]
        public string mensajeRespuesta { get; set; } // Si el campo “estadoRespuesta” viene a 0 en este campo vendrá el texto de error que se ha de mostrar al usuario para indicarle por qué no se ha podido subir correctamente su reporte.
    }
}
