using Catalog.Contracts;
using CommonLibrary.Interfaces;
using MassTransit;
using Trading.API.Entities;

namespace Trading.API.Consumers;

public class CatalogItemCreatedConsumer : IConsumer<CatalogItemCreated>
{
    private readonly IRepository<CatalogItem> _repository;

    public CatalogItemCreatedConsumer(IRepository<CatalogItem> repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task Consume(ConsumeContext<CatalogItemCreated> context)
    {
        var message = context.Message;

        var item = await _repository.GetAsync(message.ItemId);

        if (item is not null)
        {
            return;
        }

        item = new CatalogItem
        {
            Id = message.ItemId,
            Name = message.Name,
            Description = message.Description,
            Price = message.Price
        };

        await _repository.CreateAsync(item);
    }
}
