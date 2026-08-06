# Current State

> 这是会话恢复和上下文压缩后的首要读取文件。只记录当前有效状态，不记录完整历史。

## Snapshot

- Updated: 2026-08-06
- Brief maturity: `Execution Ready`
- Requirement gate: `Ready`
- Project phase: 阶段 2，需求、架构和合同确认
- Active task: 固化技能侧车、完整 capability 定位、ITR 成对字段、技能流程和几何叠加合同
- Active requirement IDs: REQ-001 至 REQ-011
- Current branch or workspace: `NTSD_GAS` / `Tools/DatSkillFlowWeb`
- Current build ID or revision: `20260806030537013-d7fe711275344872a488644d0c3bb98d`
- Open P0-P2 issues:
  - P1：浏览器自动化被 Windows Chrome 进程和页面文件提交压力阻塞，尚无当前 UI 的 E3/E4 证据。
  - P1：当前客户端尚未建立技能、帧流和全部 DAT 块检查器。
  - P1：当前客户端 capability 索引仍是 `frameId:key`，不支持重复块安全编辑。
- Next required action: 实现完整 capability locator、ITR 成对字段、技能流程和几何叠加，再进入最小 UI 垂直闭环。

## Completed

- 接入真实 `data.txt`、DAT、BMP、项目会话、编辑、预览、保存和关闭 API。
- Native preview 明确限制为 Naruto OID 2。
- 修复 `project-client.js` 缺失于浏览器静态白名单导致页面永远停在初始状态的问题。
- 补齐 session `dirty` 响应合同。
- 完整测试 267 项：266 通过、0 失败、1 跳过。
- 真实服务链路通过：静态模块 200、137 个 OID、Naruto 打开、字段编辑、Native preview 和关闭。
- 完成 large-goal Brief Expansion 和需求成熟度门禁。
- 用户确认技能名称与起始帧、单角色预览、项目侧车文件、全部 DAT 块查看/编辑/几何叠加。
- GPT 视觉稿完成可实现性评审。
- 完成阶段 1 的 sidecar、DAT block、客户端 UI 架构审计。
- 建立 `ARCHITECTURE.md`，明确技能侧车、完整 capability、pair、flow、overlay、按钮和响应式合同。
- 完成技能侧车服务：schema/UTF-8/大小与数量边界、固定路径、独立 revision/etag、首次创建、外部变更和 CAS 错误映射。
- 完成 native 安全目录创建和独立 sidecar 文档注册，侧车不会复用或污染 DAT documentId。
- 接入 `/api/project/skills` GET/POST、CLI 生命周期和 Origin/token 保护。
- 聚焦侧车/API/native 安全测试通过，覆盖非法 schema、并发冲突、目录创建和 DAT 文档隔离。

## In Progress

- 实现阶段 2 剩余合同：完整 capability locator、ITR pair、技能流程、几何叠加和客户端状态模型。

## Blocked

- 当前构建的隔离浏览器渲染与交互自动化。
- Cause: Windows 检测到约 802 个 Chrome 进程、约 42 GB Chrome working set，Chrome DLL 因页面文件/内存提交压力无法载入。
- Required decision or evidence: 不得擅自终止用户 Chrome；需要可用的隔离浏览器环境后才能达到 E3/E4。

## Confirmed Constraints

- 使用 `C:\Users\Logan\.codex\templates\large-goal`，不重新设计项目模板。
- Standard 模式。
- 不依赖 Unity 运行。
- 技能不自动推断，由用户维护名称和起始帧。
- 技能元数据保存在 `.dat-skill-flow/skills.json`。
- 第一阶段使用单角色预览。
- 第一阶段包含 `itr`、`bdy`、`opoint`、`wpoint`、`bpoint`、`cpoint` 查看、编辑和几何叠加。
- 复杂对象生成、武器联动、抓取和命中结果语义延后。
- DAT 和 `ntsd_cpp` 是数据与预览权威，UI 不创造战斗规则。
- 用户可见功能未达到 E4 不得标记通过。

## Unknowns and Assumptions

- `[Unknown]` `ntsd_cpp` 后续为复杂块运行语义提供哪些新输出。
- `[Inferred]` 当前 DatProjection 已包含六类块的主要结构化字段，需要逐字段审计 capability 覆盖。
- `[Recommended Default]` 固定高对比叠加色，第一阶段不提供颜色配置。
- `[User-Authorized Default]` 桌面四区布局，中屏收起侧栏，窄屏标签页。

## Recent Decisions

- DEC-001: 使用 Standard large-goal 模式。
- DEC-002: 技能由名称和起始帧定义。
- DEC-003: 技能元数据使用项目侧车文件。
- DEC-004: 第一阶段使用单角色预览。
- DEC-005: 第一阶段纳入全部现有 DAT 块查看、编辑和几何叠加。
- DEC-006: GPT 图作为修正后的视觉方向稿。
- DEC-007: itr 成对动作字段原子编辑。

## Validation

- Last build command and result: `npm test`，266 passed / 0 failed / 1 skipped；build ID `20260806030537013-d7fe711275344872a488644d0c3bb98d`。
- Last startup command and result: `node scripts/start.mjs ... --port 4173`，PID 27164，启动成功。
- Automated test environment: Node integration/unit tests；隔离 Chrome 当前阻塞。
- Highest evidence level reached: 现有服务链路 E2；API 交互已自动运行，但用户可见 UI 尚未达到 E3/E4。
- Evidence locations: `ACCEPTANCE.md`；侧车 unit/API/native integration tests；Factory 当前会话测试日志。
- Runtime, console or network errors: 缺失浏览器模块问题已修复；当前静态模块均返回 200。
- Started processes and cleanup result: 本地服务 PID 27164 为用户访问保留；失败的 agent-browser session 已关闭。
- Remaining verification: 当前构建真实渲染、按钮状态、技能流程、叠加层和响应式 E4。

## Next Actions

1. 修正客户端完整 capability locator。
2. 实现 ITR 成对字段原子编辑合同。
3. 实现技能帧流程和几何叠加，并补充纯逻辑测试。
4. 进入阶段 3 最小垂直闭环并恢复 E3/E4 浏览器验证。

## Recovery Checkpoint

恢复本项目时，必须先读取：

1. `PROJECT_CHARTER.md`
2. `CURRENT_STATE.md`
3. `DECISIONS.md`
4. 当前阶段相关的需求、架构和验收文件

恢复后先复述目标、阶段、约束、未知项和下一步，再继续修改。
