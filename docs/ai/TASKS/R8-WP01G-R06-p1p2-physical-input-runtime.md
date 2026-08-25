# R8-WP01G-R06 — P1/P2 physical input source and roster joint runtime

> 建立日期：2026-08-23  
> 状态：`COMPLETE AT AVAILABLE EVIDENCE / UNITY INPUTSYSTEM S4 PASS / C++ FULL TRACE BLOCKED`  
> D-ID：`D-INP-004`

## Goal

以C++ Release P1/P2 poll与默认键位为权威，补齐Unity `Player_2`缺失的三个physical action source，并在
production Play Mode证明P1、P2各自的physical held/press/release进入canonical `FrameInputSet`和绑定runtime
slot时不串键，同时保留Unity 8-slot扩展与现有crossed action adapter。

## Scope

### 允许

1. 只读复核C++ `DEFAULT_P1/DEFAULT_P2`、poll order和Unity action/routing；
2. 修改`NTSDInputConfig.inputactions`的`Player_2` action/binding；
3. 让Unity Input System正规重新生成对应wrapper，不手改生成代码内容；
4. 在任何脚本修改前建立独立Change Record；
5. 必要时新增Editor-only two-player Play probe；
6. 使用InputSystem device-state事件，经正式action callback、local provider、FrameInputSet、roster resolver到runtime；
7. 验证P1与P2的方向、三个action、held/press/release、cooldown/history和无串键；
8. fresh compile、focused tests、full self-check、live Play、0GC边界和ledger validator。

### 禁止

- 不修改、运行、构建或写入C++ authority；
- 不直接写FrameInputSet、SimInputBuffer、runtime key/cooldown/history制造PASS；
- 不改变`CharacterInputModule`既有crossed action语义；
- 不把P2补齐扩展成输入系统重构或控制器重映射UI；
- 不把Unity local capacity从8回退到2；
- 不处理AI、negative-link AI child、debug function keys、merge/split、render G4、T8、IL2CPP、Android或服务器。

## Authority / Evidence

- `J:\QQFile\NTSD2.4\ntsd_release\include\input_handler.h:9-17`；
- `src/core/main.cpp:2379-2380,4607-4608`；
- `src/input/input_handler.cpp:1555-1609`；
- Unity `NTSDInputConfig.inputactions`、`CharacterInputModule`、`LocalSimulationFrameInputProvider`、
  `SimulationFrameInputModule`与roster state；
- `R3-INP-004-001`只作为packet后routing历史证据，不能裁决physical source。

## Required behavior

| Physical key | Unity action | Canonical button | C++ runtime key |
|---|---|---|---|
| P2 numpad1 | `Attack` | `Jump` | `key_jump` |
| P2 numpad2 | `Jump` | `Defend` | `key_defend` |
| P2 numpad3 | `Defend` | `Attack` | `key_attack` |
| P2 arrows | `Move` | Up/Down/Left/Right | matching direction key |

P1 W/S/A/D/J/K/L保持现状，不能因P2修复发生回归。

## Files likely involved

- `Assets/NTSD/Config/InputConfig/NTSDInputConfig.inputactions`；
- Unity自动生成的`Assets/NTSD/Config/InputConfig/NTSDInputConfig.cs`；
- 新增或扩展的Editor-only input contract test / Play probe；
- Change Ledger、STATE、all-diff register、evidence report和handoff。

## Verification

1. asset静态结构：P1/P2均有Move/Attack/Jump/Defend，P2 exact numpad bindings正确；
2. action lookup：两个`CharacterInputModule`分别绑定Player_1/Player_2且四action非null；
3. canonical capture：同tick包含player slot0/1，按钮、Pressed、Released独立；
4. routing：slot0/1 runtime/stable binding保持；
5. runtime：P1/P2 key、prev、cooldown、history与C++ source contract一致；
6. crossed mapping：P2 numpad1/2/3分别进入Jump/Defend/Attack canonical语义；
7. no-cross：P1输入不写P2，P2输入不写P1；
8. release和重复held不重复写history；
9. fresh compile0、focused PASS、Play PASS、self-check PASS、Console0、ledger PASS。

## Stop conditions

- InputAction wrapper无法由asset正规生成而需要手改自动生成逻辑；
- 发现AppManager/roster实际无法创建第二human production entity；
- 需要改变crossed mapping、FrameInputSet schema、worker或8-slot容量；
- first difference超出P2 physical source；
- 用户改变P2键位或本地多人需求。

## Out of scope

AI、negative-link AI child、3+ physical mapping、重绑定UI、手柄方案、联机、render、T8、IL2CPP、Android、服务器、
C++ executable/full trace。

## Authorization

用户于2026-08-23明确批准：`批准执行 R8-WP01G-R06，恢复目标`。Change
`R8-P2INPUT-001 / IN_PROGRESS`已建立；脚本/asset写入必须保持本合同范围。

## Completion evidence

- Unity正规生成wrapper；fresh compile0；
- exact binding/crossed adapter focused 2/2 PASS；相关input regression 47/47 PASS；
- 未保存two-human Play clone经正式对象池/roster/action callback→FrameInputSet→runtime完成11/11 physical
  press/held/release/no-cross case；
- full self-check于2026-08-23 19:37:29 PASS；Play结束前Console error0；ledger81/96与scoped
  diff-check PASS；
- 详细证据见`RESEARCH/R8-WP01G-R06-p1p2-physical-input-runtime-evidence-20260823.md`；
- C++ full trace仍受R1-WP02阻塞，本包不宣称C++ executable动态认证。
