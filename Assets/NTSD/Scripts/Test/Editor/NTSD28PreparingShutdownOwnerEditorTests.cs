#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ExceptionServices;
using MoreMountains.Tools;
using NTSD.Animation;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class NTSD28PreparingShutdownOwnerEditorTests
    {
        private readonly List<GameObject> owned = new List<GameObject>();
        private SimulationTickDriver driver;
        private LF2ObjectPool pool;
        private LF2ObjectPointFactory factory;
        private object previousPool;
        private object previousFactory;
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

        private T New<T>() where T : Component
        {
            var go = new GameObject(typeof(T).Name + "_PreparationTest") { hideFlags = HideFlags.HideAndDontSave };
            go.SetActive(false);
            owned.Add(go);
            return go.AddComponent<T>();
        }

        private static FieldInfo Singleton<T>() where T : Component =>
            typeof(MMSingleton<T>).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);

        private static object Invoke(object target, string name, params object[] args)
        {
            MethodInfo method = target.GetType().GetMethod(name, PrivateInstance | BindingFlags.Public);
            Assert.That(method, Is.Not.Null, "Required preparation API missing: " + name);
            try { return method.Invoke(target, args); }
            catch (TargetInvocationException error) when (error.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(error.InnerException).Throw();
                throw;
            }
        }

        private void SetDriver(string field, object value) =>
            typeof(SimulationTickDriver).GetField(field, PrivateInstance).SetValue(driver, value);

        [SetUp]
        public void SetUp()
        {
            previousPool = Singleton<LF2ObjectPool>().GetValue(null);
            previousFactory = Singleton<LF2ObjectPointFactory>().GetValue(null);
            driver = New<SimulationTickDriver>();
            pool = New<LF2ObjectPool>();
            factory = New<LF2ObjectPointFactory>();
            Singleton<LF2ObjectPool>().SetValue(null, pool);
            Singleton<LF2ObjectPointFactory>().SetValue(null, factory);
            Invoke(driver, "EnterPreparingState");
            SetDriver("_world", new SimulationWorld());
        }

        [TearDown]
        public void TearDown()
        {
            Singleton<LF2ObjectPool>().SetValue(null, previousPool);
            Singleton<LF2ObjectPointFactory>().SetValue(null, previousFactory);
            foreach (GameObject go in owned) if (go != null) UnityEngine.Object.DestroyImmediate(go);
            owned.Clear();
        }

        [Test]
        public void PreparingBeforeBirth_CapturesBothOwners_WithoutSealing_AndIsIdempotent()
        {
            Invoke(driver, "PrepareBattleRuntimeServices");
            Assert.That(typeof(SimulationTickDriver).GetField("_battleObjectPool", PrivateInstance).GetValue(driver), Is.SameAs(pool));
            Assert.That(typeof(SimulationTickDriver).GetField("_battleObjectPointFactory", PrivateInstance).GetValue(driver), Is.SameAs(factory));
            Assert.That(driver.World.RuntimeCapacity.IsSealed, Is.False);
            Assert.That(driver.LifecycleState, Is.EqualTo(BattleRuntimeLifecycleState.Preparing));
            Invoke(driver, "PrepareBattleRuntimeServices");
        }

        [TestCase(BattleRuntimeLifecycleState.Running)]
        [TestCase(BattleRuntimeLifecycleState.Stopping)]
        [TestCase(BattleRuntimeLifecycleState.Stopped)]
        public void InactivePreparation_RejectsWithoutCreatingServices(BattleRuntimeLifecycleState state)
        {
            SetDriver("lifecycleState", state);
            Singleton<LF2ObjectPool>().SetValue(null, null);
            Singleton<LF2ObjectPointFactory>().SetValue(null, null);
            Assert.Throws<InvalidOperationException>(() => Invoke(driver, "PrepareBattleRuntimeServices"));
            Assert.That(LF2ObjectPool.TryGetInstance(), Is.Null);
            Assert.That(LF2ObjectPointFactory.TryGetInstance(), Is.Null);
        }

        [Test]
        public void ExistingForeignPoolBorrower_RejectsBeforeTakingOwnership()
        {
            var borrowers = new HashSet<GameObject> { New<SpriteRenderer>().gameObject };
            typeof(LF2ObjectPool).GetField("_activeObjects", PrivateInstance).SetValue(pool, borrowers);
            Assert.Throws<InvalidOperationException>(() => Invoke(driver, "PrepareBattleRuntimeServices"));
            Assert.That(typeof(SimulationTickDriver).GetField("_battleObjectPool", PrivateInstance).GetValue(driver), Is.Null);
            Assert.That(borrowers.Count, Is.EqualTo(1));
        }

        [Test]
        public void ExistingFactoryQueue_RejectsWithoutDiscardingForeignWork()
        {
            factory.EnqueueCreateObject(new OPointCreateTask());
            Assert.That(factory.PendingTaskCountForDiagnostics, Is.EqualTo(1));
            Assert.Throws<InvalidOperationException>(() => Invoke(driver, "PrepareBattleRuntimeServices"));
            Assert.That(factory.PendingTaskCountForDiagnostics, Is.EqualTo(1));
        }

        [Test]
        public void PreparedOwnerReplacement_RejectsBeforeChangingCapturedOwner()
        {
            Invoke(driver, "PrepareBattleRuntimeServices");
            Singleton<LF2ObjectPool>().SetValue(null, New<LF2ObjectPool>());
            Assert.Throws<InvalidOperationException>(() => Invoke(driver, "PrepareBattleRuntimeServices"));
            Assert.That(typeof(SimulationTickDriver).GetField("_battleObjectPool", PrivateInstance).GetValue(driver), Is.SameAs(pool));
        }

        [Test]
        public void ContinuationGuard_RejectsStoppedReenteredAndDifferentWorld()
        {
            PropertyInfo generationProperty = typeof(SimulationTickDriver).GetProperty("PreparationGeneration", PrivateInstance | BindingFlags.Public);
            Assert.That(generationProperty, Is.Not.Null);
            object generation = generationProperty.GetValue(driver);
            SimulationWorld world = driver.World;
            Assert.That(Invoke(driver, "IsBattlePreparationCurrent", generation, world), Is.True);
            Assert.That(Invoke(driver, "IsBattlePreparationCurrent", generation, new SimulationWorld()), Is.False);
            SetDriver("lifecycleState", BattleRuntimeLifecycleState.Stopped);
            Assert.That(Invoke(driver, "IsBattlePreparationCurrent", generation, world), Is.False);
            Invoke(driver, "EnterPreparingState");
            Assert.That(driver.World, Is.SameAs(world));
            Assert.That(Invoke(driver, "IsBattlePreparationCurrent", generation, world), Is.False);
            Assert.That(Invoke(driver, "IsBattlePreparationCurrent", generationProperty.GetValue(driver), world), Is.True);
        }
    }
}
#endif
