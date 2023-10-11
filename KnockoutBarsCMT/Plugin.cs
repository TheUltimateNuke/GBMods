using BepInEx;

namespace KnockoutBarsCMT
{
    public class Plugin : CementTools.CementMod
    {
        private void Awake()
        {
            On.Femur.Actor.OnClientStateChange += Actor_OnClientStateChange;
        }

        private void Actor_OnClientStateChange(On.Femur.Actor.orig_OnClientStateChange orig, Femur.Actor actor, Femur.Actor.ActorState value)
        {
            if (value == Femur.Actor.ActorState.Unconscious)
            { 
                actor.statusHandeler.displayTimer = float.MaxValue;
                actor.statusHandeler.showStatusBar = true;
                actor.statusHandeler.statusBarTransform.gameObject.SetActive(true);
                actor.statusHandeler.statusBarTransform.Find("healthBarBack").gameObject.SetActive(false);
                actor.statusHandeler.statusBarTransform.Find("staminaBarBack").gameObject.SetActive(false);
            }
            else
            {
                actor.statusHandeler.displayTimer = 0f;
                actor.statusHandeler.showStatusBar = false;
                actor.statusHandeler.statusBarTransform.gameObject.SetActive(false);
                actor.statusHandeler.statusBarTransform.Find("healthBarBack").gameObject.SetActive(true);
                actor.statusHandeler.statusBarTransform.Find("staminaBarBack").gameObject.SetActive(true);
            }

        }
    }
}
