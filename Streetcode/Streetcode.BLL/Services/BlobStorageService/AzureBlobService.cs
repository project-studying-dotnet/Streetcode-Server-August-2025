using System.Security.Cryptography;
using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using Streetcode.BLL.Interfaces.BlobStorage;

namespace Streetcode.BLL.Services.BlobStorageService;

public class AzureBlobService : IBlobService
{
    private readonly BlobContainerClient _blobContainerClient;

    public AzureBlobService(IOptions<AzureBlobEnvironmentVariables> azureOptions)
    {
        var options = azureOptions.Value;

        var blobServiceClient = new BlobServiceClient(options.ConnectionString);
        _blobContainerClient = blobServiceClient.GetBlobContainerClient(options.ContainerName);

        _blobContainerClient.CreateIfNotExists();
    }

    public string SaveFileInStorage(string base64, string name, string mimeType)
    {
        var bytes = Convert.FromBase64String(base64);

        var createdBlobName = $"{DateTime.Now}{name}"
            .Replace(" ", "_")
            .Replace(":", "-")
            .Replace(".", "_");

        var hashBlobName = HashFunction(createdBlobName);

        using var stream = new MemoryStream(bytes);
        var blobClient = _blobContainerClient.GetBlobClient($"{hashBlobName}.{mimeType}");

        blobClient.Upload(stream, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = GetContentType(mimeType)
            }
        });

        return $"{hashBlobName}.{mimeType}";
    }

    public MemoryStream FindFileInStorageAsMemoryStream(string name)
    {
        var bytes = FindFileInStorage(name);
        return new MemoryStream(bytes);
    }

    public string FindFileInStorageAsBase64(string name)
    {
        var bytes = FindFileInStorage(name);
        return Convert.ToBase64String(bytes);
    }

    public string UpdateFileInStorage(string previousBlobName, string base64Format, string newBlobName, string extension)
    {
        DeleteFileInStorage(previousBlobName);
        return SaveFileInStorage(base64Format, newBlobName, extension);
    }

    public void DeleteFileInStorage(string name)
    {
        var blobClient = _blobContainerClient.GetBlobClient(name);
        blobClient.DeleteIfExists();
    }

    private static string HashFunction(string createdFileName)
    {
        var enc = Encoding.UTF8;
        var result = SHA256.HashData(enc.GetBytes(createdFileName));
        return Convert.ToBase64String(result).Replace('/', '_');
    }

    private static string GetContentType(string mimeType)
    {
        return mimeType.ToLower() switch
        {
            "jpg" or "jpeg" => "image/jpeg",
            "png" => "image/png",
            "gif" => "image/gif",
            "mp3" => "audio/mpeg",
            "wav" => "audio/wav",
            "pdf" => "application/pdf",
            _ => "application/octet-stream"
        };
    }

    private byte[] FindFileInStorage(string blobName)
    {
        var blobClient = _blobContainerClient.GetBlobClient(blobName);

        if (!blobClient.Exists())
        {
            throw new FileNotFoundException($"Blob with name {blobName} not found.");
        }

        var downloadInfo = blobClient.DownloadContent();
        return downloadInfo.Value.Content.ToArray();
    }
}
