using CareerConnect.Application.Interfaces;
using Supabase;

namespace CareerConnect.Infrastructure.Services;

public class SupabaseStorageService : IStorageService
{
    private readonly Client _supabaseClient;
    private const string BucketName = "resumes";

    public SupabaseStorageService(Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        // Supabase C# SDK requires initializing the client before first use if not using auto-connect
        await _supabaseClient.InitializeAsync();

        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream, cancellationToken);
        var bytes = memoryStream.ToArray();

        var storage = _supabaseClient.Storage.From(BucketName);
        
        // Use a unique file name to avoid collisions
        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        
        var options = new Supabase.Storage.FileOptions
        {
            ContentType = contentType,
            Upsert = true
        };

        await storage.Upload(bytes, uniqueFileName, options);

        // Get public URL
        var publicUrl = storage.GetPublicUrl(uniqueFileName);
        return publicUrl;
    }

    public async Task DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        await _supabaseClient.InitializeAsync();
        
        // Extract the file name from the URL
        var uri = new Uri(fileUrl);
        var fileName = uri.Segments.Last();

        var storage = _supabaseClient.Storage.From(BucketName);
        await storage.Remove(new List<string> { fileName });
    }
}
