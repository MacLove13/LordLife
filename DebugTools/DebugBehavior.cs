using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;

namespace Bannerlord.LordLife.DebugTools
{
    /// <summary>
    /// Debug behavior that provides development shortcuts
    /// Currently supports pressing K to add 10000 gold to the player
    /// </summary>
    public class DebugBehavior : CampaignBehaviorBase
    {
        private const int GOLD_AMOUNT = 10000;

        public override void RegisterEvents()
        {
            // Register to tick event to check for key presses
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        public override void SyncData(IDataStore dataStore)
        {
            // No data to sync for debug behavior
        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            // Register tick event to check for key presses during gameplay
            CampaignEvents.TickEvent.AddNonSerializedListener(this, OnTick);
        }

        private void OnTick(float dt)
        {
            // Only check for key presses when debug is enabled
            if (!DebugSettings.IsDebugEnabled)
                return;

            // Check if K key is released (to prevent multiple rapid triggers)
            if (Input.IsKeyReleased(InputKey.K))
            {
                AddGoldToPlayer();
            }
        }

        private void AddGoldToPlayer()
        {
            // Check if we have a valid main hero
            if (Hero.MainHero == null)
            {
                TaleWorlds.Library.Debug.Print("[LordLife:Debug] MainHero não encontrado.");
                return;
            }

            // Add gold to the player
            Hero.MainHero.ChangeHeroGold(GOLD_AMOUNT);

            // Display message to the player
            InformationManager.DisplayMessage(
                new InformationMessage(
                    $"[Debug] Adicionado {GOLD_AMOUNT} denários ao jogador!",
                    Colors.Yellow
                )
            );

            TaleWorlds.Library.Debug.Print($"[LordLife:Debug] Adicionado {GOLD_AMOUNT} denários ao {Hero.MainHero.Name}");
        }
    }
}
