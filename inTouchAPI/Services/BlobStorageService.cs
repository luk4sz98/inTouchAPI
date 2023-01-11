using Azure.Storage.Blobs.Models;

namespace inTouchAPI.Services;

/// <summary>
/// Klasa odpowiedzialna za zapisywanie oraz odczyt przesłanych plików lub avatarów ze storage
/// </summary>
public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly IConfiguration _config;


    public BlobStorageService(BlobServiceClient blobServiceClient, IConfiguration config)
    {
        _blobServiceClient = blobServiceClient;
        _config = config;
    }

    /// <summary>
    /// Metoda odpowiedzialna za usunięcie avataru ze storage z platformy Azure
    /// </summary>
    /// <param name="blob">Nazwa avatara do skasowania</param>
    /// <returns>Wartość logiczną w zależności od powodzenia</returns>
    public async Task<bool> RemoveAvatarAsync(string blob)
    {
        if (string.IsNullOrEmpty(blob)) 
            return false;
        
        var avatarsContainerName = _config.GetSection("BlobStorage").GetValue<string>("AvatarsContainer");
        var avatarsContainer = _blobServiceClient.GetBlobContainerClient(avatarsContainerName);

        var response = await avatarsContainer.DeleteBlobAsync(blob, DeleteSnapshotsOption.IncludeSnapshots);

        return !response.IsError;
    }

    /// <summary>
    /// Metoda odpowiedzialna za usunięcie pliku ze storage z platformy Azure
    /// </summary>
    /// <param name="blob">Nazwa pliku do skasowania</param>
    /// <returns>Wartość logiczną w zależności od powodzenia</returns>
    public async Task<bool> RemoveMessageFileAsync(string blob)
    {
        if (string.IsNullOrEmpty(blob))
            return false;

        var messageFilesContainerName = _config.GetSection("BlobStorage").GetValue<string>("MessageFilesContainer");
        var messageFilesContainer = _blobServiceClient.GetBlobContainerClient(messageFilesContainerName);

        var response = await messageFilesContainer.DeleteBlobAsync(blob, DeleteSnapshotsOption.IncludeSnapshots);

        return !response.IsError;
    }

    /// <summary>
    /// Metoda odpowiedzialna za zapisanie avataru do storage z platformy Azure
    /// </summary>
    /// <param name="avatar">Avatara do skasowania</param>
    /// <returns>Nazwę zapisanego avatara</returns>
    public async Task<string> SaveAvatarAsync(IFormFile avatar)
    {
        var extension = Path.GetExtension(avatar.FileName);
        var blobName = Utility.GenerateRandomString(15) + extension;

        using var stream = new MemoryStream();
        await avatar.CopyToAsync(stream);
        stream.Position = 0;

        var avatarsContainerName = _config.GetSection("BlobStorage").GetValue<string>("MessageFilesContainer");
        var avatarsContainer = _blobServiceClient.GetBlobContainerClient(avatarsContainerName);

        var blobClient = avatarsContainer.GetBlobClient(blobName);
       
        BlobHttpHeaders headers = new()
        {
            ContentType = "image/" + extension[1..]
        };

        var response = await blobClient.UploadAsync(stream, headers);
        var rawResponse = response.GetRawResponse();

        if (rawResponse.IsError)
            return string.Empty;

        return blobName;
    }

    /// <summary>
    /// Metoda odpowiedzialna za zapisanie pliku przesłanego w wiadomości do storage z platformy Azure
    /// </summary>
    /// <param name="file">Plik do skasowania</param>
    /// <returns>Nazwę zapisanego pliku</returns>
    public async Task<string> SaveMessageFileAsync(IFormFile file)
    {
        var fileName = Path.GetFileNameWithoutExtension(file.FileName);
        var extension = Path.GetExtension(file.FileName);
        var blobName = fileName + extension;

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0;

        var messageFilesContainerName = _config.GetSection("BlobStorage").GetValue<string>("MessageFilesContainer");
        var messageFilesContainer = _blobServiceClient.GetBlobContainerClient(messageFilesContainerName);

        var blobClient = messageFilesContainer.GetBlobClient(blobName);

        string contentType;
        if (file.ContentType != null)
            contentType = file.ContentType;
        else
            contentType = SetContentType(extension[1..]);
        
        BlobHttpHeaders headers = new()
        {
            ContentType = contentType
        };

        var response = await blobClient.UploadAsync(stream, headers);
        var rawResponse = response.GetRawResponse();

        if (rawResponse.IsError)
            return string.Empty;

        return _config.GetSection("BlobStorage").GetValue<string>("FilesUrl") + blobName;
    }

    private static string SetContentType(string fileType)
    {
        return fileType switch
        {
            "png" or "jpg" or "jpeg" or "gif" => "image/" + fileType,
            "pdf" => "application/pdf",
            "txt" => "text/plain",
            "doc" or "docx" => "application/msword",
            "mp3" => "audio/mpeg",
            "mp4" => "video/mp4",
            _ => "application/octet-stream",
        };
    }
}
