# NTSD-M03-STRATEGY-REEVALUATION-20261010

状态：SCOPED_REASSESSMENT_COMPLETE / DIAGNOSTIC_DESIGN_READY / PRODUCTION_REPAIR_NOT_READY。

授权：用户在本讨论会话要求“那就重新调整策略，然后重新评估”。本轮重新评估原 M-03 的策略，复用实际结果并窄读相关调用链；不恢复旧候选、队列或自动监测。

完成范围：纠正计时层级、核对当前生产资源绑定证据、重新计算旧成本组及两个历史生产窗口的显示样本分类，形成下一次有限诊断的目标与退出条件。没有修改 C#、资源、设置，没有运行 Unity、测试、Profiler 或新性能窗口，没有向执行会话派发修复。

结论与策略：[REPORT](../../../artifacts/diagnostics/NTSD-M03-STRATEGY-REEVALUATION-20261010/REPORT.md)。机器复算：[analysis-summary.json](../../../artifacts/diagnostics/NTSD-M03-STRATEGY-REEVALUATION-20261010/analysis-summary.json)。

文件操作：[Operation](../FILE-OPERATIONS/NTSD-M03-STRATEGY-REEVALUATION-20261010/RECORD.md)。本轮仅文档，Change ID 不适用；后续新增诊断脚本须另有准确 Change/Operation，不能拿本报告直接改生产。

父 M03 OPEN。段内尺寸旧候选 ABANDONED/default OFF 保持。H07、producer、95、H06及设备/专项边界不变。下一评估仍在 M03 内，不进入 H08/H10。
