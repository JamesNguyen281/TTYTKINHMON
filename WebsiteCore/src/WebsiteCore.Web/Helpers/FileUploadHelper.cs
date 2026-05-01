namespace WebsiteCore.Web.Helpers;

/// <summary>
/// Helper upload file vào wwwroot/assets/admin/{folder}.
/// Trả về relative path (dùng cho image_path / attach_file_path) hoặc null nếu file invalid.
/// </summary>
public static class FileUploadHelper
{
    private static readonly string[] ImageExt = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private static readonly string[] DocExt   = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx" };
    private const long MaxImageSize = 5 * 1024 * 1024;   // 5MB
    private const long MaxDocSize   = 10 * 1024 * 1024;  // 10MB

    public static async Task<string?> SaveImageAsync(IFormFile? file, IWebHostEnvironment env, string folder)
    {
        if (file == null || file.Length == 0) return null;
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!ImageExt.Contains(ext)) return null;
        if (file.Length > MaxImageSize) return null;
        // M1: validate magic bytes — chống polyglot (PNG/JPEG header thật, không phải fake .jpg chứa HTML/JS)
        if (!await IsValidImageHeaderAsync(file, ext)) return null;
        return await SaveAsync(file, env, "images/" + folder.Trim('/'));
    }

    /// <summary>Kiểm magic bytes thật của file để xác nhận đúng image type.
    /// IFormFile cho phép OpenReadStream() nhiều lần — stream sau có thể đọc lại.</summary>
    private static async Task<bool> IsValidImageHeaderAsync(IFormFile file, string ext)
    {
        var buf = new byte[12];
        await using (var s = file.OpenReadStream())
        {
            var read = await s.ReadAsync(buf, 0, buf.Length);
            if (read < 4) return false;
        }
        // PNG: 89 50 4E 47 0D 0A 1A 0A
        if (ext == ".png")
            return buf[0] == 0x89 && buf[1] == 0x50 && buf[2] == 0x4E && buf[3] == 0x47;
        // JPEG: FF D8 FF
        if (ext is ".jpg" or ".jpeg")
            return buf[0] == 0xFF && buf[1] == 0xD8 && buf[2] == 0xFF;
        // GIF: 47 49 46 38 (GIF8)
        if (ext == ".gif")
            return buf[0] == 0x47 && buf[1] == 0x49 && buf[2] == 0x46 && buf[3] == 0x38;
        // WebP: RIFF....WEBP
        if (ext == ".webp")
            return buf[0] == 0x52 && buf[1] == 0x49 && buf[2] == 0x46 && buf[3] == 0x46
                && buf[8] == 0x57 && buf[9] == 0x45 && buf[10] == 0x42 && buf[11] == 0x50;
        return false;
    }

    public static async Task<string?> SaveDocumentAsync(IFormFile? file, IWebHostEnvironment env, string folder)
    {
        if (file == null || file.Length == 0) return null;
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!DocExt.Contains(ext) && !ImageExt.Contains(ext)) return null;
        if (file.Length > MaxDocSize) return null;
        return await SaveAsync(file, env, folder.Trim('/'));
    }

    private static async Task<string?> SaveAsync(IFormFile file, IWebHostEnvironment env, string subPath)
    {
        var dir = Path.Combine(env.WebRootPath, "assets", "admin", subPath);
        Directory.CreateDirectory(dir);
        var ext = Path.GetExtension(file.FileName);
        var safeName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{Guid.NewGuid():N}{ext}"
            .Replace(" ", "-");
        var fullPath = Path.Combine(dir, safeName);
        using (var fs = File.Create(fullPath))
        {
            await file.CopyToAsync(fs);
        }
        // Return path relative to wwwroot, no leading slash
        return $"assets/admin/{subPath}/{safeName}".Replace("\\", "/");
    }
}
