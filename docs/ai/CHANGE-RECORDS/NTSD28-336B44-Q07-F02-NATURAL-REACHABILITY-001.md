<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-F02-NATURAL-REACHABILITY-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/fast_weapon_natural_reachability_probe.cpp
authority: selected336B44 playable GameSession BattleWorld PhysicsIntegrator and current indexed weapon DAT
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-F02-NATURAL-REACHABILITY-001.md
-->

# NTSD28-336B44-Q07-F02-NATURAL-REACHABILITY-001

Created before diagnostic code. The Task records exact source/live authority, pre-edit candidate audit, owned path, bounded natural-case inputs, acceptance, non-interference and rollback. Existing production and data are outside this change. The only planned code adds an isolated source diagnostic which retains the original v5 logic but observes 128 instead of 32 complete ticks. New output must use a fresh directory and preserve the prior negative 32-tick trace. Positive F02 reachability and any Unity mismatch are unknown until execution; do not promote a rebuild to the formal root EXE.


**2026-09-30 有界阴性终态（覆盖前述候选未执行）：** 新诊断仅把既有source probe完整GameSession tick窗口32延长至128，C++同正式82文件live路径编译exit0/无诊断；在Temari19/247地面普通初态、中性输入、seed682973786、mode0/BG1、目标OID2/X1100下，OID124 tick4出生，128tick内活跃125行。state1000共有37行，但与|Vx|>9交集为0；tick35仍state1002/Vx14，tick36首次state1000/action9/Vx-4，源frame事件显示命中反应后的hold。正式根EXE SHA336B44与正式/staged `w/9.dat`逐SHA一致。只有本源载体/所选条件的阴性可达结论，未运行正式root replay或原Battle Scene F02阳性、不称全局不可达。F02父项仍RUNTIME_PENDING，后续必须找到别的真实state1000高速前置（例如正式可达非当前case的受击/释放链）才能要求根/场景阳性；无该前置时不重复合成Vx夹具充作正式证书。证据identity-and-reachability.json、sourceCSV/LFR、compile-argv/output、ledger-check-rerun.txt；早期ledger-check.txt失败是STATE漏写完整ID，已修，最终PASS1064/2覆盖。无生产/DAT/Scene/Asset/非战斗改动。
