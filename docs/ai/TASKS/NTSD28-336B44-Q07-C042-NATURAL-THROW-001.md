# NTSD28-336B44-Q07-C042-NATURAL-THROW-001

状态：`RUNTIME_PENDING / NATURAL_THROW_PATH_SCOPED_PASS / NONZERO_TRIGGER_PENDING`。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`，BATCH-04/Q07/C042。权威为正式根 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 及对应 playable `GameSession28::step()`、`SimulationTickDriver28::step()`、`BattleWorld28::advance_catch_relations()`。现有 C042 受控机制证据见同 ID 父包 `ACCEPTANCE.md`；自然根/原 Scene 尚待。

只新增 `Tools/NTSD28Q07Diagnostics/bee_natural_throw_lfr_probe.cpp` 及本包 `artifacts/diagnostics/NTSD28-336B44-Q07-C042-NATURAL-THROW-001/`。正式 OID75 `bee.dat` action355 kind-3 抓取成功转358/130，358～375 的 next 链到375投掷。探针以正式资源和完整 `GameSession28::step()` 从 OID75/action355 对另一战斗角色 action0 开始，固定种子、双方正常 HP/team/位置、每 tick 中性输入。近距和远距独立运行；记录双方 action、frame counter、关系、timeout、HP、位置/速度、RNG、帧/命中事件，并录 LFR。任何结果均保留，不能手设抓取关系、被投者计数或中途动作，不能把静态 DAT 链算阳性。

阳性标准：近距真实建立 reciprocal relation，随后在未强制字段的完整 tick 中到达 `throw_vx` 投掷分支，投掷前被投者计数非零，并有远距不投掷控制；同 seed/input/source tick 可由 LFR 在正式根运行且逐 tick 可比。源码阳性后才另建原 Battle Scene 探针 Task/Change；若源阴性，只记录位置/链路首阻并选择其他正式可达入口，不改生产。编译失败须留证修复；测试从最窄样本开始。守住原 Unity/GAS 框架、DAT 值、Scene、Prefab、ProjectSettings、菜单/结果和其他非战斗功能；禁 computer-use、第二 Unity 项目。回滚仅审阅本 ID 的诊断文件差量，保护已有脏工作。

范围增补（v1结果后、v2探针脚本修改前）：action355/X550完整Driver第1tick抓取、第55tick投掷，X1200无抓取；X530～600也投掷但被投者投掷前计数均0。正式根同LFR经修正角色75/action355启动参数后报告PASS；CRT state为独立host种子。当前自然链未触发C042非零计数，故仅在已声明同一脚本中增加正式DAT已确认的action378另一抓取→action78投掷分支选择，限制仅355/378，其余种子、对象、输入不变。根首次旧参数失败保留，原Scene仍待。

限定结果：正式源码action355近距tick1抓取/tick55投掷，action378近距tick1抓取/tick142投掷；两远距控制均阴性。两近距LFR经336B44正式根报告PASS，限定实体/RNG字段1360/1360及2720/2720零差；独立CRT state/根额外EOF排除。两投掷前被投者计数都为0，未达到本Task的C042非零计数阳性门；原Battle Scene未运行。按G1转C044可达首差，C042保持RUNTIME_PENDING。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C042-NATURAL-THROW-001/REPORT.md)。
