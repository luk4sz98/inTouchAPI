namespace inTouchAPI.Services;

public interface IBlobStorageService
{
    Task<bool> RemoveBlobAsync(string blob);
    Task<string> SaveBlobAsync(IFormFile avatar);
}