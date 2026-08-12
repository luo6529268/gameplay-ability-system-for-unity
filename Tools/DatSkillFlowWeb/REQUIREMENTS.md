# Requirements

## Status Vocabulary

`Proposed` · `Confirmed` · `In Progress` · `Validated` · `Deferred` · `Rejected` · `Blocked`

## Requirement Register

| ID | Requirement | Source | Evidence | Priority | Status | Validation |
|---|---|---|---|---|---|---|
| REQ-001 | 专业技能编辑器四区布局 | User / Visual brief | Confirmed | P0 | Validated | VAL-001 |
| REQ-002 | 技能入口显示侧车元数据 | User / 2026-08-07 clarification | Confirmed | P0 | Validated | VAL-002 |
| REQ-003 | 当前技能真实 DAT 帧流程 | User / Product goal | Confirmed | P0 | Validated | VAL-003 |
| REQ-004 | 单角色高质量场景预览 | User | Confirmed | P0 | Validated | VAL-004 |
| REQ-005 | 全部现有 DAT 块的几何叠加 | User | Confirmed | P0 | Validated | VAL-005 |
| REQ-006 | 全部现有 DAT 块的结构化查看与编辑 | User | Confirmed | P0 | Validated | VAL-006 |
| REQ-007 | 明确的按钮与操作反馈状态 | User | Confirmed | P0 | Validated | VAL-007 |
| REQ-008 | 会话修改与 DAT 覆盖清晰分离 | User / Existing safety contract | Confirmed | P0 | Validated | VAL-008 |
| REQ-009 | 桌面、中屏、窄屏自适应 | Template / Recommended default | User-authorized default | P1 | Validated | VAL-009 |
| REQ-010 | 用户可见功能自行运行到 E4 证据 | Template | Confirmed | P0 | Validated | VAL-010 |
| REQ-011 | DAT 与 Native preview 权威行为不被 UI 改写 | Project rules | Confirmed | P0 | Validated | VAL-011 |
| REQ-012 | 手工技能复制、删除与排序 | User / 2026-08-06 Phase 6 confirmation | Superseded by REQ-017 | P1 | Superseded | VAL-012 |
| REQ-013 | 模板式 lossless frame/block 结构编辑 | User / 2026-08-06 Phase 6 confirmation | Confirmed | P0 | Validated | VAL-013 |
| REQ-014 | Canvas 几何直接编辑 | User / 2026-08-06 Phase 6 confirmation | Confirmed | P1 | Validated | VAL-014 |
| REQ-015 | 可视化 Flow 与 DAT wait 视觉时间轴 | User / 2026-08-06 Phase 6 confirmation | Confirmed | P1 | Validated | VAL-015 |
| REQ-016 | 一键启动选择正式项目或测试副本 | User / 2026-08-07 clarification | Confirmed | P0 | Validated | VAL-016 |
| REQ-017 | DAT 自动状态/技能入口与跨技能链接 | User / 2026-08-07 design confirmation | Confirmed | P0 | Validated | VAL-017 |
| REQ-018 | 桌面三栏可拖动调宽 | User / 2026-08-07 explicit request | Confirmed | P1 | Validated | VAL-018 |
| REQ-019 | Native 技能 Trace 按主体、分身和投射物分类运行 | User / 2026-08-07 clarification | Confirmed | P0 | Planned | VAL-019 |
| REQ-020 | Canvas 拖动设置 P1/P2 Native 起始站位 | User / 2026-08-11 | Confirmed | P0 | Implemented | VAL-024 |

## Requirement Detail

### REQ-001: 专业技能编辑器四区布局

- Statement: 页面使用顶部状态工具栏、左侧技能与帧流、中间预览、右侧检查器和底部时间轴组成的专业编辑器布局。
- Rationale: 当前单画布加狭窄右栏的信息架构无法清晰表达技能、帧、预览和编辑状态。
- Source: 用户原始描述与 GPT 视觉方向稿评审。
- Evidence: `Confirmed`
- Preconditions: 当前项目已成功载入。
- Inputs: 项目、技能、帧、预览 Tick、字段能力。
- Outputs: 清晰分区的编辑器工作区。
- Invariants: 不显示未接线的装饰性功能。
- Dependencies: REQ-002、REQ-004、REQ-006。
- Acceptance: 用户能在一个视图中定位当前角色、技能、帧、预览、属性和时间轴。
- Validation ID: VAL-001
- Status: Validated

### REQ-002: 技能入口显示侧车元数据

- Statement: DAT 决定入口是否存在及其首帧；项目根目录 `.dat-skill-flow/skills.json` 只保存显示名称、分组、顺序、置顶、隐藏和备注。
- Rationale: 中文别名和编辑器分组不是 DAT 权威字段，不能污染 DAT；sidecar 缺失或无效也不能使真实 DAT 入口消失。
- Source: 用户于 2026-08-07 明确确认调整后的 sidecar 作用。
- Evidence: `Confirmed`
- Preconditions: 项目工作区已授权。
- Inputs: OID、由 DAT 识别的首帧及可选显示信息。
- Outputs: 可恢复、可共享的纯展示元数据。
- Invariants: 侧车文件不得包含绝对路径；不得创建 DAT 不存在的入口；不得改变 DAT 字节、state、next 或 `hit_*`。
- Dependencies: 安全工作区与安全保存合同。
- Acceptance: 修改显示信息后重启服务仍能恢复且 DAT 指纹不变；删除、缺失或损坏 sidecar 时自动入口仍由 DAT 恢复。
- Validation ID: VAL-002
- Status: Validated

### REQ-003: 当前技能真实 DAT 帧流程

- Statement: 从 DAT 自动入口首帧出发，使用真实 `next` 展开当前流程；`hit_*` 指向其他入口时显示可点击的跨技能目标，不继续吞并目标技能流程。
- Rationale: 技能编辑器必须表达帧流程而不是只列出全部 DAT 帧。
- Source: 用户确认的技能模型与产品目标。
- Evidence: `Confirmed`
- Preconditions: 当前 DAT 已识别至少一个入口首帧。
- Inputs: DAT 帧、`next`、`hit_a`、`hit_d`、`hit_j`、组合输入跳转。
- Outputs: 可选择的技能帧关系视图和时间轴标记。
- Invariants: 保留循环和分支；技能首帧为 `hit_*` 跳转后的真实目标帧；未知跳转仍保持 unresolved。
- Dependencies: REQ-002。
- Acceptance: 选择流程节点会定位预览和检查器到同一 DAT 帧。
- Validation ID: VAL-003
- Status: Validated

### REQ-004: 单角色高质量场景预览

- Statement: 预览使用真实 BMP 与 `ntsd_cpp` Tick，支持播放、暂停、单步、循环、缩放、适应窗口、网格、坐标和方向状态。
- Rationale: 当前画布空间利用差，角色与状态不清晰。
- Source: 用户确认。
- Evidence: `Confirmed`
- Preconditions: Naruto OID 2 会话与 Native preview 可用。
- Inputs: Native Tick、sprite range、BMP capability。
- Outputs: 清晰可见的单角色 2D 预览。
- Invariants: 不生成高清替代图；不把表现状态写回 DAT 或运行时。
- Dependencies: `ntsd_cpp`、BMP 资源。
- Acceptance: 当前角色、帧、朝向和播放状态在预览中可辨识。
- Validation ID: VAL-004
- Status: Validated

### REQ-005: 全部现有 DAT 块的几何叠加

- Statement: 当前帧的 `bdy`、`itr` 显示矩形，`opoint`、`wpoint`、`bpoint`、`cpoint` 显示定位点，并支持分层开关和选择。
- Rationale: 技能预览必须能解释碰撞、生成、武器和连接数据的位置。
- Source: 用户确认。
- Evidence: `Confirmed`
- Preconditions: 当前帧投影包含对应块。
- Inputs: 当前帧 block DTO 和精灵坐标合同。
- Outputs: 使用稳定颜色与图例的预览叠加。
- Invariants: 只展示 DAT 中的几何信息；不推断命中、生成或抓取结果。
- Dependencies: REQ-004、REQ-006。
- Acceptance: 切换每个叠加层只影响显示，不改变 DAT；选择叠加项会选中对应检查器块。
- Validation ID: VAL-005
- Status: Validated

### REQ-006: 全部现有 DAT 块的结构化查看与编辑

- Statement: 检查器支持帧基础字段及 `itr`、`bdy`、`opoint`、`wpoint`、`bpoint`、`cpoint` 现有字段的分组查看和 capability 编辑。
- Pair contract: `itr.catchingact` 和 `itr.caughtact` 使用两个值的原子成对编辑，不拆成两个独立 DAT 字段。
- Rationale: 只有基础标量字段不足以成为技能编辑器。
- Source: 用户确认。
- Evidence: `Confirmed`
- Preconditions: 服务器为对应字段签发编辑 capability。
- Inputs: 当前帧、block DTO、字段 capability。
- Outputs: 会话内无损修改。
- Invariants: 只编辑已签发字段；保留原始 DAT 键名和重复块顺序。
- Dependencies: DatSessionService。
- Acceptance: 修改 block 字段后叠加位置同步变化，修订版本递增且 `dirty=true`。
- Validation ID: VAL-006
- Status: Validated

### REQ-007: 明确的按钮与操作反馈状态

- Statement: 所有交互控件具备可区分的默认、悬停、按下、选中、禁用、焦点和加载状态。
- Rationale: 当前按钮反馈不明显，用户无法确认操作是否生效。
- Source: 用户原始描述。
- Evidence: `Confirmed`
- Preconditions: 无。
- Inputs: 用户指针、键盘和异步操作状态。
- Outputs: 视觉与文本反馈。
- Invariants: 颜色不是唯一状态线索；危险操作与普通操作不共用样式。
- Dependencies: REQ-001。
- Acceptance: 自动交互截图可观察到播放选中、加载禁用、按下和未保存状态。
- Validation ID: VAL-007
- Status: Validated

### REQ-008: 会话修改与 DAT 覆盖清晰分离

- Statement: 页面明确区分应用会话修改与覆盖 DAT 文件，并对未保存修改提供离开保护。
- Rationale: DAT 覆盖是高影响操作，不能与普通字段编辑混淆。
- Source: 用户视觉目标与现有安全合同。
- Evidence: `Confirmed`
- Preconditions: 项目会话已打开。
- Inputs: revision、dirty、save response。
- Outputs: 状态标签、保存按钮和确认提示。
- Invariants: 只有显式保存请求可以覆盖 DAT。
- Dependencies: ProjectDatService safe save。
- Acceptance: 编辑后显示未保存状态；保存成功后恢复已保存状态；切换技能不关闭同一项目会话。
- Validation ID: VAL-008
- Status: Validated

### REQ-009: 自适应布局

- Statement: 宽屏使用完整四区布局，中屏可收起侧栏，窄屏使用技能、预览、属性和时间轴标签页。
- Rationale: 简单纵向堆叠会使编辑器在窄屏不可用。
- Source: 模板与用户授权视觉方向。
- Evidence: `User-authorized default`
- Preconditions: 无。
- Inputs: viewport 尺寸。
- Outputs: 无水平溢出的可操作布局。
- Invariants: 核心操作不可因尺寸变化消失。
- Dependencies: REQ-001。
- Acceptance: 三个目标 viewport 均能完成选择技能、播放、编辑一个字段。
- Validation ID: VAL-009
- Status: Validated

### REQ-010: 自行运行验证

- Statement: 每个用户可见阶段由模型自行构建、启动并取得真实渲染和关键交互证据，至少达到 E4。
- Rationale: 用户不是基础运行测试员。
- Source: Large Goal 模板。
- Evidence: `Confirmed`
- Preconditions: 隔离浏览器环境可用。
- Inputs: 当前构建、服务、自动化客户端。
- Outputs: 截图、交互日志、资源和控制台结果。
- Invariants: HTTP 成功不能替代 UI 通过。
- Dependencies: 测试环境。
- Acceptance: `ACCEPTANCE.md` 记录构建 ID、操作、结果和证据路径。
- Validation ID: VAL-010
- Status: Validated

### REQ-011: 权威边界

- Statement: UI 只展示和编辑真实 DAT，预览只读取 `ntsd_cpp` 输出，不创造 NTSD 战斗规则。
- Rationale: 项目要求严格保留权威行为。
- Source: 项目规则。
- Evidence: `Confirmed`
- Preconditions: 无。
- Inputs: DAT、Native preview。
- Outputs: 编辑器 DTO 与表现。
- Invariants: 复杂运行语义缺少权威证据时标记待确认。
- Dependencies: 项目权威资料和 `ntsd_cpp`。
- Acceptance: 代码审查确认没有以 UI 推断替代权威运行结果。
- Validation ID: VAL-011
- Status: Validated

### REQ-012: 手工技能复制、删除与排序（已取代）

- Statement: 历史版本允许复制、删除选中技能，并将技能上移或下移一位。
- Rationale: 该合同基于 sidecar 定义技能实体；REQ-017 改为 DAT 自动定义入口后，sidecar 不再有权创建、复制或删除技能。
- Source: 用户于 2026-08-06 确认采用建议规则。
- Evidence: `Confirmed`
- Preconditions: 仅适用于 REQ-017 之前的历史版本。
- Inputs: 当前选中技能和 sidecar revision/etag。
- Outputs: 更新后的有序 `skills[]`。
- Invariants: 复制项插在原项后并追加“副本”；删除必须确认；操作后保持结果项或相邻项选中；不改变 sidecar schema 或 DAT 字节。
- Dependencies: REQ-002、REQ-008。
- Acceptance: 复制、删除、上移、下移分别只产生一次 sidecar CAS 保存；重启服务后顺序和名称恢复。
- Validation ID: VAL-012
- Status: Superseded by REQ-017

### REQ-013: 模板式 lossless frame/block 结构编辑

- Statement: 用户可复制当前完整 frame 并显式指定新 frame ID；可用当前同类 block 的完整字节作为模板新建或复制 block；可删除当前完整 frame/block。
- Rationale: 空白结构的默认字段没有权威合同，完整 span 模板能保留未知字节、格式和注释。
- Source: 用户于 2026-08-06 确认“模板式新建”。
- Evidence: `Confirmed`
- Preconditions: 当前结构具有完整、可安全复制或删除的 CST span；项目可写且 revision 最新。
- Inputs: 服务器签发的结构 capability、操作类型、新 frame ID（仅 frame 复制）。
- Outputs: 一次 revision 的 lossless 结构事务和重新签发的全部字段/结构 capability。
- Invariants: frame 复制仅修改副本 header 中的 frame ID；block 新建与复制都复制当前同类完整 span；删除不自动修复 `next`、`hit_*`、技能起始帧或其他引用；不生成空白默认字段；旧 capability 在结构事务后失效。
- Dependencies: REQ-006、REQ-008、REQ-011。
- Acceptance: 未知字段、注释、换行和非目标字节保持不变；失败、超限和 revision 冲突原子回滚；显式保存和重启后结构恢复。
- Validation ID: VAL-013
- Status: Validated

### REQ-014: Canvas 几何直接编辑

- Statement: 用户可在 Canvas 上移动现有点/矩形几何，使用矩形边角调整 x/y/w/h，并通过键盘微调。
- Rationale: 几何编辑应直接反馈到预览，而不是完全依赖检查器数字输入。
- Source: 用户于 2026-08-06 确认采用建议交互。
- Evidence: `Confirmed`
- Preconditions: 所需 x/y 或 x/y/w/h 字段已有服务器 capability；项目可写。
- Inputs: pointer drag、resize handle、Esc、方向键、Shift、4px 网格开关。
- Outputs: 拖动中的本地草稿几何，以及 pointerup 后一次原子 batch edit。
- Invariants: 默认 1px；4px 网格可切换；方向键 ±1、Shift+方向键 ±4；Esc 取消未提交交互；镜像方向使用经测试的逆变换；缺失 capability 时禁用对应操作；w/h 不得提交非正值。
- Dependencies: REQ-005、REQ-006、REQ-008。
- Acceptance: move 每次只增加一个 revision 并同时更新 x/y；resize 每次只增加一个 revision 并同时更新 x/y/w/h；冲突或失败不留下部分字段修改。
- Validation ID: VAL-014
- Status: Validated

### REQ-015: 可视化 Flow 与 DAT wait 视觉时间轴

- Statement: 当前技能以 SVG 节点和真实已有 `next`/`hit_*` 字段连线展示；用户只能将已有跳转字段重定向到已有 frame；时间轴按 `max(1, wait)` 展开并明确标记为 DAT wait 视觉比例。
- Rationale: 提升流程可读性，同时不把 DAT 值伪装成未确认的 Native tick 或秒数。
- Source: 用户于 2026-08-06 理解并确认采用安全边界，后续根据表现微调。
- Evidence: `Confirmed`
- Preconditions: 当前技能与 frame projection 已载入。
- Inputs: 真实 frame、已有字段 capability、用户选择的目标 frame。
- Outputs: 可选择节点、可选择/重定向边、按 wait 比例展开的单轨时间轴。
- Invariants: 不创建或删除缺失跳转字段；不把写 `0` 当作删除边；不自动推断主分支或运行时长；视觉单位不声明等于 Native tick 或秒。
- Dependencies: REQ-003、REQ-006、REQ-011。
- Acceptance: 节点、边和时间轴选择定位到同一 frame；重定向已有边只修改对应字段一次；分支、循环和 unresolved 仍可辨识；时间轴宽度满足 `max(1, wait)` 比例。
- Validation ID: VAL-015
- Status: Validated

### REQ-016: 正式项目与测试副本启动模式

- Statement: 双击“一键启动”时必须由用户选择正式项目或测试副本；自动化可通过显式参数选择同一模式。
- Rationale: 默认固定 LocalAppData 测试副本会让用户误以为正在编辑正式项目，也无法在仓库中持久化正式技能 sidecar。
- Source: 用户于 2026-08-07 明确要求启动时选择正式或测试模式。
- Evidence: `Confirmed`
- Preconditions: 仓库 `Assets/NTSD/Config/data.txt`、Native preview 和 Node 24 可用。
- Inputs: 交互选择，或 `-Mode Project` / `-Mode Test`。
- Outputs: 正式模式使用仓库根 workspace；测试模式使用 `%LOCALAPPDATA%\DatSkillFlowWeb\test-workspace`。
- Invariants: 正式模式不复制或删除仓库 Config；测试模式只有显式 `-ResetWorkspace` 才重建；`-ResetWorkspace` 不允许用于正式模式；两种模式都不生成演示技能；服务仍使用随机 loopback 端口并在就绪后打开浏览器。
- Dependencies: REQ-002、REQ-008、现有 safe workspace/save 合同。
- Acceptance: 无参数双击显示中文正式/测试/取消选择；两种显式模式将精确 workspace 传给服务；取消不构建或启动；冲突参数被拒绝；正式保存继续经过页面确认和恢复备份。
- Validation ID: VAL-016
- Status: Validated

### REQ-017: DAT 自动基础上下文、完整动作与内部阶段

- Statement: 左侧基础状态和完整动作必须直接从当前 DAT 自动派生。底层保留每个非零有效 `hit_*` 精确目标；正式列表将 state 0/1/2 聚合为 standing/walking/running 上下文，并只把没有基础直达或独立外部来源、仅由其他动作内部触发的目标融合为内部阶段。
- Rationale: 正式项目即使没有 sidecar，也应自动表达“基础状态 → 输入路线 → 完整动作 → 内部阶段”，避免 standing 变体和连续技后段被平铺为同级技能，也避免沿 standing 全量展开导致动作边界消失。
- Source: 用户于 2026-08-07 确认 DAT 自动入口；2026-08-11 明确要求融合被拆开的 `hit_*` 链，并重新平衡 standing/walking 的多入口展示。
- Evidence: `Confirmed`
- Preconditions: 当前对象 DAT 已解密并投影 frame 标题、state、next 和 `hit_*`。
- Inputs: frame 标题段、frame ID/occurrence、state、next、全部受支持 `hit_*`，以及可选 sidecar 展示覆盖。
- Outputs: 聚合的基础状态上下文；只显示动作根的完整动作列表；每个动作的多条入口路线、内部阶段和共享父动作归属；全部 Frame 的精确底层关系。
- Invariants: `hit_*` 精确目标仍是底层入口身份；值 `0` 不得被解释为 frame 0 入口；sidecar 不得创建入口或改变 DAT；标题相同不能单独作为融合依据；拥有基础状态直达路线的目标不能被内部融合；共享内部阶段不得任意只归属一个父动作。
- Dependencies: REQ-002、REQ-003、REQ-006、REQ-015。
- Acceptance: standing 的多个 state-0 变体聚合为一个基础上下文并统计可用动作；普通基础直达技能保持独立完整动作；只从动作内部进入的第二段/第三段不再平铺，而显示在父动作详情；共享内部阶段关联全部父动作；同时拥有基础直达入口的目标保持独立；sidecar 别名仍只改变显示。
- Validation ID: VAL-017
- Status: Validated

### REQ-018: 桌面三栏可拖动调宽

- Statement: 桌面布局的状态/技能区、预览区和属性区之间必须提供两条可拖动分隔条；鼠标、触控笔和键盘均可调整左右栏宽度，容器变化后自动保持有效边界。
- Rationale: 固定三栏宽度无法兼顾长技能名称、主预览画布和密集属性字段，用户需要按当前任务分配屏幕空间。
- Source: 用户于 2026-08-07 明确要求“左中右侧，要支持拖动”。
- Evidence: `Confirmed`
- Preconditions: viewport 宽度大于 850px，编辑器工作区已渲染。
- Inputs: 左/右 separator 的 pointer delta、方向键、Shift 加速、Esc 取消，以及工作区 ResizeObserver 尺寸。
- Outputs: 左右栏 CSS 宽度、剩余中栏宽度和同步更新的 separator ARIA value。
- Invariants: 左栏保持 200–420px，右栏保持 240–460px；中栏在紧凑桌面至少 360px、宽屏至少 420px；极限拖动和窗口缩放不得产生页面水平溢出；Esc 恢复本次拖动起点；不写 localStorage、DAT 或 sidecar。
- Dependencies: REQ-001、REQ-007、REQ-009。
- Acceptance: 1440×900 与 1024×768 可真实拖动两条分隔条并保持中栏最小宽度；方向键可调宽，拖动中 Esc 恢复；拖动中 resize 会结束交互并重新 clamp；390×844 隐藏分隔条且四标签页继续可用；console/error 为空。
- Validation ID: VAL-018
- Status: Validated

### REQ-019: Native 技能 Trace 按动作入口、主体、分身和投射物分类运行

- Statement: 预览从真实输入准备开始，按 `ntsd_cpp` 真实逻辑 tick 运行；只有主体进入所选完整动作拥有的 Frame 链后，才允许在其回到有效地面 idle 时结束主体技能进度；opoint 生成的角色/分身只需确认成功释放和首个有效快照；武器/投射物必须继续处理飞行、地面、碰撞和权威失效路径。
- Rationale: 分身后续属于 AI，不应拖长技能结束；投掷武器的轨迹和落地碰撞仍是技能可观察结果，不能因主体 idle 被截断。
- Source: 用户于 2026-08-07 对 Naruto 分身技能和 Frame 263 投掷武器场景的明确澄清。
- Evidence: `Confirmed`
- Preconditions: `ntsd_cpp` 可读取 root 与派生 OID 的 DAT；网页能按 Native entity 的 OID 加载对应 DAT/catalog 的 `rawObjectType`，并读取 Native 逐 tick entity 和必要状态字段。
- Inputs: 已成功触发的 root start frame、DAT/C++ runner seed、Native logical tick。
- Outputs: root actor 状态、opoint 生成事件、角色分身首个有效快照、投射物逐 tick 世界 Trace、分类完成原因。
- Invariants: 不调用 UI 键盘模拟；输入准备阶段返回 idle 不得结束尚未进入的动作；零等待首帧被 Native 跳过时可由同一动作拥有的后继 Frame 识别入口；未进入动作时必须报告 `entry-not-reached`；不把 DAT `wait` 当作 Native tick；不等待角色分身 AI 生命周期；不提前截断武器/投射物；slot 释放和复用不混淆 lineage。
- Dependencies: REQ-004、REQ-005、REQ-011、`ntsd_cpp` runner、multi-OID DAT/BMP resource projection、OID catalog object-type mapping。
- Acceptance: `F0 → F110 → F0 → 目标动作` 不会在目标入口前结束；Native 从零等待入口直接推进到所属后继 Frame 时仍能识别动作开始；F300 能显示 opoint 生成的分身；主体回 idle 后网页进度停止；Frame 263 类投射物继续显示飞行、落地/碰撞或权威失效；Trace 缺少完成条件时明确报告 `entry-not-reached`/`timeout`/`persistent`，不伪造完成。
- Validation ID: VAL-019
- Status: Planned

### REQ-020: Canvas 拖动设置双角色 Native 起始站位

- Statement: 用户可在独立的站位拖动模式中拖动预览场景里的 P1 与 P2；松开后以新的起始坐标重新运行当前完整动作。
- Rationale: 技能命中、opoint 生成、抓取和投射物表现都依赖双方相对位置，只在 Canvas 上平移图片不能用于可信参数预览。
- Source: 用户于 2026-08-11 明确要求增加“通过拖动控制场景中两个角色的位置”。
- Evidence: `Confirmed`
- Preconditions: 当前角色已生成至少一段 Native 预览，Tick 0 含 slot 0/1，双方精灵资源可解析。
- Inputs: P1/P2 精灵命中区域、pointer delta、stage width/zMin/zMax、当前完整动作 scenario。
- Outputs: P1/P2 的 Native `initial` X/Y/Z、重新生成的完整 Trace、同步站位读数。
- Invariants: Canvas 横向拖动映射世界 X，纵向拖动映射地面 Z；Y 高度保持不变；X/Z 夹取在 stage 边界；站位不写入 DAT/sidecar；切换技能沿用当前站位，切换角色重置；站位模式不触发 DAT 几何编辑；位置必须参与客户端、会话和 Native 缓存键。
- Dependencies: REQ-004、REQ-005、REQ-014、REQ-019、Native `--p1-x/y/z` 与 `--p2-x/y/z`。
- Acceptance: 精确命中 P1/P2 精灵；拖动中本地即时反馈；松开后 API 严格接收两组有限坐标并重新运行；Native metadata 与 Tick 0 slot 0/1 坐标等于请求值；重置恢复 Native 默认站位。
- Validation ID: VAL-024
- Status: Implemented; browser E4 pending

## Non-Goals

| ID | Explicitly excluded item | Reason | Revisit condition |
|---|---|---|---|
| NREQ-001 | 脱离 DAT 标题、state 和跳转关系凭空生成技能语义或中文名称 | 容易误判 NTSD 数据 | 有额外权威元数据来源时 |
| NREQ-002 | 第一阶段完整运行 `opoint/wpoint/cpoint` 语义 | 当前 Native runner 合同不足 | `ntsd_cpp` 提供对应可观察输出后 |
| NREQ-003 | 历史上排除双角色可控战斗；现由 REQ-020 仅开放双方起始站位拖动 | 不开放实时玩家控制，只允许重新生成确定性 Native 预览 | 需要实时双人输入或训练场控制器时重新评审 |
| NREQ-004 | 虚构搜索、撤销、全局保存等未接线按钮 | 违反真实可用原则 | 有正式需求和完整实现时 |

## Open Questions

| ID | Question | Impact | Recommended default | Owner | Status |
|---|---|---|---|---|---|
| Q-001 | 侧车文件是否默认纳入 Git | Medium | 文件可追踪但由项目所有者决定提交 | User | Open |
| Q-002 | 几何叠加颜色是否允许用户配置 | Low | 第一阶段固定高对比色 | Droid | Open |

## Rules

- Every formal requirement receives a stable ID.
- A requirement cannot become `Confirmed` from model inference alone.
- Preserve the original user brief and distinguish user-confirmed, user-delegated, observed and inferred sources.
- High-impact questions must be confirmed or covered by explicit authority to use the recommended default before implementation.
- Medium- and low-impact defaults may proceed only when clearly recorded and reversible.
- Requirement changes must preserve history and update affected validation entries.
