using HarmonyLib;
using Il2CppFemur;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoagulationChaosMod.Patches
{
    [HarmonyPatch(typeof(Actor))]
    public static class ActorPatch
    {
        [HarmonyPatch(nameof(Actor.OnDestroy))]
        [HarmonyPrefix]
        static bool OnDestroy_Prefix()
        {
            if (Mod.playerIsBecomingJuggernaut) 
            {
                return true;
            }
            return false;
        }
    }
}
