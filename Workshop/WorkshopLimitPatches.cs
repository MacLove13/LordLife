using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.LordLife.Workshop
{
    /// <summary>
    /// Harmony patches to modify workshop limit calculations.
    /// Adds purchased workshop licenses to the base clan workshop limit.
    /// </summary>
    [HarmonyPatch]
    public static class WorkshopLimitPatches
    {
        /// <summary>
        /// Attempts to patch the workshop count limit.
        /// This may vary depending on the Bannerlord version.
        /// </summary>
        public static void PatchWorkshopLimit(Harmony harmony)
        {
            try
            {
                // Try to find and patch the workshop limit property
                var clanType = typeof(Clan);
                var workshopLimitProperty = AccessTools.Property(clanType, "WorkshopsCountLimit");
                
                if (workshopLimitProperty != null)
                {
                    var getMethod = workshopLimitProperty.GetGetMethod();
                    if (getMethod != null)
                    {
                        var postfix = new HarmonyMethod(typeof(WorkshopLimitPatches), nameof(WorkshopLimitPostfix));
                        harmony.Patch(getMethod, postfix: postfix);
                        Debug.Print("[LordLife:Workshop] Successfully patched WorkshopsCountLimit");
                    }
                }
                else
                {
                    Debug.Print("[LordLife:Workshop] WorkshopsCountLimit property not found, trying alternative methods");
                }
            }
            catch (System.Exception ex)
            {
                Debug.Print($"[LordLife:Workshop] Error patching workshop limit: {ex.Message}");
            }
        }

        /// <summary>
        /// Postfix patch for workshop limit calculation.
        /// Adds extra licenses purchased by the player.
        /// </summary>
        private static void WorkshopLimitPostfix(Clan __instance, ref int __result)
        {
            if (__instance != null)
            {
                int extraLicenses = WorkshopLicenseManager.Instance.GetExtraLicenses(__instance.StringId);
                if (extraLicenses > 0)
                {
                    __result += extraLicenses;
                    Debug.Print($"[LordLife:Workshop] Workshop limit for {__instance.Name}: {__result - extraLicenses} + {extraLicenses} (extra licenses) = {__result}");
                }
            }
        }
    }
}
