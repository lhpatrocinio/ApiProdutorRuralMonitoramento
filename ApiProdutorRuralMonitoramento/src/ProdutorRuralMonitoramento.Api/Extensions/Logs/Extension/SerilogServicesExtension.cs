using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Json;
using Serilog.Sinks.Network;
using System.Linq;
using System.Net.Sockets;

namespace ProdutorRuralMonitoramento.Api.Extensions.Logs.Extension
{
    public static class SerilogServicesExtension
    {
        public static WebApplicationBuilder AddSerilogConfiguration(this WebApplicationBuilder builder)
        {
            var serviceName = "Games.Api";
            var servicePort = 7000;

            builder.Host.UseSerilog((context, logger) =>
            {
                // Configuração básica via arquivo
                logger.ReadFrom.Configuration(context.Configuration);
                
                // Configuração de enrichers padronizada
                logger.Enrich.FromLogContext();
                logger.Enrich.WithExceptionDetails();
                logger.Enrich.WithProperty("MachineName", Environment.MachineName);
                logger.Enrich.WithProperty("service_name", serviceName);
                logger.Enrich.WithProperty("microservice", serviceName);
                logger.Enrich.WithProperty("service_port", servicePort);
                
                // Estratégias múltiplas para conectividade ELK
                if (TryConfigureTcpSink(logger, serviceName))
                {
                    // TCP configurado com sucesso
                }
                else if (TryConfigureUdpSink(logger, serviceName))
                {
                    // UDP configurado como fallback
                }
                else
                {
                    Console.WriteLine($"ℹ️ {serviceName} running with Console logging only");
                }
                
                logger.MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning);
                logger.MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning);
                logger.Filter.ByExcluding(c => c.Properties.Any(p => p.Value.ToString().Contains("/ready")));
                logger.Filter.ByExcluding(c => c.Properties.Any(p => p.Value.ToString().Contains("/liveness")));
                logger.Filter.ByExcluding(c => c.Properties.Any(p => p.Value.ToString().Contains("/hc")));
            });

            return builder;
        }

        private static bool TryConfigureTcpSink(LoggerConfiguration logger, string serviceName)
        {
            try
            {
                // Estratégia 1: IP direto do container Logstash
                logger.WriteTo.TCPSink("172.19.0.7", 5044, new CompactJsonFormatter());
                Console.WriteLine($"✅ TCP sink configured successfully for {serviceName} (direct IP: 172.19.0.7)");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ TCP sink (direct IP) failed for {serviceName}: {ex.Message}");
                
                try
                {
                    // Estratégia 2: Fallback para host.docker.internal
                    logger.WriteTo.TCPSink("host.docker.internal", 5044, new CompactJsonFormatter());
                    Console.WriteLine($"✅ TCP sink configured successfully for {serviceName} (host.docker.internal)");
                    return true;
                }
                catch (Exception ex2)
                {
                    Console.WriteLine($"⚠️ TCP sink (host.docker.internal) failed for {serviceName}: {ex2.Message}");
                    return false;
                }
            }
        }

        private static bool TryConfigureUdpSink(LoggerConfiguration logger, string serviceName)
        {
            try
            {
                // Estratégia 3: UDP com IP direto
                logger.WriteTo.Udp("172.19.0.7", 5044, AddressFamily.InterNetwork, new JsonFormatter());
                Console.WriteLine($"✅ UDP sink configured successfully for {serviceName} (direct IP)");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ UDP sink (direct IP) failed for {serviceName}: {ex.Message}");
                
                try
                {
                    // Estratégia 4: UDP com host.docker.internal
                    logger.WriteTo.Udp("host.docker.internal", 5044, AddressFamily.InterNetwork, new JsonFormatter());
                    Console.WriteLine($"✅ UDP sink configured successfully for {serviceName} (host.docker.internal)");
                    return true;
                }
                catch (Exception ex2)
                {
                    Console.WriteLine($"⚠️ UDP sink failed for {serviceName}: {ex2.Message}");
                    return false;
                }
            }
        }
    }
}
