# Task Contract — NTSD28-B2-EXIT-GATE-AUDIT-001

> 状态：`VERIFIED / B2-NOT-READY / NONAI-RNG-CROSSWALK-CLOSED / BLOCKER-FUNCTION-KEY-ROUTING / NEXT-FUNCTION-KEY-CROSSWALK`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2 EXIT`  
> 建立日期：2026-09-04

## 目标

在human/AI source-model joint trace和formal EXE behavior证据具备后，对总表I-01～I-10、R-01～R-07、
A-01～A-08逐项做B2退出审计。每项只能归为：本阶段已闭合、用户例外、由后续B3+ producer负责但B2
carrier/接口已闭合、或仍阻断B2；最终选出唯一下一实施包，禁止因已有局部trace直接跳到B3。

## 当前证据边界

- common/standing source-model exact input+dual RNG为3 ticks / 6 pairs全等；AI ticks1—2 exact+RNG全等，
  tick3首差为B11 action-content下游。
- formal EXE human/native-AI headless behavior均900tick exit0/passed；formal smoke不导出exact/per-call internals。
- DDJ/DRA production真实Play已通过；F1/F2/F5 synthetic/physical host证据与F7/F8/F9旧基线证据存在，
  但不得未经2.8逐项复核就视为I-07/I-10全部闭合。
- non-AI RNG、LFR、results continue、computer timer、later-pass producer可能属于B3/B4/B5/B7/B8/B10/B11，
  必须明确依赖归属和B2接口是否已足够，不能笼统延期。

## 允许操作

- 只读authority playable live path、Unity production/test和现有Task/Record/trace；运行只读搜索、现有测试或
  workspace Temp诊断。
- 修改本Task、Change Record、Ledger、STATE、handoff和总表的状态/路由。
- `code-path: NONE`；不得修改C#/C++/tool source、Config/DAT、Scene/Prefab、ProjectSettings、Packages或authority。
- 如发现真实阻断项，必须另立Task/Change并声明允许路径后再改代码。

## 验收

- 输出I/R/A共25项的逐项状态、现有证据、缺口、owner阶段与是否阻断B2。
- 复核所有B2 Change Record的“production unconnected / consumer unmigrated / pending”是否已被后续包supersede，
  或仍代表真实缺口。
- 选出一个最小、可测试的下一包；若B2可退出，则写明被B11内容或B3+ behavior阻断的仅是后续领域，
  并给出阶段退出证据；否则保持B2 active。
- Change Ledger validator通过。

## 回滚

仅回滚本次治理状态与路由文字；production与authority零修改。

## 审计结论

- B2不可退出。真实B2阻断族有两个：
  1. authority非AI的50个synchronized文本表达式与2个direct CRT消费尚未映射/迁移；
  2. F3～F10 route、auto-repeat/F3-lock/global-delay拒绝和host/session handoff未按2.8建立，Unity现有
     F7/F8/F9仍是旧合同。
- 下一唯一包：`NTSD28-B2-NONAI-RNG-CALLSITE-CROSSWALK-001`。先冻结fresh重计的50+2调用之live owner、条件、顺序、
  下游阶段和Unity候选；交叉表闭合后再拆最小production包。
- F4离场的结果/场景effect归B8，F11/F12音量effect归B10，I-08 Results continue归B8，A-06
  computer-timer tail placement归B3/B4；这些后续owner不授权忽略其B2 route/carrier接口。
- I-09 LFR格式兼容当前不是战斗等价的必要条件：Unity已有确定性FrameInput/journal，正式EXE行为已用内置
  smoke观察。若后续B12需要直接消费LFR，再另建adapter；当前不把格式不同伪装成已兼容。

后续`NTSD28-B2-NONAI-RNG-CALLSITE-CROSSWALK-001`已按fresh 50+2闭合库存并把未迁移consumer精确路由
到B3～B8/B11/B12；它不再是“未知交叉”阻断。B2当前唯一可独立继续的阻断为function-key route/reject。
