# 普通三角色队伍选择更正与最小读点

方案更正：撤回CASE-MATRIX-CANDIDATE及V2中的“三人同一team”候选，不能因UI三个Confirmed就断言长窗成立。

已有当前权威执行表`Assets/NTSD/Docs/ntsd28-logan-336b44-vs-unity-battle-alignment.md:784`及其C009原件，具名记载第二存活阵营返回会暂停结果倒计时，剩一组恢复；源/根/原Scene有受控限定同态。故同队单阵营有提前结果流程风险，不需要通过新Player撞一次来发现。该旧证明不自动证明本Menu具体mode/team映射，不声称当前正常三人已测试。

角色、技能、次数与窗口不改：2/16/507三个真实人类槽，P1轮换，其他人中性，0电脑，一次组合。队伍改为候选的不同合法选项、至少两个真实存活阵营；**暂不写死SelectTeam index对应的team值**。保留真实Menu出生位置，不手调HP/PP/动作、胜负或结算，不强续已经结束的Battle。实际资格必须mode/team/阵营数满足公开合同、仍Running且结果流程未提前开始；若技能自然导致终局，保留该事实、停止余轮，不能把结果页/后半空场计作原1800tick稳态。

最小已定位新只读请求：

1. `Assets/NTSD/Scripts/UI/SelectRoleItem.cs::GetFinalTeam`588–606（下一声明ShowCountdown在608），以及与它直接相连的公开队伍可选范围数据。目的只确认UI选项→team/独立阵营语义；不展开slot输入更新或其它角色流程。
2. `SelectGameModeController::OnVSMode`53–59的真实VS配置来源，已有review-02请求；结合纯数据GameModeConfig两值，不能把default battleGameModeId1写成实际VS值。
3. **仅上述和既有C009证据仍不足时**，再请求`Assets/NTSD/Scripts/Simulation/Ecs/Results/BattleResultsOutcomeHostWriter.cs::AdvanceNativeFlowBeforeCombat`20–92、`UpdateSummaryActivation`138–244，裁到mode gate/live group/结果开始退出条件，不继续其它writer/caller。该路径可能属活跃冻结范围，当前只rg声明/结束括号定位，未打开方法体；如需读取由主会话按现有授权明确处置，不能泛读Q06或改规则。

复用证据入口为C009 RETURNING-GROUP/RETURN-RESUME/RETURN-RESUME-SCENE；本轮只查当前总表已核状态/链接，没有运行这些旧组。具体队伍值将在公开映射供证后补同一CASE表的新澄清，不新建实验/进程或优化Task。
