# F02 高速武器自然前置可达性

状态：`VERIFIED_SCOPED_NEGATIVE / F02_PARENT_RUNTIME_PENDING`。当前规则是 type4/6、物理入口原帧state1000、减速后|Vx|>9选择动作40，落地可在同轮覆盖。

初轮只读排除：旧kind3高初速的OID123实际动作0..5均state3005；101/124直接高初速OPoint进入state1002；当前索引内容中直接state1000的101/124/422/600 OPoint初速不超过5。原已有OID600初始Vx±20等源码/Unity完整tick通过，但LFR不带初始Vx，不能当正式EXE自然证书。

已用原封v5源码诊断在正式内容上从普通Temari19/地面动作247、中性输入生成OID124：tick4子体slot50/action41/state1002/Vx30，tick5速度14，至tick32仍state1002/速度14，没有F02入口。源CSV/LFR在`temari247-source-v5`，此为阴性边界；尚未用正式根或Unity场景断言F02。新Task/Record已在扩展CPP之前创建，正在把同一自然局延长至128tick以检验落地/状态转换；下一步依结果裁决是否值得正式根回放。


**2026-09-30 有界阴性终态（覆盖前述候选未执行）：** 新诊断仅把既有source probe完整GameSession tick窗口32延长至128，C++同正式82文件live路径编译exit0/无诊断；在Temari19/247地面普通初态、中性输入、seed682973786、mode0/BG1、目标OID2/X1100下，OID124 tick4出生，128tick内活跃125行。state1000共有37行，但与|Vx|>9交集为0；tick35仍state1002/Vx14，tick36首次state1000/action9/Vx-4，源frame事件显示命中反应后的hold。正式根EXE SHA336B44与正式/staged `w/9.dat`逐SHA一致。只有本源载体/所选条件的阴性可达结论，未运行正式root replay或原Battle Scene F02阳性、不称全局不可达。F02父项仍RUNTIME_PENDING，后续必须找到别的真实state1000高速前置（例如正式可达非当前case的受击/释放链）才能要求根/场景阳性；无该前置时不重复合成Vx夹具充作正式证书。证据identity-and-reachability.json、sourceCSV/LFR、compile-argv/output、ledger-check-rerun.txt；早期ledger-check.txt失败是STATE漏写完整ID，已修，最终PASS1064/2覆盖。无生产/DAT/Scene/Asset/非战斗改动。
