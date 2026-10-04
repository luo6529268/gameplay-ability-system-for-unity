<!-- CHANGE-RECORD
id: NTSD28-336B44-Q12-REENTRY-LIFECYCLE-WITNESS-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q12ReentryLifecycleWitnessEditor.cs
authority: user-approved 336B44 Q12 representative matrix and original Battle Scene reentry; existing C056 ordered shutdown evidence
evidence: docs/ai/TASKS/NTSD28-336B44-Q12-REENTRY-LIFECYCLE-WITNESS-001.md
-->

# Q12 原 Battle Scene 同冻结版重进生命周期见证

2026-10-04 `VERIFIED / SCOPED_REENTRY_PASS`：实际文件为 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q12ReentryLifecycleWitnessEditor.cs` 及 Unity 自动生成关联 `.meta`；只新增 Editor 菜单快照入口，不改变生产行为。原 Editor Assets/Refresh 导入后，生成 Editor csproj 包含本文件，`dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly` 0 错/299 警告。连续两轮原 Battle Scene Play，live tick 419/256、两轮各 2 名活动角色；两次退出后 Scene clean、Scene Driver.World 空、活动池对象/sprite 0。Battle/Menu/两配置与两个程序集 SHA 在两轮前后相同。四份原件、SHA 和适用范围见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q12-REENTRY-LIFECYCLE-WITNESS-001/REPORT.md)。旧合成输入试验已回滚；其失败不阻断本独立生命周期子门。未知：正式同帧 Present、真人手按、a7 自身实播及本场景以外的全面游戏表现；不声称总目标完全对齐。

2026-10-04 已新建唯一 Editor-only 脚本 `NTSD28Q12ReentryLifecycleWitnessEditor.cs`：两个菜单分别只读抓取生产 Driver/World、tick、角色、池及 Scene SHA/dirty，并以 `FileMode.CreateNew` 产生 live/exit 原件；没有键盘事件、生产写入或 Scene 保存。先前 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly` exit0/0错/332警告，但当时生成 csproj 尚未纳入新脚本，不能算本脚本编译通过。原 Editor 已请求 scripts refresh；待实际导入、重建后再记编译状态。旧诊断脚本 diff 已精确归零。两轮 Play 尚未运行。

脚本改前登记。原状、精确代码路径、输入与生产不变量、验收及回滚见[Task](../TASKS/NTSD28-336B44-Q12-REENTRY-LIFECYCLE-WITNESS-001.md)。仅新增 Editor-only 只读采样入口，预期不会改变战斗、Scene 或 ProjectSettings。待运行后登记真实文件、编译、两轮 Play 和残留结果；编译或单轮 Play 不足以标为完成。
