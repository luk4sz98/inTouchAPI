namespace inTouchAPI.Helpers;

public static class Utility
{
    public static string GenerateRandomString(int length)
    {
        var random = new Random();
        var chars = "ABCDEFGHIJKLMNOPQRSTUWXYZ1234567890abcdefghijklmnopqrstuvwxyz_";
        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
