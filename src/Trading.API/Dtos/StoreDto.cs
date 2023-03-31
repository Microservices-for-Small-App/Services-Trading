namespace Trading.API.Dtos;

public record StoreDto(IEnumerable<StoreItemDto> Items, decimal UserGil);