<!-- CHANGE-RECORD
id: NTSD28-336B44-Q10-C053-DOUBLE-HIT-AUDIO-AUDIT-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q10Diagnostics/c053_natural_double_audio_probe.cpp
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C053NaturalDoubleBattlePlayProbeEditor.cs
authority: selected 336B44 playable GameSession28 last_tick audio_events and same-seed original Unity Battle Scene
evidence: docs/ai/TASKS/NTSD28-336B44-Q10-C053-DOUBLE-HIT-AUDIO-AUDIT-001.md
-->

# C053 双自然命中逐tick音频首差审计

脚本前记录。Q07/C053三人自然双命中原Scene所选战斗字段132/132同正式源，但现有只读hit plan ShadowCompare于tick7两次writer音频bit63 mismatch。生产`QueueBattleSound`使用源规则X，投影`ProjectQueuedSound`多处仍传物理X；bit63还合并队列计数、cue、tick、指纹，不能仅凭静态代码断定原因。正式336B44 `WorldAudioEvent28`含source/world_x/channel/resource_path，Unity`PendingSoundEvent`含cue/worldX/tick；须逐事件观察。

本包仅新增离线C++诊断及扩展现有请求式Editor探针为第三唯一RunId，输出两个版本的真实逐tick事件。不改生产/正式DAT/Scene；首两轮ShadowCompare及无Shadow结果均保留。源/Unity事件格式差异须显式映射并留下原始值，不因正常化丢证。范围、验收、风险与回滚见Task。脚本改后登记实测、未证、聚焦验证；若音频首差确立，另建生产改动合同。

实际修改：新增 `Tools/NTSD28Q10Diagnostics/c053_natural_double_audio_probe.cpp` 导出正式 `audio_events` 的 source/channel/path/X；复用 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C053NaturalDoubleBattlePlayProbeEditor.cs` 只加唯一 Q10 请求/结果路径、关闭只读 ShadowCompare 并采集每 tick 真实 `PendingSounds`。该复用改动覆盖测试探针中前一 Q07 请求入口；此前 JSON 均原样保留。生产 `QueueBattleSound`、DAT/图片/Scene/配置均未修改。

验证：正式根 EXE SHA 336B44…7BD3；正式诊断闭包编译 exit0/stderr0、两次各18行且同SHA；`dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly` 配合 Temp-only targets 对测试脚本编译 0 error（250 warnings）；原 Editor 刷新并实际 Play 12 tick，结果 `SCOPED_PASS / DONE`。10 个音效事件逐 tick 计数、路径、X、时点、顺序 42/42；11 个战斗字段×12 tick 132/132，共174/174。Battle/Menu/GameConfig/Mode Asset 四保护SHA相同，Scene clean、已退出Play。原始/机器报告见[本包报告](../../../artifacts/diagnostics/NTSD28-336B44-Q10-C053-DOUBLE-HIT-AUDIO-AUDIT-001/REPORT.md)。`Tools/Validate-ChangeLedger.ps1` 结果另补于本次最终核验记录。

未验证：`PendingSounds` 之后的 clip 选择/播放 voice/声像/音量/扬声器，玩家物理键自然选招、全 World 和 Q10 整体。旧 Q07 ShadowCompare writer bit63 仍报告不一致；实际音效队列同正式版，投影的物理 X 与真实源规则 X 是未逐字段归因的诊断候选。若修改投影，另建 Change。回滚须先审阅本包代码与诊断原件，不删除或覆盖已有用户文件。

最终核验：`Tools/Validate-ChangeLedger.ps1` exit0、`Change ledger validation PASSED`，1120 Records、当前 diff 的2个脚本均 COVERED；非当前 diff 的历史声明有 WARNING，不影响本包覆盖，原始输出保留在[校验日志](../../../artifacts/diagnostics/NTSD28-336B44-Q10-C053-DOUBLE-HIT-AUDIO-AUDIT-001/change-ledger-validation.txt)。`git diff --check` exit0，仅 Git 的 LF→CRLF 工作区提示，无空白错误。
