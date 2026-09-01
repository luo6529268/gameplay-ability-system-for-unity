# Current State

> 这是会话恢复和上下文压缩后的首要读取文件。只记录当前有效状态，不记录完整历史。

## 2026-08-30 主预览表现更新

- 状态：`RUNTIME_PENDING`。主编辑器预览已增加 `30Hz 原始 / 60Hz 平滑 / 120Hz 平滑` 表现选择，默认 120Hz；30Hz battle/Native Tick、DAT wait、frame、输入、碰撞和对象生命周期均未改变。
- 参考：Codex 会话 `019ff015-6f9c-7652-8c40-034b476b1c7a` 的 NTSD 2.8 本地 `render_snapshot` / `presentation_interpolation`。只移植表现合同，不移植 2.8 的 `0..998` Frame 范围或战斗规则。
- 连续性：只对相邻 Tick、同 lineage、holder/link/target 连续且 velocity 支持位移的实体使用精确 `x/y/z` 表现 delta；传送、slot reuse、新生实体和关系切换保持离散。
- 权威分离：sprite、shadow 与 camera 使用同一 presentation Tick；DAT overlay、站位拖动、几何编辑、Frame/pic/facing/lifecycle 继续使用 current Native authority Tick。暂停/编辑态始终显示离散 Tick。
- 外观：Canvas 可在宽预览区放大到 1040px，增加场景边框/阴影与表现频率 HUD；坐标轴只在站位/几何编辑时显示。
- 证据：build `20260830084617618-18ef901e469444d9b80e355a62838458`；focused `23/23`；全部 unit `315 passed / 0 failed / 1 skipped`；非构建 integration `78/78`；manifest/server integration `25/25`；Change Ledger validator PASS（114 records / 32 governed diffs）。
- 未完成：浏览器 Canvas E4。当前会话的 localhost 浏览器权限此前被拒绝，本次未重试或绕过，因此不能声明实际 60/120Hz 视觉观感已通过。

## Snapshot

- Updated: 2026-08-11
- Brief maturity: `Validated`
- Requirement gate: `Ready`
- Project phase: 阶段 8；Native Trace/多 OID 资源投影保持完成，左侧“基础状态 / 完整动作 / 全部 Frame”信息架构及保守动作融合已完成源码、测试和构建，新的浏览器 E4 待用户侧确认
- Active task: 补丁包完整 DAT 目录覆盖与 opoint 多实体预览
- Active requirement IDs: REQ-001 至 REQ-020
- Current branch or workspace: `feat/dat-skill-flow-editor` / `Tools/DatSkillFlowWeb`
- Current build ID or revision: `20260811081412844-8a4fe6f0b8b240d4931e7a078547bde7`
- Open P0-P2 issues: 无已确认代码缺陷；基础状态/完整动作融合已取得自动化证据，但本轮未启动浏览器，因此尚未取得正式项目 E4 视觉证据。
- Next required action: 用户直接运行一键启动确认新布局；重点检查基础状态只显示 standing/walking/running 上下文、完整动作显示多入口路线与动作内阶段、全部 Frame 仍可逐帧定位。

## Completed

- 新增 P1/P2 起始站位拖动模式：Canvas 精确命中两名角色，横向映射 Native X、纵向映射地面 Z、保持 Y 并夹取 stage 边界；松开后以新站位重新运行当前完整动作，切换技能沿用站位，切换角色或“重置”恢复 Native 默认值。真实 OID 70 CLI 验证 P1 `(210,0,350)`、P2 `(650,0,470)` 与 Tick 0 完全一致；完整测试 383 项：382 通过、0 失败、1 跳过。浏览器 E4 待用户确认。
- 修复补丁 DAT 格式识别：Data Changer 的 123 字节加密封套含有 `file` 文本，旧逻辑因此把加密 DAT 误判为明文，导致 package-local overlay 在正式服务链路中无法被 Native 解析。现在按真实 frame/sprite range 结构选择明文或解密文档；正式 `ProjectDatService` 已验证 immNarutodr F327 在 Tick 76/86 生成 OID 466，Tick 86 投掷实体 `v.x=15`，播放尾迹延续到 Tick 120，三组关键资源哈希与补丁包 BMP 一致。完整测试 381 项：380 通过、0 失败、1 跳过。
- 左栏默认收敛为“基础状态 / 完整动作 / 全部 Frame”三类导航和统一筛选；standing/walking/running 作为上下文合并展示，不再按每个 `hit_*` 重复生成技能行。
- 完整动作保留每条真实入口路线，并仅把没有直接基础入口、没有外部入口且可由动作链归属的 `hit_*` 目标折叠为动作内阶段；共享阶段可同时显示在多个父动作中，存在直接基础路线的目标继续保持独立动作。
- 单 Frame 选择不再直接调用 raw frame preview：先按真实 DAT `next` 链选择最早完整动作入口，Native 回放后再定位目标 Frame；F210→F211→F212 有聚焦回归，找不到入口时拒绝伪造起点。
- 底部 DAT wait 视觉轴替换为根实体真实 Native Tick/Frame 分段；连续相同 Frame 合并，离开后回访生成新段，root 缺失时不伪造 Frame。
- 中间预览明确显示完整动作标题/入口/当前 Frame，仍是不可由玩家控制的战斗场景；右侧帧参数检查器保持原有 capability 和编辑合同。
- 动作结构中的起点和内部 `hit_*` 阶段均为可点击按钮；点击内部阶段时先从父动作真实入口回放，再依据当前 Native Trace 的来源 Frame 逐层追加输入并定位目标，不把内部 Frame 伪造成预览起点。Naruto F271 的嵌套链已验证为 `standing F0 -> F271 -> F355 -> F356`。
- 本轮完整测试 375 项：374 通过、0 失败、1 跳过；内部阶段与动作聚合聚焦测试 20/20 通过，最终 build `20260811034854877-dbcee6f9cfde4d6b8294265c5b3e1b4d`。
- 启动阶段从真实 DAT 派生并预热 86 个 Native 技能场景，同时安全读取并缓存 45 个当前战斗资源；浏览器只在服务完成准备后打开。
- `CppNativeDatPreviewRunner`、项目会话和客户端分别按根 DAT、补丁包目录内所有 DAT 字节、revision 与完整 preview intent 做有界 LRU/并发去重；编辑或依赖 DAT 改变后自动进入新 key，不复用旧结果。
- 每次技能/Frame 预览不再同步刷新 `data.txt`；显式 catalog 读取仍执行新鲜度检查并保持外部变更失效语义。
- C++ `render_resources` 直接提供 OID 33/121/205 等实体的 object type、frame center 和 sprite range；服务端不再为同一 Trace 重读辅助 DAT 猜测渲染资源。
- BMP capability 在项目响应中保持 opaque；项目打开不再逐图同步探测，首次 `/api/assets/:id` 仍通过已授权 root 做 handle-safe 读取，随后只在当前会话内复用字节。
- 真实链路基线从项目打开 75.7 秒、切换 1.4–1.5 秒，降为准备后项目打开 15.2ms、F265 首次切换 29.4ms、重复切换 3.9ms；预热资源响应 2.8ms。
- Native preview 的 C++ 源码/对象来自 `J:\QQFile\NTSD2.4\ntsd_cpp`，但运行数据根显式固定为 `J:\QQFile\NTSD 2.4.1`；CLI 按该版本的 `data\data.txt` 全量加载 137 个 object DAT 并保留原始 type。响应仅投影本次 Trace 实际出现的 OID 资源，避免把完整 catalog 重复传给浏览器。
- 补丁包会话将当前 package 的全部 OID/type/DAT 作为 package-local overlay 传入 Native，同 OID 覆盖基础 `data.txt`，包内独有 OID 也可由 opoint 生成。`immNarutodr F327 -> F343/F346 -> OID 466` 已验证：F346 投掷实体在 Tick 86 出现，初始 `dvx=15`，并解析 `rasenhandjian.bmp`。
- F271 已验证真实链路：Naruto F271/F272 生成 type-3 OID 205，OID 205 从 F99 推进到 F325+，随后生成 6 个进入可绘制帧的 OID 33 分身；不再落入旧 type-2 的 F69/F70 循环。
- 一键启动的只读 asset workspace 与 Native `--game-root` 均使用 `J:\QQFile\NTSD 2.4.1`，不得从 C++ 源码目录的父级推导运行 DAT/BMP。
- 完整自动测试 380 项：379 通过、0 失败、1 跳过；真实补丁包 F327/OID 466 回归、F210/F211/F212、F265/OID 33、F271/OID 205 -> OID 33 Native 定向验收通过。

- 接入真实 `data.txt`、DAT、BMP、项目会话、编辑、预览、保存和关闭 API。
- Native preview 支持基础 `data.txt` 中所有 type 0 角色和当前补丁 package 内的 type 0 角色；跨 package 依赖仍不会隐式混入。
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
- 新增 `一键启动.cmd` 和安全启动脚本：保留既有测试副本、随机回环端口、服务就绪后打开浏览器。
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
- 历史 Phase 6 完整测试 297 项：296 通过、0 失败、1 跳过。
- release build E4/E5：latest-wins 最终 frame 303/occurrence 225/slot 0；技能、结构、Canvas、Flow、wait 轴、signed scalar/pair、busy lock、三档 viewport 和零 console/error 均通过。
- release E5：保存 revision 6 后恢复备份/hash 存在；服务重启后 sidecar 技能、frame 589、3 个 BDY、`hit_j:302`、`dvx=-1` 恢复。
- 完成 REQ-016：无参数一键启动显示正式/测试/取消选择；显式 `-Mode Project|Test` 支持自动化；正式模式使用仓库根 workspace，测试模式保留 LocalAppData 副本，测试重置不能作用于正式项目。
- 完成 REQ-017：frame 标题经 CST/projection/project DTO 透传；当前 DAT 自动派生标题段和非零 `hit_*` 精确入口。
- Flow 的 `next` 继续展开；指向其他入口的 `hit_*` 显示为可点击叶节点，不吞并目标技能流程。
- sidecar 只保存显示名称、分组、顺序、置顶、隐藏和备注；旧 `name` 可读迁移，missing/invalid 不清空 DAT 自动入口。
- 正式 Naruto 无 sidecar 自动显示 86 个入口；standing 展开 0→1→2→3，点击 `hit_Uj` F300 后切换到 rasenganshuriken。
- 临时 sidecar 别名保存并重启恢复；Naruto DAT SHA-256 前后均为 `0493F5F76F08A363366A4C748DA97DF7ADF0F594F1264EE390E2D7AB13DD2AB9`，验收 sidecar 已删除。
- 1440×900、1024×768、390×844 均保持 86 个入口、零水平溢出；页面 error/console 为空。
- 最终完整测试 300 项：299 通过、0 失败、1 跳过；build `20260807022745217-fe7793893aac4349b461aef35a668a32`。
- 完成 REQ-018：桌面左/中和中/右边界均提供 6px 可访问 separator，支持 pointer capture、方向键/Shift、Esc 恢复和容器 resize clamp；≤850px 继续使用移动标签页。
- 真实无头 Edge 验收：1440 双拖动为 360/728/340px；1024 极限拖动为 412/360/240px；900 拖动中 resize 保持中栏 360px；390 移动标签页无 separator，全部 viewport 水平溢出为 0。
- 三栏拖动最终完整测试 310 项：309 通过、0 失败、1 跳过；build `20260807072730170-45381c691244494182d27963d1440e09`。
- 实现 REQ-019 网页侧 Trace DTO：按 catalog `type` 映射 root/actor/clone/projectile/unknown，记录逐 tick lineage、spawn/despawn、rootSkillEnded、分身释放、投射物落地/失效和 `timeout`/`persistent`。
- 服务端按 Native 输出中的 OID 加载对应 DAT frame/range/BMP capability；客户端按 OID 选择真实资源，不再把非 OID 2 实体固定绘制为 fallback。
- 客户端主体进度在 root 结束处停止，播放边界继续覆盖投射物尾迹；slot reuse 在新 lineage 前显式结束旧 lineage。
- 修复完整动作提前结束：输入准备的 `F110 → F0` 不再触发主体结束；服务端用完整动作 Frame 归属记录真实入口 Tick/Frame，支持零等待首帧被跳过；未进入目标动作返回 `entry-not-reached`；客户端拒绝动作开始前的进度和播放终点。
- 新增 `tests/unit/native-preview-trace.test.ts`，覆盖 raw object type、root/clone/projectile 分类和 slot reuse。
- 当前完整测试 313 项：312 通过、0 失败、1 跳过；build `20260807122548278-e9b45dbba5944cc7942d1feca662b133`。
- 真实 Native 服务链路已验证：F300 返回 OID 33/204/216 的 203/116/83 帧资源并分类为 clone/projectile；Frame 263 返回 OID 121 武器，两个 lineage 均以 `landed` 完成。
- 浏览器 E3 已验证：隔离 headless Chrome 在 1440×900 下实际显示 F300 主体/分身/效果和 Frame 263 预览，播放推进到后续帧，console/errors 均为空；截图保存在 `artifacts/acceptance-20260807-e3/`。

## In Progress

- REQ-019 Native 技能 Trace：网页 DTO、真实 CLI/服务链路和浏览器 E3 视觉验收均已通过。

## Blocked

- 新 UI 的浏览器 E4 视觉验收被本地浏览器对 `http://127.0.0.1:10042/` 的安全权限拒绝；按单实例约束未重试、未切换浏览器或绕过权限。源码、构建、自动测试和服务预热不受此阻塞。

## Confirmed Constraints

- 使用 `C:\Users\Logan\.codex\templates\large-goal`，不重新设计项目模板。
- Standard 模式。
- 不依赖 Unity 运行。
- 入口和首帧由当前 DAT frame 标题段与跳转关系派生，不凭空生成中文技能语义。
- `.dat-skill-flow/skills.json` 只保存纯展示覆盖，不能创建或删除入口。
- 第一阶段使用单角色预览。
- 第一阶段包含 `itr`、`bdy`、`opoint`、`wpoint`、`bpoint`、`cpoint` 查看、编辑和几何叠加。
- 复杂对象生成、武器联动、抓取和命中结果语义延后。
- DAT 和 `ntsd_cpp` 是数据与预览权威，UI 不创造战斗规则。
- 用户可见功能未达到 E4 不得标记通过。
- 临时浏览器可以启动但必须受控：先检查并优先复用可用 profile；不可用时确认空闲后再关闭；每次使用结束关闭完整进程树并确认临时 profile 残留为 0，不得结束用户普通 Chrome/Edge。
- Trace 不调用 UI 键盘模拟；从技能已成功触发语义开始。
- 网页按 C++ 对应 OID DAT/catalog 的 `rawObjectType` 分类：角色/分身只确认 opoint 释放和首个有效快照，不等待 AI；武器/投射物继续处理轨迹、落地、碰撞和失效。

## Unknowns and Assumptions

- `[Unknown]` `ntsd_cpp` 后续为复杂块运行语义提供哪些新输出。
- `[Observed]` 六类块的现有字段 capability、严格 pair 和完整 locator 已通过自动测试与 E4 编辑验收；缺失字段继续只读，不创建默认 capability。
- `[Recommended Default]` 固定高对比叠加色，第一阶段不提供颜色配置。
- `[User-Authorized Default]` 桌面四区布局，中屏收起侧栏，窄屏标签页。

## Recent Decisions

- DEC-001: 使用 Standard large-goal 模式。
- DEC-002: 已被 DEC-009 取代；历史版本由名称和起始帧定义手工技能。
- DEC-003: sidecar 只保存项目展示元数据，不拥有 DAT 入口。
- DEC-004: 第一阶段使用单角色预览。
- DEC-005: 第一阶段纳入全部现有 DAT 块查看、编辑和几何叠加。
- DEC-006: GPT 图作为修正后的视觉方向稿。
- DEC-007: itr 成对动作字段原子编辑。
- DEC-008: 一键启动默认交互选择 `Project`/`Test`；非交互必须显式 `-Mode`，`-ResetWorkspace` 仅限测试模式。
- DEC-009: 使用混合自动入口；同标题且 frame ID 连续的段合并，非零 `hit_*` 的有效目标为精确入口。
- DEC-010: 跨入口 `hit_*` 是可点击叶节点；`next` 继续展开当前流程。
- DEC-012: 复用临时浏览器并限制实例，使用后清零进程。
- DEC-013: Trace 按派生对象类别分别结束；分身不等待 AI，武器/投射物继续到权威完成。
- DEC-015: 左侧三类导航；单 Frame 只定位完整动作回放；底部使用真实 Native Frame 时间线。
- DEC-018: 完整动作进入所属 Frame 链后才允许判定结束；入口未命中不得伪报完成。

## Validation

- Last build command and result: PowerShell 通过 Node 的 npm CLI 运行完整 `npm test`，378 passed / 0 failed / 1 skipped；聚焦生命周期/客户端边界测试 11/11 通过；build ID `20260811051216032-67dc7ec75f9d46bbab121e892e507e45`。
- Last startup command and result: `start-local.ps1 -Mode Project -NoBuild -NoBrowser` 成功；86/86 Native 预览、45/45 资源完成，Native 预览阶段 27.520 秒，服务就绪于随机回环地址 `http://127.0.0.1:10042/`。
- Automated test environment: Node integration/unit tests、PowerShell 5.1 parser、tuistory PTY、正式/测试 launcher 服务、既有独立 Edge/CDP 证据。
- Highest evidence level reached: 历史编辑器核心流程 E4、显式 DAT 覆盖/恢复/重启 E5；本轮三类导航改动仅达到构建、自动测试和服务就绪，新的浏览器 E4 未完成。
- Evidence locations: `ACCEPTANCE.md`、`artifacts/acceptance-20260806-4037ab3a/`、Factory 当前会话浏览器/测试日志。
- Runtime, console or network errors: 最终自动入口页面 `errors` 和 `console` 均为空；standing→F300 后预览、选择和 Flow 一致。
- Started processes and cleanup result: 本轮真实补丁验证服务监听端口 43821，完成后由命令超时边界停止；最终视觉验收服务监听端口 43822，并在浏览器权限拒绝后主动停止。只创建一个 in-app Browser 测试标签，导航被权限策略拒绝后立即 finalize；未启动或结束用户普通 Chrome/Edge。
- External dist restoration: 已恢复 build `20260806232034560-c9ee0125ea734d98a762056770e672a7` 和 manifest SHA-256 `322446D69A55B81D531FDBF183C4AE67F324A13B7BF2B9EA877E85E80E271F53`；build/backup 名称集合与任务前快照完全一致。
- Remaining verification: 用户通过一键启动在真实 Web Canvas 中确认三类导航、F212 的 F210 完整入口、F271 分身、Native Frame 时间线以及 immNarutodr F347 的完整播放；本轮没有新的浏览器截图，不得把新 UI 或本次生命周期修复标记为 E4 通过。

## Next Actions

1. 用户运行一键启动，对三类导航和完整动作时间线做真实 Canvas 视觉确认。
2. 在“全部 Frame”选择 F212，确认从 F210/F211 初始化并定位到 F212；再检查 F265 和 F271。
3. 若浏览器权限恢复，只复用一个实例补齐 E4 截图并在结束后清理。

## Recovery Checkpoint

恢复本项目时，必须先读取：

1. `PROJECT_CHARTER.md`
2. `CURRENT_STATE.md`
3. `DECISIONS.md`
4. 当前阶段相关的需求、架构和验收文件

恢复后先复述目标、阶段、约束、未知项和下一步，再继续修改。
