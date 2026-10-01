# C051 OID20→888 effect23 正式完整会话可达性

状态：`VERIFIED_SCOPED_SOURCE_ROOT / UNITY_PENDING`。仅证明本报告初态、80完整tick及选定字段，不关闭C051/Q07。

权威：根`NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`；`source/README_SOURCE.md`声明对应的当前playable闭包及正式`resources/runtime/decoded_dat`。OID20 `c/oro/oro.dat` frame288→289的OPoint OID888/action35；OID888 `c/oro/a/atk.dat` 35→36→37→40，40～43有kind0/effect23/dvx-10。诊断新增`Tools/NTSD28Q07Diagnostics/oro_effect23_reachability_probe.cpp`，只读正式内容、不改DAT或Unity生产。

生成`g++`链接正式28 Core与playable源（具体独立参数`compile-argv-v2.txt`）exit0，产物为诊断候选，不晋升正式EXE。首次v1也编译exit0；空stderr使`compile-v1.log`未生成，实际产物存在。每个诊断输出独立目录，已有输出拒绝覆盖。seed682973786、mode0、OID20/action288/X500/Z400对OID2/action0/Z400、HP/MP500，中性输入80tick。源面右X550双跑CSV SHA同为`79F92A51E8D8C948EAE56ACEBC305D0E965264DA6BB78DA6FE8EEC9F95A902A8`，LFR SHA同为`DF7B5511A00EFBF265C58EE9AE410AB2956B4B0443FDB801FF9168A645498865`。

面右X550：OID888在tick4出生，tick8进入action40、X549，tick9对X550目标应用effect23/dvx-10，水平冲量-10，目标HP500→435。面左X350：子体tick8 action40/X351，tick9同effect23，冲量+10，目标HP500→435。两例当前根EXE LFR报告均`passed:true`/failureCode0/declaredTicks80；对根trace tick1～80逐tick比较父动作、子体数量/动作/X、目标动作/X/HP/Vx，各8字段×80=640/640，无差。根headless另产生包装tick，未纳入此同态比较；报告`nativeParityClaim:false`，所以只能声明所列字段。左向根首次只覆盖slot0朝向，slot1初态朝向不符导致72字段差；第二次同时覆盖slot0/slot1朝向1后640/640同态，首次原件保留。

所谓远距X1200仍于tick9命中；X2000初态被正式stage钳到X1330，子体tick8跟至X1329，也于tick9命中。它们不是无命中控制，不能据此声称不受距离约束或实现错误。尚未做同初态Unity完整Driver、护甲消费、物理玩家按键或原Battle Scene Play。下一仅对面右/面左已证阳性追加严格336B44 Unity raw诊断；若实际首差才建立最小生产修复包。
