# NTSD28-336B44-Q07-C053-NATURAL-REACH-001

状态：`RUNTIME_PENDING / SOURCE_GENERIC_UJ_REACHED`。父项 C053 `hit_Uj` 锁存帧读取与 NTSD28-UNITY-BATTLE-REALIGNMENT-001 / G1 / BATCH-04 / Q07。此包只验证正式内容可达性，不关闭父 C053/Q07。

权威：当前根 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`，对应 playable `GameSession28::step`、`SimulationTickDriver28::step`、`BattleWorld28::resolve_confirmed_unarmored_standard_hit` 与正式 `resources/runtime/decoded_dat`。`data/data.txt` 的 OID702/63/808 分别对应 `c/jira/sag.dat`、`c/hir/hir.dat`、`a/kat/kat.dat`；前两者 frame554 含 OID808/action150 OPoint。OID808 的153/154接地 `hit_g:155`、155→156 与 Uj 字段构成只读候选，是否同 tick 被命中未知。

准确代码范围：新建 `Tools/NTSD28Q07Diagnostics/type3_latched_uj_reachability_probe.cpp` 一处，只调用当前正式 `GameSession28`、正式 DAT 与完整 Driver，记录每 tick 的 OID808 子体槽/动作/锁存/位置、候选/命中结果、双方 RNG；保留所有失败和阴性。诊断初态明确用 OID702或63 的 action554、X500/Z400，与 OID2 的中性对手不同 X 距离，种子682973786、mode0；此 action 起点是受控正式 DAT 帧，不能称物理键自然选招。只在证明 OID808 子体实际出生后观察153/154→155→156和标准命中的组合；不得制造被测结果或凭静态邻接标阳性。必要时输出当前正式 GameSession LFR，根 EXE 能消费后才做正式根同态。

不修改正式 source、DAT、Unity C#、Scene、Prefab、GameConfig、GAS 或非战斗流程。不启动第二 Unity Editor、不用 computer-use。验证为诊断编译0错误、相同输入双跑一致、逐 tick 事件与阴性对照分开报告；若没有 C053 正例，记为有界阴性并转寻其它可达初态。回滚仅审阅本 ID 的新增诊断与文档，不处理工作树已有内容；脚本修改前建立本 Task/Change/Ledger/STATE/Handoff，交付前运行 ChangeLedger validator。

首轮 v1 使用初态 action554 跑 OID702/X550、700、1200 和 OID63/X550、700 五例，各120tick均无子体；该初态从未经过正式帧计数0的 OPoint 入口，不能当作这些角色的自然生成阴性。当前 `simulation_tick_driver.cpp::materialize_native_frame_zero_entry` 明确要求帧计数0，而正式两个角色的 frame553 为 `wait:0 next:554`。同一已声明诊断路径的 v2 改为从正式前驱553进入554，并记录 actor frame counter 与 `spawns.spawned`，保留v1全部失败输出；不改 DAT 或正式规则。只有 v2 真正出生 OID808 后才继续判定 C053 后续门。

v2 当前五例均由生产完整 tick 在tick1生成OID808/slot50，tick6到153、tick7接地转155、tick8到156；`childHits=0`，没有 Uj 响应，不能关闭C053。编译0错误。下一步在同一诊断脚本中按原 Task 预定增 `GameSessionLfr28` 录制和最终LFR输出，仅对当前已证子体正例用根336B44回放，保留上述零命中结果；再找合法攻击者同 tick 命中，不改生产规则/内容。

v3 正式OID702/X700双跑的tick/RNG/LFR各同SHA；根336B44接受同一LFR，报告 `passed=true`，逐tick所列16字段×120tick=1920/1920零首差。此仅证明OID808生成及153→155→156链；零命中，C053仍待。下一诊断 v4 仍在同一准确脚本路径，加入**可选**正式第三战斗对象 OID875（`c/ank/a/atk.dat`）action55、team2、配置X，原两人/中性输入路径保持，用其持续 `itr.kind0/effect2` 探索同帧 OID808 命中。正式 kind 表未见875→808 特化记录，仍须以实际 `WorldStandardHitResult28` 验泛型Uj。先限定少数X位置；三对象初态是受控设置，不称自然玩家技能，也不假定根LFR支持三对象。修改前后分开归档，不动 DAT。

v4 实际：X580/620/650/700 四组各120tick，OID808 均tick1出生、tick6到153、tick7被正式OID875 effect2标准命中且响应156（每组1次）；当前与锁存两帧的Uj同为156，不能验证C053分读。X620根LFR回放failureCode46，首tick HP总和1500/1000揭示CLI仅载两人，不能判规则差异。三人阳性只各单跑，原Scene和可区分Uj条件待。完整边界及原件见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-NATURAL-REACH-001/REPORT.md)；本包不继续为相同响应值追加无鉴别力位置组合，转下一个正式可达首差。

v5 脚本修改前追加**同一 C053 可达性诊断**：现有 v4 OID702/X700 + OID2/X700 + OID875/action55/X620 在tick7命中OID808一次，双跑CSV/RNG/LFR各SHA相同。正式 `battle_world.cpp` 的命中消费按攻击者槽位升序、victim-rest按攻击者分别记；正式 OID808 frame153 的Uj=156，而首次命中后的当前156无Uj（默认20），这使第二个同tick命中成为分读鉴别条件。只在同一诊断脚本加入**可选第四**正式OID875/action55/team2、独立X参数与同tick每条子体命中状态/响应明细；原两人/三人模式和旧输出不覆盖。源完整Driver仅测试少数第三/第四X组合、双跑阳性与阴性；若第二命中被其它门挡住，按真实结果记录，不能制造分歧。四人根headless能力未证，v4三人已证不能由两人CLI回放，先不将失败误作战斗规则。仅此诊断CPP可改，不改正式源码、DAT、Unity生产、Scene或非战斗。若有鉴别力正例，再另包做原Battle Scene和正式根可用载体验证；若没有则标条件待触发。

v5 实际：正式 OID875 槽2/3 X620/620、580/620、620/650、620/700 四组完整120tick均在tick7各两次 `applied/effect2/Uj156`，以独立攻击者victim-rest通过；X620/620的CSV/RNG/LFR双跑逐字节相同。首击后当前动作156、锁存仍153的鉴别解释来自正式源码分支/顺序及最终响应，逐命中锁存值未直接记录。四人根CLI回放未做，因为现有三人回放已证载体只重建两人；原Unity Scene未验。本诊断 `SOURCE_DISCRIMINATING_UJ_PASS / UNITY_SCENE_PENDING`，父C053/Q07开放。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-NATURAL-REACH-001/REPORT.md)。
