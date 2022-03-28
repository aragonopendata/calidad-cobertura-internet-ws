
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
