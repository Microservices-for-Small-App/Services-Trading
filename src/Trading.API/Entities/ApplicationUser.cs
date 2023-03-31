using CommonLibrary.Entities;

namespace Trading.API.Entities;

public class ApplicationUser : IEntity
{
    public Guid Id { get; set; }

    public decimal Gil { get; set; }
}
