using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var app = builder.AddProject<WinampToSpotifyWeb>("winamptospotifyweb");

// Prometheus container scraping the app
var prometheus = builder.AddContainer("prometheus", "prom/prometheus")
    .WithBindMount("./prometheus/prometheus.yml", "/etc/prometheus/prometheus.yml")
    .WithEndpoint(port: 9090, targetPort: 9090)
    .WithArgs("--config.file=/etc/prometheus/prometheus.yml",
              "--web.enable-otlp-receiver");

// Grafana container using Prometheus as data source
var grafana = builder.AddContainer("grafana", "grafana/grafana")
    .WithVolume("grafana-storage", "/var/lib/grafana")  // Persists dashboards, users, DB
    .WithVolume("grafana-provisioning", "/etc/grafana/provisioning", isReadOnly: true)  // Optional: Provisioning YAML/JSON
                     .WithEndpoint(port: 3000, targetPort: 3000);

builder.Build().Run();
