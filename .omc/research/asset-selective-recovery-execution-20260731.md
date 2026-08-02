# Asset → Assets 选择性恢复执行记录（2026-07-31）

## 原则

- `Assets` 以 Git HEAD `ee3fc759e77b3c89531108ea25288522e7b1421` 为干净基线。
- `Asset` 是“旧文件 + 较新候选 + 损坏数据”的混合恢复目录，禁止整目录覆盖。
- 只恢复内容完整、依赖闭合、可由当前代码与测试证明等价的片段。
- `Asset` 中没有可证明晚于 HEAD 的正式场景、预制体、图片、材质、配置或 DAT 资源。

## 已恢复

1. 中央动态 Mesh：active/dirty chunk 定向清理；单次 min/max bounds 累积；2→1→0 chunk 与稳定帧测试。
2. parity/诊断：默认 slot category 保留；全零可选 probe 归一化为 null；默认关闭的 presentation phase diagnostics。
3. Tick/生命周期：`PendingFlushDestroy` mutation epoch；pending-destroy 扫描缓存；Early frame 与 PreInteraction no-op proof。
4. AI：immutable nearest snapshot facts/stamp；specialized filter 与 legacy A/B；stale epoch/generation/identity fail-closed；Unity EditMode `3/3` 通过。
5. 碰撞：role-aware direct/tree 自适应；direct threshold；共享 frame body-template cache；退化 bounds 本地 fallback；candidate 顺序、数量、RNG、slot generation、异常回退等价；Unity EditMode `21/21` 通过。
6. 中央表现基础：`LF2Sprite` managed-only 状态接口；legacy renderer 写入与托管状态拆分；shadow managed visibility boundary；`NTSDRenderSpace.ViewportTransformSnapshot`。
7. 中央资源 resolver：Configure 同引用/同契约 no-op；catalog/common/material 引用变化整代失效；destroyed Unity texture/material fail-closed；warmed template 零分配与字段一致性；Unity EditMode `15/15` 通过。

## 暂不恢复

- zero-attack-itr short circuit：依赖尚未闭合的 Late collision snapshot proof；本批明确剥离。
- flat nearest、lazy global spatial、OID dispatch、CharacterInput skip：恢复终态曾伴随 5 个 EditMode 失败和 self-check FAIL，不能直接采用。
- Late snapshot、StageRender 与 StageBounds：Registry/StageRender 恢复源损坏，需从当前基线重建。
- Stress Harness 后续诊断消费者：等待 AI/碰撞/Late/Presentation 生产 API 全部闭合。
- T8 默认 `stage.dat`：用户明确排除。
- Android 真机验证：用户自行处理。

## 明确损坏、禁止覆盖的核心文件

- `LF2ObjectPointFactory.cs`
- `LF2ObjectRenderer.cs`
- `BattleCentralRenderTypes.cs`（恢复副本实际为 Texture2D YAML）
- `BattleSpriteCatalog.cs`
- `BattlePresentationShadowBuild.cs`
- `SimulationWorld.Registry.partial.cs`
- `SimulationWorld.StageRender.partial.cs`
- `LooseQuadtreeBroadphase.cs`

其中 resolver 的较晚行为不是从损坏生产文件复制，而是依据完整测试在当前基线重建。

## 验证证据

- 标准 Unity：`D:\Unity\HubEditor\2022.3.40f1\Editor\Unity.exe`
- 隔离项目：`.omc/validation/asset-recovery-project`
- AI：`.omc/validation/AssetRecovery-AiNearestTests2-20260731.xml`，`3/3`
- Collision：`.omc/validation/AssetRecovery-RoleAwareFormalTests-20260731.xml`，`21/21`
- Resolver：`.omc/validation/AssetRecovery-ResolverTests-20260731.xml`，`15/15`