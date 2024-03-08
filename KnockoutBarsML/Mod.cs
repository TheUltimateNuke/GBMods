using Il2CppFemur;
using MelonLoader;

namespace KnockoutBarsML
{
    public static class BuildInfo
    {
        public const string Name = "KnockoutBarsML";
        public const string Author = "TheUltimateNuke";
        public const string Description = null;
        public const string Company = null;
        public const string Version = "1.0.2";
        public const string DownloadLink = null;
    }

    public class Mod : MelonMod
    {
        public override void OnInitializeMelon()
        {
            HarmonyInstance.PatchAll();
        }

        public static void EnableKnockoutBar(Actor actor)
        {
            actor.statusHandeler.displayTimer = float.MaxValue;
            actor.statusHandeler.showStatusBar = true;
            actor.statusHandeler.statusBarTransform.gameObject.SetActive(true);
            actor.statusHandeler.statusBarTransform.Find("healthBarBack").gameObject.SetActive(false);
            actor.statusHandeler.statusBarTransform.Find("staminaBarBack").gameObject.SetActive(false);
        }

        public static void DisableKnockoutBar(Actor actor)
        {
            actor.statusHandeler.displayTimer = 0f;
            actor.statusHandeler.showStatusBar = false;
            actor.statusHandeler.statusBarTransform.gameObject.SetActive(false);
            actor.statusHandeler.statusBarTransform.Find("healthBarBack").gameObject.SetActive(true);
            actor.statusHandeler.statusBarTransform.Find("staminaBarBack").gameObject.SetActive(true);
        }
    }
}