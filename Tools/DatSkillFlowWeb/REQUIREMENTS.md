# Requirements

## Status Vocabulary

`Proposed` · `Confirmed` · `In Progress` · `Validated` · `Deferred` · `Rejected` · `Blocked`

## Requirement Register

| ID | Requirement | Source | Evidence | Priority | Status | Validation |
|---|---|---|---|---|---|---|
| REQ-001 | 专业技能编辑器四区布局 | User / Visual brief | Confirmed | P0 | Confirmed | VAL-001 |
| REQ-002 | 技能名称与起始帧侧车元数据 | User | Confirmed | P0 | Confirmed | VAL-002 |
| REQ-003 | 当前技能真实 DAT 帧流程 | User / Product goal | Confirmed | P0 | Confirmed | VAL-003 |
| REQ-004 | 单角色高质量场景预览 | User | Confirmed | P0 | Confirmed | VAL-004 |
| REQ-005 | 全部现有 DAT 块的几何叠加 | User | Confirmed | P0 | Confirmed | VAL-005 |
| REQ-006 | 全部现有 DAT 块的结构化查看与编辑 | User | Confirmed | P0 | Confirmed | VAL-006 |
| REQ-007 | 明确的按钮与操作反馈状态 | User | Confirmed | P0 | Confirmed | VAL-007 |
| REQ-008 | 会话修改与 DAT 覆盖清晰分离 | User / Existing safety contract | Confirmed | P0 | Confirmed | VAL-008 |
| REQ-009 | 桌面、中屏、窄屏自适应 | Template / Recommended default | User-authorized default | P1 | Confirmed | VAL-009 |
| REQ-010 | 用户可见功能自行运行到 E4 证据 | Template | Confirmed | P0 | Confirmed | VAL-010 |
| REQ-011 | DAT 与 Native preview 权威行为不被 UI 改写 | Project rules | Confirmed | P0 | Confirmed | VAL-011 |

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
- Status: Confirmed

### REQ-002: 技能名称与起始帧侧车元数据

- Statement: 技能由用户维护的名称和起始帧组成，保存到项目根目录 `.dat-skill-flow/skills.json`。
- Rationale: 技能名称不是 DAT 权威字段，不能污染 DAT，也不能由工具自动猜测。
- Source: 用户确认。
- Evidence: `Confirmed`
- Preconditions: 项目工作区已授权。
- Inputs: OID、技能名称、起始帧。
- Outputs: 可恢复、可共享的技能元数据。
- Invariants: 侧车文件不得包含绝对路径；不得改变 DAT 字节。
- Dependencies: 安全工作区与安全保存合同。
- Acceptance: 新建或修改技能后重启服务，技能仍能恢复且 DAT 指纹不变。
- Validation ID: VAL-002
- Status: Confirmed

### REQ-003: 当前技能真实 DAT 帧流程

- Statement: 从技能起始帧出发，使用真实 `next` 和 `hit_*` 字段展示当前技能帧关系，不自动命名或分类技能。
- Rationale: 技能编辑器必须表达帧流程而不是只列出全部 DAT 帧。
- Source: 用户确认的技能模型与产品目标。
- Evidence: `Confirmed`
- Preconditions: 当前技能起始帧存在。
- Inputs: DAT 帧、`next`、`hit_a`、`hit_d`、`hit_j`、组合输入跳转。
- Outputs: 可选择的技能帧关系视图和时间轴标记。
- Invariants: 保留循环和分支；不把未知跳转解释成不存在的技能语义。
- Dependencies: REQ-002。
- Acceptance: 选择流程节点会定位预览和检查器到同一 DAT 帧。
- Validation ID: VAL-003
- Status: Confirmed

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
- Status: Confirmed

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
- Status: Confirmed

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
- Status: Confirmed

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
- Status: Confirmed

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
- Status: Confirmed

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
- Status: Confirmed

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
- Status: Confirmed

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
- Status: Confirmed

## Non-Goals

| ID | Explicitly excluded item | Reason | Revisit condition |
|---|---|---|---|
| NREQ-001 | 自动推断技能名称和分类 | 容易误判 NTSD 数据 | 有权威元数据来源时 |
| NREQ-002 | 第一阶段完整运行 `opoint/wpoint/cpoint` 语义 | 当前 Native runner 合同不足 | `ntsd_cpp` 提供对应可观察输出后 |
| NREQ-003 | 第一阶段双角色战斗预览 | 超出最小垂直切片 | 单角色和几何叠加达到 E4 后 |
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
