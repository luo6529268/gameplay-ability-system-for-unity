# C053 三角色自然双 OPoint 原 Battle Scene 对照

状态：`VERIFIED_SCOPED_NATURAL_DOUBLE_SCENE / C053_OPEN`（2026-10-02）。当前战斗规则权威是根正式 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 对应的 playable live path；本案例参照该闭包的[三人完整 GameSession 源 CSV](../NTSD28-336B44-Q07-C053-NATURAL-DOUBLE-PRODUCER-001/source-run-01.csv)，不是旧 B1E13 规则。正式根 LFR 不支持第三槽所需的初始动作覆盖，故不以不等价根 trace 冒充本组三人同态。

原 Battle Scene 的 Play 副本在 bootstrap Start 前配置三名正式角色：slot0/slot2 OID65 安科、team1、action511、源 X580；slot1 OID702 自来也、team2、action553、源 X500；三人 Y0/Z400、HP/MP500，seed682973786、mode0、difficulty0，之后逐 tick 输入中性。两 OID875 与 OID808 全由正式资源与生产 OPoint 自然生成，没有测试手工补体；初始 action 受控设置，不等于玩家物理键自然选招。

新增[请求式测试探针](../../../Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C053NaturalDoubleBattlePlayProbeEditor.cs)及唯一 `.meta`。生成 Editor 工程用 `Temp/NTSD28-Q07-C053-natural-double-scene.targets` 显式纳入新文件，`dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly -p:CustomAfterMicrosoftCommonTargets=...` 为 **0 error / 293 warning**。原 Editor `refresh_unity` 后，`Library/ScriptAssemblies/Assembly-CSharp-Editor.dll` 时间晚于探针且包含其类型；原项目没有启动第二 Editor。

[原 Scene 结果](ank580-ank580-jira500-natural-double-scene-01.json) 为 `SCOPED_PASS / DONE`：生产 Driver 从稳定暂停的全局 tick5 开始再运行 12 个完整 tick。与正式源码同初态 X580/580 的每 tick 11 个选定字段共 **132/132 零差**，见[逐字段差异清单](source-unity-selected-comparison.json)。比较字段为三角色 action、OID875 数量及 owner0/2 数量、逐槽 attacker `slot:owner:action:X` 序列、OID808 的槽位/action/锁存/HP。源码与 Unity 都在相对 tick1 自然生 OID808/slot50，tick4 生两个 OID875/slot51、52，tick7 子体 action/锁存156、HP450；Unity 的两个独立 victim rest 在 tick7 均为10。tick8 又各自然生成一名新 OID875，tick10 子体自然消失。Unity 未导出逐 hit 的内部 `applied/effect2/Uj156` 日志，所以正式源码的双 hit 记录与 Unity 末态、两个 rest 相符，但**不宣称逐 hit 内部锁存过程已直接同态**。

原 Editor 结束为 idle/非Play/非编译，active Scene 仍是 NTSD_Battle 且 clean；结果记录 `exitedPlay=true`、Scene 前后 SHA 相同。Battle/Menu Scene、GameConfig、ProjectBattleModeConfig 的[事前](protected-before.json)/[事后](protected-after.json) SHA 四项均不变。未修改生产 C#、DAT、图片、Scene、Prefab、ProjectSettings 或非战斗代码。没有为本定向探针重跑全案例或全量 SelfCheck；C053 的物理按键自然选招、逐 hit 内部字段、完整 World 和 Q07 整组仍开放。

交付审计：`Tools/Validate-ChangeLedger.ps1` exit0/PASSED（1118 Records、64 当前差异代码文件），[完整输出](ledger-validation.txt)保留；跟踪文档 `git diff --check` exit0。没有执行删除、还原、提交或推送。
