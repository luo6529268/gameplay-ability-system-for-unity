<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C053-AUX-TYPE3-SCENE-001
status: COMPILE_PASS
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C053NaturalDoubleBattlePlayProbeEditor.cs
authority: selected 336B44 root EXE and playable source controlled OID251 plus natural OID875 same-tick C053 hit witness
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C053-AUX-TYPE3-SCENE-001.md
-->

# C053 正式双命中原 Battle Scene 定向探针

脚本修改前登记。当前既有测试脚本只覆盖双安科OID875和Q10同案音频；新正式源/根阳性是两个初始角色自然生成OID875/808，外加受控第三OID251/action0，tick7对OID808两次applied。原状无同条件Unity逐writer证据，不能用既有双安科HP450证明HP440案例。

唯一脚本路径为现有 `NTSD28Q07C053NaturalDoubleBattlePlayProbeEditor.cs`；精确符号计划：请求/结果路由、`OnSceneLoaded`与`WaitForRoster`的opt-in初态、`TickRow/MeasureOneTick`选定字段与writer、`CompleteMeasurement`独立期望、`Finish`清状态。不改变旧runId、旧结果、生产战斗逻辑、资源或场景。新入口仅在请求文件且原Battle Scene clean时执行。

风险：Unity的factory生成OID251与正式LFR初始对象可能在owner/基础字段上不同；碰撞/HP若在tick7前分叉，应报告首差，不补测试专用生产逻辑。退出Play后必须通过已有Scene hash/clean检查；若当前Editor编译仍停滞，只做生成工程编译与静态核对，Play保持待验证。回滚仅按本记录审脚本增量，删除任何新增或旧文件须独立用户授权及审计。

2026-10-04 已在唯一脚本中增加独立 `AuxRequestPath/AuxRunId/AuxResultRoot`，旧Q10与双安科请求及结果不改；`OnSceneLoaded/WaitForRoster` 对新案保留两名自然OPoint角色并以现有 `BattleLogicEntityFactory.Create` 加入slot2/OID251/action0，使用共用源X/Z投影及Y=-60、mode0/seed同正式源。`MeasureOneTick` 对新案用两玩家离散空输入，记录辅助体/自然生成体/目标逐tick字段与目标命中计划、逐writer；`CompleteMeasurement` 新案单独校验tick7先slot2后slot51写者及目标HP440，旧双安科/Q10断言仍在原分支。`Finish`清辅助引用。未改Unity生产、DAT、图片、Scene、Prefab或非战斗。

生成Editor工程首轮 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly` 失败1个CS0246：新`ObjectPoint`缺少`NTSD.Animation` using；原日志保留。仅补该命名空间后同命令第二轮exit0、0 error、299 warning，原件在 `artifacts/diagnostics/NTSD28-336B44-Q07-C053-AUX-TYPE3-SCENE-001/`。此编译不代替原Unity Editor程序集重载或Play；当前状态`COMPILE_PASS / UNITY_RUNTIME_PENDING`，需复核Editor编译恢复后才请求该新案例。风险仍是factory初态或自然hit首差，届时按每tick和逐writer定位，不能把旧案例PASS当新案PASS。
