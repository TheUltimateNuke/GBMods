using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace UltiLib
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Gang Beasts.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public static class PluginInfo
        {
            public const string PLUGIN_GUID = "com.theultimatenuke.ultilib";
            public const string PLUGIN_NAME = "UltiLib";
            public const string PLUGIN_VERSION = "1.0.0";
        }

        internal static ManualLogSource LogSource { get; private set; }
        internal static ConfigFile ModConfig { get; private set; }

        private void Awake()
        {
            LogSource = Logger;
            ModConfig = Config;

            Harmony baseHarmony = new(PluginInfo.PLUGIN_GUID);
            Hooking.SetHarmony(baseHarmony);
            Hooking.InitHooks();

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}
