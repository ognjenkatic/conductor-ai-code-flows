namespace Codeflows.Metrics.Csharp.Common.Util;

public static class StringUtils
{
    private static readonly Random random = new();
    private static readonly string characters =
        "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890";

    public static string GetRandomString(int length) =>
        new(
            Enumerable
                .Range(0, length)
                .Select(n => characters[random.Next(0, characters.Length)])
                .ToArray()
        );
}
