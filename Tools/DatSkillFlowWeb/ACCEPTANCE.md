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
| REQ-001 | 当前技能、预览、属性、时间轴和保存状态在一个编辑器工作区中清晰可见 | VAL-001 | E4 | Final Edge 1440×900 interaction | Passed |
| REQ-002 | 技能元数据重启后恢复且 DAT 指纹不变 | VAL-002 | E4 | Sidecar restart plus DAT isolation tests | Passed |
| REQ-003 | 流程节点选择会同步定位预览、帧和检查器 | VAL-003 | E4 | Release 22-node/80-edge interaction plus graph tests | Passed |
| REQ-004 | 单角色真实 BMP 预览可播放、暂停、单步、缩放和适应窗口 | VAL-004 | E4 | Final Edge preview controls | Passed |
| REQ-005 | 六类 DAT 块叠加可独立切换和选择 | VAL-005 | E4 | Six toggles plus geometry/hit-test tests | Passed |
| REQ-006 | 块字段编辑后修订、脏状态和叠加位置同步变化 | VAL-006 | E4 | Edge BDY edit plus capability/pair tests | Passed |
| REQ-007 | 控件默认、悬停、按下、选中、禁用和加载状态可观察 | VAL-007 | E4 | Final Edge state interaction | Passed |
| REQ-008 | 会话修改与覆盖 DAT 分离，离开时保护未保存修改 | VAL-008 | E5 | Isolated save, backup and restart recovery | Passed |
| REQ-009 | 三种 viewport 均能完成核心流程 | VAL-009 | E4 | 1440/1024/390 Edge interaction | Passed |
| REQ-010 | 当前构建由隔离环境自动运行并保存渲染与交互证据 | VAL-010 | E4 | Release build, isolated Edge, screenshots and zero-error check | Passed |
| REQ-011 | UI 未创造 DAT 或 Native preview 运行语义 | VAL-011 | E4 | DAT/CST contracts plus final UI review | Passed |
| REQ-012 | 技能复制、删除和排序一次 CAS 保存并可重启恢复 | VAL-012 | E4 | Pure contracts plus release Edge CAS interactions | Passed |
| REQ-013 | frame/block 模板式结构事务保持非目标字节并可安全保存恢复 | VAL-013 | E5 | Unit/integration rollback tests plus release save/restart | Passed |
| REQ-014 | Canvas move/resize/keyboard 以单 revision 原子更新几何 | VAL-014 | E4 | Pure geometry tests plus real pointer/keyboard Edge interactions | Passed |
| REQ-015 | SVG Flow 只重定向已有字段，时间轴按 DAT wait 视觉比例展开 | VAL-015 | E4 | Flow/timeline contracts plus release Edge interaction | Passed |

## Validation Entry

### VAL-001: 专业编辑器工作区

- Linked requirements: REQ-001
- Purpose: 验证四区信息架构和状态层级。
- Preconditions: Naruto 项目成功载入。
- Command or procedure: 启动当前构建，捕获 1440×900 页面，选择技能和帧。
- Build ID or revision: `20260806172742780-4037ab3a29ef4617ba7386f804ae3c1b`
- Evidence level: E4
- Expected result: 当前技能、帧、角色、检查器、时间轴和保存状态清晰可见。
- Actual result: 顶部状态、技能/flow、Native preview、检查器和时间轴在同一工作区内联动，Naruto 项目和技能帧 300 成功载入。
- Evidence location: Factory 当前会话最终 Edge 交互日志。
- Environment: Isolated browser
- Started processes: Local launcher service and isolated Edge/CDP.
- Cleanup result: Passed；临时进程在验收后清理。
- Status: Passed
- Known limitations: Native preview 当前仅支持 OID 2。

### VAL-002: 技能侧车持久化

- Linked requirements: REQ-002
- Purpose: 验证技能名称和起始帧安全持久化。
- Preconditions: 测试工作区和已知 DAT 指纹。
- Command or procedure: 新建技能，重启服务，重新载入，比较侧车内容和 DAT 指纹。
- Build ID or revision: `20260806113357950-2549422f83b84f57982d5b291bfa1670`
- Evidence level: E4
- Expected result: 技能恢复，DAT 字节未变化。
- Actual result: 技能由 UI 创建/编辑并在服务重启后恢复；sidecar schema、CAS、native 安全目录和 DAT documentId 隔离测试通过，启动器保留已有测试副本。
- Evidence location: `tests/unit/project-skill-service.test.ts`、`tests/integration/safe-api.test.ts`、`tests/integration/safe-native-file.test.ts`、Factory 当前会话最终 Edge/launcher 日志。
- Environment: Temporary workspace
- Started processes: Local launcher service and isolated Edge/CDP.
- Cleanup result: Passed；测试副本仅在清理阶段移除。
- Status: Passed
- Known limitations: 侧车 Git 策略尚未决定。

### VAL-003: 技能帧流程联动

- Linked requirements: REQ-003
- Purpose: 验证真实 `next` 和 `hit_*` 关系及跨面板同步。
- Preconditions: 包含分支或循环的测试 DAT。
- Command or procedure: 选择技能，依次点击线性节点和跳转节点。
- Build ID or revision: `20260806172742780-4037ab3a29ef4617ba7386f804ae3c1b`
- Evidence level: E4
- Expected result: 流程、时间轴、预览和检查器定位到同一帧。
- Actual result: 起始帧 300 生成 22 个真实 frame 节点和 80 条真实字段边；快速选择 301→302→303 后 flow、预览和检查器最终一致为 frame 303、occurrence 225。
- Evidence location: `tests/unit/skill-flow-overlay.test.ts`、`artifacts/acceptance-20260806-4037ab3a/desktop-frame589-saved.png`、Factory 当前会话 release Edge latest-wins 日志。
- Environment: Isolated browser
- Started processes: Local launcher service and isolated Edge/CDP.
- Cleanup result: Passed。
- Status: Passed
- Known limitations: 不自动命名技能。

### VAL-004: 单角色预览交互

- Linked requirements: REQ-004
- Purpose: 验证真实 BMP、Native Tick 和预览控制。
- Preconditions: Naruto DAT、BMP 和 preview CLI 可用。
- Command or procedure: 播放、暂停、单步、循环、缩放和适应窗口。
- Build ID or revision: `20260806113357950-2549422f83b84f57982d5b291bfa1670`
- Evidence level: E4
- Expected result: 角色清晰可见，帧和方向同步，控件状态正确。
- Actual result: Native BMP 成功显示；播放/暂停、单步、循环、110% 缩放和适应窗口恢复 100% 均通过。
- Evidence location: Factory 当前会话最终 Edge 交互日志。
- Environment: Isolated browser
- Started processes: Local launcher service、Native preview CLI、isolated Edge/CDP.
- Cleanup result: Passed。
- Status: Passed
- Known limitations: 单角色场景。

### VAL-005: DAT 块几何叠加

- Linked requirements: REQ-005
- Purpose: 验证块几何与当前精灵坐标一致。
- Preconditions: 当前帧包含目标块类型。
- Command or procedure: 分别切换 `bdy`、`itr`、`opoint`、`wpoint`、`bpoint`、`cpoint`。
- Build ID or revision: `20260806113357950-2549422f83b84f57982d5b291bfa1670`
- Evidence level: E4
- Expected result: 矩形和定位点按图例显示；关闭层后消失；选择后检查器同步。
- Actual result: 六类按钮可一次全部关闭（0 active）并恢复（6 active）；矩形/点镜像投影和 topmost hit-test 自动测试通过，BDY 块可从检查器选择。
- Evidence location: `tests/unit/skill-flow-overlay.test.ts`、Factory 当前会话最终 Edge 交互日志。
- Environment: Isolated browser
- Started processes: Local launcher service and isolated Edge/CDP.
- Cleanup result: Passed。
- Status: Passed
- Known limitations: 不验证复杂运行结果。

### VAL-006: DAT 块字段编辑

- Linked requirements: REQ-006
- Purpose: 验证所有现有块的 capability 编辑合同。
- Preconditions: 测试 DAT 包含全部块类型。
- Command or procedure: 每类块修改一个现有几何字段，检查 revision、dirty 和叠加位置。
- Build ID or revision: `20260806113357950-2549422f83b84f57982d5b291bfa1670`
- Evidence level: E4
- Expected result: 修改仅进入会话，画面同步，未保存状态明确。
- Actual result: Edge 中修改 BDY x 后 revision 变为 1、dirty 显示“未保存至文件”，未覆盖 DAT；完整 locator、严格 ITR pair 和 lossless patch 自动测试通过。
- Evidence location: `tests/unit/dat-session-service.test.ts`、`tests/unit/gate1a-data-pipeline.test.ts`、`tests/integration/project-api.test.ts`、Factory 当前会话 Edge 日志。
- Environment: Isolated browser and integration fixture
- Started processes: Local launcher service and isolated Edge/CDP.
- Cleanup result: Passed；未执行 DAT 覆盖。
- Status: Passed
- Known limitations: 不新增 DAT 中缺失的字段。

### VAL-007: 控件反馈状态

- Linked requirements: REQ-007
- Purpose: 验证操作反馈可辨识。
- Preconditions: 页面载入。
- Command or procedure: 捕获默认、悬停、按下、选中、禁用、焦点和加载状态。
- Build ID or revision: `20260806113357950-2549422f83b84f57982d5b291bfa1670`
- Evidence level: E4
- Expected result: 状态不只依靠颜色，且无误导反馈。
- Actual result: 加载、禁用对象、播放 aria-pressed、overlay 选中、草稿可应用、dirty 和 revision 状态均可观察，操作不只依靠颜色。
- Evidence location: `index.html`、`src/client/styles.css`、Factory 当前会话最终 Edge 交互日志。
- Environment: Isolated browser
- Started processes: Local launcher service and isolated Edge/CDP.
- Cleanup result: Passed。
- Status: Passed
- Known limitations: 无。

### VAL-008: 会话与覆盖安全

- Linked requirements: REQ-008
- Purpose: 验证会话修改和文件覆盖边界。
- Preconditions: 临时 DAT 副本。
- Command or procedure: 编辑、切换、取消离开、保存、重启、比较恢复文件。
- Build ID or revision: `20260806113357950-2549422f83b84f57982d5b291bfa1670`
- Evidence level: E5
- Expected result: 未显式保存不改文件；保存成功后状态和文件一致；恢复信息可用。
- Actual result: 未应用草稿和已应用会话修改均保持初始 DAT 指纹 `0493F5...2AB9`；显式确认覆盖后指纹变为 `B129B7...BCB0`，恢复备份指纹等于原文件；服务重启后 BDY x=22、dirty=已保存、revision=0。
- Evidence location: Factory 当前会话 E5 Edge、SHA-256、恢复备份和服务重启日志。
- Environment: Temporary workspace
- Started processes: Local launcher service、Native preview CLI、isolated Edge/CDP。
- Cleanup result: 本轮隔离进程与测试副本在最终复核后清理。
- Status: Passed
- Known limitations: 验收仅覆盖 LocalAppData 隔离副本，不覆盖仓库 DAT。

### VAL-009: 自适应核心流程

- Linked requirements: REQ-009
- Purpose: 验证桌面、中屏和窄屏可操作性。
- Preconditions: 页面载入。
- Command or procedure: 在 1440×900、1024×768 和 390×844 viewport 完成选择、播放和编辑。
- Build ID or revision: `20260806172742780-4037ab3a29ef4617ba7386f804ae3c1b`
- Evidence level: E4
- Expected result: 无水平溢出，核心操作可见且可达。
- Actual result: 1440×900、1024×768、390×844 均无页面级水平溢出；1024px 下技能、Flow、Canvas、属性和保存控件可见；390px 下技能、预览、属性、时间轴四个标签逐一激活，关键控件均可达且 `aria-pressed` 正确。
- Evidence location: `artifacts/acceptance-20260806-4037ab3a/desktop-frame589-saved.png`、`medium-1024x768.png`、`mobile-390x844.png` 和 Factory 当前会话 Edge viewport 日志。
- Environment: Isolated browser
- Started processes: Local launcher service and isolated Edge/CDP.
- Cleanup result: Passed。
- Status: Passed
- Known limitations: 窄屏通过顶部标签在四个核心区域间切换。

### VAL-010: 当前构建自动运行

- Linked requirements: REQ-010
- Purpose: 防止旧进程、缓存或缺失模块产生伪通过。
- Preconditions: 隔离浏览器可启动。
- Command or procedure: 构建、固定 build ID、启动服务、检查所有模块和控制台、执行关键交互、保存截图、清理进程。
- Build ID or revision: `20260806172742780-4037ab3a29ef4617ba7386f804ae3c1b`
- Evidence level: E4
- Expected result: 当前构建完整运行，无资源 404 和未处理异常。
- Actual result: `npm test` 生成并固定 release build，296 tests 中 295 passed / 0 failed / 1 skipped。隔离 Edge 重开 Naruto 后仍显示 OID 2；快速选择 301/302/303 后最终一致为 frame 303、occurrence 225、slot 0；signed scalar、integer pair、edit busy/Save lock、三档 viewport 和关键 Phase 6 交互均通过。`agent-browser errors` 与 `console` 无输出，未发现页面未处理错误。
- Evidence location: Factory 当前会话 release build/CDP 日志及 `artifacts/acceptance-20260806-4037ab3a/` 三张截图。
- Environment: Windows 10
- Started processes: Local launcher service and isolated Edge/CDP；未触碰用户原有 4173 服务。
- Cleanup result: Passed；自有 Edge、服务 Node、CDP `19269` 和临时 workspace/profile 均清理到 0，未触碰来源不明浏览器进程。
- Status: Passed
- Known limitations: Native preview 仍仅接入 OID 2；浏览器证据使用 LocalAppData 隔离副本。

### VAL-011: 权威边界审查

- Linked requirements: REQ-011
- Purpose: 验证 UI 不创造战斗语义。
- Preconditions: 当前阶段代码完成。
- Command or procedure: 审查 DAT 字段来源、Native Tick 数据流和所有叠加计算。
- Build ID or revision: `20260806113357950-2549422f83b84f57982d5b291bfa1670`
- Evidence level: E4
- Expected result: 所有数据可追踪到 DAT 或 `ntsd_cpp`。
- Actual result: UI 只消费 server-issued locator、DAT projection、Native Tick 和已测试的 flow/geometry 纯函数；不支持对象禁用，缺失字段不新增，早期重复 occurrence 按既定 runtime 合同只读。
- Evidence location: `ARCHITECTURE.md`、`tests/unit/dat-session-service.test.ts`、`tests/unit/gate1a-data-pipeline.test.ts`、`tests/unit/skill-flow-overlay.test.ts`、最终 code review。
- Environment: Code review and runtime tests
- Started processes: None
- Cleanup result: Not Applicable
- Status: Passed
- Known limitations: 复杂运行语义明确延期。

### VAL-012: 技能管理

- Linked requirements: REQ-012
- Purpose: 验证技能复制、删除和顺序调整不改变 DAT 或 sidecar schema。
- Preconditions: 隔离项目包含至少两个技能。
- Command or procedure: 复制选中技能、上下移动、确认删除、重启服务并比较 sidecar 与 DAT 指纹。
- Build ID or revision: `20260806172742780-4037ab3a29ef4617ba7386f804ae3c1b`
- Evidence level: E4
- Expected result: 每个操作一次 CAS；复制项位于原项后且名称追加“副本”；操作后选择稳定；重启恢复顺序；DAT 指纹不变。
- Actual result: 纯函数测试验证按 OID 隔离、复制名称“副本”、相邻移动和删除选择规则；隔离 Edge 实际创建两个技能，复制当前项、上移一位、确认删除，每次 CAS 后列表和选择稳定。release E5 重启后 OID 2 的 `Release Timeline` sidecar 技能恢复。
- Evidence location: `tests/unit/phase6-pure-contracts.test.ts`、`tests/unit/project-skill-service.test.ts`、Factory 当前会话 release Edge 日志。
- Environment: Temporary workspace and isolated browser
- Started processes: Local launcher service and isolated Edge/CDP.
- Cleanup result: Passed；sidecar 测试副本随临时 workspace 清理。
- Status: Passed
- Known limitations: 不引入技能稳定 ID，选中身份限定于当前 sidecar revision。

### VAL-013: lossless 结构事务

- Linked requirements: REQ-013
- Purpose: 验证 frame/block 模板复制、新建和删除的字节保真、原子性与保存恢复。
- Preconditions: 含完整 frame/block span、注释、未知字段和混合换行的夹具，以及隔离可保存 DAT。
- Command or procedure: 复制 frame 到显式新 ID；模板式新建/复制/删除 block；删除 frame；验证旧 capability 失效、revision、dirty、超限/冲突回滚、保存与重启恢复。
- Build ID or revision: `20260806172742780-4037ab3a29ef4617ba7386f804ae3c1b`
- Evidence level: E5
- Expected result: 仅目标 span 改变；frame 副本只重写 header ID；不修复引用；失败无部分修改；恢复备份仍可用。
- Actual result: 单元/集成测试覆盖完整 span、未知字节/换行保留、capability 轮换、no-op、限额、preview 失败与非法请求全回滚、加密 envelope 和保存重开。release Edge 在 frame 300 上完成模板新建 BDY（2→3）、确认删除（3→2）、复制 BDY（2→3）和完整 frame 复制为 ID 589；安全保存 revision 6 产生恢复备份和目标 hash，服务重启后 frame 589 及 3 个 BDY 恢复、dirty=已保存、revision=0。
- Evidence location: `tests/unit/dat-session-service.test.ts`、`tests/integration/project-api.test.ts`、`tests/unit/phase6-pure-contracts.test.ts`、`artifacts/acceptance-20260806-4037ab3a/desktop-frame589-saved.png` 和 Factory 当前会话 release Edge 日志。
- Environment: Unit/integration fixture and temporary workspace
- Started processes: Local launcher service、Native preview CLI and isolated Edge/CDP.
- Cleanup result: Passed；仅保存 LocalAppData 隔离 DAT，恢复备份与 workspace 在取证后清理。
- Status: Passed
- Known limitations: 不从空白默认模板创建结构。

### VAL-014: Canvas 几何编辑

- Linked requirements: REQ-014
- Purpose: 验证移动、缩放、网格和键盘操作与 capability/batch revision 合同一致。
- Preconditions: 当前 frame 含可编辑点和矩形 block。
- Command or procedure: 普通/镜像方向拖动，四角缩放，切换 4px 网格，方向键与 Shift 微调，Esc 取消。
- Build ID or revision: `20260806172742780-4037ab3a29ef4617ba7386f804ae3c1b`
- Evidence level: E4
- Expected result: 拖动中只显示本地草稿；move 原子更新 x/y；resize 原子更新 x/y/w/h；非法尺寸和缺 capability 不可提交。
- Actual result: 纯函数测试覆盖镜像 move、四角 resize、1/4px snap、Shift+方向键 ±4 和草稿合同。release Edge 在重启后的真实 Canvas 上以 ArrowRight 将 BDY x 21→22（单 revision）；启用 4px 网格后真实 pointer 拖动 SE handle，将 w/h 43/62→51/70（单 revision）；随后开始 move 并按 Esc，revision 保持 2 且 frame/block 锁释放。
- Evidence location: `tests/unit/phase6-pure-contracts.test.ts`、`src/client/canvas-geometry-edit.ts`、Factory 当前会话 release Edge pointer/keyboard 日志。
- Environment: Unit tests and isolated browser
- Started processes: Local launcher service and isolated Edge/CDP.
- Cleanup result: Passed；重启后的临时 Canvas 修改未保存并随 workspace 清理。
- Status: Passed
- Known limitations: 仅编辑 DAT 中已存在且获 capability 的几何字段。

### VAL-015: SVG Flow 与 DAT wait 视觉时间轴

- Linked requirements: REQ-015
- Purpose: 验证真实已有跳转字段的图形表达、重定向和 wait 比例时间轴。
- Preconditions: 当前技能包含分支、循环、unresolved 和不同 wait 值。
- Command or procedure: 选择节点/边；将已有边重定向到已有 frame；检查字段 revision；比较 segment 比例和跨面板选择。
- Build ID or revision: `20260806172742780-4037ab3a29ef4617ba7386f804ae3c1b`
- Evidence level: E4
- Expected result: 图保留边 key、循环与 unresolved；不显示 DAT 中缺失的边；重定向只修改已有 capability；时间轴宽度为 `max(1, wait)` 视觉比例且不标秒或 Native tick。
- Actual result: release Edge 的 `Release Timeline` 从 frame 300 构建 22 个 SVG 节点、80 条真实边和 80 条可编辑已有字段边；wait 前八段视觉单位为 `2,3,1,3,1,2,2,2`。真实选择 `hit_j` 边并重定向到已有 frame 302 后只修改 `hit_j`，`next` 保持 301，单 revision；任意 edit busy 完成后 80 条边恢复可编辑。安全保存和服务重启后 `hit_j: 302` 恢复。
- Evidence location: `tests/unit/phase6-pure-contracts.test.ts`、`tests/unit/skill-flow-overlay.test.ts`、`artifacts/acceptance-20260806-4037ab3a/desktop-frame589-saved.png` 和 Factory 当前会话 release Edge 日志。
- Environment: Unit tests and isolated browser
- Started processes: Local launcher service and isolated Edge/CDP.
- Cleanup result: Passed。
- Status: Passed
- Known limitations: 不创建/删除跳转字段，不推断主分支或真实运行时长。

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
- Result: Naruto 首次打开仍受 Native preview 启动成本影响；结构 capability 生成受 field+structure 50,000 总限额约束，parser/view/Flow 热路径使用线性索引，当前 release 交互无可观察卡死。

### Failure and Recovery

- Invalid input: 技能起始帧不存在、字段值无效。
- Partial failure: BMP 缺失、Native preview 失败、侧车写入失败。
- Restart: 技能元数据和已保存 DAT 恢复。
- Reconnection or recovery: 会话失效后明确提示重新载入。

## Release Gate

- Pre-implementation maturity gate passed: Yes
- All P0 requirements validated: Yes
- Required evidence levels satisfied: Yes
- Current build or revision confirmed: Yes，`20260806172742780-4037ab3a29ef4617ba7386f804ae3c1b`
- User-visible workflows automatically exercised: Yes，release build 已覆盖核心闭环和 REQ-012 至 REQ-015。
- Runtime errors and failed resources reviewed: Yes，最终 `errors`/`console` 为空，未发现资源失败或未处理异常。
- Self-started test processes cleaned up: Yes，Node 服务、Edge 根进程/子进程和临时 profile 均为 0。
- Known failures documented: Yes；最终 release 无已知失败，1 个自动测试跳过项为既有环境性 skip。
- No unapproved scope changes: Yes
- Reproduction and deployment instructions available: Partial
- Final owner approval: Pending
