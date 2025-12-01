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
            catch (System.Reflection.ReflectionTypeLoadException ex)
            {
                Debug.Print($"[LordLife:Workshop] Reflection error patching MaximumWorkshopsPlayerCanHave: {ex.Message}");
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                Debug.Print($"[LordLife:Workshop] Target invocation error patching MaximumWorkshopsPlayerCanHave: {ex.Message}");
            }
            catch (System.ArgumentException ex)
            {
                Debug.Print($"[LordLife:Workshop] Argument error patching MaximumWorkshopsPlayerCanHave: {ex.Message}");
            }

            // Secondary patch target: GetMaxWorkshopCountForClanTier method
            // Note: This is NOT patched by default as it could affect AI clans with the same tier
            // Only uncomment if MaximumWorkshopsPlayerCanHave is not available and you need a fallback
            // if (!patchApplied)
            // {
            //     try
            //     {
            //         var campaignType = typeof(Campaign);
            //         var getMaxWorkshopMethod = AccessTools.Method(campaignType, "GetMaxWorkshopCountForClanTier");
            //         
            //         if (getMaxWorkshopMethod != null)
            //         {
            //             var postfix = new HarmonyMethod(typeof(WorkshopLimitPatches), nameof(GetMaxWorkshopCountPostfix));
            //             harmony.Patch(getMaxWorkshopMethod, postfix: postfix);
            //             Debug.Print("[LordLife:Workshop] Successfully patched GetMaxWorkshopCountForClanTier");
            //             patchApplied = true;
            //         }
            //     }
            //     catch (System.Reflection.ReflectionTypeLoadException ex)
            //     {
            //         Debug.Print($"[LordLife:Workshop] Reflection error patching GetMaxWorkshopCountForClanTier: {ex.Message}");
            //     }
            //     catch (System.Reflection.TargetInvocationException ex)
            //     {
            //         Debug.Print($"[LordLife:Workshop] Target invocation error patching GetMaxWorkshopCountForClanTier: {ex.Message}");
            //     }
            //     catch (System.ArgumentException ex)
            //     {
            //         Debug.Print($"[LordLife:Workshop] Argument error patching GetMaxWorkshopCountForClanTier: {ex.Message}");
            //     }
            // }

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
                catch (System.Reflection.ReflectionTypeLoadException ex)
                {
                    Debug.Print($"[LordLife:Workshop] Reflection error patching WorkshopsCountLimit: {ex.Message}");
                }
                catch (System.Reflection.TargetInvocationException ex)
                {
                    Debug.Print($"[LordLife:Workshop] Target invocation error patching WorkshopsCountLimit: {ex.Message}");
                }
                catch (System.ArgumentException ex)
                {
                    Debug.Print($"[LordLife:Workshop] Argument error patching WorkshopsCountLimit: {ex.Message}");
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
        /// Postfix patch for GetMaxWorkshopCountForClanTier method.
        /// WARNING: This patch is disabled by default because it could incorrectly apply extra licenses
        /// to AI clans that have the same tier as the player's clan.
        /// Note: This method is static and receives only tier, so we can only check against MainHero's clan tier,
        /// which is not sufficient to identify if the method is being called for the player's clan specifically.
        /// </summary>
        private static void GetMaxWorkshopCountPostfix(int clanTier, ref int __result)
        {
            // This method is intentionally left disabled to avoid incorrectly affecting AI clans.
            // If you need to enable it, uncomment the code in PatchWorkshopLimit() method above.
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
