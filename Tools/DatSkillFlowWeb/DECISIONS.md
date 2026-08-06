# Decision Log

## Decision Status

`Proposed` · `Confirmed` · `Superseded` · `Rejected`

## Decision Register

| ID | Decision | Status | Date | Affected Areas |
|---|---|---|---|---|
| DEC-001 | 使用 Standard large-goal 模式 | Confirmed | 2026-08-06 | 项目管理、验证 |
| DEC-002 | 技能由名称和起始帧定义 | Confirmed | 2026-08-06 | 技能模型、UI |
| DEC-003 | 技能元数据使用项目侧车文件 | Confirmed | 2026-08-06 | 数据合同、服务 |
| DEC-004 | 第一阶段使用单角色预览 | Confirmed | 2026-08-06 | 预览、范围 |
| DEC-005 | 第一阶段纳入全部现有 DAT 块查看、编辑和几何叠加 | Confirmed | 2026-08-06 | 检查器、预览 |
| DEC-006 | GPT 图作为修正后的视觉方向稿 | Confirmed | 2026-08-06 | 信息架构、视觉 |
| DEC-007 | itr 成对动作字段原子编辑 | Confirmed | 2026-08-06 | DAT capability、检查器、保存 |

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

### DEC-002: 技能由名称和起始帧定义

- Status: Confirmed
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

### DEC-003: 技能元数据使用项目侧车文件

- Status: Confirmed
- Date: 2026-08-06
- Context: 技能名称不是 DAT 权威字段。
- Options considered:
  1. 项目根 `.dat-skill-flow/skills.json`
  2. 浏览器本地存储
  3. 工具目录文件
- Decision: 使用项目侧车文件。
- Rationale: 不污染 DAT，可随项目恢复和共享，也不进入 Unity Assets 导入流程。
- Evidence: 用户选择“项目侧车文件”。
- Consequences: 需要安全、版本化且不含绝对路径的侧车合同和 API。
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

## Change Rules

- Record decisions that affect scope, architecture, public interfaces, data formats, compatibility, performance budgets or security.
- Do not silently replace a confirmed decision.
- A changed decision creates a new entry and marks the old entry `Superseded`.
- Distinguish user-confirmed decisions from model recommendations.
