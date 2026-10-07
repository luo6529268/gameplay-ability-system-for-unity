<!-- CHANGE-RECORD
id: NTSD-OPT-H11-ALLOCATION-PROVENANCE-028
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Test/Editor/BattleCentralProductionWindowSceneProbeEditor.cs
authority: user approved six-item H11 complete selected presentation zero-GC; continue actual failures without count-only stop; formal336 and production unchanged
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH28-ALLOCATION-PROVENANCE-20261007/REPORT.md
-->
# 第28批实际分配帧级归属

最新终态：PARTIAL / PROVENANCE_PASS / ZERO_GC_FAIL。一次1800frames，前后校准通过、所有记录scope有效，逐帧camera2/BeginObserver0/EndObserver13求和与汇总一致。observer13全在ordinal0/tick9，camera各1在ordinal2/tick11和ordinal7/tick16；只有帧/阶段相关性，不是调用栈或允许跳早期帧的证据。Foot/Health下限各2、两slot、11484 CPU draw录制=执行、growth0/CPUlease0；11阶段objects/slots/borrowers0、Battle clean/SHA同/原Menu恢复。187保护/10备份/HEAD保持；[归属摘要](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH28-ALLOCATION-PROVENANCE-20261007/provenance-summary-01.json)。H11完整0GC未过、H07性能未达标，不继续同构GC观察或冒充FPS收益。下文事前/运行中描述保留为历史。

脚本前PLANNED。[Task](../TASKS/NTSD-OPTIMIZATION-BATCH28-ALLOCATION-PROVENANCE-20261007.md)冻结唯一Editor代码路径，27有效校准与两个CPU正例复用；camera2/observer13汇总只有完整scope归属，真实调用点未知。只给预分配FrameSample值字段补每camera/slot/前后observer事件及valid信息，窗口后检查求和。没有热路径容器、日志/输出或生产算法；校准、整个scope和严格0GC门不削弱。

风险/生命周期：字段赋值与scalar统计包含在原完整scope，观察器自身开销仍计量；现有probe owner停止/Dispose/Play退出/域重载原样，不新增Runtime服务/11阶段、不用CPUlease或Execute返回证明GPU完成。只帧/阶段相关性，不等于分配调用栈或GPU证书。

Operation：NTSD-OPTIMIZATION-BATCH28-ALLOCATION-PROVENANCE-20261007。必须先保存唯一脚本及准确文档当前副本、非写域SHA并回链再改；旧证据保留。恢复另获准确批准，用当前副本最小patch，不用HEAD覆盖脏工作。

验收：原Editor编译，原Battle一次1800distinct camera、字段sum和汇总一致/前后校准/活动FootHealth/两slot/录制执行/0growth/CPUlease0，11阶段关闭/Scene同/Menu恢复/保护/validator。33ms/3ms/max2/checksum/RNG/publication只读、segment/fail-closed、Q06/EXT1/Mono/ATLAS门不变；两个CPU桥/旧测试不重跑。当前未编辑或执行，不称新的0GC/FPS通过。

实际仅原probe补三个ScopeResult值字段及两个当前scope缓存；camera与BeginObserver结果写入已有FrameSample，EndObserver停止后将采样结果值写回同一preallocated条目，属于既有Stop后读出结果而不是删减API/统计范围。scope外逐frame valid和sum=total审计，独立28菜单/目录；27旧入口同语义共用小factory，不改窗口或运行规则。尚待原Editor编译及一次实际窗口，未有分配调用点/0GC/FPS结论。代码前10副本/187保护和原Menu安全窗已核。

原Editorrefresh后实际重载完成/Menu idle/clean/无测试编译；仅一次加载原savedBattle并调用28菜单，camera-01已启动。ChangeLedger实际PASS1324Records/9 C#diff/4277历史warnings，未重复旧NUnit/CPU桥或1000AI长窗。待1800camera实际结果及关闭保护，不把compile作归属/0GC验收。
