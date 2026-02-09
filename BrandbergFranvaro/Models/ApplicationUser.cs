using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BrandbergFranvaro.Models;

/// <summary>
/// Utökad användare med för- och efternamn
/// </summary>
public class ApplicationUser : IdentityUser
{
    [MaxLength(100)]
    [Display(Name = "Förnamn")]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    [Display(Name = "Efternamn")]
    public string? LastName { get; set; }

    /// <summary>
    /// Returnerar fullständigt namn eller e-post om namn saknas
    /// </summary>
    public string FullName => 
        !string.IsNullOrWhiteSpace(FirstName) || !string.IsNullOrWhiteSpace(LastName)
            ? $"{FirstName} {LastName}".Trim()
            : Email ?? UserName ?? "Okänd";

    /// <summary>
    /// Navigation property för frånvaroärenden
    /// </summary>
    public virtual ICollection<AbsenceRequest> AbsenceRequests { get; set; } = new List<AbsenceRequest>();
}

