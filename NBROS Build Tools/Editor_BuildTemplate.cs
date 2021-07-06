using UnityEngine;
using UnityEditor;

namespace NBROS.Builds
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BuildTemplate), true)]
    public class Editor_BuildTemplate : Editor
    {
        public override void OnInspectorGUI()
        {
            BuildTemplate a = target as BuildTemplate;
            GUI.skin.label.richText = true;

            // Display current template settings largely
            // green text matches current player settings/target
            GUILayout.Label(string.Format(FORMAT, 
                                          Application.productName,
                                          Application.companyName,
                                          GetBuildTargetString(a), 
                                          GetBuildModeString(a),
                                          Application.version + (a.isDevBuild ? " Dev" : string.Empty),
                                          a.buildPhase));

            EditorGUILayout.Space();
            // draw default stuff
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(SEPARATOR);
            // display defines
            GUILayout.Label("Defines included: " + BuildTools.GetDefines(a.buildMode));

            EditorGUILayout.LabelField(SEPARATOR);

            // Cannot build w/ certain conditions
            if (EditorApplication.isPlaying)
            {
                GUILayout.Label("In Playmode, exit to build.");
                return;
            }
            if (EditorApplication.isCompiling)
            {
                GUILayout.Label("Compiling...");
                return;
            }

            DrawTemplateButtons(a);
        }

        void DrawTemplateButtons(BuildTemplate a)
        {
            GUILayout.FlexibleSpace();

            // Build Buttons //
            //if (GUILayout.Button("Build Asset Bundles + Game"))
            //    a.BuildGameWithAssetBundles();
            if (GUILayout.Button("Build Game"))
                a.BuildGame();
            if (GUILayout.Button("Build Scripts Only"))
                a.BuildScriptsOnly();

            EditorGUILayout.Space();

            // Misc
            if (GUILayout.Button("Open Player Settings"))
                a.GoToPlayerSettings();

            EditorGUILayout.Space();

            // Swap Editor Build Targets/Defines
            if (GUILayout.Button("Swap Active Build Target"))
                a.SwapActiveBuildTarget();
            if (GUILayout.Button("Swap Scripting Defines"))
                a.SwapScriptingDefines();

            EditorGUILayout.Space();

            // uncomment to test version increment
            /*if (GUILayout.Button("Test Version Increment"))
                BuildTools.IncrementVersionNumber(a.versionIncrement);

            EditorGUILayout.Space();*/
        }

        string GetBuildModeString(BuildTemplate buildTemplate)
        {
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
            if (defines == BuildTools.GetDefines(buildTemplate.buildMode))
                return MATCH_COLOR_HTML + buildTemplate.buildMode.ToString() + "</color>";
            else
                return buildTemplate.buildMode.ToString();
        }

        string GetBuildTargetString(BuildTemplate buildTemplate)
        {
            BuildTarget _buildTarget = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup _group = BuildPipeline.GetBuildTargetGroup(_buildTarget);

            string result;
            if (_buildTarget == buildTemplate.buildTarget)
                result = MATCH_COLOR_HTML + buildTemplate.buildTarget.ToString();
            else
                result = NORMAL_COLOR_HTML + buildTemplate.buildTarget.ToString();

            if (buildTemplate.isDevBuild)
                result += " [Dev]";
            result += "</color>";
            return result;
        }

        #region FIELDS

        const string MATCH_COLOR_HTML = "<color=green>";
        const string NORMAL_COLOR_HTML = "<color=black>";

        const string SEPARATOR = "~ ~ ~ ~ ~ ~ ~";
        const string FORMAT = "<b><size=16><i>{0}</i></size></b>" +
            "\n<size=12><i>©{1}</i></size>\n" +
            "<b>\n<size=20><i>{2}</i></size>" + 
            "\n<size=16><i>{3}</i></size></b>" +
            "\n<size=10><i>{4} {5}</i></size>";

        #endregion
    }
}
