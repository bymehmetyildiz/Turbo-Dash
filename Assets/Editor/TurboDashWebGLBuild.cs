#if UNITY_EDITOR && TURBODASH_CRAZYGAMES
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace TurboDash.Editor
{
    /// <summary>
    /// Unity 2022.3 WebGL release builds. Place this file in Assets/Editor.
    /// Switch the active build target to WebGL and save enabled scenes before use.
    /// Release settings persist; existing exports are preserved.
    /// </summary>
    public static class TurboDashWebGLBuild
    {
        [MenuItem("Tools/Turbo Dash/Build Optimized WebGL")]
        public static void BuildFromMenu()
        {
            try
            {
                BuildTo(CreateOutputPath());
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        // Use -batchmode -quit -buildTarget WebGL -executeMethod
        // TurboDash.Editor.TurboDashWebGLBuild.BuildBatch. Exceptions remain
        // uncaught so Unity returns a failing exit code to the caller.
        public static void BuildBatch()
        {
            BuildTo(CreateOutputPath());
        }

        private static string CreateOutputPath()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string folder = "WebGL-Optimized-" + DateTime.Now.ToString("yyyyMMdd-HHmmss")
                + "-" + Guid.NewGuid().ToString("N").Substring(0, 6);
            return Path.Combine(projectRoot, "Builds", folder);
        }

        /// <summary>
        /// Builds enabled scenes into a new output directory, writing a sibling
        /// .report.txt file. Throws on invalid input or a failed build; suitable
        /// for use from another editor script or a batch-mode entry point.
        /// </summary>
        public static BuildReport BuildTo(string outputDirectory)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling
                || BuildPipeline.isBuildingPlayer)
                throw new InvalidOperationException("Wait until Unity is idle and out of Play Mode before building.");
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
                throw new InvalidOperationException("Switch the active build target to WebGL in Build Settings first.");
            if (string.IsNullOrWhiteSpace(outputDirectory))
                throw new ArgumentException("Specify a new build output directory.", nameof(outputDirectory));

            string outputPath = Path.GetFullPath(outputDirectory)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string reportPath = outputPath + ".report.txt";
            if (Directory.Exists(outputPath) || File.Exists(outputPath) || File.Exists(reportPath))
                throw new IOException("Build output or report already exists. Choose a new directory: " + outputPath);

            string[] scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled)
                .Select(scene => scene.path).ToArray();
            if (scenes.Length == 0)
                throw new InvalidOperationException("Enable at least one scene in Build Settings.");
            foreach (string scenePath in scenes)
            {
                if (!File.Exists(scenePath))
                    throw new FileNotFoundException("An enabled build scene does not exist.", scenePath);
            }
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                Scene scene = SceneManager.GetSceneAt(index);
                if (scene.isDirty && scenes.Contains(scene.path))
                    throw new InvalidOperationException("Save this enabled scene before building: " + scene.path);
            }

            Directory.CreateDirectory(outputPath);
            var timer = Stopwatch.StartNew();
            BuildReport report = null;
            Exception failure = null;
            try
            {
                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
                PlayerSettings.WebGL.decompressionFallback = false;
                PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Off;
                PlayerSettings.stripEngineCode = true;
                PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.WebGL, ManagedStrippingLevel.Medium);
                PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.WebGL, Il2CppCodeGeneration.OptimizeSize);
                UnityEditor.WebGL.UserBuildSettings.codeOptimization = UnityEditor.WebGL.WasmCodeOptimization.DiskSizeLTO;
                EditorUserBuildSettings.development = false;
                AssetDatabase.SaveAssets();

                report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = scenes,
                    target = BuildTarget.WebGL,
                    locationPathName = outputPath,
                    options = BuildOptions.DetailedBuildReport
                });
                if (report == null || report.summary.result != BuildResult.Succeeded)
                    throw new BuildFailedException("Optimized WebGL build failed. See the Unity Console and " + reportPath);
            }
            catch (Exception exception)
            {
                failure = exception;
                throw;
            }
            finally
            {
                timer.Stop();
                try
                {
                    WriteReport(outputPath, reportPath, scenes, report, timer.Elapsed, failure);
                }
                catch (Exception reportError)
                {
                    Debug.LogError("Could not write WebGL build report: " + reportError);
                }
            }

            Debug.Log("Optimized WebGL build succeeded in " + timer.Elapsed.TotalSeconds.ToString("F1")
                + " seconds. Output: " + outputPath + "\nReport: " + reportPath);
            return report;
        }

        private static void WriteReport(string outputPath, string reportPath, string[] scenes,
            BuildReport report, TimeSpan elapsed, Exception failure)
        {
            var text = new StringBuilder();
            text.AppendLine("Turbo Dash - Optimized WebGL build");
            text.AppendLine("Generated (UTC): " + DateTime.UtcNow.ToString("O"));
            text.AppendLine("Unity: " + Application.unityVersion);
            text.AppendLine("Output: " + outputPath);
            text.AppendLine("Result: " + (failure != null ? "Failed" : report.summary.result.ToString()));
            text.AppendLine("Elapsed seconds: " + elapsed.TotalSeconds.ToString("F1"));
            text.AppendLine("Settings: Brotli; fallback off; debug symbols off; engine stripping on;");
            text.AppendLine("          IL2CPP OptimizeSize; DiskSizeLTO; non-development.");
            text.AppendLine("Managed stripping: " + PlayerSettings.GetManagedStrippingLevel(NamedBuildTarget.WebGL));
            text.AppendLine("Enabled scenes:\n  " + string.Join("\n  ", scenes));
            if (failure != null)
                text.AppendLine("Failure: " + failure);
            if (report != null)
            {
                text.AppendLine("Build errors: " + report.summary.totalErrors);
                text.AppendLine("Build warnings: " + report.summary.totalWarnings);
            }

            FileInfo[] files = new DirectoryInfo(outputPath).GetFiles("*", SearchOption.AllDirectories);
            long totalBytes = files.Sum(file => file.Length);
            text.AppendLine();
            text.AppendLine("Actual exported files (bytes on disk, including compressed .br files):");
            foreach (FileInfo file in files.OrderByDescending(file => file.Length))
                text.AppendLine(file.Length + " bytes\t" + MiB(file.Length) + " MiB\t"
                    + file.FullName.Substring(outputPath.Length).TrimStart(Path.DirectorySeparatorChar));
            text.AppendLine("Total export: " + totalBytes + " bytes (" + MiB(totalBytes) + " MiB)");
            text.AppendLine("This report is outside the export. Upload only the build directory.");
            text.AppendLine("Disk size is not the portal's initial-load metric; verify that metric in the portal.");

            if (report != null && report.packedAssets != null)
            {
                var largest = report.packedAssets.SelectMany(pack => pack.contents)
                    .Where(asset => !string.IsNullOrEmpty(asset.sourceAssetPath))
                    .GroupBy(asset => asset.sourceAssetPath)
                    .Select(group => new { Path = group.Key, Bytes = group.Sum(asset => (long)asset.packedSize) })
                    .OrderByDescending(asset => asset.Bytes).Take(30);
                text.AppendLine();
                text.AppendLine("Largest packed assets (before final Brotli compression; totals may include shared copies):");
                foreach (var asset in largest)
                    text.AppendLine(asset.Bytes + " bytes\t" + MiB(asset.Bytes) + " MiB\t" + asset.Path);
            }
            File.WriteAllText(reportPath, text.ToString(), new UTF8Encoding(false));
        }

        private static string MiB(long bytes)
        {
            return (bytes / (1024d * 1024d)).ToString("F2");
        }
    }
}
#endif
