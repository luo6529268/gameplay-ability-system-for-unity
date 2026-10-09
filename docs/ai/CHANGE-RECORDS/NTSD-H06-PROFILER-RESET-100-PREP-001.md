<!-- CHANGE-RECORD
id: NTSD-H06-PROFILER-RESET-100-PREP-001
status: VERIFIED
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

VERIFIED（仅准备/诊断测试）：最终原Editor修订版两个具名入口分别1/1PASS，reset2 e31b17726bf142d4b747b7381b2eabe8；capture 78eebe0088994b8ead7b77c04fd6eaac。原生产/Suite/已有测试未改。最终collect22/22/22、每事件40B，全部66首managed frame FindEffect71；raw9703396B、errors[]、exact settingsEqual、objects/slots0。源码SHA ED0F5AE09BD207762A32B15A6168751AF5A5406D247806D865B204A1CE959798。

实际报告与未验证项：artifacts/diagnostics/NTSD-H06-PROFILER-RESET-100-PREP-20261009/REPORT.md。1192保护0变化，6原dirty备份/HEAD/staged稳，原Menu clean/8roots/idle；原历史删除仅内存，磁盘删除0。FindEffect foreach枚举器分配/装箱为待生产修复核验方向，未证明对象类型/千人234全同源/95GC来源，未运行完整Driver/H07/Android或默认晋升。旧0GC FAIL及H06 BLOCKED保持。Ledger与最终文档校验在收尾记录登记。

最终治理：pwsh7.6.5准确运行Tools/Validate-ChangeLedger.ps1显式RepositoryRoot，exit0/1392Records/10diff paths/0ERROR/4265历史提示；本Change/新增脚本无警告、已准确COVERED。具名git diff --check exit0，本轮16链接有效。准备范围VERIFIED，不升格H06/H07/Android或生产0GC。两次PS5.1启动兼容失败原件保留，详报告末段。
