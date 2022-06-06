
using Messaging.ElasticSearch;
using System;
using System.Text.Json.Serialization;
using ws_cobertura.RabbitMQ;

namespace ws_cobertura.ElasticSearch
{
    [Nest.ElasticsearchType(IdProperty = nameof(IndexIdProperty))]
    [Serializable]
    public class CoberturaReportAgrupadoCuadricula : IndexedESClass
    {
        [Nest.Text(Ignore = true)]
        public override string IndexIdProperty { get { return reportid; } set { reportid = value; } }

        /////////

        public String reportid { get; set; }
        public DateTimeOffset insertion_time { get; set; }


        /////////

        public String clave { get; set; }
        public double coodenadax { get; set; }
        public double coodenaday { get; set; }
        public Decimal? mediaVelocidadSubida { get; set; }
        public Decimal? mediaVelocidadBajada { get; set; }
        public Decimal? mediaLatencia { get; set; }
        public Decimal? mediaValorIntensidadSenial { get; set; }
        public int mediaRangoIntensidadSenial { get; set; }
        public int mediaRangoVelocidadSubida { get; set; }
        public int mediaRangoVelocidadBajada { get; set; }
        public int mediaRangoLatencia { get; set; }
        public DateTimeOffset minTimestamp { get; set; }
        public DateTimeOffset maxTimestamp { get; set; }
        public long cantidadMedidas { get; set; }

        public CoberturaAgrupadoCuaLocation location { get; set; }

        public string iconoVelocidadBajada { get; set; }
        public string iconoVelocidadSubida { get; set; }
        public string iconoLatencia { get; set; }
        public string iconoIntensidad { get; set; }


        // campos para tooltip mapa, llevan doble __ para reemplazarlo en cliente y enviarlos con espacios para la UI
        public string Calidad { get; set; }
        public string Velocidad__Subida { get; set; }
        public string Velocidad__Bajada { get; set; }
        public string Latencia { get; set; }
        public string Tipo__Red { get; set; }
        public string Cantidad__Mediciones { get; set; }
        public string Municipios { get; set; }

        public string Categoria { get; set; }
        public string Intensidad { get; set; }
        public string Datos__Desde { get; set; } //Fecha__Desde
        public string Datos__Hasta { get; set; } //Fecha__Hasta


        /////////

        public CoberturaReportAgrupadoMessage message = null; // para mantener misma estructura que reportes desagrupados, permite habilitar filtro general

        public CoberturaReportAgrupadoCuadricula()
        {
            this.insertion_time = DateTimeOffset.Now;
            this.message = new CoberturaReportAgrupadoMessage();
        }

    }
    public class CoberturaAgrupadoCuaLocation
    {
        public double lat;
        public double lon;
    }

    public class CoberturaReportAgrupadoMessage {
        public string[] arrMunicipios { get; set; }

        public string[] arrCategorias { get; set; }
        public string[] arrTipoRed { get; set; }

        public string tipoRed { get; set; }

        public string textoRangoVelocidadBajada { get; set; }
    }
}
