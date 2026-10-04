# NTSD28-336B44-Q07-C053-TOBI-PHYSICAL-REACH-001

状态：`RUNTIME_PENDING`（正确OID0的自然出生源/根子门已证；`next:0`留action3已纠正，原Unity Scene及独立双Uj验收待）。父目标`NTSD28-UNITY-BATTLE-REALIGNMENT-001`，G1/Q07/C053。前期受控OID53四Y阴性及本Task前三轮OID53阴性均已被身份纠正覆盖；正式DAT的OID0跳跃帧含`hit_Fa:510`。本Task使用当前336B44 playable完整GameSession追普通输入到帧512及OID251出生。

唯一新代码路径`Tools/NTSD28Q07Diagnostics/c053_tobi_physical_reach_probe.cpp`。身份纠正后的两例：Tobi OID0/action0、Y0，先跳跃至212后以防+右+攻击三tick；以及OID0受控action212/Y-80从首tick发送同组合。每例最多40tick，远敌OID2、mode0/seed682973786、正式runtime保持。逐tick记录输入/抽样相位/Tobi action/Y、OID251首次槽/action、FrameEvent和生产Spawn事件。阳性时只为原案捕获LFR并交正式根EXE，比较公开逐tick字段；阴性按路径边界记录并停止，不继续搜索其它按键排列。

不修改Unity生产或测试、DAT、图片、Scene、Prefab、背景、项目模式、正式源码/EXE和非战斗。任何输出路径已存在时拒绝覆盖；不删文件。受控action212只作输入消费者对照，不等于自然普通跳跃。正式根阳性也不自动关闭C053/Q07，仍需Unity原Battle Scene同输入验证。回滚限新增工具/文档，删除须准确审计和用户授权。

2026-10-04 同两例补充只读字段门：前两轮`combo_state[0]=4`但没有`hit_Fa`动作尝试；同一个工具仅追加调用前action/当前帧`hit_Fa`/frame212`hit_Fa`三值，区分catalog字段与输入顺序，不新增输入案。新exe和输出目录均拒绝覆盖旧原件。

2026-10-04 权威身份纠正：正式`data/data.txt`将OID0指向`c/tobi/tobi.dat`，OID53指向`c/tobi/ttobi.dat`；只有OID0对应DAT有目标`hit_Fa:510`/frame512 OPoint。前述OID53两例阴性属于错误角色，不能裁决OID0自然输入。继续同两例但actor/config OID改为0，其他参数不变；原件保留、新v4输出，避免把错误前置写成游戏规则。

2026-10-04 当前出口：OID0普通action0跳跃案tick8经`hit_Fa`进入510、tick11自然生成OID251/action51；受控212案tick2进入510、tick5自然生成。两案40tick LFR正式根回放均PASS，选定实体/输入8字段640/640同态；两位置下子体未到action0而走3→60…63消失。原Unity Scene、自然action0同目标双Uj和全World仍待。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-TOBI-PHYSICAL-REACH-001/REPORT.md)。

2026-10-04 v5出口：同两例新增`step->spawns.events`逐tick只读字段。普通案tick11和受控案tick5各有一条OID251事件，`status=spawned(0)`、slot50、source_line1917，与子体首次可见同tick。v5当前闭包编译exit0，run-e两条LFR与已由正式根EXE回放的run-d逐文件SHA相同；未重放相同字节。Unity原Scene验收仍待。

2026-10-04 v6有限高度对照（脚本修改前）：正式`firz.dat`的action51→2→3→0静态链，在现有Y0/-80两例中被正式`native_ai.cpp` behavior7 的整数Y>-25转action60截断；不据此改DAT或Unity生产。在同一只读Tools探针保留原两案、追加一例受控OID0/action212/Y-130、相同seed/mode/远敌/三tick输入/40tick，观测OID251是否在自然出生后到action0。编译写全新v6可执行文件、run-f目录并拒绝覆盖；三案LFR中若第三案阳性，使用当前336B44正式根EXE回放并对照选定公开字段。此例只证明高度条件下的正式自然帧链，不等价普通物理按键或Unity原Battle Scene；不能用测试初态直接改生产规则。

2026-10-04 v6实际结果与前提纠错：原`51→2→3→0`假设错误；正式`FrameMachine28::step()`将`next:0`视为`stayed`，不是跳action0。新增Y-130案的OID251在tick5自然出生action51、tick13～21留action3、tick22由`hit_Fa:7`切60，40tick未到action0。新编译exit0，三案CSV/LFR产出，原两案LFR SHA稳定；高位案正式336B44根EXE回放PASS，六公开字段240/240同。受控action0双Uj只保留受控证据，后续原Unity Scene定向验收和其它真实可达前置另行论证；不再沿错误`next:0`搜索高度。Task仍`RUNTIME_PENDING`，因为Unity原Scene未验。
