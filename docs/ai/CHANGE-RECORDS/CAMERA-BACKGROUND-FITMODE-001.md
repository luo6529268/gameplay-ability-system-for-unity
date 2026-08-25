# CAMERA-BACKGROUND-FITMODE-001 — background camera fit mode

<!-- CHANGE-RECORD
id: CAMERA-BACKGROUND-FITMODE-001
status: SUPERSEDED
code-path: Assets/NTSD/Scripts/App/BattleCameraSafeArea.cs
authority: USER-DIRECTED-20260824 / UNITY-NATIVE-PRESENTATION
evidence: SUPERSEDED-BY-CAMERA-BACKGROUND-NOCROP-FIT-001
-->

> 创建日期：2026-08-24  
> 当前状态：`SUPERSEDED`  
> 类型：presentation / camera / background framing

## Goal

在已收缩的`BattleCameraSafeArea`中新增背景适配模式：保留现有“完整显示”行为，并新增默认的
“覆盖视野”行为，以消除宽背景在较窄相机 aspect 下产生的上下镂空。

## Authority / 需求

- 用户明确要求增加“全面覆盖背景地图”的模式，原因是当前上下存在镂空；
- 这是 Unity-native 表现需求，不改变 C++ battle authority、战斗 tick 或实体逻辑真相。

## Scope

仅修改`Assets/NTSD/Scripts/App/BattleCameraSafeArea.cs`：

| Mode | 正交尺寸公式 | 预期结果 |
|---|---|---|
| `ContainBackground` | `max(extents.y, extents.x / aspect)` | 完整显示背景；比例不一致时可出现镂空。 |
| `CoverViewport` | `min(extents.y, extents.x / aspect)` | 背景填满整个相机视野；不拉伸，但会裁切比例较长方向的边缘。 |

- 新字段默认`CoverViewport`，使当前未序列化该新字段的场景直接消除上下镂空；
- 绝不修改 Camera Transform、rect、URP、SpriteRenderer scale、背景资源、安全区、viewport布局、follow、bounds、camera offset或调试逻辑；
- 保留 Inspector 可选项，让用户可切回`ContainBackground`。

## 已知权衡

背景比例为`2.134`、相机为`16:9`时，不拉伸且无镂空不可能同时完整显示两侧：

- `ContainBackground`：高度多出约`2.822` world units，产生上下区域；
- `CoverViewport`：会在左右各裁切约`2.509` world units，完全填满画面。

## Files likely involved

- `Assets/NTSD/Scripts/App/BattleCameraSafeArea.cs`
- `docs/ai/CHANGE-LEDGER.md`
- `docs/ai/STATE.md`
- 本 Change Record、Task 与 Handoff。

## Acceptance

1. Unity 编译成功，无本脚本 C# error；
2. 当前场景可选择两种 mode；默认`CoverViewport`；
3. 当前`Bg (2)`与16:9相机下，`CoverViewport`计算得到`orthographicSize=7.0570316`，不出现上下镂空；
4. `ContainBackground`仍保持原`8.468438`完整显示公式；
5. 两种 mode 均不改写相机 Transform；
6. 短 Play、filtered Console、Ledger validator 与 scoped diff check 通过。

## Rollback

只回退本脚本中的 mode enum/field/formula及本 Change 文档；不触及场景、资源或战斗代码。需要用户明确授权才可执行回退。

## 实际改动

- 已新增私有枚举`BackgroundFitMode`，其两个 Inspector 选项为`完整显示（可能留空）`与`覆盖视野（裁切边缘）`；
- 已新增序列化字段`backgroundFitMode`，默认`CoverViewport`；因为当前场景没有这一新字段，Unity会采用该默认值；
- `RefreshBackgroundContainSize`现在先计算 vertical/horizontal 两个候选尺寸，再按 mode 选择`Mathf.Max`（Contain）或`Mathf.Min`（Cover）；
- 未新增任何 Transform、viewport、safe-area、follow、debug、camera offset或背景 scale writer。

## 验证状态

| 层级 | 实际结果 | 状态 |
|---|---|---|
| Unity compile | 待最终代码重新编译 | `PENDING` |
| Scene mode 字段 | 待Unity导入后确认 Inspector/组件可解析 | `PENDING` |
| 数值合同 | 待确认Contain=`8.468438`、Cover=`7.057032` | `PENDING` |
| 短 Play / Console | 待执行 | `PENDING` |
| Ledger / diff | 待最终代码后执行 | `PENDING` |

## 最终验证

| 层级 | 实际结果 | 状态 |
|---|---|---|
| Unity compile | `Assembly-CSharp.dll`于`2026-08-24 15:05:20`重新生成；当前`Editor.log`未发现`error CS`或本脚本编译错误 | `PASS` |
| Inspector / mode | 当前`NTSD_Battle`组件正常解析，`backgroundFitMode=1`（`CoverViewport`）；两个 enum 分支均由可序列化下拉字段承载 | `PASS` |
| Cover 数值 | 背景 bounds=`30.1100025 × 14.1140633`，aspect=`1.77777779`；`min(7.0570316, 15.0550013 / aspect)=7.0570316`，等于实际`orthographicSize=7.05703163` | `PASS` |
| Contain 分支 | 代码分支保留`max(7.0570316, 15.0550013 / aspect)=8.468438`；未修改用户的dirty scene去切换并保存该选项 | `STATIC PASS` |
| Transform 边界 | `ScenesCamera`仍为`(-1.77,-6.69,-10)`；本Change不含Transform writer | `PASS` |
| 短 Play | UnityMCP进入/退出Play成功，`BattleTestBootstrap` complete，`BattleCameraSafeArea` filtered Console error=`0` | `PASS` |
| Ledger / diff | `Tools/Validate-ChangeLedger.ps1`与scoped `git diff --check`待文档收口后重跑 | `PENDING` |

## Superseded correction

用户在实际画面中确认`CoverViewport`会裁切原始背景两侧内容，明确表示该取舍不可接受。
本 Record 的编译、数值与短 Play 证据只证明该中间裁切实现能够运行，不代表用户需求已满足。
该枚举分支将被移除，并由`CAMERA-BACKGROUND-NOCROP-FIT-001`替代为“完整显示 / 全面覆盖但保留全部内容”的无裁切方案。

## Out of scope

改变背景贴图比例、相机跟随、相机位置、UI布局、战斗逻辑、C++ authority、完整 scene YAML 清理和性能重构。
