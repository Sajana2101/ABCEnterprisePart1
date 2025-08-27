namespace ABCEnterprisePart1.Services;

using Azure.Storage.Files.Shares;

public class FileService
{
    private readonly ShareClient _shareClient;
    public FileService(string connectionString, string shareName)
    {
        _shareClient = new ShareClient(connectionString, shareName);
        _shareClient.CreateIfNotExists();
    }

    public async Task<string> UploadAsync(string fileName, Stream fileStream)
    {
        var directory = _shareClient.GetRootDirectoryClient();

        // Make file name unique
        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";

        var fileClient = directory.GetFileClient(uniqueFileName);
        await fileClient.CreateAsync(fileStream.Length);
        await fileClient.UploadAsync(fileStream);

        return fileClient.Uri.ToString();
    }



    public async Task<List<string>> ListFilesAsync()
    {
        var directory = _shareClient.GetRootDirectoryClient();
        var files = new List<string>();

        await foreach (var fileItem in directory.GetFilesAndDirectoriesAsync())
        {
            if (!fileItem.IsDirectory)
                files.Add(fileItem.Name);
        }

        return files;
    }
}