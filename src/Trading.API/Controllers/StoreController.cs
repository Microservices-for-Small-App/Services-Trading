using CommonLibrary.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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


}
