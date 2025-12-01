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
        /// Patches Romance.GetRomanticLevel to log romance level checks for debugging.
        /// This helps track romance progression between the player and other characters.
        /// </summary>
        [HarmonyPatch(typeof(Romance), nameof(Romance.GetRomanticLevel))]
        [HarmonyPostfix]
        public static void GetRomanticLevelPostfix(Hero hero1, Hero hero2, ref Romance.RomanceLevelEnum __result)
        {
            if (hero1 == null || hero2 == null)
            {
                return;
            }

            bool involvesPlayer = hero1 == Hero.MainHero || hero2 == Hero.MainHero;

            if (involvesPlayer)
            {
                Debug.Print($"[LordLife:MarryAnyone] Romance level between {hero1.Name} and {hero2.Name}: {__result}");
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
