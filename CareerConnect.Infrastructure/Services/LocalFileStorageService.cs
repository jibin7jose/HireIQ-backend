using CareerConnect.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace CareerConnect.Infrastructure.Services;

public class LocalFileStorageService : IStorageService
{
    private readonly IWebHostEnvironment _env;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LocalFileStorageService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
    {
        _env = env;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var uploadPath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "resumes");
        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(uploadPath, uniqueFileName);

        using (var fileStreamTarget = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fileStreamTarget, cancellationToken);
        }

        var request = _httpContextAccessor.HttpContext?.Request;
        if (request != null)
        {
            var baseUrl = $"{request.Scheme}://{request.Host.Value}{request.PathBase.Value}";
            return $"{baseUrl}/resumes/{uniqueFileName}";
        }

        // Fallback if HttpContext is not available
        return $"/resumes/{uniqueFileName}";
    }

    public Task DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(fileUrl)) return Task.CompletedTask;

        var uri = new Uri(fileUrl, UriKind.RelativeOrAbsolute);
        var fileName = uri.IsAbsoluteUri ? uri.Segments.Last() : fileUrl.Split('/').Last();
        
        var filePath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "resumes", fileName);
        
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }
}
