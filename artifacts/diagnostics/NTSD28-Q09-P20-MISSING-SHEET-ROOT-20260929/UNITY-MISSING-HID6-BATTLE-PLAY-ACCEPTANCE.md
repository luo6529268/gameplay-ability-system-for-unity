# Q09/P-20 原 Battle Scene 隔离缺 hid6 自然 Play 验收

状态：`NTSD28-Q09-P20-MISSING-SHEET-BATTLE-PLAY-001 / VERIFIED_SCOPED_ORIGINAL_BATTLE_PLAY`。这是缺单张**角色本体**图的实际 Battle 场景运行证据；Q07/BATCH-04、Q09/P-20 其它分支和总目标仍开放。未把本项计入 Q07。

权威对照：唯一正式根 `NTSD2.8-Logan.exe` SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033` 在只缺 `c/hid/hid6.png` 的隔离资源根自然回放 30 tick，32/32 World hash 与完整根相同，tick16–31 的对应 sprite 命令由 2 条降为 1 条，见[正式负例验收](ACCEPTANCE.md)。原 Unity 项目临时根按[准备记录](UNITY-NEGATIVE-ROOT-PREP.md)保留其它资源，九张飞段 PNG 逐 SHA 与原内容一致，只缺 hid6；原 Unity 正式内容根 `Assets/NTSD/Content/LoganRuntime` 的 hid6 仍在。

原 Editor PID11944 在唯一保存的 `NTSD_Battle.unity` 上执行了独立请求 `missing-hid6-battle-20260929-01`。进入 Play 前，诊断探针检查保存的 GameConfig Asset 和临时根，并用 `DontSave` 副本选择临时根；Scene 加载时再次确认该副本。物理 J1–2、K9–10、L+D+J13–14 经现有输入链和完整 Driver 自然到 430/pic119。原始[缺图报告](missing-hid6-battle-20260929-01.json) SHA-256 `C6700F98A5E50FEEFF58F75F538C15EF0393FDD988D2EF32E019AA65AC2CEA85`：30 个已完成 tick，起止 5→35；第一次 430 是相对 tick15、绝对 tick20，`actorRenderPic=119`、快照 pic119，中央帧有效、World Camera 启用；`actorBodyCommand=false`、`actorCatalogEntry=false`，目标角色本体命令仍为 true，绘制前后战斗 checksum 相等。状态 `PASS_SCOPED_MISSING_BODY_FRAME430`，`firstDifference`/`error` 均空。

将其与原 Editor 完整根同一自然物理输入的[阳性报告](../NTSD28-Q09-P20-HIDAN-BATTLE-COMMAND-001/hidan-frame430-binding-20260929-01.json)（SHA-256 `42FCE3415A41AF76A76EBE9C8F4EA1AD5FA5BB87943EF84D950D163D9E1C6EF7`）逐 tick 比较：两侧各30行，选定的 tick/input phase、物理键、同步 RNG 前后计数/索引/调用点、对象/slot 数、两角色的 slot/stable ID/OID/frame/HP/PP/方向/物理及规则 X/Y/Z/抓取关系、两玩家 buttons/pressed/released 共54字段每 tick，**1,620/1,620 相同**。首次430帧两侧的战斗 checksum 也相同；完整根 actor body 有、缺图根 actor body 无，目标 body 两侧都有，中央命令总数恰少1。该比较只覆盖选定 Unity 运行字段与一帧命令，不代表两端使用相同地图/Z初值或逐像素等价。

退出时报告 `shutdownStatus=Completed`、`shutdownStage=RuntimeMapCleared`，World 已解绑、对象/slot/池借用均0，物理键回到 neutral；原 Editor 回到 Edit/idle，Battle Scene `isDirty=false` 且磁盘哈希前后相同。探针 `diagnosticConfigRestored=true`，表示它销毁自己识别的诊断副本并重新绑定保存的 GameConfig。保存资产的本轮 SHA-256：Battle `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`、Menu `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`、GameConfig `0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`、ProjectBattleModeConfig `B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`、EditorBuildSettings `8D621A077642B5154BA305861CA57DFB549FF4FDE5BC58F3E284536A5982F01E`，均与 Play 前一致。没有修改 DAT 值、正式或 Unity PNG、Scene、保存的配置 Asset、ProjectSettings、生产战斗脚本或非战斗逻辑。

本 Play 关闭了“缺选中角色本体 sheet 时，原 Unity 战斗不能启动/不能自然到帧”的验证缺口。它不证明所有缺图组合、正式根自身窗口 GPU 与 Unity 同视口逐像素、Q09/P-20 全矩阵或最终游戏表现完全对齐。以上限定以原始报告和保存的正式负例为准。

最终治理检查：`dotnet build Assembly-CSharp-Editor.csproj --no-restore -nologo -v:q -clp:ErrorsOnly` 0 error/214 warning；原 Editor 的 `Assembly-CSharp-Editor.dll` 时间晚于本探针源码，完成导入并产生上述真实 Play 报告。`Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <本仓库>` 退出0/PASSED（1010 Records、38 个当前 diff 代码文件覆盖）；`git -c core.safecrlf=false diff --check` 退出0。五个保存文件的 Git diff 路径均为空。本包只扩 Editor 诊断脚本；没有重复跑角色矩阵或全套测试。
