# Task Contract — NTSD28-B0-BASELINE-EXIT-AUDIT-001

> 状态：`VERIFIED / B0-BASELINE-READY / B1-READY / GOVERNANCE-ONLY`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B0 EXIT`  
> 建立日期：2026-09-03

## 目标

审计B0定义“2.8/Unity双端schema、输入、RNG、slot、实体字段和comparator”是否已具备可进入B1的
真实、可恢复基线；明确区分“基线就绪”与“行为已对齐”，并把已测差异/缺失路由到后续阶段。

## 结论

`B0_BASELINE_READY / B1_READY`。依据：

- full trace v2合同、47字段schema、entity raw validator/comparator已存在；当前maturity 38 verified /
  0 candidate / 9 missing，missing未被0值掩盖。
- authority source-model与Unity Editor exporter都能从同一OID2/7、同seed、同非空三tick场景输出
  completed-tick raw；各自两次确定性、validator通过。
- applied input mask两侧相同：`(17,2)/(1,96)/(0,12)`。
- slot capacity 1000/400按用户例外处理；同场景occupant/epoch/lifecycle shared domain相等。
- RNG真实拓扑差异被显式冻结：authority CRT+synchronized，Unity deterministic；调用向量
  authority两流均0/0/0、Unity1/2/1；没有伪映射。
- domain first-difference与entity first-difference均可重复输出；工具正反例、build、Unity compile/
  focused与Ledger证据闭合。

## 后续路由

- B1：33ms/3ms、Host/flow/pause/step/追帧；当前固定30Hz旧合同不得继续定义新权威。
- B2：input phase/proxy与双RNG生产实现、call-site/per-call trace；当前RNG topology是首差。
- B4/B5：runtimeState/environment/collision/platform/armor等字段与writer。
- B7：lifecycle pending/code与真实birth/death/reuse场景矩阵。
- B11：`baseMaxMp`及内容/数值，继续等待用户策略，不在B0修改。
- B12：正式EXE runtime/全场景证书与长时无漂移；当前source-model capture始终certificate false。

## 边界

本审计不修改任何脚本、runtime、DAT、Scene、资源或权威目录；不把B0写成战斗已对齐。

