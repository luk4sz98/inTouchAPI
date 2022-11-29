﻿using Azure.Storage.Blobs.Models;

namespace inTouchAPI.Services;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _blobContainerClient;

    public BlobStorageService(BlobContainerClient blobContainerClient)
    {
        _blobContainerClient = blobContainerClient;
    }

    public async Task<bool> RemoveBlobAsync(string blob)
    {
        if (string.IsNullOrEmpty(blob)) return false;

        var response = await _blobContainerClient.DeleteBlobAsync(blob, DeleteSnapshotsOption.IncludeSnapshots);

        return !response.IsError;
    }

    public async Task<string> SaveBlobAsync(IFormFile avatar)
    {
        var extension = Path.GetExtension(avatar.FileName);
        var blobName = Utility.GenerateRandomString(15) + extension;

        using var stream = new MemoryStream();
        await avatar.CopyToAsync(stream);
        stream.Position = 0;

        var response = await _blobContainerClient.UploadBlobAsync(blobName, stream);
        var rawResponse = response.GetRawResponse();

        if (rawResponse.IsError) 
            return string.Empty;

        var blobClient = _blobContainerClient.GetBlobClient(blobName);
        
        BlobProperties properties = await blobClient.GetPropertiesAsync();
        
        var headers = new BlobHttpHeaders
        {
            ContentType = "image/" + extension.Substring(1),
            ContentLanguage = properties.ContentLanguage,
            CacheControl = properties.CacheControl,
            ContentDisposition = properties.ContentDisposition,
            ContentEncoding = properties.ContentEncoding,
            ContentHash = properties.ContentHash
        };
        await blobClient.SetHttpHeadersAsync(headers);

        return blobName;
    }
}
