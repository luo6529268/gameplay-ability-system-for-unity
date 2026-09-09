# Task Contract — NTSD28-B3-C25I-ARMOR-RECOVERY-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / CORE_SEAM_DEFINED`
> 依赖：`NTSD28-B3-C25G-FRAME-BODY-001 / VERIFIED`

## 目标

只读闭合C25i gate、timer/reload状态机、Unity carrier与正式内容依赖，确定可在B3安全实施的programmatic core/placement，以及必须留给B5和H/B11的正式armor schema/content/hit producer。

## 结论

- `RuntimeArmorHp118`与`ArmorRecoveryTimer11C`的reset/copy/snapshot/checksum已具备。
- C25i production owner缺失，必须插在C25h之后、C25j之前。
- 可通过entity只读profile seam和程序化fixture先实现算法；默认无profile返回false，不改变当前正式内容。
- Unity没有armor block model/parser/converter，正式Authority armor18块与hit初始化不可在用户内容策略前接入，归B5+H/B11。

## 不做

不修改C#、Config、DAT、parser/model、Scene或Authority；实施另用`NTSD28-B3-C25I-ARMOR-RECOVERY-001`。
