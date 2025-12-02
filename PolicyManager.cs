using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.LordLife
{
    /// <summary>
    /// Manages access to custom policies defined in XML.
    /// </summary>
    public static class PolicyManager
    {
        private const string FEUDOS_LEAIS_POLICY_ID = "feudos_leais";

        /// <summary>
        /// Gets the Feudos Leais policy object from the game's object manager.
        /// Returns null if the policy hasn't been loaded yet.
        /// </summary>
        public static PolicyObject? GetFeudosLeaisPolicy()
        {
            if (Game.Current?.ObjectManager == null)
            {
                return null;
            }

            try
            {
                var policy = Game.Current.ObjectManager.GetObject<PolicyObject>(FEUDOS_LEAIS_POLICY_ID);
                return policy;
            }
            catch (System.Exception ex)
            {
                Debug.Print($"[LordLife:PolicyManager] Failed to find policy with ID '{FEUDOS_LEAIS_POLICY_ID}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Checks if the Feudos Leais policy is active in the given kingdom.
        /// </summary>
        public static bool IsFeudosLeaisPolicyActive(Kingdom kingdom)
        {
            if (kingdom == null)
            {
                return false;
            }

            var policy = GetFeudosLeaisPolicy();
            if (policy == null)
            {
                return false;
            }

            return kingdom.ActivePolicies.Contains(policy);
        }
    }
}
