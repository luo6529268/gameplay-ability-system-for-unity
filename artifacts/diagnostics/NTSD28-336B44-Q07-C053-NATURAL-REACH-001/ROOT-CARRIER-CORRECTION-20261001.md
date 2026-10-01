# C053 正式根 LFR 失败阶段归因更正

状态：`READ_ONLY_CARRIER_AUDIT / C053_OPEN`。本次只读既有 336B44 正式根报告、trace、CLI 参数、C053 源码夹具及 C052 成功根 trace；未重新执行 EXE，未修改生产脚本、DAT、Scene 或旧原件。

原报告把 C053 的 failure46 归因为“headless 只重建两名战斗者”，该表述已被原 trace 否定。C053 根 trace 的 tick0 实际有三实体：slot0/OID702/action553、slot1/OID2/action0、slot2/OID875/type3/**action0**。源码夹具 `type3_latched_uj_reachability_probe.cpp::make_config` 为第三实体设置 **action55**。根 trace tick1 仅剩 slot0/1 和新生 OID808/slot50；OID875 消失，因此 LFR 首行 HP 总和正式源码1500、根1000，报告在 `playback_tick_or_checksum` 给出 failure46。仅从 tick0/1 无法断定 OID875 消失的完整内部原因，但初始动作已在 tick0 不同，足以否定同初态证明。

CLI 参数和正式 `source/ntsd28_playable/src/main.cpp` 仅有 `--lfr-slot0-action`、`--lfr-slot1-action`；既有 C053 参数设 slot0=553、slot1=0，没有 slot2 动作覆盖。正式 playable README 同样只声明这两个动作参数。对照 C052 的三人根 trace，slot2/OID2/action0 从 tick0 保留到 tick1且回放通过；说明“全局只能重建两人”的旧推断错误。C053 此案不能用原 LFR 与正式根证明四对象/双命中同态，亦不能把失败视作 Unity 规则缺陷。

C053 源→原Unity Battle Scene 的受控8tick×13字段104/104零差仍独立有效；逐hit瞬间锁存、对象池借用、真实玩家选招及正式根同初态门继续开放。后续若要正式根证书，须找到不需要 slot2/3 非默认动作注入的自然入口或权威支持的同态载体，不更改正式 EXE/规则来适应诊断。
