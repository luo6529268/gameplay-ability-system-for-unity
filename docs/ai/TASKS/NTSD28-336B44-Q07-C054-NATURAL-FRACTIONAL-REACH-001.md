# NTSD28-336B44-Q07-C054-NATURAL-FRACTIONAL-REACH-001

状态：`SOURCE_SCOPED_NEGATIVE / RUNTIME_PENDING`。父项 C054 / G1 / BATCH-04 / Q07 / NTSD28-UNITY-BATTLE-REALIGNMENT-001。

权威与原状：当前336B44正式 `battle_world.cpp::advance_native_fusions` 在解融合时把恢复伙伴的精确XYZ从主角整数XYZ重建；Unity共用TrySplit已按此规则修正、原Editor聚焦1/1，正式源码受控注入小数的条件样本已通过。正式 `data/fusion.dat` 第二记录为OID10/11→52、HP<375、state2、decrease200、wait100、cover1；`data.txt`对应Deidara/Sasuke/Kyu，OID10/11 DAT均声明action9/state2。记录第二项的OID52 action310在DAT文本未声明，但正式`DatDocument::frame`对0..998空帧返回零记录，不能据此判断融合不可达。当前未证明正常完整tick里减时到零前主角精确位置自然带小数，根/Unity原Scene后继未验。

实施范围：只新增 `Tools/NTSD28Q07Diagnostics/fusion_natural_fractional_reachability_probe.cpp` 及本Task/Change/Ledger/STATE/Handoff和诊断原件；链接当前正式28 Core+3 playable源，读正式`resources/runtime`。受控正式角色初态（OID10/11/action9/HP100/位置与关系符合DAT记录）后，不手动清融合计时、不写精确坐标，以离散中性/方向/跳跃输入跑到计时自然到零，记录每tick当前OID、动作、精确/整数XYZ、融合计时/事件、RNG和是否解融合；阴性按样本范围保留。绝不改DAT、正式源码、Unity生产/Scene或非战斗。

出口：编译0错，原件双跑可重现；若有融合且自然解融合瞬间主角精确与整数至少一轴不同，才升级为C054自然小数候选并查根LFR是否能同初态，然后独立原Scene；否则记录可达性阴性，不把受控小数注入样本冒充自然。回滚仅审阅本新诊断脚本和文档；不清理或覆盖现有脏工作树。

2026-10-01 脚本范围续订：第二记录中性/持续向右240tick自然拆分在201且精确值均为整数，跳跃样本在171停止，双跑CSV/summary同SHA。为检查更贴近既有受控小数证据的第一记录，允许同一新诊断脚本增加只读`row0`选项：正式OID7/8→51、action9、HP100、decrease4500，跑到最多4550tick，仍不得手动置零计时或注入小数；保留row1各原件、另目录输出。若自然Battle提前停止则记录终止，不造阳性。其它路径与验收边界不变。

运行结果：记录2中性/持续向右tick201自然拆分、已完成tick均无精确/整数分歧；晚跳于171无完成tick。记录1 X320初试未融合，改按既有正式正例X304后tick1融合，三组到tick349，tick350无完成tick、计时仍4151，未拆分。两记录有效版本均双跑CSV/summary同SHA，v3/v5编译日志0字节，保留首次失败。详[有界报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C054-NATURAL-FRACTIONAL-REACH-001/REPORT.md)。未找到阳性，故不发根/Unity小数后继Play；C054/Q07/总目标开放。
