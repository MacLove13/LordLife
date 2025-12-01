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
            bool patchApplied = false;

            // Primary patch target: MaximumWorkshopsPlayerCanHave property in Campaign
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
                        patchApplied = true;
                    }
                }
                else
                {
                    Debug.Print("[LordLife:Workshop] MaximumWorkshopsPlayerCanHave property not found in Campaign class");
                }
            }
            catch (System.Exception ex) when (ex is System.Reflection.ReflectionTypeLoadException || 
                                              ex is System.Reflection.TargetInvocationException || 
                                              ex is System.ArgumentException)
            {
                LogPatchError("MaximumWorkshopsPlayerCanHave", ex);
            }

            // Secondary patch target: WorkshopsCountLimit property on Clan
            if (!patchApplied)
            {
                try
                {
                    var clanType = typeof(Clan);
                    var workshopLimitProperty = AccessTools.Property(clanType, "WorkshopsCountLimit");
                    
                    if (workshopLimitProperty != null)
                    {
                        var getMethod = workshopLimitProperty.GetGetMethod();
                        if (getMethod != null)
                        {
                            var postfix = new HarmonyMethod(typeof(WorkshopLimitPatches), nameof(WorkshopLimitPropertyPostfix));
                            harmony.Patch(getMethod, postfix: postfix);
                            Debug.Print("[LordLife:Workshop] Successfully patched Clan.WorkshopsCountLimit property");
                            patchApplied = true;
                        }
                    }
                }
                catch (System.Exception ex) when (ex is System.Reflection.ReflectionTypeLoadException || 
                                                  ex is System.Reflection.TargetInvocationException || 
                                                  ex is System.ArgumentException)
                {
                    LogPatchError("WorkshopsCountLimit", ex);
                }
            }

            if (!patchApplied)
            {
                Debug.Print("[LordLife:Workshop] WARNING: No workshop limit patch was applied. Workshop licenses may not work correctly.");
            }
        }

        /// <summary>
        /// Postfix patch for Campaign.MaximumWorkshopsPlayerCanHave property.
        /// This is the primary patch target for controlling the player's workshop limit.
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
                        Debug.Print($"[LordLife:Workshop] MaximumWorkshopsPlayerCanHave: {originalResult} + {extraLicenses} (extra licenses) = {__result}");
                    }
                }
            }
            catch (System.NullReferenceException ex)
            {
                Debug.Print($"[LordLife:Workshop] Null reference in MaximumWorkshopsPlayerCanHavePostfix: {ex.Message}");
            }
        }

        /// <summary>
        /// Postfix patch for Clan.WorkshopsCountLimit property (secondary fallback).
        /// Adds extra licenses purchased by the player's clan only.
        /// This safely handles both player and AI clans by checking clan context.
        /// </summary>
        private static void WorkshopLimitPropertyPostfix(Clan __instance, ref int __result)
        {
            if (__instance != null)
            {
                int extraLicenses = WorkshopLicenseManager.Instance.GetExtraLicenses(__instance.StringId);
                if (extraLicenses > 0)
                {
                    int originalResult = __result;
                    __result += extraLicenses;
                    Debug.Print($"[LordLife:Workshop] Workshop limit for {__instance.Name}: {originalResult} + {extraLicenses} (extra licenses) = {__result}");
                }
            }
        }
    }
}
