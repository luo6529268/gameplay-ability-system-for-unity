# NTSD 当前权威恢复入口

> **USER HOLD（2026-09-09）：** 用户要求停止当前 `NTSD28-UNITY-BATTLE-REALIGNMENT-001` 自动推进，
> 改由 GLM 先核验当前进度、现有生产脚本和证据，再整理真正遗漏与未处理项；禁止从头重做已完成逻辑。
> 暂停期间不得启动新对齐包、继续Play验收、修复独立SelfCheck/stress失败或
> 修改production/content/Scene；新会话可直接复制
> `docs/ai/GLM-INCREMENTAL-CONTINUATION-PROMPT-2026-09-09.md`，详细证据交接见
> `docs/ai/GLM-REALIGNMENT-HANDOFF-2026-09-09.md`。总目标保持 `FULL_ALIGNMENT_INCOMPLETE`。

> **B6 positive-link validation已退休（2026-09-09）：** `NTSD28-B6-POSITIVE-LINK-VALIDATION-RETIREMENT-PRODUCTION-001 / VERIFIED / RED_4_FAIL_2_PASS_OF_6 / FOCUSED_6_OF_6 / RELATED_33_OF_33 / B6_CATEGORY_103_OF_103 / NTSD28_229_OF_229 / STRESS_255_OF_256_1_UNRELATED_AI_REPORT / REAL_BATTLE_GRAB_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_R2_CPOINT_SYNC / CONSOLE_0_ERROR / SCENE_UNCHANGED / PHASE_33 / NO_POSITIVE_EVENT`。正式post-catch现直接进入stage clamp/第二次held；compat entry无写入无event，lifecycle transaction保持唯一原子cleanup owner。下一先做B6剩余入口/exit只读复核，不凭旧backlog直接启动跨域实现。

> **B6 held injury caughtact event已验证（2026-09-09）：** `NTSD28-B6-HELD-INJURY-CAUGHTACT-EVENT-PRODUCTION-001 / VERIFIED / RED_5_FAIL_10_PASS_OF_15 / FOCUSED_15_OF_15 / B6_CATEGORY_97_OF_97 / NTSD28_223_OF_223 / HITPLAN_185_OF_185 / PREINTERACTION_15_OF_15 / REAL_BATTLE_GRAB_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_POSITIVE_LINK / CONSOLE_0_ERROR / SCENE_UNCHANGED / LEDGER_PASS_428_364 / POST_SETTLEMENT_EVENT_EXACT`。真实applied正injury event现于完整settlement后按序消费，Play count0→1且无重复；下一strict首差为positive-link validation retirement。

> **B6 held injury caughtact event启动记录（已由上条VERIFIED关闭）：** 本包曾以`IN_PROGRESS / TEST_FIRST / PRODUCTION_UNCHANGED`启动；恢复时以上条最终证据为准。

> **B6 held injury accounting/cover已验证（2026-09-09）：** `NTSD28-B6-HELD-INJURY-ACCOUNTING-COVER-PRODUCTION-001 / VERIFIED / RED_14_FAIL_3_PASS_OF_17 / FOCUSED_17_OF_17 / B6_CATEGORY_82_OF_82 / NTSD28_208_OF_208 / HITPLAN_185_OF_185 / PREINTERACTION_15_OF_15 / RELATED_FIXTURES_9_OF_9 / REAL_BATTLE_GRAB_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_POSITIVE_LINK / CONSOLE_0_ERROR / SCENE_UNCHANGED / LEDGER_PASS_427_363 / CANONICAL_ACCOUNTING_COVER_EXACT`。`IncomingDamageScale340`、direct owner/type0 self、HP/HPBound/consumed/score/KO与cover exclusion timers已闭合；下一strict entry是caughtact event。

> **B6 settlement vaction preflight已验证（2026-09-09）：** `NTSD28-B6-CATCH-SETTLEMENT-VACTION-PREFLIGHT-PRODUCTION-001 / VERIFIED / RED_4_FAIL_4_PASS_OF_8 / FOCUSED_8_OF_8 / B6_CATEGORY_65_OF_65 / NTSD28_191_OF_191 / HITPLAN_185_OF_185 / PREINTERACTION_15_OF_15 / TARGETED_PLAY_8_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_HELD_ACCOUNTING / CONSOLE_0_ERROR / SCENE_UNCHANGED / LEDGER_PASS_426_362 / ACTUAL_PREFLIGHT_EXACT`。signed/zero vaction提交与post-action frame/kind2 terminal fence已闭合；下一严格入口是held injury exact accounting。

> **B6 mixed catch advance/exact consumer已验证（2026-09-09）：** `NTSD28-B6-CATCH-EXACT-CONSUMER-AND-ADVANCE-ORDER-PRODUCTION-001 / VERIFIED / RED_1_PASS_7_FAIL_OF_8 / FOCUSED_16_OF_16 / PREINTERACTION_15_OF_15 / B6_CATEGORY_57_OF_57 / HITPLAN_185_OF_185 / NTSD28_183_OF_183 / TARGETED_PLAY_16_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_HELD_ACCOUNTING / CONSOLE_0_ERROR / SCENE_UNCHANGED / LEDGER_PASS_425_361 / SINGLE_MIXED_ADVANCE_EXACT_CONSUMERS`。single slot升序mixed advance、三个plain exact +0x90 consumer及mismatch/negative-release terminal fence已闭合；下一严格入口是settlement vaction preflight，再到held accounting。

> **B6 catch relation exact-field production已验证（2026-09-09）：** `NTSD28-B6-CATCH-RELATION-EXACT-FIELDS-PRODUCTION-001 / VERIFIED / RED_0_OF_3 / FOCUSED_19_OF_19 / B6_CATEGORY_41_OF_41 / HITPLAN_185_OF_185 / NTSD28_167_OF_167 / TARGETED_PLAY_19_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_HELD_ACCOUNTING / CONSOLE_0_ERROR / SCENE_UNCHANGED / EXACT_RELATION_ATOMIC`。actual/HitPlan现闭合kind3 first/signed、双frame preflight、exact+compat与respond；current criminal和kind1均绿。下一严格入口是mixed catch advance/control-flow fences。

> **B6 invalid negative-held reciprocal preserve已验证（2026-09-09）：** `NTSD28-B6-HELD-INVALID-RECIPROCAL-PRESERVE-PRODUCTION-001 / VERIFIED / RED_2_OF_2 / FOCUSED_7_OF_7 / B6_CATEGORY_22_OF_22 / RELATED_47_OF_47 / BROAD_56_OF_62_6_UNRELATED_NATIVE_INPUT_PROXY / TARGETED_PLAY_7_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_HELD_ACCOUNTING / CONSOLE_0_ERROR / SCENE_BASELINE_RESTORED / DIAGNOSTIC_PRESERVE`。C09/C20 missing/out-of-range/mismatch现在只计数、可选trace并preserve；slot0/high、RNG/sentinel、lifecycle-clean与0B已绿。下一严格入口是catch relation exact-field producer。

> **B6 entity-link lifecycle cleanup已验证（2026-09-09）：** `NTSD28-B6-ENTITY-LINK-LIFECYCLE-CLEANUP-PRODUCTION-001 / VERIFIED / FOCUSED_7_OF_7 / B6_CATEGORY_15_OF_15 / RELATED_91_OF_91 / TARGETED_PLAY_7_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_HELD_ACCOUNTING / CONSOLE_0_ERROR / SCENE_BASELINE_RESTORED / ATOMIC_RELEASE_CLEANUP / RED_NOT_EXECUTED`。成功slot release与generation row release/reuse之间现原子清held/catch exact+compat反向关系；P7旧复用夹具已纠正，full SelfCheck恢复停在独立held injury accounting。下一严格包为invalid reciprocal preserve，不混入catch算法或positive-pass retirement。

> **B6 CPoint throw精确子集已验证（2026-09-09）：** `NTSD28-B6-CPOINT-THROW-ENVIRONMENT-VZ-PRODUCTION-001 / VERIFIED / FOCUSED_8_OF_8 / RELATED_17_OF_17 / TARGETED_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_HELD_ACCOUNTING / CONSOLE_0_ERROR / SCENE_CURRENT_BASELINE_UNCHANGED / FULL_RESOURCE_DEFERRED / ILLEGAL_CATEGORY_RETIRED`。display/environment/self-source/WeaponCount exclusion/Vz XOR已获真实Unity与Play证据；full SelfCheck已越过throw并停在独立held injury accounting。完整MP resource仍后置B7/B8/B11/H。

> **B5 negative environment shared recovery已验证（2026-09-09）：** `NTSD28-B5-NEGATIVE-ENVIRONMENT-RECOVERY-PRODUCTION-001 / VERIFIED / RED_11_OF_12 / FOCUSED_12_OF_12 / RELATED_154_OF_154 / RELATED_B5_981_OF_981 / TARGETED_PLAY_12_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / SINGLE_EXACT_TRANSACTION / FLUTE_FALSE_POSITIVE_RETIRED`。Legacy/DataOriented/derived现共享negative EnvironmentState/native phase/rule/900-scale/two-hop exact transaction并post-accounting clamp0；其当时的CPoint throw Vz阻塞已由后继B6包关闭，fresh full SelfCheck当前推进到独立held injury accounting。B6 producer、B8 event与联合schema后置。

> **B5 negative environment clamp合同纠正（2026-09-09）：** `NTSD28-B5-NEGATIVE-ENVIRONMENT-CLAMP-CORRECTION-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / SOURCE_HASH_MATCH / CLAMP_TO_ZERO_REQUIRED / PRIOR_NO_CLAMP_CLAUSE_SUPERSEDED`。无漂移playable source与source tests要求exact accounting后HP/HPBound clamp0；先前owner audit仅“no clamp”一句被纠正。

> **B5 negative environment rule carrier已验证（2026-09-09）：** `NTSD28-B5-NEGATIVE-ENVIRONMENT-RULE-CARRIER-001 / VERIFIED / RED_13 / FOCUSED_6_OF_6 / RELATED_106_OF_106 / NTSD28_1394_OF_1394 / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_BASELINE_UNCERTIFIED / CARRIER_READY / RECOVERY_CONSUMER_NEXT`。+0x90/default9 deterministic carrier与schema `11/20/23`闭合，exact NTSD28 broad1394/1394；下一single recovery consumer。full SelfCheck独立CPoint阻塞，Scene基线未认证。

> **B5 negative environment recovery owner审计已闭合（2026-09-09）：** `NTSD28-B5-NEGATIVE-ENVIRONMENT-RECOVERY-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / WEAPONCOUNT_WRONG_CARRIER / EXACT_TRANSACTION_REQUIRED / TWO_PACKAGE_ROUTE`。Authority是negative EnvironmentState320+native phase12+rule90/exact credit；Unity两路WeaponCount分支均错误且被flute -20真实触发。下一先补rule +0x90 carrier，再统一consumer。

> **当前最前置实施包（2026-09-09）：** `NTSD28-B5-INPUT-HP-COST-COMPAT-SHARED-TRANSACTION-PRODUCTION-001 / VERIFIED / RED_0_OF_6 / FOCUSED_6_OF_6 / RELATED_INPUT_187_OF_187 / RELATED_B5_926_OF_926 / TARGETED_PLAY_6_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / TWO_INPUT_WRITERS_RETIRED / NEGATIVE_RECOVERY_AUDIT_NEXT`。registered Legacy/DataOriented与unregistered generic action已共享existing exact transaction，两处ComboVic HP-cost writer归零；下一严格包审计两套negative recovery writer的Authority owner。

> **B5 input HP-cost compatibility stats审计已闭合（2026-09-09）：** `NTSD28-B5-INPUT-HP-COST-COMPAT-STAT-RETIREMENT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / COMPAT_PATH_LIVE / PARTIAL_RETIREMENT_UNSAFE / SHARED_TRANSACTION_PRODUCTION_DEFINED`。一个dead duplicate与一个可配置Legacy production compat都写旧ComboVic；因compat同时缺完整native cost/fallback字段，禁止仅换成+0x34C或只删writer。下一严格包复用现有exact transaction后统一退休两处。

> **当前最前置实施包（2026-09-09）：** `NTSD28-B5-TYPE3-WEAPON-FLUTE-LEGACY-STAT-WRITER-RETIREMENT-001 / VERIFIED / RED_0_OF_4 / FOCUSED_4_OF_4 / RELATED_B5_777_OF_777 / TARGETED_PLAY_4_CASES / LIVE_COLLISION_MATRIX_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / SCOPED_LEGACY_STATS_RETIRED / INPUT_COMPAT_AUDIT_NEXT`。type3/weapon victim/world与flute holder/world Authority-extra legacy镜像已退休，exact HP/KO/+0x2F4与standard/reduced/CPoint/input/recovery/schema保持；下一严格包只读审计input HP cost compat seam。

> **当前最前置实施包（2026-09-09）：** `NTSD28-B5-STANDARD-REDUCED-KNOCKOUT-PRODUCER-001 / VERIFIED / RED_1_OF_4 / FOCUSED_4_OF_4 / RELATED_270_OF_270 / TARGETED_PLAY_4_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / EXACT_KO_PRODUCER_READY / TYPE3_WEAPON_FLUTE_STATS_NEXT`。standard/reduced lethal现按same attribution在HP mutation前写exact `KnockoutCount358`，HitPlan同值；下一严格包退休type3/weapon/flute安全legacy stat镜像。

> **B5 legacy damage-stat writer retirement readiness已闭合（2026-09-09）：** `NTSD28-B5-LEGACY-DAMAGE-STAT-WRITER-RETIREMENT-READINESS-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / GLOBAL_RETIREMENT_BLOCKED / FOUR_PREREQUISITE_ROUTES`。standard/reduced缺exact KO，held缺exact accounting，input compat与negative recovery也未闭合；先补B5 standard/reduced KO，再退type3/weapon/flute安全镜像，不能直接全删。

> **当前最前置实施包（2026-09-09）：** `NTSD28-B5-ORDINARY-CREDIT-GATE-2F4-PRODUCER-CONSUMER-CORRECTION-001 / VERIFIED / RED_0_OF_5 / FOCUSED_5_OF_5 / RELATED_287_OF_287 / TARGETED_PLAY_5_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / EXACT_2F4_READERS_PRODUCERS / LEGACY_STATS_WRITER_NEXT`。standard/reduced/CPoint damage、PP threshold与type0 OPoint传播已重绑exact `OrdinaryCreditGate2F4`，non-type0不再产生该值或legacy KillCount；下一严格route为B5 legacy damage-stat writer retirement，carrier/schema继续后置。

> **当前最前置实施包（2026-09-09）：** `NTSD28-B5-TYPE3-LEGACY-HOLDERCOPY-WRITER-RETIREMENT-001 / VERIFIED / RED_0_OF_3 / FOCUSED_4_OF_4 / RELATED_284_OF_284 / TARGETED_PLAY_3_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / FOUR_TYPE3_WRITERS_RETIRED / TYPE3_SPECIFIC_FAMILY_EXIT_READY / ORDINARY_CREDIT_GATE_2F4_NEXT`。actual kind9与HitPlan三处Authority不存在的HolderCopy write已退休，type3 exact group/owner/control/action/motion保持；下一严格route为B5 `OrdinaryCreditGate2F4` producer/consumer correction，legacy stats与联合schema继续后置。

> **当前最前置实施包（2026-09-09）：** `NTSD28-B5-KIND5-LINKED-PARENT-SLOT-CORRECTION-001 / VERIFIED / RED_0_OF_5 / FOCUSED_5_OF_5 / RELATED_280_OF_280 / TARGETED_PLAY_5_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / EXACT_LINKED_PARENT_BOUND / HOLDERCOPY_UNCHANGED / TYPE3_WRITER_NEXT`。Authority linked-parent现映射HolderStableId/implicit-zero，frozen pair/kind5/negative-link不再读HolderCopy；下一严格route为type3额外HolderCopy writer退休，stats/schema继续后置。

> **当前最前置实施包（2026-09-09）：** `NTSD28-B4-REVIVAL-EXIT-AUDIT-001 / VERIFIED / REVIVAL_TRACE_EQUAL_13_RECORDS_416_FIELDS / DOUBLE_RUN_BYTE_STABLE / RELATED_54_OF_54 / TARGETED_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / B4_REVIVAL_EXIT_READY / PRODUCTION_UNCHANGED / B7_H_PRODUCER_PENDING`。Authority/Unity 13/416 first difference空且双跑稳定，真实Play/Console/Scene通过；full SelfCheck仍为既有CPoint阻塞。B4 revival consumer/exit ready，下一严格route返回B5 HolderCopy binding/extra-write corrections，B7/H producer/schema保持pending。

> **当前最前置实施包（2026-09-09）：** `NTSD28-B4-REVIVAL-NORMAL-FLOOR-RNG-PRODUCTION-001 / VERIFIED / RED_0_OF_7 / FOCUSED_7_OF_7 / RELATED_59_OF_59 / TARGETED_PLAY_7_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / EXIT_AUDIT_NEXT`。normal revival的effective floor、peer/sumX、sync RNG0x90/0x91、precise-only X/Z和vitals tail已闭合；下一严格route为B4 exit audit。

> **当前最前置实施包（2026-09-09）：** `NTSD28-B4-REVIVAL-QUEUED-CONTINUATION-PRODUCTION-001 / VERIFIED / RED_4_OF_11 / FOCUSED_13_OF_13 / RELATED_47_OF_47 / TARGETED_PLAY_13_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / NORMAL_ROUTE_NEXT / PRODUCER_B7_H_PENDING / SCHEMA_DEFERRED`。+0x360 controller/defer/group、queued字段、visual、action219/counter0/hold10与OID998调用前状态已闭合；下一严格route为normal floor/RNG。

> **当前最前置实施包（2026-09-09）：** `NTSD28-B4-REVIVAL-PARTICIPANT-GATE-KILLCOUNT-CORRECTION-001 / VERIFIED / RED_10_OF_16 / FOCUSED_16_OF_16 / RELATED_34_OF_34 / TARGETED_PLAY_20_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / QUEUED_ROUTE_NEXT`。两个legacy HitStun arm与C07 KillCount/team gate已退休，lives-first queued/primary-retain/transient-free/normal分支已恢复；下一严格route为queued continuation细节。

> **当前最前置实施包（2026-09-09）：** `NTSD28-B0-DIRECT-REVIVAL-DEFAULTS-PRODUCTION-001 / VERIFIED / RED_1_OF_3 / DIRECT_DEFAULTS_1_0_0 / FOCUSED_3_OF_3 / DIRECT_OWNER_REGRESSION_15_OF_15 / TARGETED_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / B4_RUNTIME_RESUMABLE`。direct slot0/19现于首次注册前发布Authority默认1/0/0，active snapshot正确且raw backing保持0/0/0；invalid不写。Play/Console/Scene通过，full SelfCheck独立CPoint阻塞。严格恢复B4 revival gate correction。

> **B4 revival owner审计已闭合（2026-09-09）：** `NTSD28-B4-REVIVAL-PARTICIPANT-GATE-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / KILLCOUNT_NOT_REVIVAL_AUTHORITY / THREE_ENTRY_POINTS_SPLIT / DIRECT_DEFAULT_PRODUCER_MISSING / FOUR_RUNTIME_ROUTES_DEFINED / PRODUCTION_HELD`。Authority C25/C07不读KillCount；Unity三个旧gate、wrong branch priority、primary free及queued/floor/RNG后继已拆分。Direction-B state14=235；先回退B0补direct默认字段producer。

> **当前最前置实施包（2026-09-09）：** `NTSD28-B3-LEGACY-1100-1299-CHILD-PROPAGATION-RETIREMENT-001 / VERIFIED / RED_0_OF_4 / PRODUCTION_CHILD_SCAN_REMOVED / FOCUSED_4_OF_4 / TARGETED_PLAY_PASS / BUILDS_0_ERROR / SELF_RESET_PRESERVED / CURRENT_ITACHI_1250_COVERED / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED`。Authority只写self；Unity额外world/KillCount child write已退休。RED matched为0/-99/-149/-198而normal child为40；focused4/4与真实Play通过并覆盖Itachi next1250。full SelfCheck仍由更早CPoint阻塞；严格下一包为B4 revival participant gate correction。

> **当前最前置实施包（2026-09-09）：** `NTSD28-B3-LEGACY-STATE501-TRANSFORM-RETIREMENT-001 / VERIFIED / RED_0_OF_1 / PRODUCTION_BRANCH_REMOVED / FOCUSED_1_OF_1 / EARLY_M2_11_OF_11 / TARGETED_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED`。Authority不存在且双方正式gameplay content为0的early-frame 501 self/KillCount-child transform已从fast/fallback/legacy路径退休；两组10实体canonical runtime/definition/identity/frame均保持。full SelfCheck仍由更早CPoint Vz阻塞；state500、teleport、CPoint、11xx/12xx与schema未动。严格下一包为B3 1100..1299 child propagation retirement。

> **B3 state501审计已闭合（2026-09-09）：** `NTSD28-B3-LEGACY-STATE501-OWNED-CHILD-TRANSFORM-RETIREMENT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / NO_AUTHORITY_STATE501_TRANSFORM / DIRECTION_B_GAMEPLAY_ZERO / RELEASE_GAMEPLAY_ZERO / HUD_RADAR_ONLY_TWO_TOKENS / UNITY_SYNTHETIC_BRANCH_CONFIRMED / PRODUCTION_RETIREMENT_DEFINED`。Authority C25只做8000..8999 definition transition且不扫描owner；Unity early-frame独有501 self/KillCount-child mutation。本审计无code/content/Scene/Authority改动。

> **当前最前置实施包（2026-09-09）：** `NTSD28-B2-AI-OWNER-SLOT-KILLCOUNT-CORRECTION-001 / VERIFIED / OWNER_GUARD_TRACE_EQUAL_8_RECORDS_48_FIELDS / UNITY_FOCUSED_4_OF_4 / TARGETED_PLAY_PASS / DOUBLE_RUN_BYTE_STABLE / BUILDS_0_ERROR / EXACT_OWNER_SLOT_BINDING / LEGACY_KILLCOUNT_DETACHED_FROM_AI / SELFCHECK_BLOCKED_UNRELATED`。Authority source-model与Unity production snapshot/SoA专项trace 8/48 first difference空且各自双跑稳定；B0 direct/OPoint/F8 owner `0/7/99`均被消费。真实NTSD_Battle Play通过、Console0、Scene不变；full SelfCheck仍由较后既有CPoint Vz阻塞。本结论只关闭AI owner/KillCount guard family；严格下一包为B3 legacy state501 child transform retirement audit。

> **当前最前置实施包（2026-09-09）：** `NTSD28-B0-OWNER-SLOT-PRODUCTION-EXIT-AUDIT-001 / VERIFIED / OWNER_TRACE_EQUAL_15_RECORDS_135_FIELDS / DOUBLE_RUN_BYTE_STABLE / TARGETED_PLAY_PASS / ROUTES_1_TO_4_REGRESSION_32_OF_32 / BUILDS_0_ERROR / SELFCHECK_BLOCKED_BY_UNRELATED_CPOINT / B0_OWNER_PRODUCER_EXIT_READY / B2_RUNTIME_RESUMABLE / PRODUCTION_UNCHANGED`。Authority/Unity专项trace已覆盖direct self、owner/target独立、two-hop OPoint、F8 99、state9996 -1、type3 mutation与slot reuse，15/135 first difference空且双端双跑稳定；真实NTSD_Battle Play通过、Console0、Scene不变。full SelfCheck仍被更早CPoint阻塞，不代表full parity或B8 physical F8完成。严格下一步恢复`NTSD28-B2-AI-OWNER-SLOT-KILLCOUNT-CORRECTION-001` runtime验收。

> **当前最前置实施包（2026-09-09）：** `NTSD28-B0-OWNER-SLOT-PRODUCTION-EXIT-AUDIT-001 / IN_PROGRESS / B0_OWNER_PRODUCER_ROUTE_5 / OWNER_ONLY_JOINT_TRACE_DESIGN / PRODUCTION_UNCHANGED`。route1～4已有compile+focused证据；现建立只比较+0x354/OwnerSlot的Authority/Unity规范化trace，覆盖self、two-hop OPoint、F8 99、state9996 -1、type3 mutation和slot reuse。F8 content/count/position、full parity与B8 physical consumer排除；通过前B2 runtime继续阻塞。

> **当前最前置实施包（2026-09-09）：** `NTSD28-B0-OPOINT-OWNER-PROPAGATION-PRODUCTION-001 / FOCUSED_TEST_PASS / UNITY_RUNTIME_COMPILE_PASS / OUT_OF_PROCESS_COMPILE_PASS / UNITY_FOCUSED_7_OF_7 / SELFCHECK_BLOCKED_BEFORE_PRESENTATION_ASSERT_BY_UNRELATED_CPOINT / RUNTIME_PENDING / B0_OWNER_PRODUCER_ROUTE_4 / LOGIC_AND_PRESENTATION_PRODUCERS_WRITTEN`。同一structural segment的logic-only与presentation `ProcessOneLateOpoint()`现各只写`task.ownerEntityIndex=spawner.OwnerEntityIndex`，不在通用factory推断。精确RED=`2/7`，builds均0 error；Unity 00:09:55 GREEN=`7/7`覆盖self/nonself/sentinel、kind/type/single/multi/two-hop/holder/raw backing及built-in `-1`。00:11:22 SelfCheck被更早CPoint阻塞，未到presentation新增断言；state9996/built-in维持`-1`，DAT weapon_piece归B7，8/9/13独立。下一route 5 owner producer exit audit。

> **当前最前置实施包（2026-09-08）：** `NTSD28-B0-F8-OWNER99-PRODUCTION-001 / FOCUSED_TEST_PASS / UNITY_RUNTIME_COMPILE_PASS / OUT_OF_PROCESS_COMPILE_PASS / UNITY_FOCUSED_3_OF_3 / SELFCHECK_REACHED_UNRELATED_CPOINT_AFTER_NEW_CHECK / RUNTIME_PENDING / B0_OWNER_PRODUCER_ROUTE_3 / MODE2_MATERIALIZER_OWNER_ONLY / PHYSICAL_F8_EFFECT_WIRING_EXCLUDED`。既有`Mode2Request==1 -> SpawnMode2RandomWeapons()`现只在factory前写task owner=`99`，由route 2 initializer发布claimed entity runtime；独立raw backing保持`-1`。精确RED=`1/3`，runtime/editor builds均0 error；Unity 23:25:56 GREEN=`3/3`覆盖slot50/399、四/六次RNG、frame/位置和normal owner`-1`。23:27:20 full SelfCheck通过本包检查后仍停在较后的既有CPoint throw-Vz。正式物理F8 pending consumer、Play/joint trace仍待验；下一route 4 ordinary OPoint owner propagation。

> **当前最前置实施包（2026-09-08）：** `NTSD28-B0-DIRECT-ENTITY-SELF-OWNER-PRODUCTION-001 / FOCUSED_TEST_PASS / UNITY_RUNTIME_COMPILE_PASS / UNITY_FOCUSED_15_OF_15 / SELFCHECK_BLOCKED_BY_UNRELATED_CPOINT / RUNTIME_PENDING / B0_OWNER_PRODUCER_ROUTE_2`。direct App/bootstrap已在ModuleBind前声明required/self slot，adapter按Authority只接受physical slot `0..19`，actual slot不匹配时统一reset/recycle并跳过roster；stage task携带owner=required slot，各entity OPoint initializer在首次注册前消费explicit owner，stage/results-reserve不再尾部覆盖-1。runtime `0 error / 47 warnings`、Editor进程外compile `0 error / 104 warnings`；重启后的Unity于22:51:33刷新程序集，纠正active entity runtime/raw backing测试边界后v1实际`15/15`通过。22:52:37 full SelfCheck在更早的既有CPoint throw-Vz断言停止，Play/joint trace仍待验。F8、ordinary OPoint、state9996及其他owner writer不并入。

> **route 4更正边界（2026-09-08）：** ordinary frame OPoint传播parent literal owner；hit_Fa5/6 owner与target +3F8独立。built-in OID999 weapon fragments及state9996保持owner -1，只有DAT `<weapon_piece>`继承source owner。Unity正式late OPoint producer缺task owner；DAT weapon_piece尚无parser/materializer，仅有pass skeleton，完整实现归B7；hit_Fa8/9/13继续独立。B0 route 4不得在factory中按parent做全局推断。

> **下一route 3只读边界（2026-09-08）：** Authority F8在`GameSession28::step()`完成战斗tick后的function-key tail消费pending，并将`NativeFunctionKeyDropSpawn28.owner_slot=99`原样交给`spawn_at`。Unity正式F8目前只写`FunctionKeys.PendingObjectCommand`且没有生产consumer；现有`Mode2Request==1 -> SpawnMode2RandomWeapons()`来自legacy diagnostic latch并缺owner。route 2 focused gate现已满足；route 3只允许在该既有materializer的factory调用前写task owner=99，保留`requiredRuntimeSlot=-1`的lowest-free分配且不做post-register fix-up，并验证claimed active-slot runtime/raw-trace projection，独立raw backing保持`-1`；physical F8 effect wiring继续归B8，`RunNormalDrop`继续默认owner -1，用户保留的candidate/RNG/position不得改变。

> 决策 ID：`GOVERNANCE-NTSD28-LOGAN-AUTHORITY-MIGRATION-001`  
> 生效日期：2026-09-02  
> 状态：`USER_CONFIRMED_BUGFIXED_IDENTITY / ACTIVE / PROMOTION_VERIFIED / B3_ALIGNMENT_IN_PROGRESS`

> **最高优先级恢复结论（2026-09-04 用户确认）：** 用户说明其发现并修复了 NTSD 2.8-Logan
> Bug，并明确要求继续处理。指定根当前 `NTSD2.8-Logan.exe` 的
> `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033` 与当前82-file playable
> C++/header closure manifest `39DDDA154F5632C43089E2D5F1A5755ABBFBFD131A85D6B5ABF9AC00E6A46109` 已正式晋升为唯一权威。
> 旧 `1277B70B...DAF75` / `C59BD8D3...2D75` 只保留为历史基线；不得再驱动实现。由于新版改变了
> core pass 顺序，B0～当前B3受影响结论必须显式重新核验，不能只替换哈希后继承旧证书。

这是任何新任务、上下文压缩恢复、交接或历史文档检索后必须首先读取的唯一权威入口。
若其他文档、旧 Change Record、旧 Handoff、测试名或注释与本文件冲突，以用户当前明确要求和
本文件为准；不得沿用 NTSD 2.4 的结论继续修改 Unity。

## 1. 当前唯一战斗行为权威

- 根目录：`J:\QQFile\NTSD2.8.3.3 zip\NTSD2.8.3.3\NTSD 2.8-Logan`
- 正式发行 EXE：`NTSD2.8-Logan.exe`
- EXE SHA-256：`B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`
- EXE FileVersion：`2.8.3.3`
- EXE ProductVersion：`2.8.3.3-development`
- EXE OriginalFilename：`Ntsd28Playable.exe`
- 正式启动器：`Start_NTSD2.8-Logan.cmd`
- 对应源码声明：`source\README_SOURCE.md` 明确说明 `source\` 是当前发行 EXE 对应的 C++ 源代码快照。
- 当前 playable C++/header closure manifest（82 files）：`39DDDA154F5632C43089E2D5F1A5755ABBFBFD131A85D6B5ABF9AC00E6A46109`。
- 当前 authority source-capture manifest（75 files，规则相关core/session/scenario子闭包与全部headers）：
  `07CD47A0623F23D2C439E0E85EABF2ED10F8EAE8FC7D70DDB8396C704B3D778F`。
- 先前 drift 审计记录的 `5F2E5B41...5FA9` 是旧workspace捕获脚本遗漏
  `kind_catalog.cpp`、`minibar_catalog.cpp` 后计算出的73-file子集，不能再称为完整capture manifest，更不是
  82-file playable闭包。
- 唯一 runtime 资源根：`resources\runtime`；正式启动器同时把 `--resource-root` 和
  `--complete-vfs-root` 指向该目录。

裁决优先级如下：

1. 用户在当前任务中的明确要求。
2. 上述固定 SHA 的正式 `NTSD2.8-Logan.exe` 的实际可观察行为。
3. `source\README_SOURCE.md` 声明对应正式 EXE、且实际进入 playable 构建闭包的源码。
4. 正式启动参数及 `resources\runtime` 中被正式 EXE 消费的数据。
5. 新权威目录中的 tests、diagnostics、候选 build 和研究记录，仅可作辅助证据；不能覆盖正式 EXE。
6. Unity 当前实现、self-check、旧 trace、旧对齐结论和历史文档只能作为待重新核验的实现或证据。

源码重建输出 `source\ntsd28_playable\build\Ntsd28Playable.exe` 不会自动覆盖根目录正式 EXE。
因此“源码可编译”或“候选 EXE 行为”不能自动晋升为正式发行行为；任何晋升必须由用户明确确认，
并更新本文件中的正式 EXE 指纹。

2026-09-04 以前锁定的 `1277B70B...DAF75` EXE 和 `C59BD8D3...2D75` source manifest 已被
`GOVERNANCE-NTSD28-AUTHORITY-PROMOTION-002` supersede。它们仍可用于定位这次 Bug 修复改变了哪些合同，
但不再裁决当前规则。

## 2. 当前源码恢复入口

| 领域 | 当前入口 |
|---|---|
| 正式 host 与每步调用 | `source\ntsd28_playable\src\game_session.cpp` 的 `GameSession28::step()` |
| 战斗主 tick 与 pass 顺序 | `source\ntsd28_core\src\simulation\simulation_tick_driver.cpp` 的 `SimulationTickDriver28::step(...)` |
| World、实体、关系与生命周期 | `source\ntsd28_core\src\simulation\battle_world.cpp` 的 `BattleWorld28` |
| 帧状态与帧运动 | `source\ntsd28_core\src\simulation\frame_machine.cpp`、`frame_motion.cpp` |
| 物理积分 | `source\ntsd28_core\src\simulation\physics_integrator.cpp` |
| 碰撞候选与命中消费 | `source\ntsd28_core\src\simulation\hit_candidates.cpp` 及 `battle_world.cpp` 的消费路径 |
| 输入路由与 AI | `source\ntsd28_core\src\simulation\input_routing.cpp`、`native_ai.cpp` |
| 对象生成与 OPoint | `source\ntsd28_core\src\simulation\object_spawning.cpp` 及 `battle_world.cpp` 的 tick 尾部 |
| 逻辑表现快照 | `source\ntsd28_core\src\rendering\render_snapshot.cpp` |
| 正式 D3D11 表现与插值 | `source\ntsd28_playable\src\d3d11_renderer.cpp`、`presentation_interpolation.cpp` |
| 战斗结果流程与程序入口 | `source\ntsd28_core\src\simulation\battle_flow.cpp`、`source\ntsd28_playable\src\main.cpp` |

这些文件只是入口。处理具体行为时仍须沿调用链追到字段定义、读写者、前置条件、分支顺序、
RNG、slot 生命周期和最终可观察副作用，并确认文件实际进入 playable 构建闭包。

## 3. 已观察的新基线；Unity 尚未据此改动

以下是对当前权威包的只读观察结果，不等同于 Unity 已经对齐：

- `resources\runtime\decoded_dat\data\system.dat`：正常 `fps_value: 33`，F5 快速模式
  `fps_value_f5: 3`。因此旧文档中的“权威固定精确 30 Hz / `1f / 30f`”不能继续作为
  NTSD 2.8-Logan 的规则结论；Unity 当前 `SIM_DT` 状态必须在后续独立任务中重新盘点。
- 引擎 profile 使用 `maximum_slots=1000`、物理 frame id `0..999`，transient allocation
  范围 `50..999`。这是新权威观察事实；用户已经明确要求容量模型继续使用 Unity 现有
  profile/动态逻辑容量，因此容量本身不作为待修差异。物理 slot 的扫描顺序、identity、复用、
  birth visibility 和生命周期语义仍须按新权威对齐。
- 当前权威存在两个 RNG 流：MSVCR80 CRT 流，以及同步的 3000-byte table 流；不得把旧单流
  假设直接移植为新结论。
- playable 支持 30/60/120 render FPS，正式启动器请求 120；表现插值不能反写逻辑状态。

这些基线会使旧 NTSD 2.4 的 pass、timing、slot、RNG、frame、碰撞、输入、生命周期、render
handoff 和“已对齐”结论全部进入 `REBASELINE_REQUIRED`。在完成新权威源码闭环和必要运行证据前，
不得把旧验证状态直接继承为 NTSD 2.8-Logan 的完成证明。

## 4. 历史权威的废止边界

以下内容已被本决策废止为“当前行为权威”，只允许用于历史比较、迁移线索或回归夹具：

- `J:\QQFile\NTSD2.4\ntsd_release`
- `ntsd_new.exe`
- `src\entity\game_tick.cpp::game_tick(...)` 及该旧工程的 release live path
- `J:\QQFile\NTSD2.4\ntsd_release_C#`
- 基于上述旧权威形成的 Authority400、旧三方 trace、旧 Change Record、旧 Handoff、旧
  “VERIFIED / CLOSED / 已对齐”结论

旧 C#/NTSD 2.4 C++ 对齐 campaign 文档已按用户要求完成审计并由用户从工作树删除。它们不属于
当前恢复、决策、实现、验证或 Change continuation 的输入。Git 历史中的旧路径也不得被解释为
当前实施指令，不得因为上下文压缩、搜索命中或旧状态名而恢复。

## 5. 内容权威和当前用户范围决定

- 本次只迁移战斗规则、逻辑顺序、字段语义、时序、生命周期与可观察行为的权威。
- `GOVERNANCE-S0-UNITY-CONTENT-AUTHORITY-DIRECTION-B-001` 已冻结的 Unity
  `Assets/NTSD/Config` 内容数值权威当前仍有效，但用户已明确要求处理 Unity 与新权威的
  内容、数值和资源差异。整体切换、只补缺失或分类权威策略尚未决定；决定前只允许只读内容
  inventory，不授权覆盖 DAT/PNG/WAV、Prefab、Scene 或 importer。
- 用户明确要求处理旧 `NTSDSpec`（来源为旧 `LF2_19 properties.js`）；必须先用 2.8 live path
  替换所有生产调用，再决定是否删除空壳。
- 用户明确保留 Unity 的 Slot 容量模型、头顶血条、FootSelf、移动端底部黑区/平台取景、
  多边形战斗边界、当前随机掉武器路径和固定世界相机。
- 用户明确排除完整原生 HUD、结果页与战斗内结果信息表现、背景多层/cycle 和完整原生选择流程。
- 上述保留项会产生可观察差异，因此最终只能声明“非例外战斗域完全对齐，并保留用户批准例外”，
  不能声明整个应用逐像素无差异。
- 本次只修改文档和治理恢复入口，不修改 C#、Scene、Prefab、DAT、资源、ProjectSettings、
  C++ 权威目录或任何运行行为。
- 旧的 30 Hz、400-slot、单 RNG、pass 顺序和已完成状态只被标记为待重新核验；不在本次文档迁移
  中直接改 Unity 实现。

### 5.1 Direction-B current corpus 的强制读取口径

当前Unity DAT同时存在单行与多行WPoint/CPoint/ITR subblock。任何只匹配
`wpoint: ... wpoint_end:`等单行形式的raw grep都会系统性漏计，不能作为运行时reachability、dormancy或
“唯一witness”证据。B6已由`NTSD28-B6-DIRECTION-B-MULTILINE-CORPUS-CORRECTION-AUDIT-001`纠正该问题。

后续current-content行为计数必须：

- 使用Direction-B冻结的`artifacts/diagnostics/CAP-S0-1-unity-present-content-authority/normalized-projection.tsv`，
  或使用与正式`Lf2DatDecryptor -> Lf2DatParserV2 -> Lf2DatConverter`等价且覆盖单行/多行block的解析；
- 用当前`Assets/NTSD/Config/data.txt`区分indexed production definitions与138-DAT全manifest；
- 分开记录raw explicit property与converter后runtime value，尤其CPoint front/back旧alias；
- 与release decoded corpus比较时保持同一domain、primary/all-record和indexed/all-file口径。

held/refill reachability还必须合并两类正式relation producer：ITR kind2 ground pickup与OPoint kind2
direct-link。只统计前者得到的39个source definitions不是完整held domain。自
`NTSD28-B6-HELD-RELATION-PRODUCER-DOMAIN-CORRECTION-AUDIT-001`起，current完整union为41个source
definitions / 668个source-target edges；所有WPoint branch与missing-action join必须在该edge domain上计算，
不得把pickup target笛卡尔积或OPoint target全集无条件套到无关source。

B6还确认当前playable没有独立于current DAT frame的可变武器状态。non-character `hit_Fa`、physics、hit、
landing与pickup均从实体当前action对应frame读取state；Unity `NTSDEntityRuntime.WeaponState`及其
`1002→2000→3000`、Vx-halving prelude是旧迁移遗留，不能覆盖actual frame state。Direction-B与release均以
OID124 action40..55的16帧`state1002/hit_Fa12`循环证明该差异可达；详见
`NTSD28-B6-LEGACY-WEAPON-STATE-OWNER-AUDIT-001`。动态behavior/producer应先退休，carrier删除及
snapshot/checksum schema迁移仍须与`ReleaseTick` disposition共同取得用户方向。

non-character目标还必须区分三个physical-slot字段：+0x354 owner/credit、held DVX专属+0x2F8 excluded-group
source与hit_Fa专属+0x3F8 cached/preassigned target。Unity现有`PickerStableId`底层int可复用为+0x3F8，
但generic `LF2Entity`当前错误把target读写到`OwnerSlotIndex`，会污染attribution；不得把表面仍能追踪视为
等价。current 206个generic common frames、OID219 hit_Fa5→4与9条hit_Fa3→7 next链均证明可达。完整owner
与三包顺序见`NTSD28-B6-OBJECT-AI-TARGET-3F8-OWNER-AUDIT-001`；+0x2F8 consumer必须依赖target carrier/
producer闭合，不能继续复用Spawner或Owner。

OPoint kind2、kind5 substitution及type3 linked-owner也只使用上述reciprocal relation字段；当前playable没有
Unity `TrackerFlag`或managed `TrackerParent`第二层关系。Unity两个factory对current62条OPoint kind2额外写
flag/cache，正式shared kind5 consumer则已按link解析并绕过旧raw readers。动态producer和旧consumer/cache应分步
退休；删除runtime field与base-shell snapshot handle属于与ReleaseTick/WeaponState相同的联合schema方向问题。
详见`NTSD28-B6-LEGACY-TRACKER-RELATION-OWNER-AUDIT-001`。

Unity `GrabbedBy`也不是Authority的第二relation field；它只是旧signed mirror。current62条
OPoint-kind2会写child `-1`，但shared117条ITR-kind2完全不写，且唯一gameplay readers是两个
raw-kind5旧duplicate。该carrier进入runtime snapshot和ECS fingerprint却不进checksum/parity，不能作为
canonical或compat identity。后继先复用tracker consumer退休，再退nonzero writers；field/ECS/snapshot删除
 并入ReleaseTick/WeaponState/Tracker/HolderCopySlot联合schema方向。详见
`NTSD28-B6-LEGACY-GRABBEDBY-RELATION-MIRROR-AUDIT-001`。

Unity `HolderCopySlot` 更不是一个可绑定的Authority字段；它把direct linked parent、OPoint root、
legacy damage-stat credit、type3 relation copy和stage/self slot混在同一carrier。Authority对这些语义分别使用
`linked_parent_slot`、`owner_slot`、`battle_group`、`control_slot_000`或physical world slot。审计时Unity frozen
pair/BruteForce/HitPlan曾误以HolderCopy解析linked holder；该consumer binding已由
`NTSD28-B5-KIND5-LINKED-PARENT-SLOT-CORRECTION-001`纠正为HolderStableId/implicit-zero。type3 actual/HitPlan
仍额外传播HolderCopy，因此whole-continuation继续只保留core子集，下一步必须退休type3 extra writer。原审计见
`NTSD28-B6-LEGACY-HOLDERCOPY-MULTIPLEXED-SLOT-OWNER-AUDIT-001`。

Unity `KillCount` 同样不是Authority统计或owner字段：它混合了`ordinary_credit_gate_2f4`、AI
`owner_slot`、revival资格以及legacy state501/11xx child扫描。Authority damage累计使用
`input_hp_consumed_total +0x34C`、`input_score_total_348`、`knockout_count_358`，native combo显示另用
`combo_hit_count_1e0/combo_hit_last_tick_1e4`；Unity `ComboCountAtk/Vic`、`KillStat`与world
`DamageStats/KillStats`均不得继续作为并行真相。严格顺序先回到B2 AI、B3 child、B4 revival，再修B5
`+0x2F4`与旧stats；carrier删除必须将roster/results schema1→2纳入既有13/20/23联合方向。详见
`NTSD28-B5-LEGACY-DAMAGE-STATS-OWNER-AUDIT-001`。

既有`NTSD28-B0-OWNER-SLOT-BINDING-001`只验证trace字段绑定，不代表Unity production已写对`+0x354`。
正式session direct/stage实体的owner是自身physical slot，ordinary OPoint/native object-AI/weapon-piece child继承
source owner，F8固定99，state9996 clone默认-1。Unity当前缺direct/stage self、ordinary OPoint传播与F8 99，
且generic hit_Fa仍把OwnerSlot当`+0x3F8` target cache；必须先deconflict target再补producer。B2 AI虽已读
`OwnerSlotIndex`，在本前置闭合前仍不能称formal runtime一致。详见
`NTSD28-B0-OWNER-SLOT-PRODUCTION-OWNER-AUDIT-001`。

最前置target去混淆包`NTSD28-B0-OBJECT-AI-TARGET-3F8-DECONFLICTION-001 / FOCUSED_TEST_PASS / UNITY_RUNTIME_COMPILE_PASS / 7_OF_7_PASS / SELF_CHECK_BLOCKED_BY_UNRELATED_CPOINT / RUNTIME_PENDING`已复用既有
`PickerStableId`底层int作为canonical +0x3F8，只迁移Authority已闭合的common/4/7/11与5/6 child；
hit_Fa8/9/13、+0x2F8、持久化schema及raw inactive-slot裁决保持独立。runtime与临时纳入新test source的
Editor生成工程进程外编译均为0 error，且production/runtime已在20:13 Unity assembly reload无CS错误；
当前程序集SelfCheck实际执行后被更早的既有CPoint throw-Vz断言阻塞；首轮非法category已纠正，v2 Unity
EditMode focused于20:48实际通过7/7。Play与joint trace仍待做，但本包已满足后继owner producer的
Unity compile + focused前置。下一包按既定顺序进入direct/stage self-owner；F8固定99与ordinary OPoint
owner propagation仍不得抢先并入。

## 6. 后续恢复强制流程

1. 先读本文件、`Assets/NTSD/Docs/ntsd28-logan-vs-unity-battle-alignment.md` 和根
   `AGENTS.md`，再读目标模块文档。
2. 检索到 `NTSD24_AUTHORITY_SUPERSEDED` 时，只把该文档当历史证据。
3. 从第 2 节最接近问题的当前入口追踪正式 build closure 和完整调用链。
4. 明确区分“新权威已观察”“Unity 当前状态”“推断”“未知”和“用户确认”。
5. 从新对齐总表选择具名差异 ID；新建独立 Task/Change 后才允许修改 Unity 脚本。
6. 没有新权威证据的行为保持 `UNKNOWN / REBASELINE_REQUIRED`，不得沿用旧结论补写。
