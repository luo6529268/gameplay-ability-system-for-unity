<!-- CHANGE-RECORD
id: NTSD28-B6-NATIVE-IMPACT-ATOMIC-PRODUCTION-INTEGRATION-PRODUCTION-001
status: CODE_WRITTEN
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDatHitResolver.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterHitResolver.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Weapon.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/NTSD_Extensions/NTSDItrKindHandler.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6NativeImpactAtomicIntegrationEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type3WeaponFluteLegacyStatRetirementEditorTests.cs
authority: ??2026-09-10 Goal18???????playable impact??
evidence: RED_137_FAIL_57_PASS / CODE_WRITTEN / GREEN_PENDING
-->

# NTSD28-B6-NATIVE-IMPACT-ATOMIC-PRODUCTION-INTEGRATION-PRODUCTION-001

Goal18 / 2026-09-10 / PLANNED / TEST_FIRST????????????USER_HOLD????I1?I2?I3?

????????
- Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
- Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
- Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDatHitResolver.cs
- Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterHitResolver.cs
- Assets/NTSD/Scripts/Animation/LF2Objects/LF2Weapon.cs
- Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
- Assets/NTSD/Scripts/NTSD_Extensions/NTSDItrKindHandler.cs
- Assets/NTSD/Scripts/Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs
- Assets/NTSD/Scripts/Test/Editor/NTSD28B6NativeImpactAtomicIntegrationEditorTests.cs

Unity???HitPlan???environment/impact-source???impact?WeaponCount=-20??owner preflight?object3.0?????I1???carrier???TargetCatchSourceSlot90?I2???????I3???I1/I2 focused?????shared writer?

Authority???????hit_candidates134-256?battle_world4540-4580/5270-5309/5405-5531?game_session4171-4182???????impact????????EXE???playable?????????????????????????D-022 locked immutable?

???/?????impact?????HP/PP/rest/delay/legacy stats/WeaponCount/RNG???schema/NTSDSpec/Gen/Plugins/content/Scene???I1???entity snapshot?I2????owner??????????I3??writer???lifecycle owner??snapshot??????????????????

??????RED????I1?/?/slot0/high/sentinel???????/??????I2?kind?type/gate/owner/rule/respond/immunity/state/Y/motion/pending????????????????/RNG0?warmed0B?I3??dispatch???????focused????????B6/????/Goal17/refill9/full SelfCheck/?build0error/validator/Scene SHA?kind10/11 OID36??driver scoped Play?C++?seed/input/tick firstDifference=null???harness????kind17/18 fixture-only PLAY_NOT_PERFORMED_NO_PRODUCER?

???I1/I2??????I3?????????????diff?Scene?????schema/RNG/rest/delay/??????????b1b02287??2022.3.62f3/NTSD_Battle?????Library???git add/commit/push?

????????????????????Goal17????????????Git discard???????/???RED/focused/build/Play???Temp/Goal18_PrechangeBaseline.json???6404??FRAMING=1?????2022.3.62f3???Scene dirtyfalse?SHA??D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11?

## Authority?????battle_world.cpp5405-5531?
```cpp
    if (interaction->kind == 10 || interaction->kind == 11 ||
        interaction->kind == 17 || interaction->kind == 18) {
        // FUN_0042E100 @ 0x00430E36..0x00431873. Kind 11 reaches the
        // shared impact branch only while Entity28+0x320 is negative;
        // kind 17 admits character targets only, while kind 18 starts at the
        // special-object half of the same branch.
        if (interaction->kind == 11 && target->environment_state_320 >= 0) {
            result.message = "native kind-11 requires negative Entity28+0x320";
            return result;
        }
        const bool character_target = target->object_type == 0;
        if (interaction->kind == 17 && !character_target) {
            result.message = "native kind-17 ignores non-character targets";
            return result;
        }
        if (interaction->kind == 18 && character_target) {
            result.message = "native kind-18 ignores character targets";
            return result;
        }

        if (character_target) {
            const int first_owner_slot = attacker->owner_slot;
            const auto* first_owner = first_owner_slot >= 0
                                          ? entity(static_cast<std::size_t>(first_owner_slot))
                                          : nullptr;
            const int credit_slot = first_owner == nullptr
                                        ? -1
                                        : first_owner->owner_slot;
            if (credit_slot < 0 ||
                entity(static_cast<std::size_t>(credit_slot)) == nullptr) {
                result.status = WorldRelationHitStatus28::unsupported;
                result.message =
                    "native impact owner chain is unavailable for +0x90 encoding";
                return result;
            }
            const int configured_damage = rules.impact_environment_damage_94;
            target->environment_state_320 =
                -(configured_damage > 0 ? configured_damage : 20);

            // FUN_0044E430 follows attacker +0x354 twice, then stores the
            // resulting physical slot in target +0x90 using the 0x2000 tag.
            target->catch_source_slot_90 = 0x2000 + credit_slot;
            target->impact_source_slot_164 = static_cast<int>(attacker_slot);

            target->motion.x /= 1.07;
            target->motion.z /= 1.07;
            target->pending_hit_impulse.total.x = target->motion.x;
            target->pending_hit_impulse.total.z = target->motion.z;
            const int impact_action = interaction->respond == 0
                                          ? 182
                                          : interaction->respond;
            target->frame.action = impact_action;
            result.target_action = impact_action;
            result.target_horizontal_depth_damped = true;
        } else {
            const bool special_object_type = target->object_type == 1 ||
                                             target->object_type == 4 ||
                                             target->object_type == 6;
            const bool type2 = target->object_type == 2;
            if (!special_object_type && !type2) {
                result.message = "native impact kind ignores this target object type";
                return result;
            }
            if (special_object_type && !rules.kind10_immunity_table_audited) {
                result.status = WorldRelationHitStatus28::unsupported;
                result.message =
                    "alternate system-DAT weapon_flute_sky IDs are unaudited";
                return result;
            }
            if (special_object_type) {
                // 0x00431736 first tests DAT_004A7C4C. A zero count skips
                // the type-1/4/6 impact mutation altogether. The accepted
                // locked read-only runtime table has count two (201/202),
                // while an explicit alternate empty table keeps the native
                // zero-count suppression semantics.
                if (rules.kind10_immune_object_ids.empty()) {
                    result.message =
                        "empty native weapon_flute_sky table suppressed impact";
                    return result;
                }
                const bool listed =
                    std::find(rules.kind10_immune_object_ids.begin(),
                              rules.kind10_immune_object_ids.end(),
                              target->object_id) != rules.kind10_immune_object_ids.end();
                // Native REPNE SCASD enters the damping branch only when the
                // object ID is absent. A listed ID returns unchanged.
                if (listed) {
                    result.message =
                        "system-DAT weapon_flute_sky immunity suppressed impact";
                    return result;
                }
            }
            const int target_state = frame_state_or_zero(*target, target->frame.action);
            const int preserved_state = type2 ? 2000 : 1000;
            if (target_state != preserved_state) {
                target->frame.action = 0;
                result.target_action = 0;
            }
            target->motion.x /= 1.07;
            target->motion.z /= 1.07;
            target->pending_hit_impulse.total.x = target->motion.x;
            target->pending_hit_impulse.total.z = target->motion.z;
            result.target_horizontal_depth_damped = true;
        }

        // Both halves clamp the integer Y to -2 when it is at/below the
        // impact plane and start the precise Y velocity at -6. The later
        // descent adjustment differs: 3.0 for characters, 2.3 for objects.
        if (target->position.y >= -2) {
            target->position.y = -2;
            target->position.precise_y = -2.0;
            target->motion.y = -6.0;
        }
        if (target->motion.y > -6.0) {
            target->motion.y -= character_target ? 3.0 : 2.3;
            target->pending_hit_impulse.total.y = target->motion.y;
        }
        result.target_vertical_impact_applied = true;
        result.target_environment_state_after = target->environment_state_320;
        result.target_encoded_credit_slot_after = target->catch_source_slot_90;
        result.target_impact_source_slot_after = target->impact_source_slot_164;
        result.status = WorldRelationHitStatus28::applied;
        result.message = "native kind-10/11/17/18 impact response applied";
        return result;
    }
    if (interaction->kind == 6) {
        // FUN_0042E100 @ 0x0043056E / 0x00430573..0x0043057A.  This
```

I1 focused13?I2 focused147?0B????????I3 TEST_FIRST?shared BattleDamageWriter??runtime consumer?preflight builder?HitPlan?????builder/??????rule94=20?table201/202 private readonly???game_session.h396/404?D-022?????world/schema carrier?runner?dispatch?kind17/18??attack/disposition???state12?fall???10/11??????impact???????????????

I3?focused???concrete/generic/direct/legacy dispatch?owner???environment/WeaponCount???object immunity/Y?literal respond?captured??Shadow/DataOriented?????Goal18??XML??callback???Temp??????????????RED?

I3?????2????API????World?????Temp/Goal18_I3_TestCompile.txt????Unity??RED194?=137FAIL/57PASS?Temp/Goal18_I3_RED.xml?Result.json????/?kind???1bit??owner?WeaponCount???captured???RNG??????collector??????????????????RNG????????????

??????BattleDamageWriter CreateNativeImpactPlan/ResolveNativeImpactOwner/TryApplyNativeImpact?current-DAT early dispatch???Character resolver?LF2Weapon?impact????????impact??helper?LF2Entity?17/18????arest/vrest??kind service?DAT/HitPlan disposition?17/18?runner???dispatch?HitPlan???????plan???????/RNG/??carrier/schema????impact??WeaponCount?????????CODE_WRITTEN?fresh??/focused/Play???

??GREEN146/194?48?captured?????????owner/credit??bdy????????????????collector RNG???impact?????owner/credit??body???team???candidate???????RNG???collector??48/48????diag valid=true/failures0/mismatches0?Shadow writer1/mask0?Temp/Goal18_I3_Probe2_Result.json??????scoped Play probe?????driver?current OID36??kind10/11????????collector?target postinteraction??????????logic-only?????RNG??????????Scene????1?BodyBox Width????????W??????

I3 fresh focused194/194 PASS?Temp/Goal18_I3_GREEN2.xml?GREEN2_Result.json???Shadow valid/failure0/mismatch0/writer1?DataOriented?????Play???Play/C++?????

??Play attempt1 tick6???current Tayuya243/kind10?env0?-20?credit8242?source50?action182?Y=-2/Vy=-6?pendingY0.1???HP/WeaponCount??????flag?collector???????false???????FAIL??????PASS?Temp/Goal18_Play_attempt1.json?cleanup true/Scene hash??????probe?????target.SimPostInteraction?shared writer???????????range??????????????

Play attempt2 PASS/cleanup true?actual current Tayuya243??kind??????????????current243???3?kind10+outer kind11?kind11????????kind10????????????probe?kind11 target?outer??????????candidate index3???PASS?????????????kind11??witness?Temp/Goal18_Play_attempt2.json??????

?????????impact????SelfCheck R4-HIT-02A WeaponCount=-20???-1?kind11???????EnvironmentState<0??two-hop owner?BattleHitExecutionPlanEditorTests ShadowCompare_Kind10CharacterWriterEffectMatchesAuthorityState WeaponCount??5?Vx/Vz??????owner?exact?????kind11 case?/???WeaponCount?environment?NTSD28B5Type3WeaponFluteLegacyStatRetirementEditorTests?kind10?case?WeaponCount??5??owner/exact??????stats???????/??/HolderCopy/PickupCount/kind7????????????????impact??????????

impact?focused4?=3PASS/1FAIL?Temp/Goal18_ImpactOldFocused.xml?????ProductionSources???????ProjectKind10Or11WriterEffect/ProjectScaledAirStep???????impact??????????ProjectNativeImpactWriterEffect?ProjectKind15WriterEffect?????assert???????????????????????????testNames??namespace?????????4????6?

Play attempt3????FAIL????4?????probe??Runtime.SetPosition???????????SyncIntegerPosition???collector???integer0??????????source/target.SyncIntegerPosition???integer/precise X/Z?????????????????attempt3???C++prepare??FAIL???????????
