# NTSD28-B3-C25C-E-RESOURCE-DISPLAY-INVENTORY-001 — C25c-e resource/display inventory

<!-- CHANGE-RECORD
id: NTSD28-B3-C25C-E-RESOURCE-DISPLAY-INVENTORY-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: NONE
authority: Promoted NTSD 2.8-Logan BattleWorld28 C25c-e slot writers; EXE B1E13AE1, playable closure 39DDDA15.
evidence: AUTHORITY-READ-ONLY / UNITY-READ-ONLY / FIELD-MATRIX-COMPLETE / CURRENT-MP-BINDING-CONFLICT / AUTHORITY-MAXMP-158-CMP-7-CHP-4 / UNITY-SCHEMA-MISSING / NO-CODE-OR-ASSET-WRITE / LEDGER-PASS
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / FIELD_MATRIX_COMPLETE / CURRENT-MP-CONFLICT-FOUND`

## 改前事实

- C25 writer总表只把Unity recovery标为C25c/e的部分近似，并确认C25d完整owner缺失；尚未形成字段级实施边界。
- Authority C25c-e同时依赖global phase、definition stats/bmp、frame cmp/chp、mode rules、display累积字段和多个战斗副作用，不能作为一个旧recovery方法直接替换。

## 计划

读完Authority三个slot writer与Unity runtime/content/consumer后建立字段矩阵，再决定后续carrier、algorithm、content-blocked包；本审计不改行为。

## 结果

- C25c的phase、HP三面、input cost/environment已有部分carrier；weak/regen/status/score/KO/rules大量缺失。旧recovery对所有type0做HP+1且每3 tick恢复PP，与普通Authority mode gate不等价。
- C25d 4个display value与4个step均缺失；Unity`PpDisplay`是旧输入成本累计面，语义不同。
- C25e previous-action/HP/life/group/stage载体部分可用；frame_0mp、full restore、max_mp clamp及current MP写入仍有schema/carrier/binding前置。
- 发现`current_mp`绑定冲突：`Tools/NTSD28Parity`与raw exporter读`Runtime.MP`，生产输入资源链/C06/C25b读写`Health.PP`。初值500会掩盖该冲突，必须以非零变化joint trace纠正。
- Authority content计数与完整字段表见manifest；本包无脚本/asset/Scene改动。

## 下一步

先执行`NTSD28-B3-C25C-E-CURRENT-MP-BINDING-CORRECTION-001`，再做runtime carriers；内容schema/值仍受B11用户策略gate保护。
