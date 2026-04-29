using StarterApp.Database.Models;

namespace StarterApp.Repositories;

public interface IItemRepository
{
    Task<List<Item>> GetAllAsync();
    Task<Item?> GetByIdAsync(int id);
    Task<Item> AddAsync(Item item);
    Task UpdateAsync(Item item);
}
