using MelonLoader;

namespace ModTemplateML
{
    public static class BuildInfo
    {
        public const string Name = "Mod Template";
        public const string Author = "TheUltimateNuke";
        public const string Description = null;
        public const string Company = null;
        public const string Version = "1.0.0";
        public const string DownloadLink = null;
    }

    public class Mod : MelonMod
    {
        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();

            LoggerInstance.Msg("Mod " + BuildInfo.Name + " has initialized.");
        }
    }
}