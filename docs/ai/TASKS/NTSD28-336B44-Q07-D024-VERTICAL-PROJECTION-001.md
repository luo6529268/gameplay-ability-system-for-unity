# NTSD28-336B44-Q07-D024-VERTICAL-PROJECTION-001

> **2026-10-05 D-024 R120单次原Scene命令出口已限定通过：** [原件与边界](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-VERTICAL-FRACTION-20261005/R120-SCENE-ACCEPTANCE.md)：原Editor MCP刷新后32tick PASS/DONE，相对23自然alpha0.0156545、源lround deltaY=-2，本体3.156185px符合统一1152/730倍率、地面影子0偏移。完整32规则samples与修复后基线相同；退出Scene clean/nonPlay/SHA稳、Console0error、临时请求恢复。探针Record为FOCUSED_TEST_PASS，父生产Record仍RUNTIME_PENDING；本必要ONE已完成，当前P0=DEP=ONE=0。用户保留1.5倍实体显示尺寸，DAT和非战斗边界保持。设备真实120FPS/GPU Present、落地画面及原Scene空中命中等保持实际首差/终验条件门，不自动扩成角色矩阵；Q07/Q09/Q12及总目标开放。下方PLANNED/尚未运行/R120未知等为本轮之前快照。

> **2026-10-05 命中邻例限定GREEN：** 原Editor精确6/6 PASS、生成Editor0错；[实际C14提交证据](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-VERTICAL-FRACTION-20261005/Y-HIT-RESPONSE.md)覆盖两视口空中Y相交/接触/分离、HP/HitCount/源Y/CRT及+3/-3停顿/arest4/vrest1。一个错误停顿预期已按当前正式target type分支纠正，旧失败保留。只改下方预声明的测试方法与助手，不改生产/资源；该ONE已完成，不重复已过样例。父Record仍RUNTIME_PENDING，原Scene/正式根空中同态及R120原Scene未验。

> **2026-10-05 本轮必要命中邻例：** 现有候选边界只证明相交1/接触0/分离0，没有验证伤害提交。本Task新增测试所有权仅为 `Assets/NTSD/Scripts/Test/Editor/NTSD28B5StandardHitRestProductionIntegrationEditorTests.cs` 的一个方法及专用位置助手；用源Y=-40与-11/-10/-9、恒等/固定视口共6例贯穿生产候选及C14消费，检查相交才扣10HP、命中计数和源Y不被比例换算改写。准确需求/风险/验收/回滚已在同ID Change Record脚本前登记。原Scene自然落地规则及活跃kind0火花已由后继证据补齐；Y向实际Scene命中、落地画面与R120原Scene仍保留各自限制。

> 2026-10-05 有限 GREEN：原 Editor 定向3/3、原 Battle Scene OID85/action212 的32 tick规则样本改前/后相同，画面Y差2源像素现为3.1561852/1152，与正式2/730相符；Play退出Scene clean/SHA稳。[验收原件与限制](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-VERTICAL-FRACTION-20261005/ACCEPTANCE.md)。状态 `RUNTIME_PENDING`：真实着地、Y向命中/R120 Scene、kind0火花仍待；下方 `PLANNED` 为脚本前快照。

> 2026-10-05 更新：聚焦断言已先写，生产/GREEN/原 Scene 待；下方 `PLANNED` 是脚本前快照。

状态：`PLANNED`。从 [D-024 垂直比例唯一 Task](NTSD28-336B44-Q07-D024-VERTICAL-FRACTION-001.md) 的原 Battle Scene 阳性首差进入。唯一权威为根目录 SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 的正式 EXE 与对应 playable `physics_integrator.cpp`、`object_spawning.cpp`、`battle_world.cpp` 和 `render_snapshot.cpp`。用户保留固定完整背景相机、项目地图、现有 `BattleVisualScale=1.5` 本体图片尺寸，不改 DAT 数值。

目标是源规则 Y 与项目视图 Y 的共用出口：源 `Runtime.Y/YInt/Vy`、重力、floor、平台、OPoint/武器出生和抓取状态都维持正式规则；共用 `BattleSpatialProjection` 只将全局源高度距离按视口高比 `1152/730` 投到画面。本体、头顶血条、owner-relative state9997 与 R30/R120 插值使用该出口，地面阴影不跟随 Y；普通 bdy/itr 碰撞矩形在源规则端点取整/哨兵处理后，两侧各投影一次。sprite 本地尺寸/挂点继续按已批准的 1.5 倍处理。

当前 Unity 原状与全部消费者分域见 [Y 双域审计](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-VERTICAL-FRACTION-20261005/Y-DOMAIN-AUDIT.md)。本包只覆盖这些共用身体/碰撞出口；正式 kind0 火花的源 `targetZ+hitY+jitter` 与 Unity 混合 `viewZ+sourceY` 另设同一 D-024 后续出口，在本包未解决前不能报告纵向对齐完成。WORDS 绘字和非战斗 UI 不作为此次实现入口。

代码所有权：`BattleSpatialProjection.cs`；`LF2ObjectRenderer.cs`；`BattlePresentationShadowBuild.cs`；`BattlePresentationMotionSampler.cs`、`BattlePresentationDisplayMotion.cs`；三处调用者 `BattleCentralRenderSystem.cs`、`SimulationStageRenderModule.cs`、`BattleEntityOverlayRenderer.cs`；`BruteForceSceneQuery.cs`；聚焦测试 `NTSD28D024UnifiedSpatialProjectionEditorTests.cs` 与 `BattlePresentationMotionSamplerEditorTests.cs`。如需要改变上述范围，先修订本 Task 与 Change Record，再改脚本。Scene、Prefab、DAT、图片、Unity/GAS 非战斗代码和正式源码不在范围。

2026-10-05 脚本前追加一个测试所有权：现有 `NTSD28B5MultiBodyCandidateProductionEditorTests.cs` 仅补纵向相交/接触/分离的真实候选聚焦用例，沿用原生产夹具和两种视口配置，检查 Y 矩形在 D-024 后仍与源规则同态。对应 Change Record 已同步增加 `code-path`。此增量不扩成全角色/全技能矩阵。

风险是 Y 与已投影 Z 混乘、R120 插值和离散 pivot 不连续、bdy/itr 一侧漏投、full-height sentinel 被误投，以及 frozen presentation frame 泄漏旧倍率。先用一个投影/插值聚焦 RED 断言，再改共用出口；生成 C# 与原 Editor 0 error 后，仅复用 OID85/action212 同动作同Z自然 Play、一个落地邻例和一个 Y 矩形邻接/哨兵边界；核源规则 trace 不变、身体相对影子 2 源像素在 1152 高画面约变 `3.15616` 像素、R120 中间 alpha 连续、Play 退出 World/池清空且 Scene clean/SHA 未变。未跑的层级如实保留 `RUNTIME_PENDING`。回滚仅按本 Change Record 的脚本 diff 手工反向恢复，不碰用户其它工作或 Git 破坏性操作。
