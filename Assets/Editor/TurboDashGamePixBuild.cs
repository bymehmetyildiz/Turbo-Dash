#if UNITY_EDITOR && !TURBODASH_CRAZYGAMES
using System;
using System.IO;
using System.Linq;
using GamePix.Editor;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TurboDash.Editor
{
    /// <summary>Uses the official GamePix 6.6 packager without changing texture/audio import settings.</summary>
    public static class TurboDashGamePixBuild
    {
        [MenuItem("Tools/Turbo Dash/Build GamePix")]
        public static void BuildFromMenu() { BuildBatch(); }

        public static void Configure()
        {
            if (Emscripten.editorVersion != Emscripten.pluginVersion)
                throw new BuildFailedException("The installed GamePix SDK does not match Unity's Emscripten version.");

            // Use the SDK's safe mode; preserve the project's visual and asset optimizations.
            var colorSpace = PlayerSettings.colorSpace;
            UnityConfigurator.SetWebGLSettings(true, AssetsCompression.NoOverride);
            PlayerSettings.colorSpace = colorSpace;
            PlayerSettings.SplashScreen.logos = PlayerSettings.SplashScreen.logos
                .Where(entry => !AssetDatabase.GetAssetPath(entry.logo).StartsWith("Assets/CrazySDK/", StringComparison.OrdinalIgnoreCase))
                .ToArray();
            // The SDK enables WebGL 1 on Unity 2022, which cannot render Linear color space.
            // Keep this URP project's existing WebGL 2 renderer and lighting.
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.WebGL, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.WebGL, new[] { UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 });
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.WebGL, ManagedStrippingLevel.Medium);
            UnityEditor.WebGL.UserBuildSettings.codeOptimization = UnityEditor.WebGL.WasmCodeOptimization.DiskSizeLTO;
            AssetDatabase.SaveAssets();
        }

        // -batchmode -quit -buildTarget WebGL -executeMethod TurboDash.Editor.TurboDashGamePixBuild.BuildBatch
        public static void BuildBatch()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || BuildPipeline.isBuildingPlayer)
                throw new BuildFailedException("Wait until Unity is idle before building GamePix.");
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
                throw new BuildFailedException("Switch the active build target to WebGL first.");
            string[] scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0 || scenes.Any(s => !File.Exists(s)))
                throw new BuildFailedException("Enable valid game scenes in Build Settings.");
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isDirty && scenes.Contains(scene.path))
                    throw new BuildFailedException("Save the scene before building: " + scene.path);
            }

            Configure();
            // The vendor packager clears its destination. Give it a NEW, verified child of Builds.
            string buildsRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Builds"));
            string output = Path.Combine(buildsRoot, "GamePix-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")
                + "-" + Guid.NewGuid().ToString("N").Substring(0, 6));
            if (!output.StartsWith(buildsRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                || Directory.Exists(output) || File.Exists(output))
                throw new IOException("The GamePix output must be a new directory inside Builds.");

            if (!Builder.BuildArchive(output, AssetsCompression.NoOverride, DataFileCompression.Uncompressed, false))
                throw new BuildFailedException("GamePix build failed. See the Unity log.");
            string[] packages = Directory.GetFiles(output, "*.gpx");
            if (packages.Length != 1 || new FileInfo(packages[0]).Length == 0)
                throw new BuildFailedException("GamePix did not produce a valid package.");
            File.WriteAllText(Path.Combine(buildsRoot, "GamePix-latest.txt"), output);
            Debug.Log("TURBODASH_GAMEPIX_BUILD_SUCCEEDED: " + packages[0]);
        }
    }
}
#endif
