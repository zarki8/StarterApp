namespace StarterApp.Repositories;

public interface IRepository<T>
{
    Task<List<T>> GetAllAsync();

    Task<T?> GetByIdAsync(int id);
}
