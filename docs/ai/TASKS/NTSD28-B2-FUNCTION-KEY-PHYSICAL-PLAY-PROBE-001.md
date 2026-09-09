# Task Contract — NTSD28-B2-FUNCTION-KEY-PHYSICAL-PLAY-PROBE-001

> 状态：`VERIFIED / REAL-PLAY-PHYSICAL-F3-F12-PASS / SCENE-UNCHANGED / PRODUCTION-UNCHANGED`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2 / FUNCTION KEYS`  
> 建立日期：2026-09-04

## 目标

在真实`NTSD_Battle` Play Mode与实际`Keyboard.current` InputSystem state event边界验证production physical F3/F4/F6～F12：
paused期间capture、release/rearm、tick dispatch、Bootstrap调试键隔离、F8/F9 fixed order、F11/F12 held priority及typed
handoff。Probe完成后退出Play，不保存Scene，不执行B8/B10 downstream effects。

## Authority 与前置

- Authority：formal main/GameSession function-key live closure与crosswalk manifest。
- `NTSD28-B2-FUNCTION-KEY-PRODUCTION-INTEGRATION-001`已compile/focused/worker通过，唯一B2门为真实Play physical probe。
- F1/F2/F5已有B1专项；本probe不重复改变其Host状态，只覆盖新接入F3/F4/F6～F12。

## 允许修改

- 新建`Assets/NTSD/Scripts/Test/Editor/NTSD28NativeFunctionKeyPhysicalPlayModeProbeEditor.cs`及Unity `.meta`。
- 写入忽略目录`Temp/NTSD28FunctionKeyPhysicalPlay.request`与`.result.json`。
- 本Task/Record、Ledger、STATE、handoff、总表。

不得修改production、Config/Input Actions、Scene/Prefab、DAT、Authority；不得真的执行F4 Scene close或F11/F12 audio；
不得保存Play状态或遗留Keyboard synthetic state。

## Probe合同

- 只在Play、driver lifecycle Running、world/carrier/Keyboard ready后开始；先`SetPaused(true)`。
- 每个chord通过`InputSystem.QueueStateEvent(Keyboard.current, KeyboardState)`按下，至少跨两个player frames，再发空state释放。
- 顺序验证：F6 standalone→F7→F8+F9→F3+F6→plain F10→F11+F12→F4→Ctrl+F10。
- 每个Session场景在paused手动step前验证无提前消费，step后验证accepted/count/pending与旧Flow request为0。
- F11+F12只检查typed command为VolumeUp，释放后为None；F4只检查typed leave handoff；Ctrl+F10只检查maintenance。
- finally始终发送空KeyboardState、恢复driver pause状态、移除Editor update callback并写result。

## 验收

- Unity compile0；probe source focused/static test若有则pass。
- 真实Play result为PASS，记录每个场景布尔证据、tick范围、driver lifecycle与Keyboard device。
- Play退出后Scene dirty状态不新增，Console0 error，full SelfCheck PASS，validator通过。
- 若InputSystem不允许对硬件Keyboard排队synthetic state，必须记录真实阻塞，不得用pure latch测试冒充Play physical。

## 回滚

删除probe脚本与`.meta`及Temp产物即可；production无修改。

## 完成证据

- Unity probe编译0 error；`NTSD_Battle`真实Play lifecycle=`Running`、Keyboard device=1。
- result `Temp/NTSD28FunctionKeyPhysicalPlay.result.json`：`PASS`，startTick0→endTick5；F6、F7、F8+F9、
  F3+F6、plain F10、F11+F12 held/release、F4、Ctrl+F10九组布尔全部true。
- result SHA-256：`881C44CC9F8EFB2FC5F24B91CF86CA3CC2AD457D1F4DE0C954DD9C5D553355E1`。
- 已自动退出Play；Scene SHA-256前后均为
  `20984749C3309774A02E8A174EFC2B12ED0268D2A42C7411B647AFBAA028018B`，未新增Scene写入。
- Play后Console0；full SelfCheck 2026-09-04 20:50:48 PASS，7条既有negative日志清空后Console0；
  validator `PASSED / Records 157 / governed code files 115`。
