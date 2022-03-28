using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Threading;
using ws_cobertura.ElasticSearch;
using Microsoft.Extensions.Hosting;
using NCrontab;
using Microsoft.Extensions.Options;

namespace ws_cobertura.RabbitMQ
{
    public class CoberturaESServicioAgrupado : BackgroundService
    {

        private readonly ILogger<CoberturaESServicioAgrupado> _logger;
        private readonly CoberturaReportRepository _coberturaReportRepository;
        private readonly CoberturaESCronConfiguration _coberturaESCronConfiguration;

        private CrontabSchedule _schedule;
        private DateTime _nextRun;

        private string _sCronExpression = string.Empty;

        public CoberturaESServicioAgrupado(ILogger<CoberturaESServicioAgrupado> logger, CoberturaReportRepository coberturaReportRepository, IOptions<CoberturaESCronConfiguration> coberturaESCronConfiguration)
        {
            this._coberturaESCronConfiguration = coberturaESCronConfiguration.Value;
            this._sCronExpression = _coberturaESCronConfiguration.CronExpression; //"*/30 * * * * *";
            this._logger = logger;
            this._coberturaReportRepository = coberturaReportRepository;
            this._schedule = CrontabSchedule.Parse(_sCronExpression, new CrontabSchedule.ParseOptions { IncludingSeconds = true });
            this._nextRun = _schedule.GetNextOccurrence(DateTime.Now);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            do
            {
                var now = DateTime.Now;
                //var nextrun = _schedule.GetNextOccurrence(now);

                if (now > _nextRun)
                {
                    await Process();
                    _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
                }

                await Task.Delay(5000, stoppingToken); //5 seconds delay
            }
            while (!stoppingToken.IsCancellationRequested);
        }

        private async Task<bool> Process()
        {
            return await _coberturaReportRepository.GroupCoberturaReports();
        }

    }
}
