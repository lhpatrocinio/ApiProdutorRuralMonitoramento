using Prometheus;
using System;
using System.Collections.Generic;

namespace ProdutorRuralMonitoramento.Infrastructure.Monitoring
{
    /// <summary>
    /// Componente para contabilizar métricas customizadas do aplicativo
    /// </summary>
    public class MetricsCollector
    {
        // Contador de requisições com sucesso por endpoint
        private readonly Counter _successCounter;

        // Contador de falhas por endpoint e tipo de erro
        private readonly Counter _errorCounter;

        // Contador específico para códigos de status HTTP
        private readonly Counter _httpStatusCounter;

        // Medidor de tempo de resposta por endpoint
        private readonly Histogram _responseTimeHistogram;

        // Medidor de tamanho de resposta em bytes
        private readonly Histogram _responseSizeHistogram;

        // Coleção para métricas de negócios
        private readonly Dictionary<string, Counter> _businessMetrics = new();

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="MetricsCollector"/>
        /// </summary>
        public MetricsCollector()
        {
            // Inicializa o contador de requisições com sucesso
            _successCounter = Metrics.CreateCounter(
                "app_request_success_total",
                "Total number of successful requests",
                new CounterConfiguration
                {
                    LabelNames = new[] { "endpoint" }
                });

            // Inicializa o contador de erros
            _errorCounter = Metrics.CreateCounter(
                "app_request_errors_total",
                "Total number of request errors",
                new CounterConfiguration
                {
                    LabelNames = new[] { "endpoint", "error_type" }
                });

            // Inicializa o contador de códigos de status HTTP
            _httpStatusCounter = Metrics.CreateCounter(
                "app_http_status_total",
                "Total de requisições por código de status HTTP",
                new CounterConfiguration
                {
                    LabelNames = new[] { "endpoint", "method", "status_code", "status_class" }
                });

            // Inicializa o histograma de tempo de resposta
            _responseTimeHistogram = Metrics.CreateHistogram(
                "app_request_duration_seconds",
                "Request duration in seconds",
                new HistogramConfiguration
                {
                    LabelNames = new[] { "endpoint" },
                    Buckets = new[] { 0.01, 0.05, 0.1, 0.5, 1, 2, 5, 10 } // Buckets em segundos
                });

            // Inicializa o histograma de tamanho de resposta
            _responseSizeHistogram = Metrics.CreateHistogram(
                "app_response_size_bytes",
                "Response size in bytes",
                new HistogramConfiguration
                {
                    LabelNames = new[] { "endpoint" },
                    Buckets = new[] { 100.0, 1000.0, 10000.0, 100000.0, 1000000.0 } // Buckets em bytes
                });
        }

        /// <summary>
        /// Registra uma requisição com sucesso
        /// </summary>
        /// <param name="endpoint">O endpoint da requisição</param>
        public void RecordSuccess(string endpoint)
        {
            _successCounter.WithLabels(endpoint).Inc();
        }

        /// <summary>
        /// Registra uma falha na requisição
        /// </summary>
        /// <param name="endpoint">O endpoint da requisição</param>
        /// <param name="errorType">O tipo de erro</param>
        public void RecordError(string endpoint, string errorType)
        {
            _errorCounter.WithLabels(endpoint, errorType).Inc();
        }

        /// <summary>
        /// Registra o tempo de resposta de uma requisição
        /// </summary>
        /// <param name="endpoint">O endpoint da requisição</param>
        /// <param name="seconds">O tempo em segundos</param>
        public void RecordResponseTime(string endpoint, double seconds)
        {
            _responseTimeHistogram.WithLabels(endpoint).Observe(seconds);
        }

        /// <summary>
        /// Registra o tamanho da resposta
        /// </summary>
        /// <param name="endpoint">O endpoint da requisição</param>
        /// <param name="bytes">O tamanho em bytes</param>
        public void RecordResponseSize(string endpoint, double bytes)
        {
            _responseSizeHistogram.WithLabels(endpoint).Observe(bytes);
        }

        /// <summary>
        /// Registra um código de status HTTP
        /// </summary>
        /// <param name="endpoint">O endpoint da requisição</param>
        /// <param name="method">O método HTTP (GET, POST, etc.)</param>
        /// <param name="statusCode">O código de status HTTP</param>
        public void RecordHttpStatus(string endpoint, string method, int statusCode)
        {
            // Determina a classe do status HTTP (1xx, 2xx, 3xx, 4xx, 5xx)
            string statusClass = $"{statusCode / 100}xx";

            _httpStatusCounter
                .WithLabels(endpoint, method, statusCode.ToString(), statusClass)
                .Inc();
        }

        /// <summary>
        /// Registra uma métrica de negócio personalizada
        /// </summary>
        /// <param name="name">O nome da métrica</param>
        /// <param name="description">A descrição da métrica</param>
        /// <param name="labelNames">Os nomes dos rótulos</param>
        /// <param name="labelValues">Os valores dos rótulos</param>
        public void RecordBusinessMetric(string name, string description, string[] labelNames, string[] labelValues)
        {
            if (!_businessMetrics.TryGetValue(name, out var counter))
            {
                counter = Metrics.CreateCounter(
                    $"app_business_{name}_total",
                    description,
                    new CounterConfiguration
                    {
                        LabelNames = labelNames
                    });

                _businessMetrics[name] = counter;
            }

            counter.WithLabels(labelValues).Inc();
        }

        /// <summary>
        /// Mede o tempo de execução de uma ação
        /// </summary>
        /// <param name="endpoint">O endpoint da requisição</param>
        /// <param name="action">A ação a ser executada</param>
        public void MeasureExecutionTime(string endpoint, Action action)
        {
            var timer = Metrics.CreateHistogram(
                "app_execution_time_seconds",
                "Execution time of operations in seconds",
                new HistogramConfiguration
                {
                    LabelNames = new[] { "operation" }
                }).WithLabels(endpoint).NewTimer();

            try
            {
                action();
            }
            finally
            {
                timer.Dispose();
            }
        }

        /// <summary>
        /// Mede o tempo de execução de uma função
        /// </summary>
        /// <typeparam name="T">O tipo de retorno da função</typeparam>
        /// <param name="endpoint">O endpoint da requisição</param>
        /// <param name="func">A função a ser executada</param>
        /// <returns>O resultado da função</returns>
        public T MeasureExecutionTime<T>(string endpoint, Func<T> func)
        {
            var timer = Metrics.CreateHistogram(
                "app_execution_time_seconds",
                "Execution time of operations in seconds",
                new HistogramConfiguration
                {
                    LabelNames = new[] { "operation" }
                }).WithLabels(endpoint).NewTimer();

            try
            {
                return func();
            }
            finally
            {
                timer.Dispose();
            }
        }
    }
}