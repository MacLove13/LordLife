using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;
namespace Bannerlord.LordLife
{
    public class IgrejaBehavior : CampaignBehaviorBase
    {
        // Store hero StringIds instead of Hero objects for proper serialization
        private Dictionary<string, string> _settlementPriestIds = new Dictionary<string, string>();

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_settlementPriestIds", ref _settlementPriestIds);
            
            // Ensure dictionary is initialized after loading
            _settlementPriestIds ??= new Dictionary<string, string>();
        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            AddIgrejaMenus(campaignGameStarter);
        }

        private Hero? GetCurrentSettlementPriest()
        {
            Settlement? settlement = Settlement.CurrentSettlement;
            if (settlement != null)
            {
                return GetOrCreatePriestForSettlement(settlement);
            }
            return null;
        }

        private Hero? GetExistingPriestForSettlement(Settlement settlement)
        {
            string settlementId = settlement.StringId;

            if (_settlementPriestIds.TryGetValue(settlementId, out string? existingPriestId))
            {
                // Try to find the hero by StringId
                Hero? existingPriest = Hero.AllAliveHeroes.FirstOrDefault(h => h.StringId == existingPriestId)
                    ?? Hero.DeadOrDisabledHeroes.FirstOrDefault(h => h.StringId == existingPriestId);
                
                if (existingPriest != null && existingPriest.IsAlive)
                {
                    return existingPriest;
                }
                _settlementPriestIds.Remove(settlementId);
            }

            return null;
        }

        private Hero? GetOrCreatePriestForSettlement(Settlement settlement)
        {
            Hero? existingPriest = GetExistingPriestForSettlement(settlement);
            if (existingPriest != null)
            {
                return existingPriest;
            }

            Hero? priest = CreatePriest(settlement);
            if (priest != null)
            {
                _settlementPriestIds[settlement.StringId] = priest.StringId;
            }
            return priest;
        }

        private void OnIgrejaMenuInit(MenuCallbackArgs args)
        {
            // Get or create the priest for this settlement
            Settlement? settlement = Settlement.CurrentSettlement;
            if (settlement == null) return;
            
            Hero? priest = GetOrCreatePriestForSettlement(settlement);
            if (priest != null)
            {
                Debug.Print($"[LordLife] Igreja: Padre presente - {priest.Name}");
                // Priest is already in the settlement as a notable, no need to add/remove
            }
        }

        private Hero? CreatePriest(Settlement settlement)
        {
            CharacterObject? notableTemplate = GetNotableTemplate(settlement.Culture);
            if (notableTemplate == null)
            {
                Debug.Print("[LordLife] Não foi possível encontrar template de notável para criar padre.");
                return null;
            }

            Hero priest = HeroCreator.CreateSpecialHero(
                notableTemplate,
                settlement,
                null,
                null,
                40
            );

            if (priest != null)
            {
                priest.SetName(
                    new TaleWorlds.Localization.TextObject("{=lordlife_padre_nome}Padre " + priest.FirstName),
                    priest.FirstName
                );

                // Keep the priest active so it can be interacted with
                priest.ChangeState(Hero.CharacterStates.Active);
                
                // Set as Artisan notable (clergy is a type of artisan/scholar)
                priest.SetNewOccupation(Occupation.Artisan);
                
                // Verify the priest is properly associated with the settlement
                if (priest.CurrentSettlement != settlement)
                {
                    Debug.Print($"[LordLife] Aviso: Padre {priest.Name} não foi automaticamente associado à settlement. CurrentSettlement: {priest.CurrentSettlement?.Name?.ToString() ?? "null"}");
                }
                else
                {
                    Debug.Print($"[LordLife] Padre criado: {priest.Name} para {settlement.Name}");
                }
            }

            return priest;
        }

        private CharacterObject? GetNotableTemplate(CultureObject culture)
        {
            // Use notable_artisan templates instead of wanderer templates
            string templateId = "notable_artisan_" + culture.StringId;
            CharacterObject? template = MBObjectManager.Instance.GetObject<CharacterObject>(templateId);

            if (template == null)
            {
                // Fallback to empire artisan if culture-specific not found
                template = MBObjectManager.Instance.GetObject<CharacterObject>("notable_artisan_empire");
            }

            return template;
        }

        private void AddIgrejaMenus(CampaignGameStarter campaignGameStarter)
        {
            // Add "Igreja" option to the town menu (similar to how port works)
            campaignGameStarter.AddGameMenuOption(
                "town",
                "town_igreja",
                "{=lordlife_igreja}Igreja",
                args =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                    return IsInTown();
                },
                args =>
                {
                    GameMenu.SwitchToMenu("town_igreja_menu");
                },
                false,
                0
            );

            // Create the Igreja menu with SettlementWithCharacters overlay to show the priest
            campaignGameStarter.AddGameMenu(
                "town_igreja_menu",
                "{=lordlife_igreja_desc}Você está na Igreja. O ambiente é sereno e convidativo.",
                OnIgrejaMenuInit,
                GameMenu.MenuOverlayType.SettlementWithCharacters
            );

            // Option 1: Falar com padre
            campaignGameStarter.AddGameMenuOption(
                "town_igreja_menu",
                "igreja_falar_padre",
                "{=lordlife_falar_padre}Falar com padre",
                args =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Conversation;
                    return GetCurrentSettlementPriest() != null;
                },
                args =>
                {
                    Hero? priest = GetCurrentSettlementPriest();
                    if (priest != null)
                    {
                        Debug.Print($"[LordLife] Igreja: Falar com padre - {priest.Name}");
                        // Start a conversation with the priest
                        CampaignMapConversation.OpenConversation(new ConversationCharacterData(CharacterObject.PlayerCharacter, null, false, false, false, false, false, false), new ConversationCharacterData(priest.CharacterObject, null, false, false, false, false, false, false));
                    }
                },
                false,
                0
            );

            // Option 2: Confessar pecados
            campaignGameStarter.AddGameMenuOption(
                "town_igreja_menu",
                "igreja_confessar_pecados",
                "{=lordlife_confessar}Confessar pecados",
                args =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Continue;
                    return GetCurrentSettlementPriest() != null;
                },
                args =>
                {
                    Hero? priest = GetCurrentSettlementPriest();
                    if (priest != null)
                    {
                        Debug.Print($"[LordLife] Igreja: Confessar pecados com {priest.Name}");
                        InformationManager.DisplayMessage(new InformationMessage($"[LordLife] Debug: Confessar pecados com {priest.Name}", Colors.Yellow));
                    }
                },
                false,
                1
            );

            // Option 3: Colaborar com a fé local
            campaignGameStarter.AddGameMenuOption(
                "town_igreja_menu",
                "igreja_colaborar_fe",
                "{=lordlife_colaborar_fe}Colaborar com a fé local",
                args =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Continue;
                    return true;
                },
                args =>
                {
                    Debug.Print("[LordLife] Igreja: Colaborar com a fé local selecionado.");
                    InformationManager.DisplayMessage(new InformationMessage("[LordLife] Debug: Colaborar com a fé local", Colors.Yellow));
                },
                false,
                2
            );

            // Option to leave the Igreja menu
            campaignGameStarter.AddGameMenuOption(
                "town_igreja_menu",
                "igreja_leave",
                "{=lordlife_sair}Sair",
                args =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                },
                args =>
                {
                    // Priest remains in settlement as a notable - no need to remove
                    GameMenu.SwitchToMenu("town");
                },
                true,
                99
            );
        }

        private bool IsInTown()
        {
            Settlement? currentSettlement = Settlement.CurrentSettlement;
            return currentSettlement != null && currentSettlement.IsTown;
        }
    }
}
