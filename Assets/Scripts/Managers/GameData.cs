using CrazyGames;
using UnityEngine;

/// <summary>
/// Save data. Same calls as PlayerPrefs, but on CrazyGames it goes through the SDK data module,
/// so progress follows the player's CrazyGames account instead of staying in one browser.
/// Everywhere else (and for values saved before this change) it falls back to PlayerPrefs.
/// </summary>
public static class GameData
{
    private static bool UseSdk => Platform.Ready;

    public static bool HasKey(string key)
    {
        if (UseSdk && CrazySDK.Data.HasKey(key))
            return true;

        return PlayerPrefs.HasKey(key);
    }

    public static int GetInt(string key, int defaultValue = 0)
    {
        if (UseSdk && CrazySDK.Data.HasKey(key))
            return CrazySDK.Data.GetInt(key, defaultValue);

        return PlayerPrefs.GetInt(key, defaultValue);
    }

    public static float GetFloat(string key, float defaultValue = 0f)
    {
        if (UseSdk && CrazySDK.Data.HasKey(key))
            return CrazySDK.Data.GetFloat(key, defaultValue);

        return PlayerPrefs.GetFloat(key, defaultValue);
    }

    public static string GetString(string key, string defaultValue = "")
    {
        if (UseSdk && CrazySDK.Data.HasKey(key))
            return CrazySDK.Data.GetString(key, defaultValue);

        return PlayerPrefs.GetString(key, defaultValue);
    }

    public static void SetInt(string key, int value)
    {
        if (UseSdk) CrazySDK.Data.SetInt(key, value);
        else PlayerPrefs.SetInt(key, value);
    }

    public static void SetFloat(string key, float value)
    {
        if (UseSdk) CrazySDK.Data.SetFloat(key, value);
        else PlayerPrefs.SetFloat(key, value);
    }

    public static void SetString(string key, string value)
    {
        if (UseSdk) CrazySDK.Data.SetString(key, value);
        else PlayerPrefs.SetString(key, value);
    }

    public static void DeleteKey(string key)
    {
        if (UseSdk) CrazySDK.Data.DeleteKey(key);
        PlayerPrefs.DeleteKey(key);
    }

    public static void DeleteAll()
    {
        if (UseSdk) CrazySDK.Data.DeleteAll();
        PlayerPrefs.DeleteAll();
    }

    // The SDK stores each value as it is set; only PlayerPrefs needs an explicit flush
    public static void Save()
    {
        PlayerPrefs.Save();
    }
}
