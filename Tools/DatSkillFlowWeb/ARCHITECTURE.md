# NTSD DAT 技能流程编辑器架构与接口合同

## 1. 目标与边界

本架构服务于 Standard 模式当前已验证范围（Phase 1–6），不创造 NTSD 战斗规则。

- DAT 原始字段和重复顺序由 lossless DAT/CST parser 负责。
- `ntsd_cpp` 是单角色 Native preview 的运行权威。
- 浏览器只持有 opaque session、field、structure 和 asset capability。
- 技能名称与起始帧属于项目侧车元数据，不写入 DAT。
- `itr` 的成对动作字段必须原子编辑。
- `opoint/wpoint/cpoint` 的生成、武器、抓取语义不在第一阶段推断；第一阶段只显示和编辑原始字段、几何位置。

## 2. 模块地图

```text
Browser
├── AppState / reducer
├── ProjectApi
├── SkillMetadataClient
├── SkillFlowGraph
├── FlowSvg / SkillTimeline
├── PlaybackController
├── PreviewRenderer
├── OverlayGeometry
├── CanvasGeometryEdit
├── InspectorModel
└── EditorShell / responsive views

Loopback Server
├── ProjectSkillService
├── ProjectDatService
├── DatSessionService
├── DatStructureEdit
├── WorkspaceRegistry
├── SafeSaveService
└── NativeDatPreviewRunner
```

### Browser ownership

| 模块 | 职责 | 不拥有 |
|---|---|---|
| `AppState` | 统一保存 bootstrap、catalog、skill、session、selection、playback、preview、overlay、inspector、persistence 和 layout 状态 | fetch、Canvas 或 DAT 语义 |
| `ProjectApi` | 统一请求、状态 token、响应错误、请求 epoch | DOM 和本地业务状态 |
| `SkillMetadataClient` | 读取/写入侧车技能 | DAT 文件和 Native preview |
| `SkillFlowGraph` / `FlowSvg` / `SkillTimeline` | 从起始帧遍历真实 `next/hit_*`，保留循环、分支、未知目标；渲染真实边并按 DAT wait 展开 | 自动技能命名、运行结果和时间单位 |
| `PlaybackController` | play/pause/step/seek/loop、末端停止和 timer | 修改 DAT |
| `PreviewRenderer` | DPR、viewport、BMP、sprite、camera、镜像和预览状态 | 逻辑实体真值 |
| `OverlayGeometry` | 六类块的纯坐标变换和 hit-test | 命中、生成、抓取和武器结果 |
| `CanvasGeometryEdit` | capability 约束下的 move/resize、镜像逆变换、snap、键盘和 Esc 草稿 | 创建缺失字段或提交部分几何 |
| `InspectorModel` | 完整 capability 定位、分组、draft、校验和提交 | 投影默认值伪造 capability |
| `EditorShell` | 五区布局、按钮状态、抽屉/标签页和 live region | 保存业务真值 |

## 3. AppState 合同

```text
AppState
├── bootstrap: phase, buildId, token, error
├── catalog: phase, revision, objects, selectedObjectKey
├── session: phase, sessionId, revision, oid, name, serverDirty, diagnostics
├── skills: phase, revision, etag, items, selectedSkillId, metadataDirty
├── selection: frameId, frameOccurrence, edgeId, blockType, blockIndex
├── flow: nodes, edges, cycles, unresolvedTargets
├── playback: playing, tickIndex, loopEnabled, phase
├── preview: ticks, zoom, fitMode, viewport, imageStates
├── overlays: visibilityByType, selectedOverlay, hoveredOverlay
├── inspector: fieldCapabilities, drafts, invalidFields, submitPhase
├── persistence: datSavePhase, skillSavePhase, lastSuccess, lastError
└── layout: tier, activeNarrowTab, leftOpen, rightOpen
```

修改状态必须分开：

1. `draftDirty`：输入框改了但尚未提交。
2. `serverDirty`：已应用到 DAT 会话但尚未覆盖文件。
3. `metadataDirty`：技能侧车尚未持久化。

## 4. 技能侧车合同

固定逻辑路径：

```text
.dat-skill-flow/skills.json
```

顶层 schema：

```json
{
  "schemaVersion": 1,
  "revision": 0,
  "skills": [
    {
      "oid": 2,
      "name": "用户维护名称",
      "startFrame": 0
    }
  ]
}
```

合同：

- 顶层只允许 `schemaVersion`、`revision`、`skills`。
- 技能项只允许 `oid`、`name`、`startFrame`。
- `schemaVersion=1`，`oid` 为 `0..999` 整数，`startFrame` 为 `0..599` 整数。
- `name` 为 1–256 UTF-8 字节，拒绝 NUL 和控制字符。
- 技能顺序是 UI 顺序；允许重复名称和重复起始帧。
- 不保存绝对路径、rootId、documentId、sessionId、BMP 路径或机器信息。
- 文件上限 256 KiB，技能数量上限 1000。
- 文件不存在的 GET 返回空状态，不自动写盘。
- 非法 JSON、非法 UTF-8、未知字段或版本返回 422，不自动重置。

推荐 API：

```text
GET  /api/project/skills
POST /api/project/skills
```

GET 返回：

```text
schemaVersion, revision, etag, skills
```

POST 精确请求字段：

```text
expectedRevision, expectedEtag, skills
```

服务器负责生成新 revision/etag。写入必须经过精确 Host、Origin、token、固定 root、目标锁、fingerprint compare-and-swap 和安全目录创建。sidecar revision 与 DAT session revision 完全独立。

技能管理继续复用同一 schema 和一次 CAS：

- 复制项插入原项之后，名称追加“副本”。
- 删除必须确认；上移/下移只交换相邻项。
- 查询和 mutation 必须按当前 OID 隔离，不能编辑其他 OID 的 sidecar 项。
- 成功后选择复制项、移动项或删除位置的相邻项。

## 5. Capability 定位合同

服务端已有定位字段，客户端不得压缩为 `frameId:key`：

```text
frameId
frameOccurrence
blockType
blockIndex
key
occurrence
fieldId
```

选择键：

- 帧：`frameId + frameOccurrence`
- 块：`frameId + frameOccurrence + blockType + blockIndex`
- 字段：使用服务器签发的 `fieldId`

Projection 默认值没有 capability 时必须显示为只读“DAT 未编写/默认值”，不得生成可提交输入框。

## 6. DAT 块检查器合同

右侧检查器使用以下分组：

### 帧基础

`pic`、`state`、`wait`、`next`、`dvx`、`dvy`、`dvz`、`centerx`、`centery`、全部 `hit_*`、`mp`、`vaction`。

### `bdy`

几何：`x/y/w/h`。

### `itr`

几何：`x/y/w/h/zwidth`；数值：`dvx/dvy/fall/bdefend/injury/arest/vrest/effect/attacking/respond/pickingact/pickedact/throwvx/throwvy/throwvz/throwinjury`。

成对字段：

- `catchingact: [valueA, valueB]`
- `caughtact: [valueA, valueB]`

两项必须一次提交、一次校验、一次写入同一原始字段跨度。不得把 pair 的第二个值当成独立 DAT key。

### `opoint`

`kind/x/y/action/dvx/dvy/oid/facing`。

### `wpoint`

`kind/x/y/attacking/cover/weaponact/dvx/dvy/dvz`。

### `bpoint`

`x/y`。

### `cpoint`

`kind/x/y/injury/cover/vaction/aaction/jaction/daction/taction/throwvx/throwvy/throwvz/throwinjury/hurtable/decrease/dircontrol/fronthurtact/backhurtact`。

`sound` 等资源路径第一阶段只显示安全状态，不向浏览器返回绝对路径。

## 7. 技能流程合同

从用户技能的 `startFrame` 遍历有效 DAT 帧：

- 正数且目标存在：普通边。
- `0`：原始保持值，不伪装成跳转到帧 0。
- 负数：保留原始值，并显示带原始值标签的特殊边。
- `999`、越界、不存在目标：未解析目标节点。
- `next` 和每个 `hit_*` 均保留原始键名。
- 自环、循环和多分支必须保留。
- 同一 frame ID 的运行有效定义使用最后 occurrence；被遮蔽 occurrence 仍可只读查看。
- 选择流程节点同步 frame、Native preview、检查器和时间轴。

## 8. 几何叠加合同

只使用 Native Tick 实体位置、方向、camera、frame center、sprite range 尺寸和 DAT 原始块几何。

当前精灵定位：

```text
anchorX = xInt + renderOffsetX - cameraX
anchorY = zInt + yInt
```

精灵左上角：

```text
facing=right: left = anchorX - centerx
facing=left:  left = anchorX - (spriteWidth - centerx)
top = anchorY - centery
```

局部点：

```text
facing=right: screenX = left + x
facing=left:  screenX = left + spriteWidth - x
screenY = top + y
```

矩形端点分别变换后绘制。负 `w/h` 保留原始值，不静默 clamp。

固定图层：

| 类型 | 图形 | 颜色 |
|---|---|---|
| `itr` | 矩形 | 红橙 |
| `bdy` | 矩形 | 青蓝 |
| `opoint` | 十字点 | 金色 |
| `wpoint` | 十字点 | 紫色 |
| `bpoint` | 十字点 | 绿色 |
| `cpoint` | 十字点 | 洋红 |

颜色、十字尺寸和 hit-test 半径是 UI 合同，不是 NTSD 运行规则。

禁止从字段推断命中、生成、抓取、武器最终位置、投掷轨迹或 3D 体积。

## 9. 按钮和响应式合同

按钮状态优先级：

```text
loading > disabled > selected > active > hover > default
```

`focus-visible` 与上述状态正交。

- 播放时显示暂停、`aria-pressed=true`；无循环到末端自动停止。
- 请求期间相关按钮 disabled，显示 `aria-busy` 和动作文本。
- 叠加按钮使用 `aria-pressed`，并显示类型和数量。
- 应用修改只在有 draft 且 capability 有效时可用。
- 覆盖 DAT 仅在 `serverDirty=true` 时可用，使用 danger 样式和确认。

三档布局：

- 1440×900：顶部、左 260–300、中间、右 300–340、底部时间轴。
- 1024×768：左右栏可收起抽屉，中间预览保持主区。
- 390×844：技能、预览、属性、时间轴四个标签页，无水平溢出。

## 10. Phase 6 事务与可视化合同

### Batch 字段编辑

- 单请求包含 1–16 个唯一 `fieldId`，只接受服务签发的 scalar 或 pair capability。
- Canvas move 原子提交 x/y；resize 原子提交 x/y/w/h；pair 在一个原始 value span 中提交两个 int32。
- no-op 不增加 revision；任一字段非法、冲突、preview/view 失败时整批回滚。
- edit busy 与未应用 draft/Canvas interaction 期间锁定 frame、block、skill、Flow、保存和结构操作。

### Lossless 结构事务

- frame 复制使用完整闭合 frame span，只重写副本 header 的 frame ID。
- block 新建/复制均使用当前同类完整闭合 span；删除移除完整 span。
- 不创建空白默认字段，不修复 `next`、`hit_*`、技能起始帧或其他引用。
- 每次结构事务只增加一个 revision，并重新签发全部 field/structure capability；旧 capability 立即失效。
- field + structure capability 共用 50,000 总限额；超限、非法 span、revision 冲突和 preview/view 失败无部分发布。
- 显式安全保存后沿用恢复备份、hash 和服务重启恢复合同。

### Canvas / Flow / 时间轴

- Canvas 默认 1px，可切换 4px 网格；方向键 ±1，Shift+方向键 ±4，Esc 取消且不增加 revision。
- 矩形四角 resize 要求最终 w/h 为正；镜像方向使用纯函数逆变换。
- SVG 仅把真实已有 `next`/`hit_*` capability 标为可编辑；目标只能选择已有 frame。
- 写 `0` 仍是写入原始值，不等同于删除边；循环、分支和 unresolved 保持可见。
- 时间轴宽度只表达 `max(1, wait)` 的 DAT wait 视觉比例，不标记为秒或 Native tick。

## 11. 阶段实施顺序

### Phase 2：合同确认

- sidecar schema/API/error/CAS 合同。
- 完整 capability locator 和 pair capability 合同。
- skill flow edge 合同。
- overlay geometry 纯函数合同。
- AppState 和按钮状态合同。

### Phase 3：最小垂直闭环

- 临时 sidecar 新建技能。
- 技能选择与真实 flow。
- 节点/帧/检查器联动。
- 单角色预览控制和一个 `bdy` overlay。
- 一个块字段编辑、dirty、保存边界。

### Phase 4：能力扩展

- 六类块检查器和 overlays。
- pair 编辑。
- 自适应三档。
- 侧车恢复、冲突、错误和恢复。

### Phase 5：稳定性

- 并发、损坏 sidecar、外部变化、资源失败、长列表、性能和安全审查。
- 主实体使用 Native slot 0，拒绝非法和重复 slot。
- 草稿跨导航保留；skill/edit/save 使用独占 busy 状态。
- Preview 单飞且只保留最后 pending 请求。
- 隔离 DAT 覆盖、恢复备份和服务重启达到 E5。

### Phase 6：可视化创作

- 已完成技能删除、复制、排序及 OID 隔离。
- 已完成 frame/block 的 lossless 新建、删除、复制及 commit 前 preview 回滚。
- 已完成 Canvas 几何拖拽、缩放、网格吸附、键盘微调和 Esc。
- 已完成 SVG Flow 已有边重定向和按 `wait` 展开的视觉时间轴。
- release build `20260806172742780-4037ab3a29ef4617ba7386f804ae3c1b` 达到 E4/E5。

## 12. 阶段停止条件

- 发现字段含义只能靠猜测。
- 需要修改 `ntsd_cpp` 权威逻辑但没有批准。
- sidecar 写入无法通过 native workspace 安全边界。
- 无法取得 E3/E4 证据时，不得标记用户可见功能完成。
