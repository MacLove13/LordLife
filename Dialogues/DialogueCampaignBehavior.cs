using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace Bannerlord.LordLife.Dialogues
{
    /// <summary>
    /// Campaign behavior that manages the custom dialogue system.
    /// Handles dialogue registration, cooldowns, and state persistence.
    /// </summary>
    public class DialogueCampaignBehavior : CampaignBehaviorBase
    {
        // Dictionary structure: [HeroId][DialogueId] = CooldownEntry
        private Dictionary<string, Dictionary<string, DialogueCooldownEntry>> _dialogueCooldowns;

        // Tracks which relatives have died for each hero since last conversation
        // [HeroId] = List of deceased relative IDs
        private Dictionary<string, List<string>> _deceasedRelatives;

        // Tracks active wars for war dialogue reset logic
        // [KingdomId] = List of enemy kingdom IDs
        private Dictionary<string, HashSet<string>> _activeWars;

        // Current dialogue being used (for consequence methods)
        private DialogueEntry? _currentDialogue;

        // Current hero being talked to
        private Hero? _currentConversationHero;

        // Deceased relative for condolence dialogue
        private Hero? _currentDeceasedRelative;

        // Index of the selected response for the current dialogue
        private int _selectedResponseIndex;

        public DialogueCampaignBehavior()
        {
            _dialogueCooldowns = new Dictionary<string, Dictionary<string, DialogueCooldownEntry>>();
            _deceasedRelatives = new Dictionary<string, List<string>>();
            _activeWars = new Dictionary<string, HashSet<string>>();
            _selectedResponseIndex = -1;
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
            CampaignEvents.HeroKilledEvent.AddNonSerializedListener(this, OnHeroKilled);
            CampaignEvents.WarDeclared.AddNonSerializedListener(this, OnWarDeclared);
            CampaignEvents.MakePeace.AddNonSerializedListener(this, OnPeaceMade);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("dialogueCooldowns", ref _dialogueCooldowns);
            dataStore.SyncData("deceasedRelatives", ref _deceasedRelatives);
            dataStore.SyncData("activeWars", ref _activeWars);

            // Ensure dictionaries are initialized after loading
            _dialogueCooldowns ??= new Dictionary<string, Dictionary<string, DialogueCooldownEntry>>();
            _deceasedRelatives ??= new Dictionary<string, List<string>>();
            _activeWars ??= new Dictionary<string, HashSet<string>>();
        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            RegisterDialogues(campaignGameStarter);
            UpdateActiveWars();
            Debug.Print("[LordLife:Dialogues] Sistema de diálogos registrado.");
        }

        private void OnDailyTick()
        {
            // Clean up expired cooldowns periodically
            CleanupExpiredCooldowns();
        }

        private void OnHeroKilled(Hero victim, Hero killer, KillCharacterAction.KillCharacterActionDetail detail, bool showNotification)
        {
            if (victim == null)
            {
                return;
            }

            // Track deceased relatives for condolence dialogues
            TrackDeceasedRelative(victim);
        }

        private void OnWarDeclared(IFaction faction1, IFaction faction2, DeclareWarAction.DeclareWarDetail detail)
        {
            UpdateActiveWars();
        }

        private void OnPeaceMade(IFaction faction1, IFaction faction2, MakePeaceAction.MakePeaceDetail detail)
        {
            UpdateActiveWars();
        }

        /// <summary>
        /// Tracks a deceased hero for their family members' condolence dialogues.
        /// </summary>
        private void TrackDeceasedRelative(Hero deceased)
        {
            // Get all family members of the deceased
            var familyMembers = GetFamilyMembers(deceased);

            foreach (var familyMember in familyMembers)
            {
                if (familyMember == null || familyMember == Hero.MainHero)
                {
                    continue;
                }

                string heroId = GetHeroId(familyMember);
                string deceasedId = GetHeroId(deceased);

                if (!_deceasedRelatives.ContainsKey(heroId))
                {
                    _deceasedRelatives[heroId] = new List<string>();
                }

                if (!_deceasedRelatives[heroId].Contains(deceasedId))
                {
                    _deceasedRelatives[heroId].Add(deceasedId);
                    Debug.Print($"[LordLife:Dialogues] Tracked death of {deceased.Name} for {familyMember.Name}'s condolence dialogue.");
                }
            }
        }

        /// <summary>
        /// Gets all family members of a hero.
        /// </summary>
        private IEnumerable<Hero> GetFamilyMembers(Hero hero)
        {
            var family = new List<Hero>();

            if (hero.Father != null) family.Add(hero.Father);
            if (hero.Mother != null) family.Add(hero.Mother);
            if (hero.Spouse != null) family.Add(hero.Spouse);

            foreach (var child in hero.Children)
            {
                if (child != null) family.Add(child);
            }

            foreach (var sibling in hero.Siblings)
            {
                if (sibling != null) family.Add(sibling);
            }

            return family;
        }

        /// <summary>
        /// Updates the tracking of active wars.
        /// </summary>
        private void UpdateActiveWars()
        {
            _activeWars.Clear();

            foreach (var kingdom in Kingdom.All)
            {
                if (kingdom == null) continue;

                string kingdomId = kingdom.StringId;
                _activeWars[kingdomId] = new HashSet<string>();

                foreach (var enemy in kingdom.Stances)
                {
                    if (enemy.IsAtWar && enemy.Faction2 is Kingdom enemyKingdom)
                    {
                        _activeWars[kingdomId].Add(enemyKingdom.StringId);
                    }
                }
            }
        }

        /// <summary>
        /// Generates a unique war identifier for the current wars.
        /// </summary>
        private string GetCurrentWarId(Hero hero)
        {
            var kingdom = hero.Clan?.Kingdom;
            if (kingdom == null) return string.Empty;

            if (!_activeWars.TryGetValue(kingdom.StringId, out var enemies) || enemies.Count == 0)
            {
                return string.Empty;
            }

            // Create a deterministic war ID from sorted enemy list
            var sortedEnemies = enemies.OrderBy(e => e).ToList();
            return $"{kingdom.StringId}_war_{string.Join("_", sortedEnemies)}";
        }

        /// <summary>
        /// Cleans up expired cooldowns to prevent memory bloat.
        /// </summary>
        private void CleanupExpiredCooldowns()
        {
            float currentDay = (float)CampaignTime.Now.ToDays;

            // Collect keys to remove to avoid modifying collection during iteration
            var heroKeysToRemove = new List<string>();

            foreach (var heroEntry in _dialogueCooldowns)
            {
                var dialogueKeysToRemove = new List<string>();

                foreach (var dialogueEntry in heroEntry.Value)
                {
                    // Only clean up basic/relationship cooldowns, not war/death ones
                    if (dialogueEntry.Value.UnlocksAtDay > 0 && dialogueEntry.Value.UnlocksAtDay < currentDay - 30)
                    {
                        dialogueKeysToRemove.Add(dialogueEntry.Key);
                    }
                }

                foreach (var key in dialogueKeysToRemove)
                {
                    heroEntry.Value.Remove(key);
                }

                if (heroEntry.Value.Count == 0)
                {
                    heroKeysToRemove.Add(heroEntry.Key);
                }
            }

            foreach (var key in heroKeysToRemove)
            {
                _dialogueCooldowns.Remove(key);
            }
        }

        /// <summary>
        /// Registers all dialogues with the campaign game starter.
        /// </summary>
        private void RegisterDialogues(CampaignGameStarter campaignGameStarter)
        {
            // Register all dialogue entries
            foreach (var dialogue in DialogueDefinitions.AllDialogues)
            {
                RegisterDialogueEntry(campaignGameStarter, dialogue);
            }
        }

        /// <summary>
        /// Registers a single dialogue entry with the game.
        /// </summary>
        private void RegisterDialogueEntry(CampaignGameStarter campaignGameStarter, DialogueEntry dialogue)
        {
            string playerLineId = $"lordlife_player_{dialogue.Id}";
            string npcResponseId = $"lordlife_response_{dialogue.Id}";
            string continueId = $"lordlife_continue_{dialogue.Id}";

            // Player dialogue option
            campaignGameStarter.AddPlayerLine(
                playerLineId,
                "hero_main_options",
                npcResponseId,
                $"{{={playerLineId}}}{GetDialoguePlayerText(dialogue)}",
                () => CanShowDialogue(dialogue),
                () => OnDialogueSelected(dialogue),
                dialogue.Priority,
                null,
                null);

            // NPC response (randomly selected from available responses)
            campaignGameStarter.AddDialogLine(
                npcResponseId,
                npcResponseId,
                continueId,
                $"{{={npcResponseId}}}{{LORDLIFE_NPC_RESPONSE}}",
                () => _currentDialogue == dialogue,
                () => ApplyDialogueConsequence(dialogue),
                100,
                null);

            // Continue/back option
            campaignGameStarter.AddPlayerLine(
                continueId,
                continueId,
                "hero_main_options",
                "{=lordlife_continue}Entendo...",
                null,
                null,
                100,
                null,
                null);
        }

        /// <summary>
        /// Gets the player text for a dialogue, handling special cases like death condolence.
        /// </summary>
        private string GetDialoguePlayerText(DialogueEntry dialogue)
        {
            if (dialogue.Type == DialogueType.DeathCondolence)
            {
                // The actual text will be set dynamically in CanShowDialogue
                return "{LORDLIFE_CONDOLENCE_TEXT}";
            }
            return dialogue.PlayerText;
        }

        /// <summary>
        /// Determines if a dialogue option should be shown.
        /// </summary>
        private bool CanShowDialogue(DialogueEntry dialogue)
        {
            Hero conversationHero = Hero.OneToOneConversationHero;

            if (conversationHero == null)
            {
                return false;
            }

            // Check if hero is a valid type (lord, companion, notable)
            if (!IsValidHeroType(conversationHero))
            {
                return false;
            }

            // Check relationship requirement
            int relationship = CharacterRelationManager.GetHeroRelation(Hero.MainHero, conversationHero);
            if (relationship < dialogue.MinRelationship)
            {
                return false;
            }

            // Check type-specific conditions
            switch (dialogue.Type)
            {
                case DialogueType.Basic:
                case DialogueType.Relationship:
                    return CanShowBasicDialogue(conversationHero, dialogue);

                case DialogueType.War:
                    return CanShowWarDialogue(conversationHero, dialogue);

                case DialogueType.DeathCondolence:
                    return CanShowCondolenceDialogue(conversationHero, dialogue);

                default:
                    return false;
            }
        }

        /// <summary>
        /// Checks if a hero is a valid type for dialogues.
        /// </summary>
        private bool IsValidHeroType(Hero hero)
        {
            return hero.IsLord || hero.IsWanderer || hero.IsNotable;
        }

        /// <summary>
        /// Checks if a basic/relationship dialogue can be shown (cooldown check).
        /// </summary>
        private bool CanShowBasicDialogue(Hero hero, DialogueEntry dialogue)
        {
            string heroId = GetHeroId(hero);
            float currentDay = (float)CampaignTime.Now.ToDays;

            if (_dialogueCooldowns.TryGetValue(heroId, out var heroCooldowns))
            {
                if (heroCooldowns.TryGetValue(dialogue.Id, out var cooldown))
                {
                    if (currentDay < cooldown.UnlocksAtDay)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if a war dialogue can be shown.
        /// Requires: same kingdom, kingdom at war, not used in current war.
        /// </summary>
        private bool CanShowWarDialogue(Hero hero, DialogueEntry dialogue)
        {
            // Check if both are in the same kingdom
            var playerKingdom = Hero.MainHero.Clan?.Kingdom;
            var heroKingdom = hero.Clan?.Kingdom;

            if (playerKingdom == null || heroKingdom == null || playerKingdom != heroKingdom)
            {
                return false;
            }

            // Check if the kingdom is at war
            if (!_activeWars.TryGetValue(playerKingdom.StringId, out var enemies) || enemies.Count == 0)
            {
                return false;
            }

            // Check if this dialogue was already used in the current war
            string heroId = GetHeroId(hero);
            string currentWarId = GetCurrentWarId(hero);

            if (_dialogueCooldowns.TryGetValue(heroId, out var heroCooldowns))
            {
                if (heroCooldowns.TryGetValue(dialogue.Id, out var cooldown))
                {
                    if (cooldown.LastWarId == currentWarId)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if a condolence dialogue can be shown.
        /// Requires: hero has a deceased relative that hasn't been condoled yet.
        /// </summary>
        private bool CanShowCondolenceDialogue(Hero hero, DialogueEntry dialogue)
        {
            string heroId = GetHeroId(hero);

            if (!_deceasedRelatives.TryGetValue(heroId, out var deceasedList) || deceasedList.Count == 0)
            {
                return false;
            }

            // Check if there's a deceased relative we haven't condoled yet
            if (!_dialogueCooldowns.TryGetValue(heroId, out var heroCooldowns))
            {
                heroCooldowns = new Dictionary<string, DialogueCooldownEntry>();
            }

            // Find a deceased relative not yet condoled
            foreach (var deceasedId in deceasedList)
            {
                bool alreadyCondoled = false;

                if (heroCooldowns.TryGetValue(dialogue.Id, out var cooldown))
                {
                    if (cooldown.LastCondoledRelativeId == deceasedId)
                    {
                        alreadyCondoled = true;
                    }
                }

                if (!alreadyCondoled)
                {
                    // Find the deceased hero to get their name
                    var deceased = FindHeroById(deceasedId);
                    if (deceased != null)
                    {
                        _currentDeceasedRelative = deceased;

                        // Set the dialogue text with the relative's name
                        string condolenceText = dialogue.PlayerText.Replace("{RELATIVE_NAME}", deceased.Name.ToString());
                        MBTextManager.SetTextVariable("LORDLIFE_CONDOLENCE_TEXT", condolenceText);

                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Called when a dialogue option is selected by the player.
        /// </summary>
        private void OnDialogueSelected(DialogueEntry dialogue)
        {
            _currentDialogue = dialogue;
            _currentConversationHero = Hero.OneToOneConversationHero;
            _selectedResponseIndex = -1;

            // Select a random response
            if (dialogue.Responses.Count > 0)
            {
                _selectedResponseIndex = MBRandom.RandomInt(dialogue.Responses.Count);
                var response = dialogue.Responses[_selectedResponseIndex];

                string responseText = response.Text;

                // Handle condolence text replacement
                if (dialogue.Type == DialogueType.DeathCondolence && _currentDeceasedRelative != null)
                {
                    responseText = responseText.Replace("{RELATIVE_NAME}", _currentDeceasedRelative.Name.ToString());
                }

                MBTextManager.SetTextVariable("LORDLIFE_NPC_RESPONSE", responseText);
            }
        }

        /// <summary>
        /// Applies the consequence of a dialogue (relationship change, cooldown).
        /// </summary>
        private void ApplyDialogueConsequence(DialogueEntry dialogue)
        {
            if (_currentConversationHero == null || _currentDialogue == null)
            {
                return;
            }

            // Apply relationship change from the selected response using stored index
            if (_selectedResponseIndex >= 0 && _selectedResponseIndex < dialogue.Responses.Count)
            {
                var response = dialogue.Responses[_selectedResponseIndex];

                if (response.RelationshipChange != 0)
                {
                    ChangeRelationAction.ApplyRelationChangeBetweenHeroes(
                        Hero.MainHero,
                        _currentConversationHero,
                        response.RelationshipChange,
                        true);

                    if (response.RelationshipChange > 0)
                    {
                        InformationManager.DisplayMessage(
                            new InformationMessage(
                                $"Relacionamento com {_currentConversationHero.Name} aumentou em {response.RelationshipChange}.",
                                Colors.Green));
                    }
                    else
                    {
                        InformationManager.DisplayMessage(
                            new InformationMessage(
                                $"Relacionamento com {_currentConversationHero.Name} diminuiu em {-response.RelationshipChange}.",
                                Colors.Red));
                    }
                }
            }

            // Apply cooldown
            ApplyCooldown(_currentConversationHero, dialogue);

            // Reset state
            _currentDialogue = null;
            _currentConversationHero = null;
            _currentDeceasedRelative = null;
            _selectedResponseIndex = -1;
        }

        /// <summary>
        /// Applies the appropriate cooldown for a dialogue.
        /// </summary>
        private void ApplyCooldown(Hero hero, DialogueEntry dialogue)
        {
            string heroId = GetHeroId(hero);

            if (!_dialogueCooldowns.ContainsKey(heroId))
            {
                _dialogueCooldowns[heroId] = new Dictionary<string, DialogueCooldownEntry>();
            }

            if (!_dialogueCooldowns[heroId].ContainsKey(dialogue.Id))
            {
                _dialogueCooldowns[heroId][dialogue.Id] = new DialogueCooldownEntry();
            }

            var cooldown = _dialogueCooldowns[heroId][dialogue.Id];

            switch (dialogue.Type)
            {
                case DialogueType.Basic:
                case DialogueType.Relationship:
                    cooldown.UnlocksAtDay = (float)CampaignTime.Now.ToDays + dialogue.CooldownDays;
                    Debug.Print($"[LordLife:Dialogues] Dialogue '{dialogue.Id}' with {hero.Name} on cooldown until day {cooldown.UnlocksAtDay}");
                    break;

                case DialogueType.War:
                    cooldown.LastWarId = GetCurrentWarId(hero);
                    Debug.Print($"[LordLife:Dialogues] War dialogue '{dialogue.Id}' with {hero.Name} locked for war: {cooldown.LastWarId}");
                    break;

                case DialogueType.DeathCondolence:
                    if (_currentDeceasedRelative != null)
                    {
                        cooldown.LastCondoledRelativeId = GetHeroId(_currentDeceasedRelative);
                        Debug.Print($"[LordLife:Dialogues] Condolence dialogue for {_currentDeceasedRelative.Name} with {hero.Name} completed.");
                    }
                    break;
            }
        }

        /// <summary>
        /// Gets a unique identifier for a hero.
        /// </summary>
        private string GetHeroId(Hero hero)
        {
            return hero.StringId ?? hero.Name.ToString();
        }

        /// <summary>
        /// Finds a hero by their ID.
        /// </summary>
        private Hero? FindHeroById(string heroId)
        {
            return Hero.AllAliveHeroes.FirstOrDefault(h => GetHeroId(h) == heroId)
                ?? Hero.DeadOrDisabledHeroes.FirstOrDefault(h => GetHeroId(h) == heroId);
        }
    }
}
