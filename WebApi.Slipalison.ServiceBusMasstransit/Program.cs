using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using WebApi.Slipalison.ServiceBusMasstransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var services = builder.Services;







//  services.AddApplicationInsightsTelemetry();

services.AddDbContext<OutContext>(x =>
{
    var connectionStringPost = "User ID=bitstamp;Password=HZJqcT4It3liQC4O1H1p3cGUwhlX453U;Host=dpg-chn0kvm4dad21k1d9vn0-a.oregon-postgres.render.com;Port=5432;Database=bitstamp;Pooling=true;"; //Max Pool Size=100;";
    x.UseNpgsql(connectionStringPost, options =>
    {
        options.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
        options.MigrationsHistoryTable($"__{nameof(OutContext)}");

        options.EnableRetryOnFailure(5);
        options.MinBatchSize(1);
    });
}, ServiceLifetime.Transient);



services.AddMassTransit(x =>
{

    x.AddEntityFrameworkOutbox<OutContext>(o =>
    {
        //o.QueryDelay = TimeSpan.FromSeconds(1);
        //o.DuplicateDetectionWindow = TimeSpan.FromSeconds(30);
        o.UsePostgres();
        o.UseBusOutbox();

    });

    x.SetKebabCaseEndpointNameFormatter();

    x.SetInMemorySagaRepositoryProvider();

    var entry = Assembly.GetEntryAssembly();

    //x.AddConsumers(entry);
    x.AddSagaStateMachines(entry);
    x.AddSagas(entry);


    x.UsingAzureServiceBus((context, cfg) =>
    {
        var connectionString = "Endpoint=sb://testebus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=dM4zOESViqVH6k/mPvp8zWtGR8tGaK9BW+ASbMrRJCw=";
        var queueName = "submitorder-queue";

        cfg.Host(connectionString, c => c.TransportType = Azure.Messaging.ServiceBus.ServiceBusTransportType.AmqpWebSockets);

        cfg.Message<Hello>(x =>
        {
            x.SetEntityName("topico-novo");

        });

        // TODO Partition
        cfg.Send<Hello>(x =>
        {
            x.UsePartitionKeyFormatter(p => p.Message.MyProperty);
            //   x.UseCorrelationId()
            x.UseSerializer("application/json");


        });


        // TODO Correlation
        cfg.Publish<Hello>(p =>
        {
            p.UserMetadata = "MetaDadosSigiloso";

        });

        cfg.ConfigureEndpoints(context);

    });

});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
