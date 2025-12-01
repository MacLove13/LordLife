using Bannerlord.LordLife.Dialogues;
using Bannerlord.LordLife.MarryAnyone;
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
                // Add MarryAnyone campaign behavior
                campaignGameStarter.AddBehavior(new MarryAnyoneCampaignBehavior());
                Debug.Print("[LordLife:MarryAnyone] CampaignBehavior adicionado.");

                // Add Dialogues campaign behavior
                campaignGameStarter.AddBehavior(new DialogueCampaignBehavior());
                Debug.Print("[LordLife:Dialogues] CampaignBehavior adicionado.");
            }

            if (gameStarterObject is CampaignGameStarter campaignStarter)
            {
                campaignStarter.AddBehavior(new IgrejaBehavior());
                Debug.Print("[LordLife] IgrejaBehavior registrado.");
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