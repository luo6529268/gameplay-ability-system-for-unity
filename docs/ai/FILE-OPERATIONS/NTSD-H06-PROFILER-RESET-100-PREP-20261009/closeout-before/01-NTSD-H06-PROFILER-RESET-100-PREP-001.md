<!-- CHANGE-RECORD
id: NTSD-H06-PROFILER-RESET-100-PREP-001
status: CODE_WRITTEN
code-path: Assets/NTSD/Scripts/Test/Editor/H06CollectorPreparationEditorTests.cs
authority: 2026-10-09 current user explicitly authorizes Profiler cache reset and 100-object diagnostic preparation only
evidence: docs/ai/TASKS/NTSD-H06-PROFILER-RESET-100-PREP-20261009.md
-->
# H06 Profiler清理与小规模诊断准备

脚本前登记。原状：H06四场景一致性有效、混合collector234events未过；旧Profiler frame6导出报>2GB，已有失败产物保留；原Editoridle、Menu clean。只读execute_code受命令长度限制，改用唯一新Editor准备测试，不改第三方/生产。

准确符号：H06CollectorPreparationEditorTests.ClearAuthorizedProfilerHistory、Capture100AllocationCallsites及私有fixture/settings/save/export助手。需求、owner/容量/失效、关闭、不变量、最小验证、风险、回滚见独立Task。清空内存历史不可恢复，用户已明确批准；没有磁盘删除。

唯一代码路径为新增准备测试，未经用户允许不修优化逻辑、不切默认、不更改千人验收。输入/RNG/checksum/33ms/正常pass及publication/透明segment/fail-closed不变。诊断hook/Profiler额外开销不当生产收益。
Operation：docs/ai/FILE-OPERATIONS/NTSD-H06-PROFILER-RESET-100-PREP-20261009/RECORD.md。尚未编译/执行，不称0GC通过。

CODE_WRITTEN：唯一新增准备测试已写入；两个Explicit入口及本地fixture/settings/导出助手。现有Admission/生产/Suite未改。尚未编译或执行。源码SHA C1B113575F0E85E9B86B8D24297574D516F2B82A92CE72AA0F504B955664ABA5。

准备首轮校正：preflight Begin后Assert污染计数（29），移到End后；marker调用栈22/call另独立有效，FindEffect71/40B。修订只影响新准备测试，不重跑旧四场景/长窗。原件已保存revision-01-assertion-contamination。
