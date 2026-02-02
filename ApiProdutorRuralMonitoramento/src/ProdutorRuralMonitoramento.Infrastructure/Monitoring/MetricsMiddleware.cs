using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ProdutorRuralMonitoramento.Infrastructure.Monitoring
{
    /// <summary>
    /// Middleware para coletar métricas de requisições HTTP
    /// </summary>
    public class MetricsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly MetricsCollector _metrics;

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="MetricsMiddleware"/>
        /// </summary>
        /// <param name="next">O próximo middleware no pipeline</param>
        /// <param name="metrics">O coletor de métricas</param>
        public MetricsMiddleware(RequestDelegate next, MetricsCollector metrics)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        }

        /// <summary>
        /// Invoca o middleware
        /// </summary>
        /// <param name="context">O contexto HTTP</param>
        /// <returns>Uma tarefa que representa a operação assíncrona</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.Request.Path.Value ?? "unknown";
            var stopwatch = Stopwatch.StartNew();

            // Captura o body da resposta para medir o tamanho
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                await _next(context);

                stopwatch.Stop();
                var statusCode = context.Response.StatusCode;

                // Registra o código de status HTTP
                _metrics.RecordHttpStatus(endpoint, context.Request.Method, statusCode);

                // Registra métricas de sucesso (2xx)
                if (statusCode >= 200 && statusCode < 300)
                {
                    _metrics.RecordSuccess(endpoint);
                }
                else
                {
                    _metrics.RecordError(endpoint, $"HTTP_{statusCode}");
                }

                // Registra o tempo de resposta
                _metrics.RecordResponseTime(endpoint, stopwatch.Elapsed.TotalSeconds);

                // Registra o tamanho da resposta
                _metrics.RecordResponseSize(endpoint, responseBody.Length);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                // Registra código de status 500 para exceções não tratadas
                _metrics.RecordHttpStatus(endpoint, context.Request.Method, 500);
                _metrics.RecordError(endpoint, ex.GetType().Name);
                _metrics.RecordResponseTime(endpoint, stopwatch.Elapsed.TotalSeconds);
                throw;
            }
            finally
            {
                // Copia a resposta capturada para o stream original
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }
    }
}
