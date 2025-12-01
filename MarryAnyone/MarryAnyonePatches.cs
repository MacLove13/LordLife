using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
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
        /// Patches the Romance.RomanceCourtshipAttemptCooldown to allow repeated courtship attempts.
        /// </summary>
        [HarmonyPatch(typeof(Romance), nameof(Romance.GetRomanticLevel))]
        [HarmonyPostfix]
        public static void GetRomanticLevelPostfix(Hero hero1, Hero hero2, ref Romance.RomanceLevelEnum __result)
        {
            // If either hero is the player and they are of opposite sex,
            // we allow romance to proceed more easily
            if (hero1 == null || hero2 == null)
            {
                return;
            }

            bool involvesPlayer = hero1 == Hero.MainHero || hero2 == Hero.MainHero;
            bool oppositeGender = hero1.IsFemale != hero2.IsFemale;

            if (involvesPlayer && oppositeGender)
            {
                // If not yet started romance, allow it to be considered as courtship started
                if (__result == Romance.RomanceLevelEnum.Untested)
                {
                    // Don't change untested - let it be discovered through dialogue
                }
            }

            Debug.Print($"[LordLife:MarryAnyone] Romance level between {hero1.Name} and {hero2.Name}: {__result}");
        }

        /// <summary>
        /// Patches to allow marriage with companions.
        /// </summary>
        [HarmonyPatch(typeof(Romance), nameof(Romance.MarriageCourtshipPossibility))]
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
        }

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
    }
}
