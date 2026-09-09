# Task Contract — NTSD28-B5-REMAINING-EXIT-AUDIT-006

> 状态：`VERIFIED / GOVERNANCE_ONLY / SPECIAL_HIT_LATCH_LIFECYCLE_ROUTED`
> 依赖：`NTSD28-B5-WEAPON-DURABILITY-EXIT-AUDIT-001 / VERIFIED`

## 目标

继续对 B5 current-authority hit consume/write surfaces 做剩余退出审计，选择 weapon durability 后的
下一个独立、production-reachable first difference；本包只读，不修改脚本、Scene、内容或正式权威。

## 审计结论

下一首差是 `Entity28+0x0EB special_hit_latch` 的载体身份和生命周期：

- Authority 以独立 bool 保存，出生为 false；type3 kind9/ordinary continuation/locked-kind transform
  置 true；在实体生命周期内没有逐 tick false writer。
- 两条 hit consumer 都在解析 ITR/writer 前读取该 latch；已置位 attacker 面对 type0 target 时终止
  该 attacker 的后续候选消费。
- Unity 当前把它映到旧 `HitConfirm2`。shared candidate runner 的消费门已经存在，type3 producer
  也会写 1，但 C25 post-frame tail 与下一轮 candidate collect 都把它清零。
- `HitConfirm2` 还被普通 type1/2/4 weapon 命中临时写入，因此不能简单删除其清零并把所有旧 writer
  永久化；必须建立独立 `SpecialHitLatch0EB`。
- Unity current raw projection 没有 `specialHitLatch0eb`；C++ playable trace 已直接输出该字段，故
  carrier 包必须同时闭合 snapshot/checksum/parity/raw schema。

## 后续顺序

1. `NTSD28-B5-SPECIAL-HIT-LATCH-CARRIER-001`：独立 bool carrier、birth/reuse reset、copy、snapshot、
   checksum、parity、raw trace；行为暂不接。
2. 独立 owner audit：冻结四个 current-authority producer、两个 consumer gate、actual/HitPlan write surface；
   明确哪些旧 `HitConfirm2` writer继续保留为legacy临时语义。
3. test-first atomic integration：type3 producer改写新 carrier，candidate runner改读新 carrier，HitPlan
   shadow同步；验证跨 tick 保留、reuse清零和同 attacker 后续 type0 抑制。
4. family exit audit 后再回 B5 remaining scan。

## 排除

- B6 kind2/3 catch/cpoint关系行为；
- B7 lifecycle算法本身，除新 carrier 的出生/reuse reset合同；
- B8 results/combo产品规则、B10 audio/spark、H/B11内容迁移；
- 旧 R4/C#/NTSD 2.4 `hit_confirm2` 结论；
- 正式权威目录写入。
