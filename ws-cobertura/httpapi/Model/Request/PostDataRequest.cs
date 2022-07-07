using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using ws_cobertura.ElasticSearch;

namespace ws_cobertura.httpapi.Model.Request
{
    public class PostDataRequest
    {
        [JsonPropertyName("fechaDesde")]
        public string fechaDesde { get; set; } // dd/MM/yyyy HH:mm:ss
        [JsonPropertyName("fechaHasta")]
        public string fechaHasta { get; set; } // dd/MM/yyyy HH:mm:ss
        [JsonPropertyName("municipio")]
        public string municipio { get; set; }
        [JsonPropertyName("ine")]
        public string ine { get; set; }
        [JsonPropertyName("categoria")]
        public string categoria { get; set; }

        [JsonPropertyName("operador")]
        public string operador { get; set; }

        [JsonPropertyName("latitudDesde")]
        public string latitudDesde { get; set; }
        [JsonPropertyName("latitudHasta")]
        public string latitudHasta { get; set; }
        [JsonPropertyName("longitudDesde")]
        public string longitudDesde { get; set; }
        [JsonPropertyName("longitudHasta")]
        public string longitudHasta { get; set; }
    }
}
