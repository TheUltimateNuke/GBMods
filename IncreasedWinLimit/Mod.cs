using MelonLoader;

namespace IncreasedWinLimit
{
    public static class BuildInfo
    {
        public const string Name = "IncreasedWinLimit";
        public const string Author = "TheUltimateNuke";
        public const string Description = null;
        public const string Company = null;
        public const string Version = "1.0.0";
        public const string DownloadLink = null;
    }

    public class Mod : MelonMod
    {
        public static int WinLimit => _winLimit.Value;

        private static readonly MelonPreferences_Category _melonCat = MelonPreferences.CreateCategory(BuildInfo.Name);
        private static readonly MelonPreferences_Entry<int> _winLimit = _melonCat.CreateEntry(nameof(_winLimit), 15, "Win Limit");

        public override void OnInitializeMelon()
        {
            HarmonyInstance.PatchAll();
        }
    }
}