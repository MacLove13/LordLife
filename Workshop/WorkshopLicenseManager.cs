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
        private static WorkshopLicenseManager? _instance;

        /// <summary>
        /// Dictionary storing the number of extra workshop licenses per clan.
        /// Key: Clan StringId, Value: Number of extra licenses
        /// </summary>
        private Dictionary<string, int> _extraLicenses = new Dictionary<string, int>();

        private WorkshopLicenseManager()
        {
        }

        public static WorkshopLicenseManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new WorkshopLicenseManager();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Resets the singleton instance (used when loading new games).
        /// </summary>
        public static void ResetInstance()
        {
            _instance = new WorkshopLicenseManager();
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
            if (_extraLicenses.ContainsKey(clanStringId))
            {
                _extraLicenses[clanStringId]++;
            }
            else
            {
                _extraLicenses[clanStringId] = 1;
            }
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
        /// </summary>
        public Dictionary<string, int> GetDataForSaving()
        {
            return _extraLicenses;
        }
    }
}
