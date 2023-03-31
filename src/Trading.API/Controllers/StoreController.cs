using CommonLibrary.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Trading.API.Dtos;
using Trading.API.Entities;

namespace Trading.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class StoreController : ControllerBase
{
    private readonly IRepository<CatalogItem> _catalogRepository;
    private readonly IRepository<ApplicationUser> _usersRepository;
    private readonly IRepository<InventoryItem> _inventoryRepository;

    public StoreController(IRepository<CatalogItem> catalogRepository, IRepository<ApplicationUser> usersRepository, IRepository<InventoryItem> inventoryRepository)
    {
        _catalogRepository = catalogRepository ?? throw new ArgumentNullException(nameof(catalogRepository));

        _usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));

        _inventoryRepository = inventoryRepository ?? throw new ArgumentNullException(nameof(inventoryRepository));
    }

    [HttpGet]
    public async Task<ActionResult<StoreDto>> GetAsync()
    {
        string userId = User.FindFirstValue("sub")!;

        var catalogItems = await _catalogRepository.GetAllAsync();

        var inventoryItems = await _inventoryRepository.GetAllAsync(item => item.UserId == Guid.Parse(userId));

        var user = await _usersRepository.GetAsync(Guid.Parse(userId));

        var storeDto = new StoreDto(
            catalogItems.Select(catalogItem =>
                new StoreItemDto(catalogItem.Id, catalogItem.Name!, catalogItem.Description!, catalogItem.Price,
                    inventoryItems.FirstOrDefault(inventoryItem => inventoryItem.CatalogItemId == catalogItem.Id)?.Quantity ?? 0)),
            user?.Gil ?? 0
        );

        return Ok(storeDto);
    }

}
