// CrazyGames integration preserved; excluded from the default GamePix build.
#if TURBODASH_CRAZYGAMES
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

    public static bool IsPaused => AdInProgress;
    public static void SadTime() { }
    public static bool AdsAvailable => Ready;

    // True from the ad request until the SDK reports it finished or failed
    public static bool AdInProgress { get; private set; }

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
            AdInProgress = true;
            CrazySDK.Ad.RequestAd(
                CrazyAdType.Rewarded,
                () => { },
                error =>
                {
                    AdInProgress = false;
                    Debug.Log("Rewarded ad error: " + error);
                    onFail?.Invoke();
                },
                () =>
                {
                    AdInProgress = false;
                    onReward?.Invoke();
                });
        }
        catch (Exception e)
        {
            AdInProgress = false;
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
            AdInProgress = true;
            CrazySDK.Ad.RequestAd(
                CrazyAdType.Midgame,
                () => { },
                error =>
                {
                    AdInProgress = false;
                    Debug.Log("Midgame ad error: " + error);
                    onDone?.Invoke();
                },
                () =>
                {
                    AdInProgress = false;
                    onDone?.Invoke();
                });
        }
        catch (Exception e)
        {
            AdInProgress = false;
            Debug.LogWarning("Midgame ad request failed: " + e.Message);
            onDone?.Invoke();
        }
    }
}

#else
using System;
using GamePix;
using UnityEngine;

/// <summary>GamePix is the default portal. Ads reward only on the SDK success callback.</summary>
public static class Platform
{
    private const float MidgameMinInterval = 120f;
    private static float lastMidgameTime = -999f;
    private static bool gameplayActive;
    public static bool Ready => true; // Gpx initializes before the first scene.
    public static bool AdsAvailable => Ready;
    public static bool AdInProgress { get; private set; }
    public static bool IsPaused => AdInProgress || Gpx.IsGamePaused;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        gameplayActive = false;
        AdInProgress = false;
        lastMidgameTime = -999f;
    }

    // The official Unity plugin owns loading/gameStart lifecycle signals.
    public static void GameplayStart() { gameplayActive = true; }
    public static void GameplayStop() { gameplayActive = false; }
    public static void HappyTime() { Gpx.Events.HappyMoment(); }
    public static void SadTime() { Gpx.Events.SadMoment(); }

    public static void ShowRewarded(Action onReward, Action onFail = null)
    {
        if (AdInProgress || Gpx.IsGamePaused) { onFail?.Invoke(); return; }
        AdInProgress = true;
        bool completed = false;
        Action<bool> finish = success =>
        {
            if (completed) return;
            completed = true;
            AdInProgress = false;
            if (success) onReward?.Invoke(); else onFail?.Invoke();
        };
        try { Gpx.Ads.RewardAd(() => finish(true), () => finish(false), prompt: false); }
        catch (Exception error) { Debug.LogWarning("GamePix rewarded ad failed: " + error.Message); finish(false); }
    }

    public static void ShowMidgame(Action onDone)
    {
        // A second request must not reload a scene underneath an existing ad.
        if (AdInProgress || Gpx.IsGamePaused) return;
        if (Time.realtimeSinceStartup - lastMidgameTime < MidgameMinInterval) { onDone?.Invoke(); return; }
        lastMidgameTime = Time.realtimeSinceStartup;
        AdInProgress = true;
        bool completed = false;
        Action finish = () =>
        {
            if (completed) return;
            completed = true;
            AdInProgress = false;
            onDone?.Invoke();
        };
        try { Gpx.Ads.InterstitialAd(finish, finish); }
        catch (Exception error) { Debug.LogWarning("GamePix interstitial failed: " + error.Message); finish(); }
    }
}
#endif
