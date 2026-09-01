# BATTLE-SPRITE-GRID-SEPARATOR-001 — Runtime BMP grid separator removal

<!-- CHANGE-RECORD
id: BATTLE-SPRITE-GRID-SEPARATOR-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Animation/Runtime/RuntimeSpriteProcessor.cs
code-path: Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleSpriteGridSeparatorEditorTests.cs
authority: USER-REQUEST-2026-08-31-RUNTIME-BMP-GREEN-SEPARATOR; production Naruto/Sasuke BMP pixel topology; central atlas source-identity contract
evidence: COMPILE_0 / FOCUSED_AND_ATLAS_29_29_PASS / LIVE_GREEN_SCAN_0 / PRESENTATION_ONLY / PRESERVE_GREEN_ART
-->

> 创建日期：2026-08-31  
> 当前状态：`VERIFIED / COMPILE_0 / FOCUSED_TEST_PASS / LIVE_VISUAL_PASS / PRESENTATION_ONLY`

## 1. 需求与已观察事实

- 用户在真实 Play Mode 放大图中确认角色脚下仍有一条横向纯绿色像素线；血条位置已无问题。
- `naruto_0.bmp` 与 `sasuke_0.bmp` 均为 `800x560`，DAT 单元为 `79x79`、横10列/纵7行；源图纯绿色横 gutter 位于 top-left y=`79/159/239/319/399/479/559`，纵 gutter 位于 x=`79/159/.../799`。
- `BuildIndexedSpriteRects` 的首格 Unity Rect 为 `(0,481,79,79)`，没有把 gutter 纳入声明 Rect；但 runtime central texture/atlas 上传整张 `ProcessSheetPixelsFast` 后的 sheet，而该处理只清黑色、保留 gutter 为不透明绿色，UV 边界采样可暴露相邻 gutter。

## 2. 计划与约束

1. 在 runtime sheet 像素处理阶段，只识别贯穿整张 BMP 的高覆盖率绿色横/纵分隔带，并将整条 alpha 清零。
2. 不做全局 green-key，避免删除角色图内合法绿色。
3. Sprite Rect、pivot、逻辑坐标、动画帧、战斗 tick 与 atlas 布局保持不变。
4. 同一个 BMP 可能被多个 DAT fileInfo 引用；处理结果必须只取决于 BMP 像素本身，保证中央图集按 source identity 去重时 byte-identical。
5. 聚焦测试使用真实 Naruto/Sasuke BMP 验证 gutter 清零、内容绿色不被泛化删除、Rect 仍为 `(0,481,79,79)`。
6. 图集发布、资源解析、重叠帧归属回归通过后，再以延迟生成角色的 Game 视图做绿色像素 witness。

## 3. 风险与回滚

- 风险：阈值过低会误清合法绿色艺术内容；当前只把一整行/列中至少 65% 为 chroma green（`g>200/r<40/b<40`）视为 sheet separator，并用合成测试证明单个/局部绿色仍保留。
- 回滚：移除 separator alpha clear 调用与 helper/test；不触碰血条、Scene 或战斗逻辑。

## 4. 实际实现

- `RuntimeSpriteProcessor.ClearDetectedGridSeparatorAlpha(...)` 分别扫描整张 sheet 的每行/每列；达到 65% chroma-green 覆盖率时只清该完整行/列的 alpha。
- `CharacterAnimtorManager.ProcessAndCreateSpritesAsync(...)` 在既有黑色透明处理后、生成纹理/中央图集 source 之前执行该处理。
- 初版曾按每个 DAT 的 `(width+1)x(height+1)` 声明清 gutter；真实预热发现同一 `kidomaru_3.bmp` 被不同声明引用时会生成不同像素，触发 `Conflicting decoded atlas source`。该方案未保留，已改成只依赖 BMP 像素拓扑的确定性处理，同 source 不再因调用方声明产生差异。
- 不修改 Sprite Rect、pivot、sheet source key、战斗实体或 HP UI；不是全局绿色抠图。

## 5. 验证

- `dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo /m:1 /v:minimal`：0 error、56 warnings；warnings 为工作区既有/并行代码与 Unity 依赖版本提示。
- Unity job `e08c3da8cf6149db95af721274bf8ab8`：29/29 PASS，覆盖真实 Naruto/Sasuke BMP、局部合法绿色保留、common atlas publication、catalog resolver/cache 与重叠帧归属。
- 两张生产 BMP 在处理前目标 separator 行均有超过 600 个不透明绿色像素；处理后目标行和列 alpha 数为 0，首帧内容不透明像素数保持不变，首 Rect 仍为 `(0,481,79,79)`。
- 延迟生成角色后的 1920x1080 GameView 证据 `Temp/BATTLE-SPRITE-GRID-SEPARATOR-001/fixed-game.png` 中角色正常渲染。按 `g>200/r<40/b<40` 扫描整张画面，没有长度达到 8 像素的连续绿色横线；上、下两名角色邻域的匹配绿色像素数均为 0。
- 角色、血条和场景在真实 Play Mode 正常出现，证明修正后的 source-only 处理未再阻断中央图集预热。Scene 未保存；临时 `Assets/Screenshots` 及 meta 已通过 Unity AssetDatabase 删除。
