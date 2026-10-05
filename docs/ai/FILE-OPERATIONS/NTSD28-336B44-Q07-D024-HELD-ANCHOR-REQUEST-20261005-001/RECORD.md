# NTSD28-336B44-Q07-D024-HELD-ANCHOR-REQUEST-20261005-001

状态：`RESTORED`。类型：一次性原Scene请求备份后覆盖、探针消费、终态恢复原字节；本次具名新结果的状态更新。执行者root，本会话既有战斗对齐目标、原Editor空闲可定向Play授权及D-024共用挂点修复的必要消费者验收；不新增删除/清理权限。

Task/Change：`NTSD28-336B44-Q07-D024-VERTICAL-FRACTION-001` / `NTSD28-336B44-Q07-D024-WPOINT-SCENE-PROBE-001`。工作/批准根`I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity`。仅以下逐文件范围：

- `Temp/NTSD28_Q07_AirborneIdleBattlePlay.request.json`：现有requested=false，范围外逐字节备份并核对SHA后覆为`{requested:true,runId:d024-held-anchor-20261005-01}`；已登记探针消费为false；结果DONE/原Editor非Play且同runId时按备份还原。
- `artifacts/diagnostics/NTSD28-336B44-Q07-C023-AIRBORNE-SCENE-PLAY-001/d024-held-anchor-20261005-01.json`：操作前必须不存在，只允许本次新建和进度Save更新，保留终态和失败事实，禁止覆盖旧runId。

备份/manifest在范围外`artifacts/diagnostics/NTSD28-336B44-Q07-D024-HELD-ANCHOR-REQUEST-20261005-001/`。执行前before-manifest含两文件绝对/相对路径、Git状态、存在性、大小、SHA和Scene/formal EXE身份；request-before.json保存现有未跟踪临时字节，不用HEAD。拟执行Python写具名请求→既有探针单次Play两完整Driver tick→MCP确认退出→Python逐字节恢复请求，保存开始/结束时间、执行者PID、工具原件和after-manifest。

hard gate：原Editor已经编译新探针、非Play/非编译/无测试，唯一Battle Scene clean且SHA未变。原Editor连接localhost6401，不启动第二Editor；失败不重试、不保存Scene、不触及其它资源或临时请求。不删除文件、不Git restore/reset/clean。

实际启动：before-manifest/request-before与execution-start已落盘。MCP刷新期间一次连接拒绝，原Unity PID19040与6401监听随后确认仍在，重取同连接原Editor就绪；没有重新启动Editor或测试。Console0error/新DLL晚于脚本/Scene clean/root11/SHA保持后才写具名请求。开始时间、Python PID、MCP原件见execution-start；正在等待同一runId终态。

实际终态：单次PASS/DONE两tick/global5→7，MCP核对已非Play/Scene clean/root11/Console0error。only同runId已消费requested=false且结果DONE时由Python逐字节恢复请求，68字节SHA`7486F5BB3126869EC1D32C6033B68C6A94A19797573BCD0AEE21ED220B985BF1`；结果SHA`6EF6CB7855CFC6F521FF42621B1C1F7E8B2505B362B0D850104DD8A3371740BC`保留。Scene SHA`253B2EBAD322AA6EC18488DFBBF555BB93AD897940A342E4130E77CE778F9010`与before一致。before/execution-start/request-consumed/after-manifest及UTC/PID均在范围外证据文件夹；所有断言退出0，不删除任何文件、不保存Scene、不Git丢弃，不触及其它临时请求或资源。
