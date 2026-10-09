# NTSD-H06-PROFILER-RESET-100-PREP-20261009 / PLANNED

时间：2026-10-09T19:43:21.601460+08:00。执行者：当前方案评审会话 01a11fe2-bfaa-72f3-a9ed-c8260f63b193。
用户授权：本会话“那就先把旧历史数据删除了，然后重新生成的数据，你觉得如何”及后续“那就开始处理吧，这部分属于准备阶段，不算处理优化代码逻辑，所以你可以直接处理”。仅Profiler内存缓存清空、100对象诊断准备与短采集；不恢复旧自动Goal、其它会话或优化逻辑实施。

Task：docs/ai/TASKS/NTSD-H06-PROFILER-RESET-100-PREP-20261009.md；Change：NTSD-H06-PROFILER-RESET-100-PREP-001。
准确清理：原项目Editor PID74724 / localhost6402，在idle/非Play/无编译/无其它测试且Profiler/deep/GPU关闭时，调用UnityEditorInternal.ProfilerDriver.ClearAllFrames()。旧Profiler内存历史无法完整导出，用户已明确授权清空；不假称有可恢复的完整备份，不再导出旧历史。磁盘文件删除清单为空；所有旧raw/JSON/XML/报告保留。

文件写域及真实dirty备份见before-manifest.json。五个治理文档仅追加/文首新增本任务指针；公共Temp测试XML每次自然替换前保存现存版本并核SHA。唯一新项目脚本H06CollectorPreparationEditorTests.cs及Unity生成同名meta；旧Admission/生产/Suite/Scene/Prefab/资源/ProjectSettings/InputActions/Gen/Plugins不改。保护1192项当前字节见protection-manifest.json；HEAD和staged状态已保存。

拟执行：既有MCP manage_asset(import准确新脚本)、run_tests(EditMode,准确具名Explicit清理/100对象定位)、get_test_job及read_console；先校验新程序集，所有结果独立新UUID。保留76普通/24特殊、50/50阵营、原局部距离和碰撞框；4warm及3个校准preflight样本在详细录制外，最多3次Role收集详细采集，不跑Brute长窗、不改变千人资格门。
Profiler设置及各area事前落盘，try/finally关闭自有录制/释放本次纯逻辑World/恢复本轮已知设置并前后比较。失败仍落盘；不重读活跃Q06方法体。分配栈元数据可以报告符号/行号，不据此打开禁读实现。

早期只读失败：unity status无Pipeline不能当Editor关闭；原MCP确认idle。execute_code只读查询因CodeDom命令长度限制失败，未执行目标代码；不安装/修改第三方包，改用新具名Editor准备测试安全编译。所有正式Unity调用只用原Editor，不启动第二实例。
回滚：本任务新增与文档增量可独立定位；保留全部事前dirty备份，不自动删除/回退。缓存清空后未导出细节不可恢复，已由本次明确授权覆盖。

CODE_WRITTEN：新准备脚本已登记，下一仅import准确路径；新meta由Unity生成。

原Editor新程序集UTC11:46:30晚于新脚本11:46:08，reload完成、idle；开始准确ClearAuthorizedProfilerHistory。公共XML已保存tests-before-reset.xml。

ClearAuthorizedProfilerHistory job a75502e05f5348de91824b657cf82967 1/1 PASS；first/last 0/7→-1/-1，完整设置/各area相同；无磁盘删除。reset-tests.xml保留，开始Capture100AllocationCallsites；公共XML替换前再备份tests-before-capture100.xml。

100对象首轮job ba2d06104500485cafb5486914b1dfea 1/1PASS，新raw 9912324B成功导出，3marker/66event、40B/event，首个解析managed frame LoganKindCatalog.FindEffect line71。发现preflight在recorder.Begin后立即Assert造成额外分配：29计数不可作纯collect结果，调用栈marker内22/call有效。原测试源码/XML保存revision-01-assertion-contamination；下一仅把断言移到scope结束后并补已解析managed栈断言，保留旧产物，不改生产。修订后只重跑本准备项。新缓存已完整raw落盘，可以清空后再复核。

reset2 job e31b17726bf142d4b747b7381b2eabe8 {"total": 1, "passed": 1, "failed": 0, "skipped": 0, "durationSeconds": 5.7383224, "resultState": "Passed"}; XML SHA DB3664AF6A0A346941E6B6698CC95AB8B51173AA27B70E67BC8DB97690A81EE4。

capture100-revision02 job 78eebe0088994b8ead7b77c04fd6eaac {"total": 1, "passed": 1, "failed": 0, "skipped": 0, "durationSeconds": 2.0642197, "resultState": "Passed"}; XML SHA 0C8C9245CD0012BE68A479E005AB8EBF933B41CF78D134E5BA9F6119A51AEB92。

2026-10-09T19:57:47.998605+08:00 收尾文档前准确checkpoint：closeout-before/manifest.json，8个本轮待更新文档原字节已备份；新增REPORT及allocation-summary/editor-final-state，旧内容不回退。最终源SHA ED0F5AE09BD207762A32B15A6168751AF5A5406D247806D865B204A1CE959798；最终raw9703396B/66×40B、collect22/22/22，生产FindEffect仅只读定位未修改。原Editor最后idle非Play/无tests/无编译，Menu clean/8roots/Profiler stopped；1192保护0变化，before备份6/6、HEAD/staged稳。以下仅文档/validator收尾，不新增测量。

收尾校正：Task新增报告相对链接少一级，当前字节备份task-before-link-correction.md（SHA AA09E8910EF14DC65AE8A3539B6139BD5130CB9D5BA76CBB88A29A314B747EB9），只修本轮新增链接。Validator首次使用Windows PowerShell未显式传RepositoryRoot，其参数默认PSScriptRoot空导致初始化错误、尚未执行验证；保留change-ledger-final.log/summary，下一显式传项目绝对路径重试，不修改validator脚本。git diff --check具名文档exit0，只有既有换行提示。

Validator第二次Windows PowerShell5.1将Git LF/CRLF警告转为NativeCommandError而中断（尚未进入最终PASS/FAIL判定），保留revision02日志。当前已用只读版本查询确认既有pwsh7.6.5可用，下一仅用该runtime执行同一只读validator、显式RepositoryRoot；不改validator、Git配置或忽略真实错误。

2026-10-09T20:01:15.881870+08:00 VERIFIED：最终两准确入口reset2/capture100-revision02均1/1PASS；定位FindEffect71、22×40B/call、3call、新raw9703396B成功；完整本轮Profiler/areas恢复、对象/slots0，原Editor idle/Menu clean8roots，生产及旧证据1192保护0变。validator最后pwsh7.6.5/RepositoryRoot显式exit0、0ERROR、4265历史WARNING，新增脚本已COVERED且无对应WARNING。具名diff-check exit0/16链接有效/5治理原文保留。公共Temp XML原字节与每次覆盖前检查点仍在本Operation；磁盘删除清单空，无Git改变HEAD/staged。Task与Change限定VERIFIED，H06准入BLOCKED/Brute默认/95OFF/旧Goal取消均保持。准确逐文件最终SHA见after-manifest.json；最终审计见final-validation.json。不恢复另一个会话/监视自动任务，不执行生产修复。
