using CommonLibrary.Identity;
using CommonLibrary.MassTransit;
using CommonLibrary.MongoDB.Extensions;
using CommonLibrary.Settings;
using MassTransit;
using Trading.API.StateMachines;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMongo()
                .AddJwtBearerAuthentication();

AddMassTransit(builder.Services);

builder.Services.AddControllers();
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
        configure.UseRabbitMqService();

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
}