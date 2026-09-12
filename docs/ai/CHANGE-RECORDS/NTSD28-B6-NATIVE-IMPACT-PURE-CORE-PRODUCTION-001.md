<!-- CHANGE-RECORD
id: NTSD28-B6-NATIVE-IMPACT-PURE-CORE-PRODUCTION-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleNativeImpactResolver.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6NativeImpactPureCoreEditorTests.cs
authority: ??2026-09-10 Goal18???????playable impact??
evidence: Goal18F_FOCUSED_13_147_194_PASS / PLAY_10_11_PASS / CPP_98_FIELDS_NO_DIFF / B6_963_PLUS_CORRECTED_GUARD_PASS / REFILL_9_PASS / SELFCHECK_PASS / BUILDS_0_ERROR / VALIDATOR_PASS / SCENE_UNCHANGED / Temp/Goal18F_Regression.json
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

## Goal18F 收尾追加（2026-09-12，VERIFIED，限定 impact 三包）
本追加更正此前 Goal18 的 PLANNED/IN_PROGRESS/CODE_WRITTEN、SHARED_PENDING 等恢复状态；历史段落、乱码、失败和证据均保留。用户本轮授权为断点续传，没有重做或回退 I1/I2/I3。
- I1 `NTSD28-B6-NATIVE-IMPACT-HITPLAN-CARRIERS-PRODUCTION-001`：VERIFIED。Authority 为 battle_world.cpp:5405-5531 的 environment/+0x90/+0x164 写入；瞬态 HitPlan 捕获/投影/mask，复用既有 catch-source。历史 RED 11 FAIL/2 PASS；本次共享 focused 13/13 PASS。
- I2 `NTSD28-B6-NATIVE-IMPACT-PURE-CORE-PRODUCTION-001`：VERIFIED。Authority 同段的 owner/type/immunity/respond/motion 有序事务；纯计划 resolver，拒绝零写、对象 Y 步长 2.3、字符 3.0 与除法位模式。历史 RED 执行147项，报告至少25失败且 capped，精确失败总数未知，禁止写成147 FAIL；本次147/147 PASS，warmed0B。
- I3 `NTSD28-B6-NATIVE-IMPACT-ATOMIC-PRODUCTION-INTEGRATION-PRODUCTION-001`：由 CODE_WRITTEN 经本次 FOCUSED_TEST_PASS 推进为 VERIFIED。Authority 为 hit_candidates.cpp:134-256、battle_world.cpp:4540-4580/5270-5309/5405-5531、game_session.cpp:4171-4182；concrete/generic/legacy/HitPlan 共用既有 shared writer。历史 RED137 FAIL/57 PASS，本次194/194 PASS，Shadow valid/0 mismatch。当前 Record C++ 原文与现场源码逐字匹配，正式 EXE SHA B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033；见 Temp/Goal18F_AuthorityIdentity.json。

F1：原四项旧 focused 新鲜4/4 PASS，原失败源码守卫在续传前已经修好；第二份旧 XML 实为3/3，不能误报它覆盖四项。证据 Temp/Goal18_F1_OldFocused_Result.json 与 Goal18_F1_OldFocused.xml。
F2：Temp/Goal18_PlayF_attempt1.json 与 attempt2.json 均 PASS/cleanup=true；第二次补全 PP/rest 观测。current Tayuya OID36/frame243，经真实 SimulationTickDriver，seed424242、empty input、采样 tick6/7，kind11 只有 index3 候选。environment=-20、catch_source8242、impact_source50、action182；kind10 Y=-2/Vy=-6，kind11 Y=-3.75、Vy=-8.9，Vx/Vz=/1.07 位模式匹配。HP/PP/rest/delay/WeaponCount 与 impact 边界 RNG 保持。98个 C++/Unity 比较字段 firstDifference=null，详见 Temp/Goal18_PlayF_Comparison2.json、Goal18_AuthorityTrace.json 与 Goal18_AuthorityProvenance.json。
见证边界：C++ harness 调用当前 playable core 的几何候选与 impact 阶段，恢复 Unity 采样边界 CRT 状态、使用冻结 Direction-B 帧夹具并平移 Z 原点，排除无关实体；不是完整 C++ GameSession host replay。PP/sourceArest/targetArest/targetVrest 由 Unity 新鲜采样证明不变，未宣称这些字段由 C++ 输出。RNG 不抽仅指 impact 阶段，整个 driver tick 仍有其他既有 RNG 消费。object2.3、自定义 respond 与 kind17/18 由 focused 夹具覆盖；kind17/18 为 PLAY_NOT_PERFORMED_NO_PRODUCER。

F3：唯一共享 B6 job032efcd73b5949f1bada58270878a741 执行964=610前置+354新增，原始963 PASS/1 FAIL；失败为旧 impact 名称守卫，已按原批类内期望授权只修改四个字符串。定向完整守卫4/4 + refill9/9，于 jobb3527a23f1324eb7872d68f1e64eb9dc 合计13/13 PASS；首次定向启动0tests超时单独保留。合并最新定向结果后964个用例均有PASS证据，未进行第二次整批B6，也未篡改原963/964 XML。全部前置92/80/17/72/24/23/32/140与Goal17三包13/4/10覆盖数量核对一致，见 Temp/Goal18F_B6_Coverage.json。full SelfCheck于2026-09-12 08:37:27Z新鲜PASS；汇总 Temp/Goal18F_Regression.json，原始 Temp/Goal18F_B6.xml、Goal18F_GuardAndRefill.xml、Goal18F_SelfCheck.result。
双构建实际命令：dotnet build Assembly-CSharp.csproj --no-restore -v:minimal -clp:ErrorsOnly；Editor同命令。均exit0/0error，warnings47/104；最终Editor增量后再次0error，见 Temp/Goal18F_RuntimeBuild.txt、Goal18F_EditorBuild_Final.txt。Unity重载后无编译错误，SelfCheck Console7条均预期registration/rest负向夹具，不声称Console0。
Tools/Validate-ChangeLedger.ps1通过452 records/2 governed code files；最终文件 Temp/Goal18F_Validator.txt。PowerShell默认把Git全局ignore不可读与CRLF warning当作终止错误，因此仅对子进程追加 core.excludesFile=NUL、core.safecrlf=false，保留既有safe.directory设置；不写.git/config，不绕读受限文件。历史Record非当前diff警告保留。
SelfCheck曾切换为空场景，已通过既有编辑器重新打开 NTSD_Battle，isDirty=false/root13；Scene SHA仍为D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11，见Temp/Goal18F_FinalSceneRestored.json。

F4：本次只改两个授权测试文件（probe增加8行、旧impact守卫4个字符串）与治理文档；无生产脚本变更，无schema/RNG写入/HP/PP/rest/delay主体修改，无FluteForce/+11复活。三个既有UI图片修改与.claude用户目录保留。无git add/commit/push。全部历史中文乱码只保留，本次追加为UTF-8正常中文；metadata状态和Ledger当前状态更新，不重写历史事实。回滚仍仅限获批后反向本次测试/文档增量，不回退既有三包。
三包 VERIFIED 仅指本轮批准的 impact 范围与上述分层证据，不代表整个战斗系统完全对齐；既有 USER_HOLD、内容Direction-B与默认stage资产暂缓继续有效，不启动后继任务。