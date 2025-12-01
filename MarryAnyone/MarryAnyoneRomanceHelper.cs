using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace Bannerlord.LordLife.MarryAnyone
{
    /// <summary>
    /// Helper class to determine if a character is eligible for romance and marriage.
    /// Allows the player to marry any character (lord, companion, notable) of the opposite sex.
    /// </summary>
    public static class MarryAnyoneRomanceHelper
    {
        /// <summary>
        /// Determines if the player can attempt romance with a given hero.
        /// The hero must be of the opposite sex and not already married.
        /// </summary>
        /// <param name="hero">The hero to check.</param>
        /// <returns>True if romance is possible, false otherwise.</returns>
        public static bool CanRomance(Hero hero)
        {
            if (hero == null || Hero.MainHero == null)
            {
                return false;
            }

            // Must be of opposite sex
            if (hero.IsFemale == Hero.MainHero.IsFemale)
            {
                return false;
            }

            // Cannot romance self
            if (hero == Hero.MainHero)
            {
                return false;
            }

            // Cannot romance dead heroes
            if (!hero.IsAlive)
            {
                return false;
            }

            // Cannot romance if already married to someone else
            if (hero.Spouse != null && hero.Spouse != Hero.MainHero)
            {
                return false;
            }

            // Cannot romance if player is already married to someone else
            if (Hero.MainHero.Spouse != null && Hero.MainHero.Spouse != hero)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if the player can propose marriage to a given hero.
        /// </summary>
        /// <param name="hero">The hero to check.</param>
        /// <returns>True if marriage proposal is possible, false otherwise.</returns>
        public static bool CanProposeMarriage(Hero hero)
        {
            // CanRomance already validates spouse status, aliveness, and opposite sex
            return CanRomance(hero);
        }

        /// <summary>
        /// Executes the marriage between the player and the specified hero.
        /// </summary>
        /// <param name="hero">The hero to marry.</param>
        public static void MarryPlayer(Hero hero)
        {
            if (hero == null || Hero.MainHero == null)
            {
                return;
            }

            // Use the game's marriage action
            MarriageAction.Apply(Hero.MainHero, hero);
        }

        /// <summary>
        /// Gets the type of character for display purposes.
        /// </summary>
        /// <param name="hero">The hero to check.</param>
        /// <returns>A string describing the hero type.</returns>
        public static string GetHeroTypeDescription(Hero hero)
        {
            if (hero == null)
            {
                return "Desconhecido";
            }

            if (hero.IsLord)
            {
                return "Lorde";
            }

            if (hero.IsWanderer)
            {
                return "Companheiro";
            }

            if (hero.IsNotable)
            {
                return "Notável";
            }

            return "Personagem";
        }
    }
}
