using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace SAV.Infrastructure.Services;

public interface IFileService
{
    Task<string> SaveFileAsync(IFormFile file, string folder);
    Task<bool> DeleteFileAsync(string filePath);
    Task<byte[]> GetFileAsync(string filePath);
}

public class FileService : IFileService
{
    private readonly string _uploadPath;

    public FileService(IHostEnvironment env)
    {
        _uploadPath = Path.Combine(env.ContentRootPath, "uploads");
        if (!Directory.Exists(_uploadPath))
            Directory.CreateDirectory(_uploadPath);
    }

    public async Task<string> SaveFileAsync(IFormFile file, string folder)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty");

        var folderPath = Path.Combine(_uploadPath, folder);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        var filePath = Path.Combine(folderPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return Path.Combine(folder, fileName);
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        var fullPath = Path.Combine(_uploadPath, filePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            return await Task.FromResult(true);
        }
        return await Task.FromResult(false);
    }

    public async Task<byte[]> GetFileAsync(string filePath)
    {
        var fullPath = Path.Combine(_uploadPath, filePath);
        if (File.Exists(fullPath))
        {
            return await File.ReadAllBytesAsync(fullPath);
        }
        throw new FileNotFoundException("File not found");
    }
}
