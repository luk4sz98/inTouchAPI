namespace inTouchAPI.Helpers;

public static class Utility
{
    private static readonly string[] _permittedAvatarExtensions = { ".jpeg", ".png", ".jpg" };

    public static string GenerateRandomString(int length)
    {
        var random = new Random();
        var chars = "ABCDEFGHIJKLMNOPQRSTUWXYZ1234567890abcdefghijklmnopqrstuvwxyz_";
        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public static bool IsValidAvatarExtension(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName);
        return !string.IsNullOrEmpty(extension) && _permittedAvatarExtensions.Contains(extension);
    }
}
