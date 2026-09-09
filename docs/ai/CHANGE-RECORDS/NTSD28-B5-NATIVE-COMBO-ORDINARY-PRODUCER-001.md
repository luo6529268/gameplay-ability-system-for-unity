# NTSD28-B5-NATIVE-COMBO-ORDINARY-PRODUCER-001

<!-- CHANGE-RECORD
id: NTSD28-B5-NATIVE-COMBO-ORDINARY-PRODUCER-001
status: VERIFIED
change-kind: BATTLE_BEHAVIOR
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleNativeComboOrdinaryProducer.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5NativeComboOrdinaryProducerEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD 2.8-Logan simulation_tick_driver.cpp ordinary combo producer and post-applied-hit callsite; EXE B1E13AE1, closure 39DDDA15.
evidence: missing-producer RED; focused 7/7; B5 826 assertions covered with sole MCP log pollution isolated 1/1; NTSD28 1173/1173; fresh SelfCheck PASS; Console 0 and Scene unchanged.
-->

> 状态：`VERIFIED / PRODUCTION_ROUTED / FORMAL_TUPLE_INACTIVE`

## 原状与计划

carrier已闭合但没有生产者。共享runner在普通Damage dispatch后只处理observation/abort；first-body成功在其前
提前返回。先新增直接producer与shared-runner focused测试取得缺类型RED，再实现纯slot-based producer并在
唯一Damage-success边界接线。正式tuple保持默认inactive。

## 风险与边界

- 必须在dispatch后回查target/source slot，不能持有对象引用绕过OID201等同tickdespawn。
- 不能把输入combo或`ComboCountAtk/Vic`当作计数；不能递归owner；不能给非Damage、dispatch false或
  first-body成功生产。
- 本包不实现expiry、B6 caughtact、B10显示或H内容激活。

## 实施结果

- 新增slot-based `BattleNativeComboOrdinaryProducer`：post-dispatch重新解析target/source；执行
  `recordPresent && bound == 1`、target current type0、`facing == 1 ? attacker : target`、non-type0
  source精确一次`OwnerSlotIndex` hop；missing/inactive fail closed。
- shared `BattleHitCandidateSequenceRunner`在唯一普通`Damage` dispatch成功边界调用；first-body成功仍在
  此前提前返回，非Damage与dispatch false不生产。
- 成功生产只修改独立native combo count/lastTick；lastTick为C24提交前的
  `NativeFrameSequence + 1`。正式mode tuple仍由H决定，production默认inactive。

## 验证

RED：新增focused fixture后Unity Console精确得到14条缺符号编译错误，均指向尚不存在的
`BattleNativeComboOrdinaryProducer`或`TryProduceNativeComboAfterDispatch`；Test Runner job
`d5001ee9d32e47c498f4096f5f244f28`因此发现0 tests。该证据取得于任何production实现写入前。

| 层级 | 证据 | 结果 |
|---|---|---|
| focused | job `e4865355c06547d1bf161432b5c19105` | `7/7 PASS`；含4096次warm producer `0 B` |
| B5 broad | job `22f92f5b81a1404aa19f22f3ad5e45d1` | 826项执行；唯一失败为MCP disposed NetworkStream日志污染，项目断言无失败 |
| B5污染项隔离 | job `5fd976ac069b4372b3f0b62ff2443d7c` | `1/1 PASS`，确认等价826项行为断言通过 |
| NTSD28 broad | job `30982183236c418bb438d660633cb0ad` | `1173/1173 PASS` |
| SelfCheck | `Temp/NTSD_BattleRuntimeSelfCheck.result`，2026-09-07 04:52:06 +08 | fresh `PASS` |
| Console | SelfCheck预期日志清空后的error查询 | `0` |
| Scene | active `NTSD_Battle`与磁盘文件 | `isDirty=false`、`rootCount=13`；SHA-256 `50FD4D8FAF2CC5C630886CEFD4742A5FD068E1F7AADEE89809AD19B7B85AFF3C`、mtime未变；未Play/未保存 |
| scoped diff | `git diff --check`目标已跟踪文件 + 新文件人工格式核对 | `PASS`；仅既有LF/CRLF提示 |
| change ledger | `Tools/Validate-ChangeLedger.ps1` | `PASS`；349 records / 302 governed code files |

expiry、B6 caughtact、B10显示和H正式tuple激活不属于本包；默认content路径下producer保持inactive，
因此本结论是生产接线与可注入行为闭合，不表示玩家当前已看到连击显示。
