# Task Contract — NTSD28-B6-POST-THROW-SETTLEMENT-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / ACCOUNTING_FACTS_RETAINED / ROUTING_CORRECTED_BY_NTSD28-B6-CATCH-CONTROL-FLOW-FENCES-OWNER-AUDIT-001`
> 前置：CPoint throw environment/Vz代码已写但因Unity Licensing仍`RUNTIME_PENDING`

> 2026-09-08 路由纠正：本审计的 held-injury/accounting 事实与 owner 包继续有效；但其“下一首差”遗漏了
> hurtable/vaction 写入后的 target frame/kind-2 重新预检。后继 control-flow audit 已找到 release Yamato
> tree 的 134-pair witness，因此 settlement vaction preflight 必须排在 held-injury production 之前。

## 目标

在不依赖未恢复Unity运行时的前提下，继续只读审计 Authority `advance_catch_relations()` throw
tail之后的 dircontrol、`settle_catch_relations()` injury/cover/position、caughtact combo与后置
held/stage settlement，选择下一处可独立实施的正式内容可达差异，并保持被B7/B8/B11/H阻塞的完整
resource transaction显式后置。

## 范围

- Authority `battle_world.cpp` catch settlement与`simulation_tick_driver.cpp`后置调用。
- Unity `BattleCpointWriter.SyncCaughtByCpoint/ApplyHeldInjury/SyncHeldPosition`、runtime carriers、
  current CPoint corpus和相关SelfCheck/Play probes。
- 只读；不改C#、Config、Scene、Prefab、资源、ProjectSettings或Authority。

## 验收

- 冻结 settlement 分支顺序、正式cover/injury可达计数、字段写入与event边界。
- 区分可独立修复项和full-resource依赖项，选择单一下一包或记录等待前置验证。
- 同步Ledger/STATE/handoff/总表；不得把throw当前`RUNTIME_PENDING`提升为已验证。

## 回滚

仅移除治理记录；无行为、内容或Scene回滚。

## Current corpus correction（2026-09-08）

本记录的current小样本计数由multiline总correction替换：kind1 positive injury223，cover 0:205/1:15/11:3；
throwvx58且positive throwinjury48。accounting事实与owner保留，settlement preflight现另有OID417/40-pair
current witness。
