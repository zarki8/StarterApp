using Microsoft.EntityFrameworkCore;
using StarterApp.Database.Data;
using StarterApp.Database.Models;

namespace StarterApp.Repositories;

public class ItemRepository : IItemRepository
{
    private readonly AppDbContext _context;

    public ItemRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<Item>> GetAllAsync()
    {
        return _context.Items
            .Include(i => i.Owner)
            .Where(i => i.IsActive)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public Task<Item?> GetByIdAsync(int id)
    {
        return _context.Items
            .Include(i => i.Owner)
            .FirstOrDefaultAsync(i => i.Id == id && i.IsActive);
    }

    public async Task<Item> AddAsync(Item item)
    {
        _context.Items.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task UpdateAsync(Item item)
    {
        _context.Items.Update(item);
        await _context.SaveChangesAsync();
    }
}
