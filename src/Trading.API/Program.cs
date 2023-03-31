using CommonLibrary.Identity;
using CommonLibrary.MassTransit;
using CommonLibrary.MongoDB.Extensions;
using CommonLibrary.Settings;
using GreenPipes;
using Identity.Contracts;
using Inventory.Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using System.Reflection;
using System.Text.Json.Serialization;
using Trading.API.Entities;
using Trading.API.Exceptions;
using Trading.API.Settings;
using Trading.API.SignalRHubs;
using Trading.API.StateMachines;

var builder = WebApplication.CreateBuilder(args);

const string AllowedOriginSetting = "AllowedOrigin";

// Add services to the container.
_ = builder.Services.AddSingleton(builder.Configuration?.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>()!);

_ = builder.Services.AddSingleton(builder.Configuration?.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>()!);

_ = builder.Services.AddSingleton(builder.Configuration?.GetSection(nameof(RabbitMQSettings)).Get<RabbitMQSettings>()!);

builder.Services.AddMongo()
                .AddMongoRepository<CatalogItem>("catalogitems")
                .AddMongoRepository<InventoryItem>("inventoryitems")
                .AddMongoRepository<ApplicationUser>("users")
                .AddJwtBearerAuthentication();

AddMassTransit(builder.Services);

builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
}).AddJsonOptions(options => options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IUserIdProvider, UserIdProvider>()
                    .AddSingleton<MessageHub>()
                    .AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseCors(options =>
    {
        _ = options.WithOrigins(builder.Configuration![AllowedOriginSetting]!)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHub<MessageHub>("/messageHub");

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

        _ = configure.AddSagaStateMachine<PurchaseStateMachine, PurchaseState>(sagaConfigurator =>
        {
            sagaConfigurator.UseInMemoryOutbox();
        })
         .MongoDbRepository(r =>
         {
             var serviceSettings = builder.Configuration!.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();

             var mongoSettings = builder.Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();

             r.Connection = mongoSettings!.ConnectionString;
             r.DatabaseName = serviceSettings!.ServiceName;
         });
    });

    var queueSettings = builder.Configuration!.GetSection(nameof(QueueSettings)).Get<QueueSettings>();

    EndpointConvention.Map<GrantItems>(new Uri(queueSettings?.GrantItemsQueueAddress!));

    EndpointConvention.Map<DebitGil>(new Uri(queueSettings?.DebitGilQueueAddress!));

    EndpointConvention.Map<SubtractItems>(new Uri(queueSettings?.SubtractItemsQueueAddress!));

    _ = services.AddMassTransitHostedService();

    _ = services.AddGenericRequestClient();
}