#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.DatParser;
using NTSD.EditorTools;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q07NonCharacterHitFa7EditorTests
    {
        private const string RuntimeRoot = "Assets/NTSD/Content/LoganRuntime";
        private const string KindScenario =
            "artifacts/diagnostics/NTSD28-R15-KIND-DEPENDENT-SOURCE-PREFLIGHT-001/kind213-action176-to-206-action0-candidate.json";
        private const string FullTickWitnessRoot =
            "artifacts/diagnostics/NTSD28-Q07-HITFA7-TARGET-FULLTICK-WITNESS-001";

        [Test]
        public void MissingPreassignedTargetDoesNotBirthAnExtraClone()
        {
            string output = ProjectPath(
                "Temp/NTSD28UnityTrace/FocusedTests/q07-hitfa7-missing-target.raw.jsonl");
            NTSD28UnityRawCaptureEditor.RunLoganScenarioForTests(
                ProjectPath(RuntimeRoot), ProjectPath(KindScenario), output);

            TickEnvelope tick = JsonUtility.FromJson<TickEnvelope>(
                File.ReadAllLines(output)[1]);
            Assert.That(tick.completedTick, Is.EqualTo(1));
            Assert.That(tick.entities.Select(entity => entity.slot),
                Is.EqualTo(new[] { 1 }));
            Assert.That(tick.entities[0].identity.objectId, Is.EqualTo(213));
            Assert.That(tick.entities[0].frame.action, Is.EqualTo(40));
        }

        [Test]
        public void RealOid875PreassignedTargetUsesOneVerticalStepAndNoClone()
        {
            LoganObjectCatalog catalog = LoganObjectCatalog.Read(
                BattleContentSource.ForLoganRuntime(ProjectPath(RuntimeRoot)));
            var configs = CharacterAnimtorManager.BuildCharacterFrameConfigsFromCatalog(catalog);
            Assert.That(configs[875].characterData.frames.Any(frame =>
                frame.frameId == 55 && frame.hit_Fa == 7), Is.True);

            var world = new SimulationWorld();
            var references = new BattleLogicReferencePool();
            world.BindLogicReferencePool(references);
            world.PrepareRuntimeDataCatalogForBattle(
                catalog.Entries.Select(entry =>
                    new ObjectDefinition(entry.Id, entry.Type, entry.DatPath)).ToArray(),
                id => configs.TryGetValue(id, out LF2CharacterDataWrapper wrapper)
                    ? wrapper : null,
                loganCatalog: catalog);
            world.SetLogicOnlyEntityMaterialization(true);

            var target = new LF2Character { ObjectId = 99 };
            target.ModuleInitialize();
            target.SetRequiredRuntimeSlot(0);
            target.ModuleBind(configs[99], 99, world);
            target.Initialize(500, 500);
            target.Team = 2;
            target.Runtime.SetPosition(700, 0, 120);
            target.Runtime.SyncIntegerPosition();

            var subject = new LF2SpecialAttack { ObjectId = 875 };
            subject.FrameCache.Load(configs[875]);
            subject.ImmediateFrame(55);
            subject.SetRequiredRuntimeSlot(50);
            subject.Team = 1;
            subject.Health.HP = 100;
            subject.Runtime.SetPosition(400, -30, 100);
            subject.Runtime.SetVelocity(0, 3.8, 0);
            subject.Runtime.SyncIntegerPosition();
            subject.ObjectAiTargetSlot3F8 = 0;
            world.Register(subject);

            subject.RunFrameLogicBeforeAdvance();

            Assert.That(world.ObjectCount, Is.EqualTo(2));
            Assert.That(subject.Frame.N, Is.EqualTo(55));
            Assert.That(subject.Runtime.Vx, Is.EqualTo(1.4));
            Assert.That(subject.Runtime.Vy, Is.EqualTo(4.2));
            Assert.That(subject.Runtime.Vz, Is.EqualTo(0.4));
            Assert.That(subject.Runtime.Y, Is.EqualTo(-25.8));
            Assert.That(subject.Runtime.YInt, Is.EqualTo(-30));

            CharacterMechanics.StepNonCharacterBattleLogic(subject.Runtime, 0.5);
            Assert.That(subject.Runtime.NativePreviousY104, Is.EqualTo(-30));
            Assert.That(subject.Runtime.YInt, Is.EqualTo(-30));
            Assert.That(subject.Runtime.Y, Is.EqualTo(-21.6));

            subject.Runtime.SetPosition(400, -30, 100);
            subject.Runtime.SetVelocity(14, 4.2, 2.2);
            subject.Runtime.SyncIntegerPosition();
            subject.RunFrameLogicBeforeAdvance();
            Assert.That(subject.Frame.N, Is.EqualTo(55));
            Assert.That(subject.Runtime.Vx, Is.EqualTo(14));
            Assert.That(subject.Runtime.Vz, Is.EqualTo(2.2));
            Assert.That(subject.Runtime.YInt, Is.EqualTo(-30));

            subject.Runtime.SetPosition(400, -20, 100);
            subject.Runtime.SetVelocity(20, 4.2, 3);
            subject.Runtime.SyncIntegerPosition();
            subject.RunFrameLogicBeforeAdvance();
            Assert.That(subject.Frame.N, Is.EqualTo(60));
            Assert.That(subject.Runtime.Y, Is.EqualTo(-15.8));
            Assert.That(subject.Runtime.YInt, Is.EqualTo(-20));
            Assert.That(subject.Runtime.Vx, Is.Zero);
            Assert.That(subject.Runtime.Vy, Is.Zero);
            Assert.That(subject.Runtime.Vz, Is.Zero);
            Assert.That(subject.Runtime.IsFacingLeft, Is.True);
            Assert.That(world.ObjectCount, Is.EqualTo(2));
        }

        [TestCase(false, 4, 0.0)]
        [TestCase(true, 4, 0.0)]
        [TestCase(true, -4, 0.0)]
        [TestCase(true, 6, 0.4)]
        public void RealOid875DepthDeadZoneUsesSourceCoordinates(
            bool fixedView, int sourceDepthGap, double expectedDepthVelocity)
        {
            LoganObjectCatalog catalog = LoganObjectCatalog.Read(
                BattleContentSource.ForLoganRuntime(ProjectPath(RuntimeRoot)));
            var configs = CharacterAnimtorManager.BuildCharacterFrameConfigsFromCatalog(catalog);
            Assert.That(configs[875].characterData.frames.Any(frame =>
                frame.frameId == 55 && frame.hit_Fa == 7), Is.True);

            var world = new SimulationWorld();
            if (fixedView)
                world.ConfigureFixedViewRunDistance(2048, 1152);
            world.BindLogicReferencePool(new BattleLogicReferencePool());
            world.PrepareRuntimeDataCatalogForBattle(
                catalog.Entries.Select(entry =>
                    new ObjectDefinition(entry.Id, entry.Type, entry.DatPath)).ToArray(),
                id => configs.TryGetValue(id, out LF2CharacterDataWrapper wrapper)
                    ? wrapper : null,
                loganCatalog: catalog);
            world.SetLogicOnlyEntityMaterialization(true);

            var target = new LF2Character { ObjectId = 99 };
            target.ModuleInitialize();
            target.SetRequiredRuntimeSlot(0);
            target.ModuleBind(configs[99], 99, world);
            target.Initialize(500, 500);
            target.Team = 2;

            var subject = new LF2SpecialAttack { ObjectId = 875 };
            subject.FrameCache.Load(configs[875]);
            subject.ImmediateFrame(55);
            subject.SetRequiredRuntimeSlot(50);
            subject.Team = 1;
            subject.Health.HP = 100;
            subject.ObjectAiTargetSlot3F8 = 0;
            world.Register(subject);

            void Place(NTSDEntityRuntime runtime, double y, double sourceZ)
            {
                runtime.SetPosition(
                    world.SpatialProjection.SourceToViewX(400), y,
                    world.SpatialProjection.SourceToViewZ(sourceZ));
                runtime.SyncIntegerPosition();
                runtime.SetSourceRulePosition(400, sourceZ);
                runtime.SyncSourceRuleIntegerPosition();
            }

            try
            {
                Place(subject.Runtime, -40, 380);
                Place(target.Runtime, 0, 380 + sourceDepthGap);
                subject.Runtime.SetVelocity(0, 0, 0);
                double initialViewZ = subject.Runtime.Z;

                subject.RunFrameLogicBeforeAdvance();

                Assert.That(subject.Runtime.Vz,
                    Is.EqualTo(expectedDepthVelocity).Within(1e-9));
                Assert.That(subject.Runtime.Vx, Is.Zero);
                Assert.That(subject.Runtime.Vy, Is.EqualTo(0.4).Within(1e-9));
                Assert.That(subject.Runtime.Y, Is.EqualTo(-39.6).Within(1e-9));
                Assert.That(subject.Runtime.YInt, Is.EqualTo(-40));
                Assert.That(subject.Runtime.SourceRuleZ, Is.EqualTo(380));
                Assert.That(subject.Frame.N, Is.EqualTo(55));
                Assert.That(subject.ObjectAiTargetSlot3F8, Is.Zero);
                Assert.That(world.ObjectCount, Is.EqualTo(2));

                CharacterMechanics.StepNonCharacterBattleLogic(
                    subject.Runtime, 0.5,
                    world.SpatialProjection.HorizontalScale,
                    world.SpatialProjection.DepthScale);

                double expectedScale = fixedView ? 1152.0 / 730.0 : 1.0;
                Assert.That(subject.Runtime.SourceRuleZ,
                    Is.EqualTo(380 + expectedDepthVelocity).Within(1e-9));
                Assert.That(subject.Runtime.Z - initialViewZ,
                    Is.EqualTo(expectedDepthVelocity * expectedScale).Within(1e-9));
                Assert.That(target.Runtime.SourceRuleZ,
                    Is.EqualTo(380 + sourceDepthGap));
                Assert.That(subject.Runtime.NativePreviousY104, Is.EqualTo(-40));
                Assert.That(world.ObjectCount, Is.EqualTo(2));
            }
            finally
            {
                world.Unregister(subject);
                world.Unregister(target);
            }
        }

        [TestCase(false, 207, 115, 14, -100.25, 2.8, 0, -100.25, 2.8)]
        [TestCase(true, 207, 115, 14, -100.25, 2.8, 0, -100.25, 2.8)]
        [TestCase(true, 207, 115, 14, -40.25, -2.8, 0, -40.25, -2.8)]
        [TestCase(true, 207, 115, 14, 3.75, -2.8, 0, 3.75, -2.8)]
        [TestCase(true, 207, 115, 14, -100.25, 2.8, 9, -100.25, 2.8)]
        [TestCase(true, 518, 1, 2, -100, 2.8, 0, -99, 2.0)]
        [TestCase(true, 907, 190, 12, -100, 2.8, 0, -99, 2.0)]
        public void IndexedHitFa14CommonTailPreservesVerticalStateAndAction(
            bool fixedView, int objectId, int frameId, int hitFa,
            double initialY, double initialVy, double initialVx,
            double expectedY, double expectedVy)
        {
            LoganObjectCatalog catalog = LoganObjectCatalog.Read(
                BattleContentSource.ForLoganRuntime(ProjectPath(RuntimeRoot)));
            var configs = CharacterAnimtorManager.BuildCharacterFrameConfigsFromCatalog(catalog);
            Assert.That(configs[objectId].characterData.frames.Any(frame =>
                frame.frameId == frameId && frame.hit_Fa == hitFa), Is.True);

            var world = new SimulationWorld();
            if (fixedView)
                world.ConfigureFixedViewRunDistance(2048, 1152);
            world.BindLogicReferencePool(new BattleLogicReferencePool());
            world.PrepareRuntimeDataCatalogForBattle(
                catalog.Entries.Select(entry =>
                    new ObjectDefinition(entry.Id, entry.Type, entry.DatPath)).ToArray(),
                id => configs.TryGetValue(id, out LF2CharacterDataWrapper wrapper)
                    ? wrapper : null,
                loganCatalog: catalog);
            world.SetLogicOnlyEntityMaterialization(true);

            var target = new LF2Character { ObjectId = 99 };
            target.ModuleInitialize();
            target.SetRequiredRuntimeSlot(0);
            target.ModuleBind(configs[99], 99, world);
            target.Initialize(500, 500);
            target.Team = 2;
            target.RelationTeam = 2;

            var subject = new LF2SpecialAttack { ObjectId = objectId };
            subject.FrameCache.Load(configs[objectId]);
            subject.ImmediateFrame(frameId);
            subject.SetRequiredRuntimeSlot(50);
            subject.Team = 1;
            subject.RelationTeam = 1;
            subject.Health.HP = 100;
            subject.ObjectAiTargetSlot3F8 = 0;
            world.Register(subject);

            void Place(NTSDEntityRuntime runtime, double y, int sourceZ)
            {
                runtime.SetPosition(
                    world.SpatialProjection.SourceToViewX(400), y,
                    world.SpatialProjection.SourceToViewZ(sourceZ));
                runtime.SyncIntegerPosition();
                runtime.SetSourceRulePosition(400, sourceZ);
                runtime.SyncSourceRuleIntegerPosition();
            }

            try
            {
                Place(subject.Runtime, initialY, 600);
                Place(target.Runtime, 0, 604);
                subject.Runtime.SetVelocity(initialVx, initialVy, 0);
                int initialYInt = subject.Runtime.YInt;

                subject.RunFrameLogicBeforeAdvance();

                Assert.That(subject.Frame.N, Is.EqualTo(frameId));
                Assert.That(subject.Runtime.Y, Is.EqualTo(expectedY).Within(1e-9));
                Assert.That(subject.Runtime.Vy, Is.EqualTo(expectedVy).Within(1e-9));
                Assert.That(subject.Runtime.YInt,
                    Is.EqualTo(hitFa == 14 ? initialYInt : -99));
                Assert.That(subject.Runtime.Vx, Is.EqualTo(initialVx));
                Assert.That(subject.Runtime.Vz, Is.Zero);
                Assert.That(subject.Runtime.SourceRuleX, Is.EqualTo(400));
                Assert.That(subject.Runtime.SourceRuleZ, Is.EqualTo(600));
                Assert.That(subject.Health.HP, Is.EqualTo(100));
                Assert.That(target.Health.HP, Is.EqualTo(500));
                Assert.That(target.Runtime.Y, Is.Zero);
                Assert.That(subject.ObjectAiTargetSlot3F8, Is.Zero);
                Assert.That(world.ObjectCount, Is.EqualTo(2));
            }
            finally
            {
                world.Unregister(subject);
                world.Unregister(target);
            }
        }

        [TestCase(false, 875, 50, 3, 10, true, 0.0)]
        [TestCase(true, 875, 50, 3, 10, true, 0.0)]
        [TestCase(true, 875, 50, 3, -10, true, 0.0)]
        [TestCase(true, 700, 54, 1, 6, true, 0.0)]
        [TestCase(true, 518, 1, 2, 4, true, 0.0)]
        [TestCase(true, 124, 40, 12, 4, true, 0.0)]
        [TestCase(true, 875, 50, 3, 11, true, 0.17)]
        [TestCase(true, 875, 50, 3, 10, false, 0.17)]
        public void IndexedTrackingDeadZoneUsesOneRulePositionPair(
            bool fixedView, int objectId, int frameId, int hitFa,
            int sourceDepthGap, bool initializeTargetSource,
            double expectedDepthVelocity)
        {
            LoganObjectCatalog catalog = LoganObjectCatalog.Read(
                BattleContentSource.ForLoganRuntime(ProjectPath(RuntimeRoot)));
            var configs = CharacterAnimtorManager.BuildCharacterFrameConfigsFromCatalog(catalog);
            Assert.That(configs[objectId].characterData.frames.Any(frame =>
                frame.frameId == frameId && frame.hit_Fa == hitFa), Is.True);

            var world = new SimulationWorld();
            if (fixedView)
                world.ConfigureFixedViewRunDistance(2048, 1152);
            world.BindLogicReferencePool(new BattleLogicReferencePool());
            world.PrepareRuntimeDataCatalogForBattle(
                catalog.Entries.Select(entry =>
                    new ObjectDefinition(entry.Id, entry.Type, entry.DatPath)).ToArray(),
                id => configs.TryGetValue(id, out LF2CharacterDataWrapper wrapper)
                    ? wrapper : null,
                loganCatalog: catalog);
            world.SetLogicOnlyEntityMaterialization(true);

            var target = new LF2Character { ObjectId = 99 };
            target.ModuleInitialize();
            target.SetRequiredRuntimeSlot(0);
            target.ModuleBind(configs[99], 99, world);
            target.Initialize(500, 500);
            target.Team = 2;
            target.RelationTeam = 2;

            LF2Entity subject;
            if (objectId == 124)
            {
                var weapon = new LF2Weapon { ObjectId = objectId };
                weapon.SetWeaponType(4);
                subject = weapon;
            }
            else
            {
                subject = new LF2SpecialAttack { ObjectId = objectId };
            }
            subject.FrameCache.Load(configs[objectId]);
            subject.ImmediateFrame(frameId);
            subject.SetRequiredRuntimeSlot(50);
            subject.Team = 1;
            subject.RelationTeam = 1;
            subject.Health.HP = 100;
            subject.ObjectAiTargetSlot3F8 = 0;
            world.Register(subject);

            void Place(NTSDEntityRuntime runtime, double y, int sourceZ)
            {
                runtime.SetPosition(
                    world.SpatialProjection.SourceToViewX(400), y,
                    world.SpatialProjection.SourceToViewZ(sourceZ));
                runtime.SyncIntegerPosition();
                runtime.SetSourceRulePosition(400, sourceZ);
                runtime.SyncSourceRuleIntegerPosition();
            }

            try
            {
                Place(subject.Runtime, -100, 380);
                Place(target.Runtime, 0, 380 + sourceDepthGap);
                target.Runtime.SourceRulePositionInitialized = initializeTargetSource;
                subject.Runtime.SetVelocity(0, 0, 0);
                double initialViewZ = subject.Runtime.Z;

                subject.RunFrameLogicBeforeAdvance();

                Assert.That(subject.Runtime.Vz,
                    Is.EqualTo(expectedDepthVelocity).Within(1e-7));
                Assert.That(subject.Runtime.Vx, Is.Zero);
                Assert.That(subject.Runtime.SourceRuleZ, Is.EqualTo(380));
                Assert.That(subject.ObjectAiTargetSlot3F8, Is.Zero);
                Assert.That(subject.Frame.N, Is.EqualTo(frameId));
                Assert.That(world.ObjectCount, Is.EqualTo(2));

                CharacterMechanics.StepNonCharacterBattleLogic(
                    subject.Runtime, 0.5,
                    world.SpatialProjection.HorizontalScale,
                    world.SpatialProjection.DepthScale);

                double expectedScale = fixedView ? 1152.0 / 730.0 : 1.0;
                Assert.That(subject.Runtime.SourceRuleZ,
                    Is.EqualTo(380 + expectedDepthVelocity).Within(1e-7));
                Assert.That(subject.Runtime.Z - initialViewZ,
                    Is.EqualTo(expectedDepthVelocity * expectedScale).Within(1e-7));
                Assert.That(target.Runtime.SourceRuleZ,
                    Is.EqualTo(380 + sourceDepthGap));
                Assert.That(world.ObjectCount, Is.EqualTo(2));
            }
            finally
            {
                world.Unregister(subject);
                world.Unregister(target);
            }
        }

        [TestCase(false, 29, 9, true)]
        [TestCase(true, 29, 9, true)]
        [TestCase(true, 30, 9, false)]
        [TestCase(true, 29, 10, false)]
        public void IndexedRecoveryUsesStrictSourceDistanceBounds(
            bool fixedView, int sourceDeltaX, int sourceDeltaZ, bool expectedRecovery)
        {
            LoganObjectCatalog catalog = LoganObjectCatalog.Read(
                BattleContentSource.ForLoganRuntime(ProjectPath(RuntimeRoot)));
            var configs = CharacterAnimtorManager.BuildCharacterFrameConfigsFromCatalog(catalog);
            Assert.That(configs[219].characterData.frames.Any(frame =>
                frame.frameId == 0 && frame.hit_Fa == 4), Is.True);

            var world = new SimulationWorld();
            if (fixedView)
                world.ConfigureFixedViewRunDistance(2048, 1152);
            world.BindLogicReferencePool(new BattleLogicReferencePool());
            world.PrepareRuntimeDataCatalogForBattle(
                catalog.Entries.Select(entry =>
                    new ObjectDefinition(entry.Id, entry.Type, entry.DatPath)).ToArray(),
                id => configs.TryGetValue(id, out LF2CharacterDataWrapper wrapper)
                    ? wrapper : null,
                loganCatalog: catalog);
            world.SetLogicOnlyEntityMaterialization(true);

            var target = new LF2Character { ObjectId = 99 };
            target.ModuleInitialize();
            target.SetRequiredRuntimeSlot(0);
            target.ModuleBind(configs[99], 99, world);
            target.Initialize(500, 500);
            target.Team = 2;

            var subject = new LF2SpecialAttack { ObjectId = 219 };
            subject.FrameCache.Load(configs[219]);
            subject.ImmediateFrame(0);
            subject.SetRequiredRuntimeSlot(50);
            subject.Team = 1;
            subject.Health.HP = 100;
            subject.ObjectAiTargetSlot3F8 = 0;
            world.Register(subject);

            void Place(NTSDEntityRuntime runtime, int sourceX, double y, int sourceZ)
            {
                runtime.SetPosition(
                    world.SpatialProjection.SourceToViewX(sourceX), y,
                    world.SpatialProjection.SourceToViewZ(sourceZ));
                runtime.SyncIntegerPosition();
                runtime.SetSourceRulePosition(sourceX, sourceZ);
                runtime.SyncSourceRuleIntegerPosition();
            }

            try
            {
                Place(subject.Runtime, 400, -40, 380);
                Place(target.Runtime, 400 + sourceDeltaX, 0, 380 + sourceDeltaZ);
                target.CatchTimer = 0;
                subject.Runtime.SetVelocity(0, 0, 0);

                subject.RunFrameLogicBeforeAdvance();

                Assert.That(subject.Frame.N, Is.EqualTo(expectedRecovery ? 60 : 0));
                Assert.That(target.CatchTimer, Is.EqualTo(expectedRecovery ? 100 : 0));
                Assert.That(subject.Runtime.Vx,
                    Is.EqualTo(expectedRecovery ? 0.0 : 0.7).Within(1e-7));
                Assert.That(subject.Runtime.Vz,
                    Is.EqualTo(expectedRecovery ? 0.0 : 0.4).Within(1e-7));
                Assert.That(subject.Runtime.SourceRuleX, Is.EqualTo(400));
                Assert.That(subject.Runtime.SourceRuleZ, Is.EqualTo(380));
                Assert.That(subject.Runtime.Y, Is.EqualTo(-40));
                Assert.That(target.Health.HP, Is.EqualTo(500));
                Assert.That(subject.ObjectAiTargetSlot3F8, Is.Zero);
                Assert.That(world.ObjectCount, Is.EqualTo(2));
            }
            finally
            {
                world.Unregister(subject);
                world.Unregister(target);
            }
        }

        [Test]
        public void IndexedHitFa14CommonTailSurvivesOneFullDriverTick()
        {
            LoganObjectCatalog catalog = LoganObjectCatalog.Read(
                BattleContentSource.ForLoganRuntime(ProjectPath(RuntimeRoot)));
            var configs = CharacterAnimtorManager.BuildCharacterFrameConfigsFromCatalog(catalog);
            LF2CharacterDataWrapper definition = configs[207];
            LF2FrameData sourceFrame = definition.characterData.frames.First(frame =>
                frame.frameId == 115);
            Assert.That(catalog.Entries.Single(entry => entry.Id == 207).Type, Is.EqualTo(3));
            Assert.That(definition.characterData.type_sub, Is.EqualTo(207));
            Assert.That(sourceFrame.hit_Fa, Is.EqualTo(14));
            Assert.That(sourceFrame.wait, Is.EqualTo(1));
            Assert.That(sourceFrame.state, Is.EqualTo(3003));
            Assert.That(sourceFrame.nativeDvx, Is.Zero);
            Assert.That(sourceFrame.nativeDvy, Is.Zero);
            Assert.That(sourceFrame.nativeDvz, Is.Zero);
            Assert.That(sourceFrame.opoint, Is.Null);
            Assert.That(sourceFrame.opoints, Is.Empty);

            NTSD28UnityRawCaptureEditor.WithLoganScenarioForReplayTests(
                ProjectPath(RuntimeRoot),
                ProjectPath(FullTickWitnessRoot + "/scenario.json"),
                BattleRuntimeProfile.Authority400, 3,
                (driver, inputs, identity) =>
                {
                    SimulationWorld world = driver.World;
                    world.ConfigureFixedViewRunDistance(2048, 1152);
                    LF2Entity target = world.FindEntityByRuntimeSlotForQuery(0);
                    LF2Entity oldSubject = world.FindEntityByRuntimeSlotForQuery(1);
                    Assert.That(target?.ObjectId, Is.EqualTo(99));
                    Assert.That(oldSubject?.ObjectId, Is.EqualTo(875));
                    Assert.That(world.StageDepthBoundsArePhysical, Is.False);

                    world.Unregister(oldSubject);
                    Assert.That(world.FindEntityByRuntimeSlotForQuery(1), Is.Null);
                    Assert.That(oldSubject.RegisteredWorldForSimulation, Is.Null);
                    Assert.That(oldSubject.Runtime.SlotIndex, Is.LessThan(0));

                    var subject = new LF2SpecialAttack { ObjectId = 207 };
                    subject.FrameCache.Load(definition);
                    subject.ImmediateFrame(115);
                    subject.InitializeNativeDefinitionIdentityForSpawn();
                    subject.InitializeNativeArmorRuntimeFromCurrentDefinitionForSpawn();
                    subject.SetRequiredRuntimeSlot(1);
                    subject.Team = 1;
                    subject.RelationTeam = 1;
                    subject.OwnerEntityIndex = 1;
                    subject.Health.HP = 500;
                    subject.Runtime.HPBound = 500;
                    subject.Runtime.HP3 = 500;
                    subject.Runtime.MP = 200;
                    subject.Runtime.PP = 200;
                    subject.ObjectAiTargetSlot3F8 = 0;
                    world.Register(subject);
                    Assert.That(world.FindEntityByRuntimeSlotForQuery(1), Is.SameAs(subject));
                    Assert.That(subject.RegisteredWorldForSimulation, Is.SameAs(world));
                    Assert.That(LF2Entity.ResolveCurrentDataObjectType(subject), Is.EqualTo(3));
                    Assert.That(subject.AttackingCounter, Is.Zero);

                    var rosterSlot = world.Runtime.Roster.Slots[1];
                    Assert.That(rosterSlot.Active, Is.True);
                    Assert.That(rosterSlot.RuntimeSlotIndex, Is.EqualTo(1));
                    rosterSlot.CharacterId = 207;
                    rosterSlot.StableId = subject.Runtime.StableId;
                    Assert.That(world.Runtime.Roster.Slots[1].StableId,
                        Is.EqualTo(subject.Runtime.StableId));
                    Assert.That(world.Runtime.Roster.ActiveSlotCount, Is.EqualTo(2));

                    void Place(LF2Entity entity, double y, int sourceZ)
                    {
                        entity.Runtime.SetPosition(
                            world.SpatialProjection.SourceToViewX(400), y,
                            world.SpatialProjection.SourceToViewZ(sourceZ));
                        entity.Runtime.SyncIntegerPosition();
                        entity.Runtime.SetSourceRulePosition(400, sourceZ);
                        entity.Runtime.SyncSourceRuleIntegerPosition();
                        entity.Runtime.SetVelocity(0, 0, 0);
                    }

                    Place(subject, -100.25, 600);
                    Place(target, 0, 604);
                    subject.Runtime.SetVelocity(9, 2.8, 0);
                    double initialViewX = subject.Runtime.X;
                    double initialViewZ = subject.Runtime.Z;
                    Assert.That(world.ObjectCount, Is.EqualTo(2));

                    Assert.That(driver.StepOneTick(
                        inputs[0], ignorePaused: true,
                        buildPresentation: false), Is.True);

                    Assert.That(world.FindEntityByRuntimeSlotForQuery(1), Is.SameAs(subject));
                    Assert.That(subject.Frame.N, Is.EqualTo(115));
                    Assert.That(subject.Frame.D.state, Is.EqualTo(3003));
                    Assert.That(subject.Runtime.Y, Is.EqualTo(-97.45).Within(1e-9));
                    Assert.That(subject.Runtime.Vy, Is.EqualTo(2.8).Within(1e-9));
                    Assert.That(subject.Runtime.Vx, Is.EqualTo(9));
                    Assert.That(subject.Runtime.Vz, Is.Zero);
                    Assert.That(subject.Runtime.SourceRuleX, Is.EqualTo(409).Within(1e-9));
                    Assert.That(subject.Runtime.SourceRuleZ, Is.EqualTo(600).Within(1e-9));
                    Assert.That(subject.Runtime.X - initialViewX,
                        Is.EqualTo(9 * 2048.0 / 1333.0).Within(1e-9));
                    Assert.That(subject.Runtime.Z - initialViewZ, Is.Zero.Within(1e-9));
                    Assert.That(target.Runtime.SourceRuleZ, Is.EqualTo(604));
                    Assert.That(subject.ObjectAiTargetSlot3F8, Is.Zero);
                    Assert.That(world.ObjectCount, Is.EqualTo(2));
                },
                useProjectMode: true);
        }

        [Test]
        public void IndexedTrackingSourcePairSurvivesOneFullDriverTick()
        {
            string sourcePath = ProjectPath(RuntimeRoot + "/decoded_dat/c/nar/a/atk.dat");
            var definition = new LF2CharacterDataWrapper(518,
                CharacterAnimtorManager.BuildCharacterDataFromSource(
                    File.ReadAllText(sourcePath), sourcePath,
                    BattleContentSource.ForLoganRuntime(ProjectPath(RuntimeRoot))));
            LF2FrameData sourceFrame = definition.characterData.frames.First(frame =>
                frame.frameId == 1);
            Assert.That(sourceFrame.hit_Fa, Is.EqualTo(2));
            Assert.That(sourceFrame.nativeDvz, Is.Zero);
            Assert.That(sourceFrame.opoint, Is.Null);
            Assert.That(sourceFrame.opoints, Is.Empty);

            NTSD28UnityRawCaptureEditor.WithLoganScenarioForReplayTests(
                ProjectPath(RuntimeRoot),
                ProjectPath(FullTickWitnessRoot + "/scenario.json"),
                BattleRuntimeProfile.Authority400, 3,
                (driver, inputs, identity) =>
                {
                    SimulationWorld world = driver.World;
                    world.ConfigureFixedViewRunDistance(2048, 1152);
                    LF2Entity target = world.FindEntityByRuntimeSlotForQuery(0);
                    LF2Entity oldSubject = world.FindEntityByRuntimeSlotForQuery(1);
                    Assert.That(target?.ObjectId, Is.EqualTo(99));
                    Assert.That(oldSubject?.ObjectId, Is.EqualTo(875));
                    Assert.That(world.StageDepthBoundsArePhysical, Is.False);

                    world.Unregister(oldSubject);
                    Assert.That(world.FindEntityByRuntimeSlotForQuery(1), Is.Null);
                    Assert.That(oldSubject.RegisteredWorldForSimulation, Is.Null);
                    Assert.That(oldSubject.Runtime.SlotIndex, Is.LessThan(0));

                    var subject = new LF2SpecialAttack { ObjectId = 518 };
                    subject.FrameCache.Load(definition);
                    subject.ImmediateFrame(1);
                    subject.InitializeNativeDefinitionIdentityForSpawn();
                    subject.InitializeNativeArmorRuntimeFromCurrentDefinitionForSpawn();
                    subject.SetRequiredRuntimeSlot(1);
                    subject.Team = 1;
                    subject.RelationTeam = 1;
                    subject.OwnerEntityIndex = 1;
                    subject.Health.HP = 500;
                    subject.Runtime.HPBound = 500;
                    subject.Runtime.HP3 = 500;
                    subject.Runtime.MP = 200;
                    subject.Runtime.PP = 200;
                    subject.ObjectAiTargetSlot3F8 = 0;
                    world.Register(subject);
                    Assert.That(world.FindEntityByRuntimeSlotForQuery(1), Is.SameAs(subject));
                    Assert.That(subject.RegisteredWorldForSimulation, Is.SameAs(world));

                    var rosterSlot = world.Runtime.Roster.Slots[1];
                    Assert.That(rosterSlot.Active, Is.True);
                    Assert.That(rosterSlot.RuntimeSlotIndex, Is.EqualTo(1));
                    rosterSlot.CharacterId = 518;
                    rosterSlot.StableId = subject.Runtime.StableId;
                    Assert.That(world.Runtime.Roster.Slots[1].StableId,
                        Is.EqualTo(subject.Runtime.StableId));
                    Assert.That(world.Runtime.Roster.ActiveSlotCount, Is.EqualTo(2));

                    void Place(LF2Entity entity, double y, int sourceZ)
                    {
                        entity.Runtime.SetPosition(
                            world.SpatialProjection.SourceToViewX(400), y,
                            world.SpatialProjection.SourceToViewZ(sourceZ));
                        entity.Runtime.SyncIntegerPosition();
                        entity.Runtime.SetSourceRulePosition(400, sourceZ);
                        entity.Runtime.SyncSourceRuleIntegerPosition();
                        entity.Runtime.SetVelocity(0, 0, 0);
                    }

                    Place(subject, -100, 600);
                    Place(target, 0, 604);
                    double initialViewZ = subject.Runtime.Z;
                    Assert.That(world.ObjectCount, Is.EqualTo(2));

                    Assert.That(driver.StepOneTick(
                        inputs[0], ignorePaused: true,
                        buildPresentation: false), Is.True);

                    Assert.That(world.FindEntityByRuntimeSlotForQuery(1), Is.SameAs(subject));
                    Assert.That(subject.ObjectId, Is.EqualTo(518));
                    Assert.That(subject.Runtime.Vz, Is.Zero);
                    Assert.That(subject.Runtime.SourceRuleZ,
                        Is.EqualTo(600).Within(1e-9));
                    Assert.That(subject.Runtime.Z - initialViewZ,
                        Is.Zero.Within(1e-9));
                    Assert.That(target.Runtime.SourceRuleZ, Is.EqualTo(604));
                    Assert.That(subject.ObjectAiTargetSlot3F8, Is.Zero);
                    Assert.That(world.ObjectCount, Is.EqualTo(2));
                },
                useProjectMode: true);
        }

        [Test]
        public void RealOid875SourceDepthDeadZoneSurvivesFullDriverTick()
        {
            NTSD28UnityRawCaptureEditor.WithLoganScenarioForReplayTests(
                ProjectPath(RuntimeRoot),
                ProjectPath(FullTickWitnessRoot + "/scenario.json"),
                BattleRuntimeProfile.Authority400, 3,
                (driver, inputs, identity) =>
                {
                    SimulationWorld world = driver.World;
                    world.ConfigureFixedViewRunDistance(2048, 1152);
                    LF2Entity target = world.FindEntityByRuntimeSlotForQuery(0);
                    LF2Entity subject = world.FindEntityByRuntimeSlotForQuery(1);
                    Assert.That(target?.ObjectId, Is.EqualTo(99));
                    Assert.That(subject?.ObjectId, Is.EqualTo(875));
                    Assert.That(subject.Frame.N, Is.EqualTo(55));
                    Assert.That(world.StageDepthBoundsArePhysical, Is.False);

                    void Place(LF2Entity entity, double y, int sourceZ)
                    {
                        entity.Runtime.SetPosition(
                            world.SpatialProjection.SourceToViewX(400), y,
                            world.SpatialProjection.SourceToViewZ(sourceZ));
                        entity.Runtime.SyncIntegerPosition();
                        entity.Runtime.SetSourceRulePosition(400, sourceZ);
                        entity.Runtime.SyncSourceRuleIntegerPosition();
                        entity.Runtime.SetVelocity(0, 0, 0);
                    }

                    Place(subject, -40, 600);
                    Place(target, 0, 604);
                    subject.ObjectAiTargetSlot3F8 = 0;
                    double initialViewZ = subject.Runtime.Z;

                    Assert.That(driver.StepOneTick(
                        inputs[0], ignorePaused: true,
                        buildPresentation: false), Is.True);

                    subject = world.FindEntityByRuntimeSlotForQuery(1);
                    target = world.FindEntityByRuntimeSlotForQuery(0);
                    Assert.That(subject?.ObjectId, Is.EqualTo(875));
                    Assert.That(target?.ObjectId, Is.EqualTo(99));
                    Assert.That(subject.Runtime.SourceRuleZ,
                        Is.EqualTo(600).Within(1e-9));
                    Assert.That(subject.Runtime.Vz, Is.Zero);
                    Assert.That(subject.Runtime.Z - initialViewZ,
                        Is.Zero.Within(1e-9));
                    Assert.That(target.Runtime.SourceRuleZ, Is.EqualTo(604));
                    Assert.That(subject.ObjectAiTargetSlot3F8, Is.Zero);
                    Assert.That(world.ObjectCount, Is.EqualTo(2));
                    TestContext.Progress.WriteLine(
                        "HITFA7_FULL_DRIVER_ONE_TICK: sourceGap=4, " +
                        "sourceZ=600, velocityZ=0, viewDeltaZ=0, targetSlot=0");
                },
                useProjectMode: true);
        }

        [Test]
        public void RealOid875PreassignedEmptySlotUsesRawTargetPosition()
        {
            LoganObjectCatalog catalog = LoganObjectCatalog.Read(
                BattleContentSource.ForLoganRuntime(ProjectPath(RuntimeRoot)));
            var configs = CharacterAnimtorManager.BuildCharacterFrameConfigsFromCatalog(catalog);
            Assert.That(configs[875].characterData.frames.Any(frame =>
                frame.frameId == 55 && frame.hit_Fa == 7), Is.True);

            var world = new SimulationWorld();
            var subject = new LF2SpecialAttack { ObjectId = 875 };
            subject.FrameCache.Load(configs[875]);
            subject.ImmediateFrame(55);
            subject.SetRequiredRuntimeSlot(50);
            subject.Team = 1;
            subject.Health.HP = 100;
            subject.Runtime.SetPosition(100, -30, 0);
            subject.Runtime.SyncIntegerPosition();
            subject.ObjectAiTargetSlot3F8 = 10;
            world.Register(subject);

            Assert.That(world.FindEntityByRuntimeSlotIncludingPending(10), Is.Null);
            Assert.That(world.GetRawRuntimeSlotState(10).XInt, Is.Zero);
            Assert.That(world.ObjectCount, Is.EqualTo(1));
            subject.RunFrameLogicBeforeAdvance();

            Assert.That(subject.ObjectAiTargetSlot3F8, Is.EqualTo(10));
            Assert.That(subject.Runtime.Vx, Is.EqualTo(-1.4).Within(1e-9));
            Assert.That(world.ObjectCount, Is.EqualTo(1));
        }

        [Test]
        public void RealOid875PreassignedTargetMatchesSourceModelFullTicks()
        {
            string sourcePath = ProjectPath(
                FullTickWitnessRoot + "/source-stage23-witness.jsonl");
            string output = ProjectPath(
                "Temp/NTSD28UnityTrace/FocusedTests/q07-hitfa7-target-fulltick.unity.jsonl");
            var sourceRows = File.ReadAllLines(sourcePath)
                .Select(JObject.Parse).ToArray();
            Assert.That(sourceRows.Length, Is.EqualTo(4));

            var unityRows = new List<string>();
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            try
            {
                NTSD28UnityRawCaptureEditor.WithLoganScenarioForReplayTests(
                ProjectPath(RuntimeRoot),
                ProjectPath(FullTickWitnessRoot + "/scenario.json"),
                BattleRuntimeProfile.Authority400,
                3,
                (driver, inputs, identity) =>
                {
                    SimulationWorld world = driver.World;
                    var target = world.FindEntityByRuntimeSlotForQuery(0);
                    var subject = world.FindEntityByRuntimeSlotForQuery(1);
                    Assert.That(target?.ObjectId, Is.EqualTo(99));
                    Assert.That(subject?.ObjectId, Is.EqualTo(875));
                    Assert.That(subject?.Frame.N, Is.EqualTo(55));
                    subject.ObjectAiTargetSlot3F8 = 0;
                    subject.Runtime.SetVelocity(0.0, 3.8, 0.0);
                    unityRows.Add(CaptureFullTickRow(world, subject, target, 0));
                    for (int tick = 1; tick <= 2; tick++)
                    {
                        Assert.That(driver.StepOneTick(
                            inputs[tick - 1], ignorePaused: true,
                            buildPresentation: false), Is.True);
                        target = world.FindEntityByRuntimeSlotForQuery(0);
                        subject = world.FindEntityByRuntimeSlotForQuery(1);
                        Assert.That(subject, Is.Not.Null);
                        unityRows.Add(CaptureFullTickRow(
                            world, subject, target, tick));
                    }
                });
            }
            catch (Exception exception)
            {
                File.WriteAllLines(output, unityRows);
                File.WriteAllText(output + ".failure.txt", exception.ToString());
                throw;
            }

            File.WriteAllLines(output, unityRows);
            Assert.That(unityRows.Count, Is.EqualTo(3));
            for (int tick = 0; tick < unityRows.Count; tick++)
            {
                JObject unity = JObject.Parse(unityRows[tick]);
                foreach (string field in new[]
                {
                    "completedTick", "targetOid", "subjectOid", "targetSlot",
                    "action", "integerY", "previousY", "entityCount"
                })
                {
                    Assert.That(unity[field].Value<int>(),
                        Is.EqualTo(sourceRows[tick][field].Value<int>()),
                        $"tick {tick}, field {field}");
                }
                foreach (string field in new[]
                {
                    "motionX", "motionY", "motionZ", "preciseY"
                })
                {
                    Assert.That(unity[field].Value<double>(),
                        Is.EqualTo(sourceRows[tick][field].Value<double>())
                            .Within(1e-9),
                        $"tick {tick}, field {field}");
                }
            }
        }

        [Test]
        public void RealOid875PreassignedTargetTick3MaterializesOpoint()
        {
            string output = ProjectPath(
                "Temp/NTSD28UnityTrace/FocusedTests/q07-hitfa7-tick3.unity.jsonl");
            string sourcePath = ProjectPath(
                FullTickWitnessRoot + "/source-stage23-witness.jsonl");
            var sourceRows = File.ReadAllLines(sourcePath)
                .Select(JObject.Parse).ToArray();
            Assert.That(sourceRows.Length, Is.EqualTo(4));

            var unityRows = new List<string>();
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            try
            {
                NTSD28UnityRawCaptureEditor.WithLoganScenarioForReplayTests(
                    ProjectPath(RuntimeRoot),
                    ProjectPath(FullTickWitnessRoot + "/scenario.json"),
                    BattleRuntimeProfile.Authority400,
                    3,
                    (driver, inputs, identity) =>
                    {
                        SimulationWorld world = driver.World;
                        var target = world.FindEntityByRuntimeSlotForQuery(0);
                        var subject = world.FindEntityByRuntimeSlotForQuery(1);
                        Assert.That(target?.ObjectId, Is.EqualTo(99));
                        Assert.That(subject?.ObjectId, Is.EqualTo(875));
                        Assert.That(subject?.Frame.N, Is.EqualTo(55));
                        LF2ObjectPool pool = LF2ObjectPool.Instance;
                        if (!pool.IsRuntimeStateValidForAcceptance)
                        {
                            typeof(LF2ObjectPool).GetMethod(
                                "Awake",
                                System.Reflection.BindingFlags.Instance |
                                System.Reflection.BindingFlags.NonPublic)
                                ?.Invoke(pool, null);
                        }
                        Assert.That(pool.IsRuntimeStateValidForAcceptance,
                            Is.True);
                        driver.BeginBattleAllocationSeal();
                        world.SetLogicOnlyEntityMaterialization(true);
                        Assert.That(world.UsesLogicOnlyEntityMaterialization, Is.True);
                        subject.ObjectAiTargetSlot3F8 = 0;
                        subject.Runtime.SetVelocity(0.0, 3.8, 0.0);
                        unityRows.Add(CaptureFullTickRow(world, subject, target, 0));

                        for (int tick = 1; tick <= 3; tick++)
                        {
                            File.WriteAllText(output + ".preconditions.json",
                                new JObject
                                {
                                    ["nextTick"] = tick,
                                    ["worldLogicOnly"] =
                                        world.UsesLogicOnlyEntityMaterialization,
                                    ["subjectRegisteredToWorld"] =
                                        ReferenceEquals(subject.Match, world),
                                    ["subjectAtSlot1"] = ReferenceEquals(
                                        world.FindEntityByRuntimeSlotForQuery(1),
                                        subject),
                                    ["subjectObjectId"] = subject.ObjectId,
                                    ["entityCount"] = world.ObjectCount,
                                }.ToString(Newtonsoft.Json.Formatting.Indented));
                            Assert.That(driver.StepOneTick(
                                inputs[tick - 1], ignorePaused: true,
                                buildPresentation: false), Is.True);
                            target = world.FindEntityByRuntimeSlotForQuery(0);
                            subject = world.FindEntityByRuntimeSlotForQuery(1);
                            Assert.That(subject, Is.Not.Null);
                            unityRows.Add(CaptureFullTickRow(
                                world, subject, target, tick));
                        }
                        BattleRuntimeShutdownReport shutdown =
                            driver.ShutdownBattleRuntime();
                        File.WriteAllText(output + ".shutdown.json",
                            new JObject
                            {
                                ["status"] = shutdown.Status.ToString(),
                                ["completedStage"] =
                                    shutdown.CompletedStage.ToString(),
                                ["failureReason"] = shutdown.FailureReason,
                                ["remainingWorldObjects"] =
                                    shutdown.RemainingWorldObjects,
                                ["remainingRuntimeSlots"] =
                                    shutdown.RemainingRuntimeSlots,
                                ["remainingPoolBorrowers"] =
                                    shutdown.RemainingPoolBorrowers,
                            }.ToString(Newtonsoft.Json.Formatting.Indented));
                    });
            }
            catch (Exception exception)
            {
                File.WriteAllLines(output, unityRows);
                File.WriteAllText(output + ".failure.txt", exception.ToString());
                throw;
            }

            File.WriteAllLines(output, unityRows);
            Assert.That(unityRows.Count, Is.EqualTo(sourceRows.Length));
            JObject unityTick3 = JObject.Parse(unityRows[3]);
            JObject sourceTick3 = sourceRows[3];
            foreach (string field in new[]
            {
                "completedTick", "targetOid", "subjectOid", "targetSlot",
                "action", "integerY", "previousY", "entityCount"
            })
            {
                Assert.That(unityTick3[field].Value<int>(),
                    Is.EqualTo(sourceTick3[field].Value<int>()),
                    $"tick 3, field {field}");
            }
            foreach (string field in new[]
            {
                "motionX", "motionY", "motionZ", "preciseY"
            })
            {
                Assert.That(unityTick3[field].Value<double>(),
                    Is.EqualTo(sourceTick3[field].Value<double>())
                        .Within(1e-9),
                    $"tick 3, field {field}");
            }
        }

        private static string CaptureFullTickRow(
            SimulationWorld world, LF2Entity subject, LF2Entity target,
            int completedTick)
        {
            return new JObject
            {
                ["completedTick"] = completedTick,
                ["targetOid"] = target?.ObjectId ?? -1,
                ["subjectOid"] = subject.ObjectId,
                ["targetSlot"] = subject.ObjectAiTargetSlot3F8,
                ["action"] = subject.Frame.N,
                ["motionX"] = subject.Runtime.Vx,
                ["motionY"] = subject.Runtime.Vy,
                ["motionZ"] = subject.Runtime.Vz,
                ["integerY"] = subject.Runtime.YInt,
                ["preciseY"] = subject.Runtime.Y,
                ["previousY"] = subject.Runtime.NativePreviousY104,
                ["entityCount"] = world.ObjectCount,
            }.ToString(Newtonsoft.Json.Formatting.None);
        }

        private static string ProjectPath(string path)
        {
            return Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, path));
        }

        [Serializable]
        private sealed class TickEnvelope
        {
            public int completedTick;
            public EntityEnvelope[] entities;
        }

        [Serializable]
        private sealed class EntityEnvelope
        {
            public int slot;
            public IdentityEnvelope identity;
            public FrameEnvelope frame;
        }

        [Serializable]
        private sealed class IdentityEnvelope
        {
            public int objectId;
        }

        [Serializable]
        private sealed class FrameEnvelope
        {
            public int action;
        }
    }
}
#endif
