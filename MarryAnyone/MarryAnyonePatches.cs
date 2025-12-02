using HarmonyLib;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace Bannerlord.LordLife.MarryAnyone
{
    /// <summary>
    /// Harmony patches to override the vanilla marriage restrictions.
    /// This allows the player to marry any character (lord, companion, notable).
    /// </summary>
    [HarmonyPatch]
    public static class MarryAnyonePatches
    {
        /// <summary>
        /// Patches Romance.GetRomanticLevel to log romance level checks for debugging.
        /// This helps track romance progression between the player and other characters.
        /// </summary>
        [HarmonyPatch(typeof(Romance), nameof(Romance.GetRomanticLevel))]
        [HarmonyPostfix]
        public static void GetRomanticLevelPostfix(Hero person1, Hero person2, ref Romance.RomanceLevelEnum __result)
        {
            if (person1 == null || person2 == null)
            {
                return;
            }

            bool involvesPlayer = person1 == Hero.MainHero || person2 == Hero.MainHero;

            if (involvesPlayer)
            {
                Debug.Print($"[LordLife:MarryAnyone] Romance level between {person1.Name} and {person2.Name}: {__result}");
            }
        }

        /// <summary>
        /// Patches to allow marriage with companions.
        /// </summary>
        /*[HarmonyPatch(typeof(Romance), nameof(Romance.MarriageCourtshipPossibility))]
        [HarmonyPostfix]
        public static void MarriageCourtshipPossibilityPostfix(Hero person1, Hero person2, ref bool __result)
        {
            if (person1 == null || person2 == null)
            {
                return;
            }

            bool involvesPlayer = person1 == Hero.MainHero || person2 == Hero.MainHero;
            Hero otherHero = person1 == Hero.MainHero ? person2 : person1;

            // If the player is involved and the other hero is of opposite sex
            if (involvesPlayer && person1.IsFemale != person2.IsFemale)
            {
                // Check if marriage is already the case
                if (otherHero.Spouse != null)
                {
                    __result = false;
                    return;
                }

                if (Hero.MainHero.Spouse != null)
                {
                    __result = false;
                    return;
                }

                // Check if both are alive
                if (!otherHero.IsAlive || !Hero.MainHero.IsAlive)
                {
                    __result = false;
                    return;
                }

                // Allow courtship with lords, companions, and notables
                if (otherHero.IsLord || otherHero.IsWanderer || otherHero.IsNotable)
                {
                    __result = true;
                    Debug.Print($"[LordLife:MarryAnyone] Marriage courtship allowed between {person1.Name} and {person2.Name}");
                }
            }
        }*/

        /// <summary>
        /// Patches RomanceCampaignBehavior to allow romance initiation with more characters.
        /// </summary>
        [HarmonyPatch(typeof(RomanceCampaignBehavior), "conversation_player_can_open_courtship_on_condition")]
        [HarmonyPostfix]
        public static void CanOpenCourtshipPostfix(ref bool __result)
        {
            Hero conversationHero = Hero.OneToOneConversationHero;
            
            if (conversationHero == null)
            {
                return;
            }

            // If the vanilla check failed, see if our mod should allow it
            if (!__result && MarryAnyoneRomanceHelper.CanProposeMarriage(conversationHero))
            {
                // Check if it's a character type we want to allow
                if (conversationHero.IsLord || conversationHero.IsWanderer || conversationHero.IsNotable)
                {
                    __result = true;
                    Debug.Print($"[LordLife:MarryAnyone] Courtship dialogue enabled for {conversationHero.Name}");
                }
            }
        }

        /// <summary>
        /// Patches RomanceCampaignBehavior.conversation_romance_blocked_on_condition to prevent NullReferenceException.
        /// This method checks if romance should be blocked, but it doesn't handle null references properly
        /// for companions, notables, and other character types that may not have all the expected properties.
        /// We use a prefix patch to intercept the call and handle these cases safely.
        /// </summary>
        [HarmonyPatch(typeof(RomanceCampaignBehavior), "conversation_romance_blocked_on_condition")]
        [HarmonyPrefix]
        public static bool ConversationRomanceBlockedPrefix(ref bool __result)
        {
            try
            {
                Hero conversationHero = Hero.OneToOneConversationHero;
                Hero mainHero = Hero.MainHero;

                // Basic null checks to prevent NullReferenceException
                if (conversationHero == null || mainHero == null)
                {
                    __result = true; // Block romance if either hero is null
                    return false; // Skip original method
                }

                // Check if the hero is one of our supported types for romance
                if (conversationHero.IsLord || conversationHero.IsWanderer || conversationHero.IsNotable)
                {
                    // Check if romance is possible using our helper
                    if (!MarryAnyoneRomanceHelper.CanRomance(conversationHero))
                    {
                        __result = true; // Block romance
                        return false; // Skip original method
                    }
                    
                    // For lords, wanderers (companions), and notables, they may not have a Clan property
                    // The vanilla method expects Clan to exist, which causes NullReferenceException
                    // We safely allow romance for these character types even without a clan
                    __result = false; // Don't block romance
                    Debug.Print($"[LordLife:MarryAnyone] Romance not blocked for {conversationHero.Name} (supported character type)");
                    return false; // Skip original method to prevent NullReferenceException
                }

                // For all other character types, let the original method run
                return true; // Run original method
            }
            catch (System.Exception ex)
            {
                // Safety net: if any unexpected exception occurs, log it and block romance
                // This should not normally execute if our null checks are complete
                Debug.Print($"[LordLife:MarryAnyone] Exception in ConversationRomanceBlockedPrefix: {ex.GetType().Name} - {ex.Message}");
                Debug.Print($"[LordLife:MarryAnyone] Stack trace: {ex.StackTrace}");
                __result = true; // Block romance on error as a safe fallback
                return false; // Skip original method
            }
        }

        /// <summary>
        /// Patches MarriageAction.ApplyInternal to allow the player to marry any character (lord, companion, notable).
        /// This overrides the vanilla IsCoupleSuitableForMarriage check and handles clan changes appropriately.
        /// </summary>
        [HarmonyPatch(typeof(MarriageAction), "ApplyInternal")]
        [HarmonyPrefix]
        public static bool ApplyInternalPrefix(Hero firstHero, Hero secondHero, bool showNotification)
        {
            // Only intercept if the player is involved
            bool involvesPlayer = firstHero == Hero.MainHero || secondHero == Hero.MainHero;
            if (!involvesPlayer)
            {
                return true; // Let vanilla handle NPC-NPC marriages
            }

            Hero player = Hero.MainHero;
            Hero otherHero = firstHero == player ? secondHero : firstHero;

            // Basic validation checks
            if (otherHero == null || player == null)
            {
                Debug.Print("[LordLife:MarryAnyone] ApplyInternal called with null hero");
                return false; // Skip original method
            }

            if (otherHero.Spouse != null && otherHero.Spouse != player)
            {
                Debug.Print($"[LordLife:MarryAnyone] {otherHero.Name} is already married to {otherHero.Spouse.Name}");
                return false; // Skip original method
            }

            if (player.Spouse != null && player.Spouse != otherHero)
            {
                Debug.Print($"[LordLife:MarryAnyone] Player is already married to {player.Spouse.Name}");
                return false; // Skip original method
            }

            // Allow marriage with lords, companions, and notables
            if (otherHero.IsLord || otherHero.IsWanderer || otherHero.IsNotable)
            {
                Debug.Print($"[LordLife:MarryAnyone] Processing marriage between {player.Name} and {otherHero.Name}");
                
                // Calculate relationship increase (try to match vanilla behavior)
                int relationIncrease = 20; // Default value, vanilla uses Campaign.Current.Models.MarriageModel.GetEffectiveRelationIncrease
                try
                {
                    if (Campaign.Current?.Models?.MarriageModel != null)
                    {
                        relationIncrease = Campaign.Current.Models.MarriageModel.GetEffectiveRelationIncrease(firstHero, secondHero);
                    }
                }
                catch (System.Exception ex)
                {
                    // Use default if model method fails
                    Debug.Print($"[LordLife:MarryAnyone] Error getting relation increase from model: {ex.Message}");
                    Debug.Print($"[LordLife:MarryAnyone] Using default relation increase value: {relationIncrease}");
                }

                // Apply relationship increase before setting spouses
                ChangeRelationAction.ApplyRelationChangeBetweenHeroes(firstHero, secondHero, relationIncrease, showQuickNotification: false);

                // Determine clan after marriage - player's clan takes priority
                // This ensures companions and notables join the player's clan
                Clan clanAfterMarriage = player.Clan;
                
                // Determine which hero should be "firstHero" (the one staying in their clan)
                // and which should be "secondHero" (the one potentially changing clans)
                // According to vanilla logic, if clanAfterMarriage != firstHero.Clan, we swap
                Hero heroStayingInClan = firstHero;
                Hero heroChangingClan = secondHero;
                
                if (clanAfterMarriage != firstHero.Clan)
                {
                    heroStayingInClan = secondHero;
                    heroChangingClan = firstHero;
                }

                // Set spouses AFTER determining correct order
                heroStayingInClan.Spouse = heroChangingClan;
                heroChangingClan.Spouse = heroStayingInClan;

                // Dispatch marriage event with correct hero order
                CampaignEventDispatcher.Instance.OnBeforeHeroesMarried(heroStayingInClan, heroChangingClan, showNotification);

                // Handle clan change for the hero who needs to change clans
                // Only heroChangingClan should need to change clans since heroStayingInClan is already in the target clan
                if (heroChangingClan.Clan != clanAfterMarriage)
                {
                    HandleClanChangeForMarriage(heroChangingClan, clanAfterMarriage);
                }

                // End all courtships using reflection to avoid API compatibility issues
                TryEndAllCourtships(heroStayingInClan);
                TryEndAllCourtships(heroChangingClan);
                
                // Set marriage romance state
                ChangeRomanticStateAction.Apply(heroStayingInClan, heroChangingClan, Romance.RomanceLevelEnum.Marriage);

                Debug.Print($"[LordLife:MarryAnyone] Marriage completed between {heroStayingInClan.Name} and {heroChangingClan.Name}");
                
                return false; // Skip original method
            }

            // For other character types, let vanilla handle it
            return true; // Run original method
        }

        /// <summary>
        /// Tries to call Romance.EndAllCourtships using reflection to avoid API compatibility issues.
        /// </summary>
        private static void TryEndAllCourtships(Hero hero)
        {
            try
            {
                // Use reflection to call Romance.EndAllCourtships if it exists
                var romanceType = typeof(Romance);
                var method = romanceType.GetMethod("EndAllCourtships", BindingFlags.Public | BindingFlags.Static);
                if (method != null)
                {
                    method.Invoke(null, new object[] { hero });
                    Debug.Print($"[LordLife:MarryAnyone] Ended all courtships for {hero.Name}");
                }
                else
                {
                    Debug.Print($"[LordLife:MarryAnyone] Romance.EndAllCourtships method not found, skipping");
                }
            }
            catch (System.Exception ex)
            {
                Debug.Print($"[LordLife:MarryAnyone] Error calling EndAllCourtships: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles clan change when a hero marries into a different clan.
        /// Adapted from vanilla HandleClanChangeAfterMarriageForHero but with special handling for companions and notables.
        /// </summary>
        private static void HandleClanChangeForMarriage(Hero hero, Clan newClan)
        {
            if (hero == null || newClan == null || hero.Clan == newClan)
            {
                return;
            }

            Clan oldClan = hero.Clan;

            Debug.Print($"[LordLife:MarryAnyone] Changing {hero.Name}'s clan from {(oldClan != null ? oldClan.Name.ToString() : "None")} to {newClan.Name}");

            // If hero is a governor, remove them
            if (hero.GovernorOf != null)
            {
                ChangeGovernorAction.RemoveGovernorOf(hero);
            }

            // Handle party membership
            if (hero.PartyBelongedTo != null)
            {
                // Handle army membership if applicable
                if (oldClan != null && oldClan.Kingdom != newClan.Kingdom)
                {
                    if (hero.PartyBelongedTo.Army != null)
                    {
                        if (hero.PartyBelongedTo.Army.LeaderParty == hero.PartyBelongedTo)
                        {
                            DisbandArmyAction.ApplyByUnknownReason(hero.PartyBelongedTo.Army);
                        }
                        else
                        {
                            hero.PartyBelongedTo.Army = null;
                        }
                    }

                    // Finish hostile actions using reflection to avoid API compatibility issues
                    // Use the clan's kingdom if available, otherwise use the clan itself as the faction
                    // This is necessary because companions and notables may belong to a clan that is part of a kingdom
                    IFaction targetFaction;
                    if (newClan.Kingdom != null)
                    {
                        targetFaction = newClan.Kingdom;
                    }
                    else
                    {
                        // Clan implements IFaction, so no cast needed
                        targetFaction = newClan;
                    }
                    TryFinishHostileActions(hero, targetFaction);
                }

                MobileParty partyBelongedTo = hero.PartyBelongedTo;
                bool isPartyLeader = partyBelongedTo.LeaderHero == hero;
                
                // Remove from party roster
                partyBelongedTo.MemberRoster.RemoveTroop(hero.CharacterObject);
                
                // Make hero fugitive if they were a party leader
                if (isPartyLeader)
                {
                    MakeHeroFugitiveAction.Apply(hero);
                    if (partyBelongedTo.IsLordParty)
                    {
                        DisbandPartyAction.StartDisband(partyBelongedTo);
                    }
                }
            }

            // Change the clan
            hero.Clan = newClan;

            // Update home settlements for both clans
            if (oldClan != null)
            {
                foreach (Hero h in oldClan.Heroes)
                {
                    h.UpdateHomeSettlement();
                }
            }

            foreach (Hero h in newClan.Heroes)
            {
                h.UpdateHomeSettlement();
            }

            Debug.Print($"[LordLife:MarryAnyone] Clan change completed for {hero.Name}");
        }

        /// <summary>
        /// Tries to call FactionHelper.FinishAllRelatedHostileActionsOfNobleToFaction using reflection.
        /// </summary>
        private static void TryFinishHostileActions(Hero hero, IFaction faction)
        {
            try
            {
                // Try to find FactionHelper type in CampaignSystem assembly
                var campaignAssembly = typeof(Campaign).Assembly;
                var factionHelperType = campaignAssembly.GetType("TaleWorlds.CampaignSystem.FactionHelper");
                
                if (factionHelperType != null)
                {
                    var method = factionHelperType.GetMethod(
                        "FinishAllRelatedHostileActionsOfNobleToFaction",
                        BindingFlags.Public | BindingFlags.Static
                    );
                    
                    if (method != null)
                    {
                        method.Invoke(null, new object[] { hero, faction });
                        Debug.Print($"[LordLife:MarryAnyone] Finished hostile actions for {hero.Name}");
                    }
                    else
                    {
                        Debug.Print($"[LordLife:MarryAnyone] FactionHelper method not found, skipping hostile action cleanup");
                    }
                }
                else
                {
                    Debug.Print($"[LordLife:MarryAnyone] FactionHelper type not found, skipping hostile action cleanup");
                }
            }
            catch (System.Exception ex)
            {
                Debug.Print($"[LordLife:MarryAnyone] Error calling FactionHelper: {ex.Message}");
            }
        }
    }
}
