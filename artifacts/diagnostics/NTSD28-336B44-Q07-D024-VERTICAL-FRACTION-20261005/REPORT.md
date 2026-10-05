# D-024 / Q07-Q09：空中高度的画面比例候选（2026-10-05）

状态：`SOURCE_ROOT_SCENE_RULE_TRACE_PASS / DETERMINISTIC_PRESENTATION_RATIO_FIRST_DIFFERENCE_CANDIDATE / GAMEVIEW_PENDING`。本轮只读既有正式根回放、原 Battle Scene 结果、当前生产源码与配置，未运行新 Play、未改代码、DAT、图片、Scene 或相机。正式根 EXE 本轮重算 SHA-256 为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。

**自然正例。** 已有 [C023 正式源码/根样本](../NTSD28-336B44-Q07-C023-C024-ROOT-FRAME-001/guren388-airborne-child/source-entities.csv) 与 [原 Battle Scene 样本](../NTSD28-336B44-Q07-C023-AIRBORNE-SCENE-PLAY-001/airborne-idle-scene-02.json)在所选规则字段通过 1690/1690 对照；正式根 LFR 报告为 `passed=true`。Guren OID84/action388 自然生成 OID85/slot51，后者在相对 tick21、22、23 均为 action212、X742、Z402；Y 分别为 -22、-22、-20，tick22→23 仅高度改变 2 个源像素。原 Scene 样本中同三 tick 的动作和整数 XYZ 一致。这证明该分支真实进入完整 Driver，并避免以不同动作帧或不同 Z 冒充高度变化；它还不是两端实际 GPU 像素对照。

**画面投影。** 正式 playable `source/ntsd28_core/src/rendering/render_snapshot.cpp` 的普通 sprite `screen_top = position.z + position.y - center_y`（约 1723 行），正式逻辑视口高度 730。Unity `Assets/NTSD/Scripts/Animation/LF2Objects/LF2ObjectRenderer.cs::ComputeEntityBottomCenterPivotPixels` 使用 `screenY = (int)displayZ + yInt`（约 618 行），CentralOnly `BattlePresentationShadowBuild.cs` 也将同一 Y 放进本体 pivot；`BattleSpatialProjection` 目前仅定义 X 与 Z 的比例，没有 Y 投影。当前 `GameConfig.asset` 的参考画面高度为 1152，项目相机 1920×1080/PPU100/ortho5.76 也对应 1152 源像素高。Unity 的 `CharacterMechanics` 普通高度积分仍是 `runtime.Y += runtime.Vy`，现有运动采样测试明确断言 `delta.Y` 不乘视图倍率。用户刚确认保留的 `BattleVisualScale=1.5` 只改变图像本体尺寸；同一动作帧的本体位置相对地面变化不会因它自动放大。

在上述 tick22→23 条件中，正式版相对画面高度变化是 `2/730 = 0.2739726%`，Unity 当前生产公式预计是 `2/1152 = 0.1736111%`，后者仅为正式版的 `730/1152 = 0.6336806`。若按 D-024 的同画面占比目标，Unity 应出现 `2×1152/730 = 3.1561644` 个视图像素的高度变化；初始离地 22 源像素对应的目标高度为约 34.7178 视图像素。以上是**生产公式推导的画面比例候选**，并非已有 Game View 实测或正式 GUI 逐像素首差，不能提前报告生产修复通过。

**实现边界。** Y 不是单纯的显示字段。当前普通人物/武器等 Y 积分、floor/landing、平台、Y 向攻击/受击框、OPoint 相对出生与挂点均可能读写 Y；已有 [非积分写者审计](../NTSD28-USER-ALL-ENTITY-MOTION-RATIO-001/NON-INTEGRATOR-WRITER-AUDIT.md)明确将 Y 与 floor/碰撞联合合同留待处理。直接改 `Vy`、DAT、单角色 frame、或只改某一张图的 pivot 都会让规则时序与画面空间失配。应先做一次相对 tick21/23 的原 Scene 本体-阴影位移定向取证，确认生产像素；若成立，再统一定义 Y 的源规则域/视图域、取整、floor、hitbox、出生及插值消费者，在共同出口实施并用同一正例和着地/命中邻例验收。

**正式窗口可复现性限制。** 当前 `main.cpp` 将 `--headless-playback-lfr` 路由到 `run_headless_lfr_playback`，GUI 路由到 `run_windowed`，且解析器禁止 LFR 与 GUI acceptance 组合。因此不能把这条已有 LFR 直接作为正式 GUI 的同输入逐像素回放；已有正式根 trace 是规则端，正式可观察画面仍需另取得可比载体。不得为取得截图修改正式 EXE/source 或重复无同态 GUI 捕获。

## 2026-10-05 原 Battle Scene 单例取证

[原始 JSON](../NTSD28-336B44-Q07-C023-AIRBORNE-SCENE-PLAY-001/d024-vertical-20261005-01.json)：原项目 Editor 已经 MCP `refresh_unity(force/all/compile=request)`，生成 Editor 工程和原 Editor 编译均 0 C# error；单个干净 `NTSD_Battle` 场景运行现有 C023 自然 Guren OID84→OID85 探针的 `d024-vertical-` opt-in。状态 `PASS/DONE`，相对 tick21/23 对应全局 tick26/28，OID85/slot51/action212/source Z402，源 Y=-22→-20；两 tick 发布帧、中央 plan 与逻辑 tick 一致，R30 的 display alpha=1，本体/阴影命令各 1 个，视野高均为 1152 画面像素。

本体减阴影高度分别为 `21.9999313` 与 `19.9999809` 画面像素，实际变化量绝对值 `1.9999504`。Unity 画面占比 `1.9999504/1152 = 0.1736068%`；正式规则投影在 730 高视野的同 2 像素占比 `2/730 = 0.2739726%`，前者为后者的 `0.6336648`。保留当前相机的比例目标为 `2×1152/730 = 3.1561644` 个 Unity 视图像素。本例已将「仅公式候选」升级为**生产中央呈现命令的定向实测差异**，且用户确认的 1.5 倍图像尺寸在两个相同动作帧之间是常量，不解释这项运动距离差异。此处正式端的 2 源像素仍是源码投影与同输入 trace 证据，尚无正式 GUI 同态逐像素截图；不扩大为 GPU 层全局结论。

探针主动退出 Play：`exitedPlay=true`、`sceneCleanAfter=true`；Battle Scene 磁盘 SHA-256 前后及复核均为 `253B2EBAD322AA6EC18488DFBBF555BB93AD897940A342E4130E77CE778F9010`。下一步为共同 Y 源规则域/视图域合同与 floor/碰撞/出生消费者审计，未修改生产 Y、DAT、相机或 Scene。
