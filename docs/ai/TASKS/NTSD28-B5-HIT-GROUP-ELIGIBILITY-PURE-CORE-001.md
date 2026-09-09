# Task Contract — NTSD28-B5-HIT-GROUP-ELIGIBILITY-PURE-CORE-001

> 状态：`VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`
> 依赖：`NTSD28-B5-HIT-CANDIDATE-PAIR-SNAPSHOT-CARRIER-001 / VERIFIED`

## 目标

实现一个allocation-free pure resolver，严格按Authority顺序消费original kind/effect、冻结pair snapshot和world
mode `+0x18`，给出accept/reject及确定分支原因。

## 真值表

1. invalid snapshot拒绝；original kind 4/8/50绕过。
2. target current state 10/13绕过。
3. target OID212在不同OID或同OID action phase 0→5时绕过。
4. attacker group0接受；非零group下state190接受same，其他state接受different。
5. rejected side在mode1/3或state18/180且effect非21/22时接受。
6. target type1/2/4/6接受。
7. attacker type0→target type3且facing不同接受；其余拒绝。

## 边界与验收

- 新resolver和专属tests；不改query、runner、HitPlan、damage、Scene、content、snapshot schema或Authority。
- 覆盖每个接受/拒绝边界、顺序优先级、state190反转、mode/state effect exclusions、type/facing极性和warm
  0 managed allocation。
- focused、B5、完整Unity侧NTSD28、build、SelfCheck、Console、Scene及Ledger通过。

## 回滚

删除未接生产的resolver/test即可；无运行时状态或资产回滚。

## 验证结果

- RED：测试首次导入产生7个缺失resolver/type编译错误；随后仅修正一次测试公开参数不能暴露internal enum的测试签名问题，未改变resolver真值表。
- focused：`NTSD28B5HitGroupEligibilityPureCoreEditorTests` 52/52 PASS（job `9ddc93d0ccfb482ab5b091158c9feb27`），含warm zero-managed-allocation。
- 回归：B5 585/585 PASS（job `a75c3b44cb724581b9c798da16fe2149`）；Unity侧NTSD28 1143/1143 PASS（job `4086db3a9f4643cd856f6471f73a5f98`）。
- build：`dotnet build Assembly-CSharp-Editor.csproj --no-restore -clp:ErrorsOnly` 0 error、22 warning。
- runtime：`Temp/NTSD_BattleRuntimeSelfCheck.result`于2026-09-06 21:34:24写出`PASS`；最终Console error 0。
- Scene：`NTSD_Battle`、dirty=false、rootCount=13；SHA-256仍为`50FD4D8FAF2CC5C630886CEFD4742A5FD068E1F7AADEE89809AD19B7B85AFF3C`。
- audit：相关`git diff --check`通过；Change Ledger 334 records / 290 governed code files PASS。

本包没有接query、runner或HitPlan。下一包：`NTSD28-B5-HIT-GROUP-ELIGIBILITY-ATOMIC-PRODUCTION-INTEGRATION-001`。
