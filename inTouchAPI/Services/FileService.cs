using Microsoft.Net.Http.Headers;

namespace inTouchAPI.Services;

public class FileService : IFileService
{
    public void RemoveFile(string fileName)
    {
        var folderName = Path.Combine("Resources", "Images");
        var filePath =  Path.Combine(Directory.GetCurrentDirectory(), folderName);                
        Directory.Delete(filePath, true);
    }

    public async Task<string> Savefile(IFormFile avatar)
    {
        var folderName = Path.Combine("Resources", "Images");
        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), folderName);
        var fileName = Utility.GenerateRandomString(20) + Path.GetExtension(avatar.FileName);
        var fullPathToSave = Path.Combine(folderPath, fileName);
        
        using var fileStream = new FileStream(fullPathToSave, FileMode.Create);
        await avatar.CopyToAsync(fileStream);
        
        return fileName;
    }
}
