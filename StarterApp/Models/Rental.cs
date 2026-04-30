namespace StarterApp.Models;

public class Rental
{
    public int Id { get; set; }

    public int ItemId { get; set; }

    public string ItemTitle { get; set; } = string.Empty;

    public int? BorrowerId { get; set; }

    public string BorrowerName { get; set; } = string.Empty;

    public int? OwnerId { get; set; }

    public string OwnerName { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public decimal TotalPrice { get; set; }

    public DateTime? RequestedAt { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public string CounterpartyName => !string.IsNullOrWhiteSpace(OwnerName) ? OwnerName : BorrowerName;

    public string DateRangeDisplay => $"{StartDate:dd MMM yyyy} - {EndDate:dd MMM yyyy}";

    public string TotalPriceDisplay => $"£{TotalPrice:0.00}";
}
