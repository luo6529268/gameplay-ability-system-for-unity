<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-D024-PLATFORM-NATURAL-ENTRY-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/d024_platform_natural_entry_probe.cpp
authority: 336B44 playable GameSession, formal OID36 and OID56 DAT, user D-024 proportion requirement
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-D024-PLATFORM-NATURAL-ENTRY-001.md
-->

# NTSD28-336B44-Q07-D024-PLATFORM-NATURAL-ENTRY-001

脚本前原状：正式 OID56/frame182 的受控初态连续平台搬运已在当前源码、根 LFR 与原 Battle Scene 做 99/99 选定字段同态；普通初态玩家输入是否能产生这一帧及同轮平台链接仍未知。正式 `battle_world.cpp` 的 kind10/11/17 对角色可写默认 action182；`physics_integrator.cpp` 的 state12 空中分支在负环境状态与 phase 条件下也可选择182。正式多由也 `tay.dat` frame243 带 kind10；frame239→241→242→243 的静态链需要完整 tick 验证，站立帧 hit_Ua:240 不能直接当成进入239的证明。

唯一新增路径、前置、边界、验收及回滚见 Task。预期副作用仅独立诊断二进制与 CSV；不改正式源/EXE、资源或 Unity 生产。若没有自然阳性，不把有限阴性扩大成不可达结论，也不关闭 Q07/D-024。

2026-10-03 `CODE_WRITTEN / NATURAL_FRAME182_POSITIVE`：脚本首版以正式源38个playable单元编译exit0；6案×96完整tick。普通多由也 action0、防御2tick→上2tick→攻击2tick在tick6到239、tick22到243；tick23 kind10 applied1并使飞段从action0自然进入182，之后飞段规则X200→197→194与Y−2→−8→−15。首版地面 OID2 从Y−5在第22 tick前回Y0，平台链接0；受控239/243也使飞段182但不能作为普通输入证据。原始 `source-run-01` 两CSV留存。本段之后、再次修改脚本前，Task已限定新增 OID2 自然跳跃起点9～23的有限矩阵；只更新同一诊断脚本并写新 run-02，不改旧结果或正式/Unity生产。

2026-10-03 `CODE_WRITTEN / NATURAL_SINGLE_LINK_ONLY`：第二版同正式闭包编译exit0；固定OID2初始Y0，普通跳跃起点9～23共8例，9～17起点各有飞段182期间单tick建链，目标X205始终不动；19～23无链接。典型起跳9在tick24源X197/Y−8、目标X205/Y−85，虽链接1但未贴合碰撞参考；tick25源左移至194后X205超出平台严格范围。run-02摘要与逐tick原件保留；本段之后再次修改脚本前Task限定起始X175～195小矩阵，探查下落/源左移交会。保持正式DAT/Unity生产不动。

2026-10-03 `CODE_WRITTEN / NATURAL_LINK_WITHOUT_CARRY`：第三版编译exit0；固定普通输入、起跳9，起始X175/180/185/190/195分别得到0/2/3/3/3 tick平台链接，五例目标X均保持原值。X185 tick29～31目标Y−73/−65/−58，对应平台碰撞参考−50/−58/−64，帧运动时未达到 `position.y == collision_y_reference`；tick32源X173平台X范围不再包含目标185。run-03两CSV保留。再次修改脚本前Task已限定只补两条LFR（普通输入无跳、自然跳X185），交根正式EXE核可回放字段，不改旧run或生产。

2026-10-03 `VERIFIED / SCOPED_NATURAL_ENTRY_AND_BOUNDED_NO_CARRY`：第四版编译exit0；run-04两CSV与run-03逐SHA同，增加两条LFR。正式根EXE SHA核对；两条逐参正确启动各exit0/PASS，源码/根每案96tick×10字段960/960首差无，根内部平台操作报告在X185案直接列tick29～31三行来源槽1/参考−50/−58/−64，目标X始终185。根报告 `nativeParityClaim=false` 且多一个终端tick，不扩大结论。首次`Start-Process -ArgumentList` 因带空格路径不保引号致exit10，原空报告目录保留；后以逐参ProcessStartInfo修正。实际代码路径仅唯一 Tools 脚本；原Unity Scene自然三人链未运行，正式真实GPU、其它输入和非零自然搬运均待。回滚边界及全部原件见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-PLATFORM-NATURAL-ENTRY-001/REPORT.md)。
