using Sirenix.OdinInspector;
using System.IO;
using UnityEngine;

namespace Extensions.Saver
{
#pragma warning disable CA2235 // Mark all non-serializable fields
    public sealed class SaverConfig : SerializedScriptableObject
    {
                                                                public          bool    IsCustomPlace()             => place == Place.Custom;

                                                                public const    string  PATH                        = "Assets/Resources/UnitySaver Settings.asset";
                                                                public const    string  PATH_FOR_RESOURCES_LOAD     = "UnitySaver Settings";

        [BoxGroup("B", false), HorizontalGroup("B/H")]
        [TitleGroup("B/H/Settings")]                            public          bool    debug                       = true;
        [TitleGroup("B/H/Settings")]                            public          Place   place                       = Place.UnityPersistant;
        [TitleGroup("B/H/Settings"), ShowIf("IsCustomPlace")]   public          string  customLocation;

        public string Location
		{
            get
			{
                if (place == Place.UnityPersistant) return Application.persistentDataPath;
                if (place == Place.StartLocation)   return Directory.GetCurrentDirectory();
                if (place == Place.Custom)          return customLocation;

                return Application.persistentDataPath;
            }
		}

        [GUIColor(1, 0, 0, 1), PropertySpace]
        [TitleGroup("B/H/Settings"), Button(ButtonSizes.Medium)]
        public void ClearSaveFolder()
        {
            if (Directory.Exists(Location))
                Directory.Delete(Location, true);
        }
    }

    public enum Place
	{
        UnityPersistant,
        StartLocation,
        Custom
	}
#pragma warning restore CA2235 // Mark all non-serializable fields
}