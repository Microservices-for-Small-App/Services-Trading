namespace Trading.API.Contracts;

public record PurchaseRequested(
        Guid UserId,
        Guid ItemId,
        int Quantity,
        Guid CorrelationId
    );
