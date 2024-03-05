using HarmonyLib;
using Il2CppGB.UI;

namespace IncreasedWinLimit.Patches
{
    [HarmonyPatch(typeof(MenuHandlerGamemodes))]
    public static class UIPatch
    {
        [HarmonyPatch(nameof(MenuHandlerGamemodes.GenerateUI))]
        [HarmonyPostfix]
        static void GenerateUI_Postfix(MenuHandlerGamemodes __instance)
        {
            __instance.winsSetup.UpdateAllowedValues(1, Mod.WinLimit);
        }
    }

    [HarmonyPatch(typeof(MenuHandlerWins))]
    public static class TextPatch
    {
        [HarmonyPatch(nameof(MenuHandlerWins.UpdateText))]
        [HarmonyPostfix]
        static void UpdateText_Postfix(MenuHandlerWins __instance)
        {
            __instance.valueText.text = __instance.CurrentValue.ToString();
        }
    }
}
