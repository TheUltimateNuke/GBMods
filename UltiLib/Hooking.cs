using BepInEx;
using BepInEx.Bootstrap;
using Coatsink.Common;
using CoreNet.Components;
using Femur;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace UltiLib
{
    // most of this is learned and taken from the BoneLib source code: https://github.com/yowchap/BoneLib/blob/main/BoneLib/BoneLib/Hooking.cs
    public static class Hooking
    {
        private static Harmony baseHarmony;

        private static Queue<DelayedHookData> delayedHooks = new();

        public static event Action OnGameSetup;
        public static event Action OnGameStart;
        public static event Action OnGameEnded;
        public static event Action OnPostGameEnded;

        public static event Action<GrabEvent> OnGrab;

        public static event Action<Actor, Actor.ActorState> OnStateChanged;

        internal static void SetHarmony(Harmony harmony) => baseHarmony = harmony;
        internal static void InitHooks() // create default hooks.
        {
            CreateHook(typeof(NetRoundOrganiser).GetMethod(nameof(NetRoundOrganiser.InvokeOnGameSetup), AccessTools.all), typeof(Hooking).GetMethod(nameof(Invoke_OnGameSetup), AccessTools.all));
            CreateHook(typeof(NetRoundOrganiser).GetMethod(nameof(NetRoundOrganiser.InvokeOnGameStart), AccessTools.all), typeof(Hooking).GetMethod(nameof(Invoke_OnGameStart), AccessTools.all));

            CreateHook(typeof(NetRoundOrganiser).GetMethod(nameof(NetRoundOrganiser.InvokeOnGameEnded), AccessTools.all), typeof(Hooking).GetMethod(nameof(Invoke_OnGameEnded), AccessTools.all));
            CreateHook(typeof(NetRoundOrganiser).GetMethod(nameof(NetRoundOrganiser.InvokeOnPostGameEnded), AccessTools.all), typeof(Hooking).GetMethod(nameof(Invoke_OnPostGameEnded), AccessTools.all));

            CreateHook(typeof(GrabEvent).GetMethod(nameof(GrabEvent.Grab), AccessTools.all), typeof(Hooking).GetMethod(nameof(Invoke_OnGrab), AccessTools.all));

            CreateHook(typeof(Actor).GetProperty(nameof(Actor.actorState), AccessTools.all).GetSetMethod(), typeof(Hooking).GetMethod(nameof(Invoke_OnStateChangedServer), AccessTools.all));

            while (delayedHooks.Count > 0)
            {
                DelayedHookData data = delayedHooks.Dequeue();
                CreateHook(data.original, data.hook, data.isPrefix);
            }
        }

        private static void Invoke_OnStateChangedServer(Actor __instance)
        {
            OnStateChanged?.Invoke(__instance, __instance.actorState);
        }

        private static void Invoke_OnGrab(GrabEvent __instance)
        {
            OnGrab?.Invoke(__instance);
        }

        private static void Invoke_OnGameSetup()
        {
            OnGameSetup?.Invoke();
        }

        private static void Invoke_OnGameStart()
        {
            OnGameStart?.Invoke();
        }

        private static void Invoke_OnGameEnded()
        {
            OnGameEnded?.Invoke();
            Plugin.LogSource.LogDebug("Game ended!");
        }

        private static void Invoke_OnPostGameEnded()
        {
            OnPostGameEnded?.Invoke();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void CreateHook(MethodInfo original, MethodInfo hook, bool isPrefix = false)
        {
            if(original == null)
            {
                delayedHooks.Enqueue(new(original, hook, isPrefix));
                return;
            }

            Assembly callingAssembly = Assembly.GetCallingAssembly();
            BaseUnityPlugin callingMod = Chainloader.PluginInfos.FirstOrDefault(x => x.Value.Location == callingAssembly.Location).Value.Instance;
            Harmony modHarmony = callingMod != null ? new Harmony(callingMod.Info.Metadata.GUID) : baseHarmony;

            HarmonyMethod prefix = isPrefix ? new HarmonyMethod(hook) : null;
            HarmonyMethod postfix = isPrefix ? null : new HarmonyMethod(hook);
            modHarmony.Patch(original, prefix, postfix);

            Plugin.LogSource.LogDebug($"New {(isPrefix ? "PREFIX" : "POSTFIX")} on {original.DeclaringType.Name}.{original.Name} to {hook.DeclaringType.Name}.{hook.Name}");
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
