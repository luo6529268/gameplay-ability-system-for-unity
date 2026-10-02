<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C044-SELFCHECK-ORACLE-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: formal 336B44 playable BattleWorld28::advance_catch_relations negative-decrease branch and C044 verified Unity writer
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C044-SELFCHECK-ORACLE-001.md
-->

# Q07/C044 SelfCheck 跨零冲量断言版本修正

脚本修改前建立。原 Unity Editor 完整 SelfCheck 的第二轮首差是 `CheckCpointDecreaseEscape` 的旧“双方动作计数置1且命中计数不写”预期；结果已另存。正式336B44源码在负 `decrease` 跨零时写双方 pending hit contribution count=1和受害者X±4/Y-3，动作计数、即时运动不变，后续帧处理才结算；Unity生产 C044 写者此前已按该顺序修复并在原Scene受控及OID17自然样本限定验收。

计划仅更正 `BattleRuntimeSelfCheck.cs::CheckCpointDecreaseEscape` 的三处旧预期，不改生产、DAT、Scene和非战斗逻辑。正常初始速度/动作计数0，pass后双方HitCount1、目标Knockback -4/-3、目标速度仍0；帧后处理后目标速度-4/-3、HitCount0。原测试动作/等待/关系断言保持。可能影响完整SelfCheck下一处可到达的旧预期，遇到首差先记录再独立归因；不能批量使测试变绿。旧结果已保存，不删除原件。

验收：生成C#工程编译0 error；原Editor完整SelfCheck能通过此方法或明确给出下一首差；账本与差异检查通过；四保护SHA不变。回滚只对本测试精确断言，经审阅处理。

2026-10-02 代码已写：只在 `BattleRuntimeSelfCheck.cs::CheckCpointDecreaseEscape` 改三处预期。跨零pass后确认双方动作计数仍0、HitCount均1、目标速度仍0；帧后处理后确认目标速度-4/-3且双方HitCount均0。既有动作/等待/冲量/关系断言保留；无生产、DAT、Scene或非战斗改动。修后编译和Editor运行尚待，状态`CODE_WRITTEN`。

2026-10-02 修后生成工程0 error/281 warning；原Editor第三轮完整SelfCheck越过上述方法，下一首差为同一 C044 机制的 `CheckCpointEscapeAndMismatchControlFlow` 旧动作计数/即时速度断言，原始结果另存 `selfcheck-result-03.txt`。脚本继续修改前，本ID精确范围增补该方法内两个负 decrease 正例断言：首例保留动作计数0、双方HitCount1、即时速度0；帧后结算速度仍-4/-3且HitCount0；方向控制例原抓取者动作计数2保持2、被抓者0，双方HitCount1、朝向left保持。缺槽/互指不一致等阴性断言不改。仍为同一跨零时序行为及同一已声明代码路径。

范围增补代码已写：`CheckCpointEscapeAndMismatchControlFlow` 首个跨零正例断言现检查动作计数0/0、HitCount1/1、原位置及未结算速度0；方向控制正例检查HitCount1/1、抓取者原动作计数2与被抓者0、朝向left。帧后处理断言原已符合当前规则且未改。生产、DAT、Scene、阴性分支未改；下一编译与原Editor同一SelfCheck重验。

2026-10-02 修后验收：`dotnet build Assembly-CSharp-Editor.csproj --no-restore -nologo '-clp:ErrorsOnly;Summary'` 退出0、0 error/281 warning；原Editor刷新后DLL新于脚本且idle/nonPlay。完整SelfCheck第四轮越过两个 C044 方法，首差已进入独立 B5 `CheckStandardCharacterDamageAlignmentContracts` effect22 断言（`selfcheck-result-04.txt`），故本组限定`FOCUSED_TEST_PASS`，完整SelfCheck仍FAIL。第三轮同组首差另存`selfcheck-result-03.txt`，第二轮保留`selfcheck-result-02.txt`；每次菜单覆盖的是前一次本轮生成的Temp结果，覆盖前均另存。第四轮MCP命令30秒超时，但磁盘结果有更新且明确FAIL，不能据菜单返回称PASS。四保护文件SHA与`selfcheck-protected-before-01.json`逐项相同；生产/DAT/Scene/非战斗未改。

后继独立B5/C056/C054旧断言更正后，第八轮原Editor完整SelfCheck磁盘结果`PASS`（`selfcheck-result-08.txt`，SHA-256 `2F9ACB02FAA121BB2A3621951F57B4C690655337EDEE2E5AC350BE2B3BE88EA8`）；C044本组测试口径`VERIFIED`，C044父门与Q07仍开放。
