namespace ABCEnterprisePart1.Services;
using Azure.Storage.Blobs;

public class BlobService
{
    private readonly BlobContainerClient _containerClient;
    public BlobService(string connectionString, string containerName)
    {
        _containerClient = new BlobContainerClient(connectionString, containerName);
        _containerClient.CreateIfNotExists();
    }

    public async Task<string> UploadAsync(string fileName, Stream fileStream)
    {
        var blobClient = _containerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(fileStream, true);
        return blobClient.Uri.ToString();
    }
}
