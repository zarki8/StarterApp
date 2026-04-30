namespace StarterApp.Models;

public class NearbyItem
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal DailyRate { get; set; }

    public string Category { get; set; } = string.Empty;

    public int OwnerId { get; set; }

    public string OwnerName { get; set; } = string.Empty;

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public decimal? DistanceKm { get; set; }

    public bool IsAvailable { get; set; }

    public decimal? AverageRating { get; set; }

    public string ImageUrl { get; set; } = string.Empty;
}
