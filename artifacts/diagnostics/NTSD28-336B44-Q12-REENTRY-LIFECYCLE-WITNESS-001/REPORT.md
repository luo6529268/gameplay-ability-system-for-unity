# Q12 原 Battle Scene 同冻结版退出后重进见证（2026-10-04）

结论：`PASS_SCOPED_REENTRY`。原 Unity Editor PID 138072、同一 `NTSD_Battle` Scene 连续两轮真实 Play，生产 `SimulationTickDriver`、World、角色及逻辑 tick 均重新运行；每次退出后 Battle Scene clean，Scene 内 Driver 的 World 已解绑，活动池对象和 sprite 均为 0。该结果完成 Q12 代表矩阵中的“同冻结版重进”子门；不证明所有战斗规则、正式 EXE 同帧画面或真人按键完全一致。

本轮唯一测试代码是 [Editor-only 只读见证](../../../Assets/NTSD/Scripts/Test/Editor/NTSD28Q12ReentryLifecycleWitnessEditor.cs)，不注入键盘、不改 DAT、Scene、生产脚本或非战斗模块。已由原 Editor `Assets/Refresh` 导入，生成 `Assembly-CSharp-Editor.csproj` 包含新脚本后运行 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly`：0 错、299 警告。原 Editor 两次执行新菜单并实际生成四份 `FileMode.CreateNew` 原件。

| 轮次 | live 生产 World | live tick | 活动角色/实体 | 退出后 World | 退出后活动池对象/sprite | Scene clean |
| --- | --- | ---: | ---: | --- | --- | --- |
| 第一轮 | 有 | 419 | 2 / 4 | 已解绑 | 0 / 0 | 是 |
| 第二轮 | 有 | 256 | 2 / 2 | 已解绑 | 0 / 0 | 是 |

四份原件：

- [`cycle-20261004T131738357-cfcffe4c2c51470a97944aa66d7d4930-live.json`](cycle-20261004T131738357-cfcffe4c2c51470a97944aa66d7d4930-live.json) SHA-256 `CEFC255774230A30428F8BB0BF9A874E0A08717FE9C909EC9D1EC30A454CCC61`。
- [`cycle-20261004T131738357-cfcffe4c2c51470a97944aa66d7d4930-exit.json`](cycle-20261004T131738357-cfcffe4c2c51470a97944aa66d7d4930-exit.json) SHA-256 `5FAE77022B7C80F3F55D131C42A940309502D6BA463B23E01FCE9BE0A0078A4A`。
- [`cycle-20261004T132047138-00b946d5e2e94ff9b38185aef4bcbd65-live.json`](cycle-20261004T132047138-00b946d5e2e94ff9b38185aef4bcbd65-live.json) SHA-256 `378736DEEEAB4FCE92597C2108F1A8FA5F76C68AD063D4FAD756A2ADA84474D9`。
- [`cycle-20261004T132047138-00b946d5e2e94ff9b38185aef4bcbd65-exit.json`](cycle-20261004T132047138-00b946d5e2e94ff9b38185aef4bcbd65-exit.json) SHA-256 `C5F7E6305E34D5F520DF5491A93665770AE653347D10FCA7C554DAE724623C54`。

两轮前、轮间及轮后同一 SHA-256：Battle Scene `F585EBCC4F170DEC8C8B1C372F5D3BF99E55B74889119102E620C9110F79F910`、Menu `9EAAA0B4782974D74A017C367C9D5C77326C31D4281A2D820D1CBBA76986C1BA`、GameConfig `0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`、ProjectBattleModeConfig `B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`、`Assembly-CSharp.dll` `E1855B12488A274981DCF19BC031AD588D11DC6C8796D3B454CB3E3FD08D5743`、`Assembly-CSharp-Editor.dll` `143B0A5C0F2CD29580AA3A6D02854A8D3FAEA3AF060C0DE3AC6B623666ED5C26`。`LoganRuntime` 当前 3093 个文件的最新写入时间早于两轮 Play；未发现本轮时间窗的资源文件写入。该时间核对是内容未变化的辅助证据，不替代整树逐字节 hash。

退出快照的 `poolCount=0` 表示该 Scene 中池对象已经销毁，因此 `poolQuiesced=false` 是“无可检查池对象”的序列化值，不代表仍有借用。十一阶段关闭顺序、全局零残留和池 quiesced 的较强证据复用既有 [C056 原 Scene 报告](../NTSD28-336B44-Q07-C056-SCENE-SHUTDOWN-001/REPORT.md)；本次仅补两次实际重进。自然鸣人、受控战斗、Game View 与生产声效各自限定证据保持原 Q12 Task 口径，未在本包重跑。
