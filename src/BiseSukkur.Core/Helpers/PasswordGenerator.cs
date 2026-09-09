using System.Security.Cryptography;

namespace BiseSukkur.Core.Helpers;

public static class PasswordGenerator
{
    private const string Upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string Lower = "abcdefghijkmnopqrstuvwxyz";
    private const string Digits = "23456789";
    private const string Symbols = "!@#$%&*";

    public static string GenerateTemporaryPassword(int length = 14)
    {
        if (length < 12)
        {
            length = 12;
        }

        var required = new[]
        {
            Upper[RandomNumberGenerator.GetInt32(Upper.Length)],
            Lower[RandomNumberGenerator.GetInt32(Lower.Length)],
            Digits[RandomNumberGenerator.GetInt32(Digits.Length)],
            Symbols[RandomNumberGenerator.GetInt32(Symbols.Length)]
        };

        var all = Upper + Lower + Digits + Symbols;
        var chars = new char[length];
        for (var i = 0; i < required.Length; i++)
        {
            chars[i] = required[i];
        }

        for (var i = required.Length; i < length; i++)
        {
            chars[i] = all[RandomNumberGenerator.GetInt32(all.Length)];
        }

        Shuffle(chars);
        return new string(chars);
    }

    private static void Shuffle(char[] chars)
    {
        for (var i = chars.Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }
    }
}
