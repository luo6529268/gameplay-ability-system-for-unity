# Q09/P-08 随机初态可比性只读复核（2026-10-05）

结论：旧第三轮“鸣人攻击动作 60/65”不构成正式版与 Unity 战斗规则首差。两边角色、位置、HP、输入及 CRT state/calls 相同，但该选招实际消费的**同步随机表**不同。此复核不修改生产脚本、DAT、Scene 或原始测试结果，也不把旧 130/132 选定字段提升为 132/132 通过。

## 可复查的因果链

1. [正式录制探针](../../../Tools/NTSD28Q09Diagnostics/ita_equal_hp_root_lfr_probe.cpp)第 31 行设置 `config.random_seed = 2833`。正式 `source/ntsd28_core/src/simulation/native_random.cpp::NativeRandom28::reset_from_seed` 先用种子生成 3000 项同步表；按该函数的 MSVCR80 公式及 FNV-1a 哈希复算，种子 2833 的表哈希是 `f75f85682ee9412c`，3000 次后的 CRT state 为 `1671198313`。
2. `GameSessionLfr28::begin` 将当时完整同步表写入 LFR manager；`GameSessionLfrPlayback28` 从 manager 读回该表并标为 `locked_lfr_manager`。正式 [根 EXE 回放 tick0 原始 trace](../NTSD28-336B44-Q09-P08-EQUAL-HP-ROOT-LFR-001/root-playback-trace.jsonl)记录表哈希 `f75f85682ee9412c`。回放的 `BattleConfig28.random_seed` 默认为 0；`GameSession28::initialize` 先用 0 重置 RNG，再单独恢复 manager 的同步表，所以 tick0 CRT 为 `3374725112/3000`，与种子 2833 的完整 RNG 初态并不相同。这是回放格式造成的混合初态，不能只核对 CRT 两个标量。
3. [旧 Unity 第三轮原件](../NTSD28-336B44-Q09-P08-ROOT-UNITY-SAME-STATE-001/ita-equal-hp-336b44-unity-03.json)的探针调用 `NTSD28NativeRandom.ResetFromSeed(0)`；按 Unity 同一表生成公式复算，表哈希为 `68c5f16327d0ddd7`、CRT state/calls 为 `3374725112/3000`。原件的 `initialTableHash` 十进制 `7549705758528560599` 也等于 `68c5f16327d0ddd7`。
4. [旧机械对照](../NTSD28-336B44-Q09-P08-ROOT-UNITY-SAME-STATE-001/third-run-field-comparison.json)的 132 个选定字段有 130 个相同；仅相对 tick2/3 鸣人动作正式 60、Unity 65。正式根 tick2 记录同步表 `index=1/counter=1/calls=1/lastCallSite=130`；正式 `input_routing.cpp` 与 Unity `BattleCharacterActionWriter.cs` 都以 site `0x82` 的 `SynchronizedNext(..., 2)` 计算 `(value+12)*5`。复算所得表项 index1：seed2833 为 163，`(163+1)%2=0`，得动作60；seed0 为70，`(70+1)%2=1`，得动作65。这准确解释了该处动作数值，无需假设输入规则、帧机或碰撞逻辑出错。tick8 目标 HP10 与 tick22 一条 1×3 血点命令在旧 Unity 第三轮已恢复。

## 当前生产入口复核

当前 `NTSD_Battle.unity` 序列化 `effectiveAiExecutionProfile: DataOrientedCanonical`。`SimulationWorld.UsesNTSD28NativeInputPipeline` 在该 profile 下为真；`RunCharacterInputPasses` 第二趟调用 `NTSD28InputTwoPassModule.ProcessNativeSampledState`，后者调用 `BattleCharacterActionWriter.RouteNativeGroundBuiltins`，再进入上述 `RouteNativeStandingAttack`。正式 `route_native_standing_attack` 与 Unity 当前入口的无持有、kind-6计时、随机二选一、资源不足钳零及 MP 消费统计分支，在已读代码中顺序一致。旧 `LF2CharacterActionResolver.ProcessStandingActions` 只在非 native 路由/兼容分支；`LF2Entity.RunSharedCharacterDatStandingActionInputPhase` 亦由 `UsesNTSD28NativeInputPipeline` 提前返回隔离。不能因为这些旧分支还存在，就把其中 `BattleRandInt` 当作此生产案例的随机首差。

直接开局与这条固定 BGM 录制案例不可混用：正式 `BattleConfig28.bgm_selection_49f18c` 默认 0，在正式 `resolve_native_bgm_selection` 中会先消费一次同步随机数；录制探针明确设为固定 BGM 2，故不消费。Unity `SimulationTickDriver` 的直接开局调用 `ResetForDirectBattle(seed)`，也先消费一次 BGM 随机数；旧 Q09 Unity 探针则明确改用 `ResetFromSeed(0)`，正是为了和固定 BGM 的 LFR 初始索引 0 配对。直接开局的 BGM 选择内容是否与正式版相同属于独立音频边界；本条只判随后战斗随机流的调用相位，不能将固定 BGM 夹具的初态要求无条件套到所有开局。

这是调用链/字段的静态复核，不是当前程序集的 Unity Play；profile 若改变，必须重新判定实际入口。正式代码路径还需在具体新首差出现时以同完整初态的运行证据裁决，不能仅凭静态相似宣称全部战斗已对齐。

## 调度边界

旧 Unity 第三轮还早于 Q12 的共用三键重复映射纠正，不能自动当成当前生产程序集的验收。当前总表将此 Record 归 `REUSE`，P0/DEP/ONE 均为 0；本次只闭合“为什么此旧对照不可裁决”的证据，不新增必跑 Play 或生产改动。以后若玩家在当前版复现攻击动作/伤害异常，或共享随机写者改变，才用同角色、**同完整 RNG 初态**、同输入 tick 做一例定向比较；比较应覆盖实际消费的表项、动作/命中/HP 等战斗结果，并按用户例外单独判断画面，而不是逐角色逐操作穷举。

本报告的代码结论来自正式 336B44 对应 playable 源码与仓库现有 Unity 实现；正式根 EXE 的表哈希及旧 Unity 值来自上述原始运行结果。本轮未启动新 Play，也未新增 132 字段同态证书。

## 2026-10-05 后续触发时的最小同态入口（只读核对）

现有 Unity `NTSD28NativeRandom` 已有 `CaptureSynchronizedState()` 和 `RestoreSynchronized(state)`；后者只克隆同步表/索引/计数，不重置 CRT。因而如果未来同一攻击案例出现当前版可复现首差，测试夹具可在 `world.NativeRandom.ResetFromSeed(0u)` 后，从独立的 `NTSD28NativeRandom(2833u)` 捕获同步表并恢复到 World，复现正式 LFR 的“seed0 CRT + seed2833 同步表”混合初态。正式 `game_session_lfr.cpp` 从 LFR manager 恢复表、`game_session.cpp` 初始化后单独恢复表的调用链与此对应。运行前必须核对 CRT state/calls、同步表哈希、index/counter/calls、BGM 固定选择和输入相位；只相同两枚 CRT 标量不够。该入口利用现有 API，不要求修改 DAT 或生产 RNG 规则；本条没有执行新测试、也不把条件门提升为必跑任务。
