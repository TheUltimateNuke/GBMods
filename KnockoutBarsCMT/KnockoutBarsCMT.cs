using BepInEx;
using BepInEx.Bootstrap;
using CementTools;
using Femur;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace KnockoutBarsCMT
{
    public class KnockoutBarsCMT : CementMod
    {
        private Queue<DelayedHookData> delayedHooks = new();

        private void Awake()
        {
            CreateHook(typeof(Actor).GetProperty(nameof(Actor.actorState), AccessTools.all).GetSetMethod(), typeof(KnockoutBarsCMT).GetMethod(nameof(Invoke_OnStateChanged), AccessTools.all));

            Cement.Log($"Cement mod {modFile.GetString("Name")} loaded!");
        }

        private void Invoke_OnStateChanged(Actor __instance)
        {
            Actor.ActorState state = __instance.actorState;

            switch (state)
            {
                case Actor.ActorState.Unconscious:
                    __instance.statusHandeler.displayTimer = float.MaxValue;
                    __instance.statusHandeler.showStatusBar = true;
                    __instance.statusHandeler.statusBarTransform.gameObject.SetActive(true);
                    __instance.statusHandeler.statusBarTransform.Find("healthBarBack").gameObject.SetActive(false);
                    __instance.statusHandeler.statusBarTransform.Find("staminaBarBack").gameObject.SetActive(false);
                    break;
                default:
                    __instance.statusHandeler.displayTimer = 0f;
                    __instance.statusHandeler.showStatusBar = false;
                    __instance.statusHandeler.statusBarTransform.gameObject.SetActive(false);
                    __instance.statusHandeler.statusBarTransform.Find("healthBarBack").gameObject.SetActive(true);
                    __instance.statusHandeler.statusBarTransform.Find("staminaBarBack").gameObject.SetActive(true);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void CreateHook(MethodInfo original, MethodInfo hook, bool isPrefix = false)
        {
            if (original == null)
            {
                delayedHooks.Enqueue(new(original, hook, isPrefix));
                return;
            }

            Assembly callingAssembly = Assembly.GetCallingAssembly();
            Harmony modHarmony = new(modFile.GetString("Name"));

            HarmonyMethod prefix = isPrefix ? new HarmonyMethod(hook) : null;
            HarmonyMethod postfix = isPrefix ? null : new HarmonyMethod(hook);
            modHarmony.Patch(original, prefix, postfix);

            Cement.Log($"New {(isPrefix ? "PREFIX" : "POSTFIX")} on {original.DeclaringType.Name}.{original.Name} to {hook.DeclaringType.Name}.{hook.Name}");
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
