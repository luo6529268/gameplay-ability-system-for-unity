# Task Contract — NTSD28-B5-STATUS-PRODUCER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / PRODUCER_SPLIT_DEFINED`

## 目标

只读闭合state12/18 status consumer对应的B5 producer、ITR字段数据合同、RNG顺序、target-type范围与
Unity production owner，定义后续最小实施包。

## 结论

- Authority有两类producer：confirmed encoded status与unarmored hit-motion arm。
- encoded顺序严格为poison、weak、bound、facing、manacle、delay、join、mimic、dx、dy、dz、gain、confus；每个非零encoded都消费一次synchronized RNG，confus成功后还消费7次remap RNG。
- `apply_encoded_status`先取`roll[0,100)`；chance=`encoded/1000`，chance非零且`roll>=chance`时不写，否则写`encoded%1000`。poison有独立编码/判定。
- hit-motion arm在unarmored reaction后执行；仅`Fall==80 && -5<Vx<5 && itr.dvx==0`旁路。否则写hit-facing、dx/dy/dz、gain=1、picked/picking默认191/185，并把itr.dvz加入pending hit Z accumulator。
- Unity `InteractionArea`缺13个字段：delay/poison/confus/weak/manacle/join/mimic/bound/facing/dx/dy/dz/gain；Converter、CopyFrom、ECS `ItrProjection`和两套fingerprint同样缺失。
- Unity当前冻结Config的ITR中这些字段为0条；NTSD 2.8 runtime decoded DAT有92条。Direction B下不得借producer任务覆盖Unity内容，但数据合同与规则实现仍可先补。
- type0 exact/shared当前single owner为`BattleDamageWriter.ApplyStandardCharacterDamage`；其他non-type6 target与type6 hit-motion arm需后续独立覆盖，不能把type0完成外推为全B5完成。

## 实施顺序

1. 13-field ITR deterministic data carrier：model/converter/copy/ECS projection/fingerprint。
2. type0 confirmed encoded status ordered RNG producer。
3. type0 unarmored hit-motion arm producer及pending Z映射。
4. 非type0/non-type6 status与all-unarmored target扩展。
5. B8 knockout event feed仍独立。

## 不变量

不在审计包修改脚本/Config/Authority/Scene/资源；不把authority 92条内容直接导入Unity。
