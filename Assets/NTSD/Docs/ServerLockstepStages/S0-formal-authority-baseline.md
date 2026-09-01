# S0 — Formal Authority Baseline

> Current status: `WITNESS_FOCUSED_TEST_PASS / TEN_DOMAIN_CONTINUITY_READY / SHARED_DETERMINISTIC_FOUNDATIONS_READY / UNITY_POINT_FRAME_CHARACTER_DEPENDENCY_ORDER_FROZEN / OPOINT_VALUE_CONTRACT_AND_CORPUS_FROZEN / OPOINT_CLIENT_SEAM_ACTIVE / STANDING_CLIENT_AUTHORIZATION_ACTIVE / FRAME_RESOURCE_AND_PARSER_GATED / CHARACTER_CLIENT_RESOURCE_AND_SEAM_GATES_RECORDED / BACKGROUND_CONTENT_CONTRACT_AND_CORPUS_FROZEN / CONTENT_BUNDLE_CONTRACT_AND_CORPUS_FROZEN / SERVER_CONTENT_CONSUMER_BOUNDARY_FROZEN / NO_SERVER_ONLY_CONTENT_SOURCE_PACKAGE_READY / RESULTS_RESERVE_TERMINAL_INTEGRATION_READY / FULL_RETURN_COMMIT_SEAM_READY / WORLD_BOOTSTRAP_FACTORY_SEAM_READY / FORMAL_SHARED_KERNEL_PENDING`
> Formal phase status: `NOT_VERIFIED`
> Last reconciled from governed evidence on 2026-08-31. No new Unity run was performed by the latest governance packages.

## 1. Objective

Prove that one Server world and at least two Client worlds can run the same formal BattleKernel from the same StartBarrier and immutable input journal, producing the same deterministic result without any Socket, public network, prediction, or state overwrite.

## 2. Player-visible result

S0 itself adds no public networking. Its successful result is invisible but foundational: the same inputs must produce the same movement, attacks, collisions, RNG, objects, HP and events in every world. A mismatch must stop promotion and name the first differing tick/domain/slot rather than being hidden by a state correction.

## 3. Entry prerequisites and upstream handoff

- C++ release live runtime remains the only battle-rule authority.
- Unity single-player BattleKernel/tick path remains functional at fixed 30 Hz.
- Test construction must not make Transform, Animator, Scene, or presentation state authoritative.
- Client actions require a bounded pre-change Task/Change Record. `GOVERNANCE-S0-S9-STANDING-CLIENT-AUTHORIZATION-002` supplies continuing approval for Queue-selected packages inside retained boundaries；`S0-WITNESS-001` remains only its own focused-test evidence.

## 4. Data contracts and execution order

Required StartBarrier facts:

- SessionId and initial authority tick;
- protocol and policy version;
- rule/catalog/stage/build fingerprints;
- canonical roster and stable slot ownership;
- deterministic RNG initial state;
- formal-world factory identity.

Execution order:

```text
Create Server world + Client world A + Client world B
    ↓
Apply identical StartBarrier and seed
    ↓
For each authority tick, provide one identical immutable FrameInputSet
    ↓
Step the same formal Kernel exactly once in every world
    ↓
Compare aggregate checksum every tick
    ↓ mismatch only
Capture structured domain/slot/generation/RNG/event first difference
```

## 5. Decisions and evidence sources

- Global design: [`../server-lockstep-s0-s9-design.md`](../server-lockstep-s0-s9-design.md).
- Current evidence ledger: [`../server-lockstep-s0-s9-progress.md`](../server-lockstep-s0-s9-progress.md).
- Formal checksum witness gate: [`../../../../../NTSD_Server/docs/ai/AUDITS/S0-FORMAL-CHECKSUM-WITNESS-GATE-001.md`](../../../../../NTSD_Server/docs/ai/AUDITS/S0-FORMAL-CHECKSUM-WITNESS-GATE-001.md).
- C++ tick/capture mapping remains evidence work; generic initial authority tick is not automatically C++ `world.game_tick`.
- Held-only and zero-carry product decisions belong to S1/S2 and do not close this formal S0 proof.

## 6. Solution and package inventory

| Package/evidence | Current package status | What it proves | What it does not prove |
|---|---|---|---|
| `S0-SERVER-BOOTSTRAP-001` | `FOCUSED_TEST_PASS` | Independent .NET solution, boundaries, commands and no-network host | Formal battle parity |
| `S0-SERVER-INMEMORY-AUTHORITY-001` | `FOCUSED_TEST_PASS` | Generic authority session/TestKernel/checksum mismatch mechanism | Real NTSD formal Kernel |
| `S0-SERVER-ROOM-JOURNAL-001` | `FOCUSED_TEST_PASS` | Stable roster, append-only journal, sequential room owner | Cross-world battle result |
| Existing Unity S0 focused fixture | historical screenshot 5/5 pass | Pre-extension in-process behavior | Current seven-test witness contract |
| Existing lockstep fixture | fresh MCP 9/9 pass | Existing lockstep regressions | Formal shared-Kernel identity |
| `S0-WITNESS-001` | `FOCUSED_TEST_PASS / CLIENT_S0_WITNESS_READY` | Mismatch-only structured witness; real-entity 1 Server + 2 Client worlds; S0 7/7 and existing 9/9 fresh pass | Server shared formal Kernel, full C++ domain/event mapping or S0 VERIFIED |
| `S0-REAL-ENTITY-TEN-DOMAIN-CONTINUITY-001` | `FOCUSED_TEST_PASS / TEN_DOMAIN_CONTINUITY_READY` | Per-tick roster input consumption and all ten named hashes in the real-entity three-world fixture; S0 8/8 | Production runtime/shared Kernel/C++ behavior correctness |
| `CLIENT-FORMAL-KERNEL-DETERMINISTIC-RNG-SHARED-OWNER-001` | `FOCUSED_TEST_PASS / SHARED_RNG_OWNER_READY` | One Server-repository-owned C++-aligned RNG source/GUID consumed by Unity and .NET; frozen vector/direct/artifact/Unity/S0 regressions pass | Complete formal BattleKernel, remaining Cuts, C++ completed-tick/domain/event closure or S0 VERIFIED |
| `CLIENT-FORMAL-KERNEL-FRAME-INPUT-SEAM-001` | `FOCUSED_TEST_PASS / FRAME_INPUT_SEAM_READY` | Public platform-independent value/hash is separated from Client capture, reusable preallocation and dense diagnostics; compile, 4/4 seam, 44/44 related, S0 8/8, lockstep 9/9, SelfCheck and warmed 0 B pass | Shared physical source ownership, .NET consumption, remaining formal Kernel Cuts, marker promotion or S0 VERIFIED |
| `CLIENT-FORMAL-KERNEL-FRAME-INPUT-SHARED-OWNER-001` | `FOCUSED_TEST_PASS / SHARED_FRAME_INPUT_OWNER_READY` | One Server-owned physical FrameInput source/GUID consumed by Unity and .NET at `0.2.0`; direct/locked-artifact and full focused regressions pass | Complete formal BattleKernel, Cut C～G, marker promotion or S0 VERIFIED |
| `CLIENT-CPP-STAGE-SPAWN-REST-ALIGNMENT-001` | `FOCUSED_TEST_PASS / STAGE_SPAWN_REST_ALIGNMENT_READY / GOVERNANCE_CLOSED` | C++ clear-on-success与conflicting-lease atomic rollback已实现；Unity compile0、focused2/2、SelfCheck、S0 8/8、lockstep9/9通过 | Complete formal Kernel/Cut C-G/marker still pending; no S0 promotion |
| `CLIENT-FORMAL-KERNEL-SLOT-LIFECYCLE-SEAM-001` | `FOCUSED_TEST_PASS / SLOT_LIFECYCLE_SEAM_READY / GOVERNANCE_CLOSED` | Client-owned BCL provisional/side-effect/commit/rollback、本地Generation与canonical allocationEpoch seam完成；Unity seam5/5、related/SelfCheck/S0/lockstep与.NET双配置通过 | Cut C production shared-owner move、formal snapshot preservation、Cut D～G、marker与S0 VERIFIED仍待 |
| `CLIENT-FORMAL-KERNEL-SLOT-LIFECYCLE-SHARED-OWNER-001` | `FOCUSED_TEST_PASS / SHARED_SLOT_LIFECYCLE_OWNER_READY / GOVERNANCE_CLOSED` | 三个BCL source/GUID由Server-owned Core单一持有；`0.3.0` direct/locked artifact、Unity package1/1+related33/33+S0 8/8+lockstep9/9+SelfCheck及Server回归通过 | Cut D～G、formal snapshot/AI、marker与S0 VERIFIED仍待 |
| `GOVERNANCE-S0-CUT-D-REST-CHECKSUM-PROJECTION-BOUNDARY-001` | `ANALYSIS_COMPLETE / SEAM_FIRST_SELECTED / GOVERNANCE_CLOSED / NO_SOURCE_CHANGE` | RuntimeRestStore BCL core与checksum/snapshot reverse dependencies、C++ rest reset/tick/spawn顺序已映射；未找到BattleWorld restore | Later vector/seam closed；future source move still separately gated |
| `GOVERNANCE-S0-REST-STATE-CROSS-CONSUMER-CONTRACT-001` | `ANALYSIS_COMPLETE / GOLDEN_CORPUS_FROZEN / DUAL_GENERATOR_AND_DIGEST_PASS / GOVERNANCE_CLOSED` | 57-line Authority400/checksum与Unity capacity/lease corpus；SHA-256 `E10CF6D...34E8`；Client seam test consumes it unchanged | Later seam closed；future source move still separately gated |
| `CLIENT-FORMAL-KERNEL-REST-STATE-SEAM-001` | `FOCUSED_TEST_PASS / CUT_D_SEAM_READY / GOVERNANCE_CLOSED` | 零分配canonical traversal与checksum/snapshot adapter外移；source未移动；57-line/dense+sparse0B与全回归通过 | Later shared-owner/Cut E-G/formal marker仍需独立包与授权；S0仍NOT_VERIFIED |
| `CLIENT-FORMAL-KERNEL-REST-STATE-SHARED-OWNER-001` | `FOCUSED_TEST_PASS / SHARED_REST_STATE_OWNER_READY / GOVERNANCE_CLOSED` | RuntimeRestStore单一source/GUID现由Server-owned Core持有；`0.4.0` direct+locked、Unity1/1+26/26+17/17+21/21+SelfCheck、Server双配置回归通过 | Cut E-G、formal AI、marker与S0 VERIFIED仍待 |
| `GOVERNANCE-S0-CUT-E-WORLD-CORE-BOUNDARY-001` | `ANALYSIS_COMPLETE / WORLD_SCALAR_SEAM_SELECTED / GOVERNANCE_CLOSED` | Broad root/content/entity moves已否决；五个BCL scalar types为exact seam | 审计未改source/build/Unity；seam另包实施 |
| `CLIENT-FORMAL-KERNEL-WORLD-SCALAR-SEAM-001` | `FOCUSED_TEST_PASS / WORLD_SCALAR_SEAM_READY / GOVERNANCE_CLOSED` | 五个BCL scalar definitions已拆分且focused通过 | Shared-owner已由后续0bk闭合；不等于formal world |
| `CLIENT-FORMAL-KERNEL-WORLD-SCALAR-SHARED-OWNER-001` | `FOCUSED_TEST_PASS / SHARED_WORLD_SCALAR_OWNER_READY / GOVERNANCE_CLOSED` | 五个scalar single source/GUID与Unity/.NET consumers通过 | Concrete world/tick仍Client-only |
| `CLIENT-FORMAL-KERNEL-ROSTER-LABEL-SHARED-OWNER-001` | `FOCUSED_TEST_PASS / SHARED_ROSTER_LABEL_OWNER_READY / GOVERNANCE_CLOSED` | Roster/label single owner与双消费者通过 | Results/concrete world仍Client-only |
| `CLIENT-CPP-RESULTS-RESERVE-TERMINAL-INTEGRATION-001` | `FOCUSED_TEST_PASS / RESULTS_RESERVE_TERMINAL_INTEGRATION_READY / GOVERNANCE_CLOSED` | C++ terminal living-team/mode4 reserve/guard顺序focused闭合 | Formal shared owner/atomic result仍不存在 |
| `GOVERNANCE-S0-FORMAL-SNAPSHOT-MARKER-READINESS-001` | `ANALYSIS_COMPLETE / FULL_RETURN_COMMIT_SEAM_SELECTED / GOVERNANCE_CLOSED / READ_ONLY` | Snapshot=S3 foundation；immutable result/AI/event/marker gate已分层 | No source/runtime proof；marker false |
| `CLIENT-FORMAL-KERNEL-FULL-RETURN-COMMIT-SEAM-001` | `FOCUSED_TEST_PASS / FULL_RETURN_COMMIT_SEAM_READY / GOVERNANCE_CLOSED` | Early return在host result发布前终止；compile0、3/3、110/110、SelfCheck与Server双配置通过 | 不提供rollback/final schema/shared world/marker |
| `GOVERNANCE-S0-FORMAL-WORLD-ATOMIC-RESULT-BOUNDARY-001` | `ANALYSIS_COMPLETE / WORLD_BOOTSTRAP_FACTORY_SEAM_SELECTED / GOVERNANCE_CLOSED / READ_ONLY` | Terminal discard/no retry；rollback与final result已正确延期 | No source/build/Unity；marker false |
| `CLIENT-FORMAL-KERNEL-WORLD-BOOTSTRAP-FACTORY-SEAM-001` | `FOCUSED_TEST_PASS / WORLD_BOOTSTRAP_FACTORY_SEAM_READY / GOVERNANCE_CLOSED` | Exact construct/validate/logic-only/seed/roster seam与4/4+114/114+SelfCheck证据 | 不含content/stage/AI/shared owner/marker |
| `GOVERNANCE-S0-FORMAL-CONTENT-FACTORY-IDENTITY-BOUNDARY-001` | `ANALYSIS_COMPLETE / IDENTITY_TOKENS_UNBOUND / GOVERNANCE_CLOSED / READ_ONLY` | rule/catalog/stage token未绑定actual content；build/factory缺失；Unity verify-before-mutation顺序已映射 | No source/build/Unity/hash/DAT action；next0ce |
| `GOVERNANCE-S0-FORMAL-CONTENT-CANONICALIZATION-CONTRACT-001` | `ANALYSIS_COMPLETE / CANONICAL_IDENTITY_LAYERS_FROZEN / GOVERNANCE_CLOSED / READ_ONLY` | 冻结semantic manifest、ordering/normalization和version boundaries | No source/build/Unity/hash/DAT action；next0cf |
| `S0-SERVER-FORMAL-CONTENT-IDENTITY-VALUE-001` | `FOCUSED_TEST_PASS / SERVER_FORMAL_CONTENT_IDENTITY_VALUE_READY / GOVERNANCE_CLOSED` | Five domain/schema/sha256 values mandatory at Server StartBarrier；Debug/Release0/0、Server chain/no-network pass | No actual producer/binding；Client unchanged；marker false |
| `GOVERNANCE-S0-FORMAL-CONTENT-PRODUCER-BINDING-BOUNDARY-001` | `ANALYSIS_COMPLETE / NO_REAL_SERVER_ONLY_PRODUCER / GOVERNANCE_CLOSED / READ_ONLY` | 映射real producer、artifact timing与pre-world comparison；future factory构造前compare | No source/build/Unity/hash/DAT action；next0ch |
| `GOVERNANCE-S0-FORMAL-CONTENT-MODEL-CLOSURE-001` | `ANALYSIS_COMPLETE / CONTENT_GRAPH_LAYERED / STAGE_SPAWN_VALUE_SEAM_SELECTED / GOVERNANCE_CLOSED / READ_ONLY` | Full catalog + transitive validation；ordered content cuts frozen | No runtime action；next0ci |
| `CLIENT-FORMAL-KERNEL-STAGE-SPAWN-VALUE-SEAM-001` | `FOCUSED_TEST_PASS / STAGE_SPAWN_VALUE_SEAM_READY / GOVERNANCE_CLOSED` | Immutable eight-scalar spawn value + loader/normal/reserve adapters；4/4+23/23+SelfCheck | No shared owner in this package/hash/DAT/rule/tick/marker |
| `GOVERNANCE-S0-STAGE-SPAWN-CROSS-CONSUMER-CONTRACT-001` | `ANALYSIS_COMPLETE / UNITY_ORDER_MAPPED / GOLDEN_CORPUS_FROZEN / GOVERNANCE_CLOSED / READ_ONLY` | 14-line corpus and Unity integration order frozen | No source/build/Unity/shared move |
| `CLIENT-FORMAL-KERNEL-STAGE-SPAWN-VALUE-SHARED-OWNER-001` | `FOCUSED_TEST_PASS / SHARED_STAGE_SPAWN_VALUE_OWNER_READY / GOVERNANCE_CLOSED` | Single Server Core source/GUID；0.7.0 direct/locked、Unity7/7+27/27+SelfCheck | Phase/campaign containers/hash/DAT/factory/marker remain |
| `GOVERNANCE-S0-FORMAL-STAGE-CONTAINER-BOUNDARY-001` | `ANALYSIS_COMPLETE / GOVERNANCE_CLOSED / READ_ONLY` | Maps immutable phase/campaign content versus loader/runtime buffers and parser first difference | No source/runtime evidence in audit |
| `CLIENT-CPP-STAGE-CAMPAIGN-PARSER-DEFAULTS-ALIGNMENT-001` | `FOCUSED_TEST_PASS / STAGE_CAMPAIGN_PARSER_DEFAULTS_ALIGNED / GOVERNANCE_CLOSED` | Failed optional parse preserves C++ `-1/1`; compile0、4/4+30/30+SelfCheck | No container/package/hash change |
| `GOVERNANCE-S0-FORMAL-STAGE-CONTAINER-SEAM-CONTRACT-001` | `ANALYSIS_COMPLETE / STAGE_CONTAINER_SEAM_SELECTED / GOVERNANCE_CLOSED / READ_ONLY` | Exact immutable API/projection/order/comment/runtime split frozen | No source/build/Unity |
| `CLIENT-FORMAL-KERNEL-STAGE-CONTAINER-SEAM-001` | `FOCUSED_TEST_PASS / STAGE_CONTAINER_SEAM_READY / GOVERNANCE_CLOSED` | Immutable world content, defensive copy and atomic fail-closed；5/5+39/39+SelfCheck | Shared owner/hash/package remains later |
| `GOVERNANCE-S0-STAGE-CONTAINER-CROSS-CONSUMER-CONTRACT-001` | `ANALYSIS_COMPLETE / GOLDEN_CORPUS_FROZEN / DUAL_DIGEST_PASS / GOVERNANCE_CLOSED / READ_ONLY` | 10-line nested/duplicate/comment corpus and SHA frozen | No source/build/Unity |
| `GOVERNANCE-S0-FORMAL-BACKGROUND-CONTENT-AUTHORITY-BOUNDARY-001` | `IN_PROGRESS / READ_ONLY` | Release/Unity background simulation-content authority boundary | Unique active package；no source/Scene/build/Unity |
| `GOVERNANCE-S0-FORMAL-CONTENT-BUNDLE-BOUNDARY-001` | `ANALYSIS_COMPLETE / BACKGROUND_CONTENT_CONTRACT_REQUIRED / GOVERNANCE_CLOSED / READ_ONLY` | Immutable full catalog/stage bundle and preworld admission boundary | Closed prerequisite audit；no source/build/Unity |
| `GOVERNANCE-S0-FORMAL-CHARACTER-AUTHORITY-FIELD-CONTRACT-001` | `ANALYSIS_COMPLETE / IMMUTABLE_OBJECT_AND_SOURCE_ORDER_CATALOG_CONTRACT_FROZEN / GOVERNANCE_CLOSED / READ_ONLY` | Exact immutable Character/Object/source-order catalog contract | Closed；no source/build/Unity |
| `GOVERNANCE-S0-CHARACTER-CATALOG-CROSS-CONSUMER-CONTRACT-001` | `ANALYSIS_COMPLETE / GOLDEN_CORPUS_FROZEN / DUAL_DIGEST_PASS / GOVERNANCE_CLOSED` | Character/Object/catalog corpus | 24 LF/4039 bytes/SHA `5ACC30...1FE4` |
| `GOVERNANCE-S0-FRAME-FULL-CATALOG-EVIDENCE-CORRECTION-001` | `ANALYSIS_COMPLETE / FULL_CATALOG_PRODUCTION_PARSER_PROJECTION_CLOSED / GOVERNANCE_CLOSED / READ_ONLY` | Full decrypted 137-object Client/release Frame evidence correction | Entries15,395/15,377；52-OID resource gate；241 pair-token gate；no source/build/Unity |
| `CLIENT-FORMAL-KERNEL-CHARACTER-CONTENT-SEAM-001` | `GATED / CLIENT_INTEGRATION_REQUIRED / CHARACTER_CONTRACT_AND_CORPUS_REQUIRED / FRAME_AND_POINT_DEPENDENCY_GATED / RESOURCE_ALIGNMENT_REQUIRED / STANDING_CLIENT_AUTHORIZED` | Binary64 immutable Character/Object/catalog projection | Task not started；must not promote existing float |
| `CLIENT-CPP-COLLISION-SPRITE-INDEPENDENCE-001` | `GATED / CLIENT_INTEGRATION_REQUIRED / STANDING_CLIENT_AUTHORIZED` | Remove uncached collision dependency on sprite availability | Task not started；preserve Frame-center/Bdy/Itr behavior |
| `CLIENT-CONTENT-CHARACTER-SOUND-ALIGNMENT-001` | `GATED / CLIENT_INTEGRATION_REQUIRED / DECLARED_RESOURCE_SCOPE_REQUIRED / STANDING_CLIENT_AUTHORIZED` | Align 156 exact release weapon-sound identities | Task not started；no resource action before its exact batch Task/Change |
| `GOVERNANCE-S0-FORMAL-CHARACTER-CONTENT-AUTHORITY-BOUNDARY-001` | `ANALYSIS_COMPLETE / CHARACTER_CONTRACT_SELECTED / GOVERNANCE_CLOSED / READ_ONLY` | 137 objects、18 binary64 fields、417 width differences、156 sound differences、source-order semantics | No source/build/Unity/resource change |
| `GOVERNANCE-S0-FRAME-CROSS-CONSUMER-CONTRACT-001` | `ANALYSIS_COMPLETE / GOLDEN_CORPUS_FROZEN / DUAL_DIGEST_PASS / GOVERNANCE_CLOSED` | Frame value/set/presence/order/rejection corpus | 30 LF/4780 bytes/SHA `747C27...22BEC`; no source/build/Unity |
| `CLIENT-FORMAL-KERNEL-FRAME-VALUE-SEAM-001` | `GATED / CLIENT_INTEGRATION_REQUIRED / RESOURCE_ALIGNMENT_REQUIRED / POINT_DEPENDENCY_GATED / STANDING_CLIENT_AUTHORIZED` | Immutable Frame/set/Vaction/metadata adapters after all dependencies | Task not started; no Client change |
| `GOVERNANCE-S0-FORMAL-FRAME-AUTHORITY-FIELD-CONTRACT-001` | `ANALYSIS_COMPLETE / FRAME_CORPUS_SELECTED / GOVERNANCE_CLOSED / READ_ONLY` | 22 ints+sound+six lists/presence/ID sort/rejection contract frozen | No source/build/Unity/resource change |
| `CLIENT-CONTENT-FRAME-STRUCTURE-ALIGNMENT-001` | `GATED / CLIENT_INTEGRATION_REQUIRED / DECLARED_RESOURCE_SCOPE_REQUIRED / FIELD_BLOCK_SCOPE_FROZEN / STANDING_CLIENT_AUTHORIZED` | 52-OID exact field/block Frame corrections split into reviewable batches | No resource change before its exact batch Task/Change |
| `CLIENT-CPP-FRAME-MULTIVALUE-PARSER-ALIGNMENT-001` | `CLOSED / VERIFIED / UNITY_COMPILE_PASS / UNITY_FOCUSED_4_4_PASS / UNITY_RELATED_287_287_PASS / BATTLE_RUNTIME_SELFCHECK_PASS / SERVER_DUAL_CONFIGURATION_PASS / NO_RESOURCE_CHANGE` | Preserve two-value Itr properties and one-value secondary-zero contract | Package verified；no resource change |
| `CLIENT-CONTENT-FRAME-STRUCTURE-ALIGNMENT-001` | `CLOSED / VERIFIED / GOVERNANCE_CLOSED / COVERAGE_52_52_PASS / SIX_SEMANTIC_BATCHES_FROZEN / NO_RESOURCE_EDIT` | Parent Frame resource correction decomposition | Queue0dk-b scalar38 rows READY；S0 unchanged |
| `GOVERNANCE-S0-FORMAL-FRAME-AUTHORITY-BOUNDARY-001` | `ANALYSIS_COMPLETE / GENERIC_SCHEMA_VALID / CURRENT_CONTENT_INCIDENCE_SUPERSEDED / GOVERNANCE_CLOSED / READ_ONLY` | 22 release ints/sound/six lists and lookup/default/order remain；full incidence moved to Queue0do-c | No source/build/Unity/resource change |
| `CLIENT-CPP-WEAPON-STRENGTH-LEGACY-RETIREMENT-001` | `GATED / CLIENT_INTEGRATION_REQUIRED / STANDING_CLIENT_AUTHORIZED` | Retire unreachable strength runtime/storage path while preserving release holder-Itr kind-5 | Task not started; no Client change |
| `GOVERNANCE-S0-FORMAL-WEAPON-STRENGTH-AUTHORITY-BOUNDARY-001` | `ANALYSIS_COMPLETE / FORMAL_CONTENT_EXCLUDED / GOVERNANCE_CLOSED / READ_ONLY` | 7 blocks/28 entries, production call graph unreachable, 312 WPoint attacking zero, release schema absent | No source/build/Unity |
| `CLIENT-FORMAL-KERNEL-CPOINT-VALUE-SEAM-001` | `CLOSED / VERIFIED / UNITY_COMPILE_PASS / UNITY_FOCUSED_13_13_PASS / UNITY_RELATED_295_295_PASS / BATTLE_RUNTIME_SELFCHECK_PASS / CPOINT_CORPUS_SHA_PASS / WARMED_PRIMARY_0B_PASS / SERVER_DUAL_CONFIGURATION_PASS / CLIENT_INTEGRATION_REQUIRED / STANDING_CLIENT_AUTHORIZED` | Immutable 19-scalar/list/primary CPoint seam | Package verified；formal marker/S0 unchanged |
| `CLIENT-CPP-CPOINT-RESOLVED-HURT-ACTION-ALIGNMENT-001` | `CLOSED / VERIFIED / UNITY_COMPILE_PASS / UNITY_FOCUSED_5_5_PASS / UNITY_RELATED_238_238_PASS / BATTLE_RUNTIME_SELFCHECK_PASS / CPOINT_CORPUS_SHA_PASS / SERVER_DUAL_CONFIGURATION_PASS` | Two caught-hurt consumers read resolved injury/cover | Package verified；marker/S0 unchanged |
| `GOVERNANCE-S0-CPOINT-CROSS-CONSUMER-CONTRACT-001` | `ANALYSIS_COMPLETE / GOLDEN_CORPUS_FROZEN / DUAL_DIGEST_PASS / GOVERNANCE_CLOSED` | Exact 19-column/list/alias/sentinel CPoint corpus | 16 LF/3700 bytes/SHA `7FDEA9...75E88`; no source/build/Unity |
| `GOVERNANCE-S0-FORMAL-CPOINT-AUTHORITY-FIELD-CONTRACT-001` | `ANALYSIS_COMPLETE / CPOINT_CORPUS_SELECTED / GOVERNANCE_CLOSED / READ_ONLY` | 19 scalars/aliases/list identity/primary/runtime owner frozen | No source/build/Unity |
| `GOVERNANCE-S0-FORMAL-CPOINT-VALUE-BOUNDARY-001` | `ANALYSIS_COMPLETE / CPOINT_AUTHORITY_CONTRACT_SELECTED / GOVERNANCE_CLOSED / READ_ONLY` | Unity CPoint graph/release differences/33-block incidence | No source/build/Unity |
| `CLIENT-FORMAL-KERNEL-BPOINT-CATALOG-SEAM-001` | `CLOSED / VERIFIED / UNITY_COMPILE_PASS / UNITY_FOCUSED_7_7_PASS / UNITY_RELATED_78_78_PASS / BATTLE_RUNTIME_SELFCHECK_PASS / BPOINT_CORPUS_SHA_PASS / SERVER_DUAL_CONFIGURATION_PASS` | Ordered two-scalar BPoint list/primary/catalog writer | Package verified；marker/S0 unchanged |
| `GOVERNANCE-S0-BPOINT-CROSS-CONSUMER-CONTRACT-001` | `ANALYSIS_COMPLETE / GOLDEN_CORPUS_FROZEN / DUAL_DIGEST_PASS / GOVERNANCE_CLOSED` | Exact two-column/list/domain BPoint corpus | 13 LF/597 bytes/SHA `AD8B3E...81647`; no source/build/Unity |
| `GOVERNANCE-S0-FORMAL-BPOINT-CATALOG-VALUE-CONTRACT-001` | `ANALYSIS_COMPLETE / BPOINT_CORPUS_SELECTED / GOVERNANCE_CLOSED / READ_ONLY` | Two scalars/list identity/primary/catalog-only domain frozen | No source/build/Unity |
| `GOVERNANCE-S0-FORMAL-BPOINT-DOMAIN-BOUNDARY-001` | `ANALYSIS_COMPLETE / BPOINT_CATALOG_VALUE_CONTRACT_SELECTED / GOVERNANCE_CLOSED / READ_ONLY` | Renderer-only live use; catalog included/battle state excluded/current content0 | No source/build/Unity |
| `GOVERNANCE-S0-WPOINT-KIND5-FALLBACK-REACHABILITY-001` | `ANALYSIS_COMPLETE / STATIC_PRODUCTION_CALL_GRAPH_UNREACHABLE / GOVERNANCE_CLOSED / READ_ONLY` | Runner preprocess covers all current production modes/fallbacks | Direct internal test entry remains later fail-closed gate |
| `CLIENT-FORMAL-KERNEL-WPOINT-VALUE-SEAM-001` | `CLOSED / VERIFIED / UNITY_COMPILE_PASS / UNITY_FOCUSED_7_7_PASS / UNITY_RELATED_239_239_PASS / BATTLE_RUNTIME_SELFCHECK_PASS / WPOINT_CORPUS_SHA_PASS / WARMED_PRIMARY_0B_PASS / SERVER_DUAL_CONFIGURATION_PASS` | Immutable nine-scalar/list/primary/default/rejection WPoint seam | Package verified；marker/S0 unchanged |
| `CLIENT-CPP-WPOINT-DEFAULT-ALIGNMENT-001` | `CLOSED / VERIFIED / UNITY_COMPILE_PASS / UNITY_FOCUSED_10_10_PASS / UNITY_RELATED_232_232_PASS / BATTLE_RUNTIME_SELFCHECK_PASS / WPOINT_CORPUS_SHA_PASS / SERVER_DUAL_CONFIGURATION_PASS` | Missing kind/default fallback correction | Package verified；marker/S0 unchanged |
| `GOVERNANCE-S0-WPOINT-CROSS-CONSUMER-CONTRACT-001` | `ANALYSIS_COMPLETE / GOLDEN_CORPUS_FROZEN / DUAL_DIGEST_PASS / GOVERNANCE_CLOSED` | Exact nine-column/list/rejection WPoint corpus | 16 LF/1608 bytes/SHA `5A3B6B...5A926`; no source/build/Unity |
| `GOVERNANCE-S0-FORMAL-WPOINT-AUTHORITY-FIELD-CONTRACT-001` | `ANALYSIS_COMPLETE / WPOINT_CORPUS_SELECTED / GOVERNANCE_CLOSED / READ_ONLY` | Nine scalars/default/list identity/primary/extras frozen | No source/build/Unity |
| `GOVERNANCE-S0-FORMAL-WPOINT-VALUE-BOUNDARY-001` | `ANALYSIS_COMPLETE / WPOINT_AUTHORITY_CONTRACT_SELECTED / GOVERNANCE_CLOSED / READ_ONLY` | Unity WPoint graph/release differences/312-block incidence | No source/build/Unity |
| `CLIENT-CPP-ITR-PARSER-DEFAULTS-ALIGNMENT-001` | `GATED / CLIENT_INTEGRATION_REQUIRED / STANDING_CLIENT_AUTHORIZED` | First Unity Itr DTO/converter default/pair correction | Task not started; no Client change |
| `GOVERNANCE-S0-ITR-CROSS-CONSUMER-CONTRACT-001` | `ANALYSIS_COMPLETE / GOLDEN_CORPUS_FROZEN / DUAL_DIGEST_PASS / GOVERNANCE_CLOSED` | Exact 26-column Itr values/rejection/source-order corpus | 13 LF/3442 bytes/SHA `0F43B2...516D1`; no source/build/Unity |
| `GOVERNANCE-S0-FORMAL-ITR-AUTHORITY-FIELD-CONTRACT-001` | `ANALYSIS_COMPLETE / ITR_CORPUS_SELECTED / GOVERNANCE_CLOSED / READ_ONLY` | Unity-first 26-scalar/default/fingerprint/projection contract frozen | No source/build/Unity |
| `GOVERNANCE-S0-FORMAL-ITR-VALUE-BOUNDARY-001` | `ANALYSIS_COMPLETE / ITR_AUTHORITY_CONTRACT_SELECTED / GOVERNANCE_CLOSED` | Mapped Itr graph, incidence and first differences | No source/build/Unity |
| `CLIENT-FORMAL-KERNEL-BDY-VALUE-SEAM-001` | `GATED / CLIENT_INTEGRATION_REQUIRED / STANDING_CLIENT_AUTHORIZED` | Exact immutable Bdy Client seam | Task not started; no Client change |
| `GOVERNANCE-S0-BDY-CROSS-CONSUMER-CONTRACT-001` | `ANALYSIS_COMPLETE / GOLDEN_CORPUS_FROZEN / DUAL_DIGEST_PASS / GOVERNANCE_CLOSED` | 10-line/508-byte exact Bdy corpus | SHA `309F4F...EE650`; no code/build/Unity |
| `GOVERNANCE-S0-FORMAL-BDY-VALUE-CONTRACT-001` | `ANALYSIS_COMPLETE / CLIENT_SEAM_SCOPE_FROZEN / GOVERNANCE_CLOSED` | Exact X/Y/W/H and geometry contract | No source/build/Unity |
| `CLIENT-FORMAL-KERNEL-OPOINT-VALUE-SEAM-001` | `CLOSED / VERIFIED / CLIENT_INTEGRATION_REQUIRED / STANDING_CLIENT_AUTHORIZED / UNITY_COMPILE_PASS / UNITY_EDITMODE_52_52_PASS / BATTLE_RUNTIME_SELFCHECK_PASS / S0_WITNESS_PASS / EXISTING_LOCKSTEP_PASS / GOLDEN_CORPUS_SHA_PASS / SERVER_DUAL_CONFIGURATION_PASS` | Exact immutable OPoint Client seam | Package verified；formal marker/S0 remain unchanged |
| `CLIENT-FORMAL-KERNEL-BDY-VALUE-SEAM-001` | `CLOSED / VERIFIED / UNITY_COMPILE_PASS / UNITY_EDITMODE_212_212_PASS / BATTLE_RUNTIME_SELFCHECK_PASS / S0_WITNESS_PASS / EXISTING_LOCKSTEP_PASS / GOLDEN_CORPUS_SHA_PASS / WARMED_0B_PASS / SERVER_DUAL_CONFIGURATION_PASS` | Exact immutable Bdy Client seam | Package verified；marker/S0 unchanged |
| `CLIENT-CPP-ITR-PARSER-DEFAULTS-ALIGNMENT-001` | `CLOSED / VERIFIED / UNITY_COMPILE_PASS / UNITY_FOCUSED_6_6_PASS / UNITY_EDITMODE_212_212_PASS / BATTLE_RUNTIME_SELFCHECK_PASS / S0_WITNESS_PASS / EXISTING_LOCKSTEP_PASS / ITR_CORPUS_SHA_PASS / SERVER_DUAL_CONFIGURATION_PASS` | Itr parser default/pair correction | Package verified；marker/S0 unchanged |
| `CLIENT-CPP-WPOINT-DEFAULT-ALIGNMENT-001` | `CLOSED / VERIFIED / UNITY_COMPILE_PASS / UNITY_FOCUSED_10_10_PASS / UNITY_RELATED_232_232_PASS / BATTLE_RUNTIME_SELFCHECK_PASS / WPOINT_CORPUS_SHA_PASS / SERVER_DUAL_CONFIGURATION_PASS` | WPoint default correction | Package verified；formal marker/S0 unchanged |
| `CLIENT-FORMAL-KERNEL-WPOINT-VALUE-SEAM-001` | `CLOSED / VERIFIED / UNITY_COMPILE_PASS / UNITY_FOCUSED_7_7_PASS / UNITY_RELATED_239_239_PASS / BATTLE_RUNTIME_SELFCHECK_PASS / WPOINT_CORPUS_SHA_PASS / WARMED_PRIMARY_0B_PASS / SERVER_DUAL_CONFIGURATION_PASS` | Immutable WPoint Client seam | Package verified；formal marker/S0 unchanged |
| `CLIENT-FORMAL-KERNEL-BPOINT-CATALOG-SEAM-001` | `CLOSED / VERIFIED / UNITY_COMPILE_PASS / UNITY_FOCUSED_7_7_PASS / UNITY_RELATED_78_78_PASS / BATTLE_RUNTIME_SELFCHECK_PASS / BPOINT_CORPUS_SHA_PASS / SERVER_DUAL_CONFIGURATION_PASS` | Immutable BPoint Client catalog seam | Package verified；formal marker/S0 unchanged |
| `CLIENT-CPP-CPOINT-RESOLVED-HURT-ACTION-ALIGNMENT-001` | `CLOSED / VERIFIED / UNITY_COMPILE_PASS / UNITY_FOCUSED_5_5_PASS / UNITY_RELATED_238_238_PASS / BATTLE_RUNTIME_SELFCHECK_PASS / CPOINT_CORPUS_SHA_PASS / SERVER_DUAL_CONFIGURATION_PASS / CLIENT_INTEGRATION_REQUIRED / STANDING_CLIENT_AUTHORIZED` | CPoint resolved hurt-action correction | Package verified；Queue0dg READY；formal marker/S0 unchanged |
| `GOVERNANCE-S0-OPOINT-CROSS-CONSUMER-CONTRACT-001` | `ANALYSIS_COMPLETE / GOLDEN_CORPUS_FROZEN / DUAL_DIGEST_PASS / GOVERNANCE_CLOSED` | 10-line/852-byte exact OPoint corpus | SHA `236391...7360E`; no code/build/Unity |
| `GOVERNANCE-S0-FORMAL-OPOINT-VALUE-CONTRACT-001` | `ANALYSIS_COMPLETE / CLIENT_SEAM_SCOPE_FROZEN / GOVERNANCE_CLOSED` | Exact 8-scalar value/adapter/admission contract | Implementation remains separate |
| `GOVERNANCE-S0-FORMAL-POINT-VALUE-BOUNDARY-001` | `ANALYSIS_COMPLETE / OPOINT_VALUE_CONTRACT_SELECTED / GOVERNANCE_CLOSED` | Mapped Unity point graph and all point-family blockers | ObjectPoint selected; no source/build/Unity |
| `CLIENT-FORMAL-KERNEL-STAGE-CONTAINER-SHARED-OWNER-001` | `FOCUSED_TEST_PASS / SHARED_STAGE_CONTAINER_OWNER_READY / GOVERNANCE_CLOSED` | Single immutable source/GUID and aligned 0.8.0 consumers | 8/8+11/11+24/24、SelfCheck、direct/locked、Server dual；marker false |

## 7. Boundaries and forbidden shortcuts

- No real transport, ACK/Jitter, public listener, prediction, Gateway, database, snapshot recovery or independent-process requirement in S0.
- Do not use a generic TestKernel as the formal BattleKernel.
- Do not require S5 cross-process evidence to close S0; S0 is same-process multi-world.
- Do not allocate a full structured diagnostic snapshot every tick; capture detail after aggregate mismatch.
- Do not overwrite Client state from Server to make checksums match.
- Do not mark S0 `VERIFIED` from compilation, self-check, one NUnit fixture, or static analysis alone.

## 8. Acceptance and test matrix

| Layer | Required evidence | Current result |
|---|---|---|
| Server bootstrap/build | Debug/Release Server solution and no-network host | Passed at package scope |
| Existing Client compile/self-check | Fresh Unity compile and `BattleRuntimeSelfCheck` | Compile evidence and fresh PASS recorded; MCP Console `error CS` 0 |
| Existing focused tests | RNG package, S0 and lockstep fixtures | Fresh MCP RNG 1/1, S0 8/8 and lockstep 9/9, all 0 failed/skipped |
| Formal world identity | Same formal Kernel/factory in 1 Server + 2 Client worlds | Unity `SimulationWorld` three-host identity proven at focused scope; shared Server formal owner still pending |
| Journal parity | Same StartBarrier, seed and fixed input journal | 48-tick deterministic scenario plus 12-tick real-entity three-world fixture passed |
| Per-tick checksum | Continuous aggregate equality | Passed in current focused fixture; shared formal owner boundary remains pending |
| First difference | tick/domain/slot/generation/RNG/event witness | RNG and slot/generation typed first difference passed; formal C++ event/domain identity remains pending |
| C++ authority | Applicable live-path behavior and tick mapping | Pending where not already closed |
| 30 Hz budget | Formal multi-world test within bounded cost | Focused seven-test run completed in 1.7893356s; this is bounded fixture evidence, not production performance proof |

## 9. Failure disposition and return rule

On first mismatch, freeze the journal and witness, stop S0 promotion, and return to the formal Kernel/C++ alignment owner. Do not add protocol compensation in S1 or state overwrite in a Client adapter.

## 10. Exact exit gate

S0 becomes `VERIFIED` only when:

- one Server world and at least two Client worlds use the same formal Kernel;
- identical StartBarrier/seed/journal produces identical continuous per-tick checksums;
- a forced mismatch preserves the required typed first-difference witness;
- OfflineLocal policy and 30 Hz battle behavior are unchanged;
- required Client/formal runtime tests actually run and pass.

Until then the current status remains unchanged.

## 11. Handoff to S1

S1 may formally rely only on a verified SessionId, roster/SlotId ownership, initial tick, protocol/policy identity, immutable frame-input value and same-Kernel deterministic step boundary. Server-only S1 preimplementation does not waive this handoff gate.

## 12. Current blockers and next lawful action

- `S0-WITNESS-001` implementation/validation is complete at focused scope.
- Shared ownership now exists for deterministic RNG, FrameInput, slot/lifecycle, rest, world scalar and roster/label foundations; C++ Results terminal packages are focused ready. The complete formal BattleKernel remains absent (`KernelAbstractionsAssemblyMarker=false`).
- Queue0bz confirmed snapshot/recovery remains S3 and current Client snapshot is not cross-runtime formal proof. The immediate S0 gap is hidden early-return publication and the absence of an immutable completed-tick result.
- Queue0ca is focused closed: full-return publication is observable and early-return host results are rejected. Formal AI, versioned domain/event identity, rollback, complete shared world/tick and the final marker proof remain later named packages.
- Queue0cb is closed: terminal failed worlds are discarded/no-retry, rollback remains later, and a final result DTO is deferred until its real producer/schema exists.
- Queue0cc is focused closed; the seam remains Client-owned and makes no formal content/AI/shared-owner claim.
- Queue0cd is governance-closed: existing rule/catalog/stage values are unbound tokens, build/factory identity is absent, and formal verification must occur before world mutation.
- Queue0ce is governance-closed and Queue0cf is Server-focused closed: five typed identities are mandatory, but no actual content producer/binding exists and Client remains unchanged.
- Queue0cg is governance-closed: no real five-domain producer is Server-only and actual comparison belongs before future formal-world construction.
- Queue0ch is governance-closed; full-catalog/transitive validation and ordered content migration cuts are frozen.
- Queue0ci-0cq closed the stage-spawn and stage-container chains. Queue0cr-0ct mapped/froze OPoint contract and corpus；the historical Queue0cu authorization wait was superseded by G-22 and 0cu is now CLOSED, while the subsequent Bdy chain is also closed. DAT deployment, canonical hashes, actual producer/factory and marker remain excluded.

### 12.1 Consolidated S0 capability packages (2026-08-31)

Set by `GOVERNANCE-S0-S9-CAPABILITY-PACKAGE-CONSOLIDATION-001`; supersedes the field-level micro-row projection for all NOT-STARTED rows while preserving every CLOSED package above as prerequisite evidence:

1. `S0-FORMAL-CONTENT-CLOSURE-001` — one closure of the remaining formal simulation-content authority resource rows: Frame scalar 38 / Itr 10 / Bdy 55 / OPoint 36 / WPoint 36 / topology 56 (ex-0dk-b～g) plus the declared 156-row Character sound batch (ex-0dq). Retains the 0dk-b runtime-safety gate verbatim (`GATED / IMPLEMENTATION_NOT_STARTED / RUNTIME_SAFETY_CLEARANCE_PENDING`).
2. `S0-FORMAL-CONTENT-MODEL-INTEGRATION-001` — immutable content model, catalog, bundle, fingerprint, selection/admission and Server consumer closure (ex-0dn Frame seam, ex-0ds Character seam as internal checkpoints; binary64 must not promote existing floats).
3. `S0-FORMAL-KERNEL-ASSEMBLY-001` — assemble the completed shared owners into one formal Kernel executing `StepOneTick(FrameInputSet)`; no second Server-only battle implementation; 0dr is an internal candidate.
4. `S0-MULTIWORLD-EXIT-WITNESS-001` — evidence-only owner of the §10 exit gate.

Three-tier validation: internal checkpoint ≠ capability close ≠ phase exit (see the Server queue §4.2/§4.3). All four packages are GATED; the phase status remains `NOT_VERIFIED`.

## 13. Revision history

- 2026-08-31: `GOVERNANCE-S0-S9-CAPABILITY-PACKAGE-CONSOLIDATION-001` consolidated the not-started micro rows (0dk-b～g, 0dn, 0ds, 0dq) into the four capability packages of §12.1 with inherited acceptance matrices, retained the 0dk-b runtime-safety gate, and recorded three-tier validation. No CLOSED package, evidence or phase status changed.

- 2026-08-29: dossier created by `GOVERNANCE-S0-S9-STAGE-DOSSIERS-001`; no status change.
- 2026-08-30: `S0-WITNESS-001` fresh Unity MCP evidence closed at `FOCUSED_TEST_PASS / CLIENT_S0_WITNESS_READY`; S0 remains `NOT_VERIFIED` pending formal shared-Kernel/C++ mapping.
- 2026-08-30: `S0-REAL-ENTITY-TEN-DOMAIN-CONTINUITY-001` passed new1/1, fullS0 8/8, existing9/9 and fresh self-check; per-tick real-entity input consumption and ten-domain continuity are ready, while S0 remains NOT_VERIFIED for the separate formal-owner/C++ gate.
- 2026-08-30: `CLIENT-FORMAL-KERNEL-DETERMINISTIC-RNG-SHARED-OWNER-001` moved the only RNG source/GUID into the Server-owned UPM/.NET package. Direct/artifact and Unity focused/regression evidence passed; this closes only Cut A and leaves the formal marker false and S0 NOT_VERIFIED.
- 2026-08-30: `CLIENT-FORMAL-KERNEL-SLOT-LIFECYCLE-SHARED-OWNER-001` moved the three dependency-closed source/GUID owners into Server-owned Core at `0.3.0`; direct/locked-artifact, Unity focused/S0/lockstep/SelfCheck, Server and governance evidence passed. This closes only Cut C shared ownership; Cut D～G, formal snapshot/AI, marker and S0 verification remain open.
- 2026-08-30: `GOVERNANCE-S0-CUT-D-REST-CHECKSUM-PROJECTION-BOUNDARY-001` confirmed the rest core is BCL-only but not movable until checksum/snapshot projections are peeled outward. C++ reset/tick/spawn order was mapped, no BattleWorld restore was found, and the package changed no source or phase state.
- 2026-08-30: `GOVERNANCE-S0-REST-STATE-CROSS-CONSUMER-CONTRACT-001` froze a 57-line corpus and SHA-256 `E10CF6D...34E8` with PowerShell/JavaScript/document equality. No source or Unity action occurred; the next Client seam remains authorization-gated.
- 2026-08-30: `CLIENT-FORMAL-KERNEL-FRAME-INPUT-SEAM-001` separated the relocatable FrameInput value/hash from Client capture/preallocation/dense-trace owners. Unity compile, seam4/4, related44/44, S0 8/8, lockstep9/9, SelfCheck, warmed0B and governance passed. The separately authorized shared-owner Cut B later focused closed; Cut C slot/lifecycle seam is now independently authorized and active, while S0 remains NOT_VERIFIED.
- 2026-08-30: `CLIENT-FORMAL-KERNEL-FRAME-INPUT-SHARED-OWNER-001` moved the single FrameInput source/GUID into Server-owned Runtime/Abstractions and proved aligned `0.2.0` Unity/.NET direct/locked-artifact consumers plus full focused regressions. This closes only Cut B; marker false and S0 NOT_VERIFIED remain.
- 2026-08-30: `CLIENT-FORMAL-KERNEL-SLOT-LIFECYCLE-SEAM-001` closed the Client-owned dependency seam with canonical commit-only allocation epoch, rollback/side-effect guards, Unity `5/5` plus related regressions/SelfCheck/S0/lockstep and .NET Debug/Release evidence. It did not move production source or promote the marker; S0 remains NOT_VERIFIED.
