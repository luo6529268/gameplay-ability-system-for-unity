<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C050-VERTICAL-SKIP-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5EffectActionOverrideEditorTests.cs
authority: selected 336B44 BattleWorld28 special_link_rest unarmored vertical skip and C050 paired root/Unity first difference
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C050-VERTICAL-SKIP-001.md
-->

# C050 共用普通垂直反应早跳

脚本前：正式源/根受控近距tick2目标action259/HP465/Vy0，原Unity同tick action186/HP465/Vy0；远距战斗字段3tick无差。Unity已有共用 `IsSpecialLinkRestGate` 在 prelude 给rest，但 `ApplyStandardCharacterDamage` 的垂直响应和强制倒地尾没有读取此门。前置/字段/副作用见Task。

声明代码路径两项：共用BattleDamageWriter生产路径及既有EffectActionOverride聚焦Editor测试。预期仅特殊门下跳过垂直冲量和倒地动作写入，保持其余命中链；无DAT、Scene、资源、非战斗或架构调整。先写聚焦正反例并在原Editor执行RED，再做最小生产修正、GREEN及近远原Unity完整tick对照。回滚精确本包修改，不覆盖既有脏工作。

实际脚本变更：在 `BattleDamageWriter.ApplyStandardCharacterDamage` 读取既有 `BattleNativeOrdinaryHitPrelude.IsSpecialLinkRestGate(victim, itr)`，将此门传给普通垂直反应，并在倒地尾仅抑制普通 fall frame 写入；HP、fall 计时、水平反应、held-pair vrest 和后续命中链保留。`NTSD28B5EffectActionOverrideEditorTests.UnarmoredVerticalReaction_SpecialLatchSkipsFallingAction` 新增 kind50/52 与普通 kind0 对照。相邻同文件 C053 改动属于已有其它工作，本 Record 不认领、也不回退。

修前原项目 Editor 定向 RED：3 个参数用例中 kind50/52 因 frame186 而失败，普通 kind0 通过。修后生成工程命令 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly` exit 0，0 error、266 warnings；Unity Editor GREEN 与近远完整 Driver 重测待执行。曾用一次未锁定原项目的独立 MCP 会话触发其它项目测试 job，此 job 无本项目聚焦结果、不作为证据；后续已改为同一 MCP 会话先锁定原项目并核对 URI 再运行。

修后原项目 Editor 聚焦 job `0d977e3a50cf42ee88e6c45626a673b5` succeeded，三个参数例 kind50、52、普通kind0 全部通过，0失败。原项目同一生产Driver两组新版 raw 各3完成tick、结果PASS，正式根逐tick两实体动作/HP/Vy各18/18字段相同；近距目标tick2、3动作259/HP465/Vy0，修前动作186。两份raw头正式身份均为336B44。测试为Editor隔离聚焦与原Editor完整Driver，并非原Battle Scene Play或物理键自然入口；C050父门、Q07、总目标继续开放。DAT/Scene/资源/非战斗未改。

治理复核：`Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location).Path` exit0，两个本包脚本均有本ID精确`code-path`覆盖；历史其它Record存在非当前diff的警告，不影响本包覆盖。`git diff --check`本包文件exit0。Battle/Menu Scene及GameConfig/ProjectBattleModeConfig四SHA与开始时一致。
