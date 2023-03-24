using Catalog.Contracts;
using CommonLibrary.Interfaces;
using MassTransit;
using Trading.API.Entities;

namespace Trading.API.Consumers;

public class CatalogItemDeletedConsumer : IConsumer<CatalogItemDeleted>
{
    private readonly IRepository<CatalogItem> _repository;

    public CatalogItemDeletedConsumer(IRepository<CatalogItem> repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task Consume(ConsumeContext<CatalogItemDeleted> context)
    {
        var message = context.Message;

        var item = await _repository.GetAsync(message.ItemId);

        if (item is null)
        {
            return;
        }

        await _repository.RemoveAsync(message.ItemId);
    }
}