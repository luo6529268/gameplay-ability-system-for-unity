#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class LateOpointRuntimeSlotCapacityEditorTests
    {
        private const int SpawnOid = 31999;

        [Test]
        public void LateOpoint_PreflightsDynamicSlotsBeforePoolConstruction_AndUsesLowestFreeSlot()
        {
            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            LF2ObjectPool pool = LF2ObjectPool.Instance;
            Assert.That(factory, Is.Not.Null);
            Assert.That(pool, Is.Not.Null);
            Assert.That(LF2ReferencePool.Instance, Is.Not.Null);

            using var configs = new RuntimeObjectConfigScope(SpawnOid, BuildSpawnData());
            using var isolatedPool = new IsolatedObjectPoolScope(pool);
            using var driver = new SimulationDriverWorldScope();

            var fullWorld = new SimulationWorld();
            driver.SetWorld(fullWorld);
            var fullSpawner = new OpointSpawner(SpawnOid);
            fullWorld.Register(fullSpawner);
            for (int i = 0; i < 349; i++)
                fullWorld.Register(new DynamicSlotOccupant());

            Assert.That(fullWorld.ClaimedRuntimeSlotCountForDiagnostics, Is.EqualTo(350));
            Assert.That(pool.AvailableObjectCountForAcceptance, Is.Zero);
            Assert.That(pool.ActiveObjectCountForAcceptance, Is.Zero);

            factory.ProcessOpointSpawn(fullSpawner);

            Assert.That(pool.AvailableObjectCountForAcceptance, Is.Zero,
                "a full dynamic slot band must skip late opoint before LF2ObjectPool.Get can construct an object");
            Assert.That(pool.ActiveObjectCountForAcceptance, Is.Zero);
            Assert.That(fullWorld.ClaimedRuntimeSlotCountForDiagnostics, Is.EqualTo(350));

            var lowSlotWorld = new SimulationWorld();
            driver.SetWorld(lowSlotWorld);
            var releasedLowSlot = new DynamicSlotOccupant();
            var lowSlotSpawner = new OpointSpawner(SpawnOid);
            lowSlotWorld.Register(releasedLowSlot);
            lowSlotWorld.Register(lowSlotSpawner);
            Assert.That(releasedLowSlot.Runtime.SlotIndex, Is.EqualTo(50));
            lowSlotWorld.Unregister(releasedLowSlot);

            factory.ProcessOpointSpawn(lowSlotSpawner);

            LF2Entity spawned = lowSlotWorld.FindEntityByRuntimeSlotForQuery(50);
            Assert.That(spawned, Is.Not.Null);
            Assert.That(spawned.ObjectId, Is.EqualTo(SpawnOid));
            Assert.That(pool.ActiveObjectCountForAcceptance, Is.EqualTo(1));
            spawned.FreeEntityLikeExe();
            Assert.That(pool.ActiveObjectCountForAcceptance, Is.Zero);
        }

        private static LF2CharacterData BuildSpawnData()
        {
            return new LF2CharacterData
            {
                name = "LateOpointCapacitySpawn",
                type_sub = SpawnOid,
                frames = new List<LF2FrameData>
                {
                    new LF2FrameData
                    {
                        frameId = 0,
                        state = 9999,
                        wait = 100,
                        next = 0,
                    },
                },
            };
        }

        private class DynamicSlotOccupant : LF2OtherObject
        {
            public DynamicSlotOccupant()
            {
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
                Trans = new FrameTransistor(this);
            }
        }

        private sealed class OpointSpawner : DynamicSlotOccupant
        {
            public OpointSpawner(int spawnOid)
            {
                var frame = new LF2FrameData
                {
                    frameId = 0,
                    state = 0,
                    wait = 100,
                    next = 0,
                    centerx = 0,
                    centery = 0,
                    opoint = new ObjectPoint
                    {
                        kind = 1,
                        oid = spawnOid,
                        action = 0,
                        facing = 0,
                    },
                };
                FrameCache.Load(new LF2CharacterDataWrapper(1, new LF2CharacterData
                {
                    name = "LateOpointCapacitySpawner",
                    frames = new List<LF2FrameData> { frame },
                }));
                Frame.D = frame;
                Frame.N = 0;
                Frame.PN = 0;
                Runtime.Frame = 0;
                PS.dir = "right";
                Runtime.SetPosition(0, 0, 0);
                Runtime.SyncIntegerPosition();
            }
        }

        private sealed class RuntimeObjectConfigScope : IDisposable
        {
            private readonly GameDataManager dataManager;
            private readonly CharacterAnimtorManager animatorManager;
            private readonly FieldInfo objectLookupField;
            private readonly FieldInfo cachedConfigField;
            private readonly FieldInfo frameConfigField;
            private readonly object originalObjectLookup;
            private readonly object originalCachedConfig;
            private readonly object originalFrameConfigs;

            public RuntimeObjectConfigScope(int oid, LF2CharacterData data)
            {
                dataManager = GameDataManager.Instance;
                animatorManager = CharacterAnimtorManager.Instance;
                Assert.That(dataManager, Is.Not.Null);
                Assert.That(animatorManager, Is.Not.Null);

                const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
                objectLookupField = typeof(GameDataManager).GetField("objectLookup", flags);
                cachedConfigField = typeof(GameDataManager).GetField("cachedConfig", flags);
                frameConfigField = typeof(CharacterAnimtorManager).GetField("TotalCharacterFrameConfig", flags);
                Assert.That(objectLookupField, Is.Not.Null);
                Assert.That(cachedConfigField, Is.Not.Null);
                Assert.That(frameConfigField, Is.Not.Null);

                originalObjectLookup = objectLookupField.GetValue(dataManager);
                originalCachedConfig = cachedConfigField.GetValue(dataManager);
                originalFrameConfigs = frameConfigField.GetValue(animatorManager);

                var config = new GameDataConfig();
                var definition = new ObjectDefinition(oid, (int)LF2ObjectType.Other, "late-opoint-capacity.dat");
                config.objects.Add(definition);
                objectLookupField.SetValue(dataManager, new Dictionary<int, ObjectDefinition> { [oid] = definition });
                cachedConfigField.SetValue(dataManager, config);
                frameConfigField.SetValue(animatorManager,
                    new Dictionary<int, LF2CharacterDataWrapper> { [oid] = new LF2CharacterDataWrapper(oid, data) });
            }

            public void Dispose()
            {
                objectLookupField.SetValue(dataManager, originalObjectLookup);
                cachedConfigField.SetValue(dataManager, originalCachedConfig);
                frameConfigField.SetValue(animatorManager, originalFrameConfigs);
            }
        }

        private sealed class IsolatedObjectPoolScope : IDisposable
        {
            private readonly LF2ObjectPool pool;
            private readonly FieldInfo availableField;
            private readonly FieldInfo activeField;
            private readonly FieldInfo releaseMapField;
            private readonly FieldInfo spritePoolField;
            private readonly FieldInfo cachedPrefabField;
            private readonly object originalAvailable;
            private readonly object originalActive;
            private readonly object originalReleaseMap;
            private readonly object originalSpritePool;
            private readonly object originalCachedPrefab;

            public IsolatedObjectPoolScope(LF2ObjectPool pool)
            {
                this.pool = pool;
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
                Type type = typeof(LF2ObjectPool);
                availableField = type.GetField("_availableObjects", flags);
                activeField = type.GetField("_activeObjects", flags);
                releaseMapField = type.GetField("_releaseTimeMap", flags);
                spritePoolField = type.GetField("_spritePool", flags);
                cachedPrefabField = type.GetField("_cachedLF2ObjectPrefab", flags);
                Assert.That(availableField, Is.Not.Null);
                Assert.That(activeField, Is.Not.Null);
                Assert.That(releaseMapField, Is.Not.Null);
                Assert.That(spritePoolField, Is.Not.Null);
                Assert.That(cachedPrefabField, Is.Not.Null);

                originalAvailable = availableField.GetValue(pool);
                originalActive = activeField.GetValue(pool);
                originalReleaseMap = releaseMapField.GetValue(pool);
                originalSpritePool = spritePoolField.GetValue(pool);
                originalCachedPrefab = cachedPrefabField.GetValue(pool);

                availableField.SetValue(pool, new LinkedList<GameObject>());
                activeField.SetValue(pool, new HashSet<GameObject>());
                releaseMapField.SetValue(pool, new Dictionary<GameObject, float>());
                spritePoolField.SetValue(pool, new Stack<SpriteRenderer>());
                cachedPrefabField.SetValue(pool, null);
            }

            public void Dispose()
            {
                var objects = new HashSet<GameObject>();
                Collect(availableField.GetValue(pool), objects);
                Collect(activeField.GetValue(pool), objects);
                availableField.SetValue(pool, originalAvailable);
                activeField.SetValue(pool, originalActive);
                releaseMapField.SetValue(pool, originalReleaseMap);
                spritePoolField.SetValue(pool, originalSpritePool);
                cachedPrefabField.SetValue(pool, originalCachedPrefab);

                foreach (GameObject item in objects)
                    UnityEngine.Object.DestroyImmediate(item);
            }

            private static void Collect(object source, HashSet<GameObject> objects)
            {
                if (source is LinkedList<GameObject> available)
                {
                    foreach (GameObject item in available)
                        if (item != null) objects.Add(item);
                }
                else if (source is HashSet<GameObject> active)
                {
                    foreach (GameObject item in active)
                        if (item != null) objects.Add(item);
                }
            }
        }

        private sealed class SimulationDriverWorldScope : IDisposable
        {
            private readonly FieldInfo instanceField;
            private readonly SimulationTickDriver originalInstance;
            private readonly SimulationTickDriver driver;
            private readonly FieldInfo worldField;
            private readonly SimulationWorld originalWorld;
            private readonly GameObject temporaryDriverObject;

            public SimulationDriverWorldScope()
            {
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
                instanceField = typeof(SimulationTickDriver).BaseType?.GetField("<Instance>k__BackingField", flags);
                Assert.That(instanceField, Is.Not.Null);
                originalInstance = instanceField.GetValue(null) as SimulationTickDriver;
                driver = SimulationTickDriver.Instance;
                if (driver == null)
                {
                    temporaryDriverObject = new GameObject("LateOpointCapacity_SimulationTickDriver");
                    driver = temporaryDriverObject.AddComponent<SimulationTickDriver>();
                    instanceField.SetValue(null, driver);
                }

                worldField = typeof(SimulationTickDriver).GetField("_world",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(worldField, Is.Not.Null);
                originalWorld = worldField.GetValue(driver) as SimulationWorld;
            }

            public void SetWorld(SimulationWorld world)
            {
                worldField.SetValue(driver, world);
            }

            public void Dispose()
            {
                worldField.SetValue(driver, originalWorld);
                instanceField.SetValue(null, originalInstance);
                if (temporaryDriverObject != null)
                    UnityEngine.Object.DestroyImmediate(temporaryDriverObject);
            }
        }
    }
}
#endif
