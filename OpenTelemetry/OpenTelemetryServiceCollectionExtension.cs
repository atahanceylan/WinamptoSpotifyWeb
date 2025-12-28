using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetryLib;
using OpenTelemetryLib.Metrics;


namespace OpenTelemetry
{
    public static class OpenTelemetryServiceCollectionExtension
    {
        public static IServiceCollection AddOpenTelemetry(this IServiceCollection services, ILogger logger)
        {
            ArgumentNullException.ThrowIfNull(services);
          
            logger.LogInformation("OpenTelemetry is enabled. Configuring OpenTelemetry MeterProvider...");
            
            var endpoint = new Uri(
              (Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT") != null ? Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT")! :  "http://localhost:4318"));

            var meterProviderBuilder = Sdk.CreateMeterProviderBuilder()
              .SetResourceBuilder(
                ResourceBuilder.CreateDefault().AddService("winamptospotifyweb", serviceVersion: "1.0.0"))
              .AddMeter(WinamptoSpotifyMetricsManager.MeterName)
              .AddOtlpExporter((options, metricReader) =>
              {
                  options.Protocol = OtlpExportProtocol.Grpc; // 4317 as the grpc port.
                  options.ExportProcessorType = ExportProcessorType.Batch;
                  options.Endpoint = endpoint;
                  metricReader.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 60000; // 1 minute
                  metricReader.PeriodicExportingMetricReaderOptions.ExportTimeoutMilliseconds = 30000; // half a minute
              })
              .AddOtlpExporter((exporterOptions, metricReaderOptions) =>
              {
                  exporterOptions.Endpoint = new Uri("http://localhost:9090/api/v1/otlp/v1/metrics");
                  exporterOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
                  metricReaderOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 1000;
              });

            meterProviderBuilder.AddConsoleExporter((options, metricReader) =>
            {
                metricReader.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 10000;
            });

            //meterProviderBuilder.AddProcessInstrumentation().AddPrometheusHttpListener();

            MeterProvider? buildedMeterProvider = null;
            try
            {
                buildedMeterProvider = meterProviderBuilder.Build();
                logger.LogInformation("OpenTelemetry MeterProvider configured successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to build OpenTelemetry MeterProvider. Falling back to NoopDmmMetricsManager.");
                services.AddSingleton<IWinamptoSpotifyMetricsManager>(_ => new NoopWinamptoSpotifyMetricsManager(logger));
                return services;
            }

            services.AddSingleton(buildedMeterProvider);
            services.AddSingleton<IWinampToSpotifyWebMetrics, SpotifyServiceMetrics>();
            services.AddSingleton<IWinamptoSpotifyMetricsManager, WinamptoSpotifyMetricsManager>();
            services.AddHostedService<WinamptoSpotifyMetricsHostedService>();
            return services;
        }
    }
}
