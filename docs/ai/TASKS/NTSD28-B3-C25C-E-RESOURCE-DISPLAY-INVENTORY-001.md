# Task Contract — NTSD28-B3-C25C-E-RESOURCE-DISPLAY-INVENTORY-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / FIELD_MATRIX_COMPLETE / CURRENT-MP-CONFLICT-FOUND`
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B3 C25c-e`
> 依赖：`NTSD28-B3-C25A-B-DEFINITION-CLONE-001 / VERIFIED`

## 目标

逐字段闭合当前Authority C25c pre-display resources、C25d asymmetric display values、C25e post-display resources与Unity现有runtime/Frame/DAT/catalog/规则载体的映射，分清可直接实施、必须新增代码载体、依赖B11内容策略及应留后续B5/B8的副作用，避免把旧简化recovery直接扩写成“已对齐”。

## Authority范围

- `battle_world.cpp`的`advance_native_resources_pre_display_slot`、`advance_native_display_values_slot`、`advance_native_resources_post_display_slot`。
- C23已确认的`resource_phase_12/resource_phase_3`只作为输入，不在本包改写。
- 记录phase gate、type/lifecycle/frame前置、字段读写、step crossing、clamp、KO/score/stage-center与full-restore副作用。

## 允许路径

- 本Task/Record、Ledger、STATE、handoff、总表。
- `docs/ai/MANIFESTS/NTSD28-B3-C25C-E-RESOURCE-DISPLAY-INVENTORY.md`（新增）。
- 既有C25 writer与pass-order manifest回链。

## 禁止

- 不修改C#、Scene、Prefab、ProjectSettings、Config、DAT、PNG/WAV或Authority目录。
- 不把缺失的stats/bmp/frame字段用常量或旧NTSD2.4行为代替。
- 不改变Direction B内容保护状态、C15随机武器例外或C25f-p。

## 验收

- C25c/d/e每个Authority字段均标记Unity owner或`MISSING`，并给出后续实施包路由。
- 明确C25d当前是否有正式逻辑载体，以及旧`PpDisplay`是否可复用。
- 明确哪些算法可在不改内容资产时实现，哪些必须等待B11内容/schema决策。
- Ledger validator通过；无脚本/资源/Scene改动。

## 回滚

删除本次新增审计文档和恢复进度回链；不回退已验证C25a-b。

## 结果

- C25c/d/e逐字段矩阵已写入`docs/ai/MANIFESTS/NTSD28-B3-C25C-E-RESOURCE-DISPLAY-INVENTORY.md`。
- C25d 8个display/step carrier全部缺失，`PpDisplay`明确不可复用。
- Authority locked non-background object DAT实测max_mp 158/158、cmp 5 files/7 hits、chp 2 files/4 hits；Unity content/schema均未承载。regen_dhp/regen_hp/regen_mp/frame_0mp在该locked object corpus为0 producer，但stats-record presence仍改变默认HP恢复gate。
- 找到硬阻塞冲突：旧B0 parity/raw把current_mp投影到Runtime.MP，而正式action cost、damage、C06和C25b使用Health.PP。下一包必须以非零资源变化trace唯一纠正，禁止双写。
- Ledger validator通过；未修改脚本、Config、Scene、Prefab、ProjectSettings或Authority。
