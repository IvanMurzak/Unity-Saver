using System;
using UnityEngine;

namespace Extensions.Saver
{
    public static class SaverInitializer
    {
        private static SaverConfig _config;
        public static SaverConfig Config => _config == null ? _config = LoadOrCreateConfig() : _config;

        private static SaverConfig LoadOrCreateConfig()
        {
            var config = Resources.Load<SaverConfig>(SaverConfig.PATH_FOR_RESOURCES_LOAD);
            if (!config) throw new NullReferenceException("UnitySaver config was not found");
            return config;
        }
    }
}