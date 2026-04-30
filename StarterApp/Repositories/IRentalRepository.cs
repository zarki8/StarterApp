using StarterApp.Models;

namespace StarterApp.Repositories;

public interface IRentalRepository
{
    Task<Rental> RequestRentalAsync(int itemId, DateTime startDate, DateTime endDate);

    Task<List<Rental>> GetIncomingAsync(string? status = null);

    Task<List<Rental>> GetOutgoingAsync(string? status = null);
}
