# Task Contract — NTSD28-B5-NATIVE-COMBO-RUNTIME-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / FIVE_PACKAGE_SPLIT_DEFINED`
> 依赖：`NTSD28-B5-REMAINING-EXIT-AUDIT-011 / VERIFIED`

## 目标

只读冻结native combo runtime的字段、world mode配置、普通命中producer、state9 caughtact producer、
C25后expiry、snapshot/checksum及B10 presentation边界，给出可独立验证的最小实施包顺序。

## 边界

- 只读Authority playable closure、正式combo记录、Unity runtime/host/hit/catch/snapshot现状及既有记录。
- 不改C#、Scene、Prefab、Config、资源、ProjectSettings或Authority目录。
- 不复用输入`ComboD*`或伤害累计`ComboCountAtk/Vic`；不在本审计导入`combo_hits.png`或实现HUD。
- caughtact producer若依赖未对齐B6 settlement event，必须显式后置，不得伪造近似事件。

## 验收

- 明确entity/world carrier的默认、reset、copy、snapshot、checksum与restore责任。
- 明确ordinary producer的applied-hit边界、one-hop owner规则、target type和facing gate。
- 明确expiry的精确tick、inclusive边界、record/negative respond gate及pass owner。
- 输出实施包依赖序列并同步Ledger/STATE/handoff/总表。

## 回滚

仅移除本次治理记录；没有行为、内容或Scene回滚。

## 结果

1. `NTSD28-B5-NATIVE-COMBO-CARRIERS-001`：entity count/last-tick与world record/bound/facing/respond/
   caughtact，完整reset/copy/ECS/snapshot/checksum/restore；默认record absent，不越权激活内容。
2. `NTSD28-B5-NATIVE-COMBO-ORDINARY-PRODUCER-001`：shared candidate runner在普通Damage成功后、
   first-BDY成功分支之外生产；严格target type0、bound1、facing和one-hop owner规则。
3. `NTSD28-B5-NATIVE-COMBO-EXPIRY-001`：C25 live-slot尾之后执行inclusive expiry；不复用输入timeout10。
4. `NTSD28-B6-NATIVE-COMBO-CAUGHTACT-PRODUCER-001`：等待真实catch settlement injury event后接线。
5. `NTSD28-B10-NATIVE-COMBO-PRESENTATION-001`：等待逻辑carrier/producer后实现图集、几何和命中数字命令。

正式`ntsd.dat` tuple的默认激活属于H内容策略；在用户尚未选择内容权威方案前，carrier保持可注入但默认关闭。
