#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class BattleEcsReadOnlyShadowEditorTests
    {
        [Test]
        public void SlotBitSet_EnumeratesSetSlotsInAuthorityOrder()
        {
            var bitSet = new BattleSlotBitSet(130);
            bitSet.Set(129);
            bitSet.Set(64);
            bitSet.Set(3);

            Assert.That(bitSet.Count, Is.EqualTo(3));
            Assert.That(bitSet.FindNextSet(0), Is.EqualTo(3));
            Assert.That(bitSet.FindNextSet(4), Is.EqualTo(64));
            Assert.That(bitSet.FindNextSet(65), Is.EqualTo(129));
            Assert.That(bitSet.FindNextSet(130), Is.EqualTo(-1));

            bitSet.Clear(64);
            Assert.That(bitSet.Count, Is.EqualTo(2));
            Assert.That(bitSet.FindNextSet(4), Is.EqualTo(129));
        }

        [Test]
        public void SparseSet_SwapRemoveMaintainsSlotLookupAndFixedCapacity()
        {
            var set = new BattleSparseSet<BattleEcsOptionalLink>(8, 2);
            Assert.That(set.AddOrSet(1, new BattleEcsOptionalLink(-1, 10, 2, 3, 4)), Is.True);
            Assert.That(set.AddOrSet(6, new BattleEcsOptionalLink(-2, 20, 5, 4, 3)), Is.True);
            Assert.That(set.AddOrSet(7, new BattleEcsOptionalLink(-3, 30, 1, 2, 3)), Is.False);

            Assert.That(set.Remove(1), Is.True);
            Assert.That(set.Contains(1), Is.False);
            Assert.That(set.TryGet(6, out BattleEcsOptionalLink moved), Is.True);
            Assert.That(moved.HolderStableId, Is.EqualTo(20));
            Assert.That(set.AddOrSet(7, new BattleEcsOptionalLink(-3, 30, 1, 2, 3)), Is.True);
        }

        [Test]
        public void Capture_CopiesCoreStoresMembershipAndAllRuntimeFingerprint()
        {
            LF2Character character = CreateCharacter(50, 101, 7);
            character.Runtime.SetPosition(123, 45, -67);
            character.Runtime.SyncIntegerPosition();
            character.Runtime.SetVelocity(3.5, -2.25, 1.75);
            character.Runtime.HP = 321;
            character.Runtime.PP = 88;
            character.Runtime.KeyAttack = 1;
            character.Runtime.ComboDja = 9;
            character.Runtime.InputHistory[5] = 77;
            character.Runtime.AiControlled = true;
            character.Runtime.LinkState = -1;
            character.Runtime.HolderStableId = 9001;
            character.Runtime.TargetSlotIndex = 52;

            var ecs = new BattleEcsWorld(
                new BattleEcsCapacityProfile(BattleRuntimeProfile.MobileExtended, 1050));
            ecs.BeginCapture(12, 8);
            ecs.CaptureSlot(50, true, 4, character);

            Assert.That(ecs.TryGetEntityView(50, out BattleEcsShadowEntityView view), Is.True);
            Assert.That(view.Handle, Is.EqualTo(new RuntimeEntityHandle(50, 4)));
            Assert.That(view.StableId, Is.EqualTo(101));
            Assert.That(view.ObjectId, Is.EqualTo(7));
            Assert.That(view.X, Is.EqualTo(123));
            Assert.That(view.Y, Is.EqualTo(45));
            Assert.That(view.Z, Is.EqualTo(-67));
            Assert.That(view.Vx, Is.EqualTo(3.5));
            Assert.That(view.Hp, Is.EqualTo(321));
            Assert.That(view.Pp, Is.EqualTo(88));
            Assert.That(view.TargetSlot, Is.EqualTo(52));
            Assert.That(view.Membership.HasFlag(BattleEcsMembership.Character), Is.True);
            Assert.That(view.Membership.HasFlag(BattleEcsMembership.Active), Is.True);
            Assert.That(view.Membership.HasFlag(BattleEcsMembership.HasAi), Is.True);
            Assert.That(view.Membership.HasFlag(BattleEcsMembership.HasHolder), Is.True);
            Assert.That(
                ecs.MatchesCanonicalSlot(50, true, 4, character, out BattleEcsShadowMismatchKind kind),
                Is.True);
            Assert.That(kind, Is.EqualTo(BattleEcsShadowMismatchKind.None));

            character.Runtime.HP--;
            Assert.That(
                ecs.MatchesCanonicalSlot(50, true, 4, character, out kind),
                Is.False);
            Assert.That(kind, Is.EqualTo(BattleEcsShadowMismatchKind.Vital));

            ecs.BeginCapture(13, 8);
            ecs.CaptureSlot(50, true, 4, character);
            Assert.That(
                ecs.MatchesCanonicalSlot(50, true, 4, character, out kind),
                Is.True);
        }

        [Test]
        public void Capture_DormantAndPendingSlotsAreNotActivePassMembers()
        {
            LF2Character character = CreateCharacter(50, 102, 8);
            var ecs = new BattleEcsWorld(
                new BattleEcsCapacityProfile(BattleRuntimeProfile.Authority400, 400));

            character.Runtime.OidMergeDormant = true;
            ecs.BeginCapture(1, 1);
            ecs.CaptureSlot(50, true, 2, character);
            BattleEcsMembership dormant = ecs.GetMembership(50);
            Assert.That(dormant.HasFlag(BattleEcsMembership.Dormant), Is.True);
            Assert.That(dormant.HasFlag(BattleEcsMembership.Active), Is.False);
            Assert.That(ecs.FindNextActiveSlot(0), Is.EqualTo(-1));

            character.Runtime.OidMergeDormant = false;
            character.Runtime.PendingFlushDestroy = true;
            ecs.BeginCapture(2, 2);
            ecs.CaptureSlot(50, true, 2, character);
            BattleEcsMembership pending = ecs.GetMembership(50);
            Assert.That(pending.HasFlag(BattleEcsMembership.PendingDestroy), Is.True);
            Assert.That(pending.HasFlag(BattleEcsMembership.Active), Is.False);
            Assert.That(ecs.FindNextActiveSlot(0), Is.EqualTo(-1));
        }

        [Test]
        public void WorldShadow_DetectsMutationAndCapturesSlotGenerationReuse()
        {
            var world = new SimulationWorld();
            LF2Character first = CreateCharacter(50, 201, 11);
            world.Register(first);
            Assert.That(
                world.TryGetCurrentRuntimeHandleForDiagnostics(50, first, out RuntimeEntityHandle oldHandle),
                Is.True);

            world.CaptureBattleEcsShadowForDiagnostics(1);
            Assert.That(world.ValidateBattleEcsShadowForDiagnostics(), Is.True);
            first.Runtime.XInt++;
            Assert.That(world.ValidateBattleEcsShadowForDiagnostics(), Is.False);
            Assert.That(
                world.BattleEcsShadowDiagnosticsForDiagnostics.FirstMismatchKind,
                Is.EqualTo(BattleEcsShadowMismatchKind.Motion));

            world.Unregister(first);
            LF2Character replacement = CreateCharacter(50, 202, 12);
            world.Register(replacement);
            Assert.That(
                world.TryGetCurrentRuntimeHandleForDiagnostics(
                    50,
                    replacement,
                    out RuntimeEntityHandle replacementHandle),
                Is.True);
            Assert.That(replacementHandle.Generation, Is.GreaterThan(oldHandle.Generation));

            world.CaptureBattleEcsShadowForDiagnostics(2);
            Assert.That(
                world.TryGetBattleEcsShadowEntityForDiagnostics(50, out BattleEcsShadowEntityView view),
                Is.True);
            Assert.That(view.Handle, Is.EqualTo(replacementHandle));
            Assert.That(view.Handle, Is.Not.EqualTo(oldHandle));
            Assert.That(world.ValidateBattleEcsShadowForDiagnostics(), Is.True);
        }

        [Test]
        public void TickHook_DefaultDisabledAndCompareModePublishesCleanReadOnlyShadow()
        {
            var world = new SimulationWorld();
            var tick = new NTSDBattleTickSystem(world);
            tick.RunReleaseTick(1, false);
            Assert.That(
                world.BattleEcsShadowDiagnosticsForDiagnostics.CaptureCount,
                Is.Zero,
                "U3 shadow must remain disabled by default");

            world.ConfigureBattleEcsShadowForDiagnostics(BattleEcsShadowMode.Compare);
            tick.RunReleaseTick(2, false);
            BattleEcsShadowDiagnostics diagnostics =
                world.BattleEcsShadowDiagnosticsForDiagnostics;
            Assert.That(diagnostics.CaptureCount, Is.EqualTo(1));
            Assert.That(diagnostics.SlotVisitCount, Is.EqualTo(400));
            Assert.That(diagnostics.ValidationCount, Is.EqualTo(1));
            Assert.That(diagnostics.IsClean, Is.True);
            Assert.That(diagnostics.CapturedTick, Is.EqualTo(2));
        }

        [Test]
        public void WarmedCaptureAndValidation_DoNotAllocateManagedMemory()
        {
            LF2Character character = CreateCharacter(50, 301, 13);
            var ecs = new BattleEcsWorld(
                new BattleEcsCapacityProfile(BattleRuntimeProfile.Authority400, 400));

            ecs.BeginCapture(1, 1);
            ecs.CaptureSlot(50, true, 2, character);
            Assert.That(
                ecs.MatchesCanonicalSlot(
                    50,
                    true,
                    2,
                    character,
                    out BattleEcsShadowMismatchKind warmupKind),
                Is.True);
            Assert.That(warmupKind, Is.EqualTo(BattleEcsShadowMismatchKind.None));

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int tick = 2; tick < 258; tick++)
            {
                ecs.BeginCapture(tick, 1);
                ecs.CaptureSlot(50, true, 2, character);
                if (!ecs.MatchesCanonicalSlot(
                        50,
                        true,
                        2,
                        character,
                        out BattleEcsShadowMismatchKind mismatchKind) ||
                    mismatchKind != BattleEcsShadowMismatchKind.None)
                {
                    Assert.Fail("Warmed ECS shadow parity failed.");
                }
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero,
                "U3 capture/compare hot path must reuse fixed-capacity storage");
        }

        [Test]
        public void Extended1000_FullSlotShadowIsOrderedExactAndAllocationFreeAfterWarmup()
        {
            const int capacity = 1050;
            const int firstSlot = 50;
            const int entityCount = 1000;
            var entities = new LF2Character[capacity];
            var generations = new uint[capacity];
            var ecs = new BattleEcsWorld(
                new BattleEcsCapacityProfile(BattleRuntimeProfile.MobileExtended, capacity));

            for (int slot = firstSlot; slot < capacity; slot++)
            {
                LF2Character entity = CreateCharacter(slot, 10000 + slot, 1 + (slot % 5));
                entity.Runtime.SetPosition(slot * 3, slot % 17, -(slot * 2));
                entity.Runtime.SyncIntegerPosition();
                entity.Runtime.AiControlled = true;
                entity.Runtime.HP = 500 - (slot % 31);
                entities[slot] = entity;
                generations[slot] = (uint)(1 + (slot % 7));
            }

            CaptureExtended1000(ecs, entities, generations, 1);
            Assert.That(ecs.ClaimedCount, Is.EqualTo(entityCount));
            Assert.That(ecs.FindNextActiveSlot(0), Is.EqualTo(firstSlot));
            int observed = 0;
            for (int slot = ecs.FindNextActiveSlot(0);
                 slot >= 0;
                 slot = ecs.FindNextActiveSlot(slot + 1))
            {
                Assert.That(
                    ecs.MatchesCanonicalSlot(
                        slot,
                        true,
                        generations[slot],
                        entities[slot],
                        out BattleEcsShadowMismatchKind kind),
                    Is.True,
                    $"slot={slot}, kind={kind}");
                observed++;
            }
            Assert.That(observed, Is.EqualTo(entityCount));

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            CaptureExtended1000(ecs, entities, generations, 2);
            bool clean = true;
            for (int slot = firstSlot; slot < capacity; slot++)
            {
                clean &= ecs.MatchesCanonicalSlot(
                    slot,
                    true,
                    generations[slot],
                    entities[slot],
                    out _);
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(clean, Is.True);
            Assert.That(allocated, Is.Zero,
                "the warmed 1000-slot ECS shadow must not allocate managed memory");
        }

        private static void CaptureExtended1000(
            BattleEcsWorld ecs,
            LF2Character[] entities,
            uint[] generations,
            int tickIndex)
        {
            ecs.BeginCapture(tickIndex, 1);
            for (int slot = 0; slot < entities.Length; slot++)
            {
                LF2Character entity = entities[slot];
                ecs.CaptureSlot(slot, entity != null, generations[slot], entity);
            }
        }

        private static LF2Character CreateCharacter(int slot, int stableId, int objectId)
        {
            var character = new LF2Character();
            character.Runtime.StableId = stableId;
            character.Runtime.ObjectId = objectId;
            character.Runtime.EntityType = (int)LF2ObjectType.Character;
            character.Runtime.Team = 1;
            character.Runtime.RelationTeam = 1;
            character.Runtime.Frame = 0;
            character.Frame.D = new LF2FrameData
            {
                frameId = 0,
                state = 0,
                wait = 100,
                next = 0,
            };
            character.Frame.PN = 0;
            character.Frame.N = 0;
            character.SetRequiredRuntimeSlot(slot);
            return character;
        }
    }
}
#endif
