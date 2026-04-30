using StarterApp.Database.Models;

namespace StarterApp.Repositories;

public interface IItemRepository : IRepository<Item>
{
    Task<Item> AddAsync(Item item);
    Task UpdateAsync(Item item);
}
