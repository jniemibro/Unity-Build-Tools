using UnityEngine;

namespace NBROS.Builds
{
    [CreateAssetMenu(fileName = "Build Scene Template", menuName = "Builds/Build Scene Template", order = 1)]
    public class BuildSceneTemplate : ScriptableObject
    {
        [SerializeField]
        internal string[] scenes;

        [ContextMenu("Get BuildSettings Scenes")]
        void GetAllScenes()
        {
            scenes = BuildTools.GetBuildSettingsScenes();
        }
    }
}