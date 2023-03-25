using CommonLibrary.Identity;
using CommonLibrary.MassTransit;
using CommonLibrary.MongoDB.Extensions;
using CommonLibrary.Settings;
using GreenPipes;
using MassTransit;
using System.Reflection;
using System.Text.Json.Serialization;
using Trading.API.Entities;
using Trading.API.Exceptions;
using Trading.API.StateMachines;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMongo()
                .AddMongoRepository<CatalogItem>("catalogitems")
                .AddJwtBearerAuthentication();

AddMassTransit(builder.Services);

builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
}).AddJsonOptions(options => options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

void AddMassTransit(IServiceCollection services)
{
    _ = services.AddMassTransit(configure =>
    {
        configure.UseRabbitMqService(retryConfigurator =>
        {
            retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
            retryConfigurator.Ignore(typeof(UnknownItemException));
        });

        configure.AddConsumers(Assembly.GetEntryAssembly());

        _ = configure.AddSagaStateMachine<PurchaseStateMachine, PurchaseState>()
            .MongoDbRepository(r =>
            {
                var serviceSettings = builder.Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();

                var mongoSettings = builder.Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();

                r.Connection = mongoSettings!.ConnectionString;
                r.DatabaseName = serviceSettings!.ServiceName;
            });
    });

    _ = services.AddMassTransitHostedService();

    _ = services.AddGenericRequestClient();
}