# NTSD-M03-MATERIALIZATION-REUSE-REVIEW-20261010

PLANNED：用户要求继续验证M03直到可发送执行会话修复。当前仅只读生产链、原Editor有界验证与方案，不代做生产修复。
执行者：本讨论会话；开始2026-10-10T17:26:31.886071+08:00。
manifest列6文档真实dirty备份及SHA，before/已核存；拟操作仅前置当前状态/最终结论，不删除旧内容。新建Task/report/诊断请求输出与handoff。源码更改另建准确Change；此步无生产源码修改。
回滚基于before真实字节仅撤本任务前缀，保护并发变更，不从HEAD恢复。

新增准确诊断写域（写前）：Assets/NTSD/Scripts/Test/Editor/M03MeshUvDimensionReuseDiagnosticsEditorTests.cs及Unity生成.meta；没有原文件，新建Explicit工具，只原Editor有限局部UV计算比较，不接生产。Change NTSD-M03-UV-DIMENSION-REVIEW-001。Change Ledger真实备份before/06-ledger.bin。原命令cache跨帧方案不实施。

执行前夹具评审：逐元素比特校验改为无分配直接比较，避免计时外NUnit大量临时对象污染下一样本；尚未运行任何成本窗口。工具旧SHA 67db81dcde2acad01fa4cddf5cac87edab45728247aebaeac4f3e320cba83bf3，备份07。

GC校准纠正（实际成本0窗）：原GetAllocatedBytes校准8192B返回0，结果uv-local-cost.json保留FAILED_STOPPED。工具备份08/SHA=A764C447A70D09B19625FEECC183CC35024306DC33A0937FB8148919DAC1C6AE；仅本Editor测试改用既有BattleScopedGcAllocationRecorder前后校准及完整测量scope，输出新uv-scoped-local-cost.json，不修改Recorder，不重测已执行成本。导入首次scope=scripts未导入新文件，后all刷新已执行1项；空job不是PASS。

有据修订候选V2（不是同版重跑）：V1两槽三纹理轮换退化，不准入；原12窗384scope报告完整保留。备份09/SHA=8B71588E40D079F36BA167A577AB37C0F32EE359540E8F187D8F6834553A4742。新算法只锁定第一次Texture2DArray，不替换；其它Texture原样读，cache仅本round。仅一次新3布局相同ABBA，并增加纯2D不命中负控各4窗（共16窗）。V2任何正确性/校准/GC失败停；3含array布局须全部mean/tail判据通过；纯2D报告开销，不可隐去。新输出uv-latched-local-cost.json。复用已有41语义，不改生产。

最终有据设计V3：V2多纹理局部信号不足，纯array信号不外推，原16窗保留。新查同fixture历史89camera平均1992.056commands/segments恒1；改为复用既有canAppend段边界，任意换段清空，非纹理LRU/非跨Build。备份10/SHA=2F9CB36A4250FF9BE87CBCB67D18FCD7D41ABF506551D10F90E0890B3A4E40D6。仅一次新四布局ABBA：单array段/单2D段/两texture逐quad换段/三texture逐quad换段。前两各mean/tail严格收益门；后两负控仅报告，若平均ON相对平均OFF退化超过5%则不建议通用接线（局部容忍不是整帧验收）。不再追加本地候选版本；交回准确证据。纯UV测试不能证明真实Backend/pixel/热路径。

收尾写前：final-before-manifest保存8文档当前实际字节，前置最新有限结论，原状态/失败/实验全部保留。仅本新Change元数据PLANNED→VERIFIED（只诊断），Task→SCOPED_REVIEW_COMPLETE；派发以实际receipt为准。

VERIFIED（本讨论诊断/文档操作）：8契约测试通过、三个有据不同微测组真实完成，性能缺口与前置失败保留；155保护不变/Menu clean8，原manifest备份SHA核对一致，validator PASSED/diff0。实际派发M03段尺寸有限候选，新turn01a12535-e296-7631-935b-d964417ebd67，监测恢复。无生产源码修改、Scene/设置/资源修改、删除/移动/Git破坏操作。原件与final-before备份保留。
