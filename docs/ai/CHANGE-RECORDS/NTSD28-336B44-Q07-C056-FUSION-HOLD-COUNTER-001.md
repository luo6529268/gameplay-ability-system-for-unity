<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C056-FUSION-HOLD-COUNTER-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/fusion_hold_counter_probe.cpp
code-path: Assets/NTSD/Scripts/Simulation/Passes/Oid5152/BattleOid5152RuntimeModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06FusionRecordTransactionEditorTests.cs
authority: selected 336B44 playable fusion merge preserves old counter and latches across action replacement
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C056-FUSION-HOLD-COUNTER-001.md
-->

# C056 合体停帧动作计数及 OPoint 时点

脚本前登记。当前源码 `battle_world.cpp` 融合主角先切动作，再恢复合体前 `action_latch`、`tick_action_snapshot`、`frame_counter`；`simulation_tick_driver.cpp` 后续只在计数0时进入 OPoint 出生。Unity `TryMerge` 当前 `SetAction` 清计数和旧快照。预期副作用是合体帧进度、同 tick 子体物化及其后续池占用；不动融合距离、角色数值、内容或菜单。

先做正式当前源码正反完整 tick 诊断与 Unity 聚焦 RED，再决定最小生产改动。验收和回滚见 Task；未取得非零计数阳性前不报告修复，也不以旧版 JSON 裁决新版。此 Record 保留后续实际文件、命令、失败及未验项。

实际：新建 `fusion_hold_counter_probe.cpp`，C++20 首编失败（`char8_t` 与当前源码路径拼接不兼容），原日志保留；C++17 当前28 Core+3 playable编译0诊断。正式内容 GameSession 非零计数/停顿正例，三tick 0/0/0 OPoint 出生；零计数停顿对照1/1/0；双跑CSV同SHA，详报告。Unity既有融合测试新增聚焦断言，原生产 `TryMerge` 只改为保留计数/快照的动作写入并复位声音锁存；`dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly` exit0、264警告、0错误。原 Editor 尚未编入新脚本，未实测RED/GREEN或Play，保持 `COMPILE_PASS`。旧融合 JSON 为历史，不作为336B44当前测试预期；需后续重新定版。保护四资产SHA稳定。回滚仅本ID代码行级变更，不能覆盖已有脏工作。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C056-FUSION-HOLD-COUNTER-001/REPORT.md)。

原Editor首次聚焦新测试1/1 PASS，旧融合行0按旧 source4 JSON 失败，仅三字段（actionLatch/snapshot/counter）首差。原JSON不改，测试代码对成功融合到拆分前的旧预期做336B44三字段投影；生成工程重编译0错误、233警告，原Editor二次刷新后旧融合行0及C054拆分2/2、旧融合行1～3 3/3 PASS。历史失败及初次桥接请求超过20秒但异步任务继续成功均留证。Scene/Menu/两配置四SHA稳定；正式根与原Scene完整Driver/有序退出未验，状态 `FOCUSED_TEST_PASS / RUNTIME_PENDING`。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C056-FUSION-HOLD-COUNTER-001/REPORT.md)。

后继原Scene专包已完成受控c7/c0各3完整Driver tick：动作/计数/停顿及出生0/0/0、1/1/0同源码，c0两次OID213，退出池0/Scene clean/四SHA稳。首轮无效探针前置差留原件。本父改动现 `SCOPED_SCENE_PASS / RUNTIME_PENDING`，正式根EXE同受控条件、源活体数及本Scene关闭阶段尚无证据；见[Scene报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C056-FUSION-SCENE-PLAY-001/REPORT.md)。
