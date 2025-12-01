using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.LogEntries;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.LordLife.Dialogues
{
    /// <summary>
    /// Helper class to retrieve trade rumors for dialogue responses.
    /// Provides access to real trade information from the game.
    /// </summary>
    public static class TradeRumorHelper
    {
        /// <summary>
        /// Gets a trade rumor with configurable chance.
        /// Returns null if no rumor should be shown (generic responses will be used instead).
        /// </summary>
        /// <param name="chanceToShowRumor">Probability (0.0 to 1.0) of showing a rumor instead of a generic response.</param>
        /// <returns>A trade rumor string in Portuguese, or null if no rumor should be shown.</returns>
        public static string? GetTradeRumor(float chanceToShowRumor = 0.3f)
        {
            // Random chance to show rumor
            if (MBRandom.RandomFloat >= chanceToShowRumor)
                return null;

            try
            {
                // Try getting real trade rumor first
                var logRumor = GetRumorFromLogs();
                if (!string.IsNullOrEmpty(logRumor))
                    return TranslateToPortuguese(logRumor);

                // Generate market-based rumor
                return GenerateMarketBasedRumor();
            }
            catch (Exception ex)
            {
                // Log error but don't crash
                Debug.Print($"[LordLife:Dialogues] Error getting trade rumor: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Attempts to get a trade rumor from game log entries.
        /// Note: Direct access to trade log text is not available in the Bannerlord API.
        /// This method is kept for future implementation but currently returns null.
        /// </summary>
        private static string? GetRumorFromLogs()
        {
            // LogEntry in Bannerlord does not expose text content directly
            // The API doesn't provide GameActionLogText or similar properties
            // Future versions might use reflection or other methods to access log data
            return null;
        }

        /// <summary>
        /// Checks if a log entry is trade-related.
        /// Note: Not currently used as LogEntry text is not accessible via the API.
        /// </summary>
        private static bool IsTradeRelated(LogEntry entry)
        {
            if (entry == null) return false;

            // Check for trade-related types by examining the type name
            // Note: Specific trade log entry types are not exposed in the public API
            var typeName = entry.GetType().Name.ToLower();
            return typeName.Contains("trade") ||
                   typeName.Contains("caravan") ||
                   typeName.Contains("workshop");
        }

        /// <summary>
        /// Generates a market-based rumor using current town data.
        /// </summary>
        private static string GenerateMarketBasedRumor()
        {
            var towns = Town.AllTowns?.Where(t => t?.Settlement != null).ToList();
            
            if (towns == null || towns.Count == 0)
                return "O comércio está estável.";

            var randomTown = towns[MBRandom.RandomInt(towns.Count)];
            
            // Try to generate item-specific rumor
            if (randomTown.Settlement?.ItemRoster != null && 
                randomTown.Settlement.ItemRoster.Count > 0)
            {
                var itemRumor = GenerateItemPriceRumor(randomTown);
                if (!string.IsNullOrEmpty(itemRumor))
                    return itemRumor;
            }

            // Generic town rumor
            return $"Mercadores falam bem de {randomTown.Name}.";
        }

        /// <summary>
        /// Generates a rumor about item prices in a specific town.
        /// </summary>
        private static string? GenerateItemPriceRumor(Town town)
        {
            var roster = town.Settlement.ItemRoster;
            var itemIndex = MBRandom.RandomInt(roster.Count);
            var itemRosterElement = roster.GetElementCopyAtIndex(itemIndex);
            
            // ItemRosterElement contains EquipmentElement, which then contains ItemObject
            var item = itemRosterElement.EquipmentElement.Item;
            
            if (item == null) return null;

            var currentPrice = town.GetItemPrice(item);
            var basePrice = item.Value;
            
            if (basePrice == 0) return null;
            
            var priceRatio = currentPrice / (float)basePrice;

            if (priceRatio > 1.4f)
                return $"Ouvi dizer que {item.Name} está muito caro em {town.Name}.";
            else if (priceRatio > 1.2f)
                return $"{item.Name} está um pouco caro em {town.Name}.";
            else if (priceRatio < 0.6f)
                return $"Há ótimas ofertas de {item.Name} em {town.Name}!";
            else if (priceRatio < 0.8f)
                return $"{item.Name} está barato em {town.Name}.";
            else
                return $"Os preços de {item.Name} estão normais em {town.Name}.";
        }

        /// <summary>
        /// Translates common English trade terms to Portuguese.
        /// </summary>
        private static string TranslateToPortuguese(string englishText)
        {
            // Simple translation for common terms
            return englishText
                .Replace("You made", "Você lucrou")
                .Replace("denars profit", "denars de lucro")
                .Replace("from trading", "comercializando")
                .Replace("Your caravan made", "Sua caravana lucrou");
        }
    }
}
