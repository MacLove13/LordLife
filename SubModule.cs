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
    }
}