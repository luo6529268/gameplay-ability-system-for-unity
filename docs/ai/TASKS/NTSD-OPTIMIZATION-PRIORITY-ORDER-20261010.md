# NTSD-OPTIMIZATION-PRIORITY-ORDER-20261010

状态：DOCUMENTATION_DELIVERED / STRATEGY_DISCUSSION_PENDING。日期：2026-10-10。
需求来源：用户明确要求“重新按照优先级来整理文档，然后把第一个优化点告诉我，我们来讨论具体实现方案”。
Change：N/A，仅文档。Operation：[记录](../FILE-OPERATIONS/NTSD-OPTIMIZATION-PRIORITY-ORDER-20261010/RECORD.md)。

## 范围与交付

- 原34父项ID和6既有子方案保留，不增加优化点。按高、中、伴随保障、后续/按需、低、暂缓重排总清单与进度；编号不代表执行顺序。
- 首个讨论项：H-07既有子方案[H07-INPUT角色输入与AI输入计算](../OPTIMIZATION-REVIEW-20261009/H07-INPUT.md)。有历史热点与当前代码入口；具体可消除成本、等价改法、写域和最小验证待讨论，未到READY/实施。
- H-06 Role限定准入/FindEffect已有成果保留；H-01本地AB草案后移到Android部署阶段。producer复用暂缓/OFF、H06-ROLE-EXACT不续办、95 OFF和历史失败保持。
- 正确性、0GC、容量、有序关闭与指纹作为受影响验证，不独立无限插入诊断。额外发现方向一律不跟进；专项/AI行为/资源/设置红线不变。
- 仅14份具名正文、索引及本Task/Operation审计文件；精确写域和实际dirty备份见before-manifest.json。无代码/资源/Scene/Prefab/Settings/Packages写入，无Unity/测试/Profiler/测量，无跨会话发送、监测/Goal恢复。

## 依据与限制

本轮读取当前总清单、进度、H07-INPUT/相关既有方案和最小依赖；复用91报告与混战ON JSON，确认输入约12.1—12.6ms、AI约4.94—5.11ms、kernel3.117646ms内含nearest1.506964ms均为旧嵌套成本，不相加、不作当前收益。
窄源码核对NTSD28InputTwoPassModule、SimulationAiDecisionModule和AiDecisionKernel实际分支；DataOrientedCanonical/direct入口通过canonical writer，不能假定任何旁路即实际生产路径。本轮不读活跃Q06方法体。
现有高优先方案可以先讨论，不等于新改法已安全有效；未选出具体方案不自动增加实验或微缓存。

## 验证

检查34父项/6子项唯一性与实际排列、导航/新增链接、真实备份、21具名保护、HEAD/暂存、已有范围外diff和git diff --check；实际结果追加于Operation/本Task。
未运行编译、测试、运行时或设备验收；不将文档重排称优化完成。下一动作仅与用户讨论H07-INPUT的具体实现范围。

## 本轮实际检查

2026-10-10T10:28:54.237550+08:00：14正文和索引已整理，34父项/6子项在总清单与进度中均唯一且按计划排列；122个新增/改动本地链接有效，15实际dirty备份完整，21具名保护0变化，HEAD/暂存/范围外已有diff不变，git diff --check退出0。见[验证](../FILE-OPERATIONS/NTSD-OPTIMIZATION-PRIORITY-ORDER-20261010/verification.json)。首项仍仅讨论，未运行编译/测试/测量或发送消息。
