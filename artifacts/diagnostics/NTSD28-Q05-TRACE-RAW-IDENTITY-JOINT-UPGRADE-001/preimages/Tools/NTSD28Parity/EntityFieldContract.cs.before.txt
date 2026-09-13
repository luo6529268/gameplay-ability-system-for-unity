namespace NTSD28Parity;

internal static class EntityFieldContract
{
    internal const string VerifiedBinding = "VERIFIED";
    internal const string CandidateBinding = "CANDIDATE";
    internal const string MissingBinding = "MISSING";
    internal const string StrictComparison = "STRICT";

    internal static readonly string[] RootProperties =
    [
        "slot",
        "allocationEpoch",
        "active",
        "identity",
        "frame",
        "position",
        "motion",
        "vitals",
        "combat",
        "lifecycle",
    ];

    internal static readonly IReadOnlyDictionary<string, string[]> GroupProperties =
        new SortedDictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["identity"] =
            [
                "objectId",
                "objectType",
                "controlSlot",
                "ownerSlot",
                "battleGroup",
                "participantClass",
            ],
            ["frame"] =
            [
                "action",
                "actionLatch",
                "previousAction",
                "tickActionSnapshot",
                "frameCounter",
                "frameState",
                "facingLeft",
            ],
            ["position"] =
            [
                "x",
                "y",
                "z",
                "preciseX",
                "preciseY",
                "preciseZ",
            ],
            ["motion"] =
            [
                "x",
                "y",
                "z",
            ],
            ["vitals"] =
            [
                "currentHp",
                "effectiveMaxHp",
                "baseMaxHp",
                "currentMp",
                "baseMaxMp",
                "reviveLives",
                "reviveNextLives",
                "reviveNextHp",
            ],
            ["combat"] =
            [
                "runtimeStateCode",
                "renderPhase",
                "attackerRest",
                "collisionYReference",
                "platformSourceSlot",
                "hitReactionTimer",
                "bdefendAccumulator",
                "runtimeArmorHp",
                "armorRecoveryTimer",
                "motionHoldTimer",
                "weaponHp",
                "specialHitLatch0eb",
                "environmentState",
                "environmentSourceSlot",
            ],
            ["lifecycle"] =
            [
                "resolutionPending",
                "code",
            ],
        };

    internal static readonly EntityFieldDescriptor[] Fields =
    [
        Field("slot", "int32", "EntityState28::slot", "NTSDEntityRuntime::SlotIndex", VerifiedBinding),
        Field("allocationEpoch", "int64", "EntityState28::presentation_generation normalized as a positive per-slot lifetime epoch", "RuntimeEntityHandle::Generation normalized as a positive per-slot lifetime epoch", VerifiedBinding),
        Field("active", "boolean", "BattleWorld28::entity(slot) != nullptr", "RuntimeSlotTable claimed handle resolves to an entity", VerifiedBinding),

        Field("identity.objectId", "int32", "EntityState28::object_id", "NTSDEntityRuntime::ObjectId", VerifiedBinding),
        Field("identity.objectType", "int32", "EntityState28::object_type", "NTSDEntityRuntime::ObjType", VerifiedBinding),
        Field("identity.controlSlot", "int32", "EntityState28::control_slot_000 (shared locomotion/association field, not player ownership)", "NTSDEntityRuntime::AnimCounter", VerifiedBinding),
        Field("identity.ownerSlot", "int32", "EntityState28::owner_slot", "NTSDEntityRuntime::OwnerSlotIndex", VerifiedBinding),
        Field("identity.battleGroup", "int32", "EntityState28::battle_group", "NTSDEntityRuntime::RelationTeam", VerifiedBinding),
        Field("identity.participantClass", "int32", "EntityState28::participant_class_344", "NTSDEntityRuntime::Unk344", VerifiedBinding),

        Field("frame.action", "int32", "EntityState28::frame.action", "NTSDEntityRuntime::Frame", VerifiedBinding),
        Field("frame.actionLatch", "int32", "EntityState28::frame.action_latch", "NTSDEntityRuntime::WaitCounter / FrameTransistor::WaitCounter", VerifiedBinding),
        Field("frame.previousAction", "int32", "EntityState28::frame.previous_action_078 (per-entity post-frame tail)", "LF2FrameInfo::Prev (late-tail mirror)", VerifiedBinding),
        Field("frame.tickActionSnapshot", "int32", "EntityState28::frame.tick_action_snapshot (pre-pair-geometry snapshot)", "NTSDEntityRuntime::PrevFrame2 (collision snapshot)", VerifiedBinding),
        Field("frame.frameCounter", "int32", "EntityState28::frame.frame_counter", "NTSDEntityRuntime::AttackingCounter", VerifiedBinding),
        Field("frame.frameState", "int32", "current EntityState28 frame definition state", "NTSDEntityRuntime::FrameState", VerifiedBinding),
        Field("frame.facingLeft", "boolean", "EntityState28::frame.facing", "NTSDEntityRuntime::IsFacingLeft", VerifiedBinding),

        Field("position.x", "int32", "EntityState28::position.x", "NTSDEntityRuntime::XInt", VerifiedBinding),
        Field("position.y", "int32", "EntityState28::position.y", "NTSDEntityRuntime::YInt", VerifiedBinding),
        Field("position.z", "int32", "EntityState28::position.z", "NTSDEntityRuntime::ZInt", VerifiedBinding),
        Field("position.preciseX", "float64", "EntityState28::position.precise_x", "NTSDEntityRuntime::X", VerifiedBinding),
        Field("position.preciseY", "float64", "EntityState28::position.precise_y", "NTSDEntityRuntime::Y", VerifiedBinding),
        Field("position.preciseZ", "float64", "EntityState28::position.precise_z", "NTSDEntityRuntime::Z", VerifiedBinding),

        Field("motion.x", "float64", "EntityState28::motion.x", "NTSDEntityRuntime::Vx", VerifiedBinding),
        Field("motion.y", "float64", "EntityState28::motion.y", "NTSDEntityRuntime::Vy", VerifiedBinding),
        Field("motion.z", "float64", "EntityState28::motion.z", "NTSDEntityRuntime::Vz", VerifiedBinding),

        Field("vitals.currentHp", "int32", "EntityState28::current_hp", "NTSDEntityRuntime::HP", VerifiedBinding),
        Field("vitals.effectiveMaxHp", "int32", "EntityState28::effective_max_hp", "NTSDEntityRuntime::HPBound", VerifiedBinding),
        Field("vitals.baseMaxHp", "int32", "EntityState28::base_max_hp", "NTSDEntityRuntime::HP3", VerifiedBinding),
        Field("vitals.currentMp", "int32", "EntityState28::current_mp", "NTSDEntityRuntime::PP / LF2Health::PP", VerifiedBinding),
        Field("vitals.baseMaxMp", "int32", "EntityState28::base_max_mp", "NTSDEntityRuntime::MPMax", VerifiedBinding),
        Field("vitals.reviveLives", "int32", "EntityState28::revive_lives_30c", "NTSDEntityRuntime::HP2Orig", VerifiedBinding),
        Field("vitals.reviveNextLives", "int32", "EntityState28::revive_next_lives_310", "NTSDEntityRuntime::HPOrig", VerifiedBinding),
        Field("vitals.reviveNextHp", "int32", "EntityState28::revive_next_hp_314", "NTSDEntityRuntime::RespawnCount", VerifiedBinding),

        Field("combat.runtimeStateCode", "int32", "EntityState28::runtime_state_code", null, MissingBinding),
        Field("combat.renderPhase", "int32", "EntityState28::render_phase_008", "NTSDEntityRuntime::HitStop / LF2Entity::HitStun", VerifiedBinding),
        Field("combat.attackerRest", "int32", "EntityState28::attacker_rest", "NTSDEntityRuntime::AttackExempt", VerifiedBinding),
        Field("combat.collisionYReference", "int32", "EntityState28::collision_y_reference", "NTSDEntityRuntime::CollisionYReference", VerifiedBinding),
        Field("combat.platformSourceSlot", "int32", "EntityState28::platform_source_slot_f4", null, MissingBinding),
        Field("combat.hitReactionTimer", "int32", "EntityState28::hit_reaction_timer (20/40/60/80 reaction tier)", "NTSDEntityRuntime::Fall", VerifiedBinding),
        Field("combat.bdefendAccumulator", "int32", "EntityState28::bdefend_accumulator", "NTSDEntityRuntime::Bdefend", VerifiedBinding),
        Field("combat.runtimeArmorHp", "int32", "EntityState28::runtime_armor_hp", "NTSDEntityRuntime::RuntimeArmorHp118", VerifiedBinding),
        Field("combat.armorRecoveryTimer", "int32", "EntityState28::armor_recovery_timer", "NTSDEntityRuntime::ArmorRecoveryTimer11C", VerifiedBinding),
        Field("combat.motionHoldTimer", "int32", "EntityState28::motion_hold_timer", "NTSDEntityRuntime::FrameDelay", VerifiedBinding),
        Field("combat.weaponHp", "int32", "EntityState28::weapon_hp_31c", "NTSDEntityRuntime::WeaponFlightCounter", VerifiedBinding),
        Field("combat.specialHitLatch0eb", "boolean", "EntityState28::special_hit_latch_0eb", "NTSDEntityRuntime::SpecialHitLatch0EB", VerifiedBinding),
        Field("combat.environmentState", "int32", "EntityState28::environment_state_320", "none", MissingBinding),
        Field("combat.environmentSourceSlot", "int32", "EntityState28::environment_source_slot_160", null, MissingBinding),

        Field("lifecycle.resolutionPending", "boolean", "EntityState28::lifecycle_resolution_pending", "none", MissingBinding),
        Field("lifecycle.code", "int32", "EntityState28::lifecycle_code", null, MissingBinding),
    ];

    internal static object CreateDescriptor()
    {
        var groups = new SortedDictionary<string, object?>(StringComparer.Ordinal);
        foreach ((string group, string[] properties) in GroupProperties)
        {
            groups[group] = properties;
        }

        return new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["rootProperties"] = RootProperties,
            ["groups"] = groups,
            ["fields"] = Fields.Select(field => field.ToDescriptor()).ToArray(),
            ["fieldCount"] = Fields.Length,
            ["bindingStatusCounts"] = new SortedDictionary<string, int>(StringComparer.Ordinal)
            {
                [VerifiedBinding] = Fields.Count(field => field.BindingStatus == VerifiedBinding),
                [CandidateBinding] = Fields.Count(field => field.BindingStatus == CandidateBinding),
                [MissingBinding] = Fields.Count(field => field.BindingStatus == MissingBinding),
            },
            ["allComparisonPoliciesStrict"] = Fields.All(
                field => field.ComparisonPolicy == StrictComparison),
        };
    }

    internal static EntityFieldDescriptor FieldAtPath(string path)
    {
        return Fields.Single(field => string.Equals(
            field.Path,
            path,
            StringComparison.Ordinal));
    }

    private static EntityFieldDescriptor Field(
        string path,
        string jsonType,
        string authoritySource,
        string? unityBinding,
        string bindingStatus)
    {
        return new EntityFieldDescriptor(
            path,
            jsonType,
            authoritySource,
            unityBinding,
            bindingStatus,
            StrictComparison);
    }
}

internal sealed record EntityFieldDescriptor(
    string Path,
    string JsonType,
    string AuthoritySource,
    string? UnityBinding,
    string BindingStatus,
    string ComparisonPolicy)
{
    internal object ToDescriptor()
    {
        return new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["path"] = Path,
            ["jsonType"] = JsonType,
            ["authoritySource"] = AuthoritySource,
            ["unityBinding"] = UnityBinding,
            ["bindingStatus"] = BindingStatus,
            ["comparisonPolicy"] = ComparisonPolicy,
        };
    }
}
