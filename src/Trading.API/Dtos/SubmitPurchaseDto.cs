using System.ComponentModel.DataAnnotations;

namespace Trading.API.Dtos;

public record SubmitPurchaseDto([Required] Guid? ItemId, [Range(1, 100)] int Quantity);
