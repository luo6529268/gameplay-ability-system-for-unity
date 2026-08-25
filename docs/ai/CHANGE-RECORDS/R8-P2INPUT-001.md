# R8-P2INPUT-001 — Player_2 physical action source and P1/P2 no-cross runtime

<!-- CHANGE-RECORD
id: R8-P2INPUT-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Test/Editor/P1P2PhysicalInputSourceEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleP1P2PhysicalInputPlayModeProbeEditor.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\include\input_handler.h and src/core/main.cpp/src/input/input_handler.cpp release live source / USER-APPROVED-R8-WP01G-R06
evidence: INPUT-ASSET-WRITTEN / UNITY-GENERATED-WRAPPER / COMPILE-0 / FOCUSED-2-OF-2-PASS / INPUT-REGRESSION-47-OF-47-PASS / TWO-HUMAN-PLAY-11-OF-11-PASS / SELFCHECK-PASS / CONSOLE-0 / LEDGER-81-OF-96-PASS
-->

> 创建日期：2026-08-23  
> 当前状态：`VERIFIED`  
> 所属：`R8-WP01G-R06`

## Requirement / authority

C++ Release P1/P2默认输入合同为：P1使用W/S/A/D与L/J/K；P2使用方向键与numpad3/1/2，且主循环先
poll P1再poll P2。Unity当前`Player_2` action map只有Move，导致`CharacterInputModule`对Attack/Jump/Defend
的lookup均为null，P2无法通过物理键产生动作或组合键packet。

本Change只补齐P2 physical source，不改变既有crossed adapter：

- numpad1→Unity Attack→canonical Jump→runtime key_jump；
- numpad2→Unity Jump→canonical Defend→runtime key_defend；
- numpad3→Unity Defend→canonical Attack→runtime key_attack；
- arrows→matching direction。

## Planned files / symbols

- `NTSDInputConfig.inputactions`：Player_2新增Attack/Jump/Defend与三个exact numpad binding；
- `NTSDInputConfig.cs`：只允许Unity Input System generator从asset正规重建；不得手工编辑；
- `P1P2PhysicalInputSourceEditorTests.cs`：asset/wrapper/action/binding/canonical edge/no-cross聚焦合同；
- `BattleP1P2PhysicalInputPlayModeProbeEditor.cs`：真实InputSystem device state→action callback→
  `LocalSimulationFrameInputProvider`→`FrameInputSet`→roster slot0/1→runtime的Play证据。

## Protected boundaries

- C++ authority只读；
- P1 W/S/A/D/J/K/L与既有crossed action语义不变；
- local player capacity保持8，不回退为2；
- 不直接写FrameInputSet、SimInputBuffer、runtime key/cooldown/history制造PASS；
- 不修改AI、negative-link child、30Hz、worker、SoA/ECS、pool/0GC、CentralOnly、T8、IL2CPP、Android或服务器；
- 不手工修改Input System auto-generated wrapper内容。

## Expected side effects

- `CharacterInputModule.SetInputID(1)`绑定Player_2后四个action均非null并注册callback；
- numpad1/2/3的held/pressed/released分别映射到canonical Jump/Defend/Attack；
- P1与P2同tick可以独立进入FrameInputSet，playerSlot/runtimeSlot/stable binding不串写；
- P2 release和重复held不重复产生history edge；
- 不改变任何战斗动作规则，只恢复C++已有的第二玩家物理输入能力。

## Acceptance

以R06 Task Contract为准：asset/wrapper static、focused EditMode、真实two-player Play、fresh compile、full
self-check、Console0、0GC边界和Change Ledger validator全部通过；C++ full trace仍受R1-WP02限制，不能扩大为
C++ executable动态认证。

## Rollback

只回退本Change新增的Player_2三个action/binding、由该asset生成的wrapper对应段以及两个test-only文件；
保留P1、Player_2 Move、8-slot roster、所有既有输入/战斗代码和历史证据。

## Actual changes / verification

- `NTSDInputConfig.inputactions`只在Player_2 map新增Attack/Jump/Defend，exact binding分别为
  numpad1/numpad2/numpad3；P1和Player_2 Move未改；
- UnityMCP `refresh_unity(force/all/compile)`完成domain reload并恢复ready；Input System generator自动把
  wrapper从17,964字节更新为21,702字节，新增Player_2三个action字段/property/callback；未手改wrapper；
- 新增`P1P2PhysicalInputSourceEditorTests`：检查P1/P2 exact asset/wrapper binding，并用隔离Keyboard device
  验证J/K/L、numpad1/2/3、D/RightArrow只驱动对应action map；
- 新增`BattleP1P2PhysicalInputPlayModeProbeEditor`：11个物理case只queue keyboard state，观察正式
  action callback→FrameInputSet→roster slot0/1→runtime press/release/no-cross；不写packet/runtime；
- 首次`refresh_unity(all)`导入新文件后编译失败：probe把roster resolver返回的`LF2Entity`直接访问
  `Controller`，产生两处CS1061。已保留失败事实并最小改为验证/cast到`LF2Character`后再读取controller；
- 修正后compile0、Console项目error0；首次focused job `edf6a6428876413d939662249294b9d9`为0/2：
  wrapper Dispose在EditMode使用Destroy产生未预期日志，隔离Keyboard未绑定wrapper device mask导致J未触发。
  已最小改为`DestroyImmediate(input.asset)`并显式把wrapper devices限制为测试Keyboard；production/wrapper不改；
- 第二次focused job `35e3e5f1ad3544a09c9ea245d119ff5d`为1/2：exact binding PASS，但普通项目
  Editor Test没有引用non-auto-referenced `Unity.InputSystem.TestFramework`，直接InputSystem.Update仍与Editor
  全局设备状态竞争，J未触发测试asset。为避免引入asmdef/asmref或反射内部InputSystem，focused职责收窄为
  exact physical binding + crossed canonical adapter；真实device-state完整链保留给Play probe；
- 修正后Unity fresh compile为0 error；第三次focused job
  `9398cf0c85f4439aaa320d8f9496a940`为2/2 PASS：P1/P2 exact physical binding与crossed canonical
  adapter均通过。MCP客户端退出时出现`anyio.BrokenResourceError`，但Unity job已明确返回`succeeded`，属于
  stdio客户端关闭噪声，不是测试失败；
- 首次production Play探针在输入注入前按设计FAIL：当前`NTSD_Battle`测试启动配置只创建一个human roster
  entity，报告为`The production roster does not expose two distinct active human entities in player slots 0 and 1.`；
  未直接写第二个packet/runtime制造PASS。`BattleTestBootstrap`源码确认其production测试路径可按
  `overrideCharacterIds.Length`创建两个human，因此下一步仅在Editor Play clone的`Start`前临时注入双human
  fixture，不保存场景、不改bootstrap/gameplay，并仍由正式对象池、roster与`CharacterInputModule`建链；
- two-human Play最终11/11 PASS，slot0/1、stable100/101绑定保持，所有case均在第一次physical pulse完成
  canonical press/held/release和runtime no-cross；报告为
  `Temp/NTSD_R8_WP01G_R06_P1P2_PhysicalInput.result.json`；
- input regression job `78b2e8ccbe544012bc7b0defc97301ed`为47/47 PASS，包含warmed 256-frame
  strict delayed input 0 managed allocation；full self-check于2026-08-23 19:37:29 PASS；Play结束前
  Console error0；
- Change Ledger validator最终81 records/96 governed code files PASS，scoped `git diff --check` PASS；
- 本`VERIFIED`只裁决Unity physical-source修复和对应S4运行路径；C++ full trace仍BLOCKED，真实人手硬件
  edge仍属于D-INP-006用户验收，不扩大为全战斗或C++ executable完整认证。
