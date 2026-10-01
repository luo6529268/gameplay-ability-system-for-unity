# NTSD28-336B44-Q07-C054-DEFUSION-PRECISE-POSITION-001

状态：`FOCUSED_TEST_PASS / RUNTIME_PENDING`。父目标 NTSD28-UNITY-BATTLE-REALIGNMENT-001，G1/BATCH-04/Q07/C054。

权威：当前正式 336B44 playable `BattleWorld28::advance_native_fusions`（`source/ntsd28_core/src/simulation/battle_world.cpp:2915-2940`）恢复伙伴时先复制主角色整数 XYZ，再从整数重建伙伴精确 XYZ；主角色自身精确坐标不受这一拆分写入影响。正式 `resources/runtime/decoded_dat/data/fusion.dat` 两记录为 7/8→51、10/11→52。旧 Q06 Unity 合体测试使用整数等精确坐标，不能验此新版修复。

Unity 原状：`BattleOid5152RuntimeModule.TrySplit` 直接复制主角色 `Runtime.X/Y/Z` 精确值与 `XInt/YInt/ZInt`，并复制 source-rule 精确 X/Z 与整数 X/Z。主角色有小数时伙伴精确值与当前正式源码不同；源规则与用户批准的按比例显示域必须分别维持。

准确代码范围：`Assets/NTSD/Scripts/Simulation/Passes/Oid5152/BattleOid5152RuntimeModule.cs` 的 `TrySplit` 位置写入；`Assets/NTSD/Scripts/Test/Editor/NTSD28Q06FusionRecordTransactionEditorTests.cs` 增加当前正式 DAT 行0受控解融合小数正例，断言伙伴三轴精确值按各自整数重建、源规则精确 X/Z 按其整数重建、主角位置不变。不得改 DAT、Scene、Prefab、相机、融合准入/计时/动作、GAS 或非战斗逻辑。

预期副作用：伙伴拆分瞬间及后继运动/碰撞的位置可能变化；持有关系、快照和双域比例可能受影响。先写聚焦测试，再只改共用写入点；生成工程编译后，原 Editor 聚焦测试、正式源码受控同态、原 Battle Scene 代表及退出检查仍分别验收。Editor 未导入时只能标 `CODE_WRITTEN / UNITY_RUNTIME_PENDING`，不可关闭 C054/Q07。回滚仅逐行审阅本 ID 的两处脚本变更，不回退工作树其他修改。

只读首差与门槛见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C054-DEFUSION-POSITION-AUDIT-20261001/REPORT.md)。

2026-10-01 测试和共用拆分写入已写；测试使用的正式 `fusion.dat` 当前 SHA 为 `E0D7BF92F222C63C04D6728ECD423369F0604D77FF12DF958CE4AE76FEBA5FEE`。生成 `Assembly-CSharp-Editor.csproj` 编译 0 错误、263 警告；原 Editor 聚焦、自然 Scene、正式根精确位置证据仍待，不能关闭 C054。

2026-10-01 追加：原 Editor 已编译，C054 单项 EditMode 1/1 PASS；正式 336B44 源码重新编译的既有融合诊断 4 组运行通过，但初态没有小数，不能代替正式小数同态。原 Battle Scene 后继 tick 和退出验收仍待。[聚焦结果](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C054-DEFUSION-POSITION-AUDIT-20261001/UNITY-FOCUSED-RESULT.md)。

2026-10-01 后继诊断范围：在仓库 `Temp/NTSD28C054FormalFractional336B44-20261001/` 内新建一次性 `fusion_fractional_witness.cpp`，只复用既有融合诊断的正式 `fusion.dat`/受控对象定义和当前 336B44 playable Core。融合成功后、解融合前，显式设置主角精确 XYZ 与整数 XYZ 不同，保留正式表、槽、计时介入、后继完整 Driver tick；对两次输出逐字节比较。该样本是**源码受控分支证据**，不是正式根 EXE 自然可达或 Unity Battle Scene 证据。不得修改正式源码、DAT、Scene、生产 Unity 脚本或历史诊断源文件。当前正式根 LFR 初态只导出整数位置，未发现直接设置精确小数的入口，故不能把这个受控样本称为根同态。修改诊断脚本前在同 ID Record 声明临时路径；失败保留日志，不以改 DAT 追求通过。验收为确实进入 `defused=1`，伙伴解融合瞬间精确 XYZ 等于整数 XYZ，主角小数保持，后继 tick 可观察；随后决定是否需要原 Battle Scene 的代表验证。

2026-10-01 实际结果：当前正式 31 个 C++ 源文件编译 exit0，受控小数四行双跑字节相同，独立 40 断言 PASS。两正例主角拆分前精确 `(320.75,-0.25,250.5)`，伙伴拆分后精确/整数 `(320,0,250)`，主角小数不变；后继完整 Driver tick 已记录。阴性 HP 门和缺伙伴定义门未误晋升。证据归档于[源码受控结果](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C054-DEFUSION-POSITION-AUDIT-20261001/SOURCE-FRACTIONAL-RESULT.md)。这补上正式源码小数分支，**没有**补上根 EXE 自然链或原 Battle Scene 后继 tick；C054 保持 `FOCUSED_TEST_PASS / RUNTIME_PENDING`，Q07 开放。
