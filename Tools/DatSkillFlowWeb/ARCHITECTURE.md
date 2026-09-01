# NTSD DAT 技能流程编辑器架构与接口合同

## 1. 目标与边界

本架构服务于 Standard 模式当前已验证范围（Phase 1–6），不创造 NTSD 战斗规则。

- DAT 原始字段和重复顺序由 lossless DAT/CST parser 负责。
- `ntsd_cpp` 是单角色 Native preview 的运行权威。
- 浏览器只持有 opaque session、field、structure 和 asset capability。
- 状态/技能入口和首帧由 DAT frame 标题与跳转关系派生；侧车只保存显示元数据，不写入 DAT。
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
├── PanelLayout
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

### 启动准备与即时切换

- CLI 在发布 loopback listener 前创建默认 OID 2 会话，从当前 DAT 自动入口派生全部 preview scenario，并以最多 4 个 Native 任务并行预热。控制台必须输出进度和最终成功数。
- Native 原始结果按 `sha256(root plaintext + package catalog OID/type/plaintext bytes) + startFrame + initialFrame + inputPlan + ticks` 有界缓存；同 key 的并发请求共享同一个 Promise，失败结果必须移除。
- 会话结果再按 `session revision + 完整 preview intent` 有界缓存；DAT 编辑产生新 revision 时清除旧 revision 结果。客户端只缓存已完成 JSON，重复选择直接提交，不显示等待态。
- Native CLI 的 `render_resources` 是预览实体 frame/type/center/sprite range 的直接来源。只有旧版或测试 runner 未提供对应 OID 时，服务才回退到 catalog/DAT projection。
- Native CLI 必须像正式 LoadingScene 一样遍历运行数据根 `J:\QQFile\NTSD 2.4.1\data\data.txt` 的完整 `<object>` 段并按条目原始 `type` 注册 DAT；不得用手写 OID 子集或把 type 3/4 折叠成武器枚举。完整 catalog 只属于 Native 运行时，`render_resources` 仅输出本次 Trace 实际出现的 OID，避免扩大 API 响应。
- 补丁包会话必须将同一 package 的完整 OID/type/DAT 目录传给 Native CLI；目录以 OID 覆盖基础 `data.txt`，但不得引入其他 package 的同 OID 对象。服务端先在授权根内读取/解密，再以短生命纯文本目录传入 CLI，不向 Native 暴露未授权路径。
- DAT 只有在解密/读取与 C++ 解析全部成功后才能提交到 `GameWorld` OID 目录；失败加载不得留下默认 `CharData` 导致 opoint 生成 OID 0 幽灵实体。
- `J:\QQFile\NTSD2.4\ntsd_cpp` 只提供 C++ 源码、头文件和链接对象。Native `--game-root` 与一键启动的 `asset-workspace` 必须统一为 `J:\QQFile\NTSD 2.4.1`，禁止从 C++ 工作目录的 `..\data`/`..\chars` 隐式加载另一版本。
- 项目打开阶段只为安全、可归一化的 BMP 路径签发 opaque asset capability，不做逐图同步文件读取。首次 asset GET 在已授权 root 内执行 handle-safe 读取，并把字节限制在当前 session；关闭 session 后 capability 与缓存同时失效。
- 启动预热会解析所有 Native scenario 的 render resource 和 stage path，并在控制台阶段读取默认会话所需资源。这样页面打开只传输准备好的 session，技能切换只做本地/内存命中和 localhost JSON 传输。
- `data.txt` 新鲜度检查属于显式 catalog/open 边界，不属于已有 session 的 preview/edit/save 热路径；DAT 保存仍由 document fingerprint/CAS 保护。

### Native Trace DTO boundary

- C++ CLI 仍只负责输出权威逐 tick entity snapshot；网页不修改 CLI，也不把网页输入伪装成 C++ 输入。
- 服务端使用 `data.txt` catalog 的 `type` 复刻 `rawObjectType == 0` 角色、非 0 武器/投射物的分类，并通过对应 OID DAT projection 加载 frame/range/BMP capability。
- `NativePreviewEntityView` 追加 `kind`、`objectType`、`lineageId`、首末 tick 和资源可用性；`NativePreviewTraceView` 记录 root 结束、播放尾迹边界、spawn/despawn、分身释放、投射物落地和 `timeout/persistent`。
- slot 只用于当前快照定位；同一 slot 的 OID 变化先结束旧 lineage，再创建新 lineage，避免对象池复用混淆。
- root 主体结束只停止主体进度；播放边界继续覆盖尚未完成的投射物。分身只在首个有效快照确认，不等待 AI 生命周期。

### 主预览 presentation sampling

- 主编辑器的 Native Trace 仍固定为 30Hz 离散 Tick；`30 / 60 / 120Hz` 只控制浏览器 presentation sampling，不生成新逻辑 Tick，也不修改 trace。
- 60/120Hz 以 previous/current 相邻 Tick 为输入，current Tick 继续拥有 frame、pic、facing、spawn/despawn、hit、opoint、DAT wait 与生命周期；只计算 camera 和实体显示位置。
- 实体必须保持同 lineage、同 holder/link/target 关系，且精确 `x/y/z` 位移不超过 `max(64, max(abs(previousVelocity), abs(currentVelocity)) * 4 + 4)`，否则 fail closed 到 current Tick 离散位置。
- 插值 delta 从精确 `x/y/z` 计算，并统一应用到同一 presentation entity；shadow 与 sprite 读取同一个 sampled Tick。不得分别从最终像素、精灵轮廓或背景估算位置。
- DAT overlay、坐标轴、站位拖动、几何编辑和 hit-test 使用独立的 authority Tick。表现插值不能改变或伪装碰撞/编辑真值；暂停和编辑状态不使用中间位置。
- 主预览每个表现时刻只通过一次 `requestAnimationFrame` 请求绘制完整 Canvas；不先显示 current Tick 再覆盖中间帧。

### Browser ownership

| 模块 | 职责 | 不拥有 |
|---|---|---|
| `AppState` | 统一保存 bootstrap、catalog、skill、session、selection、playback、preview、overlay、inspector、persistence 和 layout 状态 | fetch、Canvas 或 DAT 语义 |
| `ProjectApi` | 统一请求、状态 token、响应错误、请求 epoch | DOM 和本地业务状态 |
| `SkillMetadataClient` | 读取/写入入口别名、分组、排序、置顶、隐藏和备注 | 创建入口、DAT 文件和 Native preview |
| `SkillEntries` / `SkillFlowGraph` / `FlowSvg` / `SkillTimeline` | 从 DAT 标题段和跳转关系派生入口；从首帧遍历真实边并按 DAT wait 展开 | 脱离 DAT 的技能语义、运行结果和时间单位 |
| `PlaybackController` | play/pause/step/seek/loop、末端停止和 timer | 修改 DAT |
| `PreviewRenderer` | DPR、viewport、BMP、sprite、camera、镜像和预览状态 | 逻辑实体真值 |
| `OverlayGeometry` | 六类块的纯坐标变换和 hit-test | 命中、生成、抓取和武器结果 |
| `CanvasGeometryEdit` | capability 约束下的 move/resize、镜像逆变换、snap、键盘和 Esc 草稿 | 创建缺失字段或提交部分几何 |
| `PanelLayout` | 纯计算左右栏 min/max、默认宽度、中栏预算、drag delta 和 viewport clamp | DOM 事件、持久化和 DAT 状态 |
| `InspectorModel` | 完整 capability 定位、分组、draft、校验和提交 | 投影默认值伪造 capability |
| `EditorShell` | 五区布局、按钮状态、桌面 separator、移动标签页和 live region | 保存业务真值 |

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
      "startFrame": 300,
      "displayName": "用户维护名称",
      "group": "输入技能",
      "order": 10,
      "pinned": true,
      "hidden": false,
      "notes": "只影响编辑器显示"
    }
  ]
}
```

合同：

- 顶层只允许 `schemaVersion`、`revision`、`skills`。
- 技能项只允许 `oid`、`startFrame`、`displayName`、`group`、`order`、`pinned`、`hidden`、`notes`。
- `schemaVersion=1`，`oid` 为 `0..999` 整数，`startFrame` 为 `0..599` 整数。
- `displayName`/`group`/`notes` 有独立 UTF-8 字节上限，拒绝 NUL 和控制字符；布尔值与整数必须保持精确类型。
- `(oid,startFrame)` 只覆盖 DAT 已派生入口的表现；不能凭 sidecar 创建一个 DAT 不存在的入口。
- 旧版 `name` 可读为 `displayName`；下一次保存写回新字段。GET 返回 `missing|valid|legacy|invalid` 状态。
- 不保存绝对路径、rootId、documentId、sessionId、BMP 路径或机器信息。
- 文件上限 256 KiB，技能数量上限 1000。
- 文件不存在时 GET 返回 `missing` 和空展示信息，不自动写盘。
- 非法 JSON、非法 UTF-8、未知字段或版本时 GET 返回 `invalid` 和空展示信息，DAT 自动入口继续可用；不得自动覆盖损坏文件。

推荐 API：

```text
GET  /api/project/skills
POST /api/project/skills
```

GET 返回：

```text
schemaVersion, revision, etag, sidecarStatus, skills
```

POST 精确请求字段：

```text
expectedRevision, expectedEtag, skills
```

服务器负责生成新 revision/etag。写入必须经过精确 Host、Origin、token、固定 root、目标锁、fingerprint compare-and-swap 和安全目录创建。sidecar revision 与 DAT session revision 完全独立。

展示信息管理继续复用同一 schema 和一次 CAS。客户端按 `(oid,startFrame)` 更新或删除展示覆盖，空覆盖不持久化；查询和 mutation 必须按当前 OID 隔离。sidecar 状态为 `invalid` 时禁用保存，避免把损坏文件静默覆盖。

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

底层精确入口先从每个 frame ID 的最后 occurrence 自动派生：

- frame ID 连续且标题相同的记录合并为一个标题段；相同标题的非连续段保持独立。
- 每个非零 `hit_*` 的有效目标是精确入口，目标帧即底层首帧，并汇总所有来源 frame/字段。
- 无普通 `next` 前驱的标题段保持为独立动作入口。
- sidecar 只可覆盖显示、排序、置顶和隐藏，不创建或改变 DAT 关系。

正式左侧在底层精确入口上增加完整动作归属层：

- state 0/1/2 的入口按 standing/walking/running 基础上下文聚合，保留所有变体 Frame 和可发起动作统计。
- 非基础入口先保留为动作根；如果它的全部有效来源都位于其他动作的 `next` 链内，且没有基础状态直达或未归属的外部来源，则降为内部阶段。
- 内部阶段沿自身 `next` 链继续归属父动作；嵌套内部输入递归传播完整动作根。
- 同一内部阶段可同时归属多个完整动作，不任意选择唯一父级。
- 只要目标还存在一条基础状态直达路线，就保持独立完整动作，不因其他内部来源而被吞并。
- 标题只用于显示和连续段识别，不作为内部融合的唯一证据。
- “全部 Frame”继续显示被覆盖 occurrence、运行时分支、精确 `hit_*` 来源和完整动作归属。

从所选入口的 `startFrame` 遍历有效 DAT 帧：

- 正数且目标存在：普通边。
- `0`：原始保持值，不伪装成跳转到帧 0。
- 负数：保留原始值，并显示带原始值标签的特殊边。
- `999`、越界、不存在目标：未解析目标节点。
- `next` 和每个 `hit_*` 均保留原始键名。
- `next` 正常展开；指向另一入口的 `hit_*` 产生可点击入口叶节点，不在当前流程继续展开目标技能。
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

- 1440×900：两条 6px 竖向 separator；左栏默认 286px、右栏默认 330px，中栏使用剩余宽度且至少 420px。
- 1024×768：左栏默认 230px、右栏默认 286px；左右栏均可拖动，中栏至少 360px。
- 390×844：技能、预览、属性、时间轴四个标签页，无水平溢出。

桌面 separator 合同：

- 左栏范围 200–420px，右栏范围 240–460px；拖动一侧优先保持另一侧，剩余预算不足时由 `PanelLayout` 重新 clamp。
- separator 使用 pointer capture；`pointerup` 完成，`pointercancel` 恢复，丢失 capture 清理；方向键每次 8px，Shift 每次 32px，拖动中 Esc 恢复本次起点。
- `ResizeObserver` 在容器变化时结束进行中的拖动并重新 clamp；≤850px 隐藏 separator，由移动标签页接管。
- separator 暴露 vertical `role=separator`、`aria-controls`、动态 min/max/now 和宽度 valuetext。
- 宽度仅属于当前页面布局状态，不写 localStorage、sidecar 或 DAT。

## 10. 启动模式合同

- 无参数的一键启动必须在 ConsoleHost 中选择 `Project`、`Test` 或取消，不静默选择 workspace。
- 非交互调用必须显式传入 `-Mode Project` 或 `-Mode Test`。
- `Project` 将仓库根目录作为 workspace；真实 DAT 位于 `Assets/NTSD/Config`，技能 sidecar 位于根目录 `.dat-skill-flow/skills.json`。
- `Test` 将 `%LOCALAPPDATA%\DatSkillFlowWeb\test-workspace` 作为 workspace；仅当副本不存在时初始化，或在显式 `-ResetWorkspace` 时重建。
- `-ResetWorkspace` 只能用于 `Test`，正式模式分支不得调用 `Copy-Item`、`Remove-Item` 或测试初始化函数。
- 两种模式都不创建演示技能，继续使用随机 loopback 端口、Host/Origin/token、opaque capability 和安全覆盖备份协议。
- 启动输出必须明确模式、可写 workspace、data.txt 和技能 sidecar；正式模式额外警告确认保存会写真实仓库 DAT。
- 含中文提示的 PowerShell 脚本固定使用 UTF-8 BOM，保证 Windows PowerShell 5.1 解析一致。

## 11. Phase 6 事务与可视化合同

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

## 12. 阶段实施顺序

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

- 历史版本已完成手工技能删除、复制、排序及 OID 隔离；REQ-017 现以 DAT 自动入口和纯展示 sidecar 取代手工技能实体。
- 已完成 frame/block 的 lossless 新建、删除、复制及 commit 前 preview 回滚。
- 已完成 Canvas 几何拖拽、缩放、网格吸附、键盘微调和 Esc。
- 已完成 SVG Flow 已有边重定向和按 `wait` 展开的视觉时间轴。
- release build `20260806172742780-4037ab3a29ef4617ba7386f804ae3c1b` 达到 E4/E5。

### Phase 7：DAT 自动入口

- frame 标题经 CST/project DTO lossless 投影。
- 入口由标题段、有效非零 `hit_*` 目标及普通 `next` 前驱关系自动派生。
- 跨技能 `hit_*` 作为可点击叶节点，不吞并目标技能后续流程。
- sidecar 仅保存 `(oid,startFrame)` 对应的显示覆盖，旧 `name` 可读迁移。

## 13. Native 技能 Trace 合同

Native Trace 从“技能已成功触发”的语义开始，不模拟 UI 键盘事件，也不把 DAT `wait` 当作 Native tick。

### Root 主体结束

每个 Trace 固定记录 slot 0 的 root actor。`actorSkillEnded` 只有在以下条件同时满足时成立：

- root 仍然 `active`；
- `hp > 0`；
- 当前 frame 可在 root DAT 中解析；
- frame `state == 0`；
- root 在地面（Native `y_int == 0`）。

该事件只控制网页播放进度和主体技能状态，不代表所有派生对象已经完成。

### 派生对象分类

网页服务必须根据 Native entity 的 `oid` 查找对应 DAT/catalog，并使用 C++ 对齐的 `rawObjectType` 映射出类别；不得根据 OID 名称或前端经验猜测：

- `rawObjectType == 0`：DAT 角色/分身。Trace 记录 `opointSpawned` 和首个有效世界快照，然后将该对象标记为 `aiDelegated`，不等待其 AI 后续生命周期。
- `rawObjectType != 0`：武器或投射物。Trace 继续记录其逐逻辑 tick 世界快照、位置、速度、frame、state、碰撞和失效变化。

该分类来自 `ntsd_cpp` 的 `CharData::obj_type`/`Entity::entity_type` 对应的 DAT catalog 投影；网页侧复刻该数据结构和逻辑，不修改 C++ CLI 游戏规则。

### 投射物完成

投射物只有在 Native 路径确认完成后才可从 tracked projectile 集合移除，包括：

- 对应武器物理的落地/停止或 bounce 后进入最终停止 frame；
- 地面碰撞或其他权威碰撞导致对象失效；
- `active=false`、`state=9998` 或权威释放路径。

主体回 idle 不能截断投射物 Trace。若持续投射物在最大 tick 上限内没有完成，Trace 必须显式使用 `timeout` 或 `persistent` 结束原因。

### Trace DTO 与时间轴

每个 Native tick 记录完整 active entity 快照，而不是只记录摘要 digest。每个实体至少包含 `slot`、稳定 lineage ID、`oid`、OID 对应 `rawObjectType`、`frame`、`state`、`active`、整数坐标、速度、朝向和必要 link/holder 字段。

slot 只能作为当前 tick 定位；lineage ID 必须在 slot 释放和复用后保持不混淆。Trace 需要记录 root、直接 opoint child 和递归投射物派生关系，以及 `actorSkillEnded`、`opointSpawned`、`projectileCompleted`、`traceComplete` 事件。

网页进度条使用 Native root actor tick 作为结束基准；DAT wait 轴只作视觉比例标注，不能作为真实播放时钟。

## 14. 阶段停止条件

- 发现字段含义只能靠猜测。
- 需要修改 `ntsd_cpp` 权威逻辑但没有批准。
- sidecar 写入无法通过 native workspace 安全边界。
- 无法取得 E3/E4 证据时，不得标记用户可见功能完成。
