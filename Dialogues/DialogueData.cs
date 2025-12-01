using System;
using System.Collections.Generic;

namespace Bannerlord.LordLife.Dialogues
{
    /// <summary>
    /// Represents a single dialogue option that can be presented to the player.
    /// </summary>
    public class DialogueEntry
    {
        /// <summary>
        /// Unique identifier for this dialogue entry.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The text shown to the player in the dialogue menu.
        /// </summary>
        public string PlayerText { get; }

        /// <summary>
        /// Possible responses from the NPC. One will be randomly selected.
        /// </summary>
        public List<DialogueResponse> Responses { get; }

        /// <summary>
        /// The type of dialogue, used for cooldown management.
        /// </summary>
        public DialogueType Type { get; }

        /// <summary>
        /// Minimum relationship required to unlock this dialogue.
        /// </summary>
        public int MinRelationship { get; }

        /// <summary>
        /// Number of days this dialogue is blocked after use (for normal dialogues).
        /// </summary>
        public int CooldownDays { get; }

        /// <summary>
        /// Priority of this dialogue option (higher = appears higher in menu).
        /// </summary>
        public int Priority { get; }

        public DialogueEntry(
            string id,
            string playerText,
            List<DialogueResponse> responses,
            DialogueType type = DialogueType.Basic,
            int minRelationship = 0,
            int cooldownDays = 3,
            int priority = 100)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            PlayerText = playerText ?? throw new ArgumentNullException(nameof(playerText));
            Responses = responses ?? throw new ArgumentNullException(nameof(responses));
            Type = type;
            MinRelationship = minRelationship;
            CooldownDays = cooldownDays;
            Priority = priority;
        }
    }

    /// <summary>
    /// Represents a possible NPC response to a dialogue option.
    /// </summary>
    public class DialogueResponse
    {
        /// <summary>
        /// The text the NPC will say.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Relationship change when this response is given. Can be positive or negative.
        /// </summary>
        public int RelationshipChange { get; }

        public DialogueResponse(string text, int relationshipChange = 0)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            RelationshipChange = relationshipChange;
        }
    }

    /// <summary>
    /// Types of dialogues for managing different cooldown behaviors.
    /// </summary>
    public enum DialogueType
    {
        /// <summary>
        /// Basic dialogues that have a simple day-based cooldown.
        /// </summary>
        Basic,

        /// <summary>
        /// Relationship-based dialogues that unlock at certain relationship levels.
        /// </summary>
        Relationship,

        /// <summary>
        /// War-related dialogues that only reset when a new war starts.
        /// </summary>
        War,

        /// <summary>
        /// Condolence dialogues that only reset when another relative dies.
        /// </summary>
        DeathCondolence
    }

    /// <summary>
    /// Tracks cooldown state for a dialogue with a specific NPC.
    /// </summary>
    public class DialogueCooldownEntry
    {
        /// <summary>
        /// The game day when this dialogue can be used again (for Basic/Relationship types).
        /// </summary>
        public float UnlocksAtDay { get; set; }

        /// <summary>
        /// For War dialogues: the war ID that was active when dialogue was used.
        /// </summary>
        public string? LastWarId { get; set; }

        /// <summary>
        /// For DeathCondolence dialogues: the ID of the deceased relative that was condoled.
        /// </summary>
        public string? LastCondoledRelativeId { get; set; }

        public DialogueCooldownEntry()
        {
            UnlocksAtDay = 0;
            LastWarId = null;
            LastCondoledRelativeId = null;
        }
    }
}
