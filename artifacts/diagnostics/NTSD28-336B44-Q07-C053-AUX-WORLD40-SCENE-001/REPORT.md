# Q07/C053 受控辅助双 type3 命中：原 Battle Scene 40 tick 五槽对照

状态：`VERIFIED_SCOPED_SCENE`。只关闭同一**受控辅助体初态**的 40 tick 五槽 World 对照；C053/Q07/Q12 和总目标仍开放。

权威身份：本轮重新核对根正式 `NTSD2.8-Logan.exe` SHA-256 为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。对应 playable 源与根正式 EXE 的既有[40 tick 证书](../NTSD28-336B44-Q07-C053-AUX-TYPE3-DOUBLE-REACH-001/source-root-comparison.json)为五槽有效字段 `1136/1136` 零差，根 LFR 通过。原版背景1只作为共同 Z400 的诊断前置；Unity 使用项目自有地图。

受控初态为 slot0/OID65/action511/X610、slot1/OID702/action553/X500、slot2/OID251/action0/X700/Y-60/team1，Z400、seed682973786、mode0，所有输入为 None。第三体由探针明确创建，**不代表自然玩家选招能进入 action0**。前两名角色和其 OPoint 使用正式 LoganRuntime 内容。

原项目 Editor 在唯一已保存的 `NTSD_Battle.unity` 中运行生产 `SimulationTickDriver.StepOneTick` 40 次，结果原件为 [Unity JSON](ank610-jira500-firz700-yminus60-world40-01.json)，`MEASURED_COMPARE_PENDING/DONE`、40 行、退出 Play 且 Scene clean。离线[逐 tick 配对](world40-paired.json)将 slot0/1/2/50/51 的 active 总是比较，活跃槽再比较 OID/action/HP/源 X/Y/Z；不活跃槽 payload 不参与。正式源/Unity **1136/1136 有效值零差**；同 CSV 另有 CRT state/calls、同步计数/index/calls **200/200 零差**。tick1 自然生目标 OID808/slot50，tick4 自然生 OID875/slot51；tick7 辅助 OID251 为 action20/HP368、目标 OID808 为 action156/HP440，与正式源/根相同。原 JSON 保留探针内部 `MEASURED_COMPARE_PENDING`，最终比对结论以独立配对文件为准。

编译：新增模式首次导入后原 Editor DLL 晚于脚本，原 Editor 可调用该菜单；生成 Editor 工程 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly` 第一次 exit0、0 error/330 warning。补充残留只读菜单后第二次原 Editor 导入且 DLL 晚于脚本，同一生成工程 exit0、0 error/299 warning。未运行全套 EditMode 测试，此包仅一个目标 Play。

退出保护：Play 原件记录 Battle SHA 前后均 `8CC5614574321908CDB89BAEB6BE60E41616EB3CA6F6DDEF35B34F723269047E`。本轮前后另三保护文件的 SHA 不变：Menu `9EAAA0B4782974D74A017C367C9D5C77326C31D4281A2D820D1CBBA76986C1BA`、GameConfig `0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`、ModeAsset `B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`。独立 EditMode [残留读回](world40-postplay.json) 为 `SCOPED_PASS`：唯一 Battle Scene clean/root13；原序列化 Driver 1、绑定 World 0、Scene Pool 组件 0。该读回只覆盖原 Scene 可见残留，不声称逐阶段十一项关闭轨迹都被直接观察。

代码仅扩展 [Editor 探针](../../../Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C053NaturalDoubleBattlePlayProbeEditor.cs) 的独立菜单、五槽快照和残留菜单；本模式结果只在结束时 `FileMode.CreateNew` 写一次，不覆盖旧 AuxGreen 请求/结果。未改生产战斗规则、DAT 数据、图片、Scene、Prefab 或非战斗逻辑。后续仍须另证自然第三对象可达性、真实设备按键链、其他 C053 条件与整体战斗表现。
