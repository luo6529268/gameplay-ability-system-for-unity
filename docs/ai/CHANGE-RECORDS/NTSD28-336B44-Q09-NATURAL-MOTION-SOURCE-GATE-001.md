<!-- CHANGE-RECORD
id: NTSD28-336B44-Q09-NATURAL-MOTION-SOURCE-GATE-001
status: VERIFIED
change-kind: BATTLE_Q09_EDITOR_DIAGNOSTIC_NATURAL_MOTION_SOURCE_GATE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09NameplateNaturalPlayProbeEditor.cs
authority: formal 336B44 playable presentation interpolation and Q09 natural-runtime evidence gap
evidence: docs/ai/TASKS/NTSD28-336B44-Q09-NATURAL-MOTION-SOURCE-GATE-001.md
-->

# Q09 自然可见实体的插值源坐标门

脚本修改前记录。当前正式源码按精确 XYZ 对相邻实体采样，Unity 共用采样器另在 `HasSourceRulePosition=false` 时返回 `SourcePositionUnavailable`。现有聚焦测试覆盖合成输入，2026-10-02 原 Battle Scene 同帧截图 JSON 未逐实体记录该标志，不能判断自然可达性或画面影响。[只读复核](../../../artifacts/diagnostics/NTSD28-336B44-Q09-INTERPOLATION-BOUNDARY-AUDIT-20261002/REPORT.md)。

原脚本 SHA、唯一所有权、前置条件、副作用、不可覆盖范围、验收和回滚方式见同 ID Task。本 Change 只向既有 Game View 报告增加可见实体当前/前帧 source flag、相邻帧采样状态和计数，不改变旧名牌断言、键盘输入、暂停、截图、生产模拟或表现。记录必须区分无前帧、关系/身份/运动拒绝与源坐标拒绝；没有自然阳性不改共用采样器。

已改唯一声明脚本的 Game View 报告分支：`CaptureVisibleMotion` 对已有 `Entity` 命令逐 handle 记录前后源坐标初始化标志、无前帧或 sampler 状态，并累积相邻可见实体及缺源坐标拒绝数；仅诊断读取，不改变生产、原输入、暂停、截图或历史非截图分支。新增字段为 `visibleMotionCount`、`adjacentVisibleMotionCount`、`visibleMissingSourceCount`、`visibleMotionSamples`。

生成 `Assembly-CSharp-Editor.csproj --no-restore` exit0，251既有warning/0 error，`git diff --check`通过；原Editor导入后程序集时间晚于脚本、非Play/Scene clean。首轮Play在MCP从6400切换到6401后才投菜单，战斗已到tick5304；探针171次方向采样X始终800，180tick门FAIL、未采样也未截图，唯一[失败JSON](../../../artifacts/diagnostics/NTSD28-336B44-Q09-NATURAL-GAMEVIEW-TICK-001/natural-nameplate-20261002-004008-787-4818edbea55546e99ec2918ed14ff5cd.json) SHA-256 `4B014B667D09DE081D6D92A39283C5F21DFECB531EC1769F59E882846CD29517` 保留。原Editor已退出Play、idle；不能把此结果当插值阴性。下一仅新局尽早重跑同一探针。

最终验证：新局早启及Play前聚焦另两次输入前置失败均以不同JSON留证；Editor在Play中调用自身Game View菜单后 `is_focused=true`，同一探针成功按D移动P1 X800→856。原Battle Scene发布/计划/截图后tick2152一致，两个可见OID2前后source=true、采样状态均Sampled、缺源拒绝0，1920×1080图已查看。退出idle/nonPlay/noncompiling、Scene clean/13根、四保护SHA及旧JSON/PNG SHA稳定，LoganRuntime无Git差异。生成Editor工程exit0/251既有warning/0 error；`git diff --check`exit0；ChangeLedger validator exit0/PASSED、1140 Records、16脚本diff覆盖。[完整原件、哈希与边界](../../../artifacts/diagnostics/NTSD28-336B44-Q09-NATURAL-MOTION-SOURCE-GATE-001/REPORT.md)。仅`VERIFIED_SCOPED_NATURAL_FRAME`；正式EXE像素、其它实体/断点及Q09总阶段开放。无文件删除。
