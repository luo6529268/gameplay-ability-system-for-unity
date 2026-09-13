using System;
using System.Collections.Generic;

namespace NTSD.Simulation
{
    public static class NTSD28UnityEntityRawCapture
    {
        public const string Schema = "ntsd28-unity-entity-raw-capture-v1";
        public const int FieldCount = 49;
        public const int VerifiedBindingCount = 43;

        public static readonly string[] CandidateBindings =
        {
        };

        public static readonly string[] MissingBindings =
        {
            "combat.runtimeStateCode",
            "combat.platformSourceSlot",
            "combat.environmentState",
            "combat.environmentSourceSlot",
            "lifecycle.resolutionPending",
            "lifecycle.code",
        };

        public static string CaptureTickJson(
            SimulationWorld world,
            int completedTick)
        {
            return BattleCanonicalJson.Serialize(
                CaptureTickObject(world, completedTick));
        }

        public static object CaptureTickObject(
            SimulationWorld world,
            int completedTick)
        {
            if (world == null)
                throw new ArgumentNullException(nameof(world));
            if (completedTick <= 0)
                throw new ArgumentOutOfRangeException(nameof(completedTick));

            int capacity = world.RuntimeSlotCapacityForDiagnostics;
            var entities = new List<object>(world.ObjectCount);
            for (int slot = 0; slot < capacity; slot++)
            {
                if (!world.TryGetRuntimeSlotReadOnlyViewForDiagnostics(
                        slot,
                        out RuntimeSlotTable.ReadOnlySlotView view) ||
                    !view.Claimed)
                {
                    continue;
                }

                NTSDEntityRuntime runtime = view.Entity?.Runtime;
                if (view.Entity == null || runtime == null)
                {
                    throw new InvalidOperationException(
                        $"Claimed runtime slot {slot} has no live entity runtime.");
                }
                if (view.AllocationEpoch == 0)
                {
                    throw new InvalidOperationException(
                        $"Claimed runtime slot {slot} has allocation epoch zero.");
                }

                entities.Add(ProjectEntity(view, runtime));
            }

            return DictionaryOf(
                ("bindingStatus", (object)DictionaryOf(
                    ("candidate", (object)CandidateBindings),
                    ("missing", (object)MissingBindings),
                    ("verifiedCount", VerifiedBindingCount),
                    ("candidateCount", CandidateBindings.Length),
                    ("missingCount", MissingBindings.Length),
                    ("fieldCount", FieldCount))),
                ("certificateEligible", false),
                ("completedTick", completedTick),
                ("entities", entities.ToArray()),
                ("evidenceClass", "UNITY_RAW_BINDING_DIAGNOSTIC_ONLY"),
                ("kind", "tick"),
                ("schema", Schema),
                ("slotCapacity", capacity));
        }

        private static object ProjectEntity(
            RuntimeSlotTable.ReadOnlySlotView view,
            NTSDEntityRuntime runtime)
        {
            return DictionaryOf(
                ("active", true),
                ("allocationEpoch", view.AllocationEpoch),
                ("combat", (object)DictionaryOf(
                    ("armorRecoveryTimer", runtime.ArmorRecoveryTimer11C),
                    ("attackerRest", runtime.AttackExempt),
                    ("bdefendAccumulator", runtime.Bdefend),
                    ("collisionYReference", runtime.CollisionYReference),
                    ("environmentSourceSlot", null),
                    ("environmentState", null),
                    ("hitReactionTimer", runtime.Fall),
                    ("motionHoldTimer", runtime.FrameDelay),
                    ("platformSourceSlot", null),
                    ("renderPhase", runtime.HitStop),
                    ("runtimeArmorHp", runtime.RuntimeArmorHp118),
                    ("runtimeStateCode", null),
                    ("specialHitLatch0eb", runtime.SpecialHitLatch0EB),
                    ("weaponHp", runtime.WeaponFlightCounter))),
                ("frame", (object)DictionaryOf(
                    ("action", runtime.Frame),
                    ("actionLatch", runtime.WaitCounter),
                    ("facingLeft", runtime.IsFacingLeft),
                    ("frameCounter", runtime.AttackingCounter),
                    ("frameState", view.Entity.Frame?.D?.state ?? 0),
                    ("previousAction", view.Entity.Frame?.Prev ?? 0),
                    ("tickActionSnapshot", runtime.PrevFrame2))),
                ("identity", (object)DictionaryOf(
                    ("battleGroup", runtime.RelationTeam),
                    ("controlSlot", runtime.AnimCounter),
                    ("objectId", runtime.ObjectId),
                    ("objectType", runtime.ObjType),
                    ("ownerSlot", runtime.OwnerSlotIndex),
                    ("participantClass", runtime.Unk344))),
                ("lifecycle", (object)DictionaryOf(
                    ("code", null),
                    ("resolutionPending", null))),
                ("motion", (object)DictionaryOf(
                    ("x", runtime.Vx),
                    ("y", runtime.Vy),
                    ("z", runtime.Vz))),
                ("position", (object)DictionaryOf(
                    ("preciseX", runtime.X),
                    ("preciseY", runtime.Y),
                    ("preciseZ", runtime.Z),
                    ("x", runtime.XInt),
                    ("y", runtime.YInt),
                    ("z", runtime.ZInt))),
                ("slot", view.RuntimeSlot),
                ("vitals", (object)DictionaryOf(
                    ("baseMaxHp", runtime.HP3),
                    ("baseMaxMp", runtime.MPMax),
                    ("currentHp", runtime.HP),
                    ("currentMp", runtime.PP),
                    ("effectiveMaxHp", runtime.HPBound),
                    ("reviveLives", runtime.HP2Orig),
                    ("reviveNextHp", runtime.RespawnCount),
                    ("reviveNextLives", runtime.HPOrig))));
        }

        private static SortedDictionary<string, object> DictionaryOf(
            params (string Key, object Value)[] values)
        {
            var result = new SortedDictionary<string, object>(
                StringComparer.Ordinal);
            for (int index = 0; index < values.Length; index++)
                result.Add(values[index].Key, values[index].Value);
            return result;
        }
    }
}
