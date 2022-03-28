
using Messaging.ElasticSearch;
using System;
using ws_cobertura.RabbitMQ;

namespace ws_cobertura.ElasticSearch
{
    [Nest.ElasticsearchType(IdProperty = nameof(IndexIdProperty))]
    [Serializable]
    public class CoberturaCronUltimaEjecucion : IndexedESClass
    {
        [Nest.Text(Ignore = true)]
        public override string IndexIdProperty { get { return reportid; } set { reportid = value; } }

        /////////

        public String reportid { get; set; }
        public DateTimeOffset insertion_time { get; set; }
        public DateTimeOffset fecha_ultima_ejecucion { get; set; }
        public string nombre_index { get; set; }

        /////////

        public CoberturaCronUltimaEjecucion()
        {
            this.insertion_time = DateTimeOffset.Now;
        }
    }
}
