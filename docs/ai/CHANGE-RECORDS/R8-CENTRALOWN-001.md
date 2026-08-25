# R8-CENTRALOWN-001 — CentralOnly 四态 URP ownership 联合 Play 证据

<!-- CHANGE-RECORD
id: R8-CENTRALOWN-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Test/Editor/BattleCentralFailClosedOwnershipPlayModeProbeEditor.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\render\renderer.cpp:1300-1438 / USER-APPROVED-R8-WP01G-R07C
evidence: CPP-RELEASE-SOURCE-CROSSWALK / UNITY-CURRENT-STALE-REPLACEMENT-S4-PASS / COLD-SELFCHECK-PASS / PLAY-CONSOLE0 / R8-CENTRALSEAL-001-VERIFIED
-->

> 创建日期：2026-08-23  
> 最后更新：2026-08-23  
> 类型：test / editor / render

## 1. 状态与范围

- 当前状态：`VERIFIED / TEST-ONLY`
- 所属 Work Package：`R8-WP01G-R07C`
- 只覆盖：`D-RENDER-001`的CentralOnly cold/current/last-good stale/replacement四态URP ownership；
- 不属于本次范围：R07A、R07B、R08、gameplay、P1/P2、AI、T8、IL2CPP、Android、服务器；
- 关联 Change ID：`R6-PRES-004`、`R8-SPRITEMAP-006`、`R8-SPRITEMAP-007`。

## 2. Authority / 需求依据

- C++ release `src/render/renderer.cpp:1300-1438`定义active战斗对象进入render success path；
- Unity必须保留已经批准的CentralOnly/Texture2DArray/dynamic Mesh/URP适配，不恢复Legacy production owner；
- 用户于2026-08-23明确批准执行`R8-WP01G-R07C`并恢复总目标；
- Evidence等级：C++ success-path源码合同`VERIFIED`；Unity四态真实URP Play尚待执行；C++ full trace仍`BLOCKED`。

## 3. Unity 原状与已确认差异

- `BattleRuntimeSelfCheck.CheckCentralPixelOwnershipContracts`已有cold→ready→last-good→replacement精确结构矩阵；
- WP01D-06/07已有真实Game/SceneView current central pixels；
- `BattleCentralRenderSystem`已有Editor-only ready/stale publication、submission lease、feature registration与diagnostic接口；
- 当前没有已确认的production renderer差异；缺口是同一真实Play中四态owner、tick、generation、lease、legacy suppression和isolated pixel的联合证据。

## 4. 计划改动

| 文件 | 类型 / 方法 | 改前职责 | 目标职责 |
|---|---|---|---|
| `Assets/NTSD/Scripts/Test/Editor/BattleCentralFailClosedOwnershipPlayModeProbeEditor.cs` | 新Editor-only Play probe | 不存在 | 在不改URP asset/scene/material/production registration的前提下记录安全可形成的current→stale→replacement真实Game-camera URP像素与ownership；cold不可安全形成时只引用既有exact self-check并如实标记 |
| 同名`.meta` | Unity asset identity | 不存在 | 稳定导入新probe |

## 5. 不可回退边界

- 保持CentralOnly、Texture2DArray、动态Mesh、URP；不得恢复Legacy owner或允许双画；
- 保持1.5×visual scale、fixed-world camera和现有容量合同；
- 保持30Hz、FrameInputSet、slot/generation、SoA/ECS、对象池、worker和0GC；
- C++ authority只读；不改URP renderer asset、scene、shader、material asset或production registration。

## 6. 实际改动

| 文件 | 类型 / 方法 | 实际改动 | 预期副作用 |
|---|---|---|---|
| `Assets/NTSD/Scripts/Test/Editor/BattleCentralFailClosedOwnershipPlayModeProbeEditor.cs` | 新Editor-only联合probe | 等待真实current central plan后暂停driver；采集current isolated Game-camera pixels；通过现有Editor-only stale boundary保留last-good；持有旧submission lease后以render-only dispatch发布replacement；验证generation/retire/lease/legacy suppression/checksum/feature/material/draw mode/cleanup；支持菜单与专用Temp request；另提供test-only prepare-play入口，在进入Play的同一菜单调用中先ResetRuntime，避免禁用Domain Reload时EditMode中央submission阻塞Bootstrap容量预热 | 只在用户主动菜单或明确request时临时改变central plan，不修改实体或生产配置；输出Temp JSON和3张PNG |
| 同名`.meta` | Unity asset identity | 固定新probe GUID | 仅触发Unity导入/编译 |

脚本写入完成，当前状态推进为`CODE_WRITTEN`；编译与运行证据尚未取得。

## 7. 验收与证据

| 层级 | 命令 / 场景 / 输入 | 实际结果 | 状态 |
|---|---|---|---|
| 编译 | Unity full asset refresh | 新probe真实导入；0 error | `PASS` |
| focused test / self-check | central begin-frame + worker；full self-check | job `ba8d...` 29/29；22:45:37 self-check PASS | `PASS` |
| Play Mode / 集成 | `NTSD_Battle` CentralOnly current→stale→replacement | 2026-08-23 23:12:30：三态259 pixels/hash `AE3AFF1E932B491E`一致，tick214→215/214→215、gen216→217、lease/checksum/cleanup PASS；Console0 | `PASS` |
| C++ authority 对照 | read-only source crosswalk | success path已闭合 | `PASS` |
| 可选 full trace | R1-WP02 | blocker仍存在 | `BLOCKED` |

## 8. 风险、回滚与未关闭项

- cold Play若不能在不Reset真实全局central state、不中断live feature自动注册的条件下安全形成，只保留exact self-check，不能破坏场景注册制造PASS；
- 首轮Play前的EditMode focused/self-check留下active submission，禁用Domain Reload后在Bootstrap容量预热处抛出`Cannot resize a central submission while it is published or leased`；新增prepare-play只清理该测试态并立即EnterPlay，不作为cold证据，也不修改production初始化；
- 首次真正full asset refresh发现本probe错误引用不存在的`NTSD.Simulation.Input` namespace；`FrameInputSet`实际位于`NTSD.Simulation`。已仅删除错误using并保留本失败记录，fresh compile待重跑；
- 第二次full asset refresh继续发现`NTSDRenderSpace`缺少所属`NTSD.Animation` using；已仅补充该正确namespace并保留失败记录；
- 首次成功进入场景的Play在tick195超时：world已有4 objects/2 claimed slots且feature/material已注册，但当前Game视图未主动消费worker的PublishedFrame，因此current plan未物化；probe基线尚未捕获，报告中的cleanup false不代表实体泄漏。已只在probe内于正式PublishedFrame/feature就绪后调用公开`PrepareFrame(world)`，等价于中央宿主消费边界，再由真实URP camera取像素；
- 第二轮场景Play到tick201仍无current；复用Game Visibility诊断确认driver当前由外层手动推进且`PublishedFrame`仍为空，故仅调用PrepareFrame没有输入。probe现只在PublishedFrame为空时执行一次当前tick的公开`world.RenderDispatchAll(currentTick)`，建立当前逻辑状态的正式表现快照，再PrepareFrame；不推进gameplay、不写实体字段；
- 第三轮Play到tick202仍超时，说明RenderDispatch后PrepareFrame被readiness gate拒绝；旧probe没有保存plan/diagnostic reason。已只把该分支改为首次拒绝即结构化报告plan reason、runtime refusal、frame tick、feature/material readiness，不再盲等；
- 第四轮仍以timeout结束且未进入PrepareFrame拒绝报告，证明它停在bootstrap资源/catalog前置；原probe仅给1200 Editor updates，而现有R07B生产联合probe使用12000以覆盖首次BMP/catalog加载。已只把本probe等待上限对齐为12000，不改变任何runtime gate；
- 第五轮再次在Bootstrap容量预热前遇到active submission，证明菜单Reset→异步EnterPlay之间仍有Editor URP重发布窗口。test-only prepare-play现武装一次性`playModeStateChanged`边界，在`ExitingEditMode`与`EnteredPlayMode`各ResetRuntime一次，并于EnteredPlayMode立即解除武装；不修改production Bootstrap；
- 第六轮无容量异常但仍在feature/object刚就绪的update触发timeout：计数从tick0和BMP/catalog加载前开始，timeout检查先于readiness分支。已把计数/超时移到`tick>0 + ObjectCount>0 + CentralOnly + feature registered`之后，资源加载不再消耗current-plan等待预算；
- 第七轮在scene-ready后仍超时，结合没有PrepareFrame拒绝报告，唯一剩余门为探针先SetPaused(true)并返回、而初始化链可再次SetPaused(false)，导致反复等待。四态操作在同一Editor主线程回调内同步完成且worker in-flight已有独立门，故只删除跨update paused前置；开始/结束pause状态仍不变；
- 第八轮首次进入联合捕获：current owner/tick/gen/lease/legacy suppression正确，cleanup恢复，但`source/resolved/segment=0`且isolated pixel=0；实体已注册而sprite/catalog命令尚未就绪。probe现只增加`SourceCommandCount>0 + ResolvedCommandCount>0 + SegmentCount>0`表现就绪门后再开始四态，不修改resource或renderer；
- 第九轮首次全链PASS：current/stale/replacement各259 isolated pixels且hash `AE3AFF1E932B491E`，stale保留display tick200/gen201，replacement tick201/gen202，checksum `308B7F8C9F2E3E00`不变，旧submission retire/reject/release与cleanup均true。唯一报告时点问题是replacement lease/draw在Camera.Render前采集为false/0；已改为渲染后重采并新增lease/segment/draw>0断言，待最终重跑；
- last-good像素读取必须持有合法submission lease，不能读取retired backend；
- probe必须在`finally`恢复driver pause、feature/material/draw mode及central状态；若无法可靠恢复立即停止；
- 若发现production first-difference，停止认证并拆独立repair Task/Change，不在probe内修production；
- final production first difference `B-R8-R07C-01`已由获批的`R8-WP01G-R07C-R01 / R8-CENTRALSEAL-001`关闭；首次seal先清退旧publication，Camera保持enabled，1000/0GC非回归PASS；
- 回滚方式：仅删除本Change新增的Editor-only probe及meta，并把本记录标为`ROLLED_BACK`。

## 9. Git / 交接

- 修改前工作树基线：存在大量用户/历史修改与未跟踪文件；本Change不覆盖、不清理、不回退；
- 实际diff范围：仅新Editor-only probe、`.meta`及R07C留痕文档；production脚本0改动；
- 提交hash：未提交；
- `Tools/Validate-ChangeLedger.ps1`结果：84 records / 99 governed code files，PASS；
- repair收口后的最终validator：85 records / 103 governed code files，PASS；
- 交接需优先阅读：R07C Task、R07C Handoff、本Record。
