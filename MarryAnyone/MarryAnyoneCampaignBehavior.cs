using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Bannerlord.LordLife.MarryAnyone
{
    /// <summary>
    /// Campaign behavior that adds dialogue options for romance and marriage
    /// with any character of the opposite sex.
    /// </summary>
    public class MarryAnyoneCampaignBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        public override void SyncData(IDataStore dataStore)
        {
            // No data to sync
        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            AddMarryAnyoneDialogues(campaignGameStarter);
            Debug.Print("[LordLife:MarryAnyone] Diálogos de casamento registrados.");
        }

        private void AddMarryAnyoneDialogues(CampaignGameStarter campaignGameStarter)
        {
            // Add flirtation dialogue option
            campaignGameStarter.AddPlayerLine(
                "marry_anyone_flirt",
                "hero_main_options",
                "marry_anyone_flirt_response",
                "{=marry_anyone_flirt}Eu gostaria de conhecê-lo(a) melhor...",
                MarryAnyoneFlirtCondition,
                null,
                100,
                null,
                null);

            // NPC response to flirtation
            campaignGameStarter.AddDialogLine(
                "marry_anyone_flirt_response",
                "marry_anyone_flirt_response",
                "marry_anyone_flirt_options",
                "{=marry_anyone_flirt_response}Oh? Isso é... interessante. O que você tinha em mente?",
                null,
                null,
                100,
                null);

            // Option to propose marriage
            campaignGameStarter.AddPlayerLine(
                "marry_anyone_propose",
                "marry_anyone_flirt_options",
                "marry_anyone_propose_response",
                "{=marry_anyone_propose}Eu quero casar com você!",
                MarryAnyoneProposeCondition,
                null,
                100,
                null,
                null);

            // Option to back out
            campaignGameStarter.AddPlayerLine(
                "marry_anyone_nevermind",
                "marry_anyone_flirt_options",
                "hero_main_options",
                "{=marry_anyone_nevermind}Esquece, não é nada.",
                null,
                null,
                100,
                null,
                null);

            // NPC accepts marriage proposal
            campaignGameStarter.AddDialogLine(
                "marry_anyone_propose_accept",
                "marry_anyone_propose_response",
                "marry_anyone_marriage_finalize",
                "{=marry_anyone_propose_accept}Aceito seu pedido! Vamos selar nossa união.",
                MarryAnyoneProposalAcceptCondition,
                null,
                100,
                null);

            // NPC rejects marriage proposal
            campaignGameStarter.AddDialogLine(
                "marry_anyone_propose_reject",
                "marry_anyone_propose_response",
                "hero_main_options",
                "{=marry_anyone_propose_reject}Agradeço a oferta, mas não posso aceitar. Talvez precisamos nos conhecer melhor primeiro.",
                MarryAnyoneProposalRejectCondition,
                null,
                99,
                null);

            // Finalize marriage
            campaignGameStarter.AddPlayerLine(
                "marry_anyone_marriage_confirm",
                "marry_anyone_marriage_finalize",
                "close_window",
                "{=marry_anyone_marriage_confirm}Sim, vamos nos casar!",
                null,
                MarryAnyoneMarriageConsequence,
                100,
                null,
                null);

            // Cancel marriage
            campaignGameStarter.AddPlayerLine(
                "marry_anyone_marriage_cancel",
                "marry_anyone_marriage_finalize",
                "hero_main_options",
                "{=marry_anyone_marriage_cancel}Espere, preciso pensar mais sobre isso.",
                null,
                null,
                100,
                null,
                null);
        }

        /// <summary>
        /// Condition for the flirtation dialogue option.
        /// </summary>
        private bool MarryAnyoneFlirtCondition()
        {
            Hero conversationHero = Hero.OneToOneConversationHero;
            return MarryAnyoneRomanceHelper.CanRomance(conversationHero);
        }

        /// <summary>
        /// Condition for the marriage proposal dialogue option.
        /// </summary>
        private bool MarryAnyoneProposeCondition()
        {
            Hero conversationHero = Hero.OneToOneConversationHero;
            return MarryAnyoneRomanceHelper.CanProposeMarriage(conversationHero);
        }

        /// <summary>
        /// Condition for the NPC to accept the marriage proposal.
        /// Uses relationship level to determine acceptance probability.
        /// </summary>
        private bool MarryAnyoneProposalAcceptCondition()
        {
            Hero conversationHero = Hero.OneToOneConversationHero;
            
            if (conversationHero == null)
            {
                return false;
            }

            // Get relationship between player and the hero (-100 to 100)
            int relationshipLevel = CharacterRelationManager.GetHeroRelation(Hero.MainHero, conversationHero);

            // Minimum relationship of 0 required for acceptance
            // Higher relationship increases chance of acceptance
            if (relationshipLevel < 0)
            {
                return false;
            }

            // Base acceptance chance: 50% at relationship 0, increases with relationship
            // At relationship 50+, acceptance is guaranteed
            int acceptanceChance = 50 + relationshipLevel;
            int randomValue = MBRandom.RandomInt(100);

            bool accepted = randomValue < acceptanceChance;

            Debug.Print($"[LordLife:MarryAnyone] Proposal to {conversationHero.Name}: Relationship={relationshipLevel}, Chance={acceptanceChance}%, Roll={randomValue}, Accepted={accepted}");

            return accepted;
        }

        /// <summary>
        /// Condition for the NPC to reject the marriage proposal.
        /// This is the inverse of the accept condition.
        /// </summary>
        private bool MarryAnyoneProposalRejectCondition()
        {
            // This method is called as a fallback when accept condition fails
            // It should always return true to handle the rejection case
            return true;
        }

        /// <summary>
        /// Executes the marriage between the player and the conversation hero.
        /// </summary>
        private void MarryAnyoneMarriageConsequence()
        {
            Hero conversationHero = Hero.OneToOneConversationHero;
            
            if (conversationHero == null)
            {
                return;
            }

            string heroType = MarryAnyoneRomanceHelper.GetHeroTypeDescription(conversationHero);

            MarryAnyoneRomanceHelper.MarryPlayer(conversationHero);

            InformationManager.DisplayMessage(
                new InformationMessage(
                    $"Você se casou com {conversationHero.Name} ({heroType})!",
                    Colors.Green
                )
            );

            Debug.Print($"[LordLife:MarryAnyone] Jogador casou com {conversationHero.Name} ({heroType}).");
        }
    }
}
