# NTSD28-336B44-Q07-C045-NATURAL-SCENE-001

状态：`VERIFIED_SCOPED_NATURAL`。父目标 NTSD28-UNITY-BATTLE-REALIGNMENT-001，BATCH-04/Q07/C045。正式336B44 OID65 action348/X550、action357/X700 自然抓取→376伤害已由当前源码和根同LFR逐tick证实；action348/X1200为阴性控制。Unity受控 `recover`/`cover` 修复已单独完成，不能替代自然同初态场景证书。

只新增 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C045NaturalGrabBattlePlayProbeEditor.cs` 及Unity生成的 `.meta`，复用现有原Battle Scene自然探针的只在Play克隆设置Roster、暂停后通过生产 `SimulationTickDriver.StepOneTick` 测量、退出时检查Scene哈希的模式。请求仅允许上述三组精确动作/位置，角色OID65/2、source X500/目标X、Z400、HP/MP500、mode0、seed682973786、中性输入；记录至少双方动作、X/Y、HP、停顿、抓取关系、RNG，按正式CSV字段同tick首差。先一组近距，再远距和第二入招。不得修改 DAT、Scene、现有配置、非战斗或用户比例策略，不覆盖旧结果/旧脏工作。

验收：原Editor编译0错；原 Battle Scene 进入/退出Play并clean、四保护SHA稳定；结果保留原始JSON和独立比较报告，明确源/根/Unity相等字段及首差，不把源码成功等同Unity成功。若首差只记录原因并在独立后续包修复。回滚只审阅本新增脚本和对应文档，不执行文件恢复。

结果：原Editor Tundra编译成功/0 CS error；三组35 tick Play均 CAPTURED/DONE、exit/clean。与正式源码每组22字段×35tick=770/770、三组合计2310/2310，首差0；其中两组自然抓取伤害与停顿2/-3、一组远距阴性。正式根三组120tick各自同LFR声明字段零差见前包，不能把本轮35tick扩成120tick Unity。最终原Editor idle/非Play/非编译，四保护SHA稳定；[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C045-NATURAL-SCENE-001/REPORT.md)。C045此限定自然出口完成，Q07/总目标开放。
