using Catalog.Contracts;
using CommonLibrary.Interfaces;
using MassTransit;
using Trading.API.Entities;

namespace Trading.API.Consumers;

public class CatalogItemUpdatedConsumer : IConsumer<CatalogItemUpdated>
{
    private readonly IRepository<CatalogItem> _repository;

    public CatalogItemUpdatedConsumer(IRepository<CatalogItem> repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task Consume(ConsumeContext<CatalogItemUpdated> context)
    {
        var message = context.Message;

        var item = await _repository.GetAsync(message.ItemId);

        if (item is null)
        {
            item = new CatalogItem
            {
                Id = message.ItemId,
                Name = message.Name,
                Description = message.Description,
                Price = message.Price
            };

            await _repository.CreateAsync(item);
        }
        else
        {
            item.Name = message.Name;
            item.Description = message.Description;
            item.Price = message.Price;

            await _repository.UpdateAsync(item);
        }
    }
}
