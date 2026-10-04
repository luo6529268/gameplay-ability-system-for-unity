# Q07/C053 OID251 正式 DAT 自然生成候选

**2026-10-04 帧机语义更正，覆盖下文`51→2→3→0`静态路线推断：** 正式`firz.dat`帧3的`next:0`在当前336B44 playable `FrameMachine28::step()`中表示`stayed`，不跳action0。正式OID0 Tobi自然生成的OID251在Y0/-80及新增更高受控Y-130三案均到action3；Y-130案tick13～21停在3、tick22由`hit_Fa:7`行为转60，当前根EXE同LFR回放PASS且240/240选定字段同。原“没有中断则3→0”是对DAT token的错误解释，不能再作为自然双Uj的前置。详情见[当前物理输入报告](../NTSD28-336B44-Q07-C053-TOBI-PHYSICAL-REACH-001/REPORT.md)；原文保留为已纠正历史。

**2026-10-04 身份更正，覆盖下文旧断言：** 正式`data/data.txt`将OID0映射`c/tobi/tobi.dat`，OID53映射`c/tobi/ttobi.dat`，并非两者共用前者。含frame512 `opoint oid:251`和跳跃帧`hit_Fa:510`的是OID0。后继[OID0正式源/根有限输入证据](../NTSD28-336B44-Q07-C053-TOBI-PHYSICAL-REACH-001/REPORT.md)已经证明普通跳跃案tick11自然生成OID251/action51；该源/根子门不包括Unity原Scene或自然action0双命中。下文“自然可达仍待”及“OID0与53均指向同DAT”为当时错误/过时快照，不能再作为当前结论。

历史状态：`DAT_CALLSITE_IDENTIFIED / NATURAL_REACH_PENDING`。只读当前336B44正式 `resources/runtime/decoded_dat` 与Unity暂存 `Assets/NTSD/Content/LoganRuntime/decoded_dat`；当时没有执行技能输入、正式EXE自然三人战局或Unity Play，也没有修改DAT。

正式 `data/data.txt` 将OID0与53均指向 `c/tobi/tobi.dat`，OID251/type3指向 `a/fir/firz.dat`。在正式 `decoded_dat` 全部 `.dat` 词法扫描中，唯一一处 `opoint ... oid: 251` 位于 Tobi `frame 512 air_katon`，参数为 `kind:1/action:51/dvy:5/facing:0`。同一角色的frame510→511→512声明了到该帧的顺序；jump/dash帧212/213/214/216/217声明 `hit_Fa:510`，仅证明DAT存在选招候选，不证明某段物理按键在正式运行时一定消费该字段。

`firz.dat` 的action51声明 `wait:2/next:2`，action2声明 `wait:5/next:3`，action3声明 `wait:1/next:0`；action0含 `effect:2/injury:35` 的kind0 ITR。故在没有命中、中断或生命周期提前终止等改变帧流的前提下，DAT存在 `51→2→3→0` 路线。此为静态帧链，不给出实际tick数、位置或自然同tick双命中结论。现有[源/根阳性](../NTSD28-336B44-Q07-C053-AUX-TYPE3-DOUBLE-REACH-001/REPORT.md)以受控slot2/OID251/action0起步；不能直接升级为Tobi自然生成。现有[Unity原Scene探针](../NTSD28-336B44-Q07-C053-AUX-TYPE3-SCENE-001/REPORT.md)亦只测试受控辅助对象，尚未运行。

三份正式明文与Unity暂存逐文件SHA一致：`c/tobi/tobi.dat`=`7AC8AB91E0ADE7A19E7BE1950568D080440A673C9C0107189D3465038479D61F`，`a/fir/firz.dat`=`7496A9AD256D83B6679BA4C93F877D5FC0C490E41AE8F509C792507E1FDDEACC`，`data/data.txt`=`3ED7DE4918AA7B5E94FE73A2B7D9B43DED9D10575DD182B9CA2B647EAE29A8F0`。这只证明已暂存的DAT内容一致，不证明生产加载或运行行为。

后续若需升级自然可达性，先用正式playable完整输入链确认Tobi从普通状态的按键、frame512 OPoint出生tick与其后action51/2/3/0生存轨迹，再在同输入和同位置条件下配对OID875/808命中；不能为了取得双命中改DAT或造角色专用规则。当前优先等待已编译探针的原Editor Play首差，避免在同一Q07条件上重复无界搜索。
