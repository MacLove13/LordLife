using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.LordLife
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            Debug.Print("[LordLife] SubModule carregado.");
        }

        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();

            Debug.Print("[LordLife] SubModule descarregado.");
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

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);

            if (game.GameType is Campaign)
            {
                CampaignGameStarter campaignStarter = (CampaignGameStarter)gameStarterObject;
                campaignStarter.AddBehavior(new IgrejaBehavior());
                Debug.Print("[LordLife] IgrejaBehavior registrado.");
            }
        }
    }
}