#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Reflection;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    [Category("FormalKernelStageContainerSeam")]
    public sealed class FormalKernelStageContainerSeamEditorTests
    {
        [Test]
        public void ValuesDefensivelyCopyEveryContainerLayer()
        {
            var spawnSource = new[]
            {
                new BattleStageSpawnValue(20, 2, 300, 5, -10, -20, 1.5, 8),
            };
            var phase = new BattleStagePhaseValue(900, spawnSource);
            spawnSource[0] = new BattleStageSpawnValue(99, 0, 0, 1, 0, 0, 0.0, 0);

            var phaseSource = new[] { phase };
            var campaign = new BattleStageCampaignValue(4, "first", phaseSource);
            phaseSource[0] = new BattleStagePhaseValue(1, Array.Empty<BattleStageSpawnValue>());

            var campaignSource = new[] { campaign };
            var set = new BattleStageCampaignSet(campaignSource);
            campaignSource[0] = new BattleStageCampaignValue(
                9,
                "changed",
                Array.Empty<BattleStagePhaseValue>());

            Assert.That(set.Count, Is.EqualTo(1));
            Assert.That(set[0].Id, Is.EqualTo(4));
            Assert.That(set[0].Comment, Is.EqualTo("first"));
            Assert.That(set[0].Phases[0].Bound, Is.EqualTo(900));
            Assert.That(set[0].Phases[0].Spawns[0].Id, Is.EqualTo(20));
            Assert.That(set[0].Phases[0].Spawns[0].Times, Is.EqualTo(5));
        }

        [Test]
        public void DtoProjectionPreservesOrderDuplicatesCommentAndCallerIsolation()
        {
            List<BattleStageCampaignData> source = BuildDuplicateSource();

            Assert.That(
                BattleStageCampaignValueAdapter.TryProject(source, out BattleStageCampaignSet set),
                Is.True);
            source[0].Id = 99;
            source[0].Comment = "mutated";
            source[0].Phases[0].Bound = 1;
            source[0].Phases[0].Spawns[0].Times = 99;
            source.Clear();

            Assert.That(set.Count, Is.EqualTo(2));
            Assert.That(set[0].Id, Is.EqualTo(4));
            Assert.That(set[0].Comment, Is.EqualTo("first"));
            Assert.That(set[0].Phases[0].Bound, Is.EqualTo(900));
            Assert.That(set[0].Phases[0].Spawns[0].Times, Is.EqualTo(5));
            Assert.That(set[1].Id, Is.EqualTo(4));
            Assert.That(set.FindFirstById(4), Is.SameAs(set[0]));
        }

        [Test]
        public void NullOrNonFiniteProjectionFailsWithoutPartialResult()
        {
            Assert.That(
                BattleStageCampaignValueAdapter.TryProject(null, out BattleStageCampaignSet nullSet),
                Is.False);
            Assert.That(nullSet, Is.Null);

            var nullPhase = new List<BattleStageCampaignData>
            {
                new BattleStageCampaignData
                {
                    Id = 1,
                    Phases = new List<BattleStagePhaseData> { null },
                },
            };
            Assert.That(
                BattleStageCampaignValueAdapter.TryProject(nullPhase, out BattleStageCampaignSet phaseSet),
                Is.False);
            Assert.That(phaseSet, Is.Null);

            var nonFinite = BuildDuplicateSource();
            nonFinite[0].Phases[0].Spawns[0].Ratio = double.NaN;
            Assert.That(
                BattleStageCampaignValueAdapter.TryProject(nonFinite, out BattleStageCampaignSet ratioSet),
                Is.False);
            Assert.That(ratioSet, Is.Null);
        }

        [Test]
        public void WorldConfigurationFailureLeavesContentAndRuntimeUntouched()
        {
            var world = new SimulationWorld();
            Assert.That(world.ConfigureStageCampaigns(BuildDuplicateSource(), 4, -1), Is.True);
            BattleStageCampaignSet beforeContent = world.StageCampaigns;
            int beforeSeries = world.StageProgression.StageSeriesIdx;
            int beforeWave = world.StageProgression.WaveIdx;
            bool beforeValid = world.StageProgressionValid;
            world.Runtime.StageSpawnRuntimeTargetTotal.Add(31);

            var invalid = BuildDuplicateSource();
            invalid[0].Phases[0].Spawns[0].Ratio = double.PositiveInfinity;
            Assert.That(world.ConfigureStageCampaigns(invalid, 8, 3), Is.False);

            Assert.That(world.StageCampaigns, Is.SameAs(beforeContent));
            Assert.That(world.StageProgression.StageSeriesIdx, Is.EqualTo(beforeSeries));
            Assert.That(world.StageProgression.WaveIdx, Is.EqualTo(beforeWave));
            Assert.That(world.StageProgressionValid, Is.EqualTo(beforeValid));
            Assert.That(world.Runtime.StageSpawnRuntimeTargetTotal, Is.EqualTo(new[] { 31 }));
        }

        [Test]
        public void RuntimeContentOwnerNoLongerUsesMutableDtoList()
        {
            FieldInfo field = typeof(BattleRuntimeState).GetField(
                "StageCampaigns",
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(field, Is.Not.Null);
            Assert.That(field.FieldType, Is.EqualTo(typeof(BattleStageCampaignSet)));
            Assert.That(
                typeof(BattleStageCampaignSet).Assembly.GetName().Name,
                Is.EqualTo("NTSD.Battle.Kernel"));
        }

        private static List<BattleStageCampaignData> BuildDuplicateSource()
        {
            return new List<BattleStageCampaignData>
            {
                new BattleStageCampaignData
                {
                    Id = 4,
                    Comment = "first",
                    Phases = new List<BattleStagePhaseData>
                    {
                        new BattleStagePhaseData
                        {
                            Bound = 900,
                            Spawns = new List<BattleStageSpawnData>
                            {
                                new BattleStageSpawnData
                                {
                                    Id = 20,
                                    Act = 2,
                                    Hp = 300,
                                    Times = 5,
                                    X = -10,
                                    Y = -20,
                                    Ratio = 1.5,
                                    Join = 8,
                                },
                            },
                        },
                    },
                },
                new BattleStageCampaignData
                {
                    Id = 4,
                    Comment = "second",
                },
            };
        }
    }
}
#endif
