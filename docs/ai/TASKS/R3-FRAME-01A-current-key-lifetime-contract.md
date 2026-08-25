# R3-FRAME-01A — current-key lifetime through frame advance and late frame tick

> 建立日期：2026-08-22  
> 状态：`RUNTIME_PENDING`（source/static、Unity scripts compile和full self-check均通过；physical Play Mode
> 与 C++ runtime trace保持未关闭。）  
> 顶层目标：`Assets/NTSD/Docs/cpp-release-vs-unity-battle-realignment-plan.md` 的 R3。  
> 唯一行为 authority：`J:\QQFile\NTSD2.4\ntsd_release` 的 release live runtime。  
> 执行方式：按 D-009 连续推进；脚本改动已由 `R3-FRAME-001A` Change Record 覆盖。

## Goal

只关闭 `D-MOV-001` 的代码级最小差异：让一个逻辑 tick 中已由 human poll 或 AI preparation
写入的 **current key** 保留到 C++ 同 tick 会读取它们的 F03/F09 checkpoint；不再由 Unity
`SerialTickAll` 在每个实体 frame advance 前统一清零。

本包的目标不是改变输入映射、组合键规则、AI policy、跳跃数值或 movement state machine；它只恢复
current-key 的可见性生命周期。

## Scope

允许仅：

1. 在 `SimulationWorld.SerialTickAll` 移除其 per-entity、frame-advance 前的
   `BattleCharacterInputWriter.ClearCurrentKeys(entity.Runtime)`；不把该清除移动到其他 generic
   scheduler pass；
2. 修正与该旧行为矛盾的 Unity 注释；
3. 更新 `BattleRuntimeSelfCheck` 中明确把“frame advance 前清 key”称作 authority 的历史 fixture，
   并新增/调整最小断言，使其验证 C++ 已确认的 current-key lifecycle；
4. 运行 static guard、ledger validator、existing Unity Editor scripts compile、filtered `error CS` 和
   full `BattleRuntimeSelfCheck`。

禁止：

- 修改 `FrameInputSet` 协议、InputAction asset、physical W/S/A/D/J/K/L binding、controller buffer 或
  `NTSDInputStateModule` 的 edge/history 算法；
- 修改 AI sensing/decision/policy、spatial index、worker、SoA/ECS layout、runtime profile default；
- 修改 `D-MOV-002` landing raw-frame writer、`D-MOV-003` respawn integer sync、`D-MOV-004/005`；
- 修改 CPoint、held/link、collision/hit、opoint、render、scene/DAT/pool/capacity、C++ source/build/executable/config；
- 运行或向 C++ authority 目录写入任何内容。

## Authority / Evidence

- **C++ release source — VERIFIED**：
  - `src/entity/game_tick.cpp:1002-1005` 先执行 post-cooldown human callback；
  - `src/input/input_handler.cpp:1555-1613` 的 `InputHandler::poll` 先把 prior current key 滚入
    `prev_*`，再写本 tick held `key_*`、cooldown/history；
  - `src/input/input_handler.cpp:1615-1624` 的 AI `roll_and_clear_keys` 只在 AI preparation 内发生，
    之后由同一个 AI branch 重写本 tick key；
  - `src/entity/game_tick.cpp:1247-1276` 在 input path 后才进行 frame logic / ascending frame advance；
  - `src/entity/frame_advance.cpp:80-83` 的 non-character `dvz` 读取 current up/down；
    `941-951` 的 frame 212 jump-init 读取 current direction；`977-980` 的 MP turn-around 读取
    current left/right。已读 C++ main/frame/physics source 中不存在这两个 checkpoint 前的全局
    current-key clear。
- **Unity source — VERIFIED**：
  - `NTSDBattleTickSystem.RunTick`：human input → CharacterInput → FrameAdvance；
  - `LF2Character.RunHumanInputPollPhase` 保留 local held snapshot，AI 在
    `SimulationWorld.PrepareAiInputBasic*` 内自行 roll/clear/rewire；
  - `SimulationWorld.Passes.partial.cs:599-612` 当前却在每个实体进入 frame advance 前调用
    `ClearCurrentKeys`，同时清 compatibility runtime mirror 与 canonical input-store row；
  - `LF2Entity.RunCommonFrameTick` 的 frame212 / PP turn consumer 与 C++ 的 current-key consumer
    对应。
- **C++ executable trace — BLOCKED**：R1-WP02 仍没有安全、只读、可重复的观察方案。本包只以 C++ release
  source 合同裁决，不运行 C++ executable。

## Planned behavior contract

| checkpoint | C++ contract | Unity required behavior after this package | 最小观测 |
|---|---|---|---|
| human poll | `poll` rolls prev then writes current held state | full-held packet在本 tick 写入 current key；不是由 frame advance 再清掉 | `key/prev/cd/history` |
| AI preparation | `roll_and_clear_keys` 只属于 AI producer，再重写 current key | AI 维持自身 producer 生命周期；不由 generic SerialTick 重复清除 | AI existing contract不变 |
| F03 frame advance | `dvz` / physics 前可读 current key | serial transit/advance 看到本 tick current key | probe current key |
| F09 late frame tick | 212 jump-init、MP turn读 current key | 212 按互斥 direction 覆盖 Vx/Vz；无方向时保留原 Vx/Vz | frame、Vx/Vy/Vz |
| battle entry clear | `NeedClearInput` 是独立 entry boundary | 仍只清 character input 且整个 tick early return；非-character storage不被扩大清除 | existing GT-01 |

## Files likely involved

| 文件 | 符号 | 预计改动 |
|---|---|---|
| `Assets/NTSD/Scripts/Simulation/SimulationWorld.Passes.partial.cs` | `SerialTickAll` | 删除 Unity-only generic current-key clear。 |
| `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs` | `UpdateLocalInputStateFromControllerBuffer` 注释 | 更正已废弃的“frame advance clear key”解释；不改 input algorithm。 |
| `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` | input lifetime / GT-02 / GT-03 fixtures | 将旧 C#-authority expected 改为 C++ source contract，并验证 key/prev/cd/history、transit visibility、212 velocity。 |

## Acceptance

1. **S0 source/static**：C++ input callback → frame advance → F09 current-key consumer 的调用顺序可闭合；
   Unity `SerialTickAll` 不再含 generic `ClearCurrentKeys(entity.Runtime)`。
2. **S1 focused self-check**：
   - full-held left packet只产生首次 edge；后续 held tick 保留 current key、`prev=1`、cooldown自然递减、
     不重复 push history；
   - generic frame-advance probe 在 transit 时看到完整 current key；
   - frame 212 的 right/up current key 产生 `jump_distance/-jump_distancez` 覆盖；无 direction 时继承已有
     horizontal/depth velocity；
   - `NeedClearInput` 原有 battle-entry clear / early-return contract仍通过。
3. **S2 build**：existing Unity Editor scripts compile、filtered `error CS` 为 0，full
   `BattleRuntimeSelfCheck` 实际 PASS。
4. **S3 runtime**：真实 W/S/A/D/J/K/L Play Mode binding保持 `R3-PHY-01 / UNKNOWN`；本包不把 source/self-check
   结论提升为 physical binding或整个战斗场景已对齐。
5. **S4 C++ trace**：仍为 `BLOCKED / R1-WP02`。

## Stop conditions

停止并建立新 Record，若：

- 删除 generic clear 后暴露 input-store/runtime mirror不一致、AI producer顺序或 `FrameInputSet` 协议问题；
- 需要改 human poll、AI policy、physical asset、combo/held/CPoint/collision才能让本包 self-check通过；
- 需要用 landing、respawn integer sync或 throw guard 改动来解释失败；
- C++ source contract无法继续闭合，或需要运行/修改 C++ authority runtime。

## Out of scope

`D-MOV-002` / `R3-LAND-01`、`D-MOV-003` / `R3-SYNC-RESP-01`、`D-MOV-004/005`、R3-PHY-01、R4～R8、
R1-WP02、T8 default `stage.dat`、服务器、Android。

## 实际验证结果（2026-08-22）

- **代码写入**：`SimulationWorld.SerialTickAll` 已移除 frame-advance 前的 generic
  `ClearCurrentKeys(entity.Runtime)`；human/AI producer和battle-entry clear均未改动。
- **自检迁移**：complete-held、GT-02 transit、GT-03 frame212和AUDIT6 local-held fixture已从旧 C# clear
  expectation改为本合同的 C++ current-key lifecycle。`NeedClearInput` 的 character-only clear / early-return
  断言保持。
- **首次运行 first-difference**：02:18:33 的完整 self-check失败于
  `CheckAudit6InputPhaseOrder`，原因是该 test-only fixture仍断言 serial frame advance必须清
  `localHeld.Runtime.KeyLeft`。C++ source contract与本包实现均要求保留，因此只更正该遗留断言；没有
  改动human/AI input producer或任何生产 movement公式。
- **Unity compile**：existing Unity Editor经 UnityMCP `refresh_unity(force/scripts/compile)` 于约 02:21
  完成domain reload并恢复 ready；随后 filtered `error CS` 查询为 0。
- **full self-check**：`NTSD/验证/运行战斗运行时自检` 重跑后，
  `Temp/NTSD_BattleRuntimeSelfCheck.result` 于 **2026-08-22 02:22:20 +08:00** 写入 `PASS`。
- **governance**：`Tools/Validate-ChangeLedger.ps1` PASS，`git diff --check` exit 0（只有既有 LF/CRLF
  warning）。

这些证据只证明 C++ source contract的代码级 Unity adaptation、compile和self-check；不证明 physical
W/S/A/D/J/K/L、真实场景技能链或 C++ executable trace已经通过。
