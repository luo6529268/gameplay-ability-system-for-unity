# Current State

> 这是会话恢复和上下文压缩后的首要读取文件。只记录当前有效状态，不记录完整历史。

## Snapshot

- Updated: 2026-08-06
- Brief maturity: `Validated`
- Requirement gate: `Ready`
- Project phase: 阶段 6，技能管理、lossless 结构编辑、Canvas、SVG Flow 与 DAT wait 轴已完成 E4/E5 验收
- Active task: 当前定义范围完成，等待用户决定下一阶段
- Active requirement IDs: REQ-001 至 REQ-015
- Current branch or workspace: `feat/dat-skill-flow-editor` / `Tools/DatSkillFlowWeb`
- Current build ID or revision: `20260806172742780-4037ab3a29ef4617ba7386f804ae3c1b`
- Open P0-P2 issues: 无已知代码缺陷。
- Next required action: 等待项目所有者确认当前交付；确认后再决定是否进入新的需求阶段，当前无需继续启动服务或浏览器。

## Completed

- 接入真实 `data.txt`、DAT、BMP、项目会话、编辑、预览、保存和关闭 API。
- Native preview 明确限制为 Naruto OID 2。
- 修复 `project-client.js` 缺失于浏览器静态白名单导致页面永远停在初始状态的问题。
- 补齐 session `dirty` 响应合同。
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
- 完成服务器签发的完整 field locator，包含 frame occurrence、block type/index 和 field occurrence；客户端不再构造 `frameId:key`。
- 完成 `catchingact` / `caughtact` 严格双 int32 capability，一次请求、一次 revision、一次 value-span patch，并保留原始字节。
- 增加重复 frame、重复 ITR、重复字段、pair 非法输入、pair rollback 和 HTTP pair edit 回归测试。
- 完成 `next` / `hit_*` 技能流程图纯函数，保留最后重复帧、分支、自环、循环、0/负数/999/缺失目标。
- 完成 `itr`、`bdy`、`opoint`、`wpoint`、`bpoint`、`cpoint` 几何投影、镜像坐标和 topmost hit-test 纯函数。
- 将 flow/overlay 浏览器模块加入安全静态 build allowlist，并补充 4 项纯函数回归测试。
- 完成 GPT 视觉方向对应的顶部状态、左侧技能/flow、中间 Native preview、右侧 inspector 和底部时间轴布局。
- 完成技能新建/编辑、真实 flow/timeline 联动、六类 overlay、Canvas hit-test、完整块 inspector、ITR pair 双输入和会话编辑。
- 完成 1440×900、1024×768、390×844 三档响应式布局，均无水平溢出。
- 新增 `一键启动.cmd` 和安全启动脚本：保留既有测试副本、随机回环端口、服务就绪后打开浏览器，显式 `-ResetWorkspace` 才重建副本。
- 修复 preview 乱序响应覆盖和不支持对象切换问题。
- 独立 headless Edge E4 已验证技能、20 帧流程/时间轴、预览控制、overlay、字段编辑、dirty/revision 和三档 viewport。
- Native preview 主实体固定使用权威 slot 0；非法、越界和重复 slot 在服务边界拒绝。
- 未应用草稿跨帧、block 和 preview 重渲染保留；无效草稿不可提交，存在草稿时禁止覆盖 DAT。
- skill/edit/save 使用独占 busy 状态、按钮文本和重复提交保护。
- Preview 使用单飞 latest-wins 调度，快速选择 300→301→302 只发起 2 次请求并最终保持 302。
- 完成 VAL-008 E5：草稿和会话应用不写盘；显式保存产生恢复备份；重启后字段值恢复且状态为已保存。
- 完成 Phase 6 技能管理：按当前 OID 隔离的复制、确认删除、相邻移动、一次 CAS 和选择保持。
- 完成 frame/block lossless 结构事务：完整 CST span 模板复制/新建/删除、严格回滚、capability 全轮换和 50,000 总限额。
- 将 Native preview 成功纳入 edit commit 边界；preview/view 失败不会留下隐藏 revision 或 capability 轮换。
- 完成 Canvas move、四角 resize、1/4px 网格、方向键/Shift 微调、Esc 取消和 batch 单 revision。
- 完成 SVG Flow 真节点/真实字段边、已有 frame 重定向、unresolved/cycle 保留和 `max(1, wait)` 视觉时间轴。
- 修复 edit busy 后 Flow 永久只读，以及 `dataset.oid` 字符串导致对象重开显示 OID 0 的真实浏览器缺陷。
- 最终完整测试 296 项：295 通过、0 失败、1 跳过。
- release build E4/E5：latest-wins 最终 frame 303/occurrence 225/slot 0；技能、结构、Canvas、Flow、wait 轴、signed scalar/pair、busy lock、三档 viewport 和零 console/error 均通过。
- release E5：保存 revision 6 后恢复备份/hash 存在；服务重启后 sidecar 技能、frame 589、3 个 BDY、`hit_j:302`、`dvx=-1` 恢复。

## In Progress

- 无。REQ-001 至 REQ-015 均已达到要求的证据级别。

## Blocked

- 无。

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
- `[Observed]` 六类块的现有字段 capability、严格 pair 和完整 locator 已通过自动测试与 E4 编辑验收；缺失字段继续只读，不创建默认 capability。
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

- Last build command and result: `npm test`，295 passed / 0 failed / 1 skipped；build ID `20260806172742780-4037ab3a29ef4617ba7386f804ae3c1b`。
- Last startup command and result: 使用固定 release manifest、随机 loopback 端口和 LocalAppData 隔离 workspace 启动成功；同一浏览器跨服务重启完成恢复验证。
- Automated test environment: Node integration/unit tests、独立 headless Edge/CDP、LocalAppData 隔离 DAT 副本。
- Highest evidence level reached: 用户可见编辑器核心流程 E4；显式 DAT 覆盖、恢复备份和重启持久化达到 E5。
- Evidence locations: `ACCEPTANCE.md`、`artifacts/acceptance-20260806-4037ab3a/`、Factory 当前会话浏览器/测试日志。
- Runtime, console or network errors: release 页面 `errors` 和 `console` 均为空；Preview latest-wins 最终状态一致。
- Started processes and cleanup result: 自有 Edge、服务 Node、CDP `19269`、临时 workspace/profile 均为 0；未触碰来源不明浏览器进程。
- Remaining verification: 无当前范围内必需验证。

## Next Actions

1. 等待项目所有者确认当前交付并定义下一阶段目标。
2. 若新增用户可见能力，先建立对应 REQ/VAL，再按 DAT/CST/`ntsd_cpp` 权威边界实现。

## Recovery Checkpoint

恢复本项目时，必须先读取：

1. `PROJECT_CHARTER.md`
2. `CURRENT_STATE.md`
3. `DECISIONS.md`
4. 当前阶段相关的需求、架构和验收文件

恢复后先复述目标、阶段、约束、未知项和下一步，再继续修改。
