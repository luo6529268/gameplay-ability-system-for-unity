# Asset 与 Assets 语义审查（2026-07-31）

## 范围与结论

本报告只审查恢复目录 `Asset/` 与当前活动目录 `Assets/` 的内容；没有复制、删除或修改其中任一文件，也没有运行 Unity。

**结论：不可将 `Asset` 整体视为“更新版”，也不可整体覆盖 `Assets`。** `Asset` 是历史旧文件、HEAD 后候选改动与损坏恢复块的混合；`Assets` 是完整且由 Git 验证的活动基线。`Asset` 只能作为后续逐文件、逐依赖组“选择性重放补丁”的证据来源。

证据基线：

- [文件清单与哈希审计](asset-vs-assets-inventory-20260731.md)
- [Git 历史审计](asset-vs-assets-git-history-20260731.md)

恢复目录中的完整候选也明确是未验收的中间状态：当时 EditMode 为 `301/296`（5 项失败），`BattleRuntimeSelfCheck` 为 FAIL，1000 AI 尚未达到 30 Hz。因此“语义较晚”不等于“可以直接采用”。

## 第二恢复容器映射补充

独立复核确认此前还存在一条恢复容器映射：

- `Asset/NTSD/00040000001DF94F27659311` → `Assets/NTSD/Sprite`

该容器共 688 项，其中 320 项与 `Assets/NTSD/Sprite` 内容相同，368 项内容不同；这 688 项均无 post-HEAD 时间证据。mtime 不能证明文件来源、新旧或正确性，因此不将其列入“较新候选”，并继续保持 `Assets` 为活动基线。

补充该映射后的完整目录统计如下：

| 范围 | 共同且内容相同 | 共同但内容不同 | 仅 `Asset` | 仅 `Assets` | 路径并集 |
|---|---:|---:|---:|---:|---:|
| 全树 | 4710 | 1592 | 29 | 1441 | 7772 |
| NTSD | 4696 | 1580 | 29 | 326 | 6631 |

该补充只修正恢复容器的全树清单覆盖范围。下述 **54 个 post-HEAD 候选、文件完整性分类、依赖关系及语义采用结论均不变**。

## 54 项 post-HEAD 候选统计

| 类型 | 项数 | 审查结果 |
|---|---:|---|
| 共同差异 | 32 | 18 个内容完整，14 个明确损坏 |
| recovered-only | 22 | 11 个 `.cs` 与 11 个配套 `.meta` |
| recovered-only 完整代码 | 6 | 1 个诊断运行时代码、5 个 Editor 测试 |
| recovered-only 损坏代码 | 5 | `.meta` 完整，但源码不可用 |
| 完整文本内容文件 | 24 | 后续功能、性能优化、诊断或测试；并非简单回退 |
| 损坏或内容错配文件 | 19 | 不可采用 |
| recovered-only `.meta` | 11 | GUID 唯一，但未被场景、Prefab、菜单或其他资产引用 |
| 场景、Prefab、图片或其他正式资源候选 | 0 | 无需资源迁移决策 |
| 仅 CRLF/行尾差异 | 0 | 无 |

另有 2 项 mtime 看似 post-HEAD、但 blob 已在可达 Git 历史中的文件，已排除出上述 54 项候选；它们是旧历史内容，明确保留 `Assets` 版：

- `Scripts/Animation/Rendering/BattleCentralRenderSystem.cs`
- `Scripts/Test/Editor/LooseQuadtreeNearestEditorTests.cs`

## 可优先选择性重放的两个小组

这两组是“建议后续选择性采用候选”，不是已确认可直接覆盖；采用后仍必须 fresh compile、对应 Editor tests 与 `BattleRuntimeSelfCheck`。

| 小组 | 路径 | 语义判断 |
|---|---|---|
| Dynamic Mesh Backend | `Scripts/Animation/Rendering/BattleDynamicMeshBackend.cs` | 仅清理 dirty chunk，而非扫描全部容量；quad bounds 改为 min/max 累积。 |
|  | `Scripts/Animation/Rendering/Editor/BattleDynamicMeshBackendSegmentBoundsEditorTests.cs` | 为 dirty-chunk 清理及 bounds 行为补充测试；必须与 Backend 同组。 |
| Parity Trace | `Scripts/Simulation/BattleParitySnapshot.cs` | 默认场景也保留真实 `category`，不再强制写 0。 |
|  | `Scripts/Test/Editor/BattleParityTraceEditor.cs` | 将 Unity 序列化产生的全零可选 frame-counter probe 归一化为 null；必须与 Snapshot 同组。 |

## 12 个完整但依赖不闭合的文件

这些文件确属后续工作，但不能独立采用。

| 路径 | 后续语义 | 阻塞原因 |
|---|---|---|
| `Scripts/Animation/Character/BruteForceSceneQuery.cs` | role-aware direct/tree、body template cache、zero-itr short circuit、诊断计数 | 依赖损坏的 `LooseQuadtreeBroadphase.cs`；当时 self-check 失败。 |
| `Scripts/Animation/LF2Objects/LF2Entity.cs` | shadow managed state、early/late mutation report、stage/runtime snapshot proof | 依赖 Passes、StageRender 等完整组。 |
| `Scripts/Animation/LF2Objects/LF2Sprite.cs` | 将 Sprite/可见性操作拆成 managed-only API | Renderer、Catalog、ShadowBuild 均有损坏依赖。 |
| `Scripts/Animation/Rendering/Editor/BattleCatalogCentralResourceResolverEditorTests.cs` | resolver cache 保存、失效和 destroyed-resource 测试 | 生产实现 `BattleCentralRenderTypes.cs` 损坏。 |
| `Scripts/Animation/Rendering/Editor/ProductionEntityStressEditorTests.cs` | A/B、phase timing、presentation timing、late-opoint 统计测试 | 依赖大量后续诊断 API。 |
| `Scripts/Animation/Rendering/Editor/ProductionEntityStressHarness.cs` | AI/碰撞/Late 等 A/B 开关和计时报告 | 依赖 AI、碰撞、Passes、Presentation 全组。 |
| `Scripts/Animation/Runtime/NTSDRenderSpace.cs` | viewport transform snapshot，减少重复坐标换算 | 调用链未完整恢复，snapshot 没有完整生产调用方。 |
| `Scripts/Simulation/NTSDEntityRuntime.cs` | `PendingFlushDestroy` 变为带 mutation epoch 的属性 | 依赖 Passes 和损坏的 Registry。 |
| `Scripts/Simulation/SimulationWorld.AiInput.partial.cs` | flat nearest index、snapshot facts/stamp、specialized filter、lazy spatial、OID dispatch 优化 | 规模大，多个配套测试损坏，不能单文件采用。 |
| `Scripts/Simulation/SimulationWorld.Passes.partial.cs` | early frame、late snapshot、pre-interaction、opoint guard 等热路径优化 | Registry、StageRender、部分测试损坏。 |
| `Scripts/Test/BattleRuntimeSelfCheck.cs` | AI OID dispatch 等价检查，并强化 tree/brute candidate/RNG 检查 | 单独采用会缺 AiInput、BruteForceSceneQuery 新 API。 |
| `Scripts/Test/Editor/RoleAwareCollisionShadowSelfCheckTests.cs` | direct/tree、threshold、template、fallback 等等价性测试 | broadphase 核心文件损坏。 |

## 两份仅供取证的文档

- `Docs/central-battle-render-system-plan.md`
- `Docs/HANDOFF-codex-battle-alignment.md`

它们确实记录了较晚的 2026-07-27 性能结论，但同时记录 1000 AI 未达 30 Hz、EditMode 有 5 项失败、self-check FAIL，以及应由小修补转向数据导向/ECS 热循环迁移。活动代码仍以 `Assets` HEAD 为基线时，直接覆盖这些文档会让文档与代码状态不一致。保留为恢复取证，待对应代码重新建立后再更新活动文档。

## 14 个共同差异中已损坏、必须保留 Assets 的文件

| 路径 | 损坏证据 |
|---|---|
| `Docs/csharp-vs-unity-battle-alignment.md` | 大量 NUL 和序列化二进制字段。 |
| `Scripts/Animation/Character/LF2ObjectPointFactory.cs` | 内容为程序集/Unity 类型字符串及二进制数据。 |
| `Scripts/Animation/LF2Objects/LF2ObjectRenderer.cs` | 随机二进制块。 |
| `Scripts/Animation/Rendering/BattleCentralRenderTypes.cs` | 开头为 `%YAML` / `Texture2D`。 |
| `Scripts/Animation/Runtime/BattleSpriteCatalog.cs` | 二进制污染。 |
| `Scripts/Simulation/Presentation/BattlePresentationShadowBuild.cs` | 大量二进制表和 NUL。 |
| `Scripts/Simulation/SimulationWorld.Registry.partial.cs` | 大量 NUL/二进制。 |
| `Scripts/Simulation/SimulationWorld.StageRender.partial.cs` | 二进制内容。 |
| `Scripts/Simulation/Spatial/LooseQuadtreeBroadphase.cs` | 实际为 AnimationCurve/YAML 片段。 |
| `Scripts/Test/Editor/AiMoveModeSnapshotEditorTests.cs` | 二进制内容。 |
| `Scripts/Test/Editor/BattlePresentationBeginFrameReuseEditorTests.cs` | 序列化 importer 字段。 |
| `Scripts/Test/Editor/BattlePresentationCommandWriterEditorTests.cs` | 二进制内容。 |
| `Scripts/Test/Editor/LateRuntimeSnapshotBoundaryEditorTests.cs` | 二进制内容。 |
| `Scripts/Test/Editor/TeamPartitionGroundNearestEditorTests.cs` | 二进制内容。 |

## 6 组 recovered-only 完整代码与 meta

每组均有配套 `.meta`；GUID 在 `Asset/Assets` 范围内无重复。但没有 asmdef、菜单、场景或 Prefab 显式引用，且均存在生产依赖未闭合问题，不能独立采用。

| 文件组 | 依赖判断 |
|---|---|
| `Scripts/Simulation/Presentation/BattlePresentationPhaseDiagnostics.cs` + `.meta` | 完整的 opt-in timing recorder；只被 Stress Harness/Tests 引用，正式 presentation 调用点未完整恢复。 |
| `Scripts/Test/Editor/AiNearestSpecializedFilterEditorTests.cs` + `.meta` | 依赖 `SimulationWorld.AiInput.partial.cs`。 |
| `Scripts/Test/Editor/EarlyFrameAdvanceOptimizationEditorTests.cs` + `.meta` | 依赖 `LF2Entity.cs` 与 `SimulationWorld.Passes.partial.cs`。 |
| `Scripts/Test/Editor/PendingDestroySlotAdmissionEditorTests.cs` + `.meta` | 依赖 `NTSDEntityRuntime.cs`、Passes 和损坏的 Registry。 |
| `Scripts/Test/Editor/PreInteractionNoOpProofEditorTests.cs` + `.meta` | 依赖 `LF2Entity.cs` 与 Passes。 |
| `Scripts/Test/Editor/StageBoundsRuntimeSyncEditorTests.cs` + `.meta` | 依赖 `LF2Entity.cs` 和损坏的 StageRender。 |

## 5 组 recovered-only 损坏代码与 meta

对应 `.meta` 虽完整，但不能脱离损坏源码采用：

| 文件组 | 损坏证据 |
|---|---|
| `Scripts/Test/Editor/AiNearestFlatShadowEditorTests.cs` + `.meta` | ShaderGraph JSON 片段并含 NUL。 |
| `Scripts/Test/Editor/AiSnapshotRuntimeSlotBuildEditorTests.cs` + `.meta` | 随机二进制内容。 |
| `Scripts/Test/Editor/CharacterInputBasePassEditorTests.cs` + `.meta` | 随机二进制内容。 |
| `Scripts/Test/Editor/LateFrameTickSnapshotOptimizationEditorTests.cs` + `.meta` | 随机二进制内容。 |
| `Scripts/Test/Editor/ZeroAttackItrCandidateShortCircuitEditorTests.cs` + `.meta` | ShaderGraph JSON 片段。 |

## 四个依赖组

1. **AI/碰撞组**：`BruteForceSceneQuery`、`AiInput`、`LooseQuadtreeBroadphase`、RoleAware/Nearest/ZeroItr 测试、SelfCheck。Broadphase 核心和多个测试损坏，恢复文档记录该组 self-check 失败。
2. **Tick/生命周期组**：`LF2Entity`、`NTSDEntityRuntime`、Passes、Registry、StageRender、Early/PreInteraction/Pending/Stage/Late 测试。Registry 和 StageRender 核心损坏，禁止单文件采用。
3. **中央表现组**：`LF2Sprite`、`LF2ObjectRenderer`、`BattleCentralRenderTypes`、`BattleSpriteCatalog`、ShadowBuild、`NTSDRenderSpace`、PresentationDiagnostics。多个核心生产文件损坏，无法从恢复目录直接拼回。
4. **压力测试与诊断组**：Stress Harness/Tests、PresentationDiagnostics，以及 AI/碰撞/Late 的诊断属性。该组是前三组的消费者，不是独立功能。

单文件覆盖会带来三类风险：编译缺符号；测试所述 fast/legacy 路径不存在；更危险的是代码可编译但 default-on 优化缺少完整 parity oracle。

## 采用与不采用规则

### 当前规则

1. 保持 `Assets` 作为唯一活动基线，不修改它。
2. 不整体采用 `Asset`，不复制整棵目录。
3. `Asset` 仅作为恢复补丁来源。
4. 若用户批准恢复，第一批仅考虑两个小组：Dynamic Mesh Backend + 测试；Parity Snapshot + TraceEditor。
5. 其余完整代码按四个依赖组重新生成差异，并人工重放有效 hunk；不得整文件覆盖。
6. 损坏核心文件以 `Assets` 版本为底重新实现，绝不从 `Asset` 复制。
7. 每个重放小组都必须 fresh compile、相关 Editor tests、`BattleRuntimeSelfCheck`。
8. AI/碰撞组还必须验证 slot 顺序、candidate 顺序、RNG state/call-count。
9. 在相关代码重建前，不将两份 2026-07-27 文档作为当前完成状态写回活动文档。

## 本轮操作声明

本轮仅建立审查报告。**没有对 `Asset/` 或 `Assets/` 做任何修改、复制、删除、覆盖或采用操作。**
