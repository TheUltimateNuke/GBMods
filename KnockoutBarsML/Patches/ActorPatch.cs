using HarmonyLib;
using Il2CppFemur;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnockoutBarsML.Patches
{


    public static class ActorPatch
    {
        [HarmonyPatch(typeof(Actor), nameof(Actor.OnClientStateChange))]
        static class StatePatch
        {
            [HarmonyPostfix]
            static void StateChanged_Postfix(Actor __instance)
            {
                Actor.ActorState state = __instance.actorState;
                
                switch (state)
                {
                    case Actor.ActorState.Unconscious:
                        Mod.EnableKnockoutBar(__instance);
                        break;
                    default:
                        Mod.DisableKnockoutBar(__instance);
                        break;
                }
            }
        }
    }
}
