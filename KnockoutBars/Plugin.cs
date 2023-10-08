using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Femur;
using System.Collections.Generic;
using UltiLib;

namespace KnockoutBars
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Gang Beasts.exe")]
    [BepInDependency("com.theultimatenuke.ultilib", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource LogSource { get; private set; }
        internal static ConfigFile ModConfig { get; private set; }
        internal static Dictionary<string, AssetBundleTest> AssetBundleRefs = new();

        private static void AddToDict(AssetBundleTest assetBundleTest)
        {
            AssetBundleRefs.Add(assetBundleTest.AssetBundleIdentifier, assetBundleTest);
        }
        
        private void Awake()
        {
            LogSource = Logger;
            ModConfig = Config;

            //AddToDict(new("knockoutbar.bundle", "knockout_bar", true));

            Hooking.OnStateChanged += OnStateChanged;

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        private void OnStateChanged(Actor actor, Actor.ActorState state)
        {
            switch(state)
            {
                case Actor.ActorState.Unconscious:
                    actor.statusHandeler.displayTimer = float.MaxValue; 
                    actor.statusHandeler.showStatusBar = true; 
                    actor.statusHandeler.statusBarTransform.gameObject.SetActive(true);
                    actor.statusHandeler.statusBarTransform.Find("healthBarBack").gameObject.SetActive(false);
                    actor.statusHandeler.statusBarTransform.Find("staminaBarBack").gameObject.SetActive(false);
                    break;
                default:
                    actor.statusHandeler.displayTimer = 0f; 
                    actor.statusHandeler.showStatusBar = false; 
                    actor.statusHandeler.statusBarTransform.gameObject.SetActive(false);
                    actor.statusHandeler.statusBarTransform.Find("healthBarBack").gameObject.SetActive(true);
                    actor.statusHandeler.statusBarTransform.Find("staminaBarBack").gameObject.SetActive(true);
                    break;
            }
        }
    }
}
