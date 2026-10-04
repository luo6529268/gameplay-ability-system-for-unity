# Q08/C009 原 Battle Scene 恢复分支准备与编译边界

## 2026-10-04 原 Scene 定向 Play 限定出口（覆盖下方准备快照）

原 Editor 在同一项目重新编译后，保留 v1 原件，独立 `c009-oid220-resume-scene-v2` 于原 `NTSD_Battle` Play 副本完成 20 个生产 Driver tick。[运行结果](c009-oid220-resume-scene-v2.json) `PASS/DONE`，`exitedPlay=true`、`sceneCleanAfter=true`；[正式源/Unity逐字段配对](c009-oid220-resume-scene-v2-paired.json) 的相对 tick、timer、outputTimer、组 mask 位数、OID9 子体数、OID220 所在 World 槽位 action 及 World 成员身份共 20×7=140/140、零首差。正式根与源码此前同回放所选 140/140 字段一致，但二者字段集并非完全相同，不能据此声称全 World 同态。

v1 的 `emitterAction=1000` 读取的是回收后仍被探针持有的对象引用；v2 实测 tick1 槽50为 OID220/action1，tick2 起槽50为空、`emitterInWorld=false`，与正式版发射者消失时点一致。先前把 action1000 解释为“仍在 World”是错误推断，已由槽位采样纠正，生产生命周期无需因此修改。

v2 Play 前后 Battle/Menu/GameConfig/ProjectBattleModeConfig 四文件 SHA 各自相同；Editor 当前非 Play、无运行测试、唯一 Battle Scene clean。Battle Scene 在 v1 与 v2 **两次运行之间**已有不同磁盘 SHA（v1 为 `D88AD2…`，v2 为 `CACF66…`），写入来源未证；本结论只声称各次运行内部保护成立，保留该外部变化。此证据只关闭 C009 的受控 OID220→OID9 再次单组计时恢复原 Scene 子门；物理按键自然选招、其它初态、Q08 整体与最终 Q12 仍开放。未改 Unity 生产、DAT 值、图片、Scene、Prefab 或非战斗逻辑；未重跑全套 EditMode 测试。

2026-10-04 仅扩现有 `NTSD28Q08C009ReturningGroupBattlePlayProbeEditor.cs` 的独立请求 `c009-oid220-resume-scene-v1`。原 `c009-oid304-return-scene-v1`、原结果目录、12tick预期与原运行记录保留。新请求用生产工厂在原 Battle Scene 的 Play 副本建立 OID220/type3/action0/team2，保留原正式OID56/team1、seed682973786、源X500/900/Z650、中性输入，推进20完整生产 Driver tick；按正式[源/根证据](../NTSD28-336B44-Q08-C009-RETURN-RESUME-001/REPORT.md)核验 timer/output、组mask和OID9子体数。保护四文件SHA、唯一clean Scene及退出清理路径仍沿用原探针。

只读逐SHA：当前正式 `data/data.txt`、`c/ita/a/cro2.dat`（OID220）、`c/ita/ita.dat`（OID9）、`c/hid/rea.dat`（OID56）与 Unity `Assets/NTSD/Content/LoganRuntime/decoded_dat` 对应文件四对均相同；未改DAT值。`dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly` [生成工程日志](editor-generated-project-build.txt) exit0、0 error、299 warning，仅证明生成项目语法与引用通过。

原 Editor 的 `tests.is_running=false`、非Play，Battle Scene先前只读 `isDirty=false`；但其 `compilation.is_compiling=true/last_compile_finished=null`，`Library/ScriptAssemblies/Assembly-CSharp-Editor.dll` 时间2026-10-03 21:52 UTC早于本次脚本2026-10-04 04:30 UTC，故原 Unity 导入/编译、定向 Play **均未完成**。现有 `Temp/NTSD28_Q08_C009ReturningGroupBattlePlay.request.json` 为原 runId 且 `requested=false`，本轮未写入新请求或覆盖旧结果。正式源/根140/140不得提升为Unity场景通过；C009/Q08仍开放。

[Change Ledger校验](change-ledger-validation.txt) exit0、1233条Record、本脚本被本Change ID覆盖；脚本`git diff --check` exit0。未重跑刚被用户取消的全套EditMode测试。
