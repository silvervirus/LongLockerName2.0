using Common.Utility;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Nautilus.Utility;
using Nautilus.Json.Interfaces;
using UnityEngine.ResourceManagement.ResourceProviders;
using Nautilus.Json;
using JsonReader = Newtonsoft.Json.JsonReader;
using System.Text.Json;
using Newtonsoft.Json.Linq;



namespace LongLockerNames
{
    static class Mod
    {
        public static Config config;
        private static string modDirectory;

        public static void Patch(string modDirectory = null)
        {
            Mod.modDirectory = modDirectory ?? "Subnautica_Data/Managed";
            LoadConfig();


            new Harmony("com.LongLockerNames.mod").PatchAll(Assembly.GetExecutingAssembly());


            Logger.Log("Patched");
        }

        private static string GetModInfoPath()
        {
            // Get the path to the directory where the executable is located
            string exeDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            // Combine the executable directory path with the relative mod directory
            string modDirectory = Path.Combine(exeDirectory, Mod.modDirectory);
            // Combine the mod directory path with the mod info filename
            string modInfoPath = Path.Combine(modDirectory, "Config.json");
            return modInfoPath;
        }

        public static string GetAssetPath(string filename)
        {
            // Get the path to the directory where the executable is located
            string exeDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            // Combine the executable directory path with the relative mod directory
            string modDirectory = Path.Combine(exeDirectory, Mod.modDirectory);
            // Combine the mod directory path with the assets directory and the filename
            string assetPath = Path.Combine(modDirectory, "Assets", filename);
            return assetPath;
        }

        private static void LoadConfig()
        {
            string modInfoPath = GetModInfoPath();

            if (!File.Exists(modInfoPath))
            {
                config = new Config();
                return;
            }

            string modInfoJson = File.ReadAllText(modInfoPath);
            var modInfoObject = JsonConvert.DeserializeObject<JObject>(modInfoJson);
            var configJsonElement = modInfoObject.GetValue("Config");
            string configJson = configJsonElement.ToString();
            config = JsonConvert.DeserializeObject<Config>(configJson);
            ValidateConfig();
        }

        private static void ValidateConfig()
        {
            Config defaultConfig = new Config();
            if (config == null)
            {
                config = defaultConfig;
                return;
            }

            ValidateConfigValue("SmallLockerTextLimit", 1, 500, defaultConfig);
            ValidateConfigValue("SignTextLimit", 1, 500, defaultConfig);
        }

        private static void ValidateConfigValue<T>(string field, T min, T max, Config defaultConfig) where T : IComparable
        {
            var fieldInfo = typeof(Config).GetField(field, BindingFlags.Public | BindingFlags.Instance);
            T value = (T)fieldInfo.GetValue(config);
            if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
            {
                Logger.Log("Config value for '{0}' ({1}) was not valid. Must be between {2} and {3}",
                    field,
                    value,
                    min,
                    max
                );
                fieldInfo.SetValue(config, fieldInfo.GetValue(defaultConfig));
            }
        }
    }
}