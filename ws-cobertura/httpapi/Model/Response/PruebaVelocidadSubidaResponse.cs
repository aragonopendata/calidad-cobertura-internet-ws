using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace ws_cobertura.httpapi.Model.Response
{
    class PruebaVelocidadSubidaResponse
    {
        [JsonPropertyName("estadoRespuesta")]
        public string estadoRespuesta { get; set; } // Estado de la respuesta. 1 = Correcto. 0 = Error.
        [JsonPropertyName("mensajeRespuesta")]
        public string mensajeRespuesta { get; set; } // Si el campo “estadoRespuesta” viene a 0 en este campo vendrá el texto de error que se ha de mostrar al usuario.
    }
}
