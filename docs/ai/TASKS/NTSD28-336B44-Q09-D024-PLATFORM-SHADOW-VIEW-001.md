# D-024 平台阴影源高度的共用显示出口

状态：`VERIFIED (scoped shadow display conversion)`。当前新版总表 Q07/D-024、Q09 的单一比例消费者，非新的平台搬运或角色矩阵。原Editor4/4定向GREEN与原Battle Scene31tick消费者已通过，三项自然平台阴影命令符合统一高度比例，2945项已有规则样本不变；正常关闭零残留/Scene身份稳。见[验收报告](../../../artifacts/diagnostics/NTSD28-336B44-Q09-D024-PLATFORM-SHADOW-VIEW-20261005/REPORT.md)。以下候选/拟验证为建立时快照；GPU/真人键/Legacy原Scene自然截图未验，不关闭父阶段。

## 已确认来源与候选首差

规则权威为正式根 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 及其 playable 闭包。`battle_world.cpp` 的平台 operation30 将源 `position.y + dvy` 写入 `render_shadow_offset_10c`；`render_snapshot.cpp` 使用源 `position.z + render_shadow_offset_10c` 定位阴影。两源文件均由正式 build.ps1 的 core source 闭包编译。

当前正式普通输入 OID36/56/2 链已有 [原 Battle Scene 96tick记录](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-PLATFORM-NATURAL-SCENE-001/REPORT.md)与源/根对照：相对29/30/31的鸣人平台偏移为 -50/-58/-64。当前 Unity 中央 `BattlePresentationCoordinator.BuildCommands` 与 Legacy `LF2Entity.UpdateShadow` 都直接把该源偏移加到已投影 Z；按用户 D-024 统一高度倍率1152/730，三处计算缺口分别为28.904109589、33.528767123、36.997260274视图像素。该计算是当前源码与既有可达字段的静态首差候选，尚未冒充本轮原Scene命令/GPU测量。

## 精确修改范围

- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs::UpdateShadow`：仅阴影最终高度偏移消费 `RegisteredWorldForSimulation.SpatialProjection.SourceDeltaToViewY`；无注册 World 时保持恒等，不新增 singleton 创建。
- `Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationShadowBuild.cs::BuildCommands`：仅 Shadow 位置消费冻结 frame 的同一源高度投影。snapshot 中10C保持源整数；sorting Z、脚标、身体及其它命令不变。
- `Assets/NTSD/Scripts/Test/Editor/BattlePresentationCommandWriterEditorTests.cs`：仅参数化已有两条平台阴影测试，保留恒等+23与新增固定视野-50。中央命令/冻结复制和真实 Legacy SpriteRenderer 各两参数，共4项；禁止整类/全套运行。

不改 DAT、正式源码/EXE、源 Y/Vy、平台碰撞/链接、输入/RNG、图像尺寸1.5、背景/相机/Scene、非战斗框架；不修改名字显示（WORDS用户删除）、不新增生命周期模块。现有 dirty 文件须在其基础上最小修改。

2026-10-05 原Scene消费者精确增量（脚本前）：第四路径 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C040NaturalScenePlayProbeEditor.cs`，仅既有platform模式的runId `d024-shadow-height-` 前缀。增加只读ShadowSample列表与 `CapturePlatformShadowPresentation`，将本opt-in截在既有自然输入31tick；29～31各取实际中央command与source10C，要求当前published/plan同tick且alpha1，高度偏移按独立1152/730公式正确，排序/源字段稳定，最终有序关闭零残留、保护四SHA不变。旧platform96tick和所有其它mode不变，不修改输入配方、不拍截图。运行前先按文件操作合同备份/记录旧请求与新结果生命周期，保护原Editor干净闲置状态。

## 验收与回滚

首轮RED后测试范围更正：旧中央夹具“全部非Shadow命令位置不变”包含本就跟随平台10C的姓名牌，恒等也因此失败；当前Entity隐藏、无HitRecord，只存在Shadow与OverlayGlyph。本项只排除用户已排除的OverlayGlyph位置比较，类型/Z/排序及唯一Shadow目标保持严格断言；不更改姓名牌规则、WORDS资源或增加UI验收。

先登记Record/三处恢复索引，再写测试；生成工程和原Editor精确4项应量得固定视野两出口RED而恒等保持，后生产最小修复→同4项GREEN。随后若追加原Scene自然阴影命令出口，必须先补充本Task/Record准确探针符号与临时请求Operation；复用已存在自然三人输入前31tick，不扫新平台/角色。所选源字段、排序、脚标及关闭/Scene身份保持；GPU、真人键和其它未覆盖条件如实保留。

回滚只前向撤销本ID精确显示换算与新增参数化测试，不执行git restore/reset/clean、不回退用户已有dirty或旧已证行为；保留所有失败、计算与验收原件。
