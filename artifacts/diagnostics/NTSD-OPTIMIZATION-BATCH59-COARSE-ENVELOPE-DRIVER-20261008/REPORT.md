# 第59批：保守包络完整 Driver 资格结果

结论：DRIVER_QUALIFICATION_PASS / FOCUSED_TEST_PASS / NOT_ADMITTED。8/8 聚焦检查通过，四组 OFF/ON 共88组同tick对照、176次完整逻辑tick执行，已声明比较项没有差异。没有本批实际Scene性能、FPS或可靠0GC结论；候选仍默认OFF，H-07/H-11与首阶段仍未完成。

## 实际验证与范围

- 唯一修改脚本：Assets/NTSD/Scripts/Test/Editor/BattleBruteProductionAdmissionEditorTests.cs。对事前current-dirty副本为98增/1删；Run只增加nullable coarseEnvelope，显式新case设置候选，旧调用/断言/CompareEveryTick/生命周期保持。
- TickRow新增实际flag、directions、rejects测试输出；其分配发生于测试侧，不当战斗0GC证据。
- Query、Suite、Host、renderer及生产默认未改。四已准入快路径均true；kind5、eligibility、rejected-binding、coarse-dispatch、timing均false。
- 有效RED：job 5aedbbdbf584448c9cffc6156b49f8d2，4/4因Run缺显式候选接入失败，2.991045s，0skip；均在Driver启动前命中接入护栏。test-red-original-01.xml保留。
- GREEN：job f5219d6a418a4ec2b6f8f61aea276f77，新5项＋旧default/request纯检查3项，8total/8passed/0failed/0skip，483.266580s。test-green-original-01.xml及compact terminal保留。该耗时包含冷载与测试日志，不是战斗性能样本。
- 四新case均运行完整Driver；比较tick/entities/声明extended或parity hashes/lockstep hashes/native与legacy随机数调用。两千人布局各10个声明hash域；两root原parity为9域，lockstep10域。
- 适用formal root仅旧oid/action/hp/vx字段，沿原打印精度容差；正式EXE与具名trace身份已核。不扩大为整个native逐位等价、自然按键或全角色验收。
- 既有WithLoganScenarioForReplayTests末尾objects/claimed slots/pool active三项0断言通过；不是本批真实Scene关闭/重进或完整11阶段trace。

| case | OFF/ON配对tick | 已比较差异 | ON实际方向 / 提前拒绝 | 其他观察 |
|---|---:|---:|---:|---|
| right-x550 | 12 | 0 | 24 / 16 | entities 2→3；适用formal字段通过 |
| left-x350 | 12 | 0 | 18 / 14 | entities 2→3；适用formal字段通过 |
| Dispersed1000 | 32 | 0 | 19,237,743 / 17,145,891 | active实体1000→1000，32tick均实际应用 |
| Combat1000 | 32 | 0 | 16,691,292 / 13,906,488 | active实体1000→1000，32tick均实际应用 |

每组baseline候选flag为false且计数0，candidate每tick为true；现有cache每tick应用/noFallback门通过。方向计数不等于被减少的独立pair数量，也不换算FPS或收益百分比。详细文件名、SHA、字段和结果见[qualification-summary-01.json](qualification-summary-01.json)。

## 失败和观测限制

原Editor PID19040/6402，未启动第二Editor。首次import后domain reload使state/RED launch连接失败，没有获得job，不算RED；test-red-launch-01.json保留，随后同Editor有效RED。GREEN运行时主线程同步测试导致部分MCP读超时，观察超时不是作业失败，未重启/重跑。成功终态的includeDetails=true含约7MiB日志导致工具输出截断，改为查询同job compact终态，并复制、结构核验原XML，不把截断包装成测试失败或重跑理由。XML的testcasecount9756是整个树规模，执行总数只有8。

## 保护与审计

事前八准确dirty副本含公共Temp/Goal18_LastTestResults.xml，全部在脚本和治理修改前核SHA；未用HEAD替代dirty。原公共XML22215B/SHA3AA8BA31D4D239A987E4A29F036BFF56E564D5BEB558E41E5B4C55D8E0675FF7保存在Operation。现有全局RunFinished callback自动覆盖该Temp，callback本批未改；RED、GREEN各自新副本均在下一测试前保存，旧输出不覆盖。
- RED XML SHA16F48F317E67E89A1491229F5D4F42E83694D73F0C36CDE30A7975C2D1DEE826。
- GREEN XML SHA F4AB092E77B94B76CBC3F90CA5839A1342FC2669D68942EA358B8D5D2A3111B3，7,193,758B。
- Admission final SHA F53FD85CDFC738D467108E9E890AAA9343065298BCEE3C67861CB9C5151D0D00。
- terminal-source-audit-01.json：92guards、8backup、HEAD同；公共XML与保存GREEN同；shared pressure request absent。后继只读重核92guards仍同。
- Query58 SHA2EAF124DCC51EA10D752F64CDFA34CD7CADDAD0CDB9F187E4B95DC3FD9D11970、58fixture SHA C2D330956550050AF6D956E4A8711A68D3C11AE51AD9B047D0A9FFEDA0C5F129未改。
- editor-final-01.json记录原Menu单Scene clean8roots/idle/nonPlay/noTest/noCompile。未宣称全部Console历史无错误。
- validation-before-terminal-01.json为实际pwsh validator exit0（1355Records/20governed/4252历史WARNING/0ERROR），git diff --check exit0（26换行警告）；未保存完整validator stdout，只保存实际摘要。本次终态文档更新后的校验另保存，不伪称事前结果已覆盖后写文档。

## 尚未完成与下一动作

终态文档校验（UTC2026-10-07T22:34:56.0237854Z）：实际pwsh Tools/Validate-ChangeLedger.ps1 exit0，1355Records/20governed/4252历史WARNING/0ERROR；git diff --check exit0，26换行警告。实际摘要保存[validation-final-01.json](validation-final-01.json)，完整stdout未落盘。没有本轮新增Unity执行或测量。文档更新工具最初因同patch重复目标被拒绝，五首行只读核实仍为原PLANNED，拆为合法唯一目标patch后成功；一次读取命令PowerShell管道解析失败后只读修正，不影响源或测试。

最新实际千人结果仍是57：logic P95 102.513/109.402ms、drop807/755，超过33ms阶段门。58的局部22.7393%/7.8923%信号不外推。H11仍保留43首次12迟发event有效FAIL，47零事件不替代；旧Q09资源FAIL及55深层30B未知不抹除。

下一[第60批Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH60-COARSE-ENVELOPE-WINDOWS-20261008.md)准确限定现有Suite四窗：Dispersed OFF/ON、Combat OFF/ON，各120warm+180sample，保留完整Driver可靠GC observer、实际1000AI、原请求与关闭保护；只回答本候选真实收益，不重复本资格/旧局部cost/57窗口，不自动推广或解冻collector/EXT1/Mono/ATLAS。必要stride/实际驻留预算在推广前另有证据，不用理论17B当实际bytes。

阶段4/6、限定产物5/6、34父关闭0；22—59共38已执行，下一60仅READY，不计已执行。Goal保持active，不因批次结束或累计次数停止。没有本批SelfCheck全套、实际Play、正式1800、Android、GPU、Frame Debugger、Profiler或M0执行。
