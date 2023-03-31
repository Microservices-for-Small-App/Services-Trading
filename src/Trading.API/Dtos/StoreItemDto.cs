namespace Trading.API.Dtos;

public record StoreItemDto(Guid Id, string Name, string Description, decimal Price, int OwnedQuantity);
