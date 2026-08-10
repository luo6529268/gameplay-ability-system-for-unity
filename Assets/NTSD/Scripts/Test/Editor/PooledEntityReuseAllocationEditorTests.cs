using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Input;
using NTSD.Simulation;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class PooledEntityReuseAllocationEditorTests
    {
        [Test]
        public void FrameCache_RepeatedDatLoad_DoesNotAllocateAfterConstruction()
        {
            var data = new LF2CharacterData();
            for (int frameId = 0; frameId < 120; frameId++)
            {
                data.frames.Add(new LF2FrameData
                {
                    frameId = frameId,
                    frameName = frameId % 2 == 0 ? "even" : "odd",
                });
            }

            var wrapper = new LF2CharacterDataWrapper(7, data);
            var cache = new LF2FrameCache();
            cache.Load(wrapper);

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int iteration = 0; iteration < 100; iteration++)
                cache.Load(wrapper);
            long allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocatedBytes, Is.Zero);
            Assert.That(cache.GetFrameDataById(42), Is.SameAs(data.frames[42]));
        }

        [Test]
        public void PooledLogicEntity_Reset_ReusesOwnedReferenceComponents()
        {
            var character = new LF2Character();
            var weapon = new LF2Weapon();
            var specialAttack = new LF2SpecialAttack();
            var otherObject = new LF2OtherObject();

            object characterController = character.Controller;
            object weaponPhysics = weapon.PS;
            object weaponFrame = weapon.Frame;
            object weaponEffect = weapon.Effect;
            object weaponRest = weapon.ItrRest;
            object weaponSprite = weapon.Sprite;
            object weaponTransistor = weapon.Trans;
            object specialPhysics = specialAttack.PS;
            object specialFrame = specialAttack.Frame;
            object specialEffect = specialAttack.Effect;
            object specialRest = specialAttack.ItrRest;
            object specialSprite = specialAttack.Sprite;
            object specialTransistor = specialAttack.Trans;
            object otherPhysics = otherObject.PS;
            object otherFrame = otherObject.Frame;
            object otherEffect = otherObject.Effect;
            object otherRest = otherObject.ItrRest;
            object otherSprite = otherObject.Sprite;
            object otherTransistor = otherObject.Trans;

            character.Reset();
            weapon.Reset();
            specialAttack.Reset();
            otherObject.Reset();

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int iteration = 0; iteration < 100; iteration++)
            {
                character.Reset();
                weapon.Reset();
                specialAttack.Reset();
                otherObject.Reset();
            }
            long allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocatedBytes, Is.Zero);
            Assert.That(character.Controller, Is.SameAs(characterController));
            AssertOwnedComponents(
                weapon,
                weaponPhysics,
                weaponFrame,
                weaponEffect,
                weaponRest,
                weaponSprite,
                weaponTransistor);
            AssertOwnedComponents(
                specialAttack,
                specialPhysics,
                specialFrame,
                specialEffect,
                specialRest,
                specialSprite,
                specialTransistor);
            AssertOwnedComponents(
                otherObject,
                otherPhysics,
                otherFrame,
                otherEffect,
                otherRest,
                otherSprite,
                otherTransistor);
        }

        [Test]
        public void StageSpawnTask_Reconfiguration_ReusesPreparedTaskWithoutAllocating()
        {
            var spawn = new BattleStageSpawnData
            {
                Id = 7,
                Act = 112,
            };
            var task = new OPointCreateTask();
            var configurator = new StageSpawnTaskConfigurator();
            configurator.Configure(
                task,
                spawn,
                10,
                -20,
                200,
                "right",
                50);

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int iteration = 0; iteration < 100; iteration++)
            {
                configurator.Configure(
                    task,
                    spawn,
                    10,
                    -20,
                    200,
                    "right",
                    50);
            }
            long allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocatedBytes, Is.Zero);
            Assert.That(task.opoint.oid, Is.EqualTo(7));
            Assert.That(task.opoint.action, Is.EqualTo(112));
            Assert.That(task.requiredRuntimeSlot, Is.EqualTo(50));
            Assert.That(task.releaseSpawnSemantic, Is.EqualTo(ReleaseSpawnSemantic.StageSpawnAt));
        }

        [Test]
        public void SimInputBuffer_RepeatedEnqueueAndConsume_DoesNotAllocate()
        {
            var buffer = new SimInputBuffer();
            buffer.EnqueueForTick(1, FuncKeyMask.left, true);
            Assert.That(buffer.TryDequeueAll(1, out SimInputEventBatch warmup), Is.True);
            Assert.That(warmup.Count, Is.EqualTo(1));

            bool contentsValid = true;
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int tick = 2; tick < 1002; tick++)
            {
                buffer.EnqueueForTick(tick, FuncKeyMask.left, true);
                buffer.EnqueueForTick(tick, FuncKeyMask.att, false);
                if (!buffer.TryDequeueAll(tick, out SimInputEventBatch batch) ||
                    batch.Count != 2 ||
                    batch[0].key != FuncKeyMask.left ||
                    batch[1].key != FuncKeyMask.att)
                {
                    contentsValid = false;
                }
            }
            long allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocatedBytes, Is.Zero);
            Assert.That(contentsValid, Is.True);
            Assert.That(buffer.BufferedTickCount, Is.Zero);
            Assert.That(buffer.RejectedEventCount, Is.Zero);
        }

        [Test]
        public void SharedCharacterDatFrameAdvance_ReusesOwnedMechanicsWithoutAllocating()
        {
            var data = new LF2CharacterData
            {
                name = "SharedCharacterDatAllocationProbe",
                type_sub = (int)LF2ObjectType.Character,
            };
            data.frames.Add(new LF2FrameData
            {
                frameId = 0,
                state = LF2States.Standing,
                wait = 10000,
                next = 0,
                centerx = 39,
                centery = 79,
            });

            var entity = new LF2SpecialAttack
            {
                ObjectId = 700,
            };
            entity.FrameCache.Load(
                new LF2CharacterDataWrapper(entity.ObjectId, data));
            entity.ImmediateFrame(0);
            entity.Runtime.SetPosition(0.0, 0.0, 0.0);
            entity.Runtime.SetVelocity(1.0, 0.0, 0.0);
            entity.Runtime.SyncIntegerPosition();
            entity.SimTU(1);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int tickIndex = 2; tickIndex < 258; tickIndex++)
                entity.SimTU(tickIndex);
            long allocatedBytes =
                GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocatedBytes, Is.Zero);
        }

        [Test]
        public void PooledCharacter_ModuleInitialize_ReusesOwnedMechanicsWithoutAllocating()
        {
            var character = new LF2Character();
            character.ModuleInitialize();

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int iteration = 0; iteration < 256; iteration++)
                character.ModuleInitialize();
            long allocatedBytes =
                GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocatedBytes, Is.Zero);
            Assert.That(character.Runtime.X, Is.Zero);
            Assert.That(character.Runtime.Y, Is.Zero);
            Assert.That(character.Runtime.Z, Is.Zero);
            Assert.That(character.Runtime.Vx, Is.Zero);
            Assert.That(character.Runtime.Vy, Is.Zero);
            Assert.That(character.Runtime.Vz, Is.Zero);
        }

        [Test]
        public void HeldWeaponResults_AreValueTypesAndRepeatedActDoesNotAllocate()
        {
            var holderData = new LF2CharacterData
            {
                name = "HeldWeaponAllocationHolder",
                type_sub = (int)LF2ObjectType.Character,
            };
            holderData.frames.Add(new LF2FrameData
            {
                frameId = 0,
                state = LF2States.Standing,
                wait = 10000,
                centerx = 39,
                centery = 79,
            });

            var weaponData = new LF2CharacterData
            {
                name = "HeldWeaponAllocationWeapon",
                type_sub = (int)LF2ObjectType.LightWeapon,
            };
            weaponData.frames.Add(new LF2FrameData
            {
                frameId = 0,
                state = LF2States.WeaponOnHand,
                wait = 10000,
                centerx = 20,
                centery = 20,
            });

            var holder = new LF2Character();
            holder.FrameCache.Load(new LF2CharacterDataWrapper(1, holderData));
            holder.ImmediateFrame(0);
            holder.Runtime.Dir = "right";
            holder.Runtime.SetPosition(100.0, 0.0, 200.0);
            holder.Runtime.SyncIntegerPosition();

            var weapon = new AllocationProbeWeapon();
            weapon.SetWeaponType((int)LF2ObjectType.LightWeapon);
            weapon.FrameCache.Load(new LF2CharacterDataWrapper(100, weaponData));
            weapon.ImmediateFrame(0);
            weapon.Runtime.Dir = "right";
            weapon.SetWeaponStrengthList(new List<WeaponStrengthEntry>
            {
                new WeaponStrengthEntry
                {
                    index = 1,
                },
            });

            var heldPoint = new WeaponPoint
            {
                weaponact = 0,
            };
            var attackPoint = new WeaponPoint
            {
                attacking = 1,
            };

            weapon.Act(holder, heldPoint, Vector3.zero);
            weapon.InvokeProcessAttack(holder, attackPoint, weapon.Frame.D);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            WeaponActResult lastAct = default;
            WeaponAttackResult lastAttack = default;
            for (int iteration = 0; iteration < 256; iteration++)
            {
                lastAct = weapon.Act(holder, heldPoint, Vector3.zero);
                lastAttack = weapon.InvokeProcessAttack(
                    holder,
                    attackPoint,
                    weapon.Frame.D);
            }
            long allocatedBytes =
                GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(typeof(WeaponActResult).IsValueType, Is.True);
            Assert.That(typeof(WeaponAttackResult).IsValueType, Is.True);
            Assert.That(allocatedBytes, Is.Zero);
            Assert.That(lastAct.Thrown, Is.False);
            Assert.That(lastAttack.HitUid, Is.Zero);
        }

        [Test]
        public void HeldWeaponAttack_WithBodyQuery_ReusesInteractionAreaWithoutAllocating()
        {
            var weaponData = new LF2CharacterData
            {
                name = "HeldWeaponBodyQueryAllocationProbe",
                type_sub = (int)LF2ObjectType.LightWeapon,
            };
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = LF2States.WeaponOnHand,
                wait = 10000,
                centerx = 20,
                centery = 20,
            };
            frame.bodies.Add(new BodyBox { x = 0, y = 0, w = 40, h = 40 });
            frame.itrs.Add(new InteractionArea
            {
                kind = 0,
                x = 0,
                y = 0,
                w = 40,
                h = 40,
            });
            weaponData.frames.Add(frame);

            var weapon = new AllocationProbeWeapon();
            weapon.SetWeaponType((int)LF2ObjectType.LightWeapon);
            weapon.FrameCache.Load(new LF2CharacterDataWrapper(100, weaponData));
            weapon.ImmediateFrame(0);
            weapon.Runtime.Dir = "right";
            weapon.SetWeaponStrengthList(new List<WeaponStrengthEntry>
            {
                new WeaponStrengthEntry
                {
                    index = 1,
                    dvx = 3,
                    dvy = 7,
                    fall = 70,
                    vrest = 10,
                    injury = 20,
                },
            });
            var world = new SimulationWorld();
            world.Register(weapon);
            var attackPoint = new WeaponPoint { attacking = 1 };

            weapon.InvokeProcessAttack(null, attackPoint, frame);
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int iteration = 0; iteration < 256; iteration++)
                weapon.InvokeProcessAttack(null, attackPoint, frame);
            long allocatedBytes =
                GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocatedBytes, Is.Zero);
        }

        [Test]
        public void HeldWeaponThrow_RepeatedProductionReleasePath_DoesNotAllocate()
        {
            var holderData = new LF2CharacterData
            {
                name = "HeldWeaponThrowHolder",
                type_sub = (int)LF2ObjectType.Character,
            };
            holderData.frames.Add(new LF2FrameData
            {
                frameId = 0,
                state = LF2States.Standing,
                wait = 10000,
                centerx = 39,
                centery = 79,
            });

            var weaponData = new LF2CharacterData
            {
                name = "HeldWeaponThrowWeapon",
                type_sub = (int)LF2ObjectType.LightWeapon,
            };
            weaponData.frames.Add(new LF2FrameData
            {
                frameId = 0,
                state = LF2States.WeaponOnHand,
                wait = 10000,
                centerx = 20,
                centery = 20,
            });
            weaponData.frames.Add(new LF2FrameData
            {
                frameId = 40,
                state = LF2States.WeaponThrowing,
                wait = 10000,
                centerx = 20,
                centery = 20,
            });

            var holder = new LF2Character();
            holder.FrameCache.Load(new LF2CharacterDataWrapper(1, holderData));
            holder.ImmediateFrame(0);
            holder.Runtime.Dir = "right";
            holder.Runtime.SetPosition(100.0, 0.0, 200.0);
            holder.Runtime.SyncIntegerPosition();

            var weapon = new AllocationProbeWeapon();
            weapon.SetWeaponType((int)LF2ObjectType.LightWeapon);
            weapon.FrameCache.Load(new LF2CharacterDataWrapper(100, weaponData));
            weapon.ImmediateFrame(0);
            weapon.Runtime.Dir = "right";

            var world = new SimulationWorld();
            world.Register(holder);
            world.Register(weapon);
            var throwPoint = new WeaponPoint
            {
                weaponact = 0,
                dvx = 12,
                dvy = -4,
                dvz = 3,
            };

            PrepareHeldWeaponThrowIteration(holder, weapon);
            Assert.That(
                weapon.Act(holder, throwPoint, Vector3.zero).Thrown,
                Is.True);

            bool allThrowsAccepted = true;
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int iteration = 0; iteration < 256; iteration++)
            {
                PrepareHeldWeaponThrowIteration(holder, weapon);
                if (!weapon.Act(holder, throwPoint, Vector3.zero).Thrown)
                    allThrowsAccepted = false;
            }
            long allocatedBytes =
                GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocatedBytes, Is.Zero);
            Assert.That(allThrowsAccepted, Is.True);
            Assert.That(holder.Runtime.HeldWeaponStableId, Is.EqualTo(-1));
            Assert.That(weapon.Runtime.LinkState, Is.Zero);
            Assert.That(weapon.Frame.N, Is.EqualTo(40));
        }

        [Test]
        public void BurningWeaponLanding_ReusesSplashInteractionAreaWithoutAllocating()
        {
            var weaponData = new LF2CharacterData
            {
                name = "BurningWeaponLandingAllocationProbe",
                type_sub = (int)LF2ObjectType.Character,
                weapon_drop_hurt = 10,
            };
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = LF2States.Burning,
                wait = 10000,
                centerx = 20,
                centery = 20,
            };
            frame.bodies.Add(new BodyBox { x = 0, y = 0, w = 40, h = 40 });
            frame.itrs.Add(new InteractionArea
            {
                kind = 0,
                x = 0,
                y = 0,
                w = 40,
                h = 40,
            });
            weaponData.frames.Add(frame);

            var weapon = new AllocationProbeWeapon();
            weapon.SetWeaponType((int)LF2ObjectType.Character);
            weapon.FrameCache.Load(new LF2CharacterDataWrapper(100, weaponData));
            weapon.ImmediateFrame(0);
            weapon.Health.HP = 100000;
            var world = new SimulationWorld();
            world.Register(weapon);

            PrepareBurningLandingIteration(weapon);
            weapon.InvokeOnLanded();
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int iteration = 0; iteration < 256; iteration++)
            {
                PrepareBurningLandingIteration(weapon);
                weapon.InvokeOnLanded();
            }
            long allocatedBytes =
                GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocatedBytes, Is.Zero);
        }

        [Test]
        public void HeldObjectPass_MissingWPointUsesMatchOwnedDefaultWithoutAllocating()
        {
            var holderData = new LF2CharacterData
            {
                name = "HeldObjectDefaultPointHolder",
                type_sub = (int)LF2ObjectType.Character,
            };
            holderData.frames.Add(new LF2FrameData
            {
                frameId = 0,
                state = LF2States.Standing,
                wait = 10000,
                centerx = 39,
                centery = 79,
            });

            var heldData = new LF2CharacterData
            {
                name = "HeldObjectDefaultPointEntity",
                type_sub = (int)LF2ObjectType.SpecialAttack,
            };
            heldData.frames.Add(new LF2FrameData
            {
                frameId = 0,
                state = LF2States.Standing,
                wait = 10000,
                centerx = 20,
                centery = 20,
            });

            var holder = new LF2Character();
            holder.FrameCache.Load(new LF2CharacterDataWrapper(1, holderData));
            holder.ImmediateFrame(0);
            holder.Runtime.Dir = "right";
            holder.Runtime.SetPosition(100.0, 0.0, 200.0);
            holder.Runtime.SyncIntegerPosition();

            var held = new LF2SpecialAttack();
            held.FrameCache.Load(new LF2CharacterDataWrapper(300, heldData));
            held.ImmediateFrame(0);
            held.Runtime.Dir = "right";

            var world = new SimulationWorld();
            world.Register(holder);
            world.Register(held);
            holder.Runtime.LinkState = 1;
            holder.Runtime.TargetSlotIndex = held.Runtime.SlotIndex;
            holder.Runtime.HeldWeaponStableId = held.Runtime.SlotIndex;
            held.Runtime.LinkState = -1;
            held.Runtime.HolderStableId = holder.Runtime.SlotIndex;

            world.HeldObjectProcessAll(1);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int tickIndex = 2; tickIndex < 258; tickIndex++)
                world.HeldObjectProcessAll(tickIndex);
            long allocatedBytes =
                GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocatedBytes, Is.Zero);
            Assert.That(holder.Runtime.LinkState, Is.EqualTo(1));
            Assert.That(held.Runtime.LinkState, Is.EqualTo(-1));
            Assert.That(held.Runtime.HolderStableId, Is.EqualTo(holder.Runtime.SlotIndex));
        }

        [Test]
        public void StandardCharacterHit_RepeatedAfterWarmup_DoesNotAllocate()
        {
            LF2CharacterData data = BuildStandardHitData();
            var wrapper = new LF2CharacterDataWrapper(1, data);
            var world = new SimulationWorld();
            var attacker = new AllocationProbeCharacter();
            var victim = new AllocationProbeCharacter();
            attacker.FrameCache.Load(wrapper);
            victim.FrameCache.Load(wrapper);
            attacker.ImmediateFrame(0);
            victim.ImmediateFrame(0);
            attacker.Runtime.Dir = "right";
            victim.Runtime.Dir = "left";
            world.Register(attacker);
            world.Register(victim);

            var itr = new InteractionArea
            {
                kind = 0,
                injury = 1,
                fall = 1,
                dvx = 1,
                arest = 4,
                vrest = 0,
                effect = 0,
            };

            PrepareStandardHitIteration(world, attacker, victim);
            Assert.That(victim.Hit(itr, attacker, Vector3.zero, default), Is.True);

            bool allHitsAccepted = true;
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int iteration = 0; iteration < 256; iteration++)
            {
                PrepareStandardHitIteration(world, attacker, victim);
                if (!victim.Hit(itr, attacker, Vector3.zero, default))
                    allHitsAccepted = false;
            }
            long allocatedBytes =
                GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocatedBytes, Is.Zero);
            Assert.That(allHitsAccepted, Is.True);
            Assert.That(world.PendingSounds.Count, Is.GreaterThan(0));
        }

        [Test]
        public void LethalCharacterHitAndDeathTransition_RepeatedAfterWarmup_DoesNotAllocate()
        {
            LF2CharacterData data = BuildStandardHitData();
            var wrapper = new LF2CharacterDataWrapper(1, data);
            var world = new SimulationWorld();
            var attacker = new AllocationProbeCharacter();
            var victim = new AllocationProbeCharacter();
            attacker.FrameCache.Load(wrapper);
            victim.FrameCache.Load(wrapper);
            attacker.ImmediateFrame(0);
            victim.ImmediateFrame(0);
            attacker.Runtime.Dir = "right";
            victim.Runtime.Dir = "left";
            world.Register(attacker);
            world.Register(victim);

            var itr = new InteractionArea
            {
                kind = 0,
                injury = 100,
                fall = 1,
                dvx = 1,
                arest = 4,
                vrest = 0,
                effect = 0,
            };

            PrepareLethalHitIteration(world, attacker, victim);
            bool warmupHitAccepted =
                victim.Hit(itr, attacker, Vector3.zero, default);
            if (!warmupHitAccepted)
            {
                Assert.Fail(
                    $"warmup lethal hit rejected; hp={victim.Health.HP}, dead={victim.Dead}, frame={victim.Frame.N}");
            }
            victim.FrameDelay = 0;
            victim.InvokeRunTUCore();
            if (!victim.Dead)
            {
                Assert.Fail(
                    $"warmup death transition did not set Dead; hp={victim.Health.HP}, frameDelay={victim.FrameDelay}, frame={victim.Frame.N}");
            }

            int rejectedHitCount = 0;
            int missedDeathCount = 0;
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int iteration = 0; iteration < 256; iteration++)
            {
                PrepareLethalHitIteration(world, attacker, victim);
                if (!victim.Hit(itr, attacker, Vector3.zero, default))
                {
                    rejectedHitCount++;
                    continue;
                }

                victim.FrameDelay = 0;
                victim.InvokeRunTUCore();
                if (!victim.Dead)
                    missedDeathCount++;
            }
            long allocatedBytes =
                GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocatedBytes, Is.Zero);
            Assert.That(rejectedHitCount, Is.Zero);
            Assert.That(missedDeathCount, Is.Zero);
            Assert.That(victim.Health.HP, Is.LessThanOrEqualTo(0));
            Assert.That(victim.Dead, Is.True);
        }

        [Test]
        public void LightWeaponLanding_RepeatedAfterWarmup_DoesNotAllocate()
        {
            var data = new LF2CharacterData
            {
                name = "LandingAllocationProbe",
                type_sub = (int)LF2ObjectType.LightWeapon,
                weapon_drop_hurt = 1,
                weapon_drop_sound = "SFX_DROP_ALLOCATION_PROBE",
            };
            data.frames.Add(new LF2FrameData
            {
                frameId = 0,
                state = LF2States.WeaponThrowing,
                wait = 10000,
                next = 0,
                centerx = 20,
                centery = 20,
            });
            data.frames.Add(new LF2FrameData
            {
                frameId = 7,
                state = LF2States.WeaponInSky,
                wait = 10000,
                next = 7,
                centerx = 20,
                centery = 20,
            });

            var weapon = new AllocationProbeWeapon
            {
                ObjectId = 100,
            };
            weapon.SetWeaponType((int)LF2ObjectType.LightWeapon);
            weapon.FrameCache.Load(new LF2CharacterDataWrapper(weapon.ObjectId, data));
            var world = new SimulationWorld();
            world.Register(weapon);

            PrepareLandingIteration(world, weapon);
            Assert.That(weapon.InvokeCurrentDatLanding(
                (int)LF2ObjectType.LightWeapon,
                weapon.Frame.D,
                12.0,
                crossedGround: true), Is.True);

            bool allLandingsAccepted = true;
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int iteration = 0; iteration < 256; iteration++)
            {
                PrepareLandingIteration(world, weapon);
                if (!weapon.InvokeCurrentDatLanding(
                        (int)LF2ObjectType.LightWeapon,
                        weapon.Frame.D,
                        12.0,
                        crossedGround: true))
                {
                    allLandingsAccepted = false;
                }
            }
            long allocatedBytes =
                GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocatedBytes, Is.Zero);
            Assert.That(allLandingsAccepted, Is.True);
            Assert.That(weapon.Frame.N, Is.EqualTo(7));
            Assert.That(world.PendingSounds, Has.Count.EqualTo(1));
        }

        [Test]
        public void OtherObjectFrameTransit_QueuesMatchOwnedSoundWithoutAllocating()
        {
            var data = new LF2CharacterData
            {
                name = "OtherObjectFrameSoundAllocationProbe",
                type_sub = (int)LF2ObjectType.Other,
            };
            data.frames.Add(new LF2FrameData
            {
                frameId = 0,
                state = LF2States.Standing,
                wait = 10000,
                next = 0,
                sound = "SFX_OTHER_FRAME_0",
            });
            data.frames.Add(new LF2FrameData
            {
                frameId = 1,
                state = LF2States.Standing,
                wait = 10000,
                next = 1,
                sound = "SFX_OTHER_FRAME_1",
            });

            var entity = new LF2OtherObject();
            entity.FrameCache.Load(new LF2CharacterDataWrapper(999, data));
            entity.ImmediateFrame(0);
            entity.Runtime.SetPosition(123.0, 0.0, 0.0);
            entity.Runtime.SyncIntegerPosition();
            var world = new SimulationWorld();
            world.Register(entity);

            entity.OnFrameTransit(1, switchDirAfterTrans: false);
            world.PendingSounds.Clear();

            bool allEventsQueued = true;
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int iteration = 0; iteration < 256; iteration++)
            {
                world.PendingSounds.Clear();
                entity.OnFrameTransit(iteration & 1, switchDirAfterTrans: false);
                if (world.PendingSounds.Count != 1)
                    allEventsQueued = false;
            }
            long allocatedBytes =
                GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocatedBytes, Is.Zero);
            Assert.That(allEventsQueued, Is.True);
            Assert.That(world.PendingSounds[0].WorldX, Is.EqualTo(123));
        }

        [Test]
        public void SealedWorld_InvalidRegistrationDiagnostics_DoNotAllocate()
        {
            var world = new SimulationWorld();
            var registered = new LF2Character();
            var missing = new LF2Character();
            world.Register(registered);
            world.RuntimeCapacity.Seal();

            world.Register(null);
            world.Unregister(null);
            world.Register(registered);
            world.Unregister(missing);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int iteration = 0; iteration < 256; iteration++)
            {
                world.Register(null);
                world.Unregister(null);
                world.Register(registered);
                world.Unregister(missing);
            }
            long allocatedBytes =
                GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocatedBytes, Is.Zero);
            Assert.That(
                world.NullRegistrationRejectCountForDiagnostics,
                Is.EqualTo(257));
            Assert.That(
                world.DuplicateRegistrationRejectCountForDiagnostics,
                Is.EqualTo(257));
            Assert.That(
                world.MissingUnregisterCountForDiagnostics,
                Is.EqualTo(514));
        }

        [Test]
        public void SealedWorld_RuntimeSlotExhaustion_DoesNotAllocate()
        {
            var world = new SimulationWorld(
                BattleRuntimeProfile.DesktopExtended,
                51);
            var occupant = new LF2OtherObject();
            var rejected = new LF2OtherObject();
            world.Register(occupant);
            Assert.That(occupant.Runtime.SlotIndex, Is.EqualTo(50));
            world.RuntimeCapacity.Seal();

            world.Register(rejected);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int iteration = 0; iteration < 256; iteration++)
                world.Register(rejected);
            long allocatedBytes =
                GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocatedBytes, Is.Zero);
            Assert.That(rejected.Runtime.SlotIndex, Is.EqualTo(-1));
            Assert.That(
                world.RuntimeSlotCapacityRejectCountForDiagnostics,
                Is.EqualTo(257));
            Assert.That(world.RuntimeCapacity.RejectedGrowthCount, Is.EqualTo(257));
        }

        [Test]
        public void SealedWorld_InvalidFrameAndInitTaskDiagnostics_DoNotAllocate()
        {
            var world = new SimulationWorld();
            var character = new LF2Character();
            var other = new LF2OtherObject();
            var specialAttack = new LF2SpecialAttack();
            var weapon = new LF2Weapon();
            world.Register(character);
            world.Register(other);
            world.Register(specialAttack);
            world.Register(weapon);
            world.RuntimeCapacity.Seal();

            bool previousStateLogEnabled = NTSD.Tools.Log.StateLogEnabled;
            NTSD.Tools.Log.StateLogEnabled = true;
            try
            {
                character.OnFrameTransit(999999, switchDirAfterTrans: false);
                other.Init(null, null);
                specialAttack.Init(null, null);
                weapon.Init(null, null);

                _ = GC.GetAllocatedBytesForCurrentThread();
                long before = GC.GetAllocatedBytesForCurrentThread();
                for (int iteration = 0; iteration < 256; iteration++)
                {
                    character.OnFrameTransit(999999, switchDirAfterTrans: false);
                    other.Init(null, null);
                    specialAttack.Init(null, null);
                    weapon.Init(null, null);
                }
                long allocatedBytes =
                    GC.GetAllocatedBytesForCurrentThread() - before;

                Assert.That(allocatedBytes, Is.Zero);
            }
            finally
            {
                NTSD.Tools.Log.StateLogEnabled = previousStateLogEnabled;
            }

            Assert.That(
                character.InvalidFrameTransitionCountForDiagnostics,
                Is.EqualTo(257));
            Assert.That(
                other.InvalidInitTaskTypeCountForDiagnostics,
                Is.EqualTo(257));
            Assert.That(
                specialAttack.InvalidInitTaskTypeCountForDiagnostics,
                Is.EqualTo(257));
            Assert.That(
                weapon.InvalidInitTaskTypeCountForDiagnostics,
                Is.EqualTo(257));
        }

        [Test]
        public void SealedStageSpawnBufferPool_RejectsWithoutAllocating()
        {
            var pool = new StageSpawnRuntimeBufferPool();
            var targetTotal = new List<int>();
            var entryCount = new List<int>();
            var spawnedTotal = new List<int>();
            var activeSlots = new List<int[]>();
            pool.Prepare(
                null,
                targetTotal,
                entryCount,
                spawnedTotal,
                activeSlots);

            Assert.That(pool.Rent(), Is.Null);

            bool allRejected = true;
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int iteration = 0; iteration < 256; iteration++)
            {
                if (pool.Rent() != null)
                    allRejected = false;
            }
            long allocatedBytes =
                GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocatedBytes, Is.Zero);
            Assert.That(allRejected, Is.True);
            Assert.That(pool.RejectedRentCountForDiagnostics, Is.EqualTo(257));
        }

        [Test]
        public void SealedCollisionCandidateListPool_RejectsWithoutAllocating()
        {
            var data = new LF2CharacterData
            {
                name = "CandidateListCapacityProbe",
                type_sub = (int)LF2ObjectType.Character,
            };
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = LF2States.Standing,
                wait = 10000,
                next = 0,
                centerx = 39,
                centery = 79,
            };
            frame.itrs.Add(new InteractionArea
            {
                kind = 0,
                x = 0,
                y = 0,
                w = 40,
                h = 40,
            });
            data.frames.Add(frame);

            var attacker = new LF2Character();
            attacker.FrameCache.Load(new LF2CharacterDataWrapper(1, data));
            attacker.ImmediateFrame(0);
            var world = new SimulationWorld();
            world.Register(attacker);
            world.RuntimeCapacity.Seal();
            var query = (BruteForceSceneQuery)world.SceneQuery;

            query.CollectCollisionCandidates();

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int iteration = 0; iteration < 256; iteration++)
                query.CollectCollisionCandidates();
            long allocatedBytes =
                GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocatedBytes, Is.Zero);
            Assert.That(query.FormalCollectionAborted, Is.True);
            Assert.That(
                query.CandidateListRejectedRentCountForDiagnostics,
                Is.EqualTo(257));
            Assert.That(query.ActiveCandidateListCountForDiagnostics, Is.Zero);
        }

        [Test]
        public void SealedOpointTaskPools_RejectExhaustionWithoutAllocating()
        {
            LF2ReferencePool pool = LF2ReferencePool.Instance;
            bool restoreSealedState = pool.IsBattleCapacitySealed;
            pool.UnsealBattleCapacity();
            pool.PrewarmTasks<OPointCreateTask>(8);
            pool.PrewarmTasks<OPointCreateMultipleTask>(8);

            int singleCount = pool.AvailableCreateTaskCountForDiagnostics;
            int multipleCount =
                pool.AvailableCreateMultipleTaskCountForDiagnostics;
            var singles = new OPointCreateTask[singleCount];
            var multiples = new OPointCreateMultipleTask[multipleCount];

            try
            {
                pool.SealBattleCapacity();
                for (int index = 0; index < singleCount; index++)
                    singles[index] = pool.Fetch<OPointCreateTask>();
                for (int index = 0; index < multipleCount; index++)
                    multiples[index] = pool.Fetch<OPointCreateMultipleTask>();

                long rejectedBefore = pool.RejectedTaskFetchCount;
                bool allRejected = true;
                _ = GC.GetAllocatedBytesForCurrentThread();
                long before = GC.GetAllocatedBytesForCurrentThread();
                for (int iteration = 0; iteration < 256; iteration++)
                {
                    if (pool.Fetch<OPointCreateTask>() != null ||
                        pool.Fetch<OPointCreateMultipleTask>() != null)
                    {
                        allRejected = false;
                    }
                }
                long allocatedBytes =
                    GC.GetAllocatedBytesForCurrentThread() - before;

                Assert.That(allocatedBytes, Is.Zero);
                Assert.That(allRejected, Is.True);
                Assert.That(
                    pool.RejectedTaskFetchCount - rejectedBefore,
                    Is.EqualTo(512));
            }
            finally
            {
                for (int index = 0; index < singleCount; index++)
                    pool.Recycle(singles[index]);
                for (int index = 0; index < multipleCount; index++)
                    pool.Recycle(multiples[index]);

                if (!restoreSealedState)
                    pool.UnsealBattleCapacity();
            }
        }

        [Test]
        public void UnloadedGameDataQueries_UseOwnedEmptyBuffersWithoutAllocatingOrLogging()
        {
            GameDataManager manager = GameDataManager.Instance;
            Assert.That(manager, Is.Not.Null);

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            FieldInfo cachedConfigField = typeof(GameDataManager).GetField(
                "cachedConfig",
                flags);
            FieldInfo objectLookupField = typeof(GameDataManager).GetField(
                "objectLookup",
                flags);
            Assert.That(cachedConfigField, Is.Not.Null);
            Assert.That(objectLookupField, Is.Not.Null);

            object originalConfig = cachedConfigField.GetValue(manager);
            object originalLookup = objectLookupField.GetValue(manager);
            try
            {
                cachedConfigField.SetValue(manager, null);
                objectLookupField.SetValue(manager, null);

                List<ObjectDefinition> firstEmptyObjects = manager.GetAllObjects();
                List<ObjectDefinition> firstEmptyType = manager.GetObjectsByType(0);
                manager.GetObjectById(0);
                manager.GetBackgroundById(0);

                bool reusedOwnedBuffers = true;
                _ = GC.GetAllocatedBytesForCurrentThread();
                long before = GC.GetAllocatedBytesForCurrentThread();
                for (int iteration = 0; iteration < 256; iteration++)
                {
                    reusedOwnedBuffers &= ReferenceEquals(
                        firstEmptyObjects,
                        manager.GetAllObjects());
                    reusedOwnedBuffers &= ReferenceEquals(
                        firstEmptyType,
                        manager.GetObjectsByType(iteration));
                    manager.GetObjectById(iteration);
                    manager.GetBackgroundById(iteration);
                }
                long allocatedBytes =
                    GC.GetAllocatedBytesForCurrentThread() - before;

                Assert.That(allocatedBytes, Is.Zero);
                Assert.That(reusedOwnedBuffers, Is.True);
            }
            finally
            {
                cachedConfigField.SetValue(manager, originalConfig);
                objectLookupField.SetValue(manager, originalLookup);
            }
        }

        private static LF2CharacterData BuildStandardHitData()
        {
            var data = new LF2CharacterData
            {
                name = "StandardHitAllocationProbe",
                type_sub = (int)LF2ObjectType.Character,
                weapon_hit_sound = "SFX_HIT_ALLOCATION_PROBE",
            };
            data.frames.Add(new LF2FrameData
            {
                frameId = 0,
                state = LF2States.Standing,
                wait = 10000,
                next = 0,
                centerx = 39,
                centery = 79,
            });
            for (int frameId = LF2StandardFrames.Injured;
                 frameId <= LF2StandardFrames.Injured9;
                 frameId++)
            {
                data.frames.Add(new LF2FrameData
                {
                    frameId = frameId,
                    state = LF2States.Injured,
                    wait = 10000,
                    next = frameId,
                    centerx = 39,
                    centery = 79,
                });
            }

            data.frames.Add(new LF2FrameData
            {
                frameId = LF2StandardFrames.FallingFront,
                state = LF2States.Falling,
                wait = 10000,
                next = LF2StandardFrames.FallingFront,
                centerx = 39,
                centery = 79,
            });
            data.frames.Add(new LF2FrameData
            {
                frameId = LF2StandardFrames.FallingBack,
                state = LF2States.Falling,
                wait = 10000,
                next = LF2StandardFrames.FallingBack,
                centerx = 39,
                centery = 79,
            });
            return data;
        }

        private static void PrepareStandardHitIteration(
            SimulationWorld world,
            AllocationProbeCharacter attacker,
            AllocationProbeCharacter victim)
        {
            world.PendingSounds.Clear();
            attacker.ImmediateFrame(0);
            attacker.AttackExempt = 0;
            attacker.FrameDelay = 0;
            attacker.AttackingCounter = 0;
            attacker.ClearSparkRecords();
            attacker.ItrRest.Reset();
            victim.ImmediateFrame(0);
            victim.Health.HP = 10000;
            victim.Health.HPBound = 10000;
            victim.Health.HPLost = 0;
            victim.FallCounter = 0;
            victim.HitStateCount = 0;
            victim.HitCount = 0;
            victim.FrameDelay = 0;
            victim.AttackingCounter = 0;
            victim.KnockbackVx = 0.0;
            victim.KnockbackVy = 0.0;
            victim.KnockbackVz = 0.0;
            victim.Runtime.SetPosition(0.0, 0.0, 0.0);
            victim.Runtime.SetVelocity(0.0, 0.0, 0.0);
            victim.Runtime.SyncIntegerPosition();
            victim.ClearSparkRecords();
            victim.ItrRest.Reset();
        }

        private static void PrepareLethalHitIteration(
            SimulationWorld world,
            AllocationProbeCharacter attacker,
            AllocationProbeCharacter victim)
        {
            PrepareStandardHitIteration(world, attacker, victim);
            victim.Health.HP = 1;
            victim.Health.HPBound = 1;
            victim.Dead = false;
        }

        private static void PrepareHeldWeaponThrowIteration(
            LF2Character holder,
            AllocationProbeWeapon weapon)
        {
            holder.ImmediateFrame(0);
            holder.Runtime.Dir = "right";
            holder.Runtime.KeyUp = 0;
            holder.Runtime.KeyDown = 0;
            weapon.ImmediateFrame(0);
            weapon.Runtime.Dir = "right";
            weapon.Runtime.SetVelocity(0.0, 0.0, 0.0);
            holder.HoldWeapon(weapon);
        }

        private static void PrepareLandingIteration(
            SimulationWorld world,
            AllocationProbeWeapon weapon)
        {
            world.PendingSounds.Clear();
            weapon.ImmediateFrame(0);
            weapon.Runtime.Dir = "right";
            weapon.Runtime.SetPosition(0.0, -1.0, 0.0);
            weapon.Runtime.SetVelocity(8.0, 12.0, 0.0);
            weapon.Runtime.WeaponFlightCounter = 1000;
            weapon.Runtime.SyncIntegerPosition();
        }

        private static void PrepareBurningLandingIteration(
            AllocationProbeWeapon weapon)
        {
            weapon.ImmediateFrame(0);
            weapon.Runtime.Dir = "right";
            weapon.Runtime.SetPosition(0.0, 0.0, 0.0);
            weapon.Runtime.SetVelocity(10.0, 18.0, 0.0);
            weapon.Runtime.SyncIntegerPosition();
        }

        private sealed class AllocationProbeCharacter : LF2Character
        {
            public void ClearSparkRecords()
            {
                ResetSpark();
            }

            public void InvokeRunTUCore()
            {
                RunTUCore();
            }
        }

        private sealed class AllocationProbeWeapon : LF2Weapon
        {
            public WeaponAttackResult InvokeProcessAttack(
                LF2Entity holder,
                WeaponPoint wpoint,
                LF2FrameData frame)
            {
                return base.ProcessAttack(holder, wpoint, frame);
            }

            public bool InvokeCurrentDatLanding(
                int dataType,
                LF2FrameData frame,
                double landingVy,
                bool crossedGround)
            {
                return ApplyCurrentDatNonCharacterLanding(
                    dataType,
                    frame,
                    landingVy,
                    crossedGround);
            }

            public void InvokeOnLanded()
            {
                base.OnLanded();
            }
        }

        private static void AssertOwnedComponents(
            LF2Entity entity,
            object physics,
            object frame,
            object effect,
            object rest,
            object sprite,
            object transistor)
        {
            Assert.That(entity.PS, Is.SameAs(physics));
            Assert.That(entity.Frame, Is.SameAs(frame));
            Assert.That(entity.Effect, Is.SameAs(effect));
            Assert.That(entity.ItrRest, Is.SameAs(rest));
            Assert.That(entity.Sprite, Is.SameAs(sprite));
            Assert.That(entity.Trans, Is.SameAs(transistor));
        }
    }
}
