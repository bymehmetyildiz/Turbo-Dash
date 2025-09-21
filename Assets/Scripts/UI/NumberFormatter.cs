using UnityEngine;

public static class NumberFormatter
{
    /// <summary>
    /// Formats large numbers into short form with suffixes (k, M, B, T).
    /// Example: 1500 -> "1.5k", 2000000 -> "2M"
    /// </summary>
    public static string FormatNumber(long num)
    {
        if (num >= 1_000_000_000_000) return (num / 1_000_000_000_000f).ToString("0.#") + "T";
        if (num >= 1_000_000_000) return (num / 1_000_000_000f).ToString("0.#") + "B";
        if (num >= 1_000_000) return (num / 1_000_000f).ToString("0.#") + "M";
        if (num >= 1_0000) return (num / 1_0000f).ToString("0.#") + "k";
        return num.ToString();
    }
}