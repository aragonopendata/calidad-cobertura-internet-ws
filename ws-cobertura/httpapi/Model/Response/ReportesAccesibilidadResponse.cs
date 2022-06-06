using System.Collections.Generic;
using System.Text.Json.Serialization;
using ws_cobertura.ElasticSearch;

namespace ws_cobertura.httpapi.Model.Response
{
    public class ReportesAccesibilidadResponse
    {
        [JsonPropertyName("total")]
        public long total { get; set; }
        [JsonPropertyName("paginator")]
        public CoberturaPaginatorHelper paginator { get; set; }
        [JsonPropertyName("documents")]
        public List<DatosAccesibilidad> documents { get; set; } 
    }

    public class DatosAccesibilidad {
        [JsonPropertyName("Fecha")]
        public string Fecha { get; set; }
        [JsonPropertyName("Municipio")]
        public string Municipio { get; set; }
        [JsonPropertyName("Categoria")]
        public string Categoria { get; set; }
        [JsonPropertyName("Calidad")]
        public string Calidad { get; set; }
        [JsonPropertyName("Velocidad__Bajada")]
        public string Velocidad__Bajada { get; set; }
    }
}
