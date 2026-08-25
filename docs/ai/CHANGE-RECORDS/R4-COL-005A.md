# R4-COL-005A — kind1 non-character target consume

<!-- CHANGE-RECORD
id: R4-COL-005A
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleInteractionWriter.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\entity\collision_collect.cpp:264-335; J:\QQFile\NTSD2.4\ntsd_release\src\entity\collision.cpp:250-263,921-993
evidence: SOURCE-CONTRACT-VERIFIED / CODE-WRITTEN / UNITY-COMPILE-PASS-20260822-0505+08 / FULL-SELF-CHECK-PASS-20260822-050517+08 / PLAYMODE-PENDING / CXX-RUNTIME-TRACE-BLOCKED
-->

> 创建日期：2026-08-22  
> 当前状态：`RUNTIME_PENDING` — 最小代码与focused fixture已通过Unity compile和full self-check；C++ runtime trace、真实 Play Mode与D-COL-005B仍未关闭。  
> 所属 Work Package：`R4-COL-05A`。  
> 唯一 authority：`J:\QQFile\NTSD2.4\ntsd_release` release live source。

## 1. 目标与范围

仅收窄 `BattleInteractionWriter.TryApplyGrab` 的 target Character gate：kind3保持限定，kind1恢复C++通用
Entity consume。加入 character-attacker / non-character-target的frozen-candidate正例和kind3负例。

## 2. Authority / 差异依据

- **C++ VERIFIED**：kind3/8 type gate明确，kind1没有；kind1 `case 1`写通用 Entity字段；
- **Unity VERIFIED**：collector正确，但writer错误拦截kind1 non-character target；
- **C++ trace BLOCKED**：不运行、构建、修改或写入 authority。

## 3. 允许 / 禁止路径

允许：`BattleInteractionWriter.TryApplyGrab` 与对应 self-check。  
禁止：`BruteForceSceneQuery`、kind1 selector/RNG、weapon/special attacker dispatch、pickup、CPoint、held/link、
opoint、scheduler、DAT/资源、render、D-COL-005B+。

## 4. 实际代码写入

- `BattleInteractionWriter.TryApplyGrab` 仍先拒绝非 kind1/3；type gate现在仅在 `kind == 3` 时拒绝
  non-Character victim；
- 没有改动 `BruteForceSceneQuery`、candidate selector/RNG、weapon/special dispatch或pickup；
- `BattleRuntimeSelfCheck`添加 `CheckKind1NonCharacterTargetConsumeContracts`：走 frozen candidate collect和
  `PostInteractionTickAll`，对kind1 non-character正例和kind3负例分别断言 candidate与case1 field writer结果；
- 首次 compile的 CS0165来自测试 short-circuit局部变量，已用显式 `default` 初值最小修复，未改变运行时逻辑。

## 5. 实际验证

| 检查 | 实际结果 |
|---|---|
| C++ authority | 只读复核 `Makefile:11-35`、`collision_collect.cpp:264-335`、`collision.cpp:250-263,921-993`；未运行、构建、修改或写入 authority。 |
| focused fixture | kind1 character attacker → LightWeapon-type target冻结1个candidate并写 action/relation/duration/fall；kind3同类 target冻结0个candidate且不写字段。 |
| 首次 Unity compile | `BattleRuntimeSelfCheck.cs(14735,50) CS0165`，原因是 `&&` 短路下的 `out candidates` 可能未赋值；已修复。 |
| 最终 Unity compile | 2026-08-22 05:05 +08:00，现有Unity 2022.3.62f3 / UnityMCP port 6401 refresh后，Console `error CS`=0。 |
| Full self-check | `Temp/NTSD_BattleRuntimeSelfCheck.result=PASS`，最后写入2026-08-22 05:05:17 +08:00。 |
| Play Mode / C++ trace | 未执行；前者待真实场景，后者受 `R1-WP02=BLOCKED`限制。 |

## 6. 回滚与未关闭项

## 5. 回滚与未关闭项

回滚范围仅为本记录两份脚本和关联文档；未提交。C++ runtime trace、真实 Play Mode和D-COL-005B始终保持未关闭。
