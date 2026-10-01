# NTSD28-336B44-Q07-C042-NATURAL-SCENE-001

状态：`VERIFIED_SCOPED_NATURAL_PATH / NONZERO_TRIGGER_PENDING`。父目标 NTSD28-UNITY-BATTLE-REALIGNMENT-001，BATCH-04/Q07/C042。当前正式336B44 OID75 `bee.dat` action355/X500 对鸣人X550的自然抓取→tick55投掷，已由完整源码和根同LFR各80tick限定证实；被投者投掷前动作计数为0，所以此场景只验自然投掷链，不能证明C042“非零计数保留”的特定分支。

只新增 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C042NaturalThrowBattlePlayProbeEditor.cs` 和Unity生成的 `.meta`，复用原Battle Scene自然探针的Play克隆roster、暂停后生产`SimulationTickDriver.StepOneTick`、退出Scene哈希检查；精确请求仅 OID75/action355 X500→OID2/action0 X550、Z400、HP/MP500、mode0、seed682973786、中性输入，运行60 tick，记录两体动作/计数、位置/速度、HP、抓取槽/时限与RNG。以[正式源/根报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C042-NATURAL-THROW-001/REPORT.md)的同tick CSV作独立首差比较；若自然Unity出现差异，先登记，再另立最小生产修复包。

不编辑DAT数值、Scene、资源、Unity/GAS框架或非战斗功能；不冒充物理按键或C042非零计数正例。验收为原Editor编译0错、原Battle Scene Play结果、源/Unity逐tick字段报告、正常退出/Scene clean及四保护SHA，最后运行ChangeLedger validator与diff check。回滚仅审阅本新增探针，不清理已有脏工作。

结果：原Editor Tundra编译成功/0 CS error；原Battle Scene自然Play60tick CAPTURED/DONE、退出clean。与正式源码15实体+5 RNG字段1200/1200、首差0；tick54被投者计数0、tick55投掷动作181及尾计数1均相同。四保护SHA稳、Editor idle/非Play/非编译。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C042-NATURAL-SCENE-001/REPORT.md)。此包仅自然路径限定验收，C042非零计数分支与Q07/总目标仍开放。
