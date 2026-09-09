#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Reflection;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5Type1ArmorRuntimeProfileIntegrationEditorTests
    {
        [Test]
        public void ProfileReadsOnlyFirstArmorBlock()
        {
            LF2Character character = CharacterWithData(
                Data(Armor(7, 3), Armor(99, 88)));

            bool found = character.TryGetNativeArmorRecoveryProfileForWorldPass(
                out int hp,
                out int recover);

            Assert.That(found, Is.True);
            Assert.That((hp, recover), Is.EqualTo((7, 3)));
        }

        [TestCase(3, 3)]
        [TestCase(0, -1)]
        [TestCase(-2, -1)]
        public void ModuleBind_InitializesBirthProfile(
            int recover,
            int expectedTimer)
        {
            LF2Character character = Bind(Data(Armor(7, recover)));

            Assert.That(character.Runtime.RuntimeArmorHp118, Is.EqualTo(7));
            Assert.That(character.Runtime.ArmorRecoveryTimer11C,
                Is.EqualTo(expectedTimer));
        }

        [Test]
        public void ReuseWithoutArmor_ClearsStaleRuntimeProfile()
        {
            LF2Character character = CharacterWithData(Data(Armor(7, 3)));
            character.Runtime.RuntimeArmorHp118 = 91;
            character.Runtime.ArmorRecoveryTimer11C = 92;

            character.ModuleBind(new LF2CharacterDataWrapper(2, Data()), 2);

            Assert.That(character.Runtime.RuntimeArmorHp118, Is.Zero);
            Assert.That(character.Runtime.ArmorRecoveryTimer11C,
                Is.EqualTo(-1));
        }

        [Test]
        public void SnapshotBindOption_PreservesRuntimeFields()
        {
            LF2Character character = CharacterWithData(Data());
            character.Runtime.RuntimeArmorHp118 = 41;
            character.Runtime.ArmorRecoveryTimer11C = 12;
            MethodInfo method = typeof(LF2Character).GetMethod(
                "ModuleBind",
                BindingFlags.Instance | BindingFlags.Public,
                null,
                new[]
                {
                    typeof(LF2CharacterDataWrapper),
                    typeof(int),
                    typeof(SimulationWorld),
                    typeof(bool),
                },
                null);
            Assert.That(method, Is.Not.Null);

            method.Invoke(character, new object[]
            {
                new LF2CharacterDataWrapper(2, Data(Armor(99, 88))),
                2,
                null,
                false,
            });

            Assert.That(character.Runtime.RuntimeArmorHp118, Is.EqualTo(41));
            Assert.That(character.Runtime.ArmorRecoveryTimer11C,
                Is.EqualTo(12));
        }

        [Test]
        public void C25iProductionOwner_ReloadsFromTypedFirstArmorProfile()
        {
            var world = new SimulationWorld();
            LF2Character character = Bind(Data(Armor(7, 2)), world);
            character.Runtime.RuntimeArmorHp118 = 0;

            world.LateEntityUpdateAll(1);
            Assert.That(
                (character.Runtime.RuntimeArmorHp118,
                 character.Runtime.ArmorRecoveryTimer11C),
                Is.EqualTo((0, 1)));
            world.LateEntityUpdateAll(2);
            Assert.That(
                (character.Runtime.RuntimeArmorHp118,
                 character.Runtime.ArmorRecoveryTimer11C),
                Is.EqualTo((7, 2)));
        }

        [Test]
        public void WarmProfileRead_AllocatesNoManagedMemory()
        {
            LF2Character character = CharacterWithData(
                Data(Armor(7, 3), Armor(99, 88)));
            _ = character.TryGetNativeArmorRecoveryProfileForWorldPass(
                out _,
                out _);
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            int checksum = 17;
            for (int index = 0; index < 4096; index++)
            {
                bool found =
                    character.TryGetNativeArmorRecoveryProfileForWorldPass(
                        out int hp,
                        out int recover);
                checksum = unchecked(checksum * 31 + (found ? 1 : 0));
                checksum = unchecked(checksum * 31 + hp);
                checksum = unchecked(checksum * 31 + recover);
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(checksum, Is.Not.Zero);
            Assert.That(allocated, Is.Zero);
        }

        private static LF2Character Bind(
            LF2CharacterData data,
            SimulationWorld world = null)
        {
            var character = new LF2Character();
            character.ModuleInitialize();
            character.ModuleBind(
                new LF2CharacterDataWrapper(1, data),
                1,
                world);
            character.Initialize(500, 500);
            character.Runtime.SuppressLateFrameTickUntilTick = 0;
            return character;
        }

        private static LF2Character CharacterWithData(LF2CharacterData data)
        {
            var character = new LF2Character();
            character.ModuleInitialize();
            character.FrameCache.Load(new LF2CharacterDataWrapper(1, data));
            return character;
        }

        private static LF2CharacterData Data(params LF2ArmorData[] armors)
        {
            return new LF2CharacterData
            {
                type_sub = (int)LF2ObjectType.Character,
                armors = new List<LF2ArmorData>(armors),
                frames = new List<LF2FrameData>
                {
                    new LF2FrameData
                    {
                        frameId = 0,
                        state = LF2States.Standing,
                        wait = 100,
                        next = 0,
                    },
                },
            };
        }

        private static LF2ArmorData Armor(int hp, int recover)
        {
            return new LF2ArmorData
            {
                type = 1,
                hp = hp,
                recover = recover,
            };
        }
    }
}
#endif
