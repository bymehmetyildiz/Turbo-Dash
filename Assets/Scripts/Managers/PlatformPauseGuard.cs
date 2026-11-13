using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

// The SDK pauses time/audio; Unity's Update, UI and unscaled tweens still run.
[DefaultExecutionOrder(-10000)]
public sealed class PlatformPauseGuard : MonoBehaviour
{
    private bool paused;
    private EventSystem suspendedInput;
    private List<Tween> suspendedTweens;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Boot()
    {
        var host = new GameObject("PlatformPauseGuard");
        DontDestroyOnLoad(host);
        host.AddComponent<PlatformPauseGuard>();
    }

    private void Update()
    {
        if (Platform.IsPaused == paused) return;
        paused = Platform.IsPaused;
        if (paused)
        {
            suspendedInput = EventSystem.current;
            if (suspendedInput != null && suspendedInput.enabled) suspendedInput.enabled = false;
            else suspendedInput = null;
            suspendedTweens = DOTween.PlayingTweens();
            if (suspendedTweens != null)
                foreach (var tween in suspendedTweens) tween.Pause();
        }
        else
        {
            if (suspendedInput != null) suspendedInput.enabled = true;
            if (suspendedTweens != null)
                foreach (var tween in suspendedTweens)
                    if (tween.IsActive()) tween.Play();
            suspendedInput = null;
            suspendedTweens = null;
        }
    }
}
