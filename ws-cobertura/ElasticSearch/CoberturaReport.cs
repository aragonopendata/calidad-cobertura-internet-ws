
using Messaging.ElasticSearch;
using System;
using ws_cobertura.RabbitMQ;

namespace ws_cobertura.ElasticSearch
{
    [Nest.ElasticsearchType(IdProperty = nameof(IndexIdProperty))]
    [Serializable]
    public class CoberturaReport : IndexedESClass
    {
        [Nest.Text(Ignore = true)]
        public override string IndexIdProperty { get { return reportid; } set { reportid = value; } }

        /////////

        public String reportid { get; set; }
        public DateTimeOffset insertion_time { get; set; }


        public CoberturaMessage message = null;

        // index autogenerado al insertar en elastic
        public string Id { get; set; }

        //campos necesarios tooltip
        public string Calidad { get; set; }
        public string Categoria { get; set; }
        public string Municipio { get; set; }
        public string Fecha { get; set; }
        public string Intensidad { get; set; }
        public string Latencia { get; set; }
        public string Velocidad__Bajada { get; set; }
        public string Velocidad__Subida { get; set; }

        public string Tipo__Red { get; set; }
        public string Operador { get; set; }
        public string Tipo__Dispositivo { get; set; }
        public string SO__Dispositivo { get; set; }
        /////////

        public CoberturaReport()
        {
            this.insertion_time = DateTimeOffset.Now;

        }

        public CoberturaReport FillFromCoberturaMessage(CoberturaMessage coberturaMessage)
        {

            this.message = coberturaMessage;

            return this;
        }
    }
}
