using StarterApp.Database.Models;
using StarterApp.Repositories;
using StarterApp.Test.Fixtures;

namespace StarterApp.Test.Repositories;

public class ItemRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public ItemRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(Skip = "Integration test requires a PostgreSQL test database configured with TEST_CONNECTION_STRING.")]
    public async Task AddAsync_ValidItem_PersistsItem()
    {
        // Arrange
        var owner = new User
        {
            FirstName = "Integration",
            LastName = "Owner",
            Email = $"owner-{Guid.NewGuid():N}@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };

        _fixture.Context.Users.Add(owner);
        await _fixture.Context.SaveChangesAsync();

        var repository = new ItemRepository(_fixture.Context);
        var item = new Item
        {
            Title = "Integration Drill",
            Description = "DatabaseFixture test item",
            DailyRate = 5,
            Category = "Tools",
            Location = "55.9533, -3.1883",
            OwnerId = owner.Id
        };

        // Act
        var savedItem = await repository.AddAsync(item);

        // Assert
        Assert.True(savedItem.Id > 0);
        Assert.NotNull(await repository.GetByIdAsync(savedItem.Id));
    }
}
