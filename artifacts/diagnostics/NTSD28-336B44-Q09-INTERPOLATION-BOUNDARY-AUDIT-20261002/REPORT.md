# Q09 相邻快照插值边界只读复核（2026-10-02）

状态：`SOURCE_UNITY_STATIC_BRANCH_REVIEW / NATURAL_RUNTIME_PENDING / Q09_OPEN`。本报告不构成正式 EXE 像素、自然 Play 插值或整阶段对齐证书。本轮未修改战斗脚本、DAT、图片、Scene 或 ProjectSettings。

## 身份和实际入口

- 根目录正式 `NTSD2.8-Logan.exe` 本轮 SHA-256 为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。
- 正式 playable `main.cpp` 在逻辑步后发布当前 render snapshot，仅当前 tick 与前次 tick 相邻时保存前次快照；`presentation_interpolation.cpp::sample_render_presentation28` 逐实体采样，`d3d11_renderer.cpp` 消费相同 delta。原 Unity 在 `BattlePresentationShadowBuild.CaptureBuildAndPublishFrame` 以已发布帧为前帧，仅相邻 tick 复制其运动状态；`BattlePresentationDisplayMotion.Prepare` 调用 `BattlePresentationMotionSampler.Sample`。

## 静态对照

| 条件 | 当前正式源码 | 当前 Unity | 证据界限 |
| --- | --- | --- | --- |
| 相邻 tick、同 slot/object/generation | 非相邻直接离散；身份变化跳过该实体 | 非相邻不采样；Handle/ObjectId 变化返回 `IdentityChanged` | 分支对应，尚无自然实体逐帧对照 |
| owner、关联父子、抓取目标/来源、交互状态 | 任一变化跳过插值 | 六项变化返回 `RelationChanged` | 字段映射仍需在自然关系事件中验 |
| X/Y/Z 突变 | 每轴阈值 `max(64, max(abs(前后 motion))*4+4)` | 同一表达式 | 公式对应，真实传送/投掷断点待验 |
| 坐标采样 | `std::lround` 源坐标插值值减当前值 | `MidpointRounding.AwayFromZero` 先在源域取整，再把 X/Z 显示 delta 经 D-024 比例投影 | 局部算式对应；完整像素/视口未验 |
| 源规则坐标缺失 | 正式 snapshot 的精确坐标参与采样，无 Unity 初始化标志 | `HasSourceRulePosition=false` 时拒绝采样 | **条件性适配差异**；正常生产可达性未证，不能称视觉故障 |

Unity 现有 `BattlePresentationMotionSamplerEditorTests` 覆盖上述合成正反条件，包括 65 像素传送和有 motion 支持的冲刺；这些测试不是正式 EXE 同帧画面。此前 Q09 发布时钟子出口已有原 Battle Scene 定向证据，故本次不重跑。

## 后续决策

1. 在原 Battle Scene 的自然相邻发布中检查可见实体的 `SourceRulePositionInitialized`、关系字段及 `Sample` 结果，优先找首次非采样的正式可达实体；无阳性不修改共用采样器。
2. 以当前正式 EXE 的真实显示帧与 Unity 已取得的同 tick 合成 Game View 定义可比区域。正式 EXE 的 `--gui-acceptance-report` 只输出状态，`offscreen_gate_main.cpp` 的 PNG 是辅助程序产物，均不能替代正式 EXE 像素。
3. Q09 的 previous/current 发布、传送/关系断点、阴影/出血/名牌等自然画面与正式 EXE 像素仍开放；本审计不提升 Q09 或任何 P 项为完成。
