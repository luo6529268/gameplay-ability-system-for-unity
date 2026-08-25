# R4-HIT-002B — kind16 character raw-frame write

<!-- CHANGE-RECORD
id: R4-HIT-002B
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\entity\hit.cpp:664-793
evidence: SOURCE-CONTRACT-VERIFIED / CODE-WRITTEN / UNITY-COMPILE-PASS-20260822-0558+08 / FULL-SELF-CHECK-PASS-20260822-055802+08 / PLAYMODE-PENDING / CXX-RUNTIME-TRACE-BLOCKED
-->

> 创建日期：2026-08-22  
> 最后更新：2026-08-22  
> 类型：battle / test  
> 当前状态：`RUNTIME_PENDING` — 最小脚本、Unity compile与full self-check已通过；C++ trace和真实Play Mode仍未关闭。

## 1. 状态与范围

- 所属 Work Package：`D-HIT-002 / R4-HIT-02B`。
- 目标：把kind16的frame200写入从有隐式副作用的`ImmediateFrame`改为raw writer，同时**保留**后续显式
  `AttackingCounter=0`。
- 不属于本次范围：kind10/11、weapon raw frame、vital/stat算法、sound、vrest、held release、RNG、diagnostic
  projection、CPoint、held/link、opoint、candidate、input、scheduler、AI、render、DAT/资源、C++ authority、Play Mode与C++ trace。
- 关联 Change ID：前置 `R4-HIT-002A`；后续预留 `R4-HIT-02C` / `R4-HIT-02D`。

## 2. Authority / 需求依据

- C++ release live path：`src/entity/hit.cpp:664-793` 的kind16 character branch。
- C++ verified行为：SFX后raw `frame=200`，下一句显式`attacking=0`，随后vrest/held release；不隐式写prev/wait。
- Unity current source：`BattleDamageWriter.ApplyKind16`先`ImmediateFrame(MpDrain)`，再显式清attacking；前者多写PN/wait。
- Evidence：`VERIFIED` source contract；`INFERRED` diagnostic projection ownership；`UNKNOWN` C++ runtime trace/Play Mode。

## 3. 计划改动

| 文件 | 类型 / 方法 | 改前职责 | 目标职责 |
|---|---|---|---|
| `Assets/NTSD/Scripts/Simulation/Ecs/BattleDamageWriter.cs` | `ApplyKind16` | implicit frame/PN/attacking/wait helper + explicit attacking clear | raw frame200 + explicit C++ attacking clear |
| `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` | kind16 snapshot fixture | 验证frame/attacking和现有stats | 加入PN/wait/Frame.Data mirror raw-write合同 |

## 4. 不可回退边界

- 不修改CentralOnly / Texture2DArray / dynamic Mesh / URP ownership；
- 不修改Authority400、MobileExtended、DesktopExtended容量合同；
- 不修改30Hz、FrameInputSet、slot/generation、SoA/ECS、对象池、worker、0 GC或global frame helper；
- 不修改R4-HIT-002A已关闭的kind10/11 callsites。

## 5. 实际改动

| 文件 | 类型 / 方法 | 实际改动 | 预期副作用 |
|---|---|---|---|
| `Assets/NTSD/Scripts/Simulation/Ecs/BattleDamageWriter.cs` | `ApplyKind16` | `ImmediateFrame(MpDrain)`替换为既有raw writer；保留下一句显式attacking clear | frame=200不再隐式改PN/wait；attacking仍由C++对应显式写入清零。 |
| `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` | kind16 snapshot fixture | 预置frame10/PN71/wait17；snapshot新增runtime frame、data id、PN、wait | 捕获隐式helper重置和frame-data mirror回归，同时保留existing vital/stat/vrest/link断言。 |

## 6. 验收与证据

| 层级 | 命令 / 场景 / 输入 | 实际结果 | 状态 |
|---|---|---|---|
| C++ source | 只读复核 `hit.cpp:664-793` | raw frame/explicit attacking顺序已确认 | `PASS` |
| focused self-check | exact/shared kind16 PN/wait/frame/attacking contract | actual/shared snapshot均通过：frame/Data=200、PN=71、wait=17、attacking=0及既有vital/stat/vrest/link/held contract成立 | `PASS` |
| Unity compile | existing Editor/UnityMCP scripts refresh | Unity 2022.3.62f3 / port6401 refresh/domain reload后 filtered `error CS`=0 | `PASS` |
| full self-check | `NTSD/验证/运行战斗运行时自检` | `Temp/NTSD_BattleRuntimeSelfCheck.result=PASS`，2026-08-22 05:58:02 +08:00 | `PASS` |
| Play Mode | kind16 character scenario | 后续独立验收 | `PENDING` |
| C++ runtime trace | R1-WP02 | 不可用，保持blocked | `BLOCKED` |

## 7. 风险、回滚与未关闭项

- 风险：如果误删除后续显式attacking clear，就会将C++行为修成错误；本次保留该句，full self-check已证明其仍生效。若改diagnostic projection或global helper会扩大scope。
- 回滚：若本包失败，只回滚本Record列出的两份脚本及关联文档，不触碰其他用户工作。
- 未关闭项：C++ trace、Play Mode、kind16 frame-advance/presentation joint、02C/02D。

## 8. Git / 交接

- 修改前工作树基线：dirty；没有回退、移动或清理预存用户/历史修改。
- 计划脚本范围：仅metadata列出的两项。
- 提交 hash：未提交。
- `Tools/Validate-ChangeLedger.ps1`：文档最终更新后需重跑；脚本写入前的planning校验已通过。
- 交接优先阅读：本Record、`TASKS/R4-HIT-02B-kind16-character-raw-frame-contract.md`、
  `RESEARCH/R4-HIT-02B-kind16-character-raw-frame-preflight-20260822.md`。
