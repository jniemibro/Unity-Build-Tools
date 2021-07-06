// Reference: https://docs.unity3d.com/Manual/BuildPlayerPipeline.html

using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

using UnityEditor.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.AddressableAssets;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.AnalyzeRules;
using UnityEditor.AddressableAssets.Build.BuildPipelineTasks;
using UnityEditor.AddressableAssets.Build.DataBuilders;

// TODO: Demo build types?

namespace NBROS.Builds
{
    [InitializeOnLoad]
    /// <summary>
    /// Post process builds. Tools for automating builds.
    /// </summary>
    public static class BuildTools
    {
        #region CTOR

        static BuildTools()
        {
            lastBuildTarget = EditorUserBuildSettings.activeBuildTarget;
            lastBuildTargetGroup = BuildPipeline.GetBuildTargetGroup(lastBuildTarget);
            lastScriptDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(lastBuildTargetGroup);
            lastBuildMode = BuildMode.None;
            LogBuildMode(lastBuildTarget, lastBuildTargetGroup);
            //m_Logger.logEnabled = false;
        }

        #endregion

        #region INIT

        static void LogBuildMode(BuildTarget target, BuildTargetGroup group)
        {
            string dfines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            m_Logger.Log(LOG_TAG, string.Format("Current build mode is {0}\nDefines = {1}", target, dfines));
        }

        #endregion

        static string GetDefaultBuildPath()
        {
            // In Assets folder (in editor)
            string path = Application.dataPath;
            // traverse to same directory as the Assets folder (next to it)
            string parentName = Directory.GetParent(path).FullName;
            path = Path.Combine(parentName, "Builds");
            // add build type/name folder
            //path = Path.Combine(path, GetBuildFolderName());
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        static string GetBuildFolderName(BuildMode buildMode, BuildTarget buildTarget, bool isDevBuild)
        {
            const string FORMAT = "{0} v{1} {2}";
            string folderName = string.Format(FORMAT,
                                              Application.productName,
                                              Application.version,
                                              buildTarget
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
            if (isDevBuild)
                folderName += " [Dev]";

            return folderName;
        }

        #region BUILD_COMMANDS

        // TODO: test multi-build
        [MenuItem("Tools/NBROS/Build/All Desktop DRM-Free", priority = 5000)]
        public static void BuildAllDesktop_DRMFree()
        {
            BuildPlayerOptions options;

            options = GetBuildPlayerOptions(BuildMode.None, BuildTarget.StandaloneOSX, GetDefaultBuildPath(), false, GetBuildSettingsScenes());
            Build(options);

            options = GetBuildPlayerOptions(BuildMode.None, BuildTarget.StandaloneWindows64, GetDefaultBuildPath(), false, GetBuildSettingsScenes());
            Build(options);

            options = GetBuildPlayerOptions(BuildMode.None, BuildTarget.StandaloneLinux64, GetDefaultBuildPath(), false, GetBuildSettingsScenes());
            Build(options);
        }

        [MenuItem("Tools/NBROS/Build/macOS/DRM Free", priority = 5000)]
        public static void BuildMacOS()
        {
            BuildPlayerOptions options = GetBuildPlayerOptions(BuildMode.None, BuildTarget.StandaloneOSX, GetDefaultBuildPath(), false, GetBuildSettingsScenes());
            Build(options);
        }

        [MenuItem("Tools/NBROS/Build/Windows/32-bit DRM Free", priority = 5000)]
        public static void BuildWindows32()
        {
            BuildPlayerOptions options = GetBuildPlayerOptions(BuildMode.None, BuildTarget.StandaloneWindows, GetDefaultBuildPath(), false, GetBuildSettingsScenes());
            Build(options);
        }

        [MenuItem("Tools/NBROS/Build/Windows/64-bit DRM Free", priority = 5000)]
        public static void BuildWindows64()
        {
            BuildPlayerOptions options = GetBuildPlayerOptions(BuildMode.None, BuildTarget.StandaloneWindows64, GetDefaultBuildPath(), false, GetBuildSettingsScenes());
            Build(options);
        }

        [MenuItem("Tools/NBROS/Build/Linux/64-bit DRM Free", priority = 5000)]
        public static void BuildLinux64()
        {
            BuildPlayerOptions options = GetBuildPlayerOptions(BuildMode.None, BuildTarget.StandaloneLinux64, GetDefaultBuildPath(), false, GetBuildSettingsScenes());
            Build(options);
        }

        [MenuItem("Tools/NBROS/Build/Nintendo Switch", priority = 5000)]
        public static void BuildNintendoSwitch()
        {
            BuildPlayerOptions options = GetBuildPlayerOptions(BuildMode.Switch, BuildTarget.Switch, GetDefaultBuildPath(), false, GetBuildSettingsScenes());
            Build(options);
        }

        static void BuildMacOS_DRMFree()
        {
            BuildPlayerOptions options = GetBuildPlayerOptions(BuildMode.None, BuildTarget.StandaloneOSX, GetDefaultBuildPath(), false, GetBuildSettingsScenes());
            PlayerSettings.runInBackground = false;
            Build(options);
        }

        static void BuildWindows32_DRMFree()
        {
            BuildPlayerOptions options = GetBuildPlayerOptions(BuildMode.None, BuildTarget.StandaloneWindows, GetDefaultBuildPath(), false, GetBuildSettingsScenes());
            PlayerSettings.runInBackground = false;
            Build(options);
        }

        static void BuildWindows64_DRMFree()
        {
            BuildPlayerOptions options = GetBuildPlayerOptions(BuildMode.None, BuildTarget.StandaloneWindows64, GetDefaultBuildPath(), false, GetBuildSettingsScenes());
            PlayerSettings.runInBackground = false;
            Build(options);
        }

        #endregion

        #region HELPERS

        static BuildPlayerOptions GetBuildPlayerOptions(BuildMode mode, BuildTarget buildTarget, string path, bool isDevBuild, params string[] scenes)
        {
            if (scenes.Length <= 0)
            {
                m_Logger.LogError(LOG_TAG, "Trying to get build player options with no passed in scenes!");
            }

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                locationPathName = Path.Combine(path, Path.Combine(GetBuildFolderName(mode, buildTarget, isDevBuild), Application.productName)),
                scenes = scenes,
                targetGroup = BuildTargetGroup.Standalone,
                target = buildTarget
            };
            return options;
        }

        /// <summary>
        /// Returns the scenes out of the editor build settings.
        /// </summary>
        /// <returns>The build settings scenes.</returns>
        internal static string[] GetBuildSettingsScenes()
        {
            string[] levels = new string[EditorBuildSettings.scenes.Length];
            for (int i = 0; i < levels.Length; i++)
                levels[i] = EditorBuildSettings.scenes[i].path;
            return levels;
        }

        internal static string RequestBuildPath()
        {
            string path = EditorUtility.SaveFolderPanel("Choose desired directory for build", "", "");
            return string.IsNullOrEmpty(path) ? string.Empty : path;
        }

        // TODO: Addressables setup
        public static void PrepareRuntimeData(BuildPlayerOptions buildOptions)
        {
            m_Logger.Log(LOG_TAG, string.Format("Beginning clean and build of Addressables..."));

            //var settings = AddressableAssetSettingsDefaultObject.Settings;
            //Debug.Assert(settings);

            //var context = new AddressablesDataBuilderInput(settings);
            /*var context = new AddressablesBuildDataBuilderContext(
                settings,
                buildOptions.targetGroup,
                buildOptions.target,
                (buildOptions.options & BuildOptions.Development) != BuildOptions.None,
                false,
                version ?? settings.PlayerBuildVersion);*/

            // Rebuild/Update every time, works?
            AddressableAssetSettings.CleanPlayerContent();
            AddressableAssetSettings.BuildPlayerContent();

            m_Logger.Log(LOG_TAG, string.Format("Addressables built successfully"));

            // what does this do?
            //AddressablesPlayerBuildResult r = settings.ActivePlayerDataBuilder.BuildData<AddressablesPlayerBuildResult>(context);

            //m_Logger.Log(LOG_TAG, string.Format("Addressables Player Build Result =\nBuild Time = {0}s\nError = {1}\nLocation Count = {2}\nOutputPath = {3}\nRegistry = {4}",
            //    r.Duration, r.Error, r.LocationCount, r.OutputPath, r.FileRegistry));
        }

        static void Build(BuildPlayerOptions options)
        {
            if (string.IsNullOrEmpty(options.locationPathName))
            {
                m_Logger.LogError(LOG_TAG, string.Format("{0} build failed. Invalid path.", options.target.ToString()));
                return;
            }

            //options.target = target;
            //var results = BuildPipeline.BuildPlayer(options);
            //m_Logger.Log(LOG_TAG, string.Format("{0}\n<color=green>Build target {1} complete.</color>",
            //                                    results, options.target.ToString()));

            // always use last build mode?
            Build(options.target, lastBuildMode, VersionIncrementType.BuildNumber, options);
        }

        internal static void Build(BuildTarget target, BuildMode buildMode, VersionIncrementType versionIncrement, BuildPlayerOptions options)
        {
            if (string.IsNullOrEmpty(options.locationPathName))
            {
                m_Logger.LogError(LOG_TAG, string.Format("{0} build failed. Invalid path.", target.ToString()));
                return;
            }

            // HACK: rename various plugin directories to exclude them
            string pluginsPath = Path.Combine(Application.dataPath, "Plugins");
            string steamPath = Path.Combine(pluginsPath, "Steam");
            if (Directory.Exists(steamPath))
            {
                switch (buildMode)
                {
                    case BuildMode.Steam:
                        break;
                    default:
                        {
                            // move steam plugin folder to 'special'/ignored folder
                            File.Move(steamPath, steamPath + "~");
                            break;
                        }
                }
                AssetDatabase.Refresh();
            }

            // update scripting defines
            SwapDefines(options.targetGroup, buildMode);

            // 
            options.target = target;
            //EditorUserBuildSettings.SetBuildLocation(options.target, options.locationPathName);

            // prepare various runtime data such as Addressables or Asset Bundles
            PrepareRuntimeData(options);

            // Commence Build //
            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            // Log results
            switch (summary.result)
            {
                case BuildResult.Succeeded:
                    {
                        m_Logger.Log(LOG_TAG, string.Format("✔ Build succeeded: Result = {0}\nDuration = {1}\nSize = {2}\nOutput Path = {3}\nBuild Warnings = {4}",
                                                        summary.result,
                                                        summary.totalTime,
                                                        summary.totalSize.ToSize(ByteSizeExtensions.SizeUnits.MB),
                                                        summary.outputPath,
                                                        summary.totalWarnings));
                        break;
                    }
                default:
                    {
                        m_Logger.Log(LOG_TAG, string.Format("✖ Build failed: Result = {0}\nErrors = {1}, Warnings = {2}",
                                                            summary.result,
                                                            summary.totalErrors,
                                                            summary.totalWarnings));
                        break;
                    }
            }

            //m_Logger.Log(LOG_TAG, string.Format("{0}\n<color=green>Build target {1} complete.</color>",
            //                        results, target.ToString()));

            // Increment non-build version numbers (Major/Minor/Patch)
            // ignore build number increments, that occurs in OnPostProcessBuild()
            switch (versionIncrement)
            {
                case VersionIncrementType.None:
                case VersionIncrementType.BuildNumber:
                    break;
                default:
                    IncrementVersionNumber(versionIncrement);
                    break;
            }

            // revert build settings back to what they were, before the build
            RevertActiveBuildTarget();
            RevertDefines();

            string revertSteamPath = steamPath + "~";
            // reverting excluded directory names back to normal
            if (Directory.Exists(revertSteamPath))
            {
                // revert file names back
                switch (buildMode)
                {
                    case BuildMode.Steam:
                        break;
                    default:
                        {
                            // move steam plugin folder to 'special'/ignored folder
                            File.Move(revertSteamPath, steamPath);
                            break;
                        }
                }
                AssetDatabase.Refresh();
            }
        }

        #endregion

        #region PER_PLATFORM_COMPILER_DEFINES

        static void RevertDefines()
        {
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);

            SwapDefines(group, lastScriptDefines);
        }

        static void SwapDefines(BuildTargetGroup buildTargetGroup, string newDefines, bool perpetual = false)
        {
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            // remove all defines in lookup, so we leave misc unity ones, such as POST_PROCESSING_STACK...etc
            var enumerator = definesLookup.GetEnumerator();
            while (enumerator.MoveNext())
            {
                defines = RemoveCompilerDefines(defines, enumerator.Current.Value);
            }
            // add new
            defines = AddCompilerDefines(defines, newDefines);

            m_Logger.Log(LOG_TAG, "Compiling " + buildTargetGroup + " with DEFINE: '" + defines + "'");
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);

            if (perpetual)
                lastScriptDefines = defines;
        }

        internal static void SwapDefines(BuildTargetGroup buildTargetGroup, BuildMode mode, bool perpetual = false)
        {
            string test;
            if (!definesLookup.TryGetValue(mode, out test))
                return;

            SwapDefines(buildTargetGroup, test, perpetual);
        }

        internal static void SwapStandaloneDefines(BuildMode mode, bool perpetual = false)
        {
            SwapDefines(BuildTargetGroup.Standalone, mode, perpetual);
        }

        // STEAM
#if UNITY_STANDALONE && !STEAM_BUILD
        [MenuItem("Tools/NBROS/Build/Modes/Switch to Steam")]
#endif
        public static void SwitchToSteam()
        {
            SwapStandaloneDefines(BuildMode.Steam);
        }

        // GOG
#if UNITY_STANDALONE && !GOG_BUILD
        [MenuItem("Tools/NBROS/Build/Modes/Switch to GOG")]
#endif
        public static void SwitchToGOG()
        {
            SwapStandaloneDefines(BuildMode.GOG);
        }

        // ARCADE
#if UNITY_STANDALONE && !ARCADE_BUILD
        [MenuItem("Tools/NBROS/Build/Modes/Switch to Arcade")]
#endif
        public static void SwitchToArcade()
        {
            SwapStandaloneDefines(BuildMode.Arcade);
        }

        // NO PLATFORM / DRM-FREE
#if UNITY_STANDALONE && (STEAM_BUILD || GOG_BUILD || ARCADE_BUILD)
    [   MenuItem("Tools/NBROS/Build/Modes/Switch to No Platform")]
#endif
        public static void SwitchToNoPlatform()
        {
            SwapStandaloneDefines(BuildMode.None);
        }

        #endregion

        #region COMPILER_DEFINES

        static string AddCompilerDefines(string defines, params string[] toAdd)
        {
            List<string> splitDefines = new List<string>(defines.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries));
            foreach (var add in toAdd)
                if (!splitDefines.Contains(add))
                    splitDefines.Add(add);

            return string.Join(";", splitDefines.ToArray());
        }

        static string RemoveCompilerDefines(string defines, params string[] toRemove)
        {
            List<string> splitDefines = new List<string>(defines.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries));
            foreach (var remove in toRemove)
                splitDefines.Remove(remove);

            return string.Join(";", splitDefines.ToArray());
        }

        #endregion

        #region INCREMENT_BUILD_NUMBER

        [MenuItem("Tools/NBROS/Build/Misc/Increment Build Number")]
        public static void IncrementBuildNumber()
        {
            IncrementVersionNumber(VersionIncrementType.BuildNumber);
        }

        internal static void IncrementVersionNumber(VersionIncrementType type = VersionIncrementType.BuildNumber)
        {
            if (type == VersionIncrementType.None)
                return;

            const int MAX_INDEX = 3;
            // 0 = major, 1 = minor, 2 = patch, 3 = build number
            int maxIndex = MAX_INDEX;
            int index = MAX_INDEX;

            // e.g. v1.0.1, no build number
            string currentVersion = Application.version;
            //string currentVersion = PlayerSettings.bundleVersion;
            string[] parts = currentVersion.Split('.', 'b');
            List<string> temp = new List<string>(parts);

            // if no build number, add a zero on the end for it
            if (temp.Count <= MAX_INDEX)
            {
                if (!currentVersion.Contains("b"))
                    temp.Add("0");
            }
            else
            {
                while (temp.Count > MAX_INDEX + 1)
                {
                    temp.RemoveAt(temp.Count - 1);
                }
            }

            // start max index off as last possible number index
            maxIndex = Mathf.Min(MAX_INDEX, temp.Count - 1);
            switch (type)
            {
                // e.g. v1
                case VersionIncrementType.MajorUpdate:
                    index = 0;
                    while (index >= maxIndex)
                    {
                        temp.Insert(maxIndex, "0");
                        maxIndex++;
                    }
                    break;
                // v1.1
                case VersionIncrementType.MinorUpdate:
                    index = 1;
                    while (index >= maxIndex)
                    {
                        temp.Insert(maxIndex, "0");
                        maxIndex++;
                    }
                    break;
                // v1.1.1
                case VersionIncrementType.Patch:
                    index = 2;
                    while (index >= maxIndex)
                    {
                        temp.Insert(maxIndex, "0");
                        maxIndex++;
                    }
                    break;
                // v1.1.1b1
                default:
                    if (maxIndex <= 0)
                    {
                        temp.Insert(maxIndex, "0");
                        maxIndex++;
                    }
                    index = maxIndex;
                    break;
            }
            // update max index to at least encompass the desired index
            maxIndex = Mathf.Max(index, maxIndex);
            parts = temp.ToArray();

            for (int i = 0; i < parts.Length; i++)
            {
                if (i == index)
                {
                    int desiredNumber = int.Parse(parts[i]);
                    desiredNumber++;
                    parts[i] = desiredNumber.ToString();
                }
                else
                {
                    // reset numbers after desired index
                    if (index < i)
                        parts[i] = "0";
                }
            }

            // last number should be the build number
            string buildNumberString = parts[parts.Length - 1];
            // e.g. v1.0.1b5, new version adds build number on
            string newVersion = string.Empty;
            //newVersion = string.Format("{0}.{1}.{2}b{3}");
            for (int i = 0; i < parts.Length; i++)
            {
                if (i == 0)
                    newVersion += parts[i];
                else if (i < maxIndex)
                    newVersion += "." + parts[i];
                else // last number should be the build number
                    newVersion += "b" + parts[i];
            }
            //newVersion += buildNumberString;

            // ANDROID Specififc: generate version code for Android...
            int newVersionCode = int.Parse(newVersion.Replace("b", "").Replace(".", ""));

            UpdateVersions(currentVersion, newVersion, buildNumberString, newVersionCode);
        }

        static void UpdateVersions(string oldVersion, string newVersion, string buildNumber, int newVersionCode)
        {
            m_Logger.Log(LOG_TAG, "IncrementBuild: Old = " + oldVersion + ",  New = " + newVersion + "\nNew Version Code = " + newVersionCode);

            // TODO: test these changes
            //PlayerSettings.protectGraphicsMemory = true;

            // Updating versions/build numbers //
            PlayerSettings.bundleVersion = newVersion;

            // update various platform's build numbers
            PlayerSettings.macOS.buildNumber = buildNumber;
            PlayerSettings.iOS.buildNumber = buildNumber;
            PlayerSettings.tvOS.buildNumber = buildNumber;

            // unique console adjustments
            PlayerSettings.PS4.masterVersion = newVersion;
            PlayerSettings.PS4.appVersion = newVersion;

            PlayerSettings.XboxOne.Version = newVersion;

            PlayerSettings.Switch.displayVersion = newVersion;
            PlayerSettings.Switch.releaseVersion = newVersion;

            // Android wants a special code
            PlayerSettings.Android.bundleVersionCode = newVersionCode;
        }

        #endregion

        internal static void SwapActiveBuildTarget(BuildTarget buildTarget, BuildTargetGroup buildTargetGroup, BuildMode buildMode)
        {
            if (!EditorUserBuildSettings.SwitchActiveBuildTargetAsync(buildTargetGroup, buildTarget))
            {

            }
            else
            {
                lastBuildTarget = buildTarget;
                lastBuildTargetGroup = buildTargetGroup;
                lastBuildMode = buildMode;
            }
        }

        static void RevertActiveBuildTarget()
        {
            if (!EditorUserBuildSettings.SwitchActiveBuildTargetAsync(lastBuildTargetGroup, lastBuildTarget))
            {
                Debug.LogError("Failed to revert build target to " + lastBuildTarget);
            }
            //BuildTools.SwapDefines(lastBuildTargetGroup, lastBuildMode);
        }

        #region POST_PROCESS_BUILDS

        [PostProcessBuildAttribute(1080)]
        public static void OnPostProcessBuild(BuildTarget target, string path)
        {
            m_Logger.Log(LOG_TAG, string.Format("OnPostProcessBuild {0} {1}", target, path));

            CopyMiscFiles(target, path);
            UpdateReadMe(path);

            // +1 to build number, so next build will be different
            IncrementBuildNumber();

            // reveal in Explorer/Finder
            DirectoryInfo directory = new DirectoryInfo(path);
            directory = directory.Parent;
            System.Diagnostics.Process.Start(directory.FullName);

            // done beep
            EditorApplication.Beep();
        }

        static void CopyMiscFiles(BuildTarget target, string path)
        {
            DirectoryInfo buildFolder = new DirectoryInfo(path);
            buildFolder = buildFolder.Parent;

            // adjusting path, not needed?
            switch (target)
            {
                case BuildTarget.StandaloneOSX:
                    {
                        break;
                    }
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    {
                        //path = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + "_Data");
                        break;
                    }
                default:
                    break;
            }

            // copy all files in Assets/Include to build folder
            DirectoryInfo include = new DirectoryInfo(Path.Combine(Application.dataPath, "Include"));
            if (!include.Exists)
                return;
            foreach (FileInfo info in include.GetFiles())
            {
                if (!ShouldCopy(info.Extension))
                    continue;
                FileUtil.CopyFileOrDirectory(info.FullName, Path.Combine(buildFolder.FullName, info.Name));
            }
        }

        static void UpdateReadMe(string path)
        {
            DirectoryInfo buildFolder = new DirectoryInfo(path);
            buildFolder = buildFolder.Parent;

            string filePath = Path.Combine(buildFolder.FullName, READ_ME_FILE);
            string currentContent = String.Empty;
            if (File.Exists(filePath))
            {
                currentContent = File.ReadAllText(filePath);
            }
            else
            {
                Debug.LogError("Failed to find file at " + filePath);
            }
            string newContent = string.Format(READ_ME_FORMAT,
                                              Application.productName,
                                              Application.companyName,
                                              Application.version, DateTime.Now.Year
                                             );
            File.WriteAllText(filePath, newContent + currentContent);
            // set READ ME to be read only...?
            /*FileInfo fInfo = new FileInfo(filePath)
            {
                IsReadOnly = true
            };
            //File.SetAttributes(filePath, File.GetAttributes(filePath) | FileAttributes.ReadOnly);*/
        }

        static bool ShouldCopy(string extension)
        {
            return !IGNORE_FILE_EXTENSIONS.Contains(extension);
        }

        #endregion

        static bool HasInvalidPathCharacters(this char[] target)
        {
            char[] invalids = Path.GetInvalidPathChars();
            for (int i = 0; i < target.Length; i++)
            {
                for (int k = 0; k < invalids.Length; k++)
                {
                    if (invalids[k] == invalids[i])
                        return true;
                }
            }
            return false;
        }

        internal static string GetDefines(BuildMode buildMode)
        {
            string s;
            if (definesLookup.TryGetValue(buildMode, out s))
                return s;
            else
                return string.Empty;
        }

        #region FIELDS

        static readonly Dictionary<BuildMode, string> definesLookup = new Dictionary<BuildMode, string>()
        {
            {BuildMode.None, string.Empty},
            {BuildMode.Arcade, ARCADE_BUILD},
            {BuildMode.Steam, STEAM_BUILD},
            {BuildMode.GOG, GOG_BUILD},
            {BuildMode.Switch, string.Empty}
        };

        static readonly string MAC_PLUGINS_PATH = Path.Combine("Contents", "Plugins");
        static readonly string WINDOWS_PLUGINS_PATH = Path.Combine(string.Format("{0}_Data", Application.productName), "Plugins");
        //static readonly string LINUX_PLUGINS_PATH = Path.Combine(string.Format("{0}_Data", Application.productName), "Plugins");

        static ILogger m_Logger = new Logger(Debug.unityLogger);

        static string lastScriptDefines;
        static BuildMode lastBuildMode;
        static BuildTarget lastBuildTarget;
        static BuildTargetGroup lastBuildTargetGroup;

        const string STEAM_BUILD = "STEAM_BUILD";
        const string GOG_BUILD = "GOG_BUILD";
        const string ARCADE_BUILD = "ARCADE_BUILD";

        // GAME_NAME
        // ©COMPANY
        // v1.1.1b1 (2019)
        const string READ_ME_FORMAT = "{0}\n©{1}\nv{2} ({3})\n";
        const string READ_ME_FILE = "READ ME.txt";

        const string LOG_TAG = LOG_ICON + "BUILD TOOLS";
        const string LOG_ICON = "⚙";

        // exclude certain file types in Include directory from builds
        static readonly List<string> IGNORE_FILE_EXTENSIONS = new List<string>
        {
            {".meta"}
        };

        #endregion
    }
}
