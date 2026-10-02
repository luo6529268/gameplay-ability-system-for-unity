# Q07/C053 自然双命中逐 writer 字段见证

状态：`VERIFIED_SCOPED_UNITY_PER_HIT_ACTION / C053_OPEN`（2026-10-02）。当前规则权威为根正式 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 与对应 playable live path。本包在原 Unity Battle Scene 的三名**受控初始动作**角色样本中，直接见证两次真实命中 writer 后目标动作均为156；不据此关闭物理键自然选招、完整 World、根 EXE 三人同初态或 Q07。

正式 playable 完整 GameSession 源码 X580/580 样本的 tick7 [原始 CSV](../NTSD28-336B44-Q07-C053-NATURAL-DOUBLE-PRODUCER-001/source-run-01.csv) 依槽51→52记录 `51:0:2:156;52:0:2:156`，即两条 applied/effect2/post-hit action156，tick 后 OID808 action/锁存156、HP450。源码 `battle_world.cpp` 通用 type3 分支在 writer 中从 `frame.action_latch` 读取 `hit_Uj`，写 `frame.action=response_action` 和 `frame_counter=0`，**没有在该分支即时改写 `frame.action_latch`**。源码 CSV 的 tick6 action/锁存153、tick7尾部156/156与该顺序相容。

Unity 只在原有 `ShadowCompare` 的 `ObserveLegacyWriterEffect` 已取到真实 writer 后快照时，把 `target.Frame.N`、`target.Trans.WaitCounter`、`target.Health.HP` 三个标量按预分配 entry 保存到只读 view；真实命中 writer、判定、DAT/场景/音频队列没有改。原 Battle Scene `LoganRuntime`、mode0/difficulty0、seed682973786、slot0/2 OID65/action511/X580、slot1 OID702/action553/X500，后续中性输入。两个 OID875 和目标 OID808 仍由正式内容的生产 OPoint 自然生成。

首轮[原场景结果](ank580-ank580-jira500-per-hit-writer-01.json) 为 `FIRST_DIFFERENCE`：测试错误地要求每次 writer 后的 `Trans.WaitCounter` 立即变156。实际两条为 `51:156:153:475;52:156:153:450`，两条 Damage/consume/writer 的 ShadowCompare 差异0，tick尾锁存156。核对正式源码写入顺序后，修正**测试断言**为即时锁存153，并使用新 request/session/runId；首轮请求和结果均保留，没有覆盖。

第二轮[原场景结果](ank580-ank580-jira500-per-hit-writer-02.json) 为 `SCOPED_PASS / DONE`：相对tick7仍严格按 slot51→52记录 `51:156:153:475;52:156:153:450`；目标 tick 尾 action/锁存156、HP450、两个攻击者的 victim-rest各10。该tick两条 `Damage` 的 consume 指纹预测/实际各自一致，observed writer2，计划 `mismatch=0`、`failure=0`、writer差异掩码0。12个完整生产 Driver tick 的[直接源码对照](source-unity-comparison-v2.json)为选定战斗字段132/132零差，10个待播事件按相对tick的路径/世界X/顺序均一致；上一轮同初态完整事件字段对照为42/42，本轮也与上一轮所选180字段零差。正式源码 CSV 未逐hit输出目标HP，475/450仅为Unity直接观测；不能声称原生逐hit HP已配对。

验证：生成Editor工程两轮编译分别0 error/280 warning与0 error/249 warning，原Editor刷新后 `Assembly-CSharp.dll` / `Assembly-CSharp-Editor.dll` 时间晚于对应脚本。两轮均在原项目原 Battle Scene 运行并退出；第二轮 `exitedPlay=true`、`sceneCleanAfter=true`、Scene前后SHA一致，Editor最终idle/非Play/非编译。[四保护资产前](protected-before-v2.json)/[后](protected-after-v2.json) SHA全相同，`LoganRuntime`与四保护资产无Git工作区差异。未启动第二个 Unity 项目或 Editor、未用computer-use、未删除/还原文件。

变更审计：`Tools/Validate-ChangeLedger.ps1` 第二轮[记录](change-ledger-validation-v2.txt) exit0/PASSED，1125 Records及当前4个受治理脚本均有覆盖；`git diff --check` exit0。第一次插入 Ledger 误将本 ID 同一行加入四处表格，已在[Change Record](../../../docs/ai/CHANGE-RECORDS/NTSD28-336B44-Q07-C053-PER-HIT-WRITER-001.md) 预记原因和精确范围，只保留第5行、移除本包新增的其余3条重复行，未触及其它记录。

边界：根正式 EXE 的 LFR 参数不能覆盖第三初始槽所需action511，本组三人控制初态无法直接作为根 EXE 同条件逐hit证书；受控初始动作也不是玩家物理键自然选招。局部两次 Uj156 动作与源码逐hit记录相符，Q07/C053父项、全World与整场验收仍开放。后继按新版总表筛其它正式可达首差，不重复这组已证末态。
