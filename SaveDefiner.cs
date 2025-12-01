using TaleWorlds.SaveSystem;
using Bannerlord.LordLife.Dialogues;

namespace Bannerlord.LordLife
{
    /// <summary>
    /// Defines all custom saveable types for the LordLife mod.
    /// This is required by Bannerlord's save system to properly serialize custom types.
    /// </summary>
    public class SaveDefiner : SaveableTypeDefiner
    {
        // Use a unique base ID for this mod to avoid conflicts with other mods
        // This should be a large number to reduce chances of collision
        public SaveDefiner() : base(2891400)
        {
        }

        protected override void DefineClassTypes()
        {
            // Register DialogueCooldownEntry for serialization
            AddClassDefinition(typeof(DialogueCooldownEntry), 1);
        }

        protected override void DefineContainerDefinitions()
        {
            // Register container types used by the mod
            ConstructContainerDefinition(typeof(System.Collections.Generic.Dictionary<string, string>));
            ConstructContainerDefinition(typeof(System.Collections.Generic.Dictionary<string, int>));
            ConstructContainerDefinition(typeof(System.Collections.Generic.Dictionary<string, DialogueCooldownEntry>));
            ConstructContainerDefinition(typeof(System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, DialogueCooldownEntry>>));
            ConstructContainerDefinition(typeof(System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>>));
            ConstructContainerDefinition(typeof(System.Collections.Generic.Dictionary<string, System.Collections.Generic.HashSet<string>>));
            ConstructContainerDefinition(typeof(System.Collections.Generic.List<string>));
            ConstructContainerDefinition(typeof(System.Collections.Generic.HashSet<string>));
        }
    }
}
