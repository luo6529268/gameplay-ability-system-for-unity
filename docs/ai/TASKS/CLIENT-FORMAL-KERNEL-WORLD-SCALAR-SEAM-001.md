# Task Contract — CLIENT-FORMAL-KERNEL-WORLD-SCALAR-SEAM-001

> 状态：`FOCUSED_TEST_PASS / WORLD_SCALAR_SEAM_READY / GOVERNANCE_CLOSED / USER_STANDING_AUTHORIZED / CLIENT_INTEGRATION_REQUIRED / SOURCE_SEAM_ONLY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`

## 1. 目标与范围

仅将`BattleMatchRuntimeState`、`BattleStageRuntimeState`、`BattleStageProgressionState`、`BattleSlotRuntimeState`、`BattleFlowRuntimeState`五个BCL-only定义从混合`BattleRuntimeState.cs`拆入新的Client-owned `BattleWorldScalarState.cs`。新增focused structural/behavior test；不改字段、默认值、方法体、调用者、package或任何其他runtime。

## 2. 验收

先取得new owner file absent的结构红灯；再证明单一Client source owner、defaults/reset/stage-bound math与authority/metadata分类，运行Unity compile、scalar/snapshot/checksum/restore/worker、S0、lockstep、SelfCheck与治理。

## 3. 禁止

禁止移动Roster/SlotLabel/Results/stage campaign/root/entity/content/catalog；禁止改battle rules、30Hz、tick/pass/input、Scene/资源/Input Actions、checksum/snapshot/recovery、formal AI、wire/transport/数据库/公网或marker。

## 4. 关闭证据

- 拆分前job `8f3b78ebafda4def8e93d9ea52cd94d7`为4 pass/1 expected fail，唯一失败是owner file尚不存在。
- 拆分后Unity compile过滤`error CS=0`；focused job `03a44939601a44c0b798f3270b74cae0`为5/5。
- 相关scalar/snapshot/checksum/restore/worker/S0/lockstep job `34cc470c4004414ab9c36b250eca6bbd`为83/83。
- `Temp/NTSD_BattleRuntimeSelfCheck.result`于2026-08-30 20:26:09 +08:00为`PASS`。
- 仅完成五类型Client source seam；shared owner、package、marker和S0/S5状态均未变化。
