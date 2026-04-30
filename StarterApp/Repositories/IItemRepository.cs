using StarterApp.Database.Models;
using StarterApp.Models;

namespace StarterApp.Repositories;

public interface IItemRepository : IRepository<Item>
{
    Task<Item> AddAsync(Item item);
    Task UpdateAsync(Item item);
    Task<List<NearbyItem>> GetNearbyAsync(double latitude, double longitude, double radiusKm, string? category = null);
}
