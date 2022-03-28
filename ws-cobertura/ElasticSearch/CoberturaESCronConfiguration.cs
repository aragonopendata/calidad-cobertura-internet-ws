using Messaging.RabbitMQ;

namespace ws_cobertura.RabbitMQ
{
    public class CoberturaESCronConfiguration
    {
        public string CronExpression { get; set; } = string.Empty;
        public bool HasChanged(CoberturaESCronConfiguration oldConfiguration)
        {
            if (oldConfiguration == null) return true;

            bool hasChanged = HasChanged(oldConfiguration);
            hasChanged |= (oldConfiguration.CronExpression != this.CronExpression);

            return hasChanged;
        }
    }
}
