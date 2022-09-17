using ColossalFramework.UI;
using HarmonyLib;
using System;

namespace FindIt
{
    // workaround patch for the bulldoze tool hotkey issue
    // I don't know if this will work. The issue is non-reproducible
    [HarmonyPatch(typeof(KeyShortcuts))]
    [HarmonyPatch("SelectUIButton")]
    [HarmonyPatch(new Type[] { typeof(string) })]
    internal static class SelectUIButtonPatch
    {
        private static bool Prefix(ref string tagString)
        {
            if (tagString.Equals("Bulldozer"))
            {
                // Debugging.Message("Bulldoze tool hotkey pressed");
                UIComponent button = UIView.Find("BulldozerButton");
                if (button != null)
                {
                    button.SimulateClick();
                    return false;
                }
            }
            return true;
        }
    }
}
