# Task Contract — NTSD28-B5-TYPE3-KIND-CATALOG-TRANSFORM-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / IMPLEMENTATION_SEAM_DEFINED`
> 依赖：`NTSD28-B5-TYPE3-TARGET-GENERIC-CONTINUATION-001 / VERIFIED`

## 目标

只读闭合正式 NTSD 2.8-Logan type3 kind-catalog transform：确认 catalog 来源、record 顺序、bound/respond
匹配、identity/definition/action/history/relationship 写入、失败与后续分支；逐项映射 Unity 现有 OID8/209/213
硬编码、runtime identity API 与 HitPlan，输出可独立实施的最小包。

## 允许路径

- 只读 NTSD 2.8-Logan playable closure、正式 runtime kind 数据和 Unity 对应代码。
- 本 Task/Change、Ledger、STATE、handoff、总表与 type3 manifest。

## 不变量

- 本审计不修改 C#、DAT、Config、资源、Scene、Prefab、ProjectSettings 或权威目录。
- Direction B 下不得把 release kind 数据直接覆盖进冻结 Unity 内容；若 catalog 是规则常量，必须先证明其
  playable 消费方式和部署边界。
- 不将现有 OID 特判、方法名或历史 C#/2.4 行为当作正式语义。
- generic continuation、真正 kind9、effect override、pair/hold tail的已验证边界不得混写。

## 验收

形成 authority record/调用顺序/字段副作用表、Unity owner/gap 表、内容权威影响判断、实施拆包和回滚边界；
更新恢复文档并通过 Ledger validator。

## 最终结论

- 正式one-record及两个不同消费方向已闭合；candidate gate等价、transform事务确认存在差异。
- 现有carrier完整；实现不得部署release kind.dat，也不得依赖active OID209替代attacker definition。
- 下一包固定为`NTSD28-B5-TYPE3-KIND-CATALOG-TRANSFORM-001`，范围和矩阵见专项manifest。
