# NTSD28-336B44-Q07-C053-DOUBLE-UJ-SCENE-001

状态：`SCOPED_SCENE_PASS / RUNTIME_PENDING`。父项 C053 / NTSD28-UNITY-BATTLE-REALIGNMENT-001 / G1 / BATCH-04 / Q07。原源码完整 GameSession 已在OID702→808与两名正式OID875同tick的受控四人条件下，tick7记录两次 `applied/effect2/Uj156`、双跑原件同SHA；需要原项目保存 Battle Scene 的生产 Driver 同条件验证。根EXE现有headless回放只载两人，四人根同态不宣称已证。

准确范围：只新增 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C053DoubleUjBattlePlayProbeEditor.cs` 及其 Unity `.meta`，并为本次离线编译补充 `Temp/NTSD28Q07C053DoubleUj.compile.targets`；其余仅本 Task/Change、总表/诊断。生成工程旧于新脚本，临时 targets 只将该脚本加入原生成的 Editor C# 工程，不改生成工程或创建新 Unity 项目。使用现有原 Editor 的本机 Unity-MCP 刷新与请求文件机制，不启动第二Editor或computer-use。仅在原 `NTSD_Battle.unity` clean/单Scene时进入Play，将BattleTestBootstrap roster设正式OID702/2；稳定暂停后用生产 `SimulationWorld.LogicEntityFactory.Create` 从正式暂存内容建立两名 OID875/action55/team2 临时type3实体，置源X620/Z401和可用slot，重置同源seed/mode0，再用 `SimulationTickDriver.StepOneTick` 中性输入测至少8tick。记录正式OID808出生、tick7两攻击者与目标动作/锁存、各攻击者victim-rest及RNG；要求源位置/帧身份与正式源码案例一致，否则记首差。探针不得伪造第二次命中或改 DAT。

风险：原Scene原位Play、预热可能耗时；池容量封存可能拒临时实体，且从引擎配置建立OID875所需资源/slot可能与源码受控配置不同。必须先检查前置与明确错误；失败保留JSON，不为绿色修改生产或Scene。`finally`/既有有序关闭需退出Play、归还借用并核 Battle/Menu Scene、GameConfig、ProjectBattleModeConfig 四哈希；任务只证明声明字段与条件，不声称物理按键自然选招或正式根四人同态。

验收：新增Editor脚本生成工程编译0错、原Editor导入0 C# error、单例受控Play完成、逐tick JSON有原件，必要时与v5源码相同字段逐项比对，退出非Play且Scene clean/四保护SHA不变，ChangeLedger validator通过。若无法用原生产工厂建立同槽攻击者，保留阻断，C053继续RUNTIME_PENDING。回滚只审阅此新探针及其文档，不触动现有脏工作树；修改脚本前本Task/Change/Ledger/STATE/Handoff已登记。

当前证据：Temp targets 已将新探针纳入原生成 Editor C# 工程，离线 MSBuild 0 error / 277 warning；这是原Editor导入前的语法与引用检查。当前原Editor仍忙，连接显示另一个项目正在Play，本包未发送Play请求、未切Scene。后续恢复须重新确认**原项目**Editor idle且保存的Battle Scene clean、导入新脚本后0编译错，再发唯一RunId请求；结果须与正式v5源例逐tick检查，不能只看探针`SCOPED_PASS`标签。

运行续证：原Editor实际导入并编译新探针；原Battle Scene受控8tick源/Unity 13字段×8＝104/104一致，tick7双rest0→10，原件与独立比较见 `artifacts/diagnostics/NTSD28-336B44-Q07-C053-DOUBLE-UJ-SCENE-001/REPORT.md`。Play退出、Scene clean、四保护SHA保持。池借用数未由本探针导出，仍不满足本Task全部关闭标准；正式根四人回放和自然物理键均另门。上段“当前Editor忙/Play未发”是写入时的历史快照，以本段覆盖。
