# 下一F02入口只读候选（未运行、非关闭证据）

> **2026-09-30 校正（覆盖下面未验证的 kind3 推断）：** 正式 catalog 的 OID123/type6 `w/8.dat` 动作0..5全部为 state3005，故 Anko/Haku/Lee 的 kind3/Vx100 释放**不能**触发 F02 的 physics-entry state1000 条件。OID101/124 的直接高速 OPoint 也从动作45/40进入 state1002；当前索引 DAT 对 101/124/422/600 的直接 state1000 OPoint 初速均不超过5。Temari19/action247 自然生成124后，正式 playable 源码前32tick始终 state1002、Vx从30到14。后续以新增F02-NATURAL-REACHABILITY-001合同有界延长并按真实 trace 裁决，不把这些候选写成验收证书。

当前父C023仍待Scene02完整32ticks，不能因这些静态候选跳过它。

当前playable BattleWorld::held/refill/wpoint约8144先把type1/4/6有dvx的释放对象选40，但随后同一事务若wpoint.kind==3，会以0x00418726同步RNG覆盖为action0..5，再把非零dvx作为Vx。这提供自然进入state1000且高速的候选，而不是直接初始Vx20或初始action40。须实际证明释放后的原物理入口帧/速度与F02前置条件，不能只靠静态DAT存在。

正式DAT四kind3且|dvx|>9项：Anko65/274 dvx100/dvy-1；Haku/280 dvx100；Lee7/255和555 dvx100。Anko271、Haku276、Lee251 OPkind2生成123/action20，OID123 type6。Anko272/273的普通attack/defend/jump可选274，下一候选优先从可自然生成held123的初态和正式输入继续普通释放链。叶影255没有独立held生成，不能初始255就声称证得释放。

之前负候选：Kankuro puppet OP422无dvx、扩散范围-5..5，不满足>9；普通WPoint非kind3直接选40也不证明F02；OID600在已检当前内容未找到OP600，不能据局部test可用就宣称自然case存在。rootLFR不携带独立初始Vx，已有手工速度4组源码/Unity只能按其局部证据保留。

不改任何DAT，不按OID补special case；下一正式source/root/Scene诊断须另建前置合同与Change，记录完整生成/释放/首帧物理事件。此文件仅是本轮读源码候选，未生成/运行额外case。
