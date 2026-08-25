# R6-PRES-01 — active / Z / slot / per-entity command order 预检

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING`（no-code source certification）  
> Authority：C++ Release `renderer.cpp::Renderer::render_world`

## 1. 结论

C++ Release 与 Unity CentralOnly 在本包范围内没有发现需要修改 production renderer 的差异：

1. 两边都从当前 active runtime slot 的升序输入建立表现集合；
2. 两边都按有符号 `ZInt/z_int` 升序，且同Z保持runtime slot升序；
3. Unity stable-slot radix只改变索引顺序，不移动冻结的宽snapshot；fallback comparer为Z→slot→stableId，其中stableId只在理论重复slot输入时生效，正式slot table不允许重复claim；
4. 每个entity的command顺序仍为shadow→body→overlay→hit-record；
5. dynamic Mesh只合并相邻且resource-compatible command，segment按原command流提交，没有按texture跨实体重排。

本包只认证排序与command顺序，不裁决资源缺失、GPU透明primitive最终像素、shadow OID identity、visibility extra gate或spark lifecycle。

## 2. C++ source contract

- `Makefile:11-35`：`renderer.cpp`参与正式release `SRCS`；
- `renderer.cpp:1300-1308`：slot0..399收集active entity；
- `renderer.cpp:1309-1318`：insertion sort只在previous.z_int > key.z_int时移动，同Z保留原slot顺序；
- `renderer.cpp:1319-1437`：逐entity顺序为perspective field→shadow→body→overlay→hit records；
- `renderer.cpp:1438`之后才进入HUD。

## 3. Unity mapping

- `SimulationWorld.StageRender.partial.cs:385-396`：按runtime slot升序收集active entity；
- `BattlePresentationShadowBuild.cs:2215-2285`：输入必须slot/stable升序，使用有符号Z key的稳定4-pass radix；
- `BattlePresentationShadowBuild.cs:2288-2300`：fallback为Z→slot→stableId；
- `BattlePresentationShadowBuild.cs:800-821,1039-1045`：indexed presentation order通过storage index解析，base order按logical rank重算；
- `BattlePresentationShadowBuild.cs:2451-2813`：每entity的baseOrder+0/+1/+2/+3分别承载shadow/body/overlay/hit-record；
- central dynamic mesh按command index建segment并按segment升序提交，该架构边界继续受A-RENDER-001保护。

## 4. Fresh evidence

- `BattlePresentationBeginFrameReuseEditorTests` 已包含signed Z、same-Z slot tie、indexed order、stale generation与CentralOnly sort reuse；它作为R5-LIFE-01B job `582b9e9212264d39b4377b72d7e0374d`的一部分通过；
- `BattlePresentationCommandWriterEditorTests` job `5561fce764bc4baa8804ae37ca929417`：6/6 PASS，含optimized writer/reference、deferred materialization、composite command与warmed zero-allocation；
- full self-check：2026-08-22 17:49:18 `PASS`；
- 本包无C# diff，没有虚构新的Assembly-CSharp时间戳。

## 5. 状态边界

- source + Unity focused tests支持该子流程到`RUNTIME_PENDING`；
- 未取得C++ runtime trace、真实Play Mode或GPU像素证据，不能写成完整render对齐；
- D-RENDER-001/002/004/005仍各自独立；D-RENDER-003 logic half已由R5-LIFE-01B关闭到同一层级。

## 6. Reopen conditions

- presentation input不再按slot升序；
- radix改为非稳定排序或改用DisplayZ；
- command/segment按texture跨越原painter顺序重排；
- duplicate runtime slot进入production snapshot；
- Play Mode/trace显示same-Z前后遮挡与C++相反。
