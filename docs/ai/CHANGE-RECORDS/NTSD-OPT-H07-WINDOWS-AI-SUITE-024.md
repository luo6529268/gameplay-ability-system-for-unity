<!-- CHANGE-RECORD
id: NTSD-OPT-H07-WINDOWS-AI-SUITE-024
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/ProductionEntityStressHarness.cs
authority: user bounded six item first phase and start execution 2026-10-07; formal336 unchanged
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH24-WINDOWS-AI-20261007/REPORT.md
-->
# 第24批Windows活动AI有限报告入口
最终有限执行停点：windows-03完成Dispersed120+180，实际AI/roster最低1000、300000 canonical eligible=committed/fallback0、窄tick0B/capacity0拒绝；logic mean443.8275ms/p95 659.97925ms，harnessValidity=false，不是合格证书。下一Combat因pool仍sealed无法prepare，0tick；formal四窗未跑。诊断1/修复3/3到限，RUNTIME_PENDING / WAITING_USER_DECISION，不继续第4轮。原11阶段三残留0/World释放/Scene同/Menu恢复，原失败不删。父H07与Goal未达成，首阶段2/6，新批3/8已用；其余READY评估继续，Report详原件与保护。
第三固定聚焦作业469009f20e544011aff5e196bc539230实际25/25 Passed、0fail/skipped；源码快照布局适配已真实编译/导入，windows-03最后实际工作负载窗待结果。修复3/3，不自动新增轮或扩大矩阵。
原validator无WorkingTreeOnly参数，首次该参数绑定失败，不算验证成功；改用真实默认WorkingTree+untracked命令已PASS：1320records/6当前C#路径covered/0errors/4281历史warning。diffcheck0，仅CRLF提示；98其它保护当前hash不变。最终运行结果尚待，非本批交付收口。
第二窗实际创建1000，0tick/Peak population失败；before GO1000/WorldObjects2000/entity1000/slot1000，原11阶段三残留0/Scene同/Menu恢复。旧推断“本入口出生直接logic-only”被该证据修正：Driver新World在seal前flagfalse，seal1423行才开启logic-only。最后同批3/3只采集初始出生布局作严格population依据、报告字段，禁止用seal后flag判初态，也不强行物化模式变化。windows-03最终固定复验，失败不增轮。
原Editor第二固定聚焦作业cefb8489a36542158981f7a4996b0edf：25/25 Passed、0fail/skipped，9313仅discovered；17新request/OID/logic-only正负、旧capacity7、旧Legacy population1。生产规则/Factory源不改，诊断适配编译通过；windows-02真实工作负载尚待结果，修复2/3。
代码追加：诊断harness ordinary正OID准入、task显式World、合法logic-only允许Renderer空（不造shell）、当前owner/handle、population显式logic-only1000对象/实体/slot+GO0；旧Legacy默认门保持。Editor转windows-02、terminal只接受本批own root，旧文件不删，新增3OID+8population正负case；等编译17case/现有capacity focused和固定六窗。修复2/3。
同批脚本前追加：windows-01 PARTIAL，实体0/1000创建失败、selectedOid0；ordinary OPoint既有guard拒绝0（Factory455、LogicFactory38），且压力harness IsActive要求Renderer、旧population要求GO/2N，均未适配当前CentralOnly。仅诊断harness五处准确适配，生产Factory/规则不改，当前backup/边界见Task/Operation追加。11阶段三残留0、Scene同/Menu已恢复；六后续窗未跑，不称性能失败。复验2/3；诊断1已使用，不再同构观察。
重新实际编译成功并原Editor六request case 6/6 Passed（56a9b8d08db844c7a050cabaadd7a96c，实际6执行，9302仅discovered）；全部调用既有FromRequest可解析。仅验证请求入口，Play/正式报告未完成，修复1/3。domainreload期间6401短断；6402监听归属AssetImportWorker，read-only探测超时，不用于场景/执行；原主PID19040/6401恢复。
首次真实编译失败：Editor.log记录新入口151行CS0664（double 0d不能隐式转float）；原read_console error-CS过滤返回0而未覆盖此错误，不以该响应宣称编译通过。已同次静态修正为0f并请求重新编译；计入口修复复验1/3。原日志保留，原生产源不改。
事前静态核对修正：原collector parser实际接受`brute`，request使用该既有枚举字符串（不是新模式）；CPU budget字段float用0f。Task中的force-bruteforce指语义，准确JSON为brute；未运行Play/未生成错误request，生产默认保持，不增候选或测量次数。后续六case同时验证FromRequest可解析。
代码追加：新Editor Session有界六run、直接既有压力API、每run冻结request/观察/终态及11阶段退出/原Scene保护，新增六request helper case。生产runtime未改；编译/测试/Play尚未执行。
脚本前登记。新Editor入口直接调用原Config/Runner，不写/删共享request，只拥有事前不存在的终态信号和独立新输出；生产源不改，不改变访问性/默认/规则。完整范围/固定六矩阵/副作用/验收/backup/恢复见同名Task和Operation。
副作用：原Editor有界Play、既有1000AI与预热cache、观察采集成本；正常domainreload只恢复本批session，不恢复其它任务，不新建Runtime模块。不冒充Player/Android或完整父项验收。测试未运行；首阶段3/8，本条目新diagnostic1、无优化算法候选，局部入口修复最多3轮。
