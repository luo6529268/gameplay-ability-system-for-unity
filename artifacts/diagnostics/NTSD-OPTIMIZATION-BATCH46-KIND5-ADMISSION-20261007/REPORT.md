# 第46批 kind5 必要准入校验报告

最终治理已实际完成（2026-10-07T13:43:12.9296790Z）：pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity exit0，1342 Record/14 governed代码diff、4257历史warning、0error；git -c core.safecrlf=false diff --check exit0/messages0。validation-final-01.json保留。此后没有C#变更；本批资格收口，H07/H11未过、Goal active，不调用complete/blocked/paused。

## 最终限定结论（2026-10-07）

SCOPED_KIND5_QUALIFICATION_PASS / NO_DEFAULT_PROMOTION。NTSD-OPT-H07-KIND5-ADMISSION-046仅本批必要正确性资格通过；不是H07性能或H11完整0GC完成。kind5仍default false，四已准入普通Brute默认true、collector/backend/AI/cadence/资源/排序不改。Goal active，阶段4/6、限定产物5/6、34父关闭0、已执行22—46共25子批；次数只复盘。

### 实际验证

原PID19040/Unity2022.3.62f3，既有MCP6402；未另开Editor、Play/Profiler/FrameDebugger/GPU capture/M0或性能窗口。

- 新4case：job561602e5d8d44aa5b4e8de3849edf287，4/4 PASS，0skip，502.582669秒。
- 旧入口7case：job99850ae614ec44da8fbcb0c418bf69cd，7/7 PASS，0skip，513.0826837秒。两job各一次，没有重复dispatch。catalog9546不是实际执行数。
- test-qualification-01.json / test-old-entry-01.json保留所有case结果、时间/状态/错误字段，只省略重复资源加载日志。一次完整响应过长被输出截断后改为冷过滤日志，没有复跑测试。
- 新4个完整parity JSON及parity-audit-01.json逐字段重核：88个OFF/ON tick pair，共176个完整Driver执行tick，所有全域hash字符串、lockstep、RNG调用和实体数相同；不是全native World逐位或自然Scene性能证明。
- 三次测试中TCP状态观察超时、终态第三个Console查询超时均是工具观察失败。保留过程事实，同PID/同job继续；原终态get_editor_state成功证明idle/非Play/非compiling/testsInactive，manage_scene成功证明Menu单Scene8roots、isDirty=false。编译前CS筛选0，实际新case执行通过，不将失败的最后Console查询写成成功。

| 新case | 每路径完整tick / 比较pair | ON实际应用 / fallback | 有真正省扫描的tick / 最大省略数 |
|---|---|---|---|
| C051 right-x550 | 12 / 12 | 12 / 0 | 7 / 2 |
| C051 left-x350 | 12 / 12 | 12 / 0 | 8 / 2 |
| Dispersed1000 | 32 / 32 | 32 / 0 | 32 / 848001 |
| Combat1000 | 32 / 32 | 32 / 0 | 32 / 652451 |

C051正式根SHA336B44及两个root trace SHA均当前核同；四字段OID/action/HP/Vx按原CompareRoot断言通过，Vx使用正式trace打印精度容差1e-6，不冒充native完整bitwise World。千人使用当前Logan DAT＋项目mode、DataOrientedCanonical/相同scenario seed；每tick实体至少1000，canonical AI实际消耗native RNG；replay工具的ordered shutdown objects/slots/borrowers0断言逐Run通过。OFF与ON都保留原四机制true、timing关闭，仅新kind5开关不同。省扫描频次不是成本比例/FPS收益。

### 改动、证据复用与保护

唯一C#是BattleBruteProductionAdmissionEditorTests.cs：追加两方法/四case、可空kind5参数与应用证据、两个冷TickRow字段、SaveNew可选独立46根。旧7case及原断言保留；new flag只赋值本Run的局部World query，原replay owner关闭释放，无新runtime owner/buffer/生命周期或production改动。无人为制造RED，因38候选已实现，先补资格测试再首次执行。

终态test source SHA160F9D482B38AC1475C973D267D1B3E9C0C4E9C865448D2B3A33F99D6D871CC4/17514B与运行中冻结相同；37保护、9精确dirty备份、HEAD45bbed41全部同。保护包括38 Query/Suite/RoleAwareTests、Q06只hash、Scene/资源/Settings/权威/EXT1/ATLAS/Mono。235/235受影响回归按同source指纹复用，不重复全项目/历史对齐。terminal-source-audit-01.json记录全部SHA及旧入口在40根新增4个fresh GUID输出；旧原件不覆盖，FileMode.CreateNew。

首次precheck validator exit0/1342/14；diff--check2只因本批追加6段后新EOF空白，失败validation-precheck-01.json保留。仅去新空白后，13:28:15Z native pwsh实际validator exit0/error0/4257历史warning，diff--check0。最终状态/总表更新后再运行最终validator，结果追加，不伪造零warning。

### 仍未达与下一必要动作

本批不测新FPS/GC；38短窗logic P95 89.403/98.277ms、drop694/660、logic GC UNKNOWN仍有效，不能升级为120+1800正式通过。kind5局部fixture收益与非同期实景观察仍按38限定，正确性门通过不自动授权default推广、不改collector/backend。

H11保留43首次完整窗口12迟发event/ordinal937有效FAIL、第二次重进完整0event PASS；42工具源码当前固定首8个camera（CaptureFrameCount=8/HasCompletedCameraWindow），不能解释937帧。下一范围内必要动作是有界迟发事件调用点归因，先声明准确Task/Change与诊断寿命/容量，不再盲采同版完整camera刷PASS、不推断MCP导致事件、不缩scope/豁免首帧。H07准入后未完成的实际性能工作保持，专项及生产推广权限按原合同；这不是Goal暂停/blocked/complete，也不重开四个已完成阶段评估。

### 历史事前快照

PLANNED / NO_DEFAULT_PROMOTION。仅补38候选逐tick与适用正式字段证明，不测新FPS/GC、不改变生产默认。详细合同见docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH46-KIND5-ADMISSION-20261007.md。当前未执行本批验证，H07/H11未达，Goal active。
