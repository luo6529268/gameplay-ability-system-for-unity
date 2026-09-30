# Q07 千代相对 tick 54 动作分叉：只读归属审计

状态：`READ_ONLY / FORMAL_0X82_BRANCH_WITNESSED / UNITY_RNG_CALL_PENDING`。本报告不把所选字段的 1 tick 差异判为生产规则错误，也不关闭 Q07。

正式根 EXE 身份及配对 playable 出口沿用 `NTSD28-Q07-CHIYO-CONTROL-NATURAL-PLAY-001/ACCEPTANCE-20260928.md`。其正式 run2 CSV 与 Unity 原 Battle 完整 Driver 的同相位、同相对 tick 已选比较记录：相对 tick 53 双方动作 3；tick 54 正式动作 60、Unity 动作 65；tick 55 双方动作 61，随后所选动作再次相同。OID419、OID854 的数量及首傀儡动作各 120/120 同值。该比较没有冻结同一初始 World、Stage/BGM 和同步 RNG 状态或此前全部 RNG 调用，因此不能由 tick 54 一处动作分叉推出代码规则差异。

配对正式源码 `source/ntsd28_core/src/simulation/input_routing.cpp:821-850` 的无关系普通站立攻击分支，在无 kind6 覆盖时执行 `synchronized_next(0x82, 2)`，以 `(selection + 12) * 5` 选择动作 60 或 65。正式根回放 `NTSD28-Q07-CHIYO-PUPPET-AD-NATURAL-001/run2/formal-root-replay-trace.jsonl` 的 tick53/54/55 记录进一步显示：千代 interaction 均为0；同步 RNG calls 为1784/1856/1927，tick54 比相邻正常步额外多1次；tick54 `lastCallSite=130`（十六进制 `0x82`）、index1856、counter622、动作60。这证明正式端该 tick 走到 `0x82` 选择入口，返回0；trace未单列逐调用参数或 kind6 timer，不扩大成全随机流证明。

Unity 的 `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCharacterActionWriter.cs:1360-1378` 对应分支也使用 `SynchronizedNext(0x82u, 2)` 与同一公式；`Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterActionResolver.cs:106-123` 的另一站立动作入口也使用同一调用点和公式。Unity 随机流的选择值依赖其表索引及 counter，见 `Assets/NTSD/Scripts/Simulation/Core/NTSD28NativeRandom.cs:348-367`。Unity 当前 Play 报告没有记录该 tick 的调用点、选择值及入场随机状态，故尚不能证明 Unity 动作65正由同一分支返回1，也不能归因于初始化差异或上游额外 RNG 调用。

下一最窄判别门：只在原有千代同一固定输入、相对 tick 54 附近，记录 Unity 调用前同步 RNG 的 index/counter/calls、0x82 调用是否发生及其返回值，同时记录 interaction/kind6 前置门；与已有正式逐 tick RNG 状态先核进入此调用点之前的序列。若两端同状态同调用序列仍分叉，才回到战斗逻辑；若初态/此前调用不同，先定位首个 RNG 差异。物理键同相位问题单列，不能靠改 RNG 或 DAT 掩盖。不得重跑角色/时机矩阵，也不修改正式 DAT、相机或非战斗代码。

本次只读现有报告和源码，没有执行新 Play、编译或更改脚本、DAT、Scene。R06 复活回访的 producer 前置仍未满足，旧 13 记录/416 字段以及受控完整 Driver 证据不替代自然死亡到复活链。
