#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28Q06FusionBirthIdentityEditorTests
    {
        private static LF2CharacterDataWrapper Wrapper(int type, int alias, int drop)
        {
            var wrapper = NTSD28Q06OpointWeaponHpEditorTests.Wrapper(type, true, 17);
            var old = wrapper.characterData.NativeMetadata;
            wrapper.characterData.NativeMetadata = new LoganDefinitionMetadata(
                new LoganDefinitionFieldSet(new[]
                {
                    new KeyValuePair<string, string>("use_ai", alias.ToString()),
                    new KeyValuePair<string, string>("drop", drop.ToString()),
                    new KeyValuePair<string, string>("weapon_hp", "17")
                }), old.Stats);
            wrapper.characterData.use_ai = 999;
            return wrapper;
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(3)]
        [TestCase(5)]
        public void EachBirthClassInitializesPersistentIdentityAndPoolReuseReplacesOldValues(int type)
        {
            var wrapper = Wrapper(type, 31, 3);
            var world = new SimulationWorld(new RuntimeCharacterConfigResolver(_ => wrapper));
            world.SetLogicOnlyEntityMaterialization(true);
            world.PrepareRuntimeDataCatalogForBattle(new[] { new ObjectDefinition(777, type, "fusion-birth.dat") }, _ => wrapper);
            try
            {
                for (int birth = 0; birth < 2; birth++)
                {
                    var task = new OPointCreateTask
                    {
                        targetWorld = world, dir = "right", preserveActionZero = true,
                        opoint = new ObjectPoint { oid = 777, kind = 1, action = 0, hp = 41 }
                    };
                    var child = world.LogicEntityFactory.Create(task, out var failure);
                    Assert.That(child, Is.Not.Null, failure.ToString());
                    Assert.That(child.Runtime.NativeAiProfileObjectId, Is.EqualTo(31));
                    Assert.That(child.Runtime.NativeDefinitionDropMode, Is.EqualTo(3));
                    child.Runtime.FusionDisplayTimer190 = 4500;
                    child.FrameCache.Load(Wrapper(type, 32, 4));
                    Assert.That(child.Runtime.NativeAiProfileObjectId, Is.EqualTo(31));
                    Assert.That(child.Runtime.NativeDefinitionDropMode, Is.EqualTo(3));
                    Assert.That(child.Runtime.FusionDisplayTimer190, Is.EqualTo(4500));
                    child.Runtime.NativeAiProfileObjectId = -999;
                    child.Runtime.NativeDefinitionDropMode = -999;
                    child.FreeEntityLikeExe();
                    Assert.That(world.ObjectCount, Is.Zero);
                }
            }
            finally { NTSD28Q06State18SpawnEditorTests.Shutdown(world); }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void CharacterModuleBindExplicitlySeparatesBirthFromSnapshotIdentity(bool birth)
        {
            var world = new SimulationWorld();
            var character = new LF2Character { ObjectId = 777 };
            character.Runtime.NativeAiProfileObjectId = -7;
            character.Runtime.NativeDefinitionDropMode = 9;
            character.Runtime.FusionDisplayTimer190 = 4500;
            try
            {
                character.ModuleInitialize();
                character.ModuleBind(Wrapper(0, 31, 3), 777, world,
                    initializeNativeArmorRuntime: false, initializeNativeDefinitionIdentity: birth);
                Assert.That(character.Runtime.NativeAiProfileObjectId, Is.EqualTo(birth ? 31 : -7));
                Assert.That(character.Runtime.NativeDefinitionDropMode, Is.EqualTo(birth ? 3 : 9));
                Assert.That(character.Runtime.FusionDisplayTimer190, Is.EqualTo(4500));
            }
            finally { NTSD28Q06State18SpawnEditorTests.Shutdown(world); }
        }

        [Test]
        public void MissingNativeFieldsUseZeroAndManualWrapperRetainsExplicitAlias()
        {
            var child = new LF2SpecialAttack();
            var wrapper = NTSD28Q06OpointWeaponHpEditorTests.Wrapper(3, false, 0);
            wrapper.characterData.use_ai = 999;
            child.FrameCache.Load(wrapper);
            child.InitializeNativeDefinitionIdentityForSpawn();
            Assert.That(child.Runtime.NativeAiProfileObjectId, Is.Zero);
            Assert.That(child.Runtime.NativeDefinitionDropMode, Is.Zero);
            wrapper.characterData.NativeMetadata = null;
            child.InitializeNativeDefinitionIdentityForSpawn();
            Assert.That(child.Runtime.NativeAiProfileObjectId, Is.EqualTo(999));
        }
    }
}
#endif
