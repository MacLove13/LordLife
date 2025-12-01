using Bannerlord.LordLife.DebugTools;
using Bannerlord.LordLife.Dialogues;
using Bannerlord.LordLife.MarryAnyone;
using Bannerlord.LordLife.Workshop;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.LordLife
{
    public class SubModule : MBSubModuleBase
    {
        private Harmony? _harmony;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            // Initialize Harmony for MarryAnyone patches
            _harmony = new Harmony("Bannerlord.LordLife");
            _harmony.PatchAll(typeof(MarryAnyonePatches).Assembly);

            // Apply workshop limit patches
            WorkshopLimitPatches.PatchWorkshopLimit(_harmony);

            Debug.Print("[LordLife] SubModule carregado.");
            Debug.Print("[LordLife:MarryAnyone] Patches Harmony aplicados.");
        }

        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();

            // Unpatch Harmony
            _harmony?.UnpatchAll("Bannerlord.LordLife");

            Debug.Print("[LordLife] SubModule descarregado.");
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);

            if (gameStarterObject is CampaignGameStarter campaignGameStarter)
            {
                // Add Debug behavior (only active when DebugSettings.IsDebugEnabled is true)
                campaignGameStarter.AddBehavior(new DebugBehavior());
                Debug.Print("[LordLife:Debug] DebugBehavior registrado.");

                // Add MarryAnyone campaign behavior
                campaignGameStarter.AddBehavior(new MarryAnyoneCampaignBehavior());
                Debug.Print("[LordLife:MarryAnyone] CampaignBehavior adicionado.");

                // Add Dialogues campaign behavior
                campaignGameStarter.AddBehavior(new DialogueCampaignBehavior());
                Debug.Print("[LordLife:Dialogues] CampaignBehavior adicionado.");

                // Add Igreja campaign behavior
                campaignGameStarter.AddBehavior(new IgrejaBehavior());
                Debug.Print("[LordLife] IgrejaBehavior registrado.");

                // Add Low Loyalty Voting behavior
                campaignGameStarter.AddBehavior(new LowLoyaltyVotingBehavior());
                Debug.Print("[LordLife:LowLoyaltyVoting] CampaignBehavior adicionado.");

                // Add Workshop License behavior
                campaignGameStarter.AddBehavior(new WorkshopLicenseBehavior());
                campaignGameStarter.AddModel(new WorkshopLicenseBehavior.CustomWorkshopModel());
                Debug.Print("[LordLife:Workshop] WorkshopLicenseBehavior registrado.");
            }
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();

            InformationManager.DisplayMessage(
                new InformationMessage(
                    "LordLife carregado com sucesso!",
                    Colors.Green
                )
            );
        }
    }
}