# NTSD-OPTIMIZATION-SETPASS-VISUAL-INPUT-AUDIT-20261008
状态：PLANNED。类型：只读资源引用/图片头取证与文档顶部追加；不修改脚本或资源。
Task：[本次只读取证](../../TASKS/NTSD-OPTIMIZATION-SETPASS-VISUAL-INPUT-AUDIT-20261008.md)。Change ID：不适用，无新脚本改动。
需求/授权：既有六项优化目标及唯一总表维护；用户要求优先SetPass。上轮提出“本局完整视觉依赖＋预算内共享纹理绑定”的实施权限尚未得到人类确认，自动Goal续跑不改变专项授权。
执行者：/root。工作目录：I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity。
开始：2026-10-08T13:42:09.6427957Z。HEAD：06a5fe9c78681bdb8844e59804f30a9480cb221d。staged为空；原dirty全部保留。
原因：确认现有千人OID1案例的显式DAT引用集合/源sheet尺寸是否存在预算或尺寸前置，避免把本帧3纹理当完整依赖或无条件强制图集。不是新增性能候选或新的优化完成批次。
准确现有写域（仅顶部追加，历史全文逐字节保留）：
- Assets/NTSD/Docs/battle-optimization-progress-tracker.md
- Assets/NTSD/Docs/CODEX-CURRENT-HANDOFF.md
- docs/ai/STATE.md
- docs/ai/FILE-OPERATIONS/INDEX.md
ABSENT新增：
- docs/ai/TASKS/NTSD-OPTIMIZATION-SETPASS-VISUAL-INPUT-AUDIT-20261008.md
- docs/ai/FILE-OPERATIONS/NTSD-OPTIMIZATION-SETPASS-VISUAL-INPUT-AUDIT-20261008/RECORD.md、before-manifest-01.json、before-01四份准确dirty副本
- artifacts/diagnostics/NTSD-OPTIMIZATION-SETPASS-VISUAL-INPUT-AUDIT-20261008/REPORT.md、final-audit-01.json
事前manifest：before-manifest-01.json列出4文档绝对路径/字节/SHA/Git状态/备份目标、48只读输入SHA、13对象原始oid引用与22sheet真实PNG头尺寸。PNG原始RGBA量是估算，不是解码/驻留/atlas/总峰值实测。
恢复：四dirty文档复制到manifest列出的ABSENT before-01目标，Copy-Item不使用Force，复制后核SHA。记录/备份在写域之外；需要回滚时另请批准，只处理本次顶部hunk，不reset/restore用户内容。
计划实际入口：rg/Get-Content/Import-Csv/Get-FileHash；原始oid token取证和PNG前32字节只读，绝不替代生产parser/闭包；apply_patch创建本Record/Task/manifest；New-Item新建before目录/Copy-Item四份；apply_patch先登记INDEX PLANNED，再添加REPORT与三治理文档顶部；48输入/604历史guard/原文尾部字节/HEAD/staged/git diff --check/ChangeLedger核验；创建final-audit并追加实际结果。
禁止：C#/shader、资源/DAT/PNG/meta、Scene/Prefab/importer/ProjectSettings、Q06方法体、Unity/test/Profiler/测量/M0/GPU工具、PERF/ATLAS/EXT1/Mono正文或状态、Git mutation/删除/移动/覆盖旧失败。
限制：显式oid图不是生产语义依赖闭包，不证明模式/融合/隐式规则生成资源齐全、未来1000/1800窗口可用或完整预算通过。SetPass沿用77的1995；六项仍4/6，H07/H11 OPEN，57执行不变，本次不另计优化子批。

## 实际交付（2026-10-08T13:47:11.3927799Z之后）
状态：VERIFIED，仅文档操作和原始引用/PNG头取证；不表示性能或阶段通过。
实际入口：apply_patch创建Record/Task/before-manifest；New-Item创建ABSENT before-01；Copy-Item四准确dirty文档到ABSENT副本（无Force），全部SHA匹配；apply_patch登记INDEX PLANNED，再追加三个治理文档顶部和REPORT。没有C#/资源/Scene/Unity/test/测量/Git mutation。
新事实：13个显式引用注册对象、22张sheet，其中1张3000×800超现有2046尺寸；13DAT匹配正式catalog。完整生产依赖/共同资源/预算仍未证，不将正则提升为生产parser。
2026-10-08T13:47:11.3927799Z：48只读输入保持、原604历史guard保持、四backupSHA和旧全文尾部逐字节保持、HEAD同/staged空；git diff --check exit0，仅10条CRLF提示，无non-warning输出。
2026-10-08T13:47:18.1488160Z：Tools/Validate-ChangeLedger.ps1实际exit0、1373Records、1governed diff仅既有Suite且旧Record覆盖、4395历史warning未清理；未创建新脚本Change。没有编译/Play/GC/FPS/SetPass测量。
本次只读取证输出已交付，下一正式共享绑定实施仍待人类确认；57执行、4/6、H07/H11 OPEN与Goal active保持，不把静态文档包当优化收益/完成或停止边界。
final-audit-01.json在全部实际文档追加后核验48输入/604guard/四原文/备份/HEAD/staged/diff；恢复需要新的明确批准。
