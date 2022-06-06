
using Messaging.ElasticSearch;
using System;
using ws_cobertura.RabbitMQ;

namespace ws_cobertura.ElasticSearch
{
    public class CoberturaPaginatorHelper
    {
        public string lastId { get; set; }
        public string lastSort { get; set; }
        public string goTo { get; set; }

        public CoberturaPaginatorHelper()
        {
        }
    }
}
