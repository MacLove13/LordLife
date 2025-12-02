using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ScreenSystem;

namespace Bannerlord.LordLife.DebugTools
{
    /// <summary>
    /// Debug behavior that provides development shortcuts
    /// - Pressing K when inventory is open: adds 100,000 gold to the player
    /// - Pressing M during battle: kills all enemy troops
    /// </summary>
    public class DebugBehavior : CampaignBehaviorBase
    {
        private const int GOLD_AMOUNT_INVENTORY = 100000;

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
            CampaignEvents.MissionTickEvent.AddNonSerializedListener(this, OnTick);
        }

        private void OnTick(float dt)
        {
            // Only check for key presses when debug is enabled
            if (!DebugSettings.IsDebugEnabled)
                return;

            // Check if K key is released (to prevent multiple rapid triggers)
            if (Input.IsKeyReleased(InputKey.O))
            {
                HandleKKeyPress();
            }

            // Check if M key is released (to prevent multiple rapid triggers)
            if (Input.IsKeyDown(InputKey.M))
            {
                HandleMKeyPress();
            }
        }

        private void HandleKKeyPress()
        {
            AddGoldToPlayer();
        }

        private void HandleMKeyPress()
        {
            // Check if we're in a mission (battle)
            if (Mission.Current != null && Mission.Current.Mode != MissionMode.Conversation)
            {
                KillAllEnemyTroops();
            }
        }

        private bool IsInventoryScreenOpen()
        {
            // Check if the top screen is an inventory screen
            // In Bannerlord, the inventory screen class name contains "Inventory"
            var topScreen = ScreenManager.TopScreen;
            if (topScreen != null)
            {
                string screenName = topScreen.GetType().Name;
                return screenName.Contains("Inventory") || screenName.Contains("inventory");
            }
            return false;
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
            Hero.MainHero.ChangeHeroGold(GOLD_AMOUNT_INVENTORY);

            // Display message to the player
            InformationManager.DisplayMessage(
                new InformationMessage(
                    $"[Debug] Adicionado {GOLD_AMOUNT_INVENTORY} denários ao jogador!",
                    Colors.Yellow
                )
            );

            TaleWorlds.Library.Debug.Print($"[LordLife:Debug] Adicionado {GOLD_AMOUNT_INVENTORY} denários ao {Hero.MainHero.Name}");
        }

        private void KillAllEnemyTroops()
        {
            // Check if we're in a valid mission
            if (Mission.Current == null)
            {
                TaleWorlds.Library.Debug.Print("[LordLife:Debug] Não está em uma missão.");
                return;
            }

            // Get the player's agent to determine enemy side
            Agent playerAgent = Mission.Current.MainAgent;
            if (playerAgent == null)
            {
                TaleWorlds.Library.Debug.Print("[LordLife:Debug] Agente do jogador não encontrado.");
                return;
            }

            int enemiesKilled = 0;

            // Iterate through all active agents in the mission
            foreach (Agent agent in Mission.Current.Agents.ToList())
            {
                // Skip if agent is null, already dead, or is the player
                if (agent == null || !agent.IsActive() || agent == playerAgent)
                    continue;

                // Check if the agent is an enemy (different team from player)
                if (agent.Team != null && playerAgent.Team != null && agent.Team.IsEnemyOf(playerAgent.Team))
                {
                    // Kill the enemy agent by dealing massive damage
                    Blow blow = new Blow(playerAgent.Index);
                    blow.DamageType = DamageTypes.Blunt;
                    blow.BoneIndex = agent.Monster.ThoraxLookDirectionBoneIndex;
                    blow.BaseMagnitude = 10000f;
                    blow.InflictedDamage = 10000;
                    blow.SwingDirection = agent.LookDirection;
                    blow.Direction = agent.LookDirection;
                    blow.DamageCalculated = true;
                    
                    agent.Die(blow);
                    enemiesKilled++;
                }
            }

            // Display message to the player
            InformationManager.DisplayMessage(
                new InformationMessage(
                    $"[Debug] {enemiesKilled} tropas inimigas eliminadas!",
                    Colors.Red
                )
            );

            TaleWorlds.Library.Debug.Print($"[LordLife:Debug] {enemiesKilled} tropas inimigas eliminadas.");
        }
    }
}
