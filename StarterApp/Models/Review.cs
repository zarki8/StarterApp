namespace StarterApp.Models;

public class Review
{
    public int Id { get; set; }

    public int? RentalId { get; set; }

    public int? ItemId { get; set; }

    public string ItemTitle { get; set; } = string.Empty;

    public int ReviewerId { get; set; }

    public string ReviewerName { get; set; } = string.Empty;

    public int Rating { get; set; }

    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
