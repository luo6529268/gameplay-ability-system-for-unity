#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
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
    [Category("NTSD28")]
    [Category("NTSD28_Q09")]
    public sealed class NTSD28Q09KnockoutFeedRowSnapshotEditorTests
    {
        private SimulationWorld world;
        private BattleKnockoutFeedRowProjection projection;
        private BattlePresentationFrame frame;
        private BattleContentSource source;
        private string modeText;

        [SetUp]
        public void SetUp()
        {
            string root = Path.Combine(Directory.GetParent(Application.dataPath).FullName,
                "Assets/NTSD/Content/LoganRuntime");
            source = BattleContentSource.ForLoganRuntime(root);
            modeText = File.ReadAllText(Path.Combine(root,
                "decoded_dat/data/mode/ntsd.dat"));
            world = new SimulationWorld();
            world.ResetRuntimeState();
            world.Runtime.Match.BattleGameModeId = 1;
            world.RuntimeDataCatalog.Prepare(
                new[]
                {
                    new ObjectDefinition { id = 2, type = 0 },
                    new ObjectDefinition { id = 4, type = 0 },
                    new ObjectDefinition { id = 5, type = 0 },
                },
                id => new LF2CharacterDataWrapper(id,
                    new LF2CharacterData { name = "Character" + id }));
            world.Register(Character(2, 0, 1));
            world.Register(Character(4, 1, 2));
            world.Register(Character(5, 2, 3));
            projection = new BattleKnockoutFeedRowProjection();
            projection.SetFeed(LoganModeKnockoutFeedInput.Parse(modeText), source);
            frame = new BattlePresentationFrame();
        }

        [Test]
        public void ThirtyTickBoundaryRetainsNativeRowSpacingAndFrozenCopy()
        {
            world.BattleBuffersForServices.RecordNativeKnockout(Event(10, 3, 0, 1));
            world.BattleBuffersForServices.RecordNativeKnockout(Event(12, 7, 0, 2));

            frame.Reset(39);
            projection.Project(world, 39, frame);
            Assert.That(frame.KnockoutFeedRowCount, Is.EqualTo(2));
            BattleKnockoutFeedRowSnapshot first = frame.GetKnockoutFeedRow(0);
            Assert.That(first.ImageScreenTop, Is.EqualTo(132));
            Assert.That(first.TypeResourceIndex, Is.EqualTo(3));
            Assert.That(first.TypeResourceAvailable, Is.True);
            Assert.That(first.Attacker.Text, Is.EqualTo("Remie [Character2]"));
            Assert.That(first.Attacker.NativeGlyphResourceSlot, Is.EqualTo(1));
            Assert.That(first.Victim.NativeGlyphResourceSlot, Is.EqualTo(2));
            Assert.That(first.Attacker.ScreenLeft,
                Is.EqualTo(560 - first.Attacker.Text.Length * 9));
            Assert.That(frame.GetKnockoutFeedRow(1).ImageScreenTop, Is.EqualTo(172));
            Assert.That(frame.GetKnockoutFeedRow(1).TypeResourceIndex, Is.Zero);

            var frozen = new BattlePresentationFrame();
            frozen.CopyFrom(frame);
            frame.Reset(40);
            projection.Project(world, 40, frame);
            Assert.That(frame.KnockoutFeedRowCount, Is.EqualTo(1));
            Assert.That(frame.GetKnockoutFeedRow(0).ImageScreenTop, Is.EqualTo(172));
            Assert.That(frozen.KnockoutFeedRowCount, Is.EqualTo(2));
            Assert.That(frozen.GetKnockoutFeedRow(0).Attacker.Text,
                Is.EqualTo("Remie [Character2]"));

            frame.Reset(41);
            projection.Project(world, 41, frame);
            Assert.That(frame.KnockoutFeedRowCount, Is.EqualTo(1));
            Assert.That(frame.GetKnockoutFeedRow(0).ImageScreenTop, Is.EqualTo(132));
        }

        [Test]
        public void MissingAndExcludedActorsConsumeRowsWithoutDrawing()
        {
            string excludedText = modeText.Replace("<bmp_end>",
                "id: 4\n<bmp_end>");
            projection.SetFeed(LoganModeKnockoutFeedInput.Parse(excludedText), source);
            world.BattleBuffersForServices.RecordNativeKnockout(Event(10, 1, 0, 1));
            world.BattleBuffersForServices.RecordNativeKnockout(Event(11, 1, 9, 2));
            world.BattleBuffersForServices.RecordNativeKnockout(Event(12, 1, 0, 2));

            frame.Reset(20);
            projection.Project(world, 20, frame);
            Assert.That(frame.KnockoutFeedRowCount, Is.EqualTo(1));
            Assert.That(frame.KnockoutFeedNativeRecordCount, Is.EqualTo(3));
            Assert.That(frame.KnockoutFeedMissingActorCount, Is.EqualTo(1));
            Assert.That(frame.GetKnockoutFeedRow(0).ImageScreenTop, Is.EqualTo(212));
            Assert.That(frame.GetKnockoutFeedRow(0).Victim.Text,
                Is.EqualTo("John [Character5]"));
        }

        [Test]
        public void BattleOnlyNamesAndModeGateDoNotUseLocalPlayerSettings()
        {
            projection.SetNames(new MatchConfig
            {
                nativeBattlePlayerNames = new List<string>
                {
                    "Other", "Kezeal", "John", "Herkato", "5",
                    "6", "7", "8", "<No name>", "",
                },
                nativeBracketPlayerNames = new List<bool> { true },
            });
            projection.SetFeed(LoganModeKnockoutFeedInput.Parse(modeText), source);
            world.BattleBuffersForServices.RecordNativeKnockout(Event(10, 1, 0, 1));
            frame.Reset(20);
            projection.Project(world, 20, frame);
            Assert.That(frame.GetKnockoutFeedRow(0).Attacker.Text,
                Is.EqualTo("[Other] [Character2]"));

            world.Runtime.Match.BattleGameModeId = 2;
            frame.Reset(21);
            projection.Project(world, 21, frame);
            Assert.That(frame.KnockoutFeedRowCount, Is.Zero);
            Assert.That(frame.KnockoutFeedNativeRecordCount, Is.EqualTo(1));

            world.Runtime.Match.BattleGameModeId = 1;
            projection.SetNames(new MatchConfig
            {
                nativeKnockoutFeedRuntimeDisplayEnabled = false,
            });
            projection.SetFeed(LoganModeKnockoutFeedInput.Parse(modeText), source);
            frame.Reset(22);
            projection.Project(world, 22, frame);
            Assert.That(frame.KnockoutFeedNativeRecordCount, Is.EqualTo(1));
            Assert.That(frame.KnockoutFeedRowCount, Is.Zero);

            projection.SetFeed(null, null);
            frame.Reset(23);
            projection.Project(world, 23, frame);
            Assert.That(frame.KnockoutFeedRowCount, Is.Zero);
        }

        [Test]
        public void NativeNameOverridesRequireTenBoundedPayloads()
        {
            Assert.Throws<ArgumentException>(() => projection.SetNames(
                new MatchConfig
                {
                    nativeBattlePlayerNames = new List<string> { "Partial" },
                }));

            var names = new List<string>
            {
                "Remie", "Kezeal", "John", "Herkato", "5",
                "6", "7", "8", "<No name>", "",
            };
            names[0] = "12345678901";
            Assert.Throws<ArgumentException>(() => projection.SetNames(
                new MatchConfig { nativeBattlePlayerNames = names }));

            names[0] = "one\0two";
            Assert.Throws<ArgumentException>(() => projection.SetNames(
                new MatchConfig { nativeBattlePlayerNames = names }));
        }

        [Test]
        public void CentralOnlyWorkerPublicationCarriesRowsWithoutUnityResourceBinding()
        {
            world.SetBattlePresentationBackend(BattlePresentationBackendMode.CentralOnly);
            world.BattlePresentation.ConfigureKnockoutFeedNames(null);
            world.BattlePresentation.ConfigureKnockoutFeedContent(
                LoganModeKnockoutFeedInput.Parse(modeText), source);
            world.BattleBuffersForServices.RecordNativeKnockout(Event(10, 3, 0, 1));

            world.BattlePresentation.BeginSimulationWorkerFrame(world, 20);
            BattlePresentationFrame published = world.BattlePresentation.PublishedFrame;
            Assert.That(published, Is.Not.Null);
            Assert.That(published.KnockoutFeedRowCount, Is.EqualTo(1));
            Assert.That(published.GetKnockoutFeedRow(0).Attacker.Text,
                Is.EqualTo("Remie [Character2]"));
            Assert.That(published.BoundCatalogForAcceptance,
                Is.SameAs(BattleSpriteCatalog.Empty));

            world.BattlePresentation.Reset();
            world.BattlePresentation.BeginSimulationWorkerFrame(world, 21);
            Assert.That(world.BattlePresentation.PublishedFrame.KnockoutFeedRowCount,
                Is.EqualTo(1), "Snapshot restore resets frame buffers, not match input.");

            world.BattlePresentation.ClearKnockoutFeedSession();
            world.BattlePresentation.Reset();
            world.BattlePresentation.BeginSimulationWorkerFrame(world, 22);
            Assert.That(world.BattlePresentation.PublishedFrame.KnockoutFeedRowCount,
                Is.Zero, "Ordered shutdown must release the selected feed.");
        }

        private static NativeKnockoutEvent Event(int tick, int type,
            int fourOwnerSlot, int victimSlot)
        {
            return new NativeKnockoutEvent(tick, type, fourOwnerSlot,
                victimSlot, fourOwnerSlot, fourOwnerSlot);
        }

        private static LF2Character Character(int objectId, int slot, int team)
        {
            var data = new LF2CharacterData
            {
                name = "Character" + objectId,
                frames = new List<LF2FrameData>
                {
                    new LF2FrameData { frameId = 0, state = 0, wait = 1, next = 0 },
                },
            };
            var character = new LF2Character();
            character.ModuleInitialize();
            character.ObjectId = objectId;
            character.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
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
