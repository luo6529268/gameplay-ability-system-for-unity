# HANDOFF — R4-HIT-02B kind16 character raw-frame writer

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING`  
> Change ID：`R4-HIT-002B`  
> 唯一 authority：`J:\QQFile\NTSD2.4\ntsd_release` release live source；未运行、构建、修改、复制或写入C++ authority。

## 完成内容

- C++ `hit.cpp:664-793` 已只读确认kind16顺序：SFX → raw `frame=200` → explicit `attacking=0` → vrest → held release；
- Unity canonical writer `BattleDamageWriter.ApplyKind16`只将frame helper改为
  `DirectWriteRawFramePreserveWaitCounter(MpDrain)`，并保留紧随其后的`AttackingCounter=0`；
- existing actual/shared kind16 fixture现在验证frame/runtime/Data=200、PN=71、wait=17、attacking=0，且原
  lethal HP/stat/vrest/link/held random result保持；
- UnityMCP compile filtered `error CS`=0；full self-check于2026-08-22 05:58:02 +08:00写入`PASS`。

## 关键不变量

- 不要删除、移动或以helper隐式替代kind16的`AttackingCounter=0`；
- 不要修改`BattleEcsHitExecutionPlan` diagnostic projection、global frame helper、kind10/11、weapon raw-frame、
  vital/stat、sound、vrest、held release、RNG或其他战斗模块；
- C++ trace和真实Play Mode仍未取得，`RUNTIME_PENDING`不是完整kind16/R4对齐结论。

## Console 说明

full self-check后仅剩两条`Runtime rest bind failed` / mismatched-rest-binding error-level log；它们是
`RegistrationRollbackSelfCheckEntity`的现有negative control，与C# error和`PASS`结果无冲突。

## 连续下一步

按D-009自动进入`R4-HIT-02C`：只读确认 normal kind0 weapon victim（type1、type4/type6、type2）的raw frame
writer及其随机数/显式字段顺序，建立独立Task Contract/Change Record后才改`ApplyKind0WeaponVictimTail`。
