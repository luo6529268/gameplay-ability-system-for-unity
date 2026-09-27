# BATCH-04 / Q08 G-04 所选普通战斗模式字段归属

状态：`READ_ONLY_SELECTED_MODE0_CLASSIFIED / NO_NEW_CONFIRMED_LOGIC_FIRST_DIFFERENCE / G04_AGGREGATE_OPEN`。只针对现有选中护甲 HP3 根 EXE 诊断的 mode 0、stage 23，不代表其它模式或所有结果状态。

正式根 `NTSD2.8-Logan.exe` 的新鲜 SHA-256 为 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。读取 `NTSD28-Q08-ARMOR-RESULT-MILESTONES-001/formal-hp3-365-rooted-trace.jsonl` 首行得到下表正式选中值；该报告所记录的终点 LFR 失败不影响首行模式标量的实际 EXE trace 身份，但也不把它变成全状态同态证书。`source/ntsd28_playable/src/game_session.cpp` 的 `project_native_background_mode_record28` 将正式选中 mode 记录投影到 BattleConfig；`start_selected_battle` 在 World 建立时写 hit/resource 字段，`tick_options()` 在每个战斗 tick 提供恢复、stage、dynamic/drop 等字段。Unity 项目按用户决定不使用原版背景及两类 mode DAT，生产入口改由 `ProjectBattleModeConfig.asset` 提供项目自己的值；下表只比较**规则消费与本次所选值**，不要求复制被排除的 DAT。

| 正式字段 / 本次值 | Unity 当前生产归属 | 本次处置 |
|---|---|---|
| `selectedModeHitGroupGate18=0`、`selectedModeAttackingPercent1C=0` | `NTSD28HitResourceRulesRuntimeState` 默认均 0，命中/伤害 writer 读取；已有快照、checksum。 | 所选值无已证首差；非零选中值需另有项目配置与同条件入口才测试。 |
| `selectedModeAttackerInjuryMpPercent34=75`、`selectedModeTargetInjuryMpPercent38=75` | 同一 runtime 默认均 75，伤害 writer 读取。 | 所选值无已证首差；不能仅因 Asset 尚无同名 Inspector 字段改正式 DAT 或生产字段。 |
| `selectedModeDefaultHpRegenGate28=1`、`selectedModeMpRegenGate2C=1` | `BattleRecoveryStatusWriter` 默认均 1，恢复 pass 传入。 | 所选值无已证首差；其它选中模式需独立可达证据。 |
| `selectedModeStageGate50=1` | 项目模式 Asset 的选中 gate1 已在首 tick 前发布，双 profile、源/物理 X、快照与原 Battle Scene Play 有 `NTSD28-Q08-PROJECT-STAGE-GATE-001` 限定验收。 | `VERIFIED_SCOPED`，不重做此模式字段。 |
| `selectedModeDynamicBoundaryGate48=0`、`stageProgressBoundary4A0BB4=0` | 正式 `battle_world.cpp` 在 stage progress>0 时，以 gate1 **或**实体 render phase0 允许动态收窄；本次 progress0 使该收窄入口不成立。Unity项目地图/边界及 D-025 是批准例外。 | 本次没有动态收窄首差；不能把 gate0 误述为所有 phase 都禁用，也不能借 G-04 接回原版背景边界。 |
| `selectedModeDropGate4C=2` | Unity 随机掉武器路径是总表 §1.2 已批准的 gameplay 例外。 | 作为例外单列，不以正式 2 覆盖项目路径。 |
| `selectedModeReviveLivesGate54=0` | 正式 `render_snapshot.cpp` 将它用于续命数字和 nameplate 门；所检 Unity 战斗脚本无同名生产载体。 | 归 Q09/P-09、P-10 的**表现**审计；本次 0 不证明画面已对齐，不能把缺字段直接写成 Q08 规则首差。 |
| `selectedModeEtcMode=1` | 正式 `render_snapshot.cpp` 的 state9997 owner-relative/viewport 画面分支使用；当前 Unity 无同名字段。 | 归 Q09/P-12，且须先核对固定相机及已批准平台取景例外的精确边界；`data/etc.dat` OID998 含 state9997，但静态 DAT 行不是自然战斗出生见证。 |
| `battleMode=0` | Unity 当前选中普通战斗的独立 Match mode0 与 native 结果命令2已有各自限定证据。 | 其它 mode2/3/4 的结果目的地仍依总表保留权威面门槛。 |

这份分类拒绝两个不成立的捷径：其一，不能因项目 Asset 未暴露所有正式字段就直接复制排除的背景/mode DAT 数值；其二，不能因所选 mode0 的默认标量吻合便宣布 G-04、Q08 全部对齐。下一次 G-04 **生产修改**需先给出某个项目选中模式的非默认配置、正式规则消费者和同条件的 Unity 首差，或给出新正式可达的 mode 结果/事件入口；再单独建立准确 Task/Change。独立的 Q09 表现门按 P-09/P-10/P-12 处理，不反压 Q07 已过的资源与实体矩阵。

本轮仅静态源码/现有 EXE trace 与项目 Asset 核对。未运行 Unity、Play 或新正式 EXE，会话首行模式字段之外的全状态等价没有验证；无生产脚本、Asset、DAT、图片、Scene 或非战斗修改。
