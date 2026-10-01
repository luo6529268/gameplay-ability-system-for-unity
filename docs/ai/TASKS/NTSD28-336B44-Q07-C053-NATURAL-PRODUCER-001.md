# NTSD28-336B44-Q07-C053-NATURAL-PRODUCER-001

状态：`VERIFIED_SCOPED_SOURCE_ROOT_PRODUCER`。父项为新版 336B44 G1 / BATCH-04 / Q07 / C053。

目标：验证正式资源能否通过原生 OPoint 在完整 `GameSession28::step` 中生成 C053 所需的 OID875/action55 攻击者，从而绕开既有根 LFR 只能覆盖槽0/1初始动作、不能重建槽2/3 action55 的限制。此包只建立正式源码的可达或有界阴性证据，不修改 Unity 生产、DAT、Scene、Prefab、项目背景/模式或非战斗功能。

权威入口：根正式 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`；对应 playable `GameSession28::step`、`SimulationTickDriver28::step`、`object_spawning.cpp` 与 `frame_machine.cpp`。正式 `data/data.txt` 中 OID65=`c/ank/ank.dat`、OID702=`c/jira/sag.dat`；安科 action511/512/513 的 OPoint 为 OID875/action50，OID875 `c/ank/a/atk.dat` action50 `next:55`；自来也 action553 的 OPoint 为 OID808/action150。静态前驱不是运行证书。

改动范围：只新增 `Tools/NTSD28Q07Diagnostics/c053_natural_producer_probe.cpp`，输出放在新 `artifacts/diagnostics/NTSD28-336B44-Q07-C053-NATURAL-PRODUCER-001/`。诊断使用正式资源、固定 seed、不同有限 X 间距以及两名可由根 LFR 槽0/1动作覆盖的初始角色，记录每 tick 实体生成、动作/锁存、命中候选和 LFR；先验证 OID875/action50→55 与 OID808 同一世界自然共存，再看当前/锁存分读条件及实际 hit。无阳性必须记录搜索边界，不推断全局不可达。

验收：诊断由当前正式 playable 闭包源码编译0错、同输入双跑逐SHA相同，输入和输出路径不覆盖旧证据。只有源码自然阳性且根 LFR 同初态回放通过，才考虑原 Battle Scene 的对应定向验证；本 Task 不预先宣布根或 Unity 同态。若编译、时序、几何或 LFR 受阻，保留结果，C053/Q07/总目标开放。

风险与回滚：诊断编译量较大，初态的帧动作虽可由正式根槽0/1覆盖，但两技能物理生成时间和位置可能不形成双命中；受控 OID875/action55 旧案例不能代替自然正例。回滚仅审阅本 Task 新增文件和报告，不清理、覆盖或移动任何既有用户文件；若需删除产物，另按删除审计取得授权。脚本修改前须有本 Task、Change Record、Ledger/STATE/handoff 登记。

运行结果：[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-NATURAL-PRODUCER-001/REPORT.md)。正式源40tick有限位置组找到五个单Uj阳性，固定X610源双跑14/14输出文件同SHA；根336B44同LFR exit0/passed且40tick×12字段480/480相同。源OID875/808由OPoint自然出生，但两初始角色动作受控、双Uj未达、Unity原Scene未验。本Task只关闭声明的producer和根回放入口，父C053/Q07开放。
