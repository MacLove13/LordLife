using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Bannerlord.LordLife
{
    public class IgrejaBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        public override void SyncData(IDataStore dataStore)
        {
            // No data to sync for now
        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            AddIgrejaMenus(campaignGameStarter);
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
                4
            );

            // Create the Igreja menu
            campaignGameStarter.AddGameMenu(
                "town_igreja_menu",
                "{=lordlife_igreja_desc}Você está na Igreja. O ambiente é sereno e convidativo.",
                args => { },
                GameOverlays.MenuOverlayType.SettlementWithBoth
            );

            // Option 1: Falar com padre
            campaignGameStarter.AddGameMenuOption(
                "town_igreja_menu",
                "igreja_falar_padre",
                "{=lordlife_falar_padre}Falar com padre",
                args =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Conversation;
                    return true;
                },
                args =>
                {
                    Debug.Print("[LordLife] Igreja: Falar com padre selecionado.");
                    InformationManager.DisplayMessage(new InformationMessage("[LordLife] Debug: Falar com padre", Colors.Yellow));
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
                    return true;
                },
                args =>
                {
                    Debug.Print("[LordLife] Igreja: Confessar pecados selecionado.");
                    InformationManager.DisplayMessage(new InformationMessage("[LordLife] Debug: Confessar pecados", Colors.Yellow));
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
                    GameMenu.SwitchToMenu("town");
                },
                true,
                99
            );
        }

        private bool IsInTown()
        {
            Settlement currentSettlement = Settlement.CurrentSettlement;
            return currentSettlement != null && currentSettlement.IsTown;
        }
    }
}
