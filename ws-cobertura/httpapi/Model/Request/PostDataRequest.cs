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
    }
}
