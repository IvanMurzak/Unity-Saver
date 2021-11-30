using System;
using UnityEngine;

namespace Extensions.Saver
{
    public static class SaverInitializer
    {
        public static SaverConfig settings { get; private set; } = CreateSettingsConfig();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#pragma warning disable IDE0051 // Remove unused private members
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif
        internal static void Init()
#pragma warning restore IDE0051 // Remove unused private members
        {
            RefreshSettingsFromConfig();
        }

        public static void RefreshSettingsFromConfig()
        {
            if (settings == null) settings = CreateSettingsConfig();
        }

        internal static SaverConfig GetExistingDefaultUnitySettings() => settings;

        private static SaverConfig CreateSettingsConfig()
        {
            try
            {
                var config = Resources.Load<SaverConfig>(SaverConfig.PATH_FOR_RESOURCES_LOAD);
                if (!config)
                {
                    Debug.Log($"Creating new UnitySaver Config into Resources folder");
                    config = ScriptableObject.CreateInstance<SaverConfig>();
                }
                return config;
            } 
            catch (Exception e)
			{
                Debug.LogException(e);
			}

            return null;
        }
    }
}