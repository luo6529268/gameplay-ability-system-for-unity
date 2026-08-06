# Acceptance and Validation

## Validation Vocabulary

`Not Started` · `Running` · `Passed` · `Failed` · `Blocked` · `Not Applicable`

## Evidence Levels

- `E1 Build`：代码能够编译、构建或通过静态检查。
- `E2 Service`：应用或服务成功启动，资源和健康检查可访问。
- `E3 Render`：当前构建在真实运行环境中产生正确的用户可见输出。
- `E4 Interaction`：关键输入、操作和状态变化经过自动化运行验证。
- `E5 Acceptance`：需求的成功、失败、恢复和必要人工验收均满足。

用户可见功能至少达到 `E4` 才能标记 `Passed`。构建成功或 HTTP 成功不能单独作为功能通过证据。

## Pre-Implementation Maturity Gate

- Original user brief preserved: Yes，见 `PROJECT_CHARTER.md`。
- Input maturity classified: `Execution Ready`
- Mission and primary workflow described: Yes
- In Scope, Out of Scope and minimum vertical slice defined: Yes
- High-impact questions confirmed, explicitly delegated or blocked: Yes
- Recommended defaults recorded without being mislabelled as confirmed: Yes
- Observable acceptance dimensions defined: Yes
- Scale mode selected: `Standard`
- Gate status: `Ready`

## Traceability Matrix

| Requirement ID | Acceptance criterion | Validation ID | Required level | Evidence | Status |
|---|---|---|---|---|---|
| REQ-001 | 当前技能、预览、属性、时间轴和保存状态在一个编辑器工作区中清晰可见 | VAL-001 | E4 | Pending | Not Started |
| REQ-002 | 技能元数据重启后恢复且 DAT 指纹不变 | VAL-002 | E4 | Service-level sidecar/CAS tests passed; UI restart evidence pending | Running |
| REQ-003 | 流程节点选择会同步定位预览、帧和检查器 | VAL-003 | E4 | Pending | Not Started |
| REQ-004 | 单角色真实 BMP 预览可播放、暂停、单步、缩放和适应窗口 | VAL-004 | E4 | Pending | Not Started |
| REQ-005 | 六类 DAT 块叠加可独立切换和选择 | VAL-005 | E4 | Pending | Not Started |
| REQ-006 | 块字段编辑后修订、脏状态和叠加位置同步变化 | VAL-006 | E4 | Pending | Not Started |
| REQ-007 | 控件默认、悬停、按下、选中、禁用和加载状态可观察 | VAL-007 | E4 | Pending | Not Started |
| REQ-008 | 会话修改与覆盖 DAT 分离，离开时保护未保存修改 | VAL-008 | E5 | Pending | Not Started |
| REQ-009 | 三种 viewport 均能完成核心流程 | VAL-009 | E4 | Pending | Not Started |
| REQ-010 | 当前构建由隔离环境自动运行并保存渲染与交互证据 | VAL-010 | E4 | Browser environment blocked by Windows commit pressure | Blocked |
| REQ-011 | UI 未创造 DAT 或 Native preview 运行语义 | VAL-011 | E4 | Existing API and authority tests; final review pending | Running |

## Validation Entry

### VAL-001: 专业编辑器工作区

- Linked requirements: REQ-001
- Purpose: 验证四区信息架构和状态层级。
- Preconditions: Naruto 项目成功载入。
- Command or procedure: 启动当前构建，捕获 1440×900 页面，选择技能和帧。
- Build ID or revision: Pending
- Evidence level: E4
- Expected result: 当前技能、帧、角色、检查器、时间轴和保存状态清晰可见。
- Actual result: Pending
- Evidence location: Pending
- Environment: Isolated browser
- Started processes: Pending
- Cleanup result: Pending
- Status: Not Started
- Known limitations: Native preview 当前仅支持 OID 2。

### VAL-002: 技能侧车持久化

- Linked requirements: REQ-002
- Purpose: 验证技能名称和起始帧安全持久化。
- Preconditions: 测试工作区和已知 DAT 指纹。
- Command or procedure: 新建技能，重启服务，重新载入，比较侧车内容和 DAT 指纹。
- Build ID or revision: `20260806030537013-d7fe711275344872a488644d0c3bb98d`
- Evidence level: E4
- Expected result: 技能恢复，DAT 字节未变化。
- Actual result: Sidecar schema、UTF-8/大小边界、固定路径、CAS 冲突、native 安全目录创建和 DAT documentId 隔离已通过 unit/API/native integration tests；真实 UI 重启流程尚未验证。
- Evidence location: `tests/unit/project-skill-service.test.ts`、`tests/integration/safe-api.test.ts`、`tests/integration/safe-native-file.test.ts`、Factory 当前会话测试日志。
- Environment: Temporary workspace
- Started processes: Pending
- Cleanup result: Pending
- Status: Running
- Known limitations: 侧车 Git 策略尚未决定。

### VAL-003: 技能帧流程联动

- Linked requirements: REQ-003
- Purpose: 验证真实 `next` 和 `hit_*` 关系及跨面板同步。
- Preconditions: 包含分支或循环的测试 DAT。
- Command or procedure: 选择技能，依次点击线性节点和跳转节点。
- Build ID or revision: Pending
- Evidence level: E4
- Expected result: 流程、时间轴、预览和检查器定位到同一帧。
- Actual result: Pending
- Evidence location: Pending
- Environment: Isolated browser
- Started processes: Pending
- Cleanup result: Pending
- Status: Not Started
- Known limitations: 不自动命名技能。

### VAL-004: 单角色预览交互

- Linked requirements: REQ-004
- Purpose: 验证真实 BMP、Native Tick 和预览控制。
- Preconditions: Naruto DAT、BMP 和 preview CLI 可用。
- Command or procedure: 播放、暂停、单步、循环、缩放和适应窗口。
- Build ID or revision: Pending
- Evidence level: E4
- Expected result: 角色清晰可见，帧和方向同步，控件状态正确。
- Actual result: Pending
- Evidence location: Pending
- Environment: Isolated browser
- Started processes: Pending
- Cleanup result: Pending
- Status: Not Started
- Known limitations: 单角色场景。

### VAL-005: DAT 块几何叠加

- Linked requirements: REQ-005
- Purpose: 验证块几何与当前精灵坐标一致。
- Preconditions: 当前帧包含目标块类型。
- Command or procedure: 分别切换 `bdy`、`itr`、`opoint`、`wpoint`、`bpoint`、`cpoint`。
- Build ID or revision: Pending
- Evidence level: E4
- Expected result: 矩形和定位点按图例显示；关闭层后消失；选择后检查器同步。
- Actual result: Pending
- Evidence location: Pending
- Environment: Isolated browser
- Started processes: Pending
- Cleanup result: Pending
- Status: Not Started
- Known limitations: 不验证复杂运行结果。

### VAL-006: DAT 块字段编辑

- Linked requirements: REQ-006
- Purpose: 验证所有现有块的 capability 编辑合同。
- Preconditions: 测试 DAT 包含全部块类型。
- Command or procedure: 每类块修改一个现有几何字段，检查 revision、dirty 和叠加位置。
- Build ID or revision: Pending
- Evidence level: E4
- Expected result: 修改仅进入会话，画面同步，未保存状态明确。
- Actual result: Pending
- Evidence location: Pending
- Environment: Isolated browser and integration fixture
- Started processes: Pending
- Cleanup result: Pending
- Status: Not Started
- Known limitations: 不新增 DAT 中缺失的字段。

### VAL-007: 控件反馈状态

- Linked requirements: REQ-007
- Purpose: 验证操作反馈可辨识。
- Preconditions: 页面载入。
- Command or procedure: 捕获默认、悬停、按下、选中、禁用、焦点和加载状态。
- Build ID or revision: Pending
- Evidence level: E4
- Expected result: 状态不只依靠颜色，且无误导反馈。
- Actual result: Pending
- Evidence location: Pending
- Environment: Isolated browser
- Started processes: Pending
- Cleanup result: Pending
- Status: Not Started
- Known limitations: Pending

### VAL-008: 会话与覆盖安全

- Linked requirements: REQ-008
- Purpose: 验证会话修改和文件覆盖边界。
- Preconditions: 临时 DAT 副本。
- Command or procedure: 编辑、切换、取消离开、保存、重启、比较恢复文件。
- Build ID or revision: Pending
- Evidence level: E5
- Expected result: 未显式保存不改文件；保存成功后状态和文件一致；恢复信息可用。
- Actual result: Pending
- Evidence location: Pending
- Environment: Temporary workspace
- Started processes: Pending
- Cleanup result: Pending
- Status: Not Started
- Known limitations: 最终覆盖流程需要用户认可。

### VAL-009: 自适应核心流程

- Linked requirements: REQ-009
- Purpose: 验证桌面、中屏和窄屏可操作性。
- Preconditions: 页面载入。
- Command or procedure: 在 1440×900、1024×768 和 390×844 viewport 完成选择、播放和编辑。
- Build ID or revision: Pending
- Evidence level: E4
- Expected result: 无水平溢出，核心操作可见且可达。
- Actual result: Pending
- Evidence location: Pending
- Environment: Isolated browser
- Started processes: Pending
- Cleanup result: Pending
- Status: Not Started
- Known limitations: Pending

### VAL-010: 当前构建自动运行

- Linked requirements: REQ-010
- Purpose: 防止旧进程、缓存或缺失模块产生伪通过。
- Preconditions: 隔离浏览器可启动。
- Command or procedure: 构建、固定 build ID、启动服务、检查所有模块和控制台、执行关键交互、保存截图、清理进程。
- Build ID or revision: Latest verified service build before Phase 1
- Evidence level: E4
- Expected result: 当前构建完整运行，无资源 404 和未处理异常。
- Actual result: API 和模块静态验证通过；Chrome 自动化因 Windows 页面文件/提交压力无法启动。
- Evidence location: Test logs in Factory session; future artifacts pending.
- Environment: Windows 10
- Started processes: Local Node server PID 27164
- Cleanup result: Server intentionally retained for user access.
- Status: Blocked
- Known limitations: 系统检测到大量 Chrome 进程，自动化 Chrome 无法载入 DLL。

### VAL-011: 权威边界审查

- Linked requirements: REQ-011
- Purpose: 验证 UI 不创造战斗语义。
- Preconditions: 当前阶段代码完成。
- Command or procedure: 审查 DAT 字段来源、Native Tick 数据流和所有叠加计算。
- Build ID or revision: Pending
- Evidence level: E4
- Expected result: 所有数据可追踪到 DAT 或 `ntsd_cpp`。
- Actual result: Pending
- Evidence location: Pending
- Environment: Code review and runtime tests
- Started processes: None
- Cleanup result: Not Applicable
- Status: Running
- Known limitations: 复杂运行语义明确延期。

## Test Layers

### Static

- Types: Node 24 strip TypeScript syntax check and contract tests.
- Lint: Repository has no dedicated lint script.
- Format: `git diff --check`.
- Dependency or schema checks: Build manifest and strict JSON/API schemas.

### Unit and Component

- 技能流程图算法。
- 几何叠加坐标变换。
- 检查器分组和字段 capability 映射。
- 交互状态 reducer。

### Integration

- 技能侧车 API。
- 项目会话、编辑、预览、资源和关闭。
- 安全保存和恢复。
- 浏览器静态模块白名单。

### End-to-End

- 创建技能 → 选择技能 → 播放 → 选择流程帧 → 切换叠加层 → 编辑块字段 → 保存。
- Runtime or browser environment: Isolated Chrome-compatible browser.
- Visual evidence: Three target viewports.
- Interaction evidence: State and screenshot sequence.
- Console, log or network errors: Must be empty or documented.

### Performance and Reliability

- Target: 同步操作一个渲染帧内反馈；异步操作立即进入加载态。
- Measurement method: Browser timing and visible state capture.
- Baseline: 现有 Naruto 首次打开约 8–13 秒。
- Result: Pending.

### Failure and Recovery

- Invalid input: 技能起始帧不存在、字段值无效。
- Partial failure: BMP 缺失、Native preview 失败、侧车写入失败。
- Restart: 技能元数据和已保存 DAT 恢复。
- Reconnection or recovery: 会话失效后明确提示重新载入。

## Release Gate

- Pre-implementation maturity gate passed: Yes
- All P0 requirements validated: No
- Required evidence levels satisfied: No
- Current build or revision confirmed: Pending
- User-visible workflows automatically exercised: No
- Runtime errors and failed resources reviewed: Pending
- Self-started test processes cleaned up: Pending
- Known failures documented: Yes
- No unapproved scope changes: Yes
- Reproduction and deployment instructions available: Partial
- Final owner approval: Pending
