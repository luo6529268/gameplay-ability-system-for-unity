# NTSD DAT 技能流程编辑器 Operating Prompt

> 本文件由 `large-goal/PROMPT_TEMPLATE.md` 生成。项目事实以同目录的章程、需求、状态、决策和验收文件为准。

## 0. Interpretation Boundary

- 真实需求：逐步将现有 DAT 工具演进为专业技能编辑器，围绕技能、真实帧流、单角色预览、全部现有 DAT 块、字段编辑和安全保存形成闭环。
- 仅用于说明的例子：“螺旋丸”“替身术”等技能名；GPT 图中的技能区段、高清 Naruto 和未接线工具栏功能。
- 当前假设：现有 DatProjection 能提供第一阶段所需块字段，具体覆盖必须审计确认。
- 未确认内容：后续复杂 `opoint/wpoint/cpoint/itr` 运行语义所需 Native runner 扩展。
- 不得把例子、假设或未知项自动升级为正式需求。

## 1. Brief Expansion and Requirement Maturity Gate

### Input Maturity

- 用户原始描述：见 `PROJECT_CHARTER.md`。
- 当前成熟度：`Execution Ready`
- 已确认事实：见 `REQUIREMENTS.md` 的 Confirmed 项。
- 建议默认值：固定高对比叠加色；GPT 图采用修正后结构。
- 高影响未知项：无。
- 门禁状态：`Ready`

### Expansion Procedure

1. 保留用户原始描述，区分事实、建议默认值、假设和未知项。
2. 正式需求只写入 `REQUIREMENTS.md`，使用稳定 ID。
3. 高影响变更更新 `DECISIONS.md` 并等待确认。
4. 当前项目继续按最小垂直切片推进，不一次实现复杂运行语义。

### Readiness Gate

- 使命、用户、核心流程、交付物、范围、非目标、最小切片和验收维度已经明确。
- Gate 为 `Ready`。

## 2. Mission

- 项目名称：NTSD DAT 技能流程编辑器
- 总使命：让用户围绕技能理解、预览和安全编辑真实 NTSD DAT。
- 最终成果：可长期维护的本地专业技能编辑器。
- 成功标准：REQ-001 至 REQ-011 达到各自验收等级。
- 失败或阻塞标准：用户可见功能无法取得 E4、数据来源无法追踪、实现需要猜测权威语义。
- 当前规模模式：`Standard`

## 3. Scope

### In Scope

- 技能名称 + 起始帧侧车元数据。
- 真实 `next` 与 `hit_*` 帧流程。
- 单角色 Native preview。
- 六类现有 DAT 块的查看、编辑和几何叠加。
- 专业编辑器布局、操作状态、自适应和安全保存。

### Out of Scope

- 自动推断技能。
- 第一阶段复杂对象生成、武器联动、抓取或命中结果。
- 第一阶段双角色战斗。
- 虚构或未接线功能。

### Deferred

- 复杂块运行语义、双角色交互、技能模板和高级编辑能力。

## 4. Context and Evidence

- 项目现状：已有真实项目会话、DAT 无损编辑、BMP capability、Native preview、safe save 和中文技术页。
- 已有资产：`Tools/DatSkillFlowWeb`、真实 Unity 工作区、`ntsd_cpp`。
- 技术环境：Windows 10、Node 24、本地回环服务。
- 证据标签：`Observed`、`Measured`、`Confirmed`、`Inferred`、`Unknown`、`Blocked`
- 重要结论附文件、测试或运行证据；静态搜索不到不等于不存在。

## 5. System Decomposition

```text
技能编辑器
├── 技能元数据与流程
├── 预览与几何叠加
├── 当前帧检查器
├── 时间轴与交互状态
└── 本地安全服务
```

- 模块职责、输入、输出、依赖和不变量记录在 `PROJECT_CHARTER.md`。
- 跨模块实现前先补齐 API、schema、坐标和状态合同。

## 6. Requirements and Contracts

- 正式需求见 `REQUIREMENTS.md`。
- 公共接口、侧车 schema、帧关系、坐标变换、保存与响应式行为必须可追踪。
- 推断不得替代用户确认；与权威行为冲突时暂停。

## 7. Phases and Gates

当前阶段：阶段 1，审计和事实收集。

```text
阶段 0：简报扩展和需求成熟度门禁        已通过
阶段 1：审计和事实收集                  当前
阶段 2：需求、架构和合同确认
阶段 3：最小垂直闭环
阶段 4：能力扩展和集成
阶段 5：稳定性、性能和安全
阶段 6：最终验收和交付
```

每阶段必须定义目标、允许修改范围、禁止事项、交付物、验证、进入条件和停止条件，并更新 `CURRENT_STATE.md`。

## 8. Work Protocol

1. 读取适用项目规则、`PROJECT_CHARTER.md`、`CURRENT_STATE.md`、`DECISIONS.md` 和当前阶段资料。
2. 检查工作区并保护已有修改。
3. 读取当前任务对应的需求和验收 ID。
4. 提出本次最小实施范围和验证计划。
5. 先补测试或可观察失败，再实现。
6. 先完成可运行的最小闭环，再扩展能力。
7. 自行构建并启动，确认 build ID 和静态模块。
8. 运行最窄验证，再运行完整回归。
9. 使用隔离客户端取得 E3/E4 证据；不可用时标记 Blocked。
10. 停止自己启动的测试服务和浏览器，用户明确要求保留的服务除外。
11. 更新状态、需求、决策、验收和风险。
12. 汇报完成、未完成、阻塞和下一步。

## 9. Self-Run and Automated Validation

### Runtime Ownership

- 模型负责构建、启动和基础运行验证，用户只承担最终主观验收。
- 使用隔离浏览器，不控制用户现有 Chrome 或账号。
- 每次启动确认当前 build ID，检查模块 404、控制台和网络错误。

### Evidence Gate

- E1 Build、E2 Service、E3 Render、E4 Interaction、E5 Acceptance 分级见 `ACCEPTANCE.md`。
- 用户可见功能至少达到 E4 才能标记 Passed。
- 证据记录构建、环境、操作、预期、实际和路径。

### Automated Test Procedure

1. 隔离旧服务并构建。
2. 确认 build ID、健康检查和全部浏览器模块。
3. 启动隔离浏览器。
4. 检查异常、日志、资源失败和初始化状态。
5. 验证真实 BMP、技能、帧、叠加和检查器可见。
6. 执行选择、播放、暂停、单步、叠加开关、字段编辑和保存边界。
7. 覆盖成功、失败和恢复路径。
8. 保存三个 viewport 的截图和交互日志。
9. 更新 `ACCEPTANCE.md` 与 `CURRENT_STATE.md`。
10. 清理测试进程。

### Diagnostic Discipline

- 每次修复只验证一个证据支持的假设。
- 同一现象一次修改无效后先增加可观察性，不连续猜测。
- 主浏览器不可用时切换隔离备用环境；仍不可用则标记 Blocked。

## 10. Context Compression and Recovery

- 恢复后依次读取 `PROJECT_CHARTER.md`、`CURRENT_STATE.md`、`DECISIONS.md` 和当前阶段文件。
- 先复述目标、阶段、约束、未知项和下一步。
- 不依赖聊天记录保存正式需求。

## 11. Change Control and Safety

- 小范围增量修改直接记录。
- 改变使命、范围、模块边界、API、侧车 schema、兼容策略或验收标准时先提交变更说明。
- 不覆盖、回退、删除或清理用户修改。
- 不把技能侧车数据写入 DAT。
- 不为通过 UI 验收伪造数据或功能。

## 12. Deliverables and Acceptance

- 交付物：代码、Standard 模板文档、schema、测试、当前构建、运行说明和 E4/E5 证据。
- 验收标准见 `ACCEPTANCE.md`。
- 每个重要需求必须追踪到实现和验证。
- 未达到证据等级不得标记完成。
