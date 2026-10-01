# C052 静态回访：当前状态、整批终止与命中间隔门

状态：`STATIC_CORRESPONDENCE / POSITIVE_REST_REACH_PENDING`。只读核查，未修改生产或测试脚本，未运行正式根或原 Unity Scene。

当前 336B44 playable `SimulationTickDriver28::step` 先在 `simulation_tick_driver.cpp:698-700` 构建候选，再于 `:743-864` 按攻击者、候选顺序消费；`BattleWorld28::classify_ordinary_hit_eligibility` 在 `battle_world.cpp:5007-5015` 读取目标当前 action 的 state，effect21 遇 18/19 且 `victim_rest(target,attacker)==0` 时拒绝该候选并终止本攻击者余下候选。候选构建的 `scan_direction` 于同文件 `:4319-4329` 先递减/拦截既有命中间隔。正式资源 `data/data.txt` 索引了 OID211 `a/fir/fir.dat`、OID875 `c/ank/a/atk.dat`，两者有 kind0/effect21 攻击记录；这只证明内容入口存在，不证明下述正间隔组合可达。

Unity `BattleHitCandidateSequenceRunner.TryConsumeCaptured` 按候选循环，`TryConsumeCandidate` 在 `:173-179` 已读取目标当前帧 state，遇 18/19 返回 true 以终止该攻击者后续候选；现有 `BattleCollisionHitDamagePlayModeProbeEditor` 用 previous state0/current state18 和第二目标HP保持证明受控机制。Unity 此提前终止条件没有直接读取 `GetRawRestVrest`，而 native ordinary 在 `:103-106` 的首道 `CanConsumeRecordedCandidate(..., false)` 也不查 rest；后续普通命中前 `:255-257` 才检查。该写法在同 tick 前一候选改变同一 attacker-target 的 rest、而后一个 effect21 候选仍在已冻结列表中时，具有静态顺序差风险；本轮没有实际候选/命中 trace，不能称已证首差或要求修复。

下一回访应先用当前正式内容与完整 `GameSession28::step` 构造同攻击者、同目标的正 rest 后继 effect21，再放置该攻击者的第二目标，记录候选顺序、rest 和两个目标结果；有正式可达正例才做根 LFR 与 Unity 原 Scene 同态。若正式内容只在 rest=0 到达此门，就保留本项静态对应结论，不为不可达条件添加特例。C052、Q07 和总目标仍开放。
