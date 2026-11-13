// CrazyGames integration preserved; excluded from the default GamePix build.
#if TURBODASH_CRAZYGAMES
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

#else
using System.Globalization;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>GamePix synchronizes localStorage; Unity PlayerPrefs/IndexedDB is used only outside WebGL.</summary>
public static class GameData
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern string TurboDashStorageGet(string key);
    [DllImport("__Internal")] private static extern void TurboDashStorageSet(string key, string value);
    [DllImport("__Internal")] private static extern void TurboDashStorageDelete(string key);
    [DllImport("__Internal")] private static extern void TurboDashStorageClear();
    public static bool HasKey(string key) => TurboDashStorageGet(key) != null;
    public static string GetString(string key, string defaultValue = "") => TurboDashStorageGet(key) ?? defaultValue;
    public static int GetInt(string key, int defaultValue = 0) => int.TryParse(GetString(key, null), NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) ? value : defaultValue;
    public static float GetFloat(string key, float defaultValue = 0f) => float.TryParse(GetString(key, null), NumberStyles.Float, CultureInfo.InvariantCulture, out float value) ? value : defaultValue;
    public static void SetString(string key, string value) => TurboDashStorageSet(key, value ?? "");
    public static void SetInt(string key, int value) => SetString(key, value.ToString(CultureInfo.InvariantCulture));
    public static void SetFloat(string key, float value) => SetString(key, value.ToString("R", CultureInfo.InvariantCulture));
    public static void DeleteKey(string key) => TurboDashStorageDelete(key);
    public static void DeleteAll() => TurboDashStorageClear();
    public static void Save() { } // localStorage writes are synchronous.
#else
    public static bool HasKey(string key) => PlayerPrefs.HasKey(key);
    public static string GetString(string key, string defaultValue = "") => PlayerPrefs.GetString(key, defaultValue);
    public static int GetInt(string key, int defaultValue = 0) => PlayerPrefs.GetInt(key, defaultValue);
    public static float GetFloat(string key, float defaultValue = 0f) => PlayerPrefs.GetFloat(key, defaultValue);
    public static void SetString(string key, string value) => PlayerPrefs.SetString(key, value);
    public static void SetInt(string key, int value) => PlayerPrefs.SetInt(key, value);
    public static void SetFloat(string key, float value) => PlayerPrefs.SetFloat(key, value);
    public static void DeleteKey(string key) => PlayerPrefs.DeleteKey(key);
    public static void DeleteAll() => PlayerPrefs.DeleteAll();
    public static void Save() => PlayerPrefs.Save();
#endif
}
#endif
