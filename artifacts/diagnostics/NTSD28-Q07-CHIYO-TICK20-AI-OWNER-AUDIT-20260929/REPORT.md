# Q07 千代同步随机首差的正式侧归属（只读）

状态：`FORMAL_OWNER_IDENTIFIED / UNITY_OWNER_PENDING / Q07_OPEN`。本次只解析已保存的正式根回放轨迹及其 paired playable 写者，没有重跑正式版或 Unity，也没有修改生产代码、DAT、Scene、资源或非战斗逻辑。

证据输入是 `artifacts/diagnostics/NTSD28-Q07-CHIYO-PUPPET-AD-NATURAL-001/run2/formal-root-replay-trace.jsonl` 的相对 tick19–20，以及已验收的原 Battle 同条件 Unity 120 tick 报告 `artifacts/diagnostics/NTSD28-Q07-CHIYO-CONTROL-NATURAL-PLAY-001/chiyo-canonical-rng-20260928-01.json`。前一包已确认两端同步 RNG 调用数在 tick1–19 都为0，而 tick20 正式为7、Unity为6。

正式 tick19 出现 OID850/slot51/action311 和 OID204/slot52/action60；tick20 OID850/slot51/action312，正式同步 RNG 从0增至7。该 tick 的唯一 `native_ai` 事件记录 `slot=51, rngCalls=7, selectedSlot=1, primarySlot=1, primaryDistance=702`。paired playable 的 `SimulationTickDriver28::step` 在每个 `NativeAi28::step_main` 调用前后读取同步随机调用数，并写入 `native_ai_synchronized_rng_calls[slot]`；场景诊断由该字段生成 `native_ai.rngCalls`。因此正式侧这7次调用均归属 OID850/slot51 的原生 AI 步，而不是仅凭末调用点 `0x38` 猜测归属。

Unity 旧报告只记录每 tick 的随机总数，没有捕获 OID850 对应 runtime 实体的 AI 提交、目标、逐调用序列或逐实体归属。正式与 Unity 均以 `0x38` 结束 tick20，不证明 Unity 少的调用点也是 `0x38`，也不证明两边第1–6次调用一致。当前不能据此修改共享 AI 规则或将缺口归因于 D-024 比例坐标。

下一最小验证只在同一固定输入的 tick19→20 记录 Unity OID850 的出生/AI资格/目标，以及直调和接受提交的逐调用序列；对照正式侧 slot51 的前置状态和调用顺序，先找第一处不同。保留已过 120 tick 自然链证据，不扩大角色矩阵。Q07/BATCH-04 和总目标继续开放。
