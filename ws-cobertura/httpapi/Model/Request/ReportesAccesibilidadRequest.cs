using System.Text.Json.Serialization;
using ws_cobertura.ElasticSearch;

namespace ws_cobertura.httpapi.Model.Request
{
    public class ReportesAccesibilidadRequest
    {
        [JsonPropertyName("tipo")]
        public int? tipo { get; set; }
        [JsonPropertyName("skip")]
        public int? skip { get; set; }
        [JsonPropertyName("take")]
        public int? take { get; set; }

        [JsonPropertyName("paginator")]
        public CoberturaPaginatorHelper paginator { get; set; }
        [JsonPropertyName("sortBy")]
        public string sortBy { get; set; }
        [JsonPropertyName("sortOrder")]
        public string sortOrder { get; set; }


        [JsonPropertyName("fmunicipios")]
        public string[] fmunicipios { get; set; }
        [JsonPropertyName("fcategorias")]
        public string[] fcategorias { get; set; }
        [JsonPropertyName("fcalidades")]
        public string[] fcalidades { get; set; }
    }
}
