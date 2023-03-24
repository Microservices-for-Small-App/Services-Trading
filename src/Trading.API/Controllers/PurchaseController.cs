using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Trading.API.Contracts;
using Trading.API.Dtos;

namespace Trading.API.Controllers;

[Route("api/purchase")]
[ApiController]
[Authorize]
public class PurchaseController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;

    public PurchaseController(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync(SubmitPurchaseDto purchase)
    {
        var userId = User.FindFirstValue("sub");
        var correlationId = Guid.NewGuid();

        var message = new PurchaseRequested(Guid.Parse(userId!), purchase.ItemId!.Value, purchase.Quantity, correlationId);

        await _publishEndpoint.Publish(message);

        return Accepted();
    }
}
