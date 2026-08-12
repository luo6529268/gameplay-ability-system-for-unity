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
| REQ-012 | 手工技能复制、删除和排序一次 CAS 保存并可重启恢复 | VAL-012 | E4 | Historical pure contracts plus release Edge CAS interactions | Superseded by REQ-017 |
| REQ-013 | frame/block 模板式结构事务保持非目标字节并可安全保存恢复 | VAL-013 | E5 | Unit/integration rollback tests plus release save/restart | Passed |
| REQ-014 | Canvas move/resize/keyboard 以单 revision 原子更新几何 | VAL-014 | E4 | Pure geometry tests plus real pointer/keyboard Edge interactions | Passed |
| REQ-015 | SVG Flow 只重定向已有字段，时间轴按 DAT wait 视觉比例展开 | VAL-015 | E4 | Flow/timeline contracts plus release Edge interaction | Passed |
| REQ-016 | 一键启动明确选择仓库正式项目或 LocalAppData 测试副本 | VAL-016 | E4 | Launcher contract, PTY interaction and both workspace modes | Passed |
| REQ-017 | DAT 自动入口、精确 hit_* 目标首帧与可点击跨技能链接 | VAL-017 | E4 | Pure graph tests plus real Naruto browser workflow | Passed |
| REQ-018 | 桌面左/中/右区域通过两条分隔条安全调宽 | VAL-018 | E4 | Pure layout tests plus real pointer/keyboard/resize browser workflow | Passed |
| REQ-019 | Native Trace 区分主体结束、分身释放确认和投射物完成 | VAL-019 | E3 | DTO/lineage tests plus real F300/Frame 263 CLI/service trace and 1440×900 browser screenshots | Passed |
| REQ-020 | Canvas 拖动设置 P1/P2 Native 起始站位 | VAL-024 | E4 | Pointer/contract tests, real Native coordinate injection, and user Canvas confirmation | Pending E4 |

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

### VAL-016: 正式项目与测试副本启动选择

- Linked requirements: REQ-016
- Purpose: 防止一键启动静默落入测试副本，并确保正式项目和测试副本的写入边界可辨识。
- Preconditions: Windows ConsoleHost、Node 24、仓库 Config 和 LocalAppData 可用。
- Command or procedure: 双击或无 `-Mode` 启动并选择正式/测试/取消；分别以 `-Mode Project`、`-Mode Test` 非交互启动；验证 workspace 参数、提示、重置冲突和服务就绪顺序。
- Build ID or revision: `20260806231404869-63dc4e37fba448e6bfd698ca9493c473`
- Evidence level: E4
- Expected result: 正式模式精确使用仓库根 workspace 并警告真实 DAT 可写；测试模式保留隔离副本；取消不启动；正式模式不能重置；两种模式均不生成演示技能。
- Actual result: PowerShell 5.1 parser、UTF-8 BOM 和 2 项 launcher 聚焦合同通过；完整套件 297 tests 中 296 passed / 0 failed / 1 skipped。真实 PTY 显示中文正式/测试/取消选择且 `0` 无构建退出；`-Mode Project` 启动参数精确使用仓库根 workspace 并显示真实 DAT 警告，`-Mode Test` 精确使用 LocalAppData 副本；正式模式与 `-ResetWorkspace` 冲突在服务启动前被拒绝。
- Evidence location: `scripts/start-local.ps1`、`tests/unit/launcher-contract.test.ts`、本次 Factory 会话终端日志。
- Environment: Windows 10 / PowerShell 5.1
- Started processes: 两个顺序 launcher/Node 验证服务和一个隔离 tuistory PTY session；未打开浏览器。
- Cleanup result: Passed；本轮正式/测试服务、父 PowerShell、PTY session 和 tuistory daemon 均已停止，未触碰用户原有测试服务。
- Status: Passed
- Known limitations: 启动模式只决定 workspace 安全边界；入口列表由 REQ-017 的当前 DAT 自动分析负责。

### VAL-017: DAT 自动入口与跨技能边界

- Linked requirements: REQ-002、REQ-003、REQ-017
- Purpose: 验证正式 Naruto DAT 在无 sidecar 时自动生成状态/技能入口，并阻止 `hit_*` 将其他技能完整吞入当前流程。
- Preconditions: 正式仓库 Naruto DAT 可读；`.dat-skill-flow/skills.json` 缺失或使用隔离 sidecar fixture。
- Command or procedure: 运行标签投影、入口识别、Flow 和 sidecar 聚焦测试；启动正式模式浏览器，检查 `standing · F0` 与 `rasenganshuriken · F300`；选择 standing 并点击 `hit_Uj` 目标卡；验证 sidecar 别名只改变显示。
- Build ID or revision: `20260807022745217-fe7793893aac4349b461aef35a668a32`
- Evidence level: E4
- Expected result: 连续同标题段只产生一个入口；同标题不连续段保持独立；`hit_Uj:300` 的首帧为 300；standing Flow 展开 `next`，但 F300 作为可点击叶节点；sidecar 缺失/无效不清空入口且不改变 DAT。
- Actual result: 完整套件 300 tests 中 299 passed / 0 failed / 1 skipped。正式 Naruto 无 sidecar 自动显示 86 个入口；standing Flow 只展开 0→1→2→3，并显示 `hit_Uj → F300 rasenganshuriken` 目标卡；真实点击后选择、预览和 Flow 均切换到 F300。临时 sidecar 别名保存后重启恢复，DAT SHA-256 前后均为 `0493F5F76F08A363366A4C748DA97DF7ADF0F594F1264EE390E2D7AB13DD2AB9`；临时 sidecar 已删除。1440×900、1024×768、390×844 均为 86 个入口且无水平溢出；最终 build 的 Edge errors/console 为空。
- Evidence location: `src/client/skill-entries.ts`、`src/client/skill-flow.ts`、`src/server/project-skill-service.ts`、相关 unit/integration tests 和本次 Factory Edge/CDP/终端日志。
- Environment: Windows 10 / Node 24 / Edge
- Started processes: 顺序 loopback Node 服务、独立 headless Edge/CDP 与 Temp profile；未使用用户浏览器 profile。
- Cleanup result: Passed；最终 Node 服务、Edge profile 对应进程、CDP 9231 和临时 profile 均为 0/不存在；正式 sidecar 不存在；external dist manifest 已恢复为 `20260806232034560-c9ee0125ea734d98a762056770e672a7`，快照前后的 build/backup 名称集合一致。
- Status: Passed
- Known limitations: 自动名称严格来自 DAT frame 标题；中文别名仍需 sidecar 显式提供。

### VAL-018: 桌面三栏拖动调宽

- Linked requirements: REQ-018
- Purpose: 验证两条桌面分隔条的 pointer、键盘、Esc、resize clamp、ARIA 和移动端回退合同。
- Preconditions: 正式 Naruto 项目服务和隔离无头 Edge/CDP 可用。
- Command or procedure: 在 1440×900 与 1024×768 真实拖动左右 separator，执行 ArrowLeft 和拖动中 Esc，在 pointer capture 期间缩小 viewport；切换 390×844 并点击移动属性标签；读取栏宽、ARIA、overflow、console 和 errors。
- Build ID or revision: `20260807072119677-07b2c1f4bdd840ffbb52bb59b644d69e`（浏览器），`20260807072730170-45381c691244494182d27963d1440e09`（最终完整回归入口）
- Evidence level: E4
- Expected result: 左/右栏保持边界，中栏不低于 360/420px；pointer capture 和 Esc 正确清理；resize 中断拖动并重新 clamp；移动端隐藏 separator 且标签页无回归；页面无水平溢出。
- Actual result: 1440×900 双拖动后为 360/728/340px；1024×768 极限拖动为 412/360/240px；900px 拖动中 resize 后为 288/360/240px 且 capture/dragging 已清理；ArrowLeft 后左栏 280px，Esc 恢复 280px。390×844 的 separator 均为 `display:none`，属性标签切换成功。所有视口页面 overflow 为 0，console/errors 为空。最终套件 310 tests 中 309 passed / 0 failed / 1 skipped；首次最终运行遇到随机 Fetch 禁用端口和并发 manifest 瞬时 ENOENT，清理本轮验证进程后原样重跑通过。
- Evidence location: `src/client/panel-layout.ts`、`src/client/main.ts`、`src/client/styles.css`、`tests/unit/panel-layout.test.ts`、client contract tests，以及本次 Factory 无头 Edge/CDP/终端日志。
- Environment: Windows 10 / Node 24 / Edge 151 headless
- Started processes: 随机 loopback Node 正式项目服务、独立无头 Edge/CDP 12947 与 Temp profile；未使用用户浏览器 profile。
- Cleanup result: Passed；本轮正式项目 Node、可见/无头 Edge、CDP 和 Temp profile 已清理；用户既有 LocalAppData test-workspace 服务未触碰；external dist 已恢复到任务前快照。
- Status: Passed
- Known limitations: 调整结果只保留在当前页面会话，不写 localStorage；≤850px 继续使用移动标签页而非拖动。

### VAL-019: Native 技能 Trace 分类结束与投射物尾迹

- Linked requirements: REQ-019
- Purpose: 验证技能主体、opoint 分身和武器/投射物不使用同一个错误结束条件。
- Preconditions: `ntsd_cpp` runner 可运行；F300 分身路径和 Frame 263 投射物路径可取得逐 tick entity 输出。
- Command or procedure: 从技能已成功触发语义启动 Native Trace；检查 root 回 idle 的 `actorSkillEnded`；检查 F300 的角色 child 只记录释放确认和首个有效快照；检查 Frame 263 的武器 child 在 root idle 后继续记录飞行、落地/碰撞或失效；验证 slot 复用不会混淆 lineage。
- Build ID or revision: `20260807122548278-e9b45dbba5944cc7942d1feca662b133`
- Evidence level: E3
- Expected result: 网页主体进度在 root idle 停止；分身成功释放可见但不等待 AI；投射物继续到权威完成；无法完成时明确 `timeout`/`persistent`，不伪造 `traceComplete`。
- Actual result: 网页侧已按 catalog type 生成 root/actor/clone/projectile/unknown 分类，记录 lineage、spawn/despawn、rootSkillEnded、projectile landed/persistent；按 Native tick 中的 OID 加载对应 DAT frame/range/BMP，并由 renderer 按 OID 选择资源。最终套件 313 项中 312 passed / 0 failed / 1 skipped。真实服务链路已确认 F300 的 OID 33/204/216 资源分别为 203/116/83 帧，Frame 263 的两个 OID 121 weapon lineage 均以 `landed` 完成。隔离浏览器在 1440×900 下实际显示 F300 主体/分身/效果、Frame 263 主体结束状态和预览联动；播放过程中无 console/errors。
- Evidence location: `src/server/native-preview-trace.ts`、`src/server/project-dat-contract.ts`、`src/server/project-dat-service.ts`、`src/client/preview-renderer.ts`、`tests/unit/native-preview-trace.test.ts`、`artifacts/acceptance-20260807-e3/f300-desktop-1440.png`、`artifacts/acceptance-20260807-e3/f300-playing-1440.png`、`artifacts/acceptance-20260807-e3/frame263-desktop-1440.png`、`artifacts/acceptance-20260807-e3/frame263-playing-1440.png`。
- Environment: Windows 10 / Node 24 / C++ Native CLI / isolated headless Chrome via loopback CDP
- Started processes: Test-mode loopback Node service和独立临时 Chrome profile；未使用用户浏览器 profile。
- Cleanup result: Passed；测试服务、CDP、临时 Chrome profile 和 agent-browser session 均已清理。
- Status: Passed
- Known limitations: 当前 runner 仍固定 `startFrame + ticks`，不支持 per-tick input；分类依赖 catalog 对应 OID，未知 OID 明确标记为 `unknown` 并使用 fallback。

### VAL-020: 基础状态上下文与完整动作融合

- Linked requirements: REQ-002、REQ-003、REQ-017
- Purpose: 验证基础状态不会因多个 `hit_*` 路线重复铺满左栏，并确认内部阶段只在证据充分时折叠到完整动作。
- Preconditions: 当前 DAT projection 含 standing/walking/running 状态段、`next` 链和 `hit_*` 跳转来源。
- Command or procedure: 构建入口目录并分别检查基础上下文、完整动作和全部 Frame；运行共享内部阶段、直接基础路线以及真实预览入口选择的自动化回归。
- Build ID or revision: `20260811034854877-dbcee6f9cfde4d6b8294265c5b3e1b4d`
- Evidence level: E2
- Expected result: 基础状态每种上下文只显示一项；完整动作显示全部真实入口路线；没有独立路线的内部目标折叠为动作内阶段；共享目标可归属多个动作；有直接基础路线的目标不得被吞并；全部 Frame 保留逐帧检查。
- Actual result: `skill-entries.ts` 已输出 `baseContexts`、`routes`、`actionRole`、`internalStages` 和 `parentStartFrames`；客户端三页签按上下文、根动作和原始 Frame 分别渲染，并显示入口路线与内部阶段详情。动作结构中的起点和内部阶段现为可点击按钮；内部阶段选择从父动作真实入口开始，使用当前 Native Trace 定位来源 Frame，并逐层追加物理输入。Native CLI 定向验证中，`F271 -> hit_a -> F355 -> hit_d -> F356` 分别在 Tick 15、16、17 出现。完整套件 375 项中 374 passed / 0 failed / 1 skipped；内部阶段与动作聚合聚焦测试 20 项全部通过。
- Evidence location: `src/client/skill-entries.ts`、`src/client/complete-action-selection.ts`、`src/client/main.ts`、`index.html`、`src/client/styles.css`、`tests/unit/skill-entries.test.ts`、`tests/unit/complete-action-selection.test.ts`、`tests/unit/client-project-contract.test.ts`、本次 Native CLI Trace 输出。
- Environment: Windows / PowerShell / Node 24
- Started processes: 仅顺序 Node build/test 子进程；未启动浏览器或本地预览服务。
- Cleanup result: Passed；测试进程已正常退出，未创建浏览器实例；构建生成物按项目约定保留且未清理。
- Status: Pending E4；源码、自动化和 Native CLI 定向验证已通过，正式项目浏览器点击/视觉验收待用户运行一键启动确认。
- Known limitations: 需要碰撞对象、抓取关系、目标选择或跨 DAT 变身等额外运行时前置条件的分支，仍须相应 scenario/transform runtime 合同；缺少真实前置条件时界面只报告未到达，不会伪造目标 Frame。正式浏览器视觉交互仍待用户侧一键启动确认。

### VAL-021: 完整动作入口与结束边界

- Linked requirements: REQ-004、REQ-011、REQ-017、REQ-019
- Purpose: 验证组合输入准备不会在真实动作入口之前结束回放，并验证补丁零等待入口和未命中入口的状态合同。
- Preconditions: 当前完整动作目录可提供所选根动作拥有的有效 Frame；Native Trace 包含 root slot 0。
- Command or procedure: 运行 `F0 → F110 → F0 → F347`、零等待 `F235 → F236` 和未命中入口的聚焦测试；通过正式服务对 immNarutodr F347、阿斯玛 F240、二尾 F235、水月 F250 运行 120 Tick Native Trace；运行完整 `npm test`。
- Build ID or revision: `20260811051216032-67dc7ec75f9d46bbab121e892e507e45`
- Evidence level: E2
- Expected result: 动作入口前没有 completion；入口首帧被跳过时通过所属后继 Frame 记录真实开始；未命中返回 `entry-not-reached`；客户端不裁到动作开始之前。
- Actual result: 聚焦测试 11/11 通过；完整套件 379 项中 378 passed / 0 failed / 1 skipped。真实服务中 immNarutodr 在 Tick 18 进入 F347，随后运行 F348–F389 到 Tick 120，没有再使用 Tick 17 的准备 idle；二尾 F235 在 Tick 15 开始、Tick 23 结束；水月 F250 在 Tick 15 开始、主体 Tick 29 结束、尾迹 Tick 39 结束；阿斯玛 F240 在 Tick 15 开始并按 DAT 持续循环 F242/F243，正确显示 `timeout` 而非伪报完成。
- Evidence location: `src/server/native-preview-trace.ts`、`src/server/project-dat-service.ts`、`src/client/project-client.ts`、`src/client/main.ts`、`tests/unit/native-preview-trace.test.ts`、`tests/unit/project-client.test.ts` 和本次正式服务 Trace 输出。
- Environment: Windows / PowerShell / Node 24 / workspace Native CLI / NTSD 2.4.1 runtime resources
- Started processes: 两个顺序 loopback Node 验证服务；视觉阶段只创建一个 in-app Browser 测试标签，没有创建第二个浏览器实例。
- Cleanup result: Passed；两个验证服务均已停止；浏览器导航被本地权限策略拒绝后没有重试或切换浏览器，测试标签/实例已 finalize；未触碰用户普通 Chrome/Edge。
- Status: Pending E4；源码、自动化和真实 Native 服务 Trace 已通过，Web Canvas 视觉播放待用户运行一键启动确认。
- Known limitations: 需要碰撞、目标、抓取或跨 DAT 变身才能进入的分支，仍可能报告 `entry-not-reached`；这类场景需要单独补齐运行前置条件，不能由动作生命周期层伪造。

### VAL-022: 补丁包 opoint 完整目录与投掷实体

- Linked requirements: REQ-011、REQ-019
- Purpose: 验证补丁角色的 opoint 能解析同 package 中基础 `data.txt` 不存在的 OID，并把真实投掷实体交给 Canvas 资源链路。
- Preconditions: `仙人鸣人—完全修炼` 包、NTSD 2.4.1 运行资源、workspace Native CLI 可读。
- Command or procedure: 从 OID 70 `immNarutodr` 的 F327 真实输入入口运行 120 Tick；断言 F343/F346、OID 466 生成、速度/位移、type 3 和 `rasenhandjian.bmp`；运行完整 `npm test`。
- Build ID or revision: `20260811064417029-9d9cb31234734a139b8e6157ac0f4d15`
- Evidence level: E3
- Expected result: F343 的两个蓄力实体和 F346 的投掷实体均使用 OID 466；投掷实体按 DAT `dvx=15` 前移，并有可绘制资源。
- Actual result: package overlay 6/6 DAT 成功加载，总 catalog `loaded=142, failed=0`；根实体到达 F343/F346，F346 在 Tick 86 生成 slot 52 / OID 466，首个快照 `v.x=15`，下一快照 X 前移；Native `render_resources` 返回 type 3、F0 pic 70 和 `rasenhandjian.bmp`。完整测试 380 项中 379 passed / 0 failed / 1 skipped。
- Evidence location: `native/dat_preview_cli.cpp`、`src/server/project-dat-service.ts`、`tests/integration/project-api.test.ts`、`artifacts/native-regression-20260811-throw-overlay/`。
- Environment: Windows / PowerShell / Node 24 / workspace Native CLI / NTSD 2.4.1 runtime resources
- Started processes: 顺序 Native CLI 与 Node build/test 子进程；未启动新浏览器。
- Cleanup result: Passed；测试进程已自然退出，未结束用户 Chrome/Edge；生成的 Native JSON 验收物按要求保留。
- Status: Pending E4；Native/API/资源合同已验证，但本轮没有新的浏览器 Canvas 视觉验收，不宣称视觉已通过。
- Known limitations: 只覆盖当前 package 的目录；跨 package 依赖必须有显式数据模型，不会按 OID 在全补丁库中猜测匹配。

### VAL-023: Data Changer 补丁 DAT 格式识别与正式服务 OID 466 链路

- Linked requirements: REQ-011、REQ-019
- Purpose: 防止 Data Changer 加密封套中的普通文本被误当成 DAT 明文字段，并确保真实补丁包目录经 `ProjectDatService` 进入 Native 后仍能生成 opoint 实体。
- Preconditions: `仙人鸣人—完全修炼` 包、NTSD 2.4.1 运行资源和 workspace Native CLI 可读。
- Command or procedure: 使用真实 123 字节 Data Changer banner 做单元/集成回归；通过正式 `ProjectDatService` 打开 OID 70，从 F327 运行 120 Tick，并核对 Trace、实体快照及 OID 466 的 BMP capability 哈希；运行完整 `npm test` 和 `git diff --check`。
- Build ID or revision: `20260811074840342-60868528b9bc4ea59dc5cd6fac8e2ecb`
- Evidence level: E3
- Expected result: 补丁包 6 个 DAT 均以正确明文交给 Native；F343 生成两个蓄力 OID 466，F346 生成投掷 OID 466；投掷物使用补丁包资源并延长播放尾迹。
- Actual result: 根动作 Tick 18 进入 F327、Tick 97 结束；OID 466 在 Tick 76 生成 slot 50/51，在 Tick 86 生成 slot 52，首帧为 F1/pic 71、`v.x=15`，随后 X 前移并持续到 Tick 120。`immNarutodr.bmp`、`rasenhandjian.bmp`、`rasenhandjian-fa.bmp` 的服务资源 SHA-256 与补丁源文件完全一致。完整测试 381 项中 380 passed / 0 failed / 1 skipped。
- Evidence location: `src/server/project-dat-service.ts`、`tests/unit/project-dat-preview.test.ts`、`tests/integration/project-api.test.ts` 及本轮正式服务 Trace 输出。
- Environment: Windows / PowerShell / Node 24 / workspace Native CLI / NTSD 2.4.1 runtime resources
- Started processes: 仅顺序 Node build/test 与 Native CLI 子进程；未启动新的浏览器实例。
- Cleanup result: Passed；测试与 Native CLI 子进程均自然退出，未结束用户 Chrome/Edge。
- Status: Pending E4；Native/API/资源链路已验证，但当前修复尚未由用户重启一键启动后在 Web Canvas 目视确认。
- Known limitations: 真实 Canvas 视觉结果仍需用户侧重启当前旧服务后确认；跨 package 依赖仍必须显式建模，不按全补丁库 OID 猜测。

### VAL-024: 双角色起始站位拖动与 Native 重放

- Linked requirements: REQ-020
- Purpose: 验证站位拖动改变真实 Native 世界初始状态，而不是只移动 Canvas 图片。
- Preconditions: P1/P2 sprite range 可解析，当前完整动作已生成，workspace Native CLI 可运行。
- Command or procedure: 开启“拖动站位”，分别命中 P1/P2 精灵并拖动；断言 Canvas delta 映射 X/Z、Y 不变和 stage 边界夹取；检查 API、两级缓存键与六个 CLI 坐标参数；用真实 OID 70 DAT 注入 P1 `(210,0,350)`、P2 `(650,0,470)` 并读取 Tick 0；运行完整 `npm test`。
- Build ID or revision: `20260811081412844-8a4fe6f0b8b240d4931e7a078547bde7`
- Evidence level: E3
- Expected result: P1/P2 松开后重新运行完整动作；Native metadata.initial 与 Tick 0 slot 0/1 坐标等于请求值；相同站位复用缓存，不同站位生成新 Trace；重置恢复默认值。
- Actual result: 真实 CLI metadata 和 Tick 0 均返回 P1 `(210,0,350)`、P2 `(650,0,470)`；定向测试 30/30 通过；完整测试 383 项中 382 passed / 0 failed / 1 skipped。Canvas 目视交互待用户重启一键启动后确认。
- Evidence location: `src/client/main.ts`、`src/client/project-client.ts`、`src/client/preview-renderer.ts`、`src/server/project-dat-service.ts`、`tests/unit/project-client.test.ts`、`tests/unit/preview-renderer.test.ts`、`tests/integration/project-api.test.ts`。
- Environment: Windows / PowerShell / Node 24 / workspace Native CLI
- Started processes: 顺序 Node build/test 和单次 Native CLI 验证；未启动浏览器。
- Cleanup result: Passed；测试与 CLI 子进程均已退出，没有结束用户浏览器。
- Status: Pending E4；源码、API、Native 和自动化已通过，正式 Canvas 拖动仍待用户目视确认。
- Known limitations: 当前只编辑每次预览的起始站位，不提供播放中实时搬运角色，也不写入 DAT/sidecar。

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

- 从 DAT 自动入口选择技能 → 点击跨技能目标 → 播放 → 选择流程帧 → 切换叠加层 → 编辑块字段 → 保存。
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

- Invalid input: sidecar 展示文本超限/含控制字符、字段值无效。
- Partial failure: BMP 缺失、Native preview 失败、侧车写入失败。
- Restart: 技能元数据和已保存 DAT 恢复。
- Reconnection or recovery: 会话失效后明确提示重新载入。

## Release Gate

- Pre-implementation maturity gate passed: Yes
- All P0 requirements validated: Yes
- Required evidence levels satisfied: Yes
- Current build or revision confirmed: Yes，核心历史 release 为 `20260806172742780-4037ab3a29ef4617ba7386f804ae3c1b`；启动模式 build 为 `20260806231404869-63dc4e37fba448e6bfd698ca9493c473`；DAT 自动入口最终 build 为 `20260807022745217-fe7793893aac4349b461aef35a668a32`；三栏拖动浏览器 build 为 `20260807072119677-07b2c1f4bdd840ffbb52bb59b644d69e`。
- User-visible workflows automatically exercised: Yes，核心历史 release 覆盖 REQ-001 至 REQ-015；真实 PTY 和两种 launcher 服务覆盖 REQ-016；最终 Edge 正式 Naruto 流程覆盖 REQ-017；无头 Edge 的 pointer/keyboard/Esc/resize/mobile 流程覆盖 REQ-018。
- Runtime errors and failed resources reviewed: Yes，最终 `errors`/`console` 为空，未发现资源失败或未处理异常。
- Self-started test processes cleaned up: Yes，Node 服务、Edge 根进程/子进程和临时 profile 均为 0。
- Known failures documented: Yes；当前完整套件无失败，1 个自动测试跳过项为既有环境性 skip。
- No unapproved scope changes: Yes
- Reproduction and deployment instructions available: Partial
- Final owner approval: Pending
