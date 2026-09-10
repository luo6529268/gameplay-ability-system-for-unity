<!-- CHANGE-RECORD
id: NTSD28-B6-NATIVE-IMPACT-PURE-CORE-PRODUCTION-001
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleNativeImpactResolver.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6NativeImpactPureCoreEditorTests.cs
authority: ??2026-09-10 Goal18???????playable impact??
evidence: RED_COMPLETED_147_FAIL_AT_LEAST_25_CAPPED / FOCUSED_147_PASS / WARMED_0B / SHARED_PENDING
-->

# NTSD28-B6-NATIVE-IMPACT-PURE-CORE-PRODUCTION-001

Goal18 / 2026-09-10 / PLANNED / TEST_FIRST????????????USER_HOLD????I1?I2?I3?

????????
- Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleNativeImpactResolver.cs
- Assets/NTSD/Scripts/Test/Editor/NTSD28B6NativeImpactPureCoreEditorTests.cs

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

???????Pickup???internal input/readonly plan/??GetOperation??World/Entity/Unity???API stub Resolve??default????????RED????????Input?current type/state/action?oid?environment/catch/impact?two-hop????physical slots?rule94/audited/????????locked??integer/precise Y?motion/pending???OperationCount0???????RNG writer?warmed???????reflection?GC?

I2?????????API stub?????Resolve?default?Applied?false?0????????RED????????????????.meta?????????

??RED147??????MCP???25???failures_capped=true?????????????????????Temp/Goal18_I2_RED_Result.json????????Resolve???GetOperation???preflight????applied????World???CODE_WRITTEN?GREEN???

I2 focused147/147 PASS??warmed0B????Temp/Goal18_I2_GREEN_Result.json?I1/I2?????I3?????????
