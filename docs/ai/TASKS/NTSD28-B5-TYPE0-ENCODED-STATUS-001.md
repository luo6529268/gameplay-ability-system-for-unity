# Task Contract — NTSD28-B5-TYPE0-ENCODED-STATUS-001

> 状态：`VERIFIED / TYPE0_ENCODED_PRODUCER_ALIGNED / ARM_DEFERRED`
> 依赖：`NTSD28-B5-ITR-STATUS-FIELDS-001 / VERIFIED`

## 目标

在`BattleDamageWriter.ApplyStandardCharacterDamage`的type0 confirmed hit路径接入
`apply_confirmed_input_statuses`：精确保持13-field synchronized RNG顺序、编码判定与carrier写入。

## Authority 合同

- 顺序：poison→weak→bound→facing→manacle→delay→join→mimic→dx→dy→dz→gain→confus。
- 除poison外，encoded非零即先取`sync_next(callsite,100)`；chance=`encoded/1000`，chance非零且roll>=chance则不写，否则写`encoded%1000`。
- poison非零取一次100上界随机；chance=`poison%100`，chance==0或`roll+1<=chance`时解析payload：timer=`(payload/100)%1000`、type=`(payload/100)/1000`、strength=`payload%100`。
- confus成功后以callsite `0x004166B2`连续7次上界7随机生成0..6无重复remap；冲突只顺序+1取模，不额外消费RNG。
- type0 production在damage/resource之后、reaction之前调用；本包不实现紧随其后的join/mimic side effects或hit-motion arm。

## 不变量

- 0值不消费RNG、不写字段；失败chance保留原carrier。
- exact/shared type0共用BattleDamageWriter owner；非type0 producer后置。
- 不导入authority内容，不改Config/Authority/Scene/资源/snapshot schema。

## 验收

test-first覆盖0值、全字段success与20-call顺序、chance pass/fail、poison编码、confus permutation、
production integration与确定性；相关hit/RNG、NTSD28 broad、SelfCheck、Scene/Console/Ledger闭合。

## 回滚

移除producer helpers、production调用与focused test；ITR/runtime carrier保持。

## 验证结论

- test-first compile red：3个`CS0117`缺producer method。
- fresh compile error0；focused `d3124a84908244c5886e6fedfbacd94d` 4/4。
- RNG/hit related `f41c5ff0340f43ba8b201cebcac2da05` 216/216；精确NTSD28 broad `bc7e64a8d59449308c71331f844079e1` 567/567。
- SelfCheck `2026-09-05T10:42:11Z` PASS；Scene/Console/Ledger通过。
- arm、join/mimic immediate side effects与其他target types仍后置。
