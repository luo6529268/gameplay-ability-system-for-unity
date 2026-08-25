# HANDOFF — R4-HIT-02A kind10/11 character raw-frame writer

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING`  
> Change ID：`R4-HIT-002A`  
> 唯一 authority：`J:\QQFile\NTSD2.4\ntsd_release` release live source；本包未运行、构建、修改、复制或写入C++ authority。

## 完成内容

- C++ `collision.cpp:1193-1237` 已只读确认：character kind10/11在其case内使用 raw `frame=182`，不隐式写
  prev / attacking / wait；
- Unity exact `LF2CharacterHitResolver` 与 shared `LF2CharacterDatHitResolver` 的两个
  `ApplyFluteCharacterForce` 都从`ImmediateFrame(182)`改为现有
  `DirectWriteRawFramePreserveWaitCounter(182)`；
- existing self-check矩阵现覆盖exact/shared × kind10/11，验证frame182、Unity frame-data mirror、保留的
  PN / attacking / wait和既有stat contract；
- existing Unity 2022.3.62f3 / UnityMCP port6401 scripts refresh后，filtered `error CS`=0；
  完整自检在2026-08-22 05:43:54 +08:00写入`Temp/NTSD_BattleRuntimeSelfCheck.result=PASS`。

## 关键不变量

- 不要把`ImmediateFrame`、`SetFrameDirect`或`DirectWriteRawFramePreserveWaitCounter`全局替换/重构；
- `02A`只处理kind10/11 character；kind16的显式`attacking=0`、weapon victim/attacker的raw frame和CPoint
  仍是独立writer合同；
- 没有改candidate、RNG、ITR、scheduler、input、AI、render、CPoint、held/link、opoint、DAT、场景、资源或
  C++ authority。

## 验证说明

- post-self-check console有四条error-level条目：两条MCP domain-reload disposed-connection提示，以及两条
  `RegistrationRollbackSelfCheckEntity` / mismatched-rest-binding negative control。它们不是C# compile error，
  也不是本fixture failure；应结合`error CS=0`与result file `PASS`解释；
- `RUNTIME_PENDING`不等于完整对齐：没有C++ runtime trace，也没有真实flute技能Play Mode。

## 连续下一步

按照 D-009，自动进入 `R4-HIT-02B`：只读复核kind16的 raw `frame=200` 与其**显式**
`attacking=0` 的顺序，先建立独立Task Contract / Change Record，再修改`BattleDamageWriter.ApplyKind16`和其
focused self-check。不得把02A的证据套用到02B。
