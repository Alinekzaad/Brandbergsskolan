namespace BrandbergFranvaro.Services;

public class FileService : IFileService
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileService> _logger;

    private readonly long _maxFileSize;
    private readonly string[] _allowedExtensions;
    private readonly string _uploadPath;

    public FileService(
        IConfiguration configuration,
        IWebHostEnvironment environment,
        ILogger<FileService> logger)
    {
        _configuration = configuration;
        _environment = environment;
        _logger = logger;

        _maxFileSize = _configuration.GetValue<long>("FileUpload:MaxFileSizeBytes", 5 * 1024 * 1024);
        _allowedExtensions = _configuration.GetSection("FileUpload:AllowedExtensions").Get<string[]>() 
            ?? new[] { ".pdf", ".jpg", ".jpeg", ".png" };
        _uploadPath = _configuration.GetValue<string>("FileUpload:UploadPath") ?? "App_Data/Uploads";
    }

    public (bool IsValid, string? ErrorMessage) ValidateFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return (false, "Ingen fil vald.");
        }

        if (file.Length > _maxFileSize)
        {
            var maxSizeMB = _maxFileSize / (1024 * 1024);
            return (false, $"Filen är för stor. Maximal storlek är {maxSizeMB} MB.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
        {
            var allowed = string.Join(", ", _allowedExtensions);
            return (false, $"Ogiltig filtyp. Tillåtna typer: {allowed}");
        }

        return (true, null);
    }

    public async Task<(bool Success, string? FilePath, string? ErrorMessage)> SaveFileAsync(IFormFile file)
    {
        var validation = ValidateFile(file);
        if (!validation.IsValid)
        {
            return (false, null, validation.ErrorMessage);
        }

        try
        {
            var fullUploadPath = GetFullPath(_uploadPath);
            
            if (!Directory.Exists(fullUploadPath))
            {
                Directory.CreateDirectory(fullUploadPath);
            }

            // Skapa unikt filnamn
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(fullUploadPath, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Returnera relativ sökväg för lagring i databasen
            var relativePath = Path.Combine(_uploadPath, uniqueFileName);
            
            _logger.LogInformation("Fil sparad: {FilePath}", relativePath);
            
            return (true, relativePath, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid sparande av fil: {FileName}", file.FileName);
            return (false, null, "Ett fel uppstod vid uppladdning av filen. Försök igen.");
        }
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return false;
        }

        try
        {
            var fullPath = GetFullPath(filePath);
            
            if (File.Exists(fullPath))
            {
                await Task.Run(() => File.Delete(fullPath));
                _logger.LogInformation("Fil borttagen: {FilePath}", filePath);
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid borttagning av fil: {FilePath}", filePath);
            return false;
        }
    }

    public string GetContentType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream"
        };
    }

    public string GetFullPath(string relativePath)
    {
        return Path.Combine(_environment.ContentRootPath, relativePath);
    }
}

