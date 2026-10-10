<!-- CHANGE-RECORD
id: NTSD-H07-PRODUCER-ADMISSION-WINDOWS-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/H07ProducerSnapshotAdmissionEditorTests.cs
authority: User approved second named producer snapshot reuse admission, fixed admitted Role
evidence: docs/ai/TASKS/NTSD-H07-PRODUCER-SNAPSHOT-ADMISSION-20261009.md
-->
# 生产者快照复用当前同配置准入窗
脚本前登记；准确路径/符号/原状/副作用/owner容量关闭/风险/最小验证/回滚见Task。生产复用当前仍OFF；只沿冷诊断owner在新目录记录。
CODE_WRITTEN：Suite新具名producer冷入口/隔离output/序列化模式bool，沿用88 request/Apply/Observe/Restore；新增6请求等价case及Explicit设置前后只读快照。原生产World/Host及复用flag未改，Role接线保持。

最终：44/44聚焦＋设置前后各1/1 PASS；一次四120+180窗一致性/有效完整Driver稳态0GC/容量/关闭通过，混战tick +14.448675%回退未解释，NOT_ADMITTED/生产OFF。VERIFIED只指冷测量实现与记录；生产阶段未启动。真实命令、失败门、复用依据、未执行项与回滚边界见 artifacts/diagnostics/NTSD-H07-PRODUCER-SNAPSHOT-ADMISSION-20261009/REPORT.md。没有新生产state/cache，原Suite owner finally/终态清；不得同窗重跑刷PASS，后续新方向由用户决定。

交付校验实际完成：ChangeLedger exit0 / PASSED，1396 Records，6 governed dirty code paths覆盖，4417 warnings（与第一项同计数的历史声明路径等警告，非0警告）；git diff --check exit0。未执行任何Git写入。
