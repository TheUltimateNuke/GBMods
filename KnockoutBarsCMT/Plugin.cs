using BepInEx;
using Femur;

namespace KnockoutBarsCMT
{
    public class Plugin : CementTools.CementMod
    {
        private void Awake()
        {
            CementTools.Modules.HookModule.HookModule.CreateHook(new CementTools.Modules.HookModule.HookModule.CementHook
            {
                callingMod = this,
                original = typeof(Actor).GetEvent(nameof(Femur.Actor.OnActorStateChangedClient)).GetRaiseMethod(),
                hook = typeof(Plugin).GetMethod(nameof(Plugin.OnClientStateChange)),
                isPrefix = false
            }) ;
        }

        private void OnClientStateChange(Actor __instance, Actor.ActorState value)
        {
            bool isUnconsciousFlag = __instance.actorState == value && value == Actor.ActorState.Unconscious;
            __instance.statusHandeler.displayTimer = float.MaxValue * (isUnconsciousFlag ? 1 : 0);
            __instance.statusHandeler.showStatusBar = isUnconsciousFlag;
            __instance.statusHandeler.statusBarTransform.gameObject.SetActive(isUnconsciousFlag);
            __instance.statusHandeler.statusBarTransform.Find("healthBarBack").gameObject.SetActive(!isUnconsciousFlag);
            __instance.statusHandeler.statusBarTransform.Find("staminaBarBack").gameObject.SetActive(!isUnconsciousFlag);
        }
    }
}
