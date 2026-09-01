#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;
using System.Reflection;
using NTSD.App;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEditor.PackageManager;
using UnityEngine;

namespace NTSD.Test
{
    [Category("BattleRosterLabelBootstrapSeam")]
    public sealed class BattleRosterLabelBootstrapSeamEditorTests
    {
        [Test]
        public void StateAndClientAdapterHaveSeparateSingleOwnerSources()
        {
            string simulationPath = Path.Combine(
                Application.dataPath,
                "NTSD",
                "Scripts",
                "Simulation");
            string oldStatePath = Path.Combine(simulationPath, "BattleRosterLabelState.cs");
            string adapterPath = Path.Combine(
                simulationPath,
                "BattleMatchConfigRuntimeAdapter.cs");
            string mixedPath = Path.Combine(simulationPath, "BattleRuntimeState.cs");

            PackageInfo packageInfo = PackageInfo.FindForAssembly(typeof(BattleRosterRuntimeState).Assembly);
            Assert.That(packageInfo, Is.Not.Null);
            Assert.That(packageInfo.name, Is.EqualTo("com.ntsd.battle-kernel"));
            Assert.That(packageInfo.version, Is.EqualTo("0.6.0"));
            string statePath = Path.Combine(packageInfo.resolvedPath, "Runtime", "Core", "BattleRosterLabelState.cs");
            Assert.That(File.Exists(oldStatePath), Is.False);
            Assert.That(File.Exists(statePath), Is.True, "The shared roster/label owner source is missing.");
            Assert.That(typeof(BattleRosterRuntimeState).Assembly.GetName().Name, Is.EqualTo("NTSD.Battle.Kernel"));
            Assert.That(File.Exists(adapterPath), Is.True,
                "The Client MatchConfig adapter source does not exist yet.");
            string state = File.ReadAllText(statePath);
            string adapter = File.ReadAllText(adapterPath);
            string mixed = File.ReadAllText(mixedPath);
            Assert.That(state, Does.Contain("public sealed class BattleRosterRuntimeState"));
            Assert.That(state, Does.Contain("public sealed class BattleSlotLabelRuntimeState"));
            Assert.That(state, Does.Not.Contain("NTSD.App"));
            Assert.That(state, Does.Not.Contain("MatchConfig"));
            Assert.That(state, Does.Not.Contain("PlayerSlotConfig"));
            Assert.That(state, Does.Not.Contain("GameConfig"));
            Assert.That(adapter, Does.Contain("public static class BattleMatchConfigRuntimeAdapter"));
            Assert.That(adapter, Does.Contain("this BattleRosterRuntimeState roster"));
            Assert.That(adapter, Does.Contain("this BattleSlotLabelRuntimeState labels"));
            Assert.That(mixed, Does.Not.Contain("public sealed class BattleRosterRuntimeState"));
            Assert.That(mixed, Does.Not.Contain("public sealed class BattleSlotLabelRuntimeState"));
            Assert.That(mixed, Does.Contain("public sealed class BattleResultsRuntimeState"));
            Assert.That(mixed, Does.Contain("public sealed class BattleRuntimeState"));
        }

        [Test]
        public void RosterDefaultsAndResetPreserveEightCanonicalSlots()
        {
            var roster = new BattleRosterRuntimeState();
            Assert.That(roster.Slots, Is.Not.Null);
            Assert.That(roster.Slots.Length, Is.EqualTo(8));
            Assert.That(roster.ActiveSlotCount, Is.Zero);
            for (int index = 0; index < roster.Slots.Length; index++)
            {
                Assert.That(roster.Slots[index], Is.Not.Null);
                roster.Slots[index].Active = true;
                roster.Slots[index].CharacterId = index;
            }

            roster.ActiveSlotCount = 8;
            roster.Reset();
            Assert.That(roster.Slots.Length, Is.EqualTo(8));
            Assert.That(roster.ActiveSlotCount, Is.Zero);
            for (int index = 0; index < roster.Slots.Length; index++)
            {
                Assert.That(roster.Slots[index].Active, Is.False);
                Assert.That(roster.Slots[index].CharacterId, Is.EqualTo(-1));
            }
        }

        [Test]
        public void MatchConfigMappingPreservesNullDisabledAndEightSlotBoundary()
        {
            var config = new MatchConfig();
            config.players.Add(new PlayerSlotConfig
            {
                use = true,
                isHuman = true,
                characterId = 7,
                team = GameConfig.TeamIndependent,
                inputId = 0,
                aiId = -1,
            });
            config.players.Add(null);
            config.players.Add(new PlayerSlotConfig
            {
                use = false,
                characterId = 99,
                team = 3,
                inputId = 9,
                aiId = 8,
            });
            config.players.Add(new PlayerSlotConfig
            {
                use = true,
                isHuman = false,
                characterId = 11,
                team = 0,
                inputId = 0,
                aiId = 4,
            });
            for (int index = 4; index < 10; index++)
            {
                config.players.Add(new PlayerSlotConfig
                {
                    use = true,
                    isHuman = true,
                    characterId = 100 + index,
                    team = 2,
                    inputId = index + 1,
                    aiId = -1,
                });
            }

            var roster = new BattleRosterRuntimeState();
            roster.ApplyMatchConfig(config);
            Assert.That(roster.ActiveSlotCount, Is.EqualTo(6));
            Assert.That(roster.Slots[0].Active, Is.True);
            Assert.That(roster.Slots[0].Team, Is.EqualTo(10));
            Assert.That(roster.Slots[0].InputId, Is.EqualTo(1));
            Assert.That(roster.Slots[1].Active, Is.False);
            Assert.That(roster.Slots[2].Active, Is.False);
            Assert.That(roster.Slots[3].Active, Is.True);
            Assert.That(roster.Slots[3].Team, Is.EqualTo(4));
            Assert.That(roster.Slots[3].InputId, Is.EqualTo(4));
            Assert.That(roster.Slots[3].AiId, Is.EqualTo(4));
            Assert.That(roster.Slots[7].CharacterId, Is.EqualTo(107));
        }

        [Test]
        public void LabelBootstrapPreservesFirstFourActiveSlotFormatting()
        {
            var config = new MatchConfig();
            for (int index = 0; index < 6; index++)
            {
                config.players.Add(new PlayerSlotConfig
                {
                    use = index != 1,
                    characterId = index,
                });
            }

            var labels = new BattleSlotLabelRuntimeState();
            labels.ApplyBootstrapFromMatchConfig(config);
            Assert.That(labels.BattleSlotLabels.GetLength(0), Is.EqualTo(10));
            Assert.That(labels.BattleSlotLabels.GetLength(1), Is.EqualTo(12));
            Assert.That(labels.BattleSlotLabels[0, 0], Is.EqualTo('1'));
            Assert.That(labels.BattleSlotLabelState[0], Is.EqualTo(1));
            Assert.That(labels.BattleSlotLabels[1, 0], Is.EqualTo('\0'));
            Assert.That(labels.BattleSlotLabelState[1], Is.Zero);
            Assert.That(labels.BattleSlotLabels[2, 0], Is.EqualTo('3'));
            Assert.That(labels.BattleSlotLabelState[2], Is.EqualTo(3));
            Assert.That(labels.BattleSlotLabels[3, 0], Is.EqualTo('4'));
            Assert.That(labels.BattleSlotLabelState[3], Is.EqualTo(4));
            Assert.That(labels.BattleSlotLabels[4, 0], Is.EqualTo('\0'));
            Assert.That(labels.BattleSlotLabelState[4], Is.Zero);

            labels.Reset();
            Assert.That(labels.BattleSlotLabels[0, 0], Is.EqualTo('\0'));
            Assert.That(labels.BattleSlotLabelState[0], Is.Zero);
        }

        [Test]
        public void ResolverValuesStayFrozenAcrossOwnerSeam()
        {
            Type currentOwner = typeof(BattleMatchConfigRuntimeAdapter);
            MethodInfo team = currentOwner.GetMethod(
                "ResolveBattleTeam",
                BindingFlags.Public | BindingFlags.Static);
            MethodInfo input = currentOwner.GetMethod(
                "ResolveInputId",
                BindingFlags.Public | BindingFlags.Static);
            Assert.That(team, Is.Not.Null);
            Assert.That(input, Is.Not.Null);
            Assert.That(team.Invoke(null, new object[] { GameConfig.TeamIndependent, 5 }),
                Is.EqualTo(15));
            Assert.That(team.Invoke(null, new object[] { 0, 5 }), Is.EqualTo(6));
            Assert.That(team.Invoke(null, new object[] { 3, 5 }), Is.EqualTo(3));
            Assert.That(input.Invoke(null, new object[] { 0, 5 }), Is.EqualTo(6));
            Assert.That(input.Invoke(null, new object[] { 7, 5 }), Is.EqualTo(7));
        }
    }
}
#endif
