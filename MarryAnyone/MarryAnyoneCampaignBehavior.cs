using System;
using System.Collections.Generic;
using System.Linq;
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
        // Relationship bonus per correct answer during courtship
        private const int RELATION_BONUS_PER_CORRECT_ANSWER = 5;

        // Track conversation state for courtship questions
        private int _correctAnswers = 0;

        // Save data: track heroes who completed courtship
        private List<Hero> _savedCourtshipCompletedHeroes = new List<Hero>();
        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        public override void SyncData(IDataStore dataStore)
        {
            // Save/load courtship completed heroes
            dataStore.SyncData("_marryAnyoneCourtshipCompleted", ref _savedCourtshipCompletedHeroes);

            // After loading, restore the data to the static helper
            if (dataStore.IsLoading)
            {
                MarryAnyoneRomanceHelper.SetCompletedCourtshipHeroes(_savedCourtshipCompletedHeroes);
            }
            // Before saving, get the data from the static helper
            else if (dataStore.IsSaving)
            {
                _savedCourtshipCompletedHeroes = MarryAnyoneRomanceHelper.GetCompletedCourtshipHeroes().ToList();
            }
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
                MarryAnyoneFlirtConsequence,
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

            // Option to start courtship questions
            campaignGameStarter.AddPlayerLine(
                "marry_anyone_start_courtship",
                "marry_anyone_flirt_options",
                "marry_anyone_courtship_questions",
                "{=marry_anyone_start_courtship}Gostaria de conhecê-lo(a) melhor. Posso fazer algumas perguntas?",
                MarryAnyoneCourtshipQuestionsCondition,
                MarryAnyoneStartCourtshipConsequence,
                100,
                null,
                null);

            // Option to propose marriage (only after courtship)
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

            // === Courtship Questions System ===
            AddCourtshipQuestionDialogues(campaignGameStarter);

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
        /// Adds courtship question dialogues similar to vanilla game.
        /// </summary>
        private void AddCourtshipQuestionDialogues(CampaignGameStarter campaignGameStarter)
        {
            // NPC agrees to answer questions
            campaignGameStarter.AddDialogLine(
                "marry_anyone_courtship_start",
                "marry_anyone_courtship_questions",
                "marry_anyone_question_1",
                "{=marry_anyone_courtship_start}Claro, pergunte o que quiser. É bom nos conhecermos melhor.",
                null,
                null,
                100,
                null);

            // Question 1: What do you value most?
            campaignGameStarter.AddDialogLine(
                "marry_anyone_question_1",
                "marry_anyone_question_1",
                "marry_anyone_answer_1",
                "{=marry_anyone_question_1}Vou lhe fazer uma pergunta: O que você mais valoriza em um relacionamento?",
                null,
                null,
                100,
                null);

            // Answer 1a: Honesty
            campaignGameStarter.AddPlayerLine(
                "marry_anyone_answer_1a",
                "marry_anyone_answer_1",
                "marry_anyone_question_2",
                "{=marry_anyone_answer_1a}Honestidade acima de tudo.",
                null,
                () => MarryAnyoneAnswerConsequence(true),
                100,
                null,
                null);

            // Answer 1b: Loyalty
            campaignGameStarter.AddPlayerLine(
                "marry_anyone_answer_1b",
                "marry_anyone_answer_1",
                "marry_anyone_question_2",
                "{=marry_anyone_answer_1b}Lealdade e compromisso.",
                null,
                () => MarryAnyoneAnswerConsequence(true),
                100,
                null,
                null);

            // Answer 1c: Wealth
            campaignGameStarter.AddPlayerLine(
                "marry_anyone_answer_1c",
                "marry_anyone_answer_1",
                "marry_anyone_question_2",
                "{=marry_anyone_answer_1c}Prosperidade e riqueza.",
                null,
                () => MarryAnyoneAnswerConsequence(false),
                100,
                null,
                null);

            // Question 2: How do you handle conflicts?
            campaignGameStarter.AddDialogLine(
                "marry_anyone_question_2",
                "marry_anyone_question_2",
                "marry_anyone_answer_2",
                "{=marry_anyone_question_2}Interessante. Outra pergunta: Como você lida com conflitos?",
                null,
                null,
                100,
                null);

            // Answer 2a: Dialogue
            campaignGameStarter.AddPlayerLine(
                "marry_anyone_answer_2a",
                "marry_anyone_answer_2",
                "marry_anyone_question_3",
                "{=marry_anyone_answer_2a}Através do diálogo e compreensão.",
                null,
                () => MarryAnyoneAnswerConsequence(true),
                100,
                null,
                null);

            // Answer 2b: Strength
            campaignGameStarter.AddPlayerLine(
                "marry_anyone_answer_2b",
                "marry_anyone_answer_2",
                "marry_anyone_question_3",
                "{=marry_anyone_answer_2b}Demonstrando força e determinação.",
                null,
                () => MarryAnyoneAnswerConsequence(false),
                100,
                null,
                null);

            // Answer 2c: Compromise
            campaignGameStarter.AddPlayerLine(
                "marry_anyone_answer_2c",
                "marry_anyone_answer_2",
                "marry_anyone_question_3",
                "{=marry_anyone_answer_2c}Buscando compromissos justos.",
                null,
                () => MarryAnyoneAnswerConsequence(true),
                100,
                null,
                null);

            // Question 3: What are your life goals?
            campaignGameStarter.AddDialogLine(
                "marry_anyone_question_3",
                "marry_anyone_question_3",
                "marry_anyone_answer_3",
                "{=marry_anyone_question_3}E por último: Quais são seus objetivos na vida?",
                null,
                null,
                100,
                null);

            // Answer 3a: Family
            campaignGameStarter.AddPlayerLine(
                "marry_anyone_answer_3a",
                "marry_anyone_answer_3",
                "marry_anyone_courtship_complete",
                "{=marry_anyone_answer_3a}Construir uma família forte e unida.",
                null,
                () => MarryAnyoneAnswerConsequence(true),
                100,
                null,
                null);

            // Answer 3b: Power
            campaignGameStarter.AddPlayerLine(
                "marry_anyone_answer_3b",
                "marry_anyone_answer_3",
                "marry_anyone_courtship_complete",
                "{=marry_anyone_answer_3b}Conquistar poder e domínio.",
                null,
                () => MarryAnyoneAnswerConsequence(false),
                100,
                null,
                null);

            // Answer 3c: Honor
            campaignGameStarter.AddPlayerLine(
                "marry_anyone_answer_3c",
                "marry_anyone_answer_3",
                "marry_anyone_courtship_complete",
                "{=marry_anyone_answer_3c}Viver com honra e deixar um legado.",
                null,
                () => MarryAnyoneAnswerConsequence(true),
                100,
                null,
                null);

            // Courtship completion - positive outcome
            campaignGameStarter.AddDialogLine(
                "marry_anyone_courtship_complete_positive",
                "marry_anyone_courtship_complete",
                "hero_main_options",
                "{=marry_anyone_courtship_complete_positive}Suas respostas me agradaram. Acho que somos compatíveis. Talvez possamos falar mais sobre o futuro...",
                () => _correctAnswers >= 2,
                MarryAnyoneCourtshipCompleteConsequence,
                100,
                null);

            // Courtship completion - negative outcome
            campaignGameStarter.AddDialogLine(
                "marry_anyone_courtship_complete_negative",
                "marry_anyone_courtship_complete",
                "hero_main_options",
                "{=marry_anyone_courtship_complete_negative}Hmm... parece que temos visões diferentes. Talvez devêssemos nos conhecer melhor com o tempo.",
                () => _correctAnswers < 2,
                null,
                99,
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
        /// Consequence for starting flirtation - increases romance level.
        /// </summary>
        private void MarryAnyoneFlirtConsequence()
        {
            Hero conversationHero = Hero.OneToOneConversationHero;
            if (conversationHero != null)
            {
                MarryAnyoneRomanceHelper.IncreaseRomanceLevel(conversationHero);
            }
        }

        /// <summary>
        /// Condition for courtship questions - only if not yet completed.
        /// </summary>
        private bool MarryAnyoneCourtshipQuestionsCondition()
        {
            Hero conversationHero = Hero.OneToOneConversationHero;
            if (conversationHero == null)
            {
                return false;
            }

            // Can only do courtship questions if not already completed
            return !MarryAnyoneRomanceHelper.HasCompletedCourtshipQuestions(conversationHero);
        }

        /// <summary>
        /// Consequence for starting courtship questions - resets question state.
        /// </summary>
        private void MarryAnyoneStartCourtshipConsequence()
        {
            _correctAnswers = 0;
        }

        /// <summary>
        /// Consequence for answering a courtship question.
        /// </summary>
        private void MarryAnyoneAnswerConsequence(bool isCorrect)
        {
            if (isCorrect)
            {
                _correctAnswers++;
            }
        }

        /// <summary>
        /// Consequence for completing courtship questions successfully.
        /// </summary>
        private void MarryAnyoneCourtshipCompleteConsequence()
        {
            Hero conversationHero = Hero.OneToOneConversationHero;
            if (conversationHero != null)
            {
                MarryAnyoneRomanceHelper.CompleteCourtshipQuestions(conversationHero);
                MarryAnyoneRomanceHelper.IncreaseRomanceLevel(conversationHero);
                
                // Increase relationship as well
                int relationBonus = _correctAnswers * RELATION_BONUS_PER_CORRECT_ANSWER;
                CharacterRelationManager.SetHeroRelation(Hero.MainHero, conversationHero, 
                    CharacterRelationManager.GetHeroRelation(Hero.MainHero, conversationHero) + relationBonus);

                InformationManager.DisplayMessage(
                    new InformationMessage(
                        $"Você se conectou com {conversationHero.Name}! (+{relationBonus} de relacionamento)",
                        Colors.Green
                    )
                );
            }
        }

        /// <summary>
        /// Condition for the marriage proposal dialogue option.
        /// Requires courtship questions to be completed.
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
            int acceptanceChance = Math.Min(100, 50 + relationshipLevel);
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
