using System.IO;
using UnityEditor;
using UnityEngine;

namespace Extensions.Saver
{
    public static class ExtensionSaverConfigMenu
    {
        [InitializeOnLoadMethod]
        public static void Init()
		{
            GetOrCreateConfig();
        }

        [MenuItem("Edit/UnitySaver settings...", false, 250)]
        public static void OpenOrCreateConfig()
        {
            var config = GetOrCreateConfig();

            EditorUtility.FocusProjectWindow();
            EditorWindow inspectorWindow = EditorWindow.GetWindow(typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow"));
            inspectorWindow.Focus();

            Selection.activeObject = config;
        }

        public static SaverConfig GetOrCreateConfig()
        {
            var config = Resources.Load<SaverConfig>(SaverConfig.PATH_FOR_RESOURCES_LOAD);

            if (config)
            {
                return config;
            }

            config = ScriptableObject.CreateInstance<SaverConfig>();

            string directory = Path.GetDirectoryName(SaverConfig.PATH);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            AssetDatabase.CreateAsset(config, SaverConfig.PATH);
            AssetDatabase.SaveAssets();

            return config;
        }
    }
}