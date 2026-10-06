# 文件操作与未计划缺失索引

执行删除、移动、覆盖或丢弃内容前，先按 [审计合同](../file-removal-audit-contract.md) 建立记录并登记本表。状态变化追加到记录中，不删除历史事实。

| Operation / Event ID | 类型 | 状态 | 记录与证据 |
|---|---|---|---|
| NTSD28-336B44-Q07-HITFA10-COMMON-TARGET-001-EDIT-20261005 | 三脚本七文档精准编辑与新诊断 | VERIFIED | [Record](NTSD28-336B44-Q07-HITFA10-COMMON-TARGET-001-EDIT-20261005/RECORD.md) |
| NTSD28-336B44-Q07-HITFA1-3-DEAD-MOTION-GATE-001-EDIT-20261005 | 两脚本七文档精准编辑与新诊断 | VERIFIED | [Record](NTSD28-336B44-Q07-HITFA1-3-DEAD-MOTION-GATE-001-EDIT-20261005/RECORD.md) |
| NTSD28-336B44-Q07-HITFA3-INTEGER-PRECISION-001-EDIT-20261005 | 两脚本七文档精准增量，逐SHA保护dirty | VERIFIED | [Record](NTSD28-336B44-Q07-HITFA3-INTEGER-PRECISION-001-EDIT-20261005/RECORD.md) |
| NTSD28-336B44-Q07-HITFA-VERTICAL-PHASE-001-EDIT-20261005 | 共用纵向/整数阶段两个脚本限定文本增量及逐SHA before备份 | VERIFIED | [操作记录](NTSD28-336B44-Q07-HITFA-VERTICAL-PHASE-001-EDIT-20261005/RECORD.md) |
| NTSD28-336B44-Q07-HITFA14-COMMON-TAIL-001-EDIT-20261005 | 正式14共用尾部三脚本限定文本增量及逐SHA before备份 | VERIFIED | [操作记录](NTSD28-336B44-Q07-HITFA14-COMMON-TAIL-001-EDIT-20261005/RECORD.md) |
| NTSD28-336B44-Q07-D024-TRACKING-RULE-POSITION-001-EDIT-20261005 | 共用追踪source坐标三个脚本限定文本增量及逐SHA before备份；无删除/移动 | VERIFIED | [操作记录](NTSD28-336B44-Q07-D024-TRACKING-RULE-POSITION-001-EDIT-20261005/RECORD.md) |
| NTSD28-336B44-Q09-D024-SHADOW-REQUEST-20261005-001 | 原Scene平台阴影比例单次31tick请求CreateNew/消费inactive保留及唯一新结果更新；原请求不存在 | VERIFIED | [操作记录](NTSD28-336B44-Q09-D024-SHADOW-REQUEST-20261005-001/RECORD.md) |
| NTSD28-336B44-Q07-D024-R120-REQUEST-20261005-001 | 原Battle Scene R120中间alpha临时请求的有备份覆盖/消费/恢复及新结果更新 | RESTORED | [操作记录](NTSD28-336B44-Q07-D024-R120-REQUEST-20261005-001/RECORD.md) |
| NTSD28-336B44-Q07-D024-SPARK-REQUEST-20261005-001 | 新建临时 Game View 请求并由既有探针消费时覆盖其 requested 位 | VERIFIED | [操作记录](NTSD28-336B44-Q07-D024-SPARK-REQUEST-20261005-001/RECORD.md) |
| NTSD28-336B44-Q07-D024-SCENE-REQUEST-20261005-001 | 原 Battle Scene D-024 纵向 GREEN 临时请求的有备份覆盖/恢复 | VERIFIED | [操作记录](NTSD28-336B44-Q07-D024-SCENE-REQUEST-20261005-001/RECORD.md) |
| NTSD-MENU-FONT-3500-PLUS-REBIND-001-PREPARE | 代码与目标字体/菜单场景有审计的覆盖准备 | PARTIAL（代码完成，Scene/字库未写） | [记录](NTSD-MENU-FONT-3500-PLUS-REBIND-001-PREPARE/RECORD.md) |
| NTSD28-C050-SCENE-REQUEST-20261001-001 | 本包新建 C050 Scene Play 临时请求消费后自动清理 | VERIFIED（仅临时请求生命周期） | [操作记录](NTSD28-C050-SCENE-REQUEST-20261001-001/RECORD.md) |
| NTSD28-C051-ARMOR-SCENE-REQUEST-20261001-001 | 本轮新建护甲 Scene Play 临时请求消费后自动清理 | VERIFIED（仅临时请求生命周期） | [操作记录](NTSD28-C051-ARMOR-SCENE-REQUEST-20261001-001/RECORD.md) |
| NTSD28-C051-ARMOR-RAW-REQUEST-LIFECYCLE-20261001-001 | 本轮新建的护甲/无甲raw临时请求消费后自动清理 | VERIFIED（仅临时请求生命周期） | [操作记录](NTSD28-C051-ARMOR-RAW-REQUEST-LIFECYCLE-20261001-001/RECORD.md) |
| NTSD28-C051-RAW-REQUEST-LIFECYCLE-20261001-001 | C051左右诊断新生成的单一临时请求消费后清理 | VERIFIED（仅临时请求生命周期） | [操作记录](NTSD28-C051-RAW-REQUEST-LIFECYCLE-20261001-001/RECORD.md) |
| NTSD28-LOGAN-CONTENT-DISAPPEARANCE-20261001 | 未计划缺失；恢复尝试；外部恢复观察 | 原因/执行者未证；本任务恢复命令REJECTED；当前文件存在且Git无差异 | [调查记录](../../../artifacts/diagnostics/NTSD28-LOGAN-CONTENT-DISAPPEARANCE-20261001/REPORT.md)、[2974项清单](../../../artifacts/diagnostics/NTSD28-LOGAN-CONTENT-DISAPPEARANCE-20261001/deleted-tracked-paths.txt)、[96项保护哈希](../../../artifacts/diagnostics/NTSD28-LOGAN-CONTENT-DISAPPEARANCE-20261001/survivor-sha256-before.json) |

| NTSD-MENU-LOOP-CAROUSEL-001-PREPARE | 有审计的代码准备及文档追加 | PLANNED | [记录](NTSD-MENU-LOOP-CAROUSEL-001-PREPARE/RECORD.md) |


| NTSD-MENU-LOOP-CAROUSEL-001-SCENE-OBSERVATION | Menu末次磁盘SHA变化，写入者未证 | UNKNOWN_CAUSE | [记录](NTSD-MENU-LOOP-CAROUSEL-001-SCENE-OBSERVATION/RECORD.md) |
| NTSD-MENU-CAROUSEL-VISUAL-001-PREPARE | 有备份的循环列表视觉代码修改与治理追加 | VERIFIED | [记录](NTSD-MENU-CAROUSEL-VISUAL-001-PREPARE/RECORD.md) |
| NTSD-MENU-FONT-3500-REBIND-001-PREPARE | 将已有Menu对象从0480字库重绑至3500，保留预先改动 | VERIFIED | [记录](NTSD-MENU-FONT-3500-REBIND-001-PREPARE/RECORD.md) |
| NTSD28-336B44-Q07-C056-SCENE-SHUTDOWN-EDITOR-COMPILE-001 | 原Editor脚本编译的两个主要程序集预备覆盖 | VERIFIED | [记录](NTSD28-336B44-Q07-C056-SCENE-SHUTDOWN-EDITOR-COMPILE-001/RECORD.md) |

- NTSD-BATTLE-HUD-REFRESH-001 / PLANNED: [Record](NTSD-BATTLE-HUD-REFRESH-001/RECORD.md), current HUD script implementation; exact backups saved.

- NTSD-BATTLE-HUD-REFRESH-001 / VERIFIED (file operation only): script edits/backups complete; generated-project COMPILE_PASS, Unity runtime acceptance pending. See [Record](NTSD-BATTLE-HUD-REFRESH-001/RECORD.md).

- NTSD-BATTLE-HUD-EVENTS-001 / PLANNED: [Record](NTSD-BATTLE-HUD-EVENTS-001/RECORD.md), event-driven HUD; exact baseline preserved.

- NTSD-BATTLE-HUD-VIEW-TEST-REMOVAL-001 / PLANNED: [Record](NTSD-BATTLE-HUD-VIEW-TEST-REMOVAL-001/RECORD.md), exact user-authorized test script/meta deletion with verified backups.

- NTSD-BATTLE-HUD-VIEW-TEST-REMOVAL-001 / VERIFIED: exact two deletions confirmed; SHA backups and protected hashes verified in postcheck.json.

NTSD-BATTLE-CONTROLS-LIFECYCLE-001 / IN_PROGRESS: two local lifecycle fixes only; see docs/ai/CHANGE-RECORDS/NTSD-BATTLE-CONTROLS-LIFECYCLE-001.md and FILE-OPERATIONS/NTSD-BATTLE-CONTROLS-LIFECYCLE-001/RECORD.md. No Editor operation.

NTSD-BATTLE-CONTROLS-LIFECYCLE-001 / VERIFIED file operation; backups/prechange and postchange evidence retained.

NTSD-BUTTON-PRESS-CONSISTENCY-001 / IN_PROGRESS: left-pointer and built-in clear-state consistency only. See Change Record and operation; no Editor changes.

NTSD-BUTTON-PRESS-CONSISTENCY-001 / VERIFIED file operation: exact source backup and postchange hash retained.

- NTSD-BUTTON-MULTIPOINTER-001 / PLANNED: [record](NTSD-BUTTON-MULTIPOINTER-001/RECORD.md), authorized pointer aggregation and cleanup; exact backups/manifest under artifacts.

- NTSD-BUTTON-MULTIPOINTER-001 / VERIFIED: audited source edit and governance append; [record](NTSD-BUTTON-MULTIPOINTER-001/RECORD.md).

- NTSD-BUTTON-RIPPLE-001 / PLANNED: [NTSD-BUTTON-RIPPLE-001](NTSD-BUTTON-RIPPLE-001/RECORD.md), new opt-in ripple and isolated preview.

- NTSD-BUTTON-RIPPLE-001 / VERIFIED: new opt-in component/ring/prefab, isolated Unity actual capture. [NTSD-BUTTON-RIPPLE-001](NTSD-BUTTON-RIPPLE-001/RECORD.md).

- NTSD-BUTTON-RIPPLE-POOL-001 / PLANNED: [NTSD-BUTTON-RIPPLE-POOL-001](NTSD-BUTTON-RIPPLE-POOL-001/RECORD.md).

- NTSD-BUTTON-RIPPLE-POOL-001 / VERIFIED: audited source/prefab update and isolated actual preview; [NTSD-BUTTON-RIPPLE-POOL-001](NTSD-BUTTON-RIPPLE-POOL-001/RECORD.md).

- NTSD-BATTLE-HUD-NATIVE-RESOURCE-001 / PLANNED: [NTSD-BATTLE-HUD-NATIVE-RESOURCE-001](NTSD-BATTLE-HUD-NATIVE-RESOURCE-001/RECORD.md).

NTSD-BATTLE-HUD-EVENT-TEST-REMOVAL-001 / IN_PROGRESS: user-authorized exact HUD event test+meta removal, protected PP chain/other tests/Scenes; [Record](docs/ai/CHANGE-RECORDS/NTSD-BATTLE-HUD-EVENT-TEST-REMOVAL-001.md), operation docs/ai/FILE-OPERATIONS/NTSD-BATTLE-HUD-EVENT-TEST-REMOVAL-001/RECORD.md.

NTSD-BATTLE-HUD-EVENT-TEST-REMOVAL-001 / VERIFIED: exact HUD event test+meta deleted with recoverable byte backups; no live references;639 protected hashes unchanged; independent Editor compile0errors. Supersedes retention of event tests in earlier records only; PP fix and historical evidence retained. Evidence artifacts/diagnostics/NTSD-BATTLE-HUD-EVENT-TEST-REMOVAL-001/REPORT.txt.

NTSD-BATTLE-COMBO-INPUT-HISTORY-001 / IN_PROGRESS: input-key history view, completed-frame main-thread events; Record docs/ai/CHANGE-RECORDS/NTSD-BATTLE-COMBO-INPUT-HISTORY-001.md.

NTSD-BATTLE-COMBO-INPUT-HISTORY-001 / RUNTIME_PENDING (original Scene/device/worker): input-sequence event UI implemented; approved ComboPanel7sprite bindings only; independent compile0errors +isolated Unity35assertions PASS, actual DRA screenshot. PP/buttons/ripple unchanged. Record docs/ai/CHANGE-RECORDS/NTSD-BATTLE-COMBO-INPUT-HISTORY-001.md; artifacts/diagnostics/NTSD-BATTLE-COMBO-INPUT-HISTORY-001/REPORT.txt.

NTSD-BATTLE-COMBO-NATIVE-SEQUENCE-001 / IN_PROGRESS: replace display6history with realnative5history change snapshots, consumedvisualhold0.5s, always-visible background and KeyIcon array. Record docs/ai/CHANGE-RECORDS/NTSD-BATTLE-COMBO-NATIVE-SEQUENCE-001.md.

NTSD-BATTLE-COMBO-SCENE-VALIDATION-001 / VERIFIED: original saved BattleScene validation using temporary Editor probe; old loaded assembly explains reported4symptoms. Record docs/ai/CHANGE-RECORDS/NTSD-BATTLE-COMBO-SCENE-VALIDATION-001.md.

NTSD-BATTLE-COMBO-SCENE-VALIDATION-001 / VERIFIED: original saved BattleScene physical-device/controlledtick K,L-D-J full native consumption->UI tested; final137assertions includes tick assertions; widths406/573/740/907/1074; temporary probe removed; originalEditor refreshed/nonplaying/Scene clean and SHA unchanged. Report artifacts/diagnostics/NTSD-BATTLE-COMBO-SCENE-VALIDATION-001/REPORT.md. Old compiled DLL + hotreload empty keyIcons corrected by refresh/reload; only additional runtime edit is width formula. No full battle-alignment claim.

NTSD-BATTLE-COMBO-EXISTING-POOL-001 / PLANNED: replace Combo UI Instantiate with existing MMMiniObjectPooler; preserve latest user edits pending confirmation. Record docs/ai/CHANGE-RECORDS/NTSD-BATTLE-COMBO-EXISTING-POOL-001.md.

NTSD-BATTLE-COMBO-FIXED-SLOTS-001 / PLANNED: user-authored6icon/5arrow slots, remove View pooling. Record docs/ai/CHANGE-RECORDS/NTSD-BATTLE-COMBO-FIXED-SLOTS-001.md; file audit docs/ai/FILE-OPERATIONS/NTSD-BATTLE-COMBO-FIXED-SLOTS-001/RECORD.md.

NTSD-BATTLE-CONTROLS-BINDING-001 / PLANNED: wire Controls to selected-human immutable HUD InputId. Record docs/ai/CHANGE-RECORDS/NTSD-BATTLE-CONTROLS-BINDING-001.md; audit docs/ai/FILE-OPERATIONS/NTSD-BATTLE-CONTROLS-BINDING-001/RECORD.md.

| NTSD-BATTLE-DIRECTION-HYBRID-001 | Direction UI adapter edits, hookup and audited temporary probe removal | VERIFIED | [Record](NTSD-BATTLE-DIRECTION-HYBRID-001/RECORD.md) |

| NTSD-ROLE-SELECTION-UI-001 | Role selection scoped edits and backups | PARTIAL | [Record](NTSD-ROLE-SELECTION-UI-001/RECORD.md) |

| NTSD-KYUBI-SMALL-120X108-20261005 | User-requested 4t_kyubi_s.png resize 60x54 to120x108; original backup | PLANNED | [Record](NTSD-KYUBI-SMALL-120X108-20261005/RECORD.md) |

| NTSD-KYUBI-SMALL-120X108-20261005 | Completion:120x108,12960 pixel comparisons pass,meta unchanged | VERIFIED | [Record](NTSD-KYUBI-SMALL-120X108-20261005/RECORD.md) |

NTSD-WORDS-PREWARM-DEPENDENCY-001 / PLANNED / scoped prewarm source edit / FILE-OPERATIONS/NTSD-WORDS-PREWARM-DEPENDENCY-001/RECORD.md
# 2026-10-05 持有挂点原Scene请求操作

- `NTSD28-336B44-Q07-D024-HELD-ANCHOR-REQUEST-20261005-001 / PLANNED`：[Record](NTSD28-336B44-Q07-D024-HELD-ANCHOR-REQUEST-20261005-001/RECORD.md)，仅既有Airborne请求备份/消费/恢复与具名新结果更新，原Scene两tick消费者验证，不删除资源。

  后继状态`RESTORED`：单次PASS/DONE后原请求68字节/SHA严格恢复，新结果保留，Scene clean/SHA稳/Console0error；详同Record及after-manifest，PLANNED是执行前事实。

| NTSD28-336B44-Q07-HITFA-ACCELERATION-PRECISION-001-EDIT-20261005 | VERIFIED | 两脚本及七治理文档精确增量，before逐SHA保护dirty；无删除移动 | [Record](NTSD28-336B44-Q07-HITFA-ACCELERATION-PRECISION-001-EDIT-20261005/RECORD.md) |

| NTSD28-336B44-Q07-HITFA1-COMMON-TAIL-001-EDIT-20261005 | VERIFIED | 两脚本七文档精确增量＋新诊断cpp，before逐SHA保护dirty | [Record](NTSD28-336B44-Q07-HITFA1-COMMON-TAIL-001-EDIT-20261005/RECORD.md) |


| NTSD28-336B44-Q07-COMMON-TARGET-CACHED-TYPE-001-EDIT-20261005 | 两脚本七文档精确编辑与新诊断 | VERIFIED | [Record](NTSD28-336B44-Q07-COMMON-TARGET-CACHED-TYPE-001-EDIT-20261005/RECORD.md) |

| NTSD28-336B44-Q07-COMMON-TARGET-STALE-LIVE-001-EDIT-20261006 | 两脚本七文档精确编辑与新诊断 | VERIFIED | [Record](NTSD28-336B44-Q07-COMMON-TARGET-STALE-LIVE-001-EDIT-20261006/RECORD.md) |

- NTSD28-336B44-Q07-STALE-LIVE-ROOT-DOC-20261006 / PLANNED / 既有Q07包正式根倒地目标四tick见证及用户非战斗修改边界记录 / [Record](NTSD28-336B44-Q07-STALE-LIVE-ROOT-DOC-20261006/RECORD.md)

2026-10-05T17:25:40.204162+00:00 > **2026-10-06 正式根倒地目标四tick限定补证完成：** 同Task内正式336B44 EXE headless回放exit0/passed=true，206/frame54面对持续99/frame230/state14/HP500目标，四tick X500→500.7→502.1→504.2→507、Vx0.7/1.4/2.1/2.8、末动作0。目标仍在场/倒地，直接证明本例追踪运动继续；私有3F8未导出。复用旧健康目标Core/Unity11主体字段各55/55同，仅输出对照，非完整同初态/全World。正式地图Z钳制另列用户例外。未改C#/DAT/Scene/源码、未运行Unity/旧七项，11保护SHA稳。用户确认非战斗修改不引出全局恢复前置；父Record仍RUNTIME_PENDING，父Q/目标开放，229Record/68未关REUSE54/TRIGGER14不变。本出口完成，下一仅按当前总表实际首差/必要终验，停止源码历史扩检。 原件：artifacts/diagnostics/NTSD28-336B44-Q07-COMMON-TARGET-STALE-LIVE-20261006/root-lying-witness-20261006/REPORT.md

- NTSD28-336B44-Q07-STALE-LIVE-ROOT-DOC-20261006 / VERIFIED / 9文档追加与原件备份核对、正式根四步限定补证完成；生产/资源/Scene不改 / [Record](NTSD28-336B44-Q07-STALE-LIVE-ROOT-DOC-20261006/RECORD.md)

- `NTSD28-336B44-Q10-C032-EVIDENCE-REUSE-20261006` — `PLANNED`：仅同步C032已有自然voice/隔离软件PCM限定证据；4个文档精确范围及备份见 [NTSD28-336B44-Q10-C032-EVIDENCE-REUSE-20261006/RECORD.md](NTSD28-336B44-Q10-C032-EVIDENCE-REUSE-20261006/RECORD.md)。

- `NTSD28-336B44-Q10-C032-EVIDENCE-REUSE-20261006` — `VERIFIED`：4文档状态更正完成；18项原件检查通过、9项保护SHA稳定；无新脚本/Play/任务，父C032仍RUNTIME_PENDING。

- `NTSD28-UNITY-BATTLE-REALIGNMENT-CLOSURE-20261006` — `PLANNED`：用户明确收尾当前目标；9文档当前字节备份、限定证据与精确范围见 [NTSD28-UNITY-BATTLE-REALIGNMENT-CLOSURE-20261006/RECORD.md](NTSD28-UNITY-BATTLE-REALIGNMENT-CLOSURE-20261006/RECORD.md)。

- `NTSD28-UNITY-BATTLE-REALIGNMENT-CLOSURE-20261006` — `VERIFIED`：用户确认本轮按总表限定验收收尾；9文档备份/准确变更/保护SHA核对完成，无生产/Scene/资源修改；旧Record层级保留，后续另定目标。

| NTSD28-336B44-BATTLE-AUDIO-ALIGNMENT-20261006-PREPARE | RUNNING | 授权战斗音效代码与治理最小编辑，14项原字节备份；不删资源 | [NTSD28-336B44-BATTLE-AUDIO-ALIGNMENT-20261006-PREPARE](NTSD28-336B44-BATTLE-AUDIO-ALIGNMENT-20261006-PREPARE/RECORD.md) |

| NTSD28-336B44-BATTLE-AUDIO-ALIGNMENT-20261006-WAV-STAGE | RUNNING | 978战斗音效引用只新增缺失，保留既有12；不含BGM/未引用菜单音 | [NTSD28-336B44-BATTLE-AUDIO-ALIGNMENT-20261006-WAV-STAGE](NTSD28-336B44-BATTLE-AUDIO-ALIGNMENT-20261006-WAV-STAGE/RECORD.md) |

- `NTSD28-336B44-BATTLE-AUDIO-KIND9-FORMAL-CONFIRM-20261006` — `PLANNED`: formal kind9 read-only runtime confirmation; exact document backups and create-only diagnostics. [Record](NTSD28-336B44-BATTLE-AUDIO-KIND9-FORMAL-CONFIRM-20261006/RECORD.md).

- `NTSD28-336B44-BATTLE-AUDIO-KIND9-FORMAL-CONFIRM-20261006` — `RUNNING`: formal4 reports passed/16 actual audio ticks; raw4pass + directordinary2pass; original broad2fail and diagnostic failures retained. Exact backups in same Record.

- 2026-10-05T20:46:38.812613+00:00 NTSD28-336B44-BATTLE-AUDIO-KIND9-FORMAL-CONFIRM-20261006: VERIFIED file-operation closure; exact final manifest and 21 verified backup hashes in [RECORD](NTSD28-336B44-BATTLE-AUDIO-KIND9-FORMAL-CONFIRM-20261006/RECORD.md). Protected681/SHA336B44 unchanged; ledger/diff exit0; audio acceptance boundaries unchanged.

- 2026-10-05T20:54:11.712179+00:00 NTSD28-336B44-TRANSFORM-HITBOX-DIAGNOSIS-REQUEST-20261006: PLANNED, two original-Editor controlled DAT transformation captures; own request consumption recorded in [NTSD28-336B44-TRANSFORM-HITBOX-DIAGNOSIS-REQUEST-20261006](NTSD28-336B44-TRANSFORM-HITBOX-DIAGNOSIS-REQUEST-20261006/RECORD.md).

- 2026-10-05T20:59:50.490438+00:00 NTSD28-336B44-TRANSFORM-HITBOX-DIAGNOSIS-REQUEST-20261006: VERIFIED own request consumption and documentation updates; before/after/backups in [RECORD](NTSD28-336B44-TRANSFORM-HITBOX-DIAGNOSIS-REQUEST-20261006/RECORD.md). One schema failure retained, two v2 PASS, no production edits; USER_REPRO_PENDING.
