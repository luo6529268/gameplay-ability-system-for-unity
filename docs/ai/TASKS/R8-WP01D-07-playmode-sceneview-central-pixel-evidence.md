# R8-WP01D-07 — Play Mode SceneView central pixel evidence

> 日期：2026-08-23  
> 状态：`VERIFIED / EDITOR-ONLY SCENEVIEW S4`  
> Change ID：`R8-SPRITEMAP-007`

## Goal

在不依赖logic-only池对象`Transform`、不恢复Legacy `SpriteRenderer`且不修改production渲染的前提下，
用真实`CameraType.SceneView`验证Play Mode SceneView能否取得并绘制与world camera相同的当前中央submission，
从而关闭`D-RENDER-006`的Scene View可观察性证据缺口，或输出首个通用断点。

## Scope

- 新增一个Editor-only显式Play probe；
- 等待battle ready、worker idle与current immutable central plan；
- 获取`SceneView.lastActiveSceneView.camera`并记录其真实`CameraType.SceneView`；
- 暂时把SceneView camera投影对齐正式world camera，使用独立RenderTexture、透明clear及空culling mask，
  让结果只包含`BattleRenderFeature`中央提交像素；
- 记录SceneView camera gate、submission lease tick/generation、segment/draw和非透明像素；
- 将isolated SceneView central pixels与结构化JSON写入项目`Temp/`；
- 在`finally`恢复SceneView camera、driver pause与world计数；production、scene asset、URP asset均0改动。

## Authority / Evidence

- C++ release `src/render/renderer.cpp:581-624`及Makefile确认live render handoff消费frame/source rect并按
  逻辑位置提交；C++不定义Unity SceneView API；
- 用户要求运行时Scene模式可观察所有实体；该项是已批准Unity表现适配的验收要求；
- Unity `BattleCentralRenderSystem.CanRenderCamera`明确允许Play Mode Base `CameraType.SceneView`；
- `BattleCentralLatestFrameMaterializationEditorTests.CamerasCannotAcquireOldSubmissionUntilLatestPublicationIsMaterialized`
  已证明SceneView只有在最新world-camera publication materialized后才能取得current lease；
- 006 final已证明Game链为3 snapshots→6 resolved commands→1 draw，当前不能再用tick1空采样推断
  worker/central/URP故障。

## Files likely involved

- `Assets/NTSD/Scripts/Test/Editor/BattleCentralSceneViewPixelPlayModeProbeEditor.cs`（新增）
- 对应`.meta`
- 本Task、Change Record、Ledger、STATE、register、orchestration与handoff

## Deliverables

1. SceneView camera identity/gate/lease证据；
2. isolated SceneView central render PNG与非透明像素计数；
3. world/plan/tick/submission/cleanup结构化JSON；
4. 若像素为0，输出最早失败阶段，不在本包直接改production。

## Verification

1. fresh Unity compile 0 error；
2. existing SceneView camera gate/materialization focused tests PASS；
3. explicit Play probe PASS且isolated SceneView central nontransparent pixels > 0；
4. Game submission仍current，driver/world/SceneView camera状态恢复；
5. full `BattleRuntimeSelfCheck`与ledger validator PASS。

## Stop conditions

- 需要修改production renderer、URP asset、scene、camera component或DAT/BMP；
- SceneView camera实际不能进入URP feature，或render需要长期改变Editor视图状态；
- 需要角色/技能/OID/frame/file特判；
- 需要改变approved adapters或C++ authority。

命中stop condition时只记录generic first-difference并新建独立production repair Change Record；不得在007顺手修复。

## Out of scope

- gameplay、输入、碰撞、命中、对象生命周期；
- Legacy production owner或双画；
- authored state8000缺样本、C++ full trace、T8、Android、1000 AI、Player/IL2CPP、服务器。

## Result

- fresh compile：source 12:03:53 < `Assembly-CSharp-Editor.dll` 12:04:04，C# compiler error 0；
- focused job `9dfeda6b0663429a9caf20df64048fb9`：
  `BattleCentralLatestFrameMaterializationEditorTests` 13/13 PASS，其中SceneView latest-publication gate用例PASS；
- clean Play final：`Temp/NTSD_R8_WP01D_07_SceneViewPixels.result.json`为PASS；
- 真实camera：`SceneCamera / CameraType.SceneView`，production gate=true，current lease=true；
- plan/lease：simulation/display/lease tick均2，generation=3，pending/materialized tick均2，非stale；
- submission：4 source、4 resolved、1 segment；960×540白底isolated render产生575个non-clear pixels，
  hash=`C292967D753744C2`；证据图为
  `Temp/R8-WP01D-07/R8-WP01D-07-sceneview-central-isolated.png`；
- cleanup：world object 4→4、claimed slots 2→2，camera设置和driver pause恢复；clean Play Console 0 error；
- 12:05:47 full `BattleRuntimeSelfCheck=PASS`；
- ledger validator：67 records / 66 governed code files PASS；
- production、scene、URP asset、DAT/BMP、Legacy owner与C++ authority均0改动。

结论：SceneView中央像素链没有production first-difference。先前空Scene截图来自180×936窄viewport和
logic-only池对象Transform不承载中央表现逻辑位置，不能据此修改renderer。
