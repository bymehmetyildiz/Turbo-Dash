using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Follows the CrazyGames "mute audio" setting: when the portal mutes the game, all game audio goes silent,
/// and it comes back when the portal unmutes. The game's own volume sliders are left untouched.
/// </summary>
public class PlatformAudio : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern int CrazyIsAudioMuted();
#endif

    private const float pollInterval = 0.25f;

    public static bool MutedByPlatform { get; private set; }

    private float nextPollTime;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Boot()
    {
        MutedByPlatform = false;

        GameObject host = new GameObject("PlatformAudio");
        host.AddComponent<PlatformAudio>();
        DontDestroyOnLoad(host);
    }

    private void Update()
    {
        if (Time.unscaledTime < nextPollTime)
            return;

        nextPollTime = Time.unscaledTime + pollInterval;

        if (!Platform.Ready)
            return;

        bool muted = ReadMuted();

        // The SDK silences and restores the listener itself around an ad; do not fight it while one is showing.
        if (Platform.AdInProgress)
        {
            MutedByPlatform = muted;
            return;
        }

        if (muted)
        {
            // Re-asserted on every poll, so nothing (for example the end of an ad) can switch the sound back on
            if (AudioListener.volume != 0f)
                AudioListener.volume = 0f;
        }
        else if (MutedByPlatform || AudioListener.volume == 0f)
        {
            AudioListener.volume = 1f;
        }

        MutedByPlatform = muted;
    }

    private static bool ReadMuted()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return CrazyIsAudioMuted() == 1;
#else
        return false;
#endif
    }
}
