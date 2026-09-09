# NTSD28-B5-FIRST-BDY-RESPONSE-CARRIER-001

<!-- CHANGE-RECORD
id: NTSD28-B5-FIRST-BDY-RESPONSE-CARRIER-001
status: VERIFIED
change-kind: CONTENT_CARRIER
code-path: Assets/NTSD/Scripts/Animation/LF2FrameData.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5FirstBodyResponseCarrierEditorTests.cs
authority: NTSD 2.8-Logan BattleWorld28::resolve_confirmed_unarmored_hit first-current-Bdy kind/respond; EXE B1E13AE1, closure 39DDDA15.
evidence: RED 7 expected CS1061; focused 4/4; B5 614/614; NTSD28 1079/1079; SelfCheck PASS; compile/Console/Scene/Ledger clean.
-->

> 状态：`VERIFIED / CARRIER_READY / BEHAVIOR_UNCONNECTED`

## Authority与原状

Authority在普通unarmored tail前读取当前帧第一个BDY的kind/respond。Unity仅把first kind保存到用途受限的
`primaryBodyKindForEffectSuppression`，没有respond carrier；formal body geometry有刻意冻结的X/Y/W/H合同。

## 实际修改

- `LF2FrameData`保留既有`primaryBodyKindForEffectSuppression`字段，并增加只读`PrimaryBodyKind`与
  `primaryBodyRespondForHitResponse`/`PrimaryBodyRespond`。
- converter只从source order第一个BDY读取kind/respond；缺失值为0，重复token遵循parser既有的
  last-wins语义；secondary BDY不覆盖。
- `BattleBodyBoxValue`仍精确只有X/Y/W/H四个公开属性，未把kind/respond混入formal geometry。
- 新增`NTSD28B5FirstBodyResponseCarrierEditorTests`，覆盖first-only、missing、duplicate、geometry及
  冻结Unity `criminal.dat`的22个可达first-BDY 1xxx帧。

## 验证证据

- TEST-FIRST RED：生产实现前得到7个预期CS1061，均为缺失`PrimaryBodyKind`/`PrimaryBodyRespond`；
  没有其他生产编译错误。
- focused：job `beb8a0041d9641f3a8ad1ac47cea389f`，4/4 PASS。
- B5 broad：job `304994b7e08e48cfb5d3c9dbf7efed57`，614/614 PASS。
- NTSD28 broad：job `37668ede44314b8f85ed91c6d6b8ec13`，1079/1079 PASS。
- `BattleRuntimeSelfCheck`：2026-09-06 23:53:40，`PASS`；其7条预期negative-path错误清理后，
  Unity Console error=0。
- Unity Editor：idle、`is_compiling=false`、`is_playing=false`、`ready_for_tools=true`。
- Scene：`NTSD_Battle.unity` dirty=false、rootCount=13；SHA-256仍为
  `50FD4D8FAF2CC5C630886CEFD4742A5FD068E1F7AADEE89809AD19B7B85AFF3C`。
- `git diff --check`通过；`Validate-ChangeLedger.ps1`通过（339 records / 293 governed code files）。
- 另运行一次全EditMode 2777项（namespace调整前），存在项目中既有、与本包无关的失败；本包4项均通过，
  因此该次全量结果不作为全项目绿色证据。

## 边界

本包只建立数据载体，尚未接入命中runner、RNG、action/hold/manual-damage或HitPlan。first-BDY响应行为仍
必须由后续pure core与atomic production包完成，不能把本Record解释为行为已对齐。

## 回滚

删除新增carrier/入口、converter赋值和对应测试即可；不触及用户内容。
