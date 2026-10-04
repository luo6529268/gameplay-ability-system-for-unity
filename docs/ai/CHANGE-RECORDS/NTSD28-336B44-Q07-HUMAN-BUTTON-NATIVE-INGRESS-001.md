<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-HUMAN-BUTTON-NATIVE-INGRESS-001
status: SUPERSEDED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeInputProducerMigrationEditorTests.cs
code-path: Assets/NTSD/Scripts/Input/NTSDInputStateModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Input/NTSD28InputTwoPassModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09P08SameStateBattlePlayProbeEditor.cs
authority: selected 336B44 root EXE and corresponding playable native input routing
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-HUMAN-BUTTON-NATIVE-INGRESS-001.md
-->

# 人类战斗三键正式输入入口

**2026-10-04 更正：`SUPERSEDED`。** 本 Record 的下游人类 native 三键再轮换已被 Q12 自然键原 Scene 证据证伪：物理控制器本来就把 J/K/L 编成旧 `jump/def/att`，正式 producer 再投影到正式 4/5/6；新增轮换使物理 L 变攻击、combo1 无法起步。[Q12 精确更正](NTSD28-336B44-Q12-DOUBLE-BUTTON-REMAP-CORRECTION-001.md)已撤去该生产 hunk 并以原 Scene 自然 J/K/L 复验。旧定向 RED→GREEN 和 22tick 合成 `SimulationInputButtons.Attack` 是**直接输入测试载体的结果**，其“玩家输入修好”结论撤销；所有原件和前后状态仍保留。本 Record 新增的独立回归请求路径仍留作历史诊断，不再排执行队列。

脚本前登记。原 Battle Scene 修正 Attack 的同初态 Play 已得 tick2 正式动作60/Unity动作110、之后正式血点阳性/Unity阴性，退出 clean 与四 SHA 稳；具体原件见 `artifacts/diagnostics/NTSD28-336B44-Q09-P08-ROOT-UNITY-SAME-STATE-001/ita-equal-hp-336b44-unity-02.json`。正式索引与当前 Unity 冻结布局静态追踪指向人类输入旧字段适配，但生产修复须由聚焦 RED 证明。只改 Task 声明的测试、人类本地输入与必要的状态回读；AI 旧布局、非 native、DAT/资源/Scene/非战斗不得改变。预期影响、风险、验收和精确回滚见 [Task](../TASKS/NTSD28-336B44-Q07-HUMAN-BUTTON-NATIVE-INGRESS-001.md)。执行后追加实际文件/符号、测试/构建/Play结果与未验项，状态不得超过证据。

2026-10-04 test-first 脚本已写：`NTSD28NativeInputProducerMigrationEditorTests.DataOrientedHumanSemanticButtons_ReachFormalThreeButtonIndices` 新增完整人类输入缓冲→producer→正式 proxy 的 Attack/Jump/Defend 三组 one-hot 断言；生产脚本尚未改。生成 Editor 工程 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly` exit0、0 error、332 warning；`Tools/Validate-ChangeLedger.ps1 -RepositoryRoot` exit0，测试脚本 diff 已被本 Record 覆盖。原 Editor 仍在 Play/现有程序集时间早于本测试脚本，定向 RED 尚未运行；不能宣称修复或新版输入通过。当前状态保持 `IN_PROGRESS`。
2026-10-04 原 Editor 定向 RED：新测试已入 `Assembly-CSharp-Editor.dll`，只运行 `NTSD.Test.NTSD28NativeInputProducerMigrationEditorTests.DataOrientedHumanSemanticButtons_ReachFormalThreeButtonIndices`，job `26f844e9164348649265f0ed7aeeb133`；1/1 按预期失败，`att` 正式 index4 期望1/实际0。原件：[EditMode job](../../../artifacts/diagnostics/NTSD28-336B44-Q07-HUMAN-BUTTON-NATIVE-INGRESS-001/editmode-red-20261004.json)。这只证明当前人类语义到正式 proxy 的测试首差；生产脚本尚未改、GREEN/原 Scene 修复后复验未发生。用户要求先重排新版总表并调整待办，故本 Record 保持 `IN_PROGRESS`，代码修复在文档队列完成后按新顺位恢复。
2026-10-04 队列整理完成后生产脚本已写、尚待 GREEN：`NTSDInputStateModule.UpdateFromBuffer` 的完整包基态按人类/native 旧字段布局逆读；`SyncToRuntime` 仅对已注册、非 AI 且 native pipeline 的人类输入，将语义 Attack/Jump/Defend 及 previous 一次投影到现有 `KeyJump/KeyDefend/KeyAttack` 载体。`SyncFromRuntime` 加可选人类 native 逆投影，`NTSD28InputTwoPassModule.ProcessNativeSampledState` 只在 native 人类路径启用；AI 原布局、非 native、`FreezeProducerState` 与正式索引不变。聚焦测试在同一三键用例增加按住和释放的 current/previous/本地语义断言。尚未运行改后编译/原 Editor GREEN/原 Battle Scene，状态只为 `CODE_WRITTEN`；如新首差出现，先以同初态 tick 定位，不加角色或 DAT 特判。
2026-10-04 生成 Editor 工程定向编译：`dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly` exit0、0 error、332 warning，[原始输出](../../../artifacts/diagnostics/NTSD28-336B44-Q07-HUMAN-BUTTON-NATIVE-INGRESS-001/generated-editor-build-after-fix-20261004.txt)。此证据仅为生成工程编译；原 Editor 新程序集、三键 GREEN、AI/非 native 相邻测试、原 Battle Scene 22tick 复验和四 SHA 待。状态 `COMPILE_PASS`，不把 RED 前的第一轮血点作为修复通过。
2026-10-04 原 Editor 新程序集重载后，精确筛选 5 项 EditMode 测试 job `dabf1b5adc334ff1bd737a1ee71cea72`，5/5 PASS、0 failed：三键正式索引及 current/previous/held/release，native freeze 双 bank、DataOriented 第二 pass、legacy human 原行为、同步 AI 旧布局。原始结果：[EditMode GREEN](../../../artifacts/diagnostics/NTSD28-336B44-Q07-HUMAN-BUTTON-NATIVE-INGRESS-001/editmode-green-20261004.json)。这只关闭聚焦测试；原 Battle Scene 的同初态 22 tick Attack 复验和真实物理键仍待，状态 `FOCUSED_TEST_PASS`。
2026-10-04 原 Scene 复验前的测试载体计划：先保留旧 `Temp/NTSD28_Q09_P08SameState.request.json`（runId `...-02`）原字节；只给既有 Q09/P-08 Editor 探针增加一个优先读取的 Q07 回归请求路径，使第三轮使用新文件和新结果 ID。此变更仅在 Editor 诊断文件中，不影响生产输入、DAT、Scene、旧请求和旧结果。运行前核对唯一 clean Battle Scene、非 Play、编译空闲与四保护文件 SHA；运行后核对 22 tick、清洁退出与同一四 SHA。若第三轮未取得正式同态，保留结果定位首差；本 Record 状态不因计划提高。
2026-10-04 测试载体实际只在 `NTSD28Q09P08SameStateBattlePlayProbeEditor.TryStart` 增加新请求路径优先读取，旧请求及原结果保留；生成 Editor 编译 exit0/0 error/332 warning、原 Editor 新 DLL 已导入，Ledger validator exit0。新请求 `ita-equal-hp-336b44-unity-03` 在原唯一 clean Battle Scene 完成22生产 Driver tick，[原件](../../../artifacts/diagnostics/NTSD28-336B44-Q09-P08-ROOT-UNITY-SAME-STATE-001/ita-equal-hp-336b44-unity-03.json)和[逐字段配对](../../../artifacts/diagnostics/NTSD28-336B44-Q09-P08-ROOT-UNITY-SAME-STATE-001/third-run-field-comparison.json)：六字段130/132同，唯tick2/3鸣人正式60、Unity65；tick8目标HP10、tick22一条1×3血点命令均恢复。此次原 Scene退出非Play、clean，四保护文件前后 SHA 完全一致（`protected-before-third-play.txt` / `protected-after-third-play.txt`）。
这两处剩余动作差不能判为生产规则差：根 LFR 明确从 manager 注入同步随机表，初态哈希 `f75f85682ee9412c`；Unity 探针仅 `ResetFromSeed(0)`，表哈希 `68c5f16327d0ddd7`。二者CRT状态/调用数同为`3374725112/3000`，不代表同步随机表同态；正式站立攻击在site `0x82`按该表二选一。原110动作首差已消除、共同攻击/伤害/血点链恢复，但未做同表逐tick验收或真实人手键；保持 `RUNTIME_PENDING`、不新增角色或DAT特判。下一按总表复用此证据，未出现新的非例外首差不重复跑22tick。
