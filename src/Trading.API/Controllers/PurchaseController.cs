using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Trading.API.Contracts;
using Trading.API.Dtos;
using Trading.API.StateMachines;

namespace Trading.API.Controllers;

[Route("api/purchase")]
[ApiController]
[Authorize]
public class PurchaseController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IRequestClient<GetPurchaseState> _purchaseClient;

    public PurchaseController(IPublishEndpoint publishEndpoint, IRequestClient<GetPurchaseState> purchaseClient)
    {
        _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));

        _purchaseClient = purchaseClient ?? throw new ArgumentNullException(nameof(purchaseClient));
    }

    [HttpGet("status/{correlationId}")]
    public async Task<ActionResult<PurchaseDto>> GetStatusAsync(Guid correlationId)
    {
        var response = await _purchaseClient.GetResponse<PurchaseState>(new GetPurchaseState(correlationId));

        var purchaseState = response.Message;

        var purchase = new PurchaseDto(purchaseState.UserId, purchaseState.ItemId, purchaseState.PurchaseTotal,
            purchaseState.Quantity, purchaseState.CurrentState!, purchaseState.ErrorMessage!, purchaseState.Received, purchaseState.LastUpdated);

        return Ok(purchase);
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync(SubmitPurchaseDto purchase)
    {
        var userId = User.FindFirstValue("sub");
        var correlationId = Guid.NewGuid();

        var message = new PurchaseRequested(Guid.Parse(userId!), purchase.ItemId!.Value, purchase.Quantity, correlationId);

        await _publishEndpoint.Publish(message);

        await Task.Delay(TimeSpan.FromSeconds(5));

        return AcceptedAtAction(nameof(GetStatusAsync), new { correlationId }, new { correlationId });
    }

}
