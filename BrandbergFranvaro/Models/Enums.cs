namespace BrandbergFranvaro.Models;

/// <summary>
/// Typ av frånvaro
/// </summary>
public enum AbsenceType
{
    Sjuk = 1,
    VAB = 2,
    Semester = 3,
    Ledig = 4,
    Annat = 5
}

/// <summary>
/// Omfattning av frånvaron
/// </summary>
public enum DayPart
{
    Heldag = 1,
    Förmiddag = 2,
    Eftermiddag = 3
}

/// <summary>
/// Status för frånvaroärende
/// </summary>
public enum AbsenceStatus
{
    Skickad = 1,
    Godkänd = 2,
    Avslagen = 3
}

/// <summary>
/// Hjälpklass för att hämta svenska namn på enums
/// </summary>
public static class EnumExtensions
{
    public static string ToSwedish(this AbsenceType type) => type switch
    {
        AbsenceType.Sjuk => "Sjuk",
        AbsenceType.VAB => "VAB",
        AbsenceType.Semester => "Semester",
        AbsenceType.Ledig => "Ledig",
        AbsenceType.Annat => "Annat",
        _ => type.ToString()
    };

    public static string ToSwedish(this DayPart part) => part switch
    {
        DayPart.Heldag => "Heldag",
        DayPart.Förmiddag => "Förmiddag",
        DayPart.Eftermiddag => "Eftermiddag",
        _ => part.ToString()
    };

    public static string ToSwedish(this AbsenceStatus status) => status switch
    {
        AbsenceStatus.Skickad => "Skickad",
        AbsenceStatus.Godkänd => "Godkänd",
        AbsenceStatus.Avslagen => "Avslagen",
        _ => status.ToString()
    };

    public static string ToBadgeClass(this AbsenceStatus status) => status switch
    {
        AbsenceStatus.Skickad => "bg-warning text-dark",
        AbsenceStatus.Godkänd => "bg-success",
        AbsenceStatus.Avslagen => "bg-danger",
        _ => "bg-secondary"
    };

    public static string ToBadgeClass(this AbsenceType type) => type switch
    {
        AbsenceType.Sjuk => "bg-danger",
        AbsenceType.VAB => "bg-info text-dark",
        AbsenceType.Semester => "bg-success",
        AbsenceType.Ledig => "bg-primary",
        AbsenceType.Annat => "bg-secondary",
        _ => "bg-secondary"
    };
}

