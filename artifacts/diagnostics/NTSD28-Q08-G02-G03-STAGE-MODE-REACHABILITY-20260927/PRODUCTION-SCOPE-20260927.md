# Q08 G-02/G-03 stage gate 生产入口核对（2026-09-27）

状态：`READ_ONLY_SCOPE_MAPPED / PRODUCTION_NOT_CHANGED`。本项属于总表 `BATCH-04 / Q08`，不是 Q07 的重复验收。Q07 已过的角色、飞行物、非音效子体和持武器代表案例按总表 §0.14.4 复用。

正式规则依据：根 `NTSD2.8-Logan.exe` SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033` 的直接 LFR 输出及对应 playable `GameSession28::step` → `SimulationTickDriver28::step` → `BattleWorld28::settle_ordinary_stage_bounds`。所选 mode record 的 `+0x50` 为 1 或 3 时，所有 type0 槽都走角色 X 边界，特殊组谓词为 battle_group -1；其他值的低槽用普通角色边界，高槽用临时体边界。正式根三 tick 鸣人分身 slot50 在 gate1 下 X 为“未生/-42/0”；原 Editor 完整 Driver 的源/物理 X 为“未生/-42/-49”。根 LFR 与录制 Session 的 CRT RNG 状态不同，此差异只构成该边界的直接输出见证，不构成全状态同态证书，详 `root-lfr/ROOT-LFR-ACCEPTANCE.md`。

Unity 生产路径和最小改动边界：

1. `ProjectBattleModeConfig` 是用户批准的项目模式 Asset，目前只提供 combo/KO。所选 stage gate 必须从此类项目自有配置进入冻结的发布快照，并在 Battle 开始前写入确定性运行时。原版 `data/bg_mode.dat`、`data/bg/*.dat`、`data/mode.dat`、`data/mode/ntsd.dat` 均为用户排除项，不得作为生产输入。模式/地图选择与 gate 的映射须明确，不得直接把 `BattleGameModeId` 猜作正式的 background-mode record index。
2. `BattleEcsCharacterPreFrameBoundsPass.TryApplyExactCharacter` 写 DataOriented 物理 X 并调用 `NTSDEntityRuntime.ClampSourceRuleCharacterX`；`SimulationStageRenderModule.RunLegacyPreFrameBoundsAll` 经 `LF2Entity.ApplyPreFrameXBounds` 写 Legacy 物理 X 和同一源规则 X。三个写者必须消费同一个 gate 及组谓词，保留 D-024 双坐标比例和 D-025 非角色可行走区 304 tick 例外。
3. `BattleRuntimeState` 是 Client 所有的可恢复状态容器。`BattleStageRuntimeState`、`BattleMatchRuntimeState` 来自 `I:/GitHub/Unity_GAS/NTSD_Server/packages/com.ntsd.battle-kernel`；本包不得为了一个 Client 战斗门擅改共享 Kernel。新标量若放入 Client 容器，必须在 `Reset`、`BattleWorldCoreScalarSnapshot`、`BattleStateSnapshotRestore`、`BattleLockstepChecksumModule` 和完整 parity 输出中同一版本窗口接线；现有 core/aggregate/checksum schema 分别为 13/29/33。旧快照拒绝及两 profile 恢复须有聚焦证据。
4. `ProjectBattleModeConfig.Snapshot.Fingerprint` 进入 `LoganModeComboInput.InputFingerprint` 和发布内容身份。配置值改变须产生不同身份；不能只改 Asset 的序列化字段而不改冻结指纹，也不能在 tick 中临时读取 ScriptableObject。

下一个**生产** Task/Change 的退出条件：先为 gate0、gate1 和 gate3 的高槽 type0、低槽普通组/特殊组建立正式公式与原 Editor DataOriented/Legacy 正反测试；再完成配置发布、双写者、源规则 X、复位/快照/校验/内容身份的原子接线；旧快照拒绝、同 tick 重放、原 Editor 三 tick 鸣人分身、相邻 D-024/D-025 与有序退出须过。根 LFR RNG 不同的问题仍独立标记，不能用数值恰好相同掩盖。生产代码、Asset、DAT、Scene、共享 Kernel 及非战斗模块在本次只读核对中均未修改。
