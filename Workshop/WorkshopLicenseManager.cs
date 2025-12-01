using System;
using System.Collections.Generic;
using TaleWorlds.SaveSystem;

namespace Bannerlord.LordLife.Workshop
{
    /// <summary>
    /// Singleton manager to track extra workshop licenses purchased by clans.
    /// This data persists across save games.
    /// </summary>
    public class WorkshopLicenseManager
    {
        private static readonly Lazy<WorkshopLicenseManager> _lazyInstance = 
            new Lazy<WorkshopLicenseManager>(() => new WorkshopLicenseManager());

        /// <summary>
        /// Dictionary storing the number of extra workshop licenses per clan.
        /// Key: Clan StringId, Value: Number of extra licenses
        /// </summary>
        private Dictionary<string, int> _extraLicenses = new Dictionary<string, int>();

        private WorkshopLicenseManager()
        {
        }

        public static WorkshopLicenseManager Instance => _lazyInstance.Value;

        /// <summary>
        /// Resets the singleton instance (used when loading new games).
        /// </summary>
        public static void ResetInstance()
        {
            // Clear the existing data
            _lazyInstance.Value._extraLicenses = new Dictionary<string, int>();
        }

        /// <summary>
        /// Gets the number of extra workshop licenses for a clan.
        /// </summary>
        public int GetExtraLicenses(string clanStringId)
        {
            if (_extraLicenses.TryGetValue(clanStringId, out int count))
            {
                return count;
            }
            return 0;
        }

        /// <summary>
        /// Adds one extra workshop license for a clan.
        /// </summary>
        public void AddWorkshopLicense(string clanStringId)
        {
            _extraLicenses[clanStringId] = _extraLicenses.TryGetValue(clanStringId, out var count) ? count + 1 : 1;
        }

        /// <summary>
        /// Loads extra licenses data from the save system.
        /// </summary>
        public void LoadData(Dictionary<string, int> data)
        {
            _extraLicenses = data ?? new Dictionary<string, int>();
        }

        /// <summary>
        /// Gets the extra licenses data for saving.
        /// Returns a copy to prevent external modifications.
        /// </summary>
        public Dictionary<string, int> GetDataForSaving()
        {
            return new Dictionary<string, int>(_extraLicenses);
        }
    }
}
