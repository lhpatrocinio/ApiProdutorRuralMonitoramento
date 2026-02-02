namespace ProdutorRuralMonitoramento.Api.Extensions.Logs.Models
{
    public class LogString
    {
        private LogString(string value) => StringValue = value;

        public static LogString GetLog(string value) => new LogString(value);
        public string StringValue { get; set; }
    }
}
