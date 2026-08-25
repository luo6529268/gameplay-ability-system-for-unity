# R3-FRAME-02A — remove Unity-only ThrowFrameGuard gates

> 建立日期：2026-08-22  
> 状态：`RUNTIME_PENDING`（shared-DAT fixture 两处编译修复后，existing Unity Editor compile和full self-check已重新通过；仍缺 C++ runtime trace 与 Play Mode。）  
> 顶层目标：`Assets/NTSD/Docs/cpp-release-vs-unity-battle-realignment-plan.md` 的 R3。  
> 唯一行为 authority：`J:\QQFile\NTSD2.4\ntsd_release` release live source。  
> 对应差异：`D-MOV-004`。  
> 前置调查：`RESEARCH/R3-FRAME-02-reachability-preflight-20260822.md`。

## Goal

移除 Unity 中三处 `ThrowFrameGuard == currentFrame` 的 F03/F07 early return。当前 release C++ source没有同名
field的 conditional reader，也没有 nonnegative writer；该 Unity-only gate会在 test / 非标准状态把 frame advance或
late frame tick错误跳过。

## Scope

允许仅修改：

1. `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`
   - `TryEnterReleaseFrameAdvanceAfterDelay` 的 F03 guard；
   - `RunCommonFrameTick` 的 fallback/shared F07 guard。
2. `Assets/NTSD/Scripts/Simulation/Ecs/BattleEcsCharacterFrameTickPass.cs`
   - exact character F07 guard。
3. `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
   - 一个 exact/shared F03/F07 focused fixture。

禁止：

- 修改 `ThrowFrameGuard` field/reset/held-release cleanup writer；
- 修改 frame-delay、link、cpoint、physics、counter、state2000、weapon/held/CPoint/opoint、scheduler、renderer、scene/DAT、pool或 C++；
- 同包处理 `D-MOV-005`、R4+、physical input或 C++ trace。

## Authority / Evidence

### VERIFIED — C++ release source

- release Makefile包含 `frame_advance.cpp`、`physics.cpp`、`game_tick.cpp`；
- `frame_advance.cpp:25-48` / `824-906` 的 F03/F07 paths无 `throw_frame_guard` condition；
- complete source inventory确认 field没有 conditional read、没有 nonnegative writer，只有 default/reset/held cleanup的 `-1` writes。

### VERIFIED — Unity source

- Unity有三处上述 reader：F03 common helper、exact F07、fallback/shared F07；
- normal production source没有 nonnegative writer，当前两处 nonnegative assignment只在 self-check；
- F03 removal必须同时覆盖 exact与shared，因为两者复用同一 helper；F07 removal必须同时覆盖 exact pass与fallback common path。

### INFERRED / not included

`D-MOV-005` 的 state2000 exact gap在当前 type0 character DAT inventory不具可达性；它不构成本包脚本修改依据。

## Deliverables

1. 只删除三处 reader；
2. 新 focused self-check：用 test-only nonnegative matching guard值，证明 exact/shared F03仍推进 physics，并证明
   exact/shared F07仍执行 counter；
3. 更新 Change Record、ledger、STATE、diff register、主计划和 handoff；
4. Unity Editor refresh/compile、full self-check、ledger validator、`git diff --check`的真实结果。

## Verification

| 层级 | 要求 |
|---|---|
| S0 source/static | 三个 Unity reader移除；C++ reader/nonnegative writer仍为0；cleanup `-1` writers仍在。 |
| S1 exact F03 | matching nonnegative test-only value不得阻止 exact character physics/tail integer update。 |
| S2 shared F03 | matching value不得阻止 shared character-DAT physics/tail integer update。 |
| S3 exact F07 | matching value不得阻止 exact late counter运行。 |
| S4 shared F07 | matching value不得阻止 shared late counter运行。 |
| S5 regression | existing Unity Editor scripts compile、full self-check、ledger validator、diff check通过。 |
| S6 boundary | 最高 `RUNTIME_PENDING`；C++ runtime trace / real held-throw Play Mode仍不关闭。 |

## Stop conditions

- 发现 C++ release live source存在遗漏的 conditional read或 nonnegative writer；
- fixture只能通过修改 held/link/CPoint/weapon release/scheduler/physics才能构建；
- reader删除导致 unrelated established contract失败且需要扩大范围；
- 需要变更 capacity、CentralOnly、30Hz、FrameInputSet、SoA/pool或 C++ authority。

## Out of scope

`D-MOV-001/002/003/005`、R3-PHY-01、R4～R8、R1-WP02、T8 default `stage.dat`、服务器、Android。

## 实际验证结果（2026-08-22）

- **最小脚本写入**：删除三个 Unity-only reader：`LF2Entity` F03、`BattleEcsCharacterFrameTickPass`
  exact F07、`LF2Entity.RunCommonFrameTick` fallback/shared F07。`ThrowFrameGuard` field和所有 `-1`
  reset / held-release cleanup writer保持不变。
- **focused fixture**：新增 `CheckThrowFrameGuardDoesNotGateReleasePasses`。该 fixture只用 test-only
  matching nonnegative value覆盖 exact/shared character-DAT：F03 后两者 `XInt` 从10到13，F07 后两者
  `AttackingCounter` 为1。旧 Unity reader会把上述四个操作都直接 return。
- **static**：production `ThrowFrameGuard >= / ==` reader为0，production nonnegative writer为0，exact
  frame-tick code不再引用该 field。
- **Unity compile / self-check**：现有 Unity Editor（MCP port 6401）在 03:14:53 +08:00 scripts refresh/compile
  后 idle/ready；菜单 `NTSD/验证/运行战斗运行时自检` 实际执行，
  `Temp/NTSD_BattleRuntimeSelfCheck.result=PASS`，最后写入 2026-08-22 03:15:33 +08:00。
- **治理检查**：`Tools/Validate-ChangeLedger.ps1` PASS（14 records / 12 governed code files）；
  `git diff --check` exit 0，只有既有 LF/CRLF warning。
- **边界**：未运行或写入 C++ runtime；未做 real held-throw / frame flow Play Mode。
  `D-MOV-005`仅记录为当前 assets下 exact route不可达，不由本包宣称所有 future DAT都已验收。因此本包为
  `RUNTIME_PENDING`。

## 后续编译修复（2026-08-22，进行中）

在 `R4-COL-001` 触发的 existing Unity Editor scripts refresh 中，Console 发现本 fixture 的
`SelfCheckCharacterDatShell.CurrentFrameId` 两处访问不存在（CS1061，24399 / 24425）。仅将这两个
shared-DAT test-only read 改为既有的 `Frame.N` 后重跑 compile/self-check；此修复不改变本合同的
production reader removal，也不触及其他模块。

### 修复验证

- UnityMCP existing Editor refresh/compile于 03:40:14 +08:00 ready，`error CS` 为0；
- 完整 `BattleRuntimeSelfCheck` 于 03:44:57 +08:00 写入 `PASS`；
- 因此恢复 `RUNTIME_PENDING`，而不是扩展为 C++ runtime / Play Mode 已对齐。
