namespace inTouchAPI.Helpers;

/// <summary>
/// Klasa statyczna z metodami ogólno dostępnymi
/// </summary>
public static class Utility
{
    private static readonly string[] _permittedAvatarExtensions = { ".jpeg", ".png", ".jpg" };

    /// <summary>
    /// Metoda generuje napis o zadanej długości
    /// </summary>
    /// <param name="length">Określa ilość znaków w wygenerowanym napisie</param>
    /// <returns></returns>
    public static string GenerateRandomString(int length)
    {
        var random = new Random();
        var chars = "ABCDEFGHIJKLMNOPQRSTUWXYZ1234567890abcdefghijklmnopqrstuvwxyz_";
        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    /// <summary>
    /// Metoda sprawdzająca czy rozszerzenie przesłanego pliku jest jednym
    /// z dozwolonych dla avataru
    /// </summary>
    /// <param name="file">Avatar przesłany przez użytkownika</param>
    /// <returns></returns>
    public static bool IsValidAvatarExtension(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName);
        return !string.IsNullOrEmpty(extension) && _permittedAvatarExtensions.Contains(extension);
    }
}
