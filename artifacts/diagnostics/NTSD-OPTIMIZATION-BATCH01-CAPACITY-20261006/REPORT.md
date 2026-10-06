# 第一批优化证据报告

Task：NTSD-OPTIMIZATION-BATCH01-CAPACITY-20261006
Change：NTSD-OPT-H11-CACHE-PREWARM-001 / RUNTIME_PENDING（子批编译/聚焦通过，父项开放）
范围：H-11缓存预热小批，不是完整0GC或性能达标报告。

2026-10-06：静态确认中央DisplayMotion预热遗漏、chunk Upload描述分配路径；
当前Editor PID19040存在，CLI Pipeline不支持（没有package），HTTP MCP8080未连接。
CLI --no-pager参数被当前版本拒绝，去掉后status/pipeline list有效；不安装/升级或另开Editor。
前置PowerShell快照脚本一次语法错误未写文件，改正后before.json已保存。
原根README.md不存在；已读取根AGENTS、当前优化/启动合同、Unity版本与依赖。
无新的GC/帧率/内存实测结果；后续精确结果与失败原件逐项追加。

## RED与当前写入

原Editor既有TCP6402已连接，get_editor_state确认Menu/idle/非Play，
manage_scene确认clean，manage_graphics确认URP；未切换场景。
execute_code CodeDOM命令长度失败，返回原错误（未执行代码），不修改插件规避；
原资源查询可用，不需要新Editor或Pipeline升级。
新测试编译后，精确class job117da49c9b9340b6a02cde0683ddecaf完成7项，
6个预期失败；[原件](red-result.json)。测试树total9028不是实际执行数量。
2026-10-06两生产文件已补预热（CODE_WRITTEN），尚待新编译和GREEN。
第一轮ChangeLedger验证exit0/1297records，仅新测试1个code-path覆盖；
历史not-in-diff warnings不触发全量旧任务恢复。

## GREEN、编译与证据边界

2026-10-06 原Editor实际编译：Assembly-CSharp.dll 18:38:09、Editor.dll 18:38:10，
后续原Editor idle/not compiling，error CS查询0；非隔离编译或仅静态推断。

| 入口 | 实际结果 | 原件 |
|---|---|---|
| run_tests EditMode，SubMesh/SegmentBounds两旧class | 8/8 PASS，0 skipped/failed | [原件](mesh-regression-result.json) |
| run_tests EditMode，NTSD.Test.BattlePresentationCapacityPrewarmEditorTests | 7/7 PASS，0 skipped/failed | [原件](prewarm-green-result.json) |
| get_editor_state / manage_scene get_active / read_console error CS | Menu非Play、idle、clean；0条error CS | [原件](editor-post.json) |
| Tools/Validate-ChangeLedger.ps1 | exit0，1297 records，当前3个code-path均覆盖；4251条历史not-in-diff warnings | [汇总](change-ledger-validation.json) |

旧8项请求误填了新class的namespace，因此该job只执行旧8项；没有把未执行项算通过。
随后用准确NTSD.Test namespace独立启动并通过7项，两个job与原始请求均保留。
工具progress.total9028表示完整测试树，不是本轮运行9028项；本轮GREEN合计15项。

局部0GC检查：高slot1049、预热1050，DisplayMotion lookup在已预热后重复64次，
GC.GetAllocatedBytesForCurrentThread差值0B、5个数组identity不变、当前publication PreciseX仍120。
不测量RenderPass录制/提交/native/GPU，也不把这64次同取样正例晋升完整battle路径0GC。
描述数组正例覆盖1/32/4096/4097容量、strict 1→32高水位、不缩小已分配存储；
旧回归覆盖tail/physical submesh high-water、mesh恢复、bounds/empty/chunk。

仅两runtime预热hunk：central接入已有DisplayMotion.PrepareCapacity；backend给每chunk
现有SubMeshDescriptor[]提前分配保守上界。未重写Build/UV/排序/shader或DrawMesh。
托管缓存成本前移至启动，steady驻留必须记账；native/GPU结构首次分配、总峰值未测。
父H-11仍OPEN：未封闭硬上限/seal/超限整份拒绝或未预热diagnostic增长，不冒称全0GC。

## 未运行与下一验收门

- 未执行完整M0、1000实体/AI压测、Profiler/Frame Debugger/GPU capture、Player或Android构建/真机。
- 未切换原Menu、进入Battle Play或执行实际enter/exit/re-enter，所以11阶段运行验收仍pending。
- 未生成固定输入跨架构checksum/正式EXE全域对照；预热不改模拟字段，不能据此宣布全域重新对齐。
- EXT-1保持PROPOSED / MODIFY_REQUIRED，专项M0未启动；MONO保持原专项门，
  PERF/ATLAS/MONO正文、bank/预算/格式/segment/fail-closed合同均未修改。
- 本轮无文件删除/移动、无破坏性Git/提交/push；现有两份未跟踪authority-content JSONL保留。

统一34项进度总表记录父项关闭0，而不是用本子批PASS把所有优化或Android标成完成。

## 文件与文档收尾

git -c core.safecrlf=false diff --check初次报告本包新EOF空行；仅去掉本包新空行后，
重新检查exit0。34个唯一ID与主表集合完全一致，高12/中14/低8，
本批7份文档相对链接无缺失，新test无行尾空白；[静态汇总](static-validation.json)。
8份保护文件操作前后SHA相同，含Menu/Battle、ProjectSettings、manifest、两shader、
EXT-1 index和Q06 ShadowBuild。现有Editor自动生成的两meta保留，不手写GUID。
精确after路径/SHA、保护与工作树清单见
[after](../../../docs/ai/FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH01-CAPACITY-20261006/after.json)。
