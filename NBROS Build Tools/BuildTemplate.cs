using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace NBROS.Builds
{
    [CreateAssetMenu(fileName = "BuildTemplate", menuName = "Builds/Build Template", order = 1)]
    public class BuildTemplate : ScriptableObject
    {
        #region BUILD

        //[ContextMenu("Build Scripts-Only")]
        internal void BuildScriptsOnly()
        {
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                options = BuildOptions.BuildScriptsOnly,
                target = buildTarget
            };
            Build(buildPlayerOptions);
        }

        //[ContextMenu("Build Asset Bundles + Game")]
        internal void BuildGameWithAssetBundles()
        {
            // Building asset bundles not implemented. Not needed with Addressables?
            EditorApplication.Beep();
            Debug.LogError("Not implemented.");
            //BuildGame();
        }

        //[ContextMenu("Build Game")]
        internal void BuildGame()
        {
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                targetGroup = GetBuildTargetGroup(),
                target = buildTarget
            };
            Build(buildPlayerOptions);
        }

        void Build(BuildPlayerOptions buildPlayerOptions)
        {
            if (isDevBuild)
                buildPlayerOptions.options |= BuildOptions.Development;
            buildPlayerOptions.options |= BuildOptions.ShowBuiltPlayer;

            // set building scenes to assigned asset, if one, otherwise all scenes
            buildPlayerOptions.scenes = scenes ? scenes.scenes : BuildTools.GetBuildSettingsScenes();
            // request a build path
            string path = BuildTools.RequestBuildPath();

            // add a folder onto build path
            string folderName = GetBuildFolderName(buildPlayerOptions);
            path = Path.Combine(path, folderName);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            // name of actual application, just use productName?
            //path = Path.Combine(path, string.Format("{0}{1}", Application.productName, GetApplicationSuffix()));
            path = Path.Combine(path, Application.productName);
            buildPlayerOptions.locationPathName = path;

            BuildTools.Build(buildTarget, buildMode, versionIncrement, buildPlayerOptions);
        }

        #endregion

        //[ContextMenu("Go to PlayerSettings")]
        internal void GoToPlayerSettings()
        {
            SettingsService.OpenProjectSettings("Project/Player");
        }

        internal void SwapScriptingDefines()
        {
            BuildTools.SwapStandaloneDefines(buildMode, true);
        }

        internal void SwapActiveBuildTarget()
        {
            BuildTools.SwapActiveBuildTarget(buildTarget, GetBuildTargetGroup(), buildMode);
        }

        string GetBuildFolderName(BuildPlayerOptions buildPlayerOptions)
        {
            const string FORMAT = "{0} v{1} {2}";
            string folderName = string.Format(FORMAT,
                                              Application.productName,
                                              Application.version,
                                              buildPlayerOptions.target
                                              );
            // append build mode
            switch (buildMode)
            {
                case BuildMode.None:
                    break;
                default:
                    folderName += string.Format(" {0}", buildMode);
                    break;
            }
            // append a suffix for dev builds
            if (buildPlayerOptions.options.HasFlag(BuildOptions.Development))
                folderName += " [Dev]";

            return folderName;
        }

        BuildTargetGroup GetBuildTargetGroup()
        {
            BuildTargetGroup result;
            switch (buildTarget)
            {
                case BuildTarget.iOS:
                    result = BuildTargetGroup.iOS;
                    break;
                case BuildTarget.Switch:
                    result = BuildTargetGroup.Switch;
                    break;
                case BuildTarget.WebGL:
                    result = BuildTargetGroup.WebGL;
                    break;
                default:
                    result = BuildTargetGroup.Standalone;
                    break;
            }
            return result;
        }

        string GetApplicationSuffix()
        {
            const string DEV = "Dev";

            string s = string.Empty;

            string versionEdit = Application.version.Replace('.', '_');
            s += string.Format(" v{0}", versionEdit);

            if (isDevBuild)
                s += string.Format(" [{0}]", DEV);

            switch (buildPhase)
            {
                case BuildPhase.Release:
                    break;
                default:
                    s += string.Format(" [{0}]", buildPhase);
                    break;
            }

            // add file suffix, if one
            string test;
            if (fileSuffixLookup.TryGetValue(buildTarget, out test))
                s += test;

            return s;
        }

        #region FIELDS

        static readonly Dictionary<BuildTarget, string> fileSuffixLookup = new Dictionary<BuildTarget, string>()
        {
            {BuildTarget.StandaloneWindows, ".exe"},
            {BuildTarget.StandaloneWindows64, ".exe"},
        };

        [SerializeField]
        internal BuildTarget buildTarget;
        [SerializeField]
        internal BuildMode buildMode;
        [SerializeField]
        internal BuildPhase buildPhase = BuildPhase.Release;

        [Space()]
        [SerializeField]
        internal VersionIncrementType versionIncrement = VersionIncrementType.BuildNumber;

        [Space()]
        [SerializeField]
        internal bool isDevBuild;

        [Header("Optional")]
        [SerializeField] BuildSceneTemplate scenes;

        const string LOG_TAG = "BUILD TEMPLATE";

        #endregion
    }

    public static class BuildOptionsExtensions
    {
        public static bool HasFlag(this BuildOptions flags, BuildOptions flag)
        {
            return (flags & flag) != 0;
        }
    }
}