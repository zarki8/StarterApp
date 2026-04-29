using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StarterApp.Database.Models;

[Table("items")]
[PrimaryKey(nameof(Id))]
public class Item
{
    public int Id { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public decimal DailyRate { get; set; }

    [Required]
    public string Category { get; set; } = string.Empty;

    [Required]
    public string Location { get; set; } = string.Empty;

    public int OwnerId { get; set; }

    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }

    public bool IsActive { get; set; } = true;

    public User? Owner { get; set; }
}
