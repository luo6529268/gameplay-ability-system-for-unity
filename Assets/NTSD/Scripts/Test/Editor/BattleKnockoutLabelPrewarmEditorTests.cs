#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.App;
using NTSD.DatParser;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class BattleKnockoutLabelPrewarmEditorTests
    {
        private SimulationWorld world;
        private BattleKnockoutFeedRowProjection projection;
        private BattlePresentationFrame frame;
        private BattleContentSource source;
        private LoganModeKnockoutFeedInput feed;
        private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        [SetUp]
        public void SetUp()
        {
            source = BattleContentSource.ForLoganRuntime(Path.Combine(
                Directory.GetParent(Application.dataPath).FullName, "Assets/NTSD/Content/LoganRuntime"));
            feed = LoganModeKnockoutFeedInput.FromProjectSnapshot(ProjectBattleModeConfig.LoadDefault().Capture());
            world = new SimulationWorld(BattleRuntimeProfile.DesktopExtended, 1050);
            world.ResetRuntimeState();
            world.Runtime.Match.BattleGameModeId = 1;
            SetCatalog("Naruto", "佐助");
            projection = new BattleKnockoutFeedRowProjection();
            projection.SetFeed(feed, source);
            frame = new BattlePresentationFrame();
        }

        [TearDown]
        public void TearDown()
        {
            world.RuntimeCapacity.Unseal();
            world.ResetRuntimeState();
        }

        [Test]
        public void ColdPreparationExistsAndDoesNotPublish()
        {
            Prepare();
            Assert.That(CacheCount(), Is.EqualTo(44));
            Assert.That(frame.KnockoutFeedRowCount, Is.Zero);
            Assert.That(world.NativeKnockoutEvents.Count, Is.Zero);
        }

        [TestCase(0, "Remie")]
        [TestCase(1, "Kezeal")]
        [TestCase(7, "8")]
        [TestCase(8, "<No name>")]
        [TestCase(9, "")]
        [TestCase(10, "Com")]
        [TestCase(100, "Com")]
        [TestCase(998, "Com")]
        public void FirstVisibleNamePreservesNativeSlotAndUtf8Width(int slot, string prefix)
        {
            AddActors(slot, 2);
            Prepare();
            world.RuntimeCapacity.Seal();
            Project();
            BattleKnockoutFeedNameSnapshot name = frame.GetKnockoutFeedRow(0).Attacker;
            string expected = prefix + " [Naruto]";
            Assert.That(name.Text, Is.EqualTo(expected));
            Assert.That(name.Slot, Is.EqualTo(slot));
            Assert.That(name.BattleGroup, Is.EqualTo(1));
            Assert.That(name.ScreenLeft, Is.EqualTo(feed.AttackerScreenLeft -
                (feed.AttackerRightAligned ? Encoding.UTF8.GetByteCount(expected) * 9 : 0)));
            Assert.That(name.TeamColored, Is.EqualTo(feed.AttackerTeamColor));
            Assert.That(CacheCount(), Is.EqualTo(44));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void AppendFlagKeepsBareOrCharacterName(bool append)
        {
            ProjectBattleModeConfig fixture = UnityEngine.Object.Instantiate(ProjectBattleModeConfig.LoadDefault());
            try
            {
                fixture.Knockout.attackerAppendCharacterName = append;
                projection.SetFeed(LoganModeKnockoutFeedInput.FromProjectSnapshot(fixture.Capture()), source);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(fixture);
            }
            AddActors(12, 2);
            Prepare();
            Project();
            Assert.That(frame.GetKnockoutFeedRow(0).Attacker.Text,
                Is.EqualTo(append ? "Com [Naruto]" : "Com"));
        }

        [TestCase(5)]
        [TestCase(99999)]
        public void EmptyOrMissingCatalogNameUsesPreparedNone(int objectId)
        {
            AddActors(12, objectId);
            Prepare();
            world.RuntimeCapacity.Seal();
            Project();
            Assert.That(frame.GetKnockoutFeedRow(0).Attacker.Text, Is.EqualTo("Com [none]"));
            Assert.That(CacheCount(), Is.EqualTo(44));
        }

        [Test]
        public void CustomNamesBracketUtf8AndFrozenCopyRemainExact()
        {
            projection.SetNames(new MatchConfig
            {
                nativeBattlePlayerNames = new List<string>
                {
                    "鸣人", "B", "C", "D", "E", "F", "G", "H", "I", "",
                },
                nativeBracketPlayerNames = new List<bool> { true },
            });
            projection.SetFeed(feed, source);
            AddActors(0, 2);
            Prepare();
            Project();
            BattleKnockoutFeedNameSnapshot frozen = frame.GetKnockoutFeedRow(0).Attacker;
            Assert.That(frozen.Text, Is.EqualTo("[鸣人] [Naruto]"));
            projection.SetNames(null);
            projection.SetFeed(feed, source);
            Prepare();
            Project();
            Assert.That(frame.GetKnockoutFeedRow(0).Attacker.Text, Is.EqualTo("Remie [Naruto]"));
            Assert.That(frozen.Text, Is.EqualTo("[鸣人] [Naruto]"));
        }

        [Test]
        public void SameTickOidChangeUsesAlreadyPreparedName()
        {
            LF2Character actor = AddActors(12, 2);
            Prepare();
            world.RuntimeCapacity.Seal();
            Project();
            actor.ObjectId = 4;
            Project();
            Assert.That(frame.GetKnockoutFeedRow(0).Attacker.Text, Is.EqualTo("Com [佐助]"));
            Assert.That(CacheCount(), Is.EqualTo(44));
        }

        [Test]
        public void UnsealedGenerationChangeRebuildsColdTexts()
        {
            AddActors(12, 2);
            Prepare();
            Project();
            SetCatalog("Changed", "Other");
            Project();
            Assert.That(frame.GetKnockoutFeedRow(0).Attacker.Text, Is.EqualTo("Com [Changed]"));
        }

        [Test]
        public void SealedGenerationChangeRejectsInsteadOfGrowing()
        {
            AddActors(12, 2);
            Prepare();
            world.RuntimeCapacity.Seal();
            SetCatalog("Changed", "Other");
            Assert.Throws<InvalidOperationException>(Project);
            Assert.That(CacheCount(), Is.EqualTo(44));
            Assert.That(frame.KnockoutFeedRowCount, Is.Zero);
        }

        [Test]
        public void SealedUnpreparedInputRejectsWithoutPartialRows()
        {
            AddActors(12, 2);
            world.RuntimeCapacity.Seal();
            Assert.Throws<InvalidOperationException>(Project);
            Assert.That(frame.KnockoutFeedRowCount, Is.Zero);
            Assert.That(CacheCount(), Is.Zero);
        }

        [Test]
        public void FeedRebindInvalidatesPreparation()
        {
            AddActors(12, 2);
            Prepare();
            projection.SetFeed(feed, source);
            world.RuntimeCapacity.Seal();
            Assert.Throws<InvalidOperationException>(Project);
        }

        [Test]
        public void ManagedColdInputCanPrepareWithoutUnityManager()
        {
            AddActors(12, 2);
            Project();
            Assert.That(frame.GetKnockoutFeedRow(0).Attacker.Text, Is.EqualTo("Com [Naruto]"));
            Assert.That(CacheCount(), Is.EqualTo(44));
        }

        [Test]
        public void ComputerTextClassDoesNotShareSnapshotSlotOrTeam()
        {
            LF2Character actor = AddActors(998, 2);
            Prepare();
            Project();
            actor.RelationTeam = 5;
            Project();
            var name = frame.GetKnockoutFeedRow(0).Attacker;
            Assert.That(name.Slot, Is.EqualTo(998));
            Assert.That(name.BattleGroup, Is.EqualTo(5));
            Assert.That(name.NativeGlyphResourceSlot, Is.EqualTo(feed.AttackerTeamColor ? 5 : 0));
            Assert.That(CacheCount(), Is.EqualTo(44));
        }

        [Test]
        public void FirstAndRepeatedPreparedProjectionHasCalibratedZeroEvents()
        {
            AddActors(998, 2);
            Prepare();
            MethodInfo prepareFrame = typeof(BattlePresentationFrame).GetMethod("PrepareCapacity",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(prepareFrame, Is.Not.Null);
            prepareFrame.Invoke(frame, new object[] { 1050, 1050, 1050 });
            world.RuntimeCapacity.Seal();
            string frameBefore = DescribeFrameStorage();
            using var recorder = new BattleScopedGcAllocationRecorder();
            var before = recorder.Calibrate();
            Assert.That(before.passed, Is.True, "Reliable positive/empty control required.");
            bool started = recorder.Begin();
            projection.Project(world, 20, frame);
            for (int index = 0; index < 32; index++)
            {
                frame.Reset(20);
                projection.Project(world, 20, frame);
            }
            var scope = recorder.End();
            var after = recorder.Calibrate();
            Assert.That(started, Is.True);
            Assert.That(BattleScopedGcAllocationRecorder.HasZeroEvents(scope, before, after), Is.True,
                "Full selected Project including first row; events=" + scope.allocationEvents +
                ", samples=" + scope.sampleCount + ", valid=" + scope.valid + ", before=" + before.passed +
                ", after=" + after.passed + ", storageBefore=" + frameBefore + ", storageAfter=" +
                DescribeFrameStorage() + ". Not a full Driver/FPS certificate.");
            Assert.That(frame.GetKnockoutFeedRow(0).Attacker.Text, Is.EqualTo("Com [Naruto]"));
            Assert.That(CacheCount(), Is.EqualTo(44));
        }

        [Test]
        public void ThirtyTickRowsAndFrozenTextRemainExactWithCurrentResourceAvailability()
        {
            AddActors(998, 2);
            world.BattleBuffersForServices.RecordNativeKnockout(new NativeKnockoutEvent(12, 7, 998, 999, 998, 998));
            Prepare();
            world.RuntimeCapacity.Seal();
            frame.Reset(39);
            projection.Project(world, 39, frame);
            Assert.That(frame.KnockoutFeedRowCount, Is.EqualTo(2));
            BattleKnockoutFeedRowSnapshot first = frame.GetKnockoutFeedRow(0);
            Assert.That(first.ImageScreenTop, Is.EqualTo(feed.ScreenTop));
            Assert.That(first.TypeResourceIndex, Is.EqualTo(3));
            Assert.That(first.TypeResourceAvailable,
                Is.EqualTo(File.Exists(source.ResolveImagePath(feed.TypeResourcePath(3), null))));
            Assert.That(first.Attacker.Text, Is.EqualTo("Com [Naruto]"));
            Assert.That(frame.GetKnockoutFeedRow(1).ImageScreenTop, Is.EqualTo(feed.ScreenTop + feed.RowSpacing));
            Assert.That(frame.GetKnockoutFeedRow(1).TypeResourceIndex, Is.Zero);
            var frozen = new BattlePresentationFrame();
            frozen.CopyFrom(frame);
            frame.Reset(40);
            projection.Project(world, 40, frame);
            Assert.That(frame.KnockoutFeedRowCount, Is.EqualTo(1));
            Assert.That(frame.GetKnockoutFeedRow(0).ImageScreenTop, Is.EqualTo(feed.ScreenTop + feed.RowSpacing));
            Assert.That(frozen.KnockoutFeedRowCount, Is.EqualTo(2));
            Assert.That(frozen.GetKnockoutFeedRow(0).Attacker.Text, Is.EqualTo("Com [Naruto]"));
            frame.Reset(41);
            projection.Project(world, 41, frame);
            Assert.That(frame.KnockoutFeedRowCount, Is.EqualTo(1));
            Assert.That(frame.GetKnockoutFeedRow(0).ImageScreenTop, Is.EqualTo(feed.ScreenTop));
        }

        private void Prepare()
        {
            MethodInfo method = typeof(BattleKnockoutFeedRowProjection).GetMethod(
                "PrepareLabels", InstanceFlags, null, new[] { typeof(BattleRuntimeDataCatalog) }, null);
            Assert.That(method, Is.Not.Null, "Cold managed catalog preparation API must exist.");
            method.Invoke(projection, new object[] { world.RuntimeDataCatalog });
        }

        private string DescribeFrameStorage()
        {
            var text = new StringBuilder();
            text.Append("rowCapacity=").Append(typeof(BattlePresentationFrame)
                .GetProperty("KnockoutFeedRowCapacity", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?.GetValue(frame)).Append(';');
            foreach (FieldInfo field in typeof(BattlePresentationFrame).GetFields(InstanceFlags))
            {
                if (field.GetValue(frame) is List<BattleKnockoutFeedRowSnapshot> rows)
                    text.Append(field.Name).Append('=').Append(rows.Count).Append('/').Append(rows.Capacity).Append(';');
            }
            foreach (MethodInfo method in typeof(BattlePresentationFrame).GetMethods(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                if (method.Name.IndexOf("Capacity", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    text.Append(method).Append(';');
                    foreach (ParameterInfo parameter in method.GetParameters())
                        text.Append(parameter.Name).Append(';');
                }
            return text.ToString();
        }

        private int CacheCount()
        {
            return ((IDictionary)typeof(BattleKnockoutFeedRowProjection)
                .GetField("labelCache", InstanceFlags).GetValue(projection)).Count;
        }

        private void Project()
        {
            frame.Reset(20);
            projection.Project(world, 20, frame);
        }

        private void SetCatalog(string first, string second)
        {
            world.RuntimeDataCatalog.Prepare(new[]
            {
                new ObjectDefinition { id = 2, type = 0 },
                new ObjectDefinition { id = 4, type = 0 },
                new ObjectDefinition { id = 5, type = 0 },
            }, id => new LF2CharacterDataWrapper(id, new LF2CharacterData
            {
                name = id == 2 ? first : id == 4 ? second : string.Empty,
            }));
        }

        private LF2Character AddActors(int slot, int objectId)
        {
            LF2Character attacker = Character(objectId, slot, 1);
            world.Register(attacker);
            LF2Character victim = Character(4, 999, 2);
            world.Register(victim);
            Assert.That(attacker.Runtime.SlotIndex, Is.EqualTo(slot));
            Assert.That(victim.Runtime.SlotIndex, Is.EqualTo(999));
            world.BattleBuffersForServices.RecordNativeKnockout(new NativeKnockoutEvent(10, 3, slot, 999, slot, slot));
            return attacker;
        }

        private static LF2Character Character(int objectId, int slot, int team)
        {
            var character = new LF2Character();
            character.ModuleInitialize();
            character.ObjectId = objectId;
            character.FrameCache.Load(new LF2CharacterDataWrapper(objectId, new LF2CharacterData
            {
                frames = new List<LF2FrameData>
                {
                    new LF2FrameData { frameId = 0, state = 0, wait = 1, next = 0 },
                },
            }));
            character.Frame.D = character.FrameCache.GetFrameDataById(0);
            character.Frame.PN = 0;
            character.Frame.N = 0;
            character.Initialize(500, 500);
            character.SetRequiredRuntimeSlot(slot);
            character.Team = team;
            character.RelationTeam = team;
            return character;
        }
    }
}
#endif
