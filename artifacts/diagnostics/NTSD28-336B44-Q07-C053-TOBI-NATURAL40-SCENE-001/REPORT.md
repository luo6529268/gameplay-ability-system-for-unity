# Q07/C053 OID0 Tobi 自然出生链：原 Battle Scene 40 tick

状态：`VERIFIED_SCOPED_SCENE`，只关闭当前正式 OID0 普通输入→OID251 自然出生这一个同初态链的 40 tick **八字段**原场景子门。Q07/C053、Q12 和总目标仍开放；不把后续阴性 action0 当成该动作全条件不可达。

当前权威根正式 `NTSD2.8-Logan.exe` 的 SHA-256 为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。对应 playable [正式源/根证据](../NTSD28-336B44-Q07-C053-TOBI-PHYSICAL-REACH-001/REPORT.md)使用 OID0 Tobi X600/Y0/Z400、OID2 对手 X1100/Y0/Z400、seed682973786、mode0：tick1–4 跳跃，tick8–10 防＋右＋攻，40 tick 所选八字段源/根320/320同。Unity仍使用项目自有背景、地图和统一坐标投影，没有引入正式背景或模式DAT。

在原 Unity 项目唯一已保存的 `NTSD_Battle.unity` 中，新40 tick菜单沿原探针的 Play clone 预配置、正式内容和生产 `SimulationTickDriver.StepOneTick` 提交同一离散输入。首次菜单调用因 Scene 当时变脏而在进入 Play 前安全拒绝，没有结果文件；第二次干净前置进入 Play，完成40个测量tick（全局tick5→45）。[Unity原始记录](tobi-jump-natural-40.json)有40行、`childBirthTick=11`；[机械配对](tobi-jump-natural-40-paired.json)把正式输入mask映射到项目既有legacy输入包，并逐tick比较输入包、输入相位、Tobi动作/Y、OID251槽/动作/X/Y，共**320/320值相同，首差0**。先前14tick原件未覆盖。

具体后续：tick11子体OID251/slot50/action51/X600/Y−110；tick14 action2/X604/Y−90；tick20 action3/X651/Y−30；tick25 action62/X663/Y−15；tick30后子体已不在World。它与正式源同态，但**没有自然进入受控双Uj用例所需的OID251/action0**，因此不能把受控双命中结论推广到本普通输入链。输入是直接提交的离散`FrameInputSet`，不证明键盘设备→Action→Provider链。

生成 Editor 工程执行 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly`，exit0、0 error、299 warnings；原Editor刷新后导入新菜单、非编译，定向 Play 实际完成。Play结果内Battle/Menu/GameConfig/ProjectBattleModeConfig四SHA前后一致。退出后原Editor非Play、唯一Battle Scene已保存；[独立EditMode残留检查](tobi-jump-natural-40-postplay.json)复核相同Battle SHA、原Scene序列化Driver1且绑定World0、Scene Pool0，状态`SCOPED_PASS`。本包未改生产战斗逻辑、DAT数值、角色图、音频、Scene、Prefab或非战斗。

仍待：其它正式可达输入/位置、自然action0同tick双Uj、真实物理键、完整World与借用数、正式EXE/Unity画面，以及Q07/C053和Q12总出口。若继续这一分支，应先取得新的正式可达阳性条件；不重复已过40tick链或用特判伪造action0。
