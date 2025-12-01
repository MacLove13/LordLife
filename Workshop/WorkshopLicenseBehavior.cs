using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Bannerlord.LordLife.Workshop
{
    /// <summary>
    /// Campaign behavior that adds the ability to purchase workshop licenses from notables
    /// in towns that allow caravan creation.
    /// </summary>
    public class WorkshopLicenseBehavior : CampaignBehaviorBase
    {
        private const int WorkshopLicenseCost = 15000;

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        public override void SyncData(IDataStore dataStore)
        {
            // Sync workshop license data directly from the manager's internal field
            // This must be a direct field reference, not a local variable, to avoid TargetInvocationException
            dataStore.SyncData("workshopExtraLicenses", ref WorkshopLicenseManager.Instance._extraLicenses);
            
            // Ensure the dictionary is initialized after loading
            WorkshopLicenseManager.Instance._extraLicenses ??= new Dictionary<string, int>();
        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            AddWorkshopLicenseDialogues(campaignGameStarter);
            Debug.Print("[LordLife:Workshop] Sistema de licença de oficina registrado.");
        }

        /// <summary>
        /// Registers workshop license purchase dialogues with notables.
        /// </summary>
        private void AddWorkshopLicenseDialogues(CampaignGameStarter campaignGameStarter)
        {
            // Player initiates purchase request
            campaignGameStarter.AddPlayerLine(
                "lordlife_workshop_license_start",
                "hero_main_options",
                "lordlife_workshop_license_response",
                "{=lordlife_workshop_buy_license}Comprar licença de Oficina",
                CanOfferWorkshopLicense,
                null,
                100,
                null,
                null);

            // Notable responds with price and confirmation request
            campaignGameStarter.AddDialogLine(
                "lordlife_workshop_license_response",
                "lordlife_workshop_license_response",
                "lordlife_workshop_license_confirm",
                "{=lordlife_workshop_license_cost}Uma nova licença de oficina custará 15000 denários. Deseja prosseguir?",
                null,
                null,
                100,
                null);

            // Player chooses to buy
            campaignGameStarter.AddPlayerLine(
                "lordlife_workshop_license_buy",
                "lordlife_workshop_license_confirm",
                "lordlife_workshop_license_purchased",
                "{=lordlife_workshop_buy}Comprar",
                CanAffordWorkshopLicense,
                OnWorkshopLicensePurchased,
                100,
                null,
                null);

            // Player doesn't have enough money
            campaignGameStarter.AddPlayerLine(
                "lordlife_workshop_license_buy_no_gold",
                "lordlife_workshop_license_confirm",
                "lordlife_workshop_license_no_gold_response",
                "{=lordlife_workshop_buy}Comprar",
                () => !CanAffordWorkshopLicense(),
                null,
                100,
                null,
                null);

            // Notable response when player has no gold
            campaignGameStarter.AddDialogLine(
                "lordlife_workshop_license_no_gold_response",
                "lordlife_workshop_license_no_gold_response",
                "hero_main_options",
                "{=lordlife_workshop_no_gold}Lamento, mas você não possui denários suficientes para comprar a licença.",
                null,
                null,
                100,
                null);

            // Player cancels purchase
            campaignGameStarter.AddPlayerLine(
                "lordlife_workshop_license_cancel",
                "lordlife_workshop_license_confirm",
                "lordlife_workshop_license_cancel_response",
                "{=lordlife_workshop_cancel}Desistir da compra",
                null,
                null,
                99,
                null,
                null);

            // Notable response when player cancels
            campaignGameStarter.AddDialogLine(
                "lordlife_workshop_license_cancel_response",
                "lordlife_workshop_license_cancel_response",
                "hero_main_options",
                "{=lordlife_workshop_cancel_response}Entendo. Se mudar de ideia, estarei aqui.",
                null,
                null,
                100,
                null);

            // Purchase completed confirmation
            campaignGameStarter.AddDialogLine(
                "lordlife_workshop_license_purchased",
                "lordlife_workshop_license_purchased",
                "hero_main_options",
                "{=lordlife_workshop_purchased}Excelente! A licença foi registrada. Seu clã agora pode possuir mais uma oficina.",
                null,
                null,
                100,
                null);
        }

        /// <summary>
        /// Checks if the current conversation hero can offer workshop licenses.
        /// Must be a notable in a town that allows caravan creation.
        /// </summary>
        private bool CanOfferWorkshopLicense()
        {
            Hero conversationHero = Hero.OneToOneConversationHero;

            if (conversationHero == null)
            {
                return false;
            }

            // Must be a notable
            if (!conversationHero.IsNotable)
            {
                return false;
            }

            // Must be in a town
            Settlement currentSettlement = Settlement.CurrentSettlement;
            if (currentSettlement == null || !currentSettlement.IsTown)
            {
                return false;
            }

            // Check if this town allows caravan creation
            // Towns that allow caravan creation have merchant notables
            if (!CanCreateCaravanInTown(currentSettlement))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if a town allows caravan creation by checking for merchant notables.
        /// </summary>
        private bool CanCreateCaravanInTown(Settlement settlement)
        {
            if (settlement == null || !settlement.IsTown)
            {
                return false;
            }

            // Check if the settlement has any merchant notables (artisan, merchant, or gangleader)
            // These are the notables that typically allow caravan creation
            var notables = settlement.Notables;
            if (notables == null)
            {
                return false;
            }

            foreach (var notable in notables)
            {
                if (notable != null && notable.IsAlive)
                {
                    // Check if notable is a merchant, artisan, or gangleader
                    if (notable.IsArtisan || notable.IsMerchant || notable.IsGangLeader)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if the player can afford the workshop license.
        /// </summary>
        private bool CanAffordWorkshopLicense()
        {
            return Hero.MainHero.Gold >= WorkshopLicenseCost;
        }

        /// <summary>
        /// Processes the workshop license purchase.
        /// Increases clan workshop limit and deducts gold.
        /// </summary>
        private void OnWorkshopLicensePurchased()
        {
            if (!CanAffordWorkshopLicense())
            {
                InformationManager.DisplayMessage(
                    new InformationMessage(
                        "Você não tem dinheiro suficiente para comprar a licença de oficina.",
                        Colors.Red));
                return;
            }

            // Deduct the cost
            GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, null, WorkshopLicenseCost, false);

            // Increase workshop limit using the Bannerlord campaign API
            if (Hero.MainHero.Clan != null)
            {
                // Use Campaign.Current to access workshop count limit
                // The default limit is based on clan tier: Clan.Tier + 1
                // We need to track additional licenses separately
                var clan = Hero.MainHero.Clan;
                
                // In Bannerlord, the CompanionLimit property exists but WorkshopLimit needs to be managed differently
                // We'll use a custom tracking system via saved data
                WorkshopLicenseManager.Instance.AddWorkshopLicense(clan.StringId);
                
                InformationManager.DisplayMessage(
                    new InformationMessage(
                        $"Licença de oficina comprada por {WorkshopLicenseCost} denários! Limite de oficinas do clã aumentado em +1.",
                        Colors.Green));

                Debug.Print($"[LordLife:Workshop] {Hero.MainHero.Name} comprou licença de oficina por {WorkshopLicenseCost}. Total de licenças extras: {WorkshopLicenseManager.Instance.GetExtraLicenses(clan.StringId)}");
            }
        }

        public class CustomWorkshopModel : DefaultWorkshopModel
        {
            public override int GetMaxWorkshopCountForClanTier(int tier)
            {
                return base.GetMaxWorkshopCountForClanTier(tier) + GetExtraLicenses();
            }

            public override int MaximumWorkshopsPlayerCanHave => base.MaximumWorkshopsPlayerCanHave + GetExtraLicenses();

            private int GetExtraLicenses()
            {
                if (Campaign.Current?.GetCampaignBehavior<WorkshopLicenseBehavior>() is { } behavior)
                {
                    var clan = Hero.MainHero.Clan;
                    return WorkshopLicenseManager.Instance.GetExtraLicenses(clan.StringId);
                    // return behavior.GetExtraLicensesFor(Hero.MainHero);
                }
                return 0;
            }
        }
    }
}
