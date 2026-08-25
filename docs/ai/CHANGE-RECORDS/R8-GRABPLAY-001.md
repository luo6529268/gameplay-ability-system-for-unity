# R8-GRABPLAY-001 — live grab / CPoint / link / held injury S4 probe

<!-- CHANGE-RECORD
id: R8-GRABPLAY-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Test/Editor/BattleGrabCpointLinkPlayModeProbeEditor.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\entity\game_tick.cpp:1441-2018;src\entity\cpoint.cpp:23-190;src\entity\weapon.cpp:13-107;Makefile
evidence: R5-CPT-001..005 and R5-LINK-001..002 have source/compile/self-check evidence but no joint production Play S4
-->

> 创建日期：2026-08-23  
> 最后更新：2026-08-23  
> 类型：test / editor / live production relation certification

## 1. 修改前状态

- C++ first-held→CPoint→weapon-sync→positive-link→second-held source合同已闭合；
- Unity scheduler与R5 writer代码已经分别修正并通过旧focused/self-check；
- 缺口是同一live production world中的正向抓取、mismatch throw、escape dircontrol、stats和link residue联合证据；
- 当前未发现新的source-confirmed production差异。

## 2. 允许改动

- 仅新增`Assets/NTSD/Scripts/Test/Editor/BattleGrabCpointLinkPlayModeProbeEditor.cs`及meta；
- 仅更新本包治理/证据文档；
- 禁止修改production gameplay、scheduler、DAT/scene、render/URP、C++ authority。

## 3. 预期副作用与保护

- probe仅在driver paused且worker idle时注册自有logic fixtures；
- global KillStats/DamageStats先保存后恢复；
- 所有probe实体必须注销，pending destroy flush，world/pool/claimed恢复；
- 不依赖具体生产角色、技能、OID或帧文件；fixture数字只编码C++ source合同。

## 4. 验收

- task required matrix全部PASS；
- compile/focused/Play/self-check/validator；
- 若first-difference出现，本Record保持`FAILED`或`BLOCKED`并拆repair，不改production。

## 5. 实际改动

- 新增Editor-only显式Play probe及meta；
- valid kind3 grab使用production `LF2CharacterInteractionResolver/BattleInteractionWriter`建立双向关系；
- 在paused live world依次调用production first-held、PreInteraction、positive-link、second-held并输出pass rows；
- source-derived matrix覆盖lethal injury/global stats/FWC/position、mismatch fallback throw、negative escape+
  dircontrol+postprocess、positive/negative residue；
- global stats、probe实体、world/pool/claimed与pause均有best-effort cleanup；
- production gameplay、scheduler、DAT/scene、render与C++均0改动。

当前状态`VERIFIED`，仅裁决WP01C-03 Unity production Play S4。

首次clean Play所有断言PASS，但报告在FramePostProcess后读取已清零的Knockback字段，并在后续negative-held
场景后读取positive target LinkState，导致JSON没有保留各自观察点的`4/-3`与`-5`。只修Editor probe取样
时点并保留首次PASS事实；production未改，必须重新compile/clean Play后才作为final evidence。

## 6. Final verification

| 层级 | 结果 | 状态 |
|---|---|---|
| compile | source 12:22:01 < Editor DLL 12:22:18；C# error 0 | `PASS` |
| positive link | job `2e1446b473a64aef81ca80fd9b69d30d` 8/8 | `PASS` |
| negative link | job `aa8d155711ac4ee5a9fc48862bf2fe42` 2/2 | `PASS` |
| invalid combined filter | job `f75fa220c274452787c1ac109e02ae33` 0 tests | `DISCARDED` |
| final clean Play | tick16→17，worker active；valid/mismatch/escape/link matrix | `PASS` |
| cleanup | objects4→4、claimed2→2、pools2→2、global stats restored、Console error0 | `PASS` |
| full self-check | 2026-08-23 12:23:59 | `PASS` |
| ledger validator | 68 records / 67 governed code files | `PASS` |
| production changes | 无 | `NOT APPLICABLE` |

final报告：`Temp/NTSD_R8_WP01C_03_GrabCpointLink.result.json`；persistent evidence：
`docs/ai/RESEARCH/R8-WP01C-03-grab-cpoint-link-held-injury-runtime-evidence-20260823.md`。

## 7. 回滚

- 删除新增probe/meta并标记`ROLLED_BACK`；
- 未提交，不触碰其他用户修改。
- 本Record VERIFIED不改变R5各production Change Record的`RUNTIME_PENDING`：C++ full trace仍BLOCKED。
