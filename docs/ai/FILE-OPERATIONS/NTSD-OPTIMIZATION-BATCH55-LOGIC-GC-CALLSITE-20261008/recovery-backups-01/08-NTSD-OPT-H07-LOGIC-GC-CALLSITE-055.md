<!-- CHANGE-RECORD
id: NTSD-OPT-H07-LOGIC-GC-CALLSITE-055
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Test/Editor/BattleOptimizationCpuGcCaptureEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleLogicGcCallsiteWindowEditorTests.cs
authority: approved finite-phase H07 necessary callsite diagnosis of Batch54 calibrated combat six steady events; Editor only, no production promotion
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH55-LOGIC-GC-CALLSITE-20261008/REPORT.md
-->
# 混战逻辑调用点

RUNTIME_PENDING / RETAINED_RAW_PARSE_PENDING：唯一55窗口DONE/cleanly stopped、actual AI/roster1000、120+180/fullDriver300有效；instrumented steady8events(first130)只能诊断，不能覆盖54六事件或称bytes/FPS。CPU窗口实际从sample36启动，capturedFrames0/connectionId-1/validMainThreadFrames0、statusPARTIAL、stopReason owner-shutdown；没有完成8frame导出，不能把空samples视为0分配。未接受frame通知的准确原因UNKNOWN，不凭名字归因Unity bug。

cpu-gc.raw保留1,040,390,105B，prior-history.raw保留35,640,372B；全部Profiler before/after逐字段相同、ordered shutdown objects/slots/borrowers0、Scene/Menu clean8roots/原Editoridle。末tick300整份extended snapshot与54逐字节相同（10 hash域同、RNG19105/276539415），非逐tick/native。63guards/HEAD同、actual request.json absent，C#source冻结未再改；旧54Temp准确backup保持。下一只读解析该已有raw，不重采、不clear历史；现39恢复入口固定旧root且要求emptyhistory，不能直接借它改写55或清空现有history。先声明准确55独立恢复/追加import和最小受影响测试，再改脚本。H07/H11未过、阶段4/6/34已执行，Goal active。

FOCUSED_TEST_PASS / WINDOWS_READY：new15＋旧完整logic observer24＋旧capture request/gate10共49/49、0skip、3.3492237s，原jobffd46ac7e1fe43558f8099bb0a84c3d9。15有效RED原件保留；两源最小diff/默认family/恢复/fresh output/完整scope人工审查。2026-10-07T19:24:46Z 63guards/8逐SHAdirty backups/HEAD同，三C#及meta冻结见pre-window-audit；原Menu clean8roots、Console仅bridge/noCS。两精确Import重载期连接拒绝后同PID恢复，未重启/第二实例。末diff-check0/25CRLF提示（先EOF空行失败已仅修本包新增部分）。尚无actual callsite，不称0GC/收益；GREEN后一次新55menu。

CODE_WRITTEN：只有两声明Editor源新增defaultfalse 55family、单请求/ownedroot、原Arm opt-in sample35和prior history保存；原8frame/128线程/恢复/退出链复用，logic owner仍完整300且新instrumented verdict强制不称0GC/byte/FPS证书。15有效RED后才实现，GREEN/实际栈尚未验。54末snapshot比较及状态归档完成，不降低性能门。

RED_CONFIRMED：原Editor job02629183f11540069ec4bb5caa8f5c87终态failed/15completed，14具名缺入口/字段失败、原8容量通过；summary null不造skip/duration。Import后两次6402拒绝连接，同PID19040且日志ReloadAssembly，随后同连接恢复，无restart/重复测试。validator0/1351Records/4253历史warnings；diff-check发现五治理新增EOF空行，仅将本包前缀移到文件头/去本包EOF空行，不清理用户内容。保存原RED，随后才实现。

TESTS_WRITTEN：新增15个具名反射case，先要求缺新入口/阈值/请求字段形成实际RED，两个现有C#仍未改。8backup（五治理副本只去本包注册前缀）现已逐SHA等before-manifest；63guards保护，Profiler未启动。精准Import新test并原Editor执行，禁止0case充数。

PLANNED；两个现有Editor脚本及新增反射聚焦测试，原状/准确符号和验收见[Task](../TASKS/NTSD-OPTIMIZATION-BATCH55-LOGIC-GC-CALLSITE-20261008.md)。起始54旧路径已诊断VERIFIED，混战6events/first158、性能P95114.910ms仍FAIL，不能改历史。新采集仅不同未解决调用点，固定8frame从sample35启动、完整Driver recorder300tick仍覆盖，不称新FPS/生产0GC通过。

所有权：Capture类owner/8frame/固定128threads，不扩热容；由Suite复用正常/异常/PlayExit/reload/owner shutdown恢复原Profiler settings。SaveNew输出独占新root，不clear/覆盖旧profile；已有history先保留。现有默认capture仍首8/sample1。回滚基于本包before-manifest/8当前dirty backups，需另获授权。本批不修改逻辑规则/关闭阶段，不依赖Q06 body。具名测试先RED再GREEN，失败保留。
