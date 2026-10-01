# C054 正式融合记录自然减时与小数可达性（2026-10-01）

权威根正式 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`；本诊断链接其对应 playable 28 Core+3 playable C++，读取正式 `resources/runtime/decoded_dat/data/fusion.dat`。没有改正式源码、DAT、Unity脚本、Scene或非战斗资源。两个记录都从正式 OID、action9/state2、HP100、同组且相距合格的受控角色初态开始；之后只投离散输入，**不手动置零融合计时、不注入精确小数**。这不是玩家从菜单自然选角/物理按键全程证书。

| 正式融合记录与输入 | 源完整 GameSession 结果 | 小数条件 |
|---|---|---|
| 记录2：OID10 Deidara + OID11 Sasuke → OID52；`decrease=200/cover=1`，中性 | tick1融合，tick201自然拆分，至tick240完整 | tick1～240拆分前精确XYZ始终等于整数XYZ；无C054小数触发 |
| 同记录，tick2～205持续向右 | tick1融合，tick201自然拆分，至tick240完整 | 精确/整数全程相等 |
| 同记录，tick170～205持续跳跃 | tick1融合，完成至tick170；tick171起`GameSession::last_tick`缺失，记`terminal=171` | **未走到拆分**，不能算阴性拆分样本 |
| 记录1：OID7 Lee + OID8 Chiyo → OID51；`decrease=4500/cover=0`，X304/300，中性、晚跳、持续向右 | 三组均tick1融合，完成至tick349，tick350无完成tick；融合计时剩4151，未拆分 | 三组全部已完成tick精确/整数全程相等，但均未触发C054拆分 |

记录1的X320/300先前一轮三组未融合，保留在`run-row0-a/`；X304/300随后按既有正式融合正例调整，成功融合，不能把X320阴性写成融合规则缺失。正式OID52 DAT未声明 action310，但`DatDocument::frame(310)`处于0～998原生零帧域，不应由DAT文本缺项推断融合不能发生；记录2实测tick1融合、tick201拆分。记录1在标准mode0的tick350无完成tick与既有350结果时点相符，但本探针未导出battle-flow终止原因，故只报告**观测到的会话出口**，不把原因推断写成实测字段。

编译：v3（记录2）和v5（记录1 X304）均链接当前正式闭包并以 `-Wall -Wextra -Wpedantic` 返回0、日志0字节。v2记录2首次在跳跃样本因无完成tick返回3，原件`run-a/`保留；v3把此情况记录为terminal，未伪造成完整tick。记录2 `run-b/run-c` 的CSV同SHA `BDE24CCA1125DD79DDDEC4E78C74F8D569FB78E5ED6C83BD4160D1330892328E`，summary同SHA `D6EB8AF009B8D4B10DC17FC8BF0497306AD7162434A3562325853CE5E1B18F93`。记录1 X304 `run-row0-b/run-row0-c` 的CSV同SHA `868DDA874655CD1795C1FA28F9FB3A8BAE2894808A7DBADC97C8BC3FE0388B4B`，summary同SHA `F1FAD0106673682877829A450832247861FAD69255F4D29E193EDCA5C09037B5`。当前诊断脚本SHA `9F1006487E538B603BB14C60A200604BE6C21B6A37F126E9BA1AA09ED071032D`。

结论只排除以上受控初态/输入窗口的**自然小数拆分阳性**：记录2确实自然拆分但无小数，记录1确实融合但会话在计时到零前停止，跳跃样本更早停止。既有人工注入小数的源码分支与Unity聚焦1/1仍有效，C054不关闭，也不为了这个阴性去改DAT或生产代码。G1下一应找能在**正式可达解融合前**产生精确/整数分歧且同一会话可持续的输入/模式；若短期找不到，按总表优先转C040/C043/C056等其它可达首差。根EXE/原Battle Scene小数后继只有阳性时才值得执行。
