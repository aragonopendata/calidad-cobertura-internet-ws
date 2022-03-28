using Messaging.Logic;
using Messaging.RabbitMQ;
using Microsoft.Extensions.Options;
using System;
using ws_cobertura.httpapi.Model.Configuration;

namespace ws_cobertura.RabbitMQ
{
    public class CoberturaRetryRule : BaseRetryRule
    {
        public CoberturaRetryRule() : base()
        {
        }

        public CoberturaRetryRule(String code, long delayMillis, int exponentialBackoffFactor, int maxRetries) : base(code, delayMillis, exponentialBackoffFactor, maxRetries)
        {
            this.Code = code;
            this.DelayMillis = delayMillis;
            this.ExponentialBackoffFactor = exponentialBackoffFactor;
            this.MaxRetries = maxRetries;
        }

        public override string DefaultErrorCode()
        {
            return DEFAULT_ERROR_CODE;
        }

        public override string DefaultSuccessCode()
        {
            return DEFAULT_SUCCESS_CODE;
        }

        public readonly static String DEFAULT_ERROR_CODE = "DEFAULT_ERROR";

        public readonly static String DEFAULT_SUCCESS_CODE = "OK";

    }
}

