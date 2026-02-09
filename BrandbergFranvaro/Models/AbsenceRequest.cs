using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BrandbergFranvaro.Models;

/// <summary>
/// Frånvaroärende
/// </summary>
[Index(nameof(UserId), nameof(StartDate))]
[Index(nameof(Status))]
[Index(nameof(Type))]
public class AbsenceRequest
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser? User { get; set; }

    [Required]
    [Display(Name = "Typ")]
    public AbsenceType Type { get; set; }

    [Required]
    [Display(Name = "Startdatum")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [Required]
    [Display(Name = "Slutdatum")]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    [Required]
    [Display(Name = "Omfattning")]
    public DayPart DayPart { get; set; } = DayPart.Heldag;

    [MaxLength(1000)]
    [Display(Name = "Kommentar")]
    public string? Comment { get; set; }

    [Required]
    [Display(Name = "Status")]
    public AbsenceStatus Status { get; set; } = AbsenceStatus.Skickad;

    [MaxLength(1000)]
    [Display(Name = "Admin-kommentar")]
    public string? AdminComment { get; set; }

    [MaxLength(500)]
    [Display(Name = "Bilaga")]
    public string? AttachmentPath { get; set; }

    [Display(Name = "Skapad")]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [Display(Name = "Uppdaterad")]
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Returnerar perioden som formaterad sträng
    /// </summary>
    public string PeriodDisplay => StartDate.Date == EndDate.Date
        ? StartDate.ToString("yyyy-MM-dd")
        : $"{StartDate:yyyy-MM-dd} – {EndDate:yyyy-MM-dd}";

    /// <summary>
    /// Antal dagar i perioden
    /// </summary>
    public int DaysCount => (EndDate.Date - StartDate.Date).Days + 1;
}

