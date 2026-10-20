using System.Globalization;

namespace BLL.Common;

public static class FileSizeFormatter
{
    private const double Kilobyte = 1024;
    private const double Megabyte = Kilobyte * 1024;

    public static string Format(long bytes) => bytes switch
    {
        < (long)Kilobyte => string.Create(CultureInfo.InvariantCulture, $"{bytes} B"),
        < (long)Megabyte => string.Create(CultureInfo.InvariantCulture, $"{bytes / Kilobyte:0.#} KB"),
        _ => string.Create(CultureInfo.InvariantCulture, $"{bytes / Megabyte:0.#} MB"),
    };
}
