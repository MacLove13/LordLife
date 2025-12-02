using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Library;

namespace Bannerlord.LordLife.MarryAnyone
{
    /// <summary>
    /// Helper class to determine if a character is eligible for romance and marriage.
    /// Allows the player to marry any character (lord, companion, notable) of the opposite sex.
    /// </summary>
    public static class MarryAnyoneRomanceHelper
    {
        // Relationship penalty applied when divorcing
        private const int DIVORCE_RELATIONSHIP_PENALTY = -30;

        // Track which heroes have completed courtship questions
        // This is persisted across save/load via CampaignBehaviorBase.SyncData() in MarryAnyoneCampaignBehavior
        private static HashSet<Hero> _courtshipQuestionsCompleted = new HashSet<Hero>();

        // Track ex-spouses for the player
        // Since the Hero class doesn't have an ExSpouses property, we track it ourselves
        private static List<Hero> _playerExSpouses = new List<Hero>();

        /// <summary>
        /// Gets the set of heroes who have completed courtship questions.
        /// Used for save/load serialization.
        /// </summary>
        public static IEnumerable<Hero> GetCompletedCourtshipHeroes()
        {
            return _courtshipQuestionsCompleted.AsEnumerable();
        }

        /// <summary>
        /// Sets the heroes who have completed courtship questions.
        /// Used for save/load deserialization.
        /// </summary>
        public static void SetCompletedCourtshipHeroes(IEnumerable<Hero> heroes)
        {
            _courtshipQuestionsCompleted.Clear();
            if (heroes != null)
            {
                foreach (var hero in heroes)
                {
                    if (hero != null)
                    {
                        _courtshipQuestionsCompleted.Add(hero);
                    }
                }
            }
        }
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
        /// Requires courtship questions to be completed and minimum romance level.
        /// </summary>
        /// <param name="hero">The hero to check.</param>
        /// <returns>True if marriage proposal is possible, false otherwise.</returns>
        public static bool CanProposeMarriage(Hero hero)
        {
            // Basic romance eligibility check
            if (!CanRomance(hero))
            {
                return false;
            }

            // Check if courtship questions have been completed
            if (!HasCompletedCourtshipQuestions(hero))
            {
                return false;
            }

            // Check romance level - need at least MatchMadeByFamily level
            Romance.RomanceLevelEnum romanceLevel = Romance.GetRomanticLevel(Hero.MainHero, hero);
            if (romanceLevel < Romance.RomanceLevelEnum.MatchMadeByFamily)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if courtship questions have been completed with the hero.
        /// </summary>
        public static bool HasCompletedCourtshipQuestions(Hero hero)
        {
            return _courtshipQuestionsCompleted.Contains(hero);
        }

        /// <summary>
        /// Marks courtship questions as completed with the hero.
        /// </summary>
        public static void CompleteCourtshipQuestions(Hero hero)
        {
            if (!_courtshipQuestionsCompleted.Contains(hero))
            {
                _courtshipQuestionsCompleted.Add(hero);
                Debug.Print($"[LordLife:MarryAnyone] Courtship questions completed with {hero.Name}");
            }
        }

        /// <summary>
        /// Increases romance level with the hero.
        /// </summary>
        public static void IncreaseRomanceLevel(Hero hero)
        {
            Romance.RomanceLevelEnum currentLevel = Romance.GetRomanticLevel(Hero.MainHero, hero);
            
            // Progress through romance stages
            Romance.RomanceLevelEnum newLevel = currentLevel;
            switch (currentLevel)
            {
                case Romance.RomanceLevelEnum.Untested:
                    newLevel = Romance.RomanceLevelEnum.MatchMadeByFamily;
                    break;
                case Romance.RomanceLevelEnum.MatchMadeByFamily:
                    newLevel = Romance.RomanceLevelEnum.CourtshipStarted;
                    break;
                case Romance.RomanceLevelEnum.CourtshipStarted:
                    newLevel = Romance.RomanceLevelEnum.CoupleDecidedThatTheyAreCompatible;
                    break;
                case Romance.RomanceLevelEnum.CoupleDecidedThatTheyAreCompatible:
                    newLevel = Romance.RomanceLevelEnum.CoupleAgreedOnMarriage;
                    break;
            }

            if (newLevel != currentLevel)
            {
                ChangeRomanticStateAction.Apply(Hero.MainHero, hero, newLevel);
                Debug.Print($"[LordLife:MarryAnyone] Romance level increased from {currentLevel} to {newLevel} with {hero.Name}");
            }
        }

        /// <summary>
        /// Resets courtship data. This is called when starting a new game
        /// or when courtship data should be cleared.
        /// </summary>
        public static void ResetCourtshipData()
        {
            _courtshipQuestionsCompleted.Clear();
            _playerExSpouses.Clear();
        }

        /// <summary>
        /// Gets the list of ex-spouses for save/load serialization.
        /// </summary>
        public static IEnumerable<Hero> GetPlayerExSpouses()
        {
            return _playerExSpouses.AsEnumerable();
        }

        /// <summary>
        /// Sets the ex-spouses list for save/load deserialization.
        /// </summary>
        public static void SetPlayerExSpouses(IEnumerable<Hero> exSpouses)
        {
            _playerExSpouses.Clear();
            if (exSpouses != null)
            {
                foreach (var hero in exSpouses)
                {
                    if (hero != null)
                    {
                        _playerExSpouses.Add(hero);
                    }
                }
            }
        }

        /// <summary>
        /// Adds a hero to the player's ex-spouses list.
        /// </summary>
        public static void AddExSpouse(Hero hero)
        {
            if (hero != null && !_playerExSpouses.Contains(hero))
            {
                _playerExSpouses.Add(hero);
                Debug.Print($"[LordLife:MarryAnyone] Added {hero.Name?.ToString() ?? "Unknown"} to ex-spouses list");
            }
        }

        /// <summary>
        /// Checks if a hero is an ex-spouse of the player.
        /// </summary>
        public static bool IsExSpouse(Hero hero)
        {
            return hero != null && _playerExSpouses.Contains(hero);
        }

        /// <summary>
        /// Gets the relationship penalty applied when divorcing.
        /// </summary>
        public static int GetDivorceRelationshipPenalty()
        {
            return DIVORCE_RELATIONSHIP_PENALTY;
        }

        /// <summary>
        /// Executes the marriage between the player and the specified hero.
        /// </summary>
        /// <param name="hero">The hero to marry.</param>
        public static void MarryPlayer(Hero hero)
        {
            if (hero == null || Hero.MainHero == null)
            {
                new InformationMessage(
                    $"Não foi possível realizar o casamento!",
                    Colors.Red
                );
                return;
            }

            // Use the game's marriage action
            MarriageAction.Apply(Hero.MainHero, hero);
        }

        /// <summary>
        /// Executes divorce between the player and their current spouse.
        /// Uses ChangeRomanticStateAction to properly end the marriage, then clears Spouse properties
        /// and adds the ex-spouse to tracking list. Also applies a relationship penalty.
        /// </summary>
        public static void DivorcePlayer()
        {
            Hero player = Hero.MainHero;
            if (player == null || player.Spouse == null)
            {
                Debug.Print("[LordLife:MarryAnyone] Cannot divorce - player has no spouse");
                return;
            }

            Hero spouse = player.Spouse;
            
            // Add to ex-spouses list before clearing spouse reference
            AddExSpouse(spouse);
            
            // Use game's ChangeRomanticStateAction to properly end the marriage
            // This ensures all marriage-related game systems are notified
            ChangeRomanticStateAction.Apply(player, spouse, Romance.RomanceLevelEnum.Ended);
            
            // Clear spouse relationship
            // Note: This is done after ChangeRomanticStateAction to ensure proper state transition
            player.Spouse = null;
            spouse.Spouse = null;
            
            // Apply relationship penalty for divorce
            int currentRelation = CharacterRelationManager.GetHeroRelation(player, spouse);
            CharacterRelationManager.SetHeroRelation(player, spouse, currentRelation + DIVORCE_RELATIONSHIP_PENALTY);
            
            Debug.Print($"[LordLife:MarryAnyone] Divorce completed between {player.Name} and {spouse.Name}");
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
