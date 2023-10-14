using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace CementSummaryHider
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("org.gangbeastsmodding.cement.blastfurnaceslag", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        private Queue<DelayedHookData> delayedHooks = new();
        private Harmony baseHarmony;

        private void Awake()
        {
            baseHarmony = new(PluginInfo.PLUGIN_GUID);
            CreateHook(typeof(CementTools.Cement).GetMethod("CreateSummary", AccessTools.all), typeof(Plugin).GetMethod(nameof(Cement_CreateSummary), AccessTools.all));

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        private static void Cement_CreateSummary(CementTools.Cement __instance)
        {
            GameObject summaryGUI = typeof(CementTools.Cement).GetField("summaryGUI", AccessTools.all).GetValue(__instance) as GameObject;
            if (summaryGUI is not null)
            {
                summaryGUI.SetActive(false);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void CreateHook(MethodInfo original, MethodInfo hook, bool isPrefix = false)
        {
            Logger.LogDebug(original);
            Logger.LogDebug(hook);
            Logger.LogDebug(isPrefix);
            if (original == null)
            {
                delayedHooks.Enqueue(new(original, hook, isPrefix));
                return;
            }

            Assembly callingAssembly = Assembly.GetCallingAssembly();
            BaseUnityPlugin callingMod = Chainloader.PluginInfos.FirstOrDefault(x => x.Value.Location == callingAssembly.Location).Value.Instance;

            HarmonyMethod prefix = isPrefix ? new HarmonyMethod(hook) : null;

            HarmonyMethod postfix = isPrefix ? null : new HarmonyMethod(hook);

            baseHarmony.Patch(original, prefix, postfix);

            Logger.LogDebug($"New {(isPrefix ? "PREFIX" : "POSTFIX")} on {original.DeclaringType.Name}.{original.Name} to {hook.DeclaringType.Name}.{hook.Name}");
        }

        private struct DelayedHookData
        {
            public MethodInfo original;
            public MethodInfo hook;
            public bool isPrefix;

            public DelayedHookData(MethodInfo original, MethodInfo hook, bool isPrefix)
            {
                this.original = original;
                this.hook = hook;
                this.isPrefix = isPrefix;
            }
        }
    }
}
