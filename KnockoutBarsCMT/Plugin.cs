using BepInEx;
using CementTools;
using Femur;
using UnityEngine;

namespace KnockoutBarsCMT
{
    public class Plugin : CementTools.CementMod
    {
        private void Awake()
        {
            
        }

        private void OnClientStateChange(Actor __instance, Actor.ActorState value)
        {
            bool isUnconsciousFlag = __instance.actorState == value && value == Actor.ActorState.Unconscious;
            Cement.Log("Changed state, now " + isUnconsciousFlag);
            __instance.statusHandeler.displayTimer = float.MaxValue * (isUnconsciousFlag ? 1 : 0);
            __instance.statusHandeler.showStatusBar = isUnconsciousFlag;
            __instance.statusHandeler.statusBarTransform.gameObject.SetActive(isUnconsciousFlag);
            __instance.statusHandeler.statusBarTransform.Find("healthBarBack").gameObject.SetActive(!isUnconsciousFlag);
            __instance.statusHandeler.statusBarTransform.Find("staminaBarBack").gameObject.SetActive(!isUnconsciousFlag);
        }
    }
}
