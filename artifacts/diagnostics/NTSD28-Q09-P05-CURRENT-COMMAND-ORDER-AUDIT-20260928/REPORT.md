# Q09/P-05 当前中央命令顺序只读复核

状态：`STATIC_ORDER_MATCH_FOR_CURRENT_CENTRAL_COMMANDS / P05_AGGREGATE_OPEN`。这份报告只修正 P-05 的下一步范围，不把源码顺序当成自然 Play、GPU 像素或正式版同视口验收。没有修改脚本、资源、DAT、Scene 或非战斗功能。

正式依据：当前根 `NTSD2.8-Logan.exe` 对应 playable 构建闭包包含 `ntsd28_core/src/rendering/render_snapshot.cpp` 和 `ntsd28_playable/src/d3d11_renderer.cpp`（`source/ntsd28_playable/scripts/build.ps1`）。`render_snapshot.cpp` 中同一实体先投影 shadow、可见 body 与 bleed，继而剩余生命、nameplate、combo；spark 事件在前段采集，但 `entity_commands` 在约 2117–2260 行按 shadow、sprite、bleed、minibar、lives、nameplate、combo、spark 建立 phase ordinal，并按 depth、反向物理 slot、phase 稳定排序。`d3d11_renderer.cpp` 约 1592–1792 行消费此流。minibar 属原生 HUD/结果表现排除范围，此复核不要求复制它。

Unity 当前 `BattlePresentationShadowBuild.BuildCommands`（约 2984–3375 行）对每个已排序实体依次写 Shadow（baseOrder+0）、Entity（+1）、可见低血量 BleedMark（+2）、overlay glyph（+2，`BattleEntityOverlayLayout.TryBuild` 先写剩余生命 Counter、再写 Label），最后 HitRecord/spark（+3）。同阶命令通过递增 `localSequence` 保持本地顺序；`ResolvePresentationBaseOrder` 对索引展示序使用 `rank*4`，不同实体以排名隔离。当前中央命令在这些**已物化且通过 gate**的类型上没有“先写 spark 再写 body”或“把 bleed 放在 body 前”的通用排序缺口。`BattleRenderCommandType` 的枚举数字不是绘制相位顺序，不能凭枚举值 3/4 判定 spark 在 bleed 前。

保留的出口：正式 mode/phase 可见性和 DAT 自定义 shadow 归 P-07/P-12；自然角色/武器的 bleed、lives、nameplate、spark 资源、gate 与相机画面仍按 P-06/P-08/P-09/P-10 验；combo 正式普通路径默认关闭，按 P-11 的启用门槛再处理。Legacy 显示路径及本轮未物化命令没有从上述静态对应获得顺序证明。P-05 只有在具名可达场景出现命令类别、同实体相位或跨实体排序首差时才另立生产 Task/Change；不再以旧表的“按整段顺序补命令”作为无条件修改指令。P-04 同 Z 的既有受控/自然像素证据各自保持原状态。

验证层级：只读源码与现有归属复核；未编译、未运行 Unity/正式 EXE、未生成图像或音频。本报告不关闭 Q09、P-05、BATCH-05 或总目标。
