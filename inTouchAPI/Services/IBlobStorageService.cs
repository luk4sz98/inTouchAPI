namespace inTouchAPI.Services;

public interface IBlobStorageService
{
    Task<bool> RemoveAvatarAsync(string blob);
    Task<bool> RemoveMessageFileAsync(string blob);
    Task<string> SaveAvatarAsync(IFormFile avatar);
    Task<string> SaveMessageFileAsync(IFormFile file);
}