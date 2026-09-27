# Q09 / P-01、P-02 展示插值入口审计（2026-09-27）

状态：`AUTHORITY_TEST_PASS / UNITY_CONSUMER_MISSING / IMPLEMENTATION_NOT_STARTED`。这是 Q09 可独立推进的表现子出口；不改变 Q07、Q08、BATCH-04 或 Q09 的开放状态。

## 当前正式版可达路径

- `source/ntsd28_playable/src/main.cpp` 约 2620–2650 行仅在 `current_battle_snapshot.tick + 1 == published.tick` 时保留上一快照，否则清空。约 2724–2758 行仅在 `render_fps > 30` 且上一快照与当前快照存在时，按当前快照开始后的墙钟时长除以 active tick 秒数计算 `[0,1]` 的 alpha，并调用 `sample_render_presentation28`；30 FPS 不走插值采样。
- `source/ntsd28_playable/src/presentation_interpolation.cpp:42` 对相邻 tick、相同 slot/OID/generation、关系字段相同、各轴位移未超过 `max(64, max(|前后 motion|)*4+4)` 的实体求展示 delta；不满足条件时采用当前快照。相机也在相邻 tick 采样，但 Unity 固定相机是用户保留的例外，不能为实现 P-02 擅自恢复移动相机。
- `source/ntsd28_playable/src/d3d11_renderer.cpp:1521` 及后续绘制按 slot 展示 delta 处理角色、阴影、流血标记、复活文字、名牌等；游戏动作、HP、碰撞仍取当前逻辑快照。
- 直接用当前正式源码的 `presentation_interpolation.cpp` 与 `tests/presentation_interpolation_tests.cpp` 以 g++ C++17 编译到项目 `Temp` 并运行：`PASS stable alpha, discrete authority, generation/relation/teleport/tick gates`。这是正式源码单测，不是根目录正式 EXE 的图像采样，也不是 Unity 运行验收。

## Unity 当前接线和首个结构缺口

- `Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs:996` 的 `RenderAlpha` 仅更新 Inspector/公开数值；`LateUpdate` 调用 `PresentLatestFrame`。`BattleCentralRenderSystem.QueueLatestPublishedFrame`/`MaterializeLatestPublishedFrameForCamera` 只取当前 `PublishedFrame`，`BattleRenderFeature.AddRenderPasses` 使用该次物化结果。未发现把相邻帧及 alpha 传入正式中央绘制消费者的路径。
- `BattlePresentationShadowBuild.CaptureBuildAndPublishFrame` 在 `frameA/frameB` 间交换 `PublishedFrame`，然后释放旧帧 publication binding。下一个 tick 会复用旧帧的存储；直接保存 `previousFrame` 引用供未来相机帧采样可能读到被重写的内容。现有 `BattlePresentationFrame.CopyFrom` 能复制冻结内容，但其资源绑定、命令物化、生命周期和零分配约束必须在正式实施 Task 中逐项验明。
- 因此 P-02 的生产改动不应是单独把 `RenderAlpha` 传给材质；必须先给中央渲染建立稳定的相邻 previous/current 展示数据，按 slot generation/关系/大位移门控，然后只改展示位置。30 FPS 保持当前离散快照；60/120 FPS 对有上一帧的连续实体采样。固定世界相机和用户批准的物理位移比例保留。

## 下一实施包的精确出口

### 2026-09-27 数据所有权与字段映射收窄

正式 `render_snapshot.cpp` 在可见 sprite/帧门之前，对 World 每个占用 slot 发布 `presentation_entities`：slot、OID、generation、精确 XYZ、motion XYZ 和六个关系字段。Unity 当前 `BattlePresentationEntitySnapshot` 只携带展示整数位置、handle/OID及 `LinkState`；`CaptureAndBuild` 又会跳过 `FirstPresentationTick` 未到、待销毁或休眠实体，所以直接把该结构当正式 motion row 会漏前一 tick 的逻辑实体。`BattlePresentationFrame` 的 A/B 发布存储在下一个 tick 被复用，`CopyFrom` 虽复制值数组，catalog 引用却由提交绑定持有，不能把整个旧 frame 作为长期 previous 所有权。

可核对的 Unity 载体候选：`RuntimeEntityHandle.Generation` 对应 generation，`OwnerSlotIndex` 对应 owner slot，`HolderStableId` 对应 linked parent，`TargetSlotIndex` 对应 linked child，`CaughtSlotIndex` 对应 catch target，`CatchSourceSlot90` 对应 catch source，`LinkState` 对应 interaction state。持武器写者同时维护 `TargetSlotIndex` 与 `HeldWeaponStableId`，但二者并非在所有路径天然同义；落代码前须以正式关系写者和 Unity 全部生产写者核对，不能把 `CatcherSlotIndex`（含编码credit路径）误作 catch source。精确 X/Z 应先读当前源规则坐标（有 `SourceRulePositionInitialized` 门），展示 delta 最后才做一次已批准的世界比例换算；Y/运动速度仍要核其源规则单位。`NTSDRenderSpace.BattleVisualScale` 是精灵尺寸倍率，不是实体位移倍率。

最窄实施顺序是先让每个已发布逻辑 tick 的当前帧自带 previous/current **纯值** motion rows 和 tick 标识，包含隐藏实体且不持有旧图片租约；在 N、N+1、N+2 发布及 A/B 复用后验证 N+1 冻结历史稳定。然后独立处理中央渲染同一逻辑 tick 可按新 Unity 展示帧重采样：当前 `BattleCentralRenderSystem` 的 publication-version 缓存和 `PrepareFrameImmediate` 的 same-tick 缓存都会直接返回旧计划，只传 alpha 无效。最后才把经正式关系/teleport 门控的 delta 应用于精灵、阴影和随身提示，并做 30 离散、60/120 采样、逻辑 checksum 不变的原 Editor 验收。已有 Q07 持武器 24-tick 同初态轨迹可复用为连续移动正例，不为扩大案例数重跑；generation、关系切换、teleport、跳 tick 是必要的反例。

先在原项目用现有正式内容/原 Battle Scene 确定一个移动实体的连续两 tick，记录原逻辑快照、中央命令和不同展示 alpha 的画面坐标；同时覆盖同 OID 但 generation 变化、关系切换、大位移与跳 tick 的禁插值对照。脚本修改前建立独立 Task、Change Record 与 Ledger，明确冻结数据所有权和有序关闭阶段。验证限此入口的正式源码测试、Unity 聚焦测试、原 Editor 中 30/60/120 展示采样与逻辑 checksum 不变；自然战斗、正式 EXE 同画面对照和 Q09 整体出口另验。若冻结 publication 与现有 worker/材质提交冲突，先留首差和风险，不用单帧临时补丁冒称对齐。
