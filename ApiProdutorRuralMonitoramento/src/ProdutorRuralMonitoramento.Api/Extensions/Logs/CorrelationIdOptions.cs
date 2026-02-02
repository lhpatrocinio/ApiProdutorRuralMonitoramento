using System;

namespace ProdutorRuralMonitoramento.Api.Extensions.Logs
{
    public class CorrelationIdOptions
    {
        private const string DefaultHeader = "X-Correlation-ID";
        public static string GuidCorrelationIdBuilder() => Guid.NewGuid().ToString();
        public static string ShortGuidCorrelationIdBuilder() => Guid.NewGuid().ToString("N");
        public string Header { get; set; } = DefaultHeader;
        public bool IncludeInResponse { get; set; } = true;
        public Func<string> CorrelationIdBuilder { get; set; } = GuidCorrelationIdBuilder;
    }
}
