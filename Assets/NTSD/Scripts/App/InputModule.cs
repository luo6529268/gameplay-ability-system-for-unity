using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace NTSD.App
{
    public enum BattleInputAction
    {
        Attack,
        Jump,
        Defend,
    }

    internal interface IPlayerActionInputSink
    {
        void SetActionPressed(BattleInputAction action, bool pressed);
    }

    public class InputModule
    {
        private NTSDInputConfig _NTSDInputConfig;
        private readonly Dictionary<int, IPlayerActionInputSink> playerActionInputs =
            new Dictionary<int, IPlayerActionInputSink>();

        public InputModule()
        {
            _NTSDInputConfig ??= new NTSDInputConfig();
        }

        public InputActionMap GetActionMapByPlayerID(int playerID) 
        {
            return _NTSDInputConfig.asset.FindActionMap($"Player_{playerID}");
        }

        internal void RegisterPlayerActionInput(
            int playerId,
            IPlayerActionInputSink inputSink)
        {
            if (playerId < 1 || inputSink == null)
                return;

            playerActionInputs[playerId] = inputSink;
        }

        internal void UnregisterPlayerActionInput(
            int playerId,
            IPlayerActionInputSink inputSink)
        {
            if (playerActionInputs.TryGetValue(playerId, out IPlayerActionInputSink current) &&
                ReferenceEquals(current, inputSink))
            {
                playerActionInputs.Remove(playerId);
            }
        }

        public bool TrySetActionPressed(
            int playerId,
            InputAction action,
            bool pressed)
        {
            if (!TryResolveBattleInputAction(action, out BattleInputAction battleAction))
                return false;

            if (!playerActionInputs.TryGetValue(playerId, out IPlayerActionInputSink inputSink))
                return false;

            inputSink.SetActionPressed(battleAction, pressed);
            return true;
        }

        private static bool TryResolveBattleInputAction(
            InputAction action,
            out BattleInputAction battleAction)
        {
            switch (action?.name)
            {
                case "Attack":
                    battleAction = BattleInputAction.Attack;
                    return true;
                case "Jump":
                    battleAction = BattleInputAction.Jump;
                    return true;
                case "Defend":
                    battleAction = BattleInputAction.Defend;
                    return true;
                default:
                    battleAction = default;
                    return false;
            }
        }
    }
}
