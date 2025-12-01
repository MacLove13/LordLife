using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using System.Linq;

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
        /// Logs patch errors with consistent formatting.
        /// </summary>
        private static void LogPatchError(string operation, System.Exception ex)
        {
            string errorType = ex switch
            {
                System.Reflection.ReflectionTypeLoadException => "Reflection error",
                System.Reflection.TargetInvocationException => "Target invocation error",
                System.ArgumentException => "Argument error",
                _ => "Error"
            };
            Debug.Print($"[LordLife:Workshop] {errorType} patching {operation}: {ex.Message}");
        }

        /// <summary>
        /// Attempts to patch the workshop count limit.
        /// This may vary depending on the Bannerlord version.
        /// </summary>
        public static void PatchWorkshopLimit(Harmony harmony)
        {
            int patchCount = 0;

            // Patch 1: Clan.WorkshopCountLimit property - Used by UI for display
            try
            {
                var clanType = typeof(Clan);
                var workshopCountLimitProperty = AccessTools.Property(clanType, "WorkshopCountLimit");
                
                if (workshopCountLimitProperty != null)
                {
                    var getMethod = workshopCountLimitProperty.GetGetMethod();
                    if (getMethod != null)
                    {
                        var postfix = new HarmonyMethod(typeof(WorkshopLimitPatches), nameof(WorkshopCountLimitPostfix));
                        harmony.Patch(getMethod, postfix: postfix);
                        Debug.Print("[LordLife:Workshop] Successfully patched Clan.WorkshopCountLimit property");
                        patchCount++;
                    }
                }
                else
                {
                    Debug.Print("[LordLife:Workshop] WorkshopCountLimit property not found, trying alternatives...");
                }
            }
            catch (System.Exception ex)
            {
                LogPatchError("Clan.WorkshopCountLimit", ex);
            }

            // Patch 2: Campaign.MaximumWorkshopsPlayerCanHave property
            try
            {
                var campaignType = typeof(Campaign);
                var maxWorkshopsProperty = AccessTools.Property(campaignType, "MaximumWorkshopsPlayerCanHave");
                
                if (maxWorkshopsProperty != null)
                {
                    var getMethod = maxWorkshopsProperty.GetGetMethod();
                    if (getMethod != null)
                    {
                        var postfix = new HarmonyMethod(typeof(WorkshopLimitPatches), nameof(MaximumWorkshopsPlayerCanHavePostfix));
                        harmony.Patch(getMethod, postfix: postfix);
                        Debug.Print("[LordLife:Workshop] Successfully patched Campaign.MaximumWorkshopsPlayerCanHave");
                        patchCount++;
                    }
                }
                else
                {
                    Debug.Print("[LordLife:Workshop] MaximumWorkshopsPlayerCanHave property not found");
                }
            }
            catch (System.Exception ex)
            {
                LogPatchError("Campaign.MaximumWorkshopsPlayerCanHave", ex);
            }

            // Patch 3: Try alternative property names that might exist in different versions
            // This is attempted regardless of previous patch success as different properties
            // may be used by different parts of the game (UI vs logic)
            try
            {
                var clanType = typeof(Clan);
                
                // Try "WorkshopsCountLimit" as alternative spelling
                var workshopLimitProperty = AccessTools.Property(clanType, "WorkshopsCountLimit");
                if (workshopLimitProperty != null)
                {
                    var getMethod = workshopLimitProperty.GetGetMethod();
                    if (getMethod != null)
                    {
                        // Uses same postfix as it handles the same Clan instance and result type
                        var postfix = new HarmonyMethod(typeof(WorkshopLimitPatches), nameof(WorkshopCountLimitPostfix));
                        harmony.Patch(getMethod, postfix: postfix);
                        Debug.Print("[LordLife:Workshop] Successfully patched Clan.WorkshopsCountLimit property (alternative)");
                        patchCount++;
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogPatchError("Clan.WorkshopsCountLimit (alternative)", ex);
            }

            if (patchCount == 0)
            {
                Debug.Print("[LordLife:Workshop] WARNING: No workshop limit patches were applied. Workshop licenses may not work correctly.");
#if DEBUG
                // Diagnostic output for debugging - only in debug builds
                Debug.Print("[LordLife:Workshop] Available Clan properties: " + string.Join(", ", typeof(Clan).GetProperties().Select(p => p.Name)));
#endif
            }
            else
            {
                Debug.Print($"[LordLife:Workshop] Applied {patchCount} workshop limit patch(es) successfully.");
            }
        }

        /// <summary>
        /// Postfix patch for Campaign.MaximumWorkshopsPlayerCanHave property.
        /// This controls the player's workshop limit in game logic.
        /// </summary>
        private static void MaximumWorkshopsPlayerCanHavePostfix(ref int __result)
        {
            try
            {
                var mainHeroClan = Hero.MainHero?.Clan;
                if (mainHeroClan != null)
                {
                    int extraLicenses = WorkshopLicenseManager.Instance.GetExtraLicenses(mainHeroClan.StringId);
                    if (extraLicenses > 0)
                    {
                        int originalResult = __result;
                        __result += extraLicenses;
                        Debug.Print($"[LordLife:Workshop] MaximumWorkshopsPlayerCanHave: {originalResult} + {extraLicenses} extra = {__result}");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.Print($"[LordLife:Workshop] Error in MaximumWorkshopsPlayerCanHavePostfix: {ex.Message}");
            }
        }

        /// <summary>
        /// Postfix patch for Clan.WorkshopCountLimit property.
        /// This is used by the UI to display the workshop limit.
        /// Adds extra licenses purchased by any clan.
        /// </summary>
        private static void WorkshopCountLimitPostfix(Clan __instance, ref int __result)
        {
            try
            {
                if (__instance != null)
                {
                    int extraLicenses = WorkshopLicenseManager.Instance.GetExtraLicenses(__instance.StringId);
                    if (extraLicenses > 0)
                    {
                        int originalResult = __result;
                        __result += extraLicenses;
                        Debug.Print($"[LordLife:Workshop] WorkshopCountLimit for {__instance.Name}: {originalResult} + {extraLicenses} extra = {__result}");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.Print($"[LordLife:Workshop] Error in WorkshopCountLimitPostfix: {ex.Message}");
            }
        }
    }
}
