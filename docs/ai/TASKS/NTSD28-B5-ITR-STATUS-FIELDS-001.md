# Task Contract — NTSD28-B5-ITR-STATUS-FIELDS-001

> 状态：`VERIFIED / ITR_STATUS_CONTRACT_READY / PRODUCER_DEFERRED`
> 依赖：`NTSD28-B5-STATUS-PRODUCER-AUDIT-001 / VERIFIED`

## 目标

为`InteractionArea`补齐13个status producer字段：delay/poison/confus/weak/manacle/join/mimic/
bound/facing/dx/dy/dz/gain，并贯通Converter、CopyFrom、ECS hit projection与fingerprint。

## 不变量

- 全部默认0；只建立数据合同，不执行status RNG、不写entity carrier、不改变命中结果。
- 保留literal `confus`；不得把`confuse`当别名。
- kind4/kind5 runtime replacement必须复制对应字段，ECS expected/observed fingerprint必须识别任一字段差异。
- 不导入authority 92条内容，不修改Unity Config、Authority、Scene、资源或snapshot schema。

## 验收

test-first覆盖parser/converter全部13项、default、CopyFrom、confuse typo不映射；相关parser/hit-plan测试、
精确NTSD28 broad、SelfCheck、Scene/Console/Ledger闭合。

## 回滚

删除13字段及converter/copy/projection/fingerprint写入与focused test；无数据迁移。

## 验证结论

- test-first compile red：53个缺字段/连锁错误。
- fresh compile error0；focused `b156533d430c4389a170e736b0253584` 5/5。
- parser/hit-plan/witness related `70af3a735207482982f8acc25f07d14d` 195/195；精确NTSD28 broad `ec1ff2f0cbe045a6b55b5e1bcbb64d38` 563/563。
- SelfCheck `2026-09-05T09:02:27Z` PASS；Scene/Console/Ledger通过。
- producer与authority content导入均未执行。
