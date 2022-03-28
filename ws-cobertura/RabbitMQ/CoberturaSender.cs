using Messaging.RabbitMQ;
using Microsoft.Extensions.Options;
using System;
using ws_cobertura.httpapi.Model.Configuration;

namespace ws_cobertura.RabbitMQ
{
    public class CoberturaSender : RabbitMQSender<CoberturaMessage>
    {
        protected override String SenderName { get { return "Cobertura"; } }
        public CoberturaSender(IOptions<CoberturaRabbitMQConfiguration> rabbitMqOptions) : base(rabbitMqOptions)
        {

        }
    }
}
