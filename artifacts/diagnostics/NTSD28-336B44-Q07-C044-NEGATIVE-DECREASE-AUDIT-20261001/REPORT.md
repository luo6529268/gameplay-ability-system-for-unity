# C044 负 `decrease` 入口与写者只读审计

状态：`FORMAL_CONTENT_CANDIDATE / RUNTIME_CROSS_ZERO_PENDING`。只读当前336B44 playable源码、正式 `resources/runtime/decoded_dat/c/gaa/gaa.dat` 与 Unity `BattleCpointWriter.RunKind1`；没有运行 C044 同态样本或修改生产。

正式 OID16 的四个 kind-3 抓取入口分别是 action245/246→120、363→400、364→420、365→370，受害者均进130。各自然帧链末端的 kind-1 负 `decrease:-3` 帧为 action128、407、427、377；它们同时含 `throwvx:3 / throwvy:-5 / vaction:180`。当前源码 `BattleWorld28::advance_catch_relations()` 在负 decrease 将 `catch_timeout_94` 降到零以下时，先选抓取者action0/被抓者action181，并写 `pending_hit_impulse` 的 contribution count 与X±4/Y-3，然后从该关系分支 `continue`；即本轮不继续执行后面的 `throwvx` 分支，也不在这一处直接写 `motion`。Unity `BattleCpointWriter.RunKind1` 同一跨零条件当前写 `KnockbackVx/Vy` 后立即写 `Runtime.Vx/Vy`。这是静态写者时序候选，尚非运行首差。

下一定向门：从正式 OID16/action245、363、364、365 与正常目标/中性输入、近远位置做完整 `GameSession28::step()` 筛选，记录关系确立、每 tick `catch_timeout_94` 和终端帧；若自然未跨零，不能假装已触发 C044。再用受控低 timeout 正反条件在源码 pass 与 Unity 共用 writer 上确认跨零首差，分清 `pending_hit_impulse` 与物理消费后的速度；根 LFR 初态若不承载 timeout，则受控低值不可冒充正式根同态。任何生产修复前另建准确 Task/Change。项目自有可行走区、位移比例、DAT 值、Scene及非战斗代码保持既定边界。
