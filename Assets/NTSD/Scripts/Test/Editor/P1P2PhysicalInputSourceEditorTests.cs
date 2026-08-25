#if UNITY_EDITOR
using System;

using NUnit.Framework;
using NTSD.Game;
using NTSD.Simulation;
using UnityEngine.InputSystem;

namespace NTSD.Test.Editor
{
    public sealed class P1P2PhysicalInputSourceEditorTests
    {
        [Test]
        public void GeneratedWrapper_ContainsExactP1AndP2PhysicalBindings()
        {
            var input = new NTSDInputConfig();
            try
            {
                InputActionMap player1 = input.asset.FindActionMap("Player_1", true);
                InputActionMap player2 = input.asset.FindActionMap("Player_2", true);

                AssertExactBinding(player1.FindAction("Move", true), "<Keyboard>/w");
                AssertExactBinding(player1.FindAction("Move", true), "<Keyboard>/s");
                AssertExactBinding(player1.FindAction("Move", true), "<Keyboard>/a");
                AssertExactBinding(player1.FindAction("Move", true), "<Keyboard>/d");
                AssertExactBinding(player1.FindAction("Attack", true), "<Keyboard>/j");
                AssertExactBinding(player1.FindAction("Jump", true), "<Keyboard>/k");
                AssertExactBinding(player1.FindAction("Defend", true), "<Keyboard>/l");

                AssertExactBinding(player2.FindAction("Move", true), "<Keyboard>/upArrow");
                AssertExactBinding(player2.FindAction("Move", true), "<Keyboard>/downArrow");
                AssertExactBinding(player2.FindAction("Move", true), "<Keyboard>/leftArrow");
                AssertExactBinding(player2.FindAction("Move", true), "<Keyboard>/rightArrow");
                AssertExactBinding(player2.FindAction("Attack", true), "<Keyboard>/numpad1");
                AssertExactBinding(player2.FindAction("Jump", true), "<Keyboard>/numpad2");
                AssertExactBinding(player2.FindAction("Defend", true), "<Keyboard>/numpad3");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(input.asset);
            }
        }

        [Test]
        public void GeneratedWrapper_PreservesCrossedCanonicalActionAdapter()
        {
            var input = new CharacterInputModule();

            input.SetAttackActionPressed(true);
            Assert.That(
                ((ILocalFrameInputSource)input).CaptureHeldSimulationButtons(),
                Is.EqualTo(SimulationInputButtons.Jump));
            input.SetAttackActionPressed(false);

            input.SetJumpActionPressed(true);
            Assert.That(
                ((ILocalFrameInputSource)input).CaptureHeldSimulationButtons(),
                Is.EqualTo(SimulationInputButtons.Defend));
            input.SetJumpActionPressed(false);

            input.SetDefendActionPressed(true);
            Assert.That(
                ((ILocalFrameInputSource)input).CaptureHeldSimulationButtons(),
                Is.EqualTo(SimulationInputButtons.Attack));
            input.SetDefendActionPressed(false);
        }

        private static void AssertExactBinding(InputAction action, string path)
        {
            Assert.That(action, Is.Not.Null);
            bool found = false;
            for (int index = 0; index < action.bindings.Count; index++)
            {
                if (string.Equals(action.bindings[index].path, path, StringComparison.Ordinal))
                {
                    found = true;
                    break;
                }
            }

            Assert.That(found, Is.True, $"Missing binding {path} on {action.actionMap?.name}/{action.name}.");
        }

    }
}
#endif
