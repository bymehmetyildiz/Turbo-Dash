using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

/// <summary>
/// Lowers the rendering cost on phones and tablets. Desktop is left exactly as configured in the project.
/// Everything here is applied from code at startup, so no quality or scene asset has to change.
/// </summary>
public static class PerformanceSettings
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern int IsMobileBrowser();

    [DllImport("__Internal")]
    private static extern void SetMaxCanvasPixelRatio(float maxRatio);
#endif

    // Phones report a pixel ratio of 2-3, which means 4-9x the pixels of the CSS size.
    // 1.5 still looks sharp on a small screen and cuts the pixel count by more than half.
    private const float mobileMaxPixelRatio = 1.5f;

    public static bool IsMobile { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Boot()
    {
        IsMobile = DetectMobile();

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (!IsMobile)
            return;

        QualitySettings.antiAliasing = 0;                       // 4x MSAA is the single most expensive setting here
        QualitySettings.shadows = ShadowQuality.Disable;        // every shadow caster is otherwise drawn twice
        QualitySettings.skinWeights = SkinWeights.TwoBones;
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
        QualitySettings.pixelLightCount = 1;

#if UNITY_WEBGL && !UNITY_EDITOR
        SetMaxCanvasPixelRatio(mobileMaxPixelRatio);
#endif
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!IsMobile)
            return;

        foreach (Camera cam in Camera.allCameras)
        {
            cam.allowHDR = false;
            cam.allowMSAA = false;

            // The scene has a post-processing layer but no volume feeding it, so on mobile it only adds full-screen copies
            PostProcessLayer postProcess = cam.GetComponent<PostProcessLayer>();
            if (postProcess != null)
                postProcess.enabled = false;
        }
    }

    private static bool DetectMobile()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return IsMobileBrowser() == 1;
#else
        return SystemInfo.deviceType == DeviceType.Handheld;
#endif
    }
}
