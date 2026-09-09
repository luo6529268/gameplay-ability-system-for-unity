#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    [Category("NTSD28")]
    [Category("NTSD28_B5")]
    public sealed class NTSD28B5RecoveryStatusConsumerEditorTests
    {
        private static IEnumerable<TestCaseData> RecoveryCases()
        {
            object[][] scenarios =
            {
                new object[] { "baseline_phase12", 12, 0, 0, 0, 100, 200, 300, 50, -1, 0, 101, 54 },
                new object[] { "baseline_phase3_pp150", 3, 0, 0, 0, 400, 400, 500, 150, 0, 0, 400, 152 },
                new object[] { "weak_below_base", 12, 1, 0, 0, 100, 200, 300, 299, -1, 0, 100, 300 },
                new object[] { "weak_at_base", 12, 1, 0, 0, 100, 200, 300, 300, -1, 0, 100, 300 },
                new object[] { "weak_above_base", 12, 1, 0, 0, 100, 200, 300, 301, -1, 0, 100, 301 },
                new object[] { "weak_phase3", 3, 1, 0, 0, 100, 200, 300, 50, -1, 0, 100, 50 },
                new object[] { "weak_full_hp", 12, 1, 0, 0, 200, 200, 300, 50, -1, 0, 200, 50 },
                new object[] { "weak_dead_hp", 12, 1, 0, 0, 0, 200, 300, 50, -1, 0, 0, 50 },
                new object[] { "weak_overrides_both", 12, 1, 1, 1, 100, 200, 300, 50, -1, 0, 100, 51 },
                new object[] { "hp_double_last_tick", 12, 0, 1, 0, 100, 200, 300, 50, -1, 0, 102, 54 },
                new object[] { "hp_double_positive", 12, 0, 2, 0, 100, 200, 300, 50, -1, 0, 102, 54 },
                new object[] { "hp_double_reaches_bound", 12, 0, 1, 0, 198, 200, 300, 50, -1, 0, 200, 54 },
                new object[] { "hp_double_off_phase", 3, 0, 1, 0, 100, 200, 300, 50, -1, 0, 100, 55 },
                new object[] { "mp_bonus_last_tick_pp150", 3, 0, 0, 1, 400, 400, 500, 150, 0, 0, 400, 153 },
                new object[] { "mp_bonus_positive", 3, 0, 0, 2, 100, 200, 300, 50, -1, 0, 100, 56 },
                new object[] { "mp_bonus_after_hp", 12, 0, 0, 1, 100, 200, 300, 50, -1, 0, 101, 55 },
                new object[] { "both_bonuses", 12, 0, 1, 1, 100, 200, 300, 50, -1, 0, 102, 55 },
                new object[] { "off_phase_timers_expire", 1, 1, 1, 1, 100, 200, 300, 50, -1, 0, 100, 50 },
                new object[] { "negative_timers_preserved", 12, -1, -1, -1, 100, 200, 300, 50, -1, 0, 101, 54 },
                new object[] { "render_negative_blocks_helper", 3, 0, 0, 1, 400, 400, 500, 150, 0, -1, 400, 150 },
                new object[] { "weak_hp_branch_ignores_render", 12, 1, 0, 1, 100, 200, 300, 299, -1, -1, 100, 300 },
                new object[] { "mp_bonus_keeps_pp151_gate", 3, 0, 0, 1, 400, 400, 500, 151, 0, 0, 400, 151 },
                new object[] { "mp_bonus_keeps_cap", 3, 0, 0, 1, 400, 400, 500, 500, -1, 0, 400, 500 },
            };
            foreach (string path in new[] { "Legacy", "Ecs", "Derived" })
            {
                foreach (object[] scenario in scenarios)
                {
                    var arguments = new object[scenario.Length + 1];
                    arguments[0] = path;
                    Array.Copy(scenario, 0, arguments, 1, scenario.Length);
                    yield return new TestCaseData(arguments)
                        .SetName($"RecoveryStatus_{path}_{scenario[0]}");
                }
            }
        }

        [TestCaseSource(nameof(RecoveryCases))]
        public void RecoveryStatus_ConsumesBeforeTimerTail(
            string path,
            string scenario,
            int tick,
            int weak,
            int hpDouble,
            int mpBonus,
            int hp,
            int hpBound,
            int baseHp,
            int pp,
            int gate,
            int renderPhase,
            int expectedHp,
            int expectedPp)
        {
            var world = new SimulationWorld();
            world.ConfigureBattleEcsCharacterRecoveryPassForDiagnostics(
                path == "Legacy"
                    ? BattleEcsCharacterRecoveryPassMode.Legacy
                    : BattleEcsCharacterRecoveryPassMode.DataOriented);
            LF2Character entity = path == "Derived"
                ? new DerivedRecoveryCharacter()
                : new LF2Character();
            InitializeCharacter(entity);
            world.Register(entity);
            try
            {
                entity.Health.HP = hp;
                entity.Health.HPBound = hpBound;
                entity.Health.HP3 = baseHp;
                entity.Health.PP = pp;
                entity.HitStun = renderPhase;
                entity.Runtime.OrdinaryCreditGate2F4 = gate;
                entity.Runtime.WeakTimer12C = weak;
                entity.Runtime.HpRegenDouble1AC = hpDouble;
                entity.Runtime.MpRegenBonusTimer1A4 = mpBonus;

                world.LateEntityUpdateAll(tick);

                BattleEcsCharacterRecoveryPassDiagnostics diagnostics =
                    world.BattleEcsCharacterRecoveryPassDiagnosticsForDiagnostics;
                Assert.That(diagnostics.ExactCharacterCount,
                    Is.EqualTo(path == "Ecs" ? 1 : 0), "exact route");
                Assert.That(diagnostics.CompatibilityFallbackCount,
                    Is.EqualTo(path == "Ecs" ? 0 : 1), "compatibility route");
                Assert.That(entity.Health.HP, Is.EqualTo(expectedHp),
                    $"{path}/{scenario}: HP must consume pre-tail status");
                Assert.That(entity.Health.PP, Is.EqualTo(expectedPp),
                    $"{path}/{scenario}: PP must consume pre-tail status");
                Assert.That(entity.Health.HPBound, Is.EqualTo(hpBound));
                Assert.That(entity.Health.HP3, Is.EqualTo(baseHp));
                Assert.That(entity.Runtime.OrdinaryCreditGate2F4, Is.EqualTo(gate));
                Assert.That(entity.Runtime.WeakTimer12C,
                    Is.EqualTo(hp > 0 && weak > 0 ? weak - 1 : weak),
                    "weak decrements after recovery only while alive");
                Assert.That(entity.Runtime.HpRegenDouble1AC,
                    Is.EqualTo(hpDouble > 0 ? hpDouble - 1 : hpDouble),
                    "HP double decrements after its final effective tick");
                Assert.That(entity.Runtime.MpRegenBonusTimer1A4,
                    Is.EqualTo(mpBonus > 0 ? mpBonus - 1 : mpBonus),
                    "MP bonus decrements after its final effective tick");
            }
            finally
            {
                world.Unregister(entity);
            }
        }

        private static void InitializeCharacter(LF2Character entity)
        {
            var data = new LF2CharacterData
            {
                name = "RecoveryStatusConsumer",
                type_sub = 0,
                frames = new List<LF2FrameData>
                {
                    new LF2FrameData
                    {
                        frameId = 0,
                        state = LF2States.Standing,
                        wait = 100,
                        next = 0,
                        pic = 999,
                    },
                },
            };
            entity.Name = data.name;
            entity.ObjectId = 9850;
            entity.ModuleInitialize();
            entity.SetRequiredRuntimeSlot(0);
            entity.FrameCache.Load(new LF2CharacterDataWrapper(9850, data));
            entity.ImmediateFrame(0);
            entity.Initialize(500, 500);
            entity.Runtime.SetPosition(0, 0, 0);
            entity.Runtime.SyncIntegerPosition();
        }

        private sealed class DerivedRecoveryCharacter : LF2Character
        {
            public override int GetCurrentDataObjectTypeForSimulation() =>
                (int)LF2ObjectType.Character;
        }
    }
}
#endif

