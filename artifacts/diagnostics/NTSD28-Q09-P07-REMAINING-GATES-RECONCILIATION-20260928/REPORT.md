# Q09/P-07 剩余阴影门槛复核（2026-09-28）

状态：`SOURCE_PATH_RECONCILED / NO_NEW_PRODUCTION_FIRST_DIFFERENCE / GPU_EXIT_OPEN`。本次只读复核正式配对 playable 所用 `ntsd28_core/src/rendering/render_snapshot.cpp` 与当前 Unity 生产出口和现有自检；未重跑测试，也未修改脚本、资源、Scene 或相机。

| 正式 `RenderSnapshotBuilder28::build` 阴影条件 | 当前 Unity 入口 | 当前证据边界 |
|---|---|---|
| `background` 存在、`shadow_path` 非空、资源尺寸正且文件可读 | 中央 `hasCommonShadow`，Legacy 已绑定项目公用阴影 | 项目背景/固定全景是用户例外；`CheckCommonShadowCentralOwnershipContracts` 覆盖空资源、无效材质/纹理和有效命令，但不等于正式同画面 GPU 对比。 |
| `interaction_state >= 0` | `LinkState >= 0` | 既有自检的 held 负例无 Shadow 命令。 |
| `render_phase_008 > -70` 且 `abs(phase)%4 < 2` | `LF2ObjectRenderer.ShouldDrawShadowForHitStop` | 既有自检覆盖 -69/-70、±1/±2，并在发布帧覆盖可见/隐藏两个相位。 |
| 当前 object ID 不是 223/224 | `CurrentDatObjectId` 不是 223/224 | 既有自检将 shell ID 与当前 DAT ID 反向组合，确认按当前定义身份判定；未按目录名或出生 ID 特判。 |
| frame state 不是 3005/9997，当前 frame 存在 | `entity.State`、`HasCurrentFrame` 与 Legacy `ShouldHideShadowForPresentation` | 既有自检覆盖两个 state 与无当前帧。 |
| BMP/frame `shadow` 不等于 1 | 共用 `IsNativeShadowSuppressedForPresentation` | 原 Editor 精确 RED 2/4→GREEN 4/4；保存的 Battle Scene 自然李 J,L 五个 OID204 在可见帧禁显子体阴影、同时普通阴影仍发布，已经单独验过。 |

正式源码还用 `render_shadow_offset_10c` 改阴影位置，并按实体 Z 赋深度；Unity 中央命令使用同名 runtime 偏移与原 Z，Legacy 位置出口也读取该偏移。已有自检验证公用阴影命令先于本体且发布不改战斗 checksum。此处是调用链和现有断言核对，**没有新鲜的完整 SelfCheck 或正式 EXE 同状态画面验收**；不能把静态对应升级为 P-07 整体通过。

下一出口应集中在正式根可观察的同状态阴影/本体关系与项目固定全景画面；仅当出现新的正式可达首差时，才扩展某个 mode/state/object 门的聚焦测试。当前正式非排除 DAT 的 `shadow_pic` 没有可达使用，`shadowsize` 只在用户排除的原版背景 DAT，不把自定义阴影图和尺寸列作无条件迁移任务。OID518 当前自然生存帧是隐藏 pic999，不能以其出生替代可见画面验收。

依据：正式 `source/ntsd28_core/src/rendering/render_snapshot.cpp` 阴影分支；Unity `BattlePresentationShadowBuild.cs`、`LF2Entity.cs`、`LF2ObjectRenderer.cs`；`BattleRuntimeSelfCheck.CheckCommonShadowCentralOwnershipContracts` 与 `CheckHitStopPresentationGates`；`NTSD28-Q09-P07-NATIVE-SHADOW-GATE-001` 和 `NTSD28-Q09-P07-LEE-NATURAL-SHADOW-PLAY-001` 的原 Editor 验收。
