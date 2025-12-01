using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Bannerlord.LordLife
{
    /// <summary>
    /// Campaign behavior that monitors settlement loyalty and triggers voting
    /// when loyalty stays below 21 for 30 days.
    /// This allows the kingdom to vote for a new lord to take ownership.
    /// </summary>
    public class LowLoyaltyVotingBehavior : CampaignBehaviorBase
    {
        // Dictionary to track how many days each settlement has had low loyalty
        // Key: Settlement.StringId, Value: Number of consecutive days with loyalty below 21
        private Dictionary<string, int> _lowLoyaltyDays;

        // Tracks settlements that are currently under voting to avoid duplicate votes
        private HashSet<string> _settlementsUnderVoting;

        private const float LOYALTY_THRESHOLD = 21f;
        private const int DAYS_REQUIRED = 30;

        public LowLoyaltyVotingBehavior()
        {
            _lowLoyaltyDays = new Dictionary<string, int>();
            _settlementsUnderVoting = new HashSet<string>();
        }

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, OnDailyTickSettlement);
            CampaignEvents.KingdomDecisionConcluded.AddNonSerializedListener(this, OnKingdomDecisionConcluded);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("lowLoyaltyDays", ref _lowLoyaltyDays);
            dataStore.SyncData("settlementsUnderVoting", ref _settlementsUnderVoting);

            // Ensure dictionaries are initialized after loading
            _lowLoyaltyDays ??= new Dictionary<string, int>();
            _settlementsUnderVoting ??= new HashSet<string>();
        }

        /// <summary>
        /// Called daily for each settlement to check loyalty levels.
        /// </summary>
        private void OnDailyTickSettlement(Settlement settlement)
        {
            // Only process towns and castles that belong to a kingdom
            if (settlement == null || (!settlement.IsTown && !settlement.IsCastle))
            {
                return;
            }

            // Must have an owner clan and be part of a kingdom
            if (settlement.OwnerClan == null || settlement.OwnerClan.Kingdom == null)
            {
                return;
            }

            // Skip if settlement is already under voting
            if (_settlementsUnderVoting.Contains(settlement.StringId))
            {
                return;
            }

            // Check loyalty (for towns, this is in settlement.Town.Loyalty)
            float loyalty = GetSettlementLoyalty(settlement);

            string settlementId = settlement.StringId;

            if (loyalty < LOYALTY_THRESHOLD)
            {
                // Increment days counter
                if (!_lowLoyaltyDays.ContainsKey(settlementId))
                {
                    _lowLoyaltyDays[settlementId] = 0;
                }

                _lowLoyaltyDays[settlementId]++;

                Debug.Print($"[LordLife:LowLoyaltyVoting] {settlement.Name} has had low loyalty ({loyalty:F1}) for {_lowLoyaltyDays[settlementId]} days.");

                // Check if threshold reached
                if (_lowLoyaltyDays[settlementId] >= DAYS_REQUIRED)
                {
                    TriggerOwnershipVote(settlement);
                }
            }
            else
            {
                // Reset counter if loyalty recovered
                if (_lowLoyaltyDays.ContainsKey(settlementId))
                {
                    Debug.Print($"[LordLife:LowLoyaltyVoting] {settlement.Name} loyalty recovered to {loyalty:F1}. Resetting counter.");
                    _lowLoyaltyDays.Remove(settlementId);
                }
            }
        }

        /// <summary>
        /// Gets the loyalty value for a settlement.
        /// </summary>
        private float GetSettlementLoyalty(Settlement settlement)
        {
            if (settlement.IsTown && settlement.Town != null)
            {
                return settlement.Town.Loyalty;
            }
            else if (settlement.IsCastle && settlement.Town != null)
            {
                // Castles also use Town component for loyalty
                return settlement.Town.Loyalty;
            }
            return 100f; // Default high value if not applicable
        }

        /// <summary>
        /// Triggers a kingdom vote to decide the new owner of a settlement.
        /// </summary>
        private void TriggerOwnershipVote(Settlement settlement)
        {
            Kingdom kingdom = settlement.OwnerClan?.Kingdom;
            if (kingdom == null)
            {
                return;
            }

            string settlementId = settlement.StringId;

            // Mark as under voting
            _settlementsUnderVoting.Add(settlementId);

            // Reset the days counter
            _lowLoyaltyDays.Remove(settlementId);

            // Notify player
            string settlementType = settlement.IsTown ? "cidade" : "castelo";
            InformationManager.DisplayMessage(
                new InformationMessage(
                    $"[LordLife] A lealdade de {settlement.Name} ficou abaixo de 21 por 30 dias. Uma votação para novo lorde foi iniciada!",
                    Colors.Yellow
                )
            );

            Debug.Print($"[LordLife:LowLoyaltyVoting] Triggering ownership vote for {settlement.Name} ({settlementType}) in kingdom {kingdom.Name}.");

            // Create and propose a kingdom decision for settlement ownership
            CreateSettlementClaimantDecision(kingdom, settlement);
        }

        /// <summary>
        /// Creates and proposes a settlement claimant decision to the kingdom.
        /// </summary>
        private void CreateSettlementClaimantDecision(Kingdom kingdom, Settlement settlement)
        {
            // Get potential claimants (clan leaders of the kingdom)
            var potentialClaimants = GetPotentialClaimants(kingdom, settlement);

            if (potentialClaimants.Count == 0)
            {
                Debug.Print($"[LordLife:LowLoyaltyVoting] No valid claimants found for {settlement.Name}. Aborting vote.");
                _settlementsUnderVoting.Remove(settlement.StringId);
                return;
            }

            // Create a SettlementClaimantDecision
            // This is Bannerlord's built-in decision type for distributing fiefs
            var decision = new SettlementClaimantDecision(
                kingdom.RulingClan,
                settlement,
                settlement.OwnerClan.Leader,
                potentialClaimants.FirstOrDefault()
            );

            // Add the decision to the kingdom
            kingdom.AddDecision(decision, true);

            Debug.Print($"[LordLife:LowLoyaltyVoting] Settlement claimant decision created for {settlement.Name}. Vote in progress.");
        }

        /// <summary>
        /// Gets the list of potential claimants for a settlement.
        /// Excludes the current owner.
        /// </summary>
        private List<Clan> GetPotentialClaimants(Kingdom kingdom, Settlement settlement)
        {
            var claimants = new List<Clan>();

            foreach (Clan clan in kingdom.Clans)
            {
                // Exclude current owner and minor factions
                if (clan != settlement.OwnerClan && !clan.IsMinorFaction && clan.Leader != null && clan.Leader.IsAlive)
                {
                    claimants.Add(clan);
                }
            }

            return claimants;
        }

        /// <summary>
        /// Called when a kingdom decision is concluded.
        /// Used to clean up tracking when a settlement vote is completed.
        /// </summary>
        private void OnKingdomDecisionConcluded(KingdomDecision decision, DecisionOutcome outcome, bool isPlayerInvolved)
        {
            if (decision is SettlementClaimantDecision settlementDecision)
            {
                Settlement settlement = settlementDecision.Settlement;
                if (settlement != null && _settlementsUnderVoting.Contains(settlement.StringId))
                {
                    _settlementsUnderVoting.Remove(settlement.StringId);
                    
                    Debug.Print($"[LordLife:LowLoyaltyVoting] Vote for {settlement.Name} concluded. New owner: {settlement.OwnerClan?.Name}");

                    // Notify player of the result
                    InformationManager.DisplayMessage(
                        new InformationMessage(
                            $"[LordLife] A votação para {settlement.Name} foi concluída. O novo lorde é {settlement.OwnerClan?.Leader?.Name}.",
                            Colors.Green
                        )
                    );
                }
            }
        }
    }
}
