using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Serilog;
using Serilog.Formatting.Json;
using Serilog.Sinks.Elasticsearch;
using Microsoft.Extensions.Logging.ApplicationInsights;

namespace B.Slipalison.ServiceBusMasstransit
{

    public class Program
    {

        public static async Task Main(string[] args) => await CreateHostBuilder(args).Build().RunAsync();



        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {

                var connectionStringPost = "User ID=bitstamp;Password=HZJqcT4It3liQC4O1H1p3cGUwhlX453U;Host=dpg-chn0kvm4dad21k1d9vn0-a.oregon-postgres.render.com;Port=5432;Database=bitstamp;Pooling=true;"; //Max Pool Size=100;";


              //  services.AddApplicationInsightsTelemetry();

                services.AddDbContext<OutContext>(x =>
                {
                   
                    x.UseNpgsql(connectionStringPost, options =>
                    {
                        options.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                        options.MigrationsHistoryTable($"__{nameof(OutContext)}");

                        options.EnableRetryOnFailure(5);
                        options.MinBatchSize(1);
                    });
                }, ServiceLifetime.Transient);

                services.AddAzBusMasstransit();



            })
            .UseSerilog((context, configuration) =>
            {
                configuration
                    .MinimumLevel.Verbose()
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .WriteTo.ApplicationInsights("InstrumentationKey=40ffb7b2-c3ca-4f39-8727-c844a4a9a692;IngestionEndpoint=https://brazilsouth-1.in.applicationinsights.azure.com/;LiveEndpoint=https://brazilsouth.livediagnostics.monitor.azure.com/", TelemetryConverter.Traces)
                    .WriteTo.Debug()
                    .WriteTo.Console(new JsonFormatter())
                    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions()
                    {
                        AutoRegisterTemplate = true,
                        AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                        IndexFormat = "webapi-{0:yyyy.MM}"
                    })
                    .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!)
                    .ReadFrom.Configuration(context.Configuration);
            });


    }
}