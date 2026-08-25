# R8-WP01G-R06 — P1/P2 physical input runtime evidence

> 日期：2026-08-23  
> D-ID：`D-INP-004`  
> 结论：`UNITY INPUTSYSTEM S4 PASS / C++ FULL TRACE BLOCKED`

## 1. Authority and first difference

C++ Release只读source合同为P1使用W/S/A/D与L/J/K，P2使用方向键与numpad3/1/2；P1、P2按固定
顺序poll。Unity原`Player_2` action map只有Move，导致Attack/Jump/Defend lookup为null，packet后routing
即使正确也无法由P2物理键产生动作。

Authority入口：

- `J:\QQFile\NTSD2.4\ntsd_release\include\input_handler.h:9-17`；
- `src/core/main.cpp:2379-2380,4607-4608`；
- `src/input/input_handler.cpp:1555-1609`。

## 2. Unity implementation

- `NTSDInputConfig.inputactions`只给`Player_2`新增Attack/Jump/Defend；
- exact binding为numpad1→Attack、numpad2→Jump、numpad3→Defend；
- Unity Input System generator正规重建`NTSDInputConfig.cs`，没有手工编辑生成内容；
- 保留现有crossed adapter，因此canonical结果为numpad1→Jump、numpad2→Defend、numpad3→Attack；
- P1、Player_2 Move、8-slot extension、FrameInputSet schema和runtime writer均未改。

## 3. Focused and compile evidence

- fresh Unity refresh/compile：0个项目C# error；
- focused job `9398cf0c85f4439aaa320d8f9496a940`：2/2 PASS，覆盖exact asset binding和crossed
  canonical adapter；
- input regression job `78b2e8ccbe544012bc7b0defc97301ed`：47/47 PASS，覆盖
  `LocalFrameInputProviderEditorTests`、`CharacterInputLiveSlotLoopEditorTests`与
  `StrictDelayedInputBufferEditorTests`，其中warmed 256-frame strict delayed input保持0 managed allocation；
- MCP stdio客户端在部分job结果返回后出现`anyio.BrokenResourceError`，但Unity job状态均为
  `succeeded`；该关闭噪声不计作项目测试失败。

## 4. Production Play evidence

当前场景默认只创建一个human。第一次探针在注入任何输入前按合同FAIL并写明缺少第二human；没有直接写
FrameInputSet或runtime制造PASS。随后Editor-only runner只在未保存的Play clone进入`Start`前临时把
`BattleTestBootstrap.overrideCharacterIds`配置为两个相同正式角色ID，由正式对象池、初始化、roster和
`CharacterInputModule.SetInputID`创建两名human；未修改bootstrap脚本或场景资产。

最终报告：`Temp/NTSD_R8_WP01G_R06_P1P2_PhysicalInput.result.json`

- success=true；tick 0→34；11/11 case；
- P1 runtime slot0/stable100；P2 runtime slot1/stable101；结束时绑定保持；
- 所有case第一次physical pulse即成功；
- 每项均观察到canonical Pressed、Held、Released和对应runtime key；
- 每项另一玩家同canonical button与runtime key保持0，no-cross=true。

| Player | Physical | Canonical | Press→release tick |
|---|---|---|---|
| P1 | D | Right | 2→3 |
| P1 | J | Jump | 5→6 |
| P1 | K | Defend | 8→9 |
| P1 | L | Attack | 11→12 |
| P2 | Up/Down/Left/Right | matching direction | 14→24 |
| P2 | numpad1 | Jump | 26→27 |
| P2 | numpad2 | Defend | 29→30 |
| P2 | numpad3 | Attack | 32→33 |

Play结束前Unity Console error为0。

## 5. Full regression and scope limit

- full `BattleRuntimeSelfCheck`：2026-08-23 19:37:29 PASS；自检按既有负路径合同会主动记录两条
  registration/release拒绝日志，另有MCP stdio disposed-object噪声；这些不改变结果文件PASS。清理这些已知
  test/tool日志后的最终Console error为0；
- Change Ledger validator：81 records / 96 governed code files PASS；scoped `git diff --check` PASS；
- 本包不改AI、组合键规则、crossed mapping、worker、ECS、render、T8、IL2CPP、Android或服务器；
- 本证据关闭Unity physical source first difference并达到Unity InputSystem S4；
- R1-WP02 C++ executable/full trace仍BLOCKED，因此不得写成C++ runtime完整认证；
- 真实人手键盘、Game窗口焦点和OS硬件edge仍属于`D-INP-006`的用户验收边界，不由自动InputSystem
  device-state证据冒充。
