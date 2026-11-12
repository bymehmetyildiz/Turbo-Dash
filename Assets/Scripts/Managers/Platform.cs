using System;
using CrazyGames;
using UnityEngine;

/// <summary>
/// The only place that talks to the CrazyGames SDK. Every call is safe when the game runs somewhere else
/// (or before the SDK has finished initializing): it simply does nothing, or reports "no ad".
/// To move to another portal, this file and GameData are the only ones to rewrite.
/// </summary>
public static class Platform
{
    // Seconds between two midgame ads requested by the game. CrazyGames also throttles on its side.
    private const float midgameMinInterval = 120f;

    private static bool gameplayActive;
    private static float lastMidgameTime = -999f;

    public static bool Ready => CrazySDK.IsAvailable && CrazySDK.IsInitialized;

    // Starts the SDK as early as possible. In the editor this completes immediately.
    // In a WebGL build LoadingScene waits for it before opening the game scene.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Boot()
    {
        gameplayActive = false;

        if (!CrazySDK.IsAvailable)
            return;

        try
        {
            CrazySDK.Init(() => Debug.Log("CrazySDK initialized"));
        }
        catch (Exception e)
        {
            Debug.LogWarning("CrazySDK init failed: " + e.Message);
        }
    }

    // ---------------- Gameplay events ----------------
    public static void GameplayStart()
    {
        if (gameplayActive)
            return;

        gameplayActive = true;
        if (Ready)
            CrazySDK.Game.GameplayStart();
    }

    public static void GameplayStop()
    {
        if (!gameplayActive)
            return;

        gameplayActive = false;
        if (Ready)
            CrazySDK.Game.GameplayStop();
    }

    // Reserved for real achievements (a new best score), as CrazyGames asks
    public static void HappyTime()
    {
        if (Ready)
            CrazySDK.Game.HappyTime();
    }

    // ---------------- Ads ----------------
    // The SDK itself mutes the audio and pauses the game while an ad is on screen.

    public static bool AdsAvailable => Ready;

    /// <summary>Rewarded ad. onReward runs only if the ad was watched to the end.</summary>
    public static void ShowRewarded(Action onReward, Action onFail = null)
    {
        if (!Ready)
        {
            onFail?.Invoke();
            return;
        }

        try
        {
            CrazySDK.Ad.RequestAd(
                CrazyAdType.Rewarded,
                () => { },
                error =>
                {
                    Debug.Log("Rewarded ad error: " + error);
                    onFail?.Invoke();
                },
                () => onReward?.Invoke());
        }
        catch (Exception e)
        {
            Debug.LogWarning("Rewarded ad request failed: " + e.Message);
            onFail?.Invoke();
        }
    }

    /// <summary>Midgame ad at a natural break. onDone always runs, with or without an ad.</summary>
    public static void ShowMidgame(Action onDone)
    {
        if (!Ready || Time.realtimeSinceStartup - lastMidgameTime < midgameMinInterval)
        {
            onDone?.Invoke();
            return;
        }

        lastMidgameTime = Time.realtimeSinceStartup;

        try
        {
            CrazySDK.Ad.RequestAd(
                CrazyAdType.Midgame,
                () => { },
                error =>
                {
                    Debug.Log("Midgame ad error: " + error);
                    onDone?.Invoke();
                },
                () => onDone?.Invoke());
        }
        catch (Exception e)
        {
            Debug.LogWarning("Midgame ad request failed: " + e.Message);
            onDone?.Invoke();
        }
    }
}
