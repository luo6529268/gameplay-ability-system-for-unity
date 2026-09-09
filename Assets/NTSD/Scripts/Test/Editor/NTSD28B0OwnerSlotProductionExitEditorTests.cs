#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace NTSD.Test.Editor
{
    [Category("NTSD28")]
    [Category("NTSD28_B0")]
    public sealed class NTSD28B0OwnerSlotProductionExitEditorTests
    {
        private const string CaptureSchema = "ntsd28-b0-owner-slot-exit-v1";
        private const string FormalAuthorityExeSha256 =
            "B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033";
        private const string ScenarioId = "b0-owner-slot-production-exit";
        private const string InputContract = "neutral-zero;f8-marker-8";
        private const string ObservationUnit = "completed-owner-transaction";
        private const int SharedSeed = 0x1234;
        private const string AuthorityRelativePath =
            "Temp/NTSD28-B0-OwnerSlotExit/authority-owner-slot.json";
        private const string UnityRelativePath =
            "Temp/NTSD28-B0-OwnerSlotExit/unity-owner-slot.json";
        private const string ComparisonRelativePath =
            "Temp/NTSD28-B0-OwnerSlotExit/comparison.json";

        [Test]
        public void OwnerProductionTrace_MatchesAuthorityAcrossAllRoute5Cases()
        {
            string authorityPath = ProjectPath(AuthorityRelativePath);
            Assert.That(File.Exists(authorityPath), Is.True,
                $"Authority owner trace is missing: {authorityPath}");

            OwnerTraceEnvelope authority = JsonUtility.FromJson<OwnerTraceEnvelope>(
                File.ReadAllText(authorityPath, Encoding.UTF8));
            ValidateAuthorityHeader(authority);

            OwnerTraceEnvelope unity = CaptureUnityTrace();
            string unityPath = ProjectPath(UnityRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(unityPath));
            File.WriteAllText(
                unityPath,
                JsonUtility.ToJson(unity, true),
                new UTF8Encoding(false));

            OwnerTraceComparison comparison = Compare(authority, unity);
            string comparisonPath = ProjectPath(ComparisonRelativePath);
            File.WriteAllText(
                comparisonPath,
                JsonUtility.ToJson(comparison, true),
                new UTF8Encoding(false));

            Assert.That(comparison.status, Is.EqualTo("equal-owner-trace"),
                comparison.firstDifference);
            Assert.That(comparison.recordsCompared, Is.EqualTo(15));
            Assert.That(comparison.fieldOccurrencesCompared, Is.EqualTo(135));
        }

        private static OwnerTraceEnvelope CaptureUnityTrace()
        {
            var records = new List<OwnerTraceRecord>(15);
            CaptureDirectSelf(records);
            CaptureTwoHopOpoint(records);
            CaptureOwnerTargetDeconfliction(records);
            CaptureF8Owner(records);
            CaptureState9996(records);
            CaptureType3Mutation(records);
            CaptureSlotReuse(records);
            Assert.That(records.Count, Is.EqualTo(15));

            return new OwnerTraceEnvelope
            {
                schema = CaptureSchema,
                producer = "UNITY_PRODUCTION_OWNER_PATHS",
                evidenceClass = "UNITY_RUNTIME_DIAGNOSTIC_ONLY",
                formalAuthorityExeSha256 = FormalAuthorityExeSha256,
                scenarioId = ScenarioId,
                seed = SharedSeed,
                inputContract = InputContract,
                observationUnit = ObservationUnit,
                records = records.ToArray(),
            };
        }

        private static void CaptureDirectSelf(List<OwnerTraceRecord> records)
        {
            var world = new SimulationWorld();
            for (int slot = 0; slot <= 1; slot++)
            {
                var entity = new LF2Character();
                Assert.That(
                    BattleMatchConfigRuntimeAdapter
                        .PrepareDirectParticipantRegistration(entity, slot),
                    Is.True);
                world.Register(entity);
                AppendRecord(
                    records,
                    world,
                    "direct-self",
                    observationTick: 1,
                    inputMarker: 0,
                    entity,
                    generationOrdinal: 1,
                    sourceSlot: slot,
                    sourceOwnerSlot: slot);
            }
        }

        private static void CaptureTwoHopOpoint(List<OwnerTraceRecord> records)
        {
            const int rootSlot = 50;
            const int rootOwner = 7;
            const int firstOid = 9104;
            const int secondOid = 9105;
            LF2FrameData firstChildFrame = FrameWithOpoint(secondOid);
            SimulationWorld world = CreateOpointWorld(
                Child(firstOid, LF2ObjectType.Character, firstChildFrame),
                Child(secondOid, LF2ObjectType.SpecialAttack, FrameWithoutOpoint()));
            ProbeOther root = new ProbeOther(
                9000,
                LF2ObjectType.Other,
                FrameWithOpoint(firstOid));
            root.OwnerEntityIndex = rootOwner;
            root.SetRequiredRuntimeSlot(rootSlot);
            world.Register(root);

            world.LateEntityUpdateAll(1);
            LF2Entity first = world.FindEntityByRuntimeSlotForQuery(51);
            Assert.That(first?.ObjectId, Is.EqualTo(firstOid));
            first.AttackingCounter = 0;
            first.FrameDelay = 0;
            first.Frame.D = firstChildFrame;
            first.Frame.N = 0;
            first.Runtime.Frame = 0;
            world.StructuralWriter.ProcessLateOpointSegment(
                world.LogicObjectPointRuntime,
                first,
                2);
            LF2Entity second = world.FindEntityByRuntimeSlotForQuery(52);
            Assert.That(second?.ObjectId, Is.EqualTo(secondOid));

            AppendRecord(records, world, "opoint-root", 1, 0, root, 1,
                rootSlot, rootOwner);
            AppendRecord(records, world, "opoint-child-1", 1, 0, first, 1,
                rootSlot, rootOwner);
            AppendRecord(records, world, "opoint-child-2", 2, 0, second, 1,
                51, rootOwner);
        }

        private static void CaptureF8Owner(List<OwnerTraceRecord> records)
        {
            const int weaponOid = 150;
            LF2CharacterDataWrapper wrapper = new LF2CharacterDataWrapper(
                weaponOid,
                new LF2CharacterData
                {
                    name = "OwnerExitF8Weapon",
                    type_sub = (int)LF2ObjectType.LightWeapon,
                    frames = new List<LF2FrameData>
                    {
                        Frame(0, LF2States.WeaponOnGround),
                        Frame(3, LF2States.WeaponInSky),
                    },
                });
            var resolver = new RuntimeCharacterConfigResolver(
                oid => oid == weaponOid ? wrapper : null);
            var world = new SimulationWorld(resolver);
            world.SetLogicOnlyEntityMaterialization(true);
            world.PrepareRuntimeDataCatalogForBattle(
                new[]
                {
                    new ObjectDefinition(
                        weaponOid,
                        (int)LF2ObjectType.LightWeapon,
                        "owner-exit-f8.dat"),
                },
                oid => oid == weaponOid ? wrapper : null);
            world.Runtime.Stage.SetSceneSnapshot(800, 180, 350, 0, 0);
            world.Rng.Seed(SharedSeed);
            world.SetMode2Request(1);
            world.Mode2RandomWeaponDropTailAll(1);

            LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(50);
            Assert.That(entity?.OwnerEntityIndex, Is.EqualTo(99));
            AppendRecord(records, world, "f8-first-spawn", 1, 8, entity, 1,
                -1, 99);
        }

        private static void CaptureOwnerTargetDeconfliction(
            List<OwnerTraceRecord> records)
        {
            var world = new SimulationWorld();
            LF2Character target = CreateCharacter(
                91000,
                new LF2CharacterData
                {
                    name = "OwnerExitTarget",
                    type_sub = (int)LF2ObjectType.Character,
                    frames = new List<LF2FrameData>
                    {
                        Frame(0, LF2States.Standing),
                    },
                });
            var source = new LF2OtherObject
            {
                ObjectId = 91001,
                Name = "OwnerExitTargetSource",
            };
            LF2FrameData sourceFrame = Frame(0, LF2States.ProjectileFlying);
            sourceFrame.hit_Fa = 3;
            source.FrameCache.Load(new LF2CharacterDataWrapper(
                source.ObjectId,
                new LF2CharacterData
                {
                    name = source.Name,
                    type_sub = (int)LF2ObjectType.Other,
                    frames = new List<LF2FrameData> { sourceFrame },
                }));
            source.ImmediateFrame(0);
            source.Health.HP = 500;
            source.Health.HPBound = 500;
            source.Health.HP3 = 500;
            target.SetRequiredRuntimeSlot(0);
            source.SetRequiredRuntimeSlot(50);
            target.Team = target.RelationTeam = 2;
            source.Team = source.RelationTeam = 1;
            source.OwnerEntityIndex = 7;
            target.Runtime.SetPosition(100.0, 0.0, 20.0);
            source.Runtime.SetPosition(0.0, 0.0, 0.0);
            target.Runtime.SyncIntegerPosition();
            source.Runtime.SyncIntegerPosition();
            world.Register(target);
            world.Register(source);

            source.RunFrameLogicBeforeAdvance();

            Assert.That(source.OwnerEntityIndex, Is.EqualTo(7));
            Assert.That(source.ObjectAiTargetSlot3F8, Is.EqualTo(0));
            AppendRecord(
                records,
                world,
                "owner-target-independent",
                1,
                0,
                source,
                1,
                50,
                7);
        }

        private static void CaptureState9996(List<OwnerTraceRecord> records)
        {
            const int sourceOid = 782;
            LF2CharacterData sourceData = new LF2CharacterData
            {
                name = "OwnerExitCloneSource",
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData>
                {
                    Frame(0, 9996),
                },
            };
            var wrappers = new Dictionary<int, LF2CharacterDataWrapper>
            {
                [sourceOid] = new LF2CharacterDataWrapper(sourceOid, sourceData),
            };
            var definitions = new List<ObjectDefinition>
            {
                new ObjectDefinition(
                    sourceOid,
                    (int)LF2ObjectType.Character,
                    "owner-exit-clone-source.dat"),
            };
            for (int oid = 217; oid <= 218; oid++)
            {
                var frames = new List<LF2FrameData>();
                for (int frameId = 0; frameId < 4; frameId++)
                    frames.Add(Frame(frameId, LF2States.WeaponInSky));
                var data = new LF2CharacterData
                {
                    name = $"OwnerExitClone{oid}",
                    type_sub = (int)LF2ObjectType.LightWeapon,
                    weapon_hp = 700 + oid,
                    frames = frames,
                };
                wrappers[oid] = new LF2CharacterDataWrapper(oid, data);
                definitions.Add(new ObjectDefinition(
                    oid,
                    (int)LF2ObjectType.LightWeapon,
                    "owner-exit-clone.dat"));
            }

            var resolver = new RuntimeCharacterConfigResolver(
                oid => wrappers.TryGetValue(oid, out LF2CharacterDataWrapper value)
                    ? value
                    : null);
            var world = new SimulationWorld(resolver);
            world.PrepareRuntimeDataCatalogForBattle(
                definitions,
                oid => wrappers.TryGetValue(oid, out LF2CharacterDataWrapper value)
                    ? value
                    : null);
            world.LogicReferencePool.Prewarm(LF2ObjectType.LightWeapon, 8);
            world.LogicReferencePool.PrewarmTasks<OPointCreateTask>(2);
            world.SetLogicOnlyEntityMaterialization(true);
            world.NativeRandom.ResetFromSeed(SharedSeed);

            LF2Character source = CreateCharacter(sourceOid, sourceData);
            source.SetRequiredRuntimeSlot(0);
            source.Runtime.SetPosition(100.75, -20.25, 200.5);
            source.Runtime.SyncIntegerPosition();
            source.AttackingCounter = 1;
            world.Register(source);
            world.LateEntityUpdateAll(1);

            for (int slot = 50; slot <= 54; slot++)
            {
                LF2Entity clone = world.FindEntityByRuntimeSlotIncludingPending(slot);
                Assert.That(clone, Is.Not.Null);
                Assert.That(clone.OwnerEntityIndex, Is.EqualTo(-1));
                AppendRecord(records, world, "state9996-clone", 1, 0, clone, 1,
                    0, -1);
            }
        }

        private static void CaptureType3Mutation(List<OwnerTraceRecord> records)
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateTypedCharacter(
                world, 9400, 50, LF2ObjectType.Character);
            TypedCharacter target = CreateTypedCharacter(
                world, 9401, 51, LF2ObjectType.SpecialAttack);
            attacker.RelationTeam = 4;
            attacker.Runtime.OwnerSlotIndex = 7;
            target.Runtime.OwnerSlotIndex = 44;

            bool applied = BattleDamageWriter.ApplyNativeType3TargetGenericContinuation(
                world,
                attacker,
                target,
                new InteractionArea { kind = 0, effect = 0 });
            Assert.That(applied, Is.True);
            Assert.That(target.Runtime.OwnerSlotIndex, Is.EqualTo(7));
            AppendRecord(records, world, "type3-owner-mutation", 1, 0, target, 1,
                50, 7);
        }

        private static void CaptureSlotReuse(List<OwnerTraceRecord> records)
        {
            var world = new SimulationWorld();
            var first = new LF2Character();
            first.SetRequiredRuntimeSlot(50);
            first.OwnerEntityIndex = 7;
            world.Register(first);
            uint firstGeneration = world.RuntimeSlotTableForModules
                .GetReadOnlyView(50).Generation;
            AppendRecord(records, world, "slot-reuse-before", 1, 0, first, 1,
                -1, 7);

            world.Unregister(first);
            var second = new LF2Character();
            second.SetRequiredRuntimeSlot(50);
            second.OwnerEntityIndex = 13;
            world.Register(second);
            uint secondGeneration = world.RuntimeSlotTableForModules
                .GetReadOnlyView(50).Generation;
            Assert.That(secondGeneration, Is.Not.EqualTo(firstGeneration));
            AppendRecord(records, world, "slot-reuse-after", 2, 0, second, 2,
                -1, 13);
        }

        private static void AppendRecord(
            List<OwnerTraceRecord> records,
            SimulationWorld world,
            string caseId,
            int observationTick,
            int inputMarker,
            LF2Entity entity,
            int generationOrdinal,
            int sourceSlot,
            int sourceOwnerSlot)
        {
            Assert.That(entity, Is.Not.Null);
            int slot = entity.Runtime.SlotIndex;
            RuntimeSlotTable.ReadOnlySlotView view =
                world.RuntimeSlotTableForModules.GetReadOnlyView(slot);
            Assert.That(view.Claimed, Is.True);
            Assert.That(view.Entity, Is.SameAs(entity));
            Assert.That(view.Generation, Is.Not.Zero);
            Assert.That(view.Entity.Runtime.OwnerSlotIndex,
                Is.EqualTo(entity.OwnerEntityIndex));
            Assert.That(view.RawRuntime, Is.Not.Null);
            Assert.That(view.RawRuntime.OwnerSlotIndex, Is.EqualTo(-1));
            records.Add(new OwnerTraceRecord
            {
                caseId = caseId,
                observationTick = observationTick,
                inputMarker = inputMarker,
                slot = slot,
                generationOrdinal = generationOrdinal,
                ownerSlot = entity.Runtime.OwnerSlotIndex,
                sourceSlot = sourceSlot,
                sourceOwnerSlot = sourceOwnerSlot,
                targetSlot3F8 = entity.Runtime.ObjectAiTargetSlot3F8,
            });
        }

        private static SimulationWorld CreateOpointWorld(
            params ChildDefinition[] children)
        {
            var wrappers = new Dictionary<int, LF2CharacterDataWrapper>();
            var definitions = new List<ObjectDefinition>();
            foreach (ChildDefinition child in children)
            {
                var data = new LF2CharacterData
                {
                    name = $"OwnerExitOpointChild{child.Oid}",
                    type_sub = (int)child.Type,
                    frames = new List<LF2FrameData> { child.Frame },
                };
                wrappers[child.Oid] = new LF2CharacterDataWrapper(child.Oid, data);
                definitions.Add(new ObjectDefinition(
                    child.Oid,
                    (int)child.Type,
                    "owner-exit-opoint.dat"));
            }
            var resolver = new RuntimeCharacterConfigResolver(
                oid => wrappers.TryGetValue(oid, out LF2CharacterDataWrapper value)
                    ? value
                    : null);
            var world = new SimulationWorld(resolver);
            world.SetLogicOnlyEntityMaterialization(true);
            world.PrepareRuntimeDataCatalogForBattle(
                definitions,
                oid => wrappers.TryGetValue(oid, out LF2CharacterDataWrapper value)
                    ? value
                    : null);
            return world;
        }

        private static ChildDefinition Child(
            int oid,
            LF2ObjectType type,
            LF2FrameData frame)
        {
            return new ChildDefinition(oid, type, frame);
        }

        private static LF2FrameData FrameWithOpoint(int childOid)
        {
            LF2FrameData frame = FrameWithoutOpoint();
            frame.opoint = new ObjectPoint
            {
                oid = childOid,
                kind = 1,
                action = 0,
                x = 39,
                y = 79,
                facing = 0,
            };
            return frame;
        }

        private static LF2FrameData FrameWithoutOpoint()
        {
            return Frame(0, LF2States.Standing);
        }

        private static LF2FrameData Frame(int id, int state)
        {
            return new LF2FrameData
            {
                frameId = id,
                state = state,
                wait = 10000,
                next = id,
                pic = 999,
                centerx = 39,
                centery = 79,
            };
        }

        private static LF2Character CreateCharacter(
            int objectId,
            LF2CharacterData data)
        {
            var entity = new LF2Character
            {
                ObjectId = objectId,
                Name = data.name,
            };
            entity.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            entity.ImmediateFrame(0);
            entity.Health.HP = 500;
            entity.Health.HPBound = 500;
            entity.Health.HP3 = 500;
            entity.Health.PP = 500;
            return entity;
        }

        private static TypedCharacter CreateTypedCharacter(
            SimulationWorld world,
            int objectId,
            int slot,
            LF2ObjectType objectType)
        {
            var frames = new List<LF2FrameData>();
            for (int id = 0; id <= 40; id++)
                frames.Add(Frame(id, LF2States.Standing));
            var data = new LF2CharacterData
            {
                name = "OwnerExitType3",
                type_sub = (int)objectType,
                frames = frames,
            };
            var entity = new TypedCharacter(objectType)
            {
                ObjectId = objectId,
                Name = data.name,
            };
            entity.SetRequiredRuntimeSlot(slot);
            entity.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            entity.ImmediateFrame(0);
            entity.Health.HP = 500;
            entity.Health.HPBound = 500;
            world.Register(entity);
            return entity;
        }

        private static void ValidateAuthorityHeader(OwnerTraceEnvelope authority)
        {
            Assert.That(authority, Is.Not.Null);
            Assert.That(authority.schema, Is.EqualTo(CaptureSchema));
            Assert.That(authority.producer, Is.EqualTo("NTSD28_PLAYABLE_SOURCE_MODEL"));
            Assert.That(authority.evidenceClass,
                Is.EqualTo("SOURCE_MODEL_DIAGNOSTIC_ONLY"));
            Assert.That(authority.formalAuthorityExeSha256,
                Is.EqualTo(FormalAuthorityExeSha256));
            Assert.That(authority.authoritySourceManifestSha256,
                Does.Match("^[0-9A-F]{64}$"));
            Assert.That(authority.captureRunnerSourceSha256,
                Does.Match("^[0-9A-F]{64}$"));
            Assert.That(authority.captureBinarySha256,
                Does.Match("^[0-9A-F]{64}$"));
            Assert.That(authority.scenarioId, Is.EqualTo(ScenarioId));
            Assert.That(authority.seed, Is.EqualTo(SharedSeed));
            Assert.That(authority.inputContract, Is.EqualTo(InputContract));
            Assert.That(authority.observationUnit, Is.EqualTo(ObservationUnit));
            Assert.That(authority.records, Has.Length.EqualTo(15));
        }

        private static OwnerTraceComparison Compare(
            OwnerTraceEnvelope authority,
            OwnerTraceEnvelope unity)
        {
            var result = new OwnerTraceComparison
            {
                schema = "ntsd28-b0-owner-slot-exit-comparison-v1",
                status = "equal-owner-trace",
                recordsCompared = 0,
                fieldOccurrencesCompared = 0,
                firstDifference = string.Empty,
            };
            if (unity.records == null || authority.records.Length != unity.records.Length)
            {
                result.status = "different";
                result.firstDifference = "record-count";
                return result;
            }

            for (int index = 0; index < authority.records.Length; index++)
            {
                OwnerTraceRecord expected = authority.records[index];
                OwnerTraceRecord actual = unity.records[index];
                string difference = FirstDifference(expected, actual);
                if (!string.IsNullOrEmpty(difference))
                {
                    result.status = "different";
                    result.firstDifference = $"record[{index}].{difference}";
                    return result;
                }
                result.recordsCompared++;
                result.fieldOccurrencesCompared += 9;
            }
            return result;
        }

        private static string FirstDifference(
            OwnerTraceRecord expected,
            OwnerTraceRecord actual)
        {
            if (expected.caseId != actual.caseId) return "caseId";
            if (expected.observationTick != actual.observationTick)
                return "observationTick";
            if (expected.inputMarker != actual.inputMarker) return "inputMarker";
            if (expected.slot != actual.slot) return "slot";
            if (expected.generationOrdinal != actual.generationOrdinal)
                return "generationOrdinal";
            if (expected.ownerSlot != actual.ownerSlot) return "ownerSlot";
            if (expected.sourceSlot != actual.sourceSlot) return "sourceSlot";
            if (expected.sourceOwnerSlot != actual.sourceOwnerSlot)
                return "sourceOwnerSlot";
            if (expected.targetSlot3F8 != actual.targetSlot3F8)
                return "targetSlot3F8";
            return string.Empty;
        }

        private static string ProjectPath(string relativePath)
        {
            string root = Directory.GetParent(Application.dataPath)?.FullName;
            return Path.GetFullPath(Path.Combine(root ?? string.Empty, relativePath));
        }

        [Serializable]
        private sealed class OwnerTraceEnvelope
        {
            public string schema = string.Empty;
            public string producer = string.Empty;
            public string evidenceClass = string.Empty;
            public string formalAuthorityExeSha256 = string.Empty;
            public string authoritySourceManifestSha256 = string.Empty;
            public string captureRunnerSourceSha256 = string.Empty;
            public string captureBinarySha256 = string.Empty;
            public string scenarioId = string.Empty;
            public int seed;
            public string inputContract = string.Empty;
            public string observationUnit = string.Empty;
            public OwnerTraceRecord[] records = Array.Empty<OwnerTraceRecord>();
        }

        [Serializable]
        private sealed class OwnerTraceRecord
        {
            public string caseId = string.Empty;
            public int observationTick;
            public int inputMarker;
            public int slot;
            public int generationOrdinal;
            public int ownerSlot;
            public int sourceSlot;
            public int sourceOwnerSlot;
            public int targetSlot3F8;
        }

        [Serializable]
        private sealed class OwnerTraceComparison
        {
            public string schema = string.Empty;
            public string status = string.Empty;
            public int recordsCompared;
            public int fieldOccurrencesCompared;
            public string firstDifference = string.Empty;
        }

        private sealed class ChildDefinition
        {
            public ChildDefinition(
                int oid,
                LF2ObjectType type,
                LF2FrameData frame)
            {
                Oid = oid;
                Type = type;
                Frame = frame;
            }

            public int Oid { get; }
            public LF2ObjectType Type { get; }
            public LF2FrameData Frame { get; }
        }

        private sealed class ProbeOther : LF2Entity
        {
            private readonly LF2ObjectType dataType;

            public ProbeOther(
                int objectId,
                LF2ObjectType dataType,
                LF2FrameData frame)
            {
                this.dataType = dataType;
                ObjectId = objectId;
                Name = "OwnerExitOpointRoot";
                Health = new LF2Health();
                Health.BindRuntime(Runtime);
                Health.HP = 500;
                Health.HPBound = 500;
                Health.HP3 = 500;
                Health.PP = 500;
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
                Trans = new FrameTransistor(this);
                FrameCache.Load(new LF2CharacterDataWrapper(
                    objectId,
                    new LF2CharacterData
                    {
                        name = Name,
                        type_sub = (int)dataType,
                        frames = new List<LF2FrameData> { frame },
                    }));
                Frame.D = frame;
                Frame.N = 0;
                Frame.PN = 0;
                Frame.Prev = 0;
                Frame.Prev2 = 0;
                Runtime.Frame = 0;
                Runtime.PrevFrame2 = 0;
                Runtime.WeaponFlightCounter = 0;
                Runtime.SetPosition(100, 0, 250);
                Runtime.SyncIntegerPosition();
                PS.dir = "right";
                AttackingCounter = 0;
                FrameDelay = 0;
                HitStun = 0;
            }

            public override LF2ObjectType ObjectTypeEnum => dataType;

            internal override bool UsesDynamicRuntimeSlot() => true;

            internal override void RunLateTailBeforePrevFrame()
            {
                AttackingCounter = 1;
            }

            public override void Reset()
            {
            }

            public override void Init(
                LF2TaskBase task,
                LF2ObjectRenderer renderer)
            {
            }
        }

        private sealed class TypedCharacter : LF2Character
        {
            private readonly LF2ObjectType objectType;

            public TypedCharacter(LF2ObjectType objectType)
            {
                this.objectType = objectType;
            }

            public override int GetCurrentDataObjectTypeForSimulation()
            {
                return (int)objectType;
            }
        }
    }

    [InitializeOnLoad]
    internal sealed class NTSD28B0OwnerSlotProductionExitRequestRunner : ICallbacks
    {
        private const string RequestRelativePath =
            "Temp/NTSD28-B0-OwnerSlotProductionExit-v1.request";
        private const string ResultRelativePath =
            "Temp/NTSD28-B0-OwnerSlotProductionExit-v1.result";
        private const string FocusedTestClass =
            "NTSD.Test.Editor.NTSD28B0OwnerSlotProductionExitEditorTests";

        private static readonly string RequestPath = ProjectPath(RequestRelativePath);
        private static readonly string ResultPath = ProjectPath(ResultRelativePath);
        private static readonly StringBuilder FailureDetails = new StringBuilder(2048);
        private static NTSD28B0OwnerSlotProductionExitRequestRunner activeCallbacks;
        private static TestRunnerApi activeApi;

        static NTSD28B0OwnerSlotProductionExitRequestRunner()
        {
            EditorApplication.update += PollRequest;
        }

        private static void PollRequest()
        {
            if (activeCallbacks != null ||
                EditorApplication.isCompiling ||
                EditorApplication.isUpdating ||
                EditorApplication.isPlayingOrWillChangePlaymode ||
                !File.Exists(RequestPath))
            {
                return;
            }
            if (File.Exists(ResultPath))
                File.Delete(ResultPath);
            File.Delete(RequestPath);

            activeCallbacks = new NTSD28B0OwnerSlotProductionExitRequestRunner();
            activeApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            activeApi.RegisterCallbacks(activeCallbacks);
            activeApi.Execute(new ExecutionSettings(
                new Filter
                {
                    testMode = TestMode.EditMode,
                    testNames = new[] { FocusedTestClass },
                })
            {
                runSynchronously = false,
            });
        }

        public void RunStarted(ITestAdaptor testsToRun)
        {
            FailureDetails.Clear();
        }

        public void RunFinished(ITestResultAdaptor result)
        {
            string text =
                $"state={result.ResultState}\n" +
                $"passed={result.PassCount}\n" +
                $"failed={result.FailCount}\n" +
                $"skipped={result.SkipCount}\n" +
                $"inconclusive={result.InconclusiveCount}\n" +
                $"message={result.Message}\n" +
                FailureDetails;
            File.WriteAllText(ResultPath, text, new UTF8Encoding(false));

            activeApi.UnregisterCallbacks(this);
            UnityEngine.Object.DestroyImmediate(activeApi);
            activeApi = null;
            activeCallbacks = null;
        }

        public void TestStarted(ITestAdaptor test)
        {
        }

        public void TestFinished(ITestResultAdaptor result)
        {
            if (result?.Test == null || result.Test.IsSuite || result.FailCount <= 0)
                return;
            FailureDetails.Append("--- failure ---\n");
            FailureDetails.Append("test=").Append(result.FullName).Append('\n');
            FailureDetails.Append("state=").Append(result.ResultState).Append('\n');
            FailureDetails.Append("message=").Append(result.Message).Append('\n');
            FailureDetails.Append("stack=").Append(result.StackTrace).Append('\n');
        }

        private static string ProjectPath(string relativePath)
        {
            string root = Directory.GetParent(Application.dataPath)?.FullName;
            return Path.GetFullPath(Path.Combine(root ?? string.Empty, relativePath));
        }
    }

    [InitializeOnLoad]
    internal static class NTSD28B0OwnerSlotProductionExitPlayRunner
    {
        private const string RequestRelativePath =
            "Temp/NTSD28-B0-OwnerSlotProductionExit-Play-v1.request";
        private const string ResultRelativePath =
            "Temp/NTSD28-B0-OwnerSlotProductionExit-Play-v1.result";
        private static readonly string RequestPath = ProjectPath(RequestRelativePath);
        private static readonly string ResultPath = ProjectPath(ResultRelativePath);
        private static bool running;

        static NTSD28B0OwnerSlotProductionExitPlayRunner()
        {
            EditorApplication.update += PollRequest;
        }

        private static void PollRequest()
        {
            if (running || EditorApplication.isCompiling ||
                EditorApplication.isUpdating || !File.Exists(RequestPath))
            {
                return;
            }
            if (!EditorApplication.isPlaying)
            {
                if (!EditorApplication.isPlayingOrWillChangePlaymode)
                    EditorApplication.EnterPlaymode();
                return;
            }

            running = true;
            File.Delete(RequestPath);
            if (File.Exists(ResultPath))
                File.Delete(ResultPath);
            try
            {
                new NTSD28B0OwnerSlotProductionExitEditorTests()
                    .OwnerProductionTrace_MatchesAuthorityAcrossAllRoute5Cases();
                File.WriteAllText(
                    ResultPath,
                    "state=Passed\nrecords=15\nfields=135\nsceneMutation=none\n",
                    new UTF8Encoding(false));
            }
            catch (Exception exception)
            {
                File.WriteAllText(
                    ResultPath,
                    "state=Failed\n" + exception,
                    new UTF8Encoding(false));
            }
            finally
            {
                EditorApplication.delayCall += ExitPlayMode;
            }
        }

        private static void ExitPlayMode()
        {
            if (EditorApplication.isPlaying)
                EditorApplication.ExitPlaymode();
            running = false;
        }

        private static string ProjectPath(string relativePath)
        {
            string root = Directory.GetParent(Application.dataPath)?.FullName;
            return Path.GetFullPath(Path.Combine(root ?? string.Empty, relativePath));
        }
    }
}
#endif
