# NTSD-OPTIMIZATION-BATCH78-SETPASS-RESOURCE-SCOPE-20261008
状态：PLANNED。类型：只读源码取证的文档追加；无代码/资源/Scene操作。
Task：docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH78-SETPASS-RESOURCE-SCOPE-20261008.md。Change ID：不适用，本包不修改脚本，不把它登记成生产Change。
需求与授权：用户问“为什么不先修复SetPass也就是渲染指令的数量问题”；既有六项优化授权包含取证、方案与唯一进度总表维护。自动Goal继续仅维持既有范围，不授予资源/ATLAS实施权限。用户尚未确认上一回复所提定向放开范围。
执行者：/root；工作目录 I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity。
原因：关闭75上游configSource是否为完整本局依赖的未知项，防止错误裁掉全局逻辑配置；将后续优先级由碰撞微候选转为具名SetPass资源绑定修复准备。
开始：2026-10-08T13:12:31.6299072+00:00；HEAD 06a5fe9c78681bdb8844e59804f30a9480cb221d；现有dirty全部保留。
准确写域：
- Assets/NTSD/Docs/battle-optimization-progress-tracker.md：只在顶部追加当前说明，历史原文完整保留。
- Assets/NTSD/Docs/CODEX-CURRENT-HANDOFF.md：只在顶部追加当前说明，历史原文完整保留。
- docs/ai/STATE.md：只在顶部追加当前说明，历史原文完整保留。
- docs/ai/FILE-OPERATIONS/INDEX.md：只在顶部追加当前说明，历史原文完整保留。
- docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH78-SETPASS-RESOURCE-SCOPE-20261008.md：ABSENT新建本次只读Task。
- docs/ai/FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH78-SETPASS-RESOURCE-SCOPE-20261008/RECORD.md、before-manifest-01.json、before-01四份副本：ABSENT新建Operation/准确副本。
- artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH78-SETPASS-RESOURCE-SCOPE-20261008/REPORT.md、final-audit-01.json：ABSENT新建静态报告及交付审计。
恢复源：before-manifest-01.json列出四份dirty文档的字节/SHA/Git状态和ABSENT备份路径；Copy-Item只复制到经检查不存在的新目标，无Force。备份在目标文档之外，执行前逐份核SHA。若需回滚，仅在另获批准后对自加hunk处理，不restore/reset用户工作树。
计划命令：rg/带行号Get-Content读取具名九脚本；Get-FileHash、git status/diff --cached只读；tools.apply_patch创建本Record/manifest/Task；New-Item创建ABSENT before目录并Copy-Item四份准确dirty副本；核SHA；apply_patch先登记INDEX PLANNED，再追加三治理文档/REPORT；604递归guard+九sourceSHA、文档尾部逐字节相等、git diff --check、未变HEAD/staged核验；apply_patch追加实际结果/最终audit。
禁止：任何C#/shader、Scene/Prefab/资源/importer/ProjectSettings/Gen/Plugins/Server写入、PERF/ATLAS/EXT1/Mono正文变更、活跃Q06方法体、Unity控制/test/Profiler/测量/M0、Git mutation、删除/覆盖历史失败。
未知：本局完整可达依赖数量/所需页数与驻留/过渡峰值尚未测；现有145页2320MiB为75计划值，不能当实际resident。SetPass最新1995未下降。

## 实际交付（2026-10-08T13:19:16.9217734+00:00之后）
状态：VERIFIED，仅限定文档操作与静态事实闭合，不是生产性能/阶段完成。
已执行：tools.apply_patch创建Record/manifest/Task；New-Item在ABSENT before-01创建目录，Copy-Item四个指定dirty文档到指定不存在副本（无Force），副本SHA全部与事前一致；先apply_patch登记INDEX PLANNED，再给原总表/STATE/handoff顶部追加78状态并创建REPORT；STATE自加链接相对路径修正，不触旧正文；最后INDEX顶部追加VERIFIED和本段。实际代码/资源/Scene修改、Unity控制/test/测量、M0/GPU工具及Git mutation均未执行。
新增事实而非重复诊断：九source重扫确认全Catalog.Entries配置来源、全图片Capture/预热、统一atlas入口、逻辑/UI/config/catalog原子发布耦合和中央全局catalog消费者/冷容量。75 snapshot SHA核同，复用77而无新SetPass数字。
2026-10-08T13:19:08.2228042+00:00四副本SHA与原历史尾部逐字节相同、九source保持、HEAD06a5fe9c...同/staged空；递归原guard604/changes[]。git diff --check exit0，只有10条CRLF提示、无non-warning输出，不清理既有换行。
Tools/Validate-ChangeLedger.ps1实际exit0，1373Records、1governed code diff仅前轮Suite且既有Record覆盖、4395历史warning不处理；未新增脚本Change/Ledger实现项。本报告静态事实不代替编译/Play/性能验证。
工具异常：一次ABSENT查询PowerShell数组语法错误，零写入后改为正确@()；一次只读审计将stderr ErrorRecord直接JSON序列化致输出截断/解析失败，零文件操作，后按ToString压缩重新核原handle-independent静态状态。没有因此重启Editor、重复测试或覆盖文件。
原始数值/备份/源码身份/最终文件hash与命令结果摘要：artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH78-SETPASS-RESOURCE-SCOPE-20261008/final-audit-01.json；所有现有dirty与失败证据保留，无删除/移动/恢复/覆写用户内容。受影响文档原文尾部继续逐字节保留，最终audit再次确认；回滚仍需新授权。
阶段保持4/6、H07/H11 OPEN、57执行含本静态包、父关闭0，Goal active；SetPass实施单项需要确认，不把单项权限门扩为整个Goal阻塞，不先开新的碰撞微候选。

