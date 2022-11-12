namespace inTouchAPI.Services;

public interface IFileService
{
    void RemoveFile(string fileName);
    Task<string> Savefile(IFormFile avatar);
}
