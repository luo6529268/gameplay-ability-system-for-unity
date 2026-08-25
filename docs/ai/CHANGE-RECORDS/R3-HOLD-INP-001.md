# R3-HOLD-INP-001 — negative-link character input eligibility

<!-- CHANGE-RECORD
id: R3-HOLD-INP-001
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleEcsCharacterInputPass.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\core\main.cpp + src\input\input_handler.cpp release live path
evidence: SOURCE-CONTRACT-VERIFIED / STATIC-PASS / UNITY-COMPILE-PASS / FOCUSED-SELF-CHECK-PASS / RUNTIME-PENDING
-->

> 创建日期：2026-08-22  
> 状态：`RUNTIME_PENDING`  
> 所属 Work Package：`R3-HOLD-INP-01`

## 1. Scope

仅处理 `D-INP-001`：去除 Unity active current-character-DAT 输入入口的 `Runtime.LinkState < 0`
总体跳过，使 C++ caller 已定义的 `apply_input` 调用资格进入既有的局部 relation / state resolver。

允许文件仅为：

- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`；
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs`；
- `Assets/NTSD/Scripts/Simulation/Ecs/BattleEcsCharacterInputPass.cs`；
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`。

不修改 frame advance、held、CPoint、link cleanup、collision/hit、opoint、AI target / dead-respawn、
packet / physical input、F1/F2、renderer、DAT、scene、C++ 或任何保护边界。

## 2. Authority / source contract

- C++ `src/core/main.cpp:5505-5522`：`game_tick > 1` 时按升序对 every active current `obj_type == 0`
  character DAT 调用 AI prepare（如 AI）与 `InputHandler::apply_input`；没有 negative-link caller filter；
- C++ `src/input/input_handler.cpp:2742-3096`：`apply_input` 的 only entry guards 是 `char_data` / current
  frame；它依次执行 combo、direct hit、state-specific action、velocity tail；没有 `link_state < 0` overall
  return；
- C++ local link-state behavior is state-specific (`link_state == 2` / held-action branches), therefore it cannot
  be represented by skipping the whole function;
- Unity static current guards are in `LF2Entity.cs:2692-2717`, `LF2Character.cs:819-866`, and
  `BattleEcsCharacterInputPass.cs:81-137`; frame-advance negative-link gate stays out of scope.

`R1-WP02` full C++ trace is still `BLOCKED`. This Record has source authority, not C++ runtime equivalence.

## 3. Initial implementation / acceptance contract

After the patch, every active current-character-DAT entry retains its existing runtime/null/type eligibility,
but no longer treats `LinkState < 0` as an entire input-pass return. Existing local held/heavy/caught state
logic remains the sole relation gate below the entry point.

Focused fixture will create an active current-character-DAT with a structurally valid negative relation and a
direct input edge. It must prove that the existing resolver and resulting-frame velocity tail are reached. The
fixture must not claim every caught/held animation is C++ runtime verified.

## 4. Protected boundaries / stop conditions

- Keep CentralOnly / Texture2DArray / dynamic Mesh / URP, 1.5× scale, fixed-world camera, capacity, 30 Hz,
  FrameInputSet, SoA/ECS, pool, worker, zero-GC, T8 deferment;
- stop if the minimal fixture needs CPoint/held/link lifecycle, frame advance, input asset or C++ changes;
- stop if a state-local relation rule cannot be sourced from C++ without expanding this Record;
- no destructive Git operation; a correction requires a new Change Record.

## 5. Actual changes (2026-08-22)

- `LF2Entity` 和 `LF2Character` 的 two character-input entry / known-character entry 都保留
  `Runtime == null` 与 current DAT type gate，但移除了 `Runtime.LinkState < 0` 的 whole-pass return；
- `BattleEcsCharacterInputPass` 的 exact-character AI path 同样只在 runtime 为 null 时 return；
- 未改 `IsBlockedByReleaseLinkOrCaughtCpoint()`，negative-link frame advance、held/CPoint/link lifecycle
  writer 与 local action resolver 的 relation checks 都保持原样；
- `BattleRuntimeSelfCheck.CheckNegativeLinkCharacterInputEligibility` 建立 valid parent/child negative
  relation，分别覆盖真实 `LF2Character` world path 与 `SelfCheckCharacterDatShell` compatibility path：
  `hit_a` direct input must reach frame 10 and apply the target frame `dvx=7` tail.

## 6. Actual verification (2026-08-22)

- local static guard contract PASSED：四个 character-input entry 中没有残留 `LinkState < 0` overall
  guard；`LF2Entity.IsBlockedByReleaseLinkOrCaughtCpoint()` 的 frame-advance negative-link gate 仍存在；
- `Tools/Validate-ChangeLedger.ps1` PASSED：6 records、7 governed code files 均有 Record 覆盖；
- `git diff --check` PASSED：没有 whitespace error；只输出工作树既有 LF/CRLF warning；
- 使用现有 Unity Editor `gameplay-ability-system-for-unity@b1b02287` 的 UnityMCP
  `refresh_unity(mode=force, scope=scripts, compile=request)` 完成 scripts domain reload；
- UnityMCP `read_console(types=error, filter_text="error CS")` 返回 0 条；
- 通过 Editor 菜单 `NTSD/验证/运行战斗运行时自检` 运行完整 `BattleRuntimeSelfCheck`；
  `Temp/NTSD_BattleRuntimeSelfCheck.result` 于 2026-08-22 00:43:55 写入 `PASS`，因此新增 focused
  fixture 已实际执行并通过。

此证据只证明 source contract、编译与 focused self-check。没有运行 C++ executable、C++ trace、
negative-link caught/held Play Mode 联合场景或 physical input；data-oriented AI entry 的改变由 source/static
coverage约束，未伪造为独立 AI runtime equivalence。因此本 Record 必须保持 `RUNTIME_PENDING`。
