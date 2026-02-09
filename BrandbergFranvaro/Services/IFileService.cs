namespace BrandbergFranvaro.Services;

public interface IFileService
{
    Task<(bool Success, string? FilePath, string? ErrorMessage)> SaveFileAsync(IFormFile file);
    Task<bool> DeleteFileAsync(string filePath);
    (bool IsValid, string? ErrorMessage) ValidateFile(IFormFile file);
    string GetContentType(string filePath);
    string GetFullPath(string relativePath);
}

