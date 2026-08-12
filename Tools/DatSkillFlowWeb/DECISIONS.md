# Decision Log

## Decision Status

`Proposed` · `Confirmed` · `Superseded` · `Rejected`

## Decision Register

| ID | Decision | Status | Date | Affected Areas |
|---|---|---|---|---|
| DEC-001 | 使用 Standard large-goal 模式 | Confirmed | 2026-08-06 | 项目管理、验证 |
| DEC-002 | 技能由名称和起始帧定义 | Superseded by DEC-009 | 2026-08-06 | 技能模型、UI |
| DEC-003 | 入口显示元数据使用项目侧车文件 | Confirmed | 2026-08-06 / 2026-08-07 revised | 数据合同、服务 |
| DEC-004 | 第一阶段使用单角色预览 | Confirmed | 2026-08-06 | 预览、范围 |
| DEC-005 | 第一阶段纳入全部现有 DAT 块查看、编辑和几何叠加 | Confirmed | 2026-08-06 | 检查器、预览 |
| DEC-006 | GPT 图作为修正后的视觉方向稿 | Confirmed | 2026-08-06 | 信息架构、视觉 |
| DEC-007 | itr 成对动作字段原子编辑 | Confirmed | 2026-08-06 | DAT capability、检查器、保存 |
| DEC-008 | 启动时选择正式项目或测试副本 | Confirmed | 2026-08-07 | 启动、安全边界 |
| DEC-009 | 使用 DAT 混合自动入口，sidecar 仅影响显示 | Superseded by DEC-017 | 2026-08-07 | 技能模型、sidecar、UI |
| DEC-010 | 跨技能 hit_* 使用可点击叶节点 | Superseded by DEC-017 | 2026-08-07 | Flow、导航 |
| DEC-012 | 复用临时浏览器并限制实例，使用后清零进程 | Confirmed | 2026-08-07 | 验收、资源管理、电脑性能 |
| DEC-013 | Trace 按派生对象类别分别结束 | Confirmed | 2026-08-07 | Native Trace、opoint、武器投射物、预览 |
| DEC-014 | 技能编辑器是主体，Native Trace 只是预览支撑 | Confirmed | 2026-08-07 | 产品范围、预览、网页架构 |
| DEC-015 | 左侧按基础状态、输入技能、全部 Frame 导航，单帧只定位完整动作回放 | Superseded by DEC-017 | 2026-08-09 | 信息架构、预览、时间线 |
| DEC-017 | 左侧按基础上下文、完整动作、全部 Frame 导航，动作内部 hit_* 保守融合 | Confirmed | 2026-08-11 | 技能模型、导航、预览、Frame 归属 |

## Decision Detail

### DEC-001: 使用 Standard large-goal 模式

- Status: Confirmed
- Date: 2026-08-06
- Context: 技能编辑器是需要长期演进的完整工具模块，涉及客户端、服务、数据合同和自动验收。
- Options considered:
  1. Lite
  2. Standard
  3. Full
- Decision: 使用 Standard。
- Rationale: 需要完整需求、状态、决策和验收追踪，但当前尚不需要 Full 的发布矩阵与迁移计划。
- Evidence: 用户要求使用 `large-goal` 模板。
- Consequences: 所有阶段使用模板文档和 E4 用户可见验证门禁。
- Rejected alternatives: Lite 追踪深度不足；Full 当前过重。
- Affected requirements: REQ-001 至 REQ-011。
- Revisit condition: 出现跨工具、多人长期协作或正式发布矩阵时升级 Full。

### DEC-002: 技能由名称和起始帧定义（已取代）

- Status: Superseded by DEC-009
- Date: 2026-08-06
- Context: DAT 没有可靠的通用技能名称字段，自动推断会创造不确定语义。
- Options considered:
  1. 名称 + 起始帧
  2. 只选起始帧
  3. 自动推断技能
- Decision: 左侧正式引入技能列表，每项由名称和起始帧组成。
- Rationale: 满足技能编辑器的组织需求，同时不猜测 NTSD。
- Evidence: 用户选择“要，名称 + 起始帧”。
- Consequences: 需要独立技能元数据和以起始帧为入口的流程视图。
- Rejected alternatives: 只选帧不够接近技能编辑器；自动推断风险不可接受。
- Affected requirements: REQ-002、REQ-003。
- Revisit condition: 获得权威技能清单或 DAT 正式技能元数据时。

### DEC-003: 入口显示元数据使用项目侧车文件

- Status: Confirmed
- Date: 2026-08-06
- Context: 中文别名、分组、顺序、置顶、隐藏和备注不是 DAT 权威字段。
- Options considered:
  1. 项目根 `.dat-skill-flow/skills.json`
  2. 浏览器本地存储
  3. 工具目录文件
- Decision: 使用项目侧车文件，但只保存 `(oid,startFrame)` 对应的纯展示覆盖。
- Rationale: 不污染 DAT，可随项目恢复和共享，也不进入 Unity Assets 导入流程。
- Evidence: 用户选择“项目侧车文件”。
- Consequences: 需要安全、版本化且不含绝对路径的侧车合同和 API；sidecar 缺失/无效不得清空 DAT 自动入口，也不得创建入口。
- Rejected alternatives: 浏览器数据不可共享；工具目录会混合项目与工具状态。
- Affected requirements: REQ-002。
- Revisit condition: 项目引入正式数据库或统一元数据服务时。

### DEC-004: 第一阶段使用单角色预览

- Status: Confirmed
- Date: 2026-08-06
- Context: 当前最需要改善单角色技能预览的清晰度和操作性。
- Options considered:
  1. 单角色场景
  2. 角色与完整碰撞结果
  3. 双角色战斗
- Decision: 第一阶段使用单角色场景。
- Rationale: 可基于当前 Native preview 完成真实闭环。
- Evidence: 用户选择“单角色场景预览”。
- Consequences: 双角色和命中结果延后。
- Rejected alternatives: 当前范围和 Native runner 合同不足。
- Affected requirements: REQ-004。
- Revisit condition: 单角色预览和几何叠加达到 E4。

### DEC-005: 第一阶段纳入全部现有 DAT 块

- Status: Confirmed
- Date: 2026-08-06
- Context: GPT 图只展示基础字段，遗漏 `opoint`、`wpoint` 和其他块，无法满足技能编辑器需求。
- Options considered:
  1. 查看 + 编辑 + 几何叠加
  2. 只查看和编辑
  3. 第一阶段暂不加入
- Decision: 对 `itr`、`bdy`、`opoint`、`wpoint`、`bpoint`、`cpoint` 完成结构化查看、字段编辑和几何叠加。
- Rationale: 现在建立正确信息架构，避免先完成外观后重构。
- Evidence: 用户选择推荐方案。
- Consequences: 第一阶段需要检查器分组、字段 capability 映射和预览叠加合同。
- Rejected alternatives: 延后会导致信息架构返工。
- Affected requirements: REQ-005、REQ-006。
- Revisit condition: 发现当前 DatProjection 缺少必要真实字段时。

### DEC-006: GPT 图作为修正后的视觉方向稿

- Status: Confirmed
- Date: 2026-08-06
- Context: 图片提供了有效的四区布局和视觉层级，但包含虚构技能、高清角色和未接线功能。
- Options considered:
  1. 1:1 照搬
  2. 保留结构并按真实能力修正
  3. 放弃该方向
- Decision: 保留顶部、左侧、中间、右侧、底部结构和深色金色视觉语言，删除虚构能力并使用真实数据。
- Rationale: 兼顾用户认可的方向和工程真实性。
- Evidence: 用户继续基于该图讨论 DAT 块范围。
- Consequences: 实现必须以真实 BMP、真实字段和真实状态为准。
- Rejected alternatives: 1:1 照搬会产生伪功能；放弃会浪费已确认方向。
- Affected requirements: REQ-001、REQ-004、REQ-007、REQ-009。
- Revisit condition: 第一阶段真实渲染评审不满足可用性目标。

### DEC-007: itr 成对动作字段原子编辑

- Status: Confirmed
- Date: 2026-08-06
- Context: `catchingact` 和 `caughtact` 各自包含两个整数，第二个值不是独立 DAT 键；普通标量 capability 无法安全表达。
- Options considered:
  1. 成对原子编辑
  2. 第一阶段只读显示
  3. 第一阶段不显示
- Decision: 使用成对编辑器，两值一次提交、一次校验、一次无损写入。
- Rationale: 满足“全部块查看和编辑”，同时不破坏原始字段跨度。
- Evidence: DAT 块能力审计报告和用户选择“成对原子编辑”。
- Consequences: DatSession capability/请求合同需要支持 pair 类型或专用 pair edit 请求；检查器必须显示两个输入框。
- Rejected alternatives: 将第二个整数当作独立字段会造成错误定位和潜在数据破坏；只读不满足已确认范围。
- Affected requirements: REQ-006、REQ-011。
- Revisit condition: DAT parser 将该字段正式建模为独立键时。

### DEC-008: 启动时选择正式项目或测试副本

- Status: Confirmed
- Date: 2026-08-07
- Context: 固定使用 LocalAppData 测试副本会让用户误以为正在编辑正式项目。
- Decision: 无参数启动显示 Project/Test/取消；非交互必须显式传入模式，`-ResetWorkspace` 仅允许 Test。
- Rationale: 让可写 workspace 和风险边界在启动时可辨识。
- Consequences: 取消先于依赖检查；任何测试副本重置前必须完成该模式所需的非破坏性前置检查。
- Affected requirements: REQ-016。

### DEC-009: 使用 DAT 混合自动入口，sidecar 仅影响显示

- Status: Superseded by DEC-017
- Date: 2026-08-07
- Context: 正式 Naruto DAT 已包含 frame 标题、state、next 和 `hit_*`，不应要求 sidecar 重复定义入口与首帧。
- Decision: 每个 frame ID 使用最后 occurrence；同标题且 frame ID 连续的段合并，非连续同标题保持独立；每个非零有效 `hit_*` 目标成为精确入口。
- Rationale: 无 sidecar 时仍能从当前 DAT 恢复完整状态/动作/输入技能列表。
- Consequences: `hit_*` 技能首帧是跳转后的目标帧；值 0 不得成为 frame 0 输入入口；手工技能复制/删除合同被取代。
- Affected requirements: REQ-002、REQ-003、REQ-017。

### DEC-010: 跨技能 hit_* 使用可点击叶节点

- Status: Superseded by DEC-017
- Date: 2026-08-07
- Context: 在 standing Flow 中完整展开 F300 技能会吞并其他技能并使图失控。
- Decision: `next` 继续展开当前流程；指向其他自动入口的 `hit_*` 只显示可点击目标卡，点击后切换到目标入口。
- Rationale: 同时保留真实跳转信息和清晰的技能边界。
- Consequences: Flow 增加 entry 叶节点类型，SVG/表格均支持切换入口。
- Affected requirements: REQ-003、REQ-015、REQ-017。

### DEC-011: 桌面双分隔条使用会话内受限宽度

- Status: Confirmed
- Date: 2026-08-07
- Context: 用户要求左/中/右区域可拖动，但移动端已经使用四标签页，且栏宽不属于项目数据。
- Decision: >850px 使用左右两条 vertical separator；纯 `PanelLayout` 计算左右栏边界和中栏预算，pointer/键盘/ResizeObserver 只更新当前页面 CSS 变量；≤850px 隐藏 separator。
- Rationale: 在不改变 DAT、sidecar 或移动信息架构的前提下提供直接调宽，并通过统一 clamp 防止页面溢出。
- Consequences: 宽度不写 localStorage，刷新后回到当前 viewport 默认值；中栏紧凑桌面至少 360px、宽屏至少 420px；separator 必须支持 ARIA、键盘和 Esc。
- Affected requirements: REQ-001、REQ-007、REQ-009、REQ-018。

### DEC-012: 复用临时浏览器并限制实例，使用后清零进程

- Status: Confirmed
- Date: 2026-08-07
- Context: 浏览器自动化会为单个隔离会话创建多个 Chrome/Edge 子进程；异常退出或重复启动会造成电脑卡顿。
- Decision: 临时浏览器可以启动，但启动前必须先检查现有 `agent-browser-*` 临时 profile 是否可用并优先复用；不可用时先确认是否仍在使用，确认空闲后才关闭。每次使用结束必须关闭完整进程树并确认对应临时 profile 残留为 0。
- Rationale: 在允许必要浏览器 E4 验收的同时，避免重复创建多个会话导致电脑卡顿，并保护用户普通 Chrome/Edge。
- Consequences: 浏览器实例数量和生命周期必须受控；清理只允许按临时 profile、启动参数或本次任务记录精确识别，不得结束用户普通 Chrome/Edge。浏览器可用性检查、复用、结束和清零应成为每次验收的固定步骤。
- Affected requirements: REQ-010、REQ-011，以及所有需要浏览器证据的 VAL。
- Revisit condition: 项目建立可靠的单实例浏览器生命周期管理后。

### DEC-013: Trace 按派生对象类别分别结束

- Status: Confirmed
- Date: 2026-08-07
- Context: 技能主体回到 idle 后，opoint 生成的分身可能继续执行 AI；投掷武器则仍有飞行、落地和碰撞逻辑，二者不能使用同一个结束条件。
- Decision: `actorSkillEnded` 只表示 root 技能主体回到有效地面 idle。网页根据 C++ 对应的 OID DAT/catalog `rawObjectType` 映射对象类别：角色类派生对象只记录 opoint 成功和首个有效快照，不等待其 AI 后续生命周期；武器类派生对象继续执行 Native tick，直到按对应 C++ 物理、落地、碰撞或失效路径完成。若投射物超过安全上限，Trace 以 `timeout` 或 `persistent` 结束。
- Rationale: 保证“成功释放分身”与“投掷物轨迹/地面碰撞”都被正确表达，同时避免 AI 生命周期把技能进度条无限延长。
- Consequences: UI 进度条以 root actor 结束为准；网页 DTO/资源图需要按 OID 携带 C++ 对象类别、派生关系、投射物完成事件和分类结束原因；DAT wait 仍只是视觉比例，不是 Native tick。`ntsd_cpp` 只读，不为网页展示专门改写 CLI 游戏逻辑。
- Affected requirements: REQ-004、REQ-011、REQ-019。
- Revisit condition: `ntsd_cpp` 提供新的对象生命周期或技能实例权威合同时。

## Change Rules
### DEC-014: 技能编辑器是主体，Native Trace 只是预览支撑

- Status: Confirmed
- Date: 2026-08-07
- Context: 项目目标是把工具演进为 NTSD DAT 技能流程编辑器；C++ 工程只是行为权威和 Native preview 数据源。
- Decision: 网页项目继续围绕技能入口、Flow、DAT 编辑、几何叠加、时间轴、预览和安全保存建设。Native Trace 只实现编辑器为准确展示技能所需的最小可观察运行结果，不在网页重建完整 C++ 游戏、AI、战斗或双角色系统。
- Rationale: 保持产品目标聚焦，避免把技能编辑器扩大成网页游戏运行时。
- Consequences: REQ-019 属于预览支撑能力；所有 Trace 字段和结束条件都必须服务于编辑器表现、定位和验证；`ntsd_cpp` 权威逻辑保持只读参考，不作为网页功能迁移目标。
- Affected requirements: REQ-001、REQ-004、REQ-005、REQ-011、REQ-019。
- Revisit condition: 用户明确提出完整战斗模拟产品需求。

### DEC-015: 左侧按基础状态、输入技能、全部 Frame 导航，单帧只定位完整动作回放

- Status: Superseded by DEC-017
- Date: 2026-08-09
- Context: 左栏同时展示入口表、Flow 表和 Flow 图，DAT 关系与运行表现混在一起；单独选择中间 Frame 还可能绕过真实输入/状态初始化。
- Decision: 左栏默认只提供“基础状态 / 输入技能 / 全部 Frame”三类导航和筛选；中间始终是不可由玩家控制的完整 Native 战斗场景；右侧帧参数检查器暂时保持原样；底部改为根实体真实 Native Tick/Frame 时间线。选择单个 Frame 时，先按 DAT `next` 链找到最早的完整动作入口，运行完整场景后再定位该 Frame；找不到入口时拒绝从孤立 Frame 启动。旧 Flow 表和 SVG 保留为兼容代码，但默认隐藏且不参与普通渲染。
- Rationale: 新用户首先看到“状态/技能 → 完整表现 → 当前帧参数”的直接关系，同时避免 F212 等中间帧缺少 Native 初始化，并减少切换时对隐藏 Flow 的重复 DOM/SVG 渲染。
- Consequences: “全部 Frame”不是新的运行入口集合；输入技能页同时容纳 DAT 输入入口和其他可播放动作；运行时间线按连续根实体 Frame 分段，重复回访形成新段，根实体缺失时不伪造段。
- Affected requirements: REQ-001、REQ-002、REQ-003、REQ-004、REQ-007、REQ-009、REQ-011、REQ-017、REQ-019。
- Revisit condition: 用户确认需要把 Flow 作为独立高级编辑页恢复，或 Native 提供比根实体 Frame 更高层的动作实例标识。

### DEC-016: 补丁角色使用包作用域身份，保留 DAT 原始 OID

- Status: Confirmed
- Date: 2026-08-10
- Context: `NTSD2.4大量人物补丁（2）` 中不同包会重复使用同一 OID；把 OID 全局重编号会破坏 DAT `opoint`、Native catalog 和对象资源引用。补丁包还可能使用任意名称的清单、错误拼写 `tupe/tpye`，或依赖编辑器补充清单。
- Decision: 启动控制台先以只读方式扫描补丁库并生成有界 JSON 索引；编辑器身份采用 `packageId + sourceOid`，传给 Native 和 DAT 内部的仍是 `sourceOid`。API 只公开 `type == 0` 的角色入口，但包内其他 type 的 DAT 与 BMP 保留为依赖目录。补丁会话只读，基础 NTSD 2.4.1 仍是默认包和 Native 未覆盖依赖的回退源。
- Rationale: 同时满足跨包 OID 唯一、DAT/Native 语义不变、角色列表清晰和坏包隔离；单个缺失路径或冲突只产生包级诊断，不使整个项目不可用。
- Consequences: 顶部 UI 使用“数据包 → 角色”两级选择；sidecar 显示元数据暂时只应用于基础包，防止同 OID 串包；包内精灵按路径后缀和唯一 basename 优先解析；Native 依赖覆盖尚未完整实现时必须显示诊断，不能声称补丁行为完全等同独立游戏包。
- Affected requirements: REQ-001、REQ-004、REQ-005、REQ-011、REQ-016、REQ-019。
- Revisit condition: Native preview CLI 提供正式的多根 catalog/overlay 输入合同，或 sidecar schema 升级为包作用域身份。

### DEC-017: 左侧按基础上下文、完整动作、全部 Frame 导航，动作内部 hit_* 保守融合

- Status: Confirmed
- Date: 2026-08-11
- Context: 把每个非零 `hit_*` 目标平铺为入口，会把 standing/walking 变体、连续技第二段和变身中间段都显示成同级技能；但把所有可追溯到 standing 的 Frame 全量展开又会把角色动作合成一张失控的大图。
- Decision: 底层继续保留每个精确 `hit_*` 目标，供 sidecar、旧 Flow、Frame 定位和兼容代码使用；正式左栏改为“基础状态 / 完整动作 / 全部 Frame”。state 0/1/2 按 standing/walking/running 上下文聚合。只有当目标的全部有效入口都来自其他动作的 `next` 链、且没有基础状态直达或独立外部来源时，才标记为内部阶段并归属到完整动作根。共享内部阶段可归属多个根；同时存在基础直达路线的目标保持独立完整动作。
- Rationale: 用户先看到“从什么状态、通过什么路线、播放哪个完整动作”，同时保留不同入口可能携带的速度、朝向和 Native 状态差异；融合规则有明确证据边界，不依赖标题相同或 Naruto 特例。
- Consequences: 完整动作列表只显示动作根并汇总多条来源路线和内部阶段；基础状态详情显示状态变体与可发起动作；全部 Frame 继续暴露底层定义。动作内部多段输入的自动执行、跨 DAT 资源切换和变身 Trace 连续性仍需独立场景合同，不能仅凭 UI 归属推断为已完成。
- Affected requirements: REQ-001、REQ-002、REQ-003、REQ-004、REQ-007、REQ-009、REQ-011、REQ-017、REQ-019。
- Revisit condition: Native 提供高层动作实例标识，或实际 DAT 证明当前保守归属规则无法区分共享动作与内部阶段。

### DEC-018: 完整动作必须先进入所属 Frame 链，之后才能判定结束

- Status: Confirmed
- Date: 2026-08-11
- Context: 组合输入会先经过 `F110 defend` 等 Native 准备状态，再短暂回到 `state 0`，下一 Tick 才进入所选动作。旧 Trace 把准备状态视为动作开始，并把入口前的 idle 视为动作结束，导致 F347 等动作只播放前置输入。部分补丁的零等待入口还会在首个快照前从 F235 推进到 F236。
- Decision: 服务端从当前完整动作目录取得所选根动作拥有的有效 Frame 集合。只有 root 首次进入该集合后，Trace 才记录 `rootSkillStartedTick` 并允许判定主体结束；实际命中的后继 Frame 记录为 `rootSkillEntryFrame`。运行到上限仍未进入动作时返回 `entry-not-reached`，不得把准备阶段伪报为完成。客户端拒绝任何早于动作开始的进度/播放终点。
- Rationale: 同时覆盖普通组合输入、零等待首帧和补丁包差异，不对角色、OID 或 Frame 编号写特例。
- Consequences: 主体仍在动作链中循环时状态为 `timeout` 并播放完整 Trace；主体结束后的投射物尾迹继续遵循 DEC-013。入口未命中会保留准备 Trace 供诊断，但界面明确显示“入口未命中”。
- Affected requirements: REQ-004、REQ-011、REQ-017、REQ-019。
- Revisit condition: Native runner 直接输出稳定的高层动作实例开始/结束标识。


- Record decisions that affect scope, architecture, public interfaces, data formats, compatibility, performance budgets or security.
- Do not silently replace a confirmed decision.
- A changed decision creates a new entry and marks the old entry `Superseded`.
- Distinguish user-confirmed decisions from model recommendations.
