# R8-SPRITEMAP-007 — Play Mode SceneView central pixel evidence

<!-- CHANGE-RECORD
id: R8-SPRITEMAP-007
status: VERIFIED
code-path: Assets/NTSD/Scripts/Test/Editor/BattleCentralSceneViewPixelPlayModeProbeEditor.cs
authority: user-approved Unity Scene View observability; J:\QQFile\NTSD2.4\ntsd_release\src\render\renderer.cpp:581-624; Unity BattleCentralRenderSystem SceneView gate
evidence: 006 Game submission passes, while the current 180x936 Scene screenshot cannot observe logic-only central coordinates
-->

> 创建日期：2026-08-23  
> 最后更新：2026-08-23  
> 类型：test / editor / SceneView central pixel evidence

## 1. 修改前状态

- 006 final已证明Game current plan和draw正常；
- production `CanRenderCamera`与focused test允许Play Mode SceneView取得latest materialized submission；
- pool GameObject Transform不是中央表现逻辑位置，`Frame Selected`不能作为SceneView像素裁决；
- 当前Scene窗口180×936截图没有实体，但证据不足，不能登记production renderer差异。

## 2. 允许改动

- 仅新增`BattleCentralSceneViewPixelPlayModeProbeEditor.cs`及meta；
- 仅新增/更新本任务治理文档；
- production、scene、URP asset、DAT/BMP、C++ authority与Legacy owner禁止修改。

## 3. 计划验收

- 使用真实SceneView camera和正式world camera投影；
- cullingMask=0、transparent clear、独立RT，隔离中央feature像素；
- 记录gate/lease/current plan/pixel count/cleanup；
- compile、focused SceneView tests、Play、full self-check、validator后如实推进状态。

## 4. 实际改动

- 新增Editor-only显式Play probe与meta；
- 等待current Central plan、暂停driver并等待worker idle；
- 对真实SceneView camera记录production gate与current submission lease；
- 暂存并恢复camera全部受影响状态，把投影临时对齐world camera，在960×按world aspect的白色隔离RT上
  使用cullingMask=0执行实际SceneView camera render；白底使黑色阴影也能进入non-clear统计；
- 统计nontransparent/nonblack-visible pixels、FNV-1a hash并写PNG/JSON；
- 没有修改production、scene、URP asset、DAT/BMP、Legacy owner或C++ authority。

## 5. 当前验证

| 层级 | 结果 | 状态 |
|---|---|---|
| compile | source 12:03:53 < Editor DLL 12:04:04；C# compiler error 0 | `PASS` |
| focused | job `9dfeda6b0663429a9caf20df64048fb9`，latest-frame/SceneView gate 13/13 | `PASS` |
| clean Play | SceneView gate/lease true；tick2/generation3 current plan；4 source/resolved、1 segment | `PASS` |
| isolated pixels | 960×540；575 non-clear pixels；hash `C292967D753744C2` | `PASS` |
| cleanup | objects 4→4、claimed 2→2；camera/driver恢复；Play Console 0 error | `PASS` |
| full self-check | 2026-08-23 12:05:47 | `PASS` |
| ledger validator | 67 records / 66 governed code files | `PASS` |
| production changes | 无 | `NOT APPLICABLE` |

final报告：`Temp/NTSD_R8_WP01D_07_SceneViewPixels.result.json`；final PNG：
`Temp/R8-WP01D-07/R8-WP01D-07-sceneview-central-isolated.png`。

首次黑底probe也为PASS（522 nonblack pixels），但黑色阴影在黑底不可观察；因此同一Record将隔离底色改为
白色并clean Play复跑，final 575 non-clear pixels作为正式证据。没有修改production。

## 6. 回滚

- 只删除新增Editor probe/meta并将Record标记`ROLLED_BACK`；
- 未提交；不触碰其他用户改动。
- 007只关闭SceneView camera/central pixel证据范围；authored state8000与C++ full trace仍独立。
