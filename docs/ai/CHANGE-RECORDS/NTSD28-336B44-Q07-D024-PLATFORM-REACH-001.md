<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-D024-PLATFORM-REACH-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/d024_formal_platform_reach_probe.cpp
authority: 336B44 playable GameSession full tick, formal OID56 frame182 and user D-024 ratio requirement
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-D024-PLATFORM-REACH-001.md
-->

# NTSD28-336B44-Q07-D024-PLATFORM-REACH-001

脚本前记录：Task 已限定唯一新增 Tools 诊断源码。原状是现有 Q09 原 Battle Scene frame130 单 tick 平台链接 PASS，D-024 连续搬运仅合成聚焦测试通过；正式唯一非零平台 DVX frame182 的完整 GameSession 条件未证。本包先做受控初态有限筛选，不改规则、内容、Unity runtime 或 Scene。预期副作用仅生成独立诊断可执行文件与 CSV/摘要；正式资源仅读。验收和回滚见 Task。筛选阳性也不能直接称玩家自然按键可达、原 Battle Scene 连续搬运或 Q07 完成。

2026-10-03 `CODE_WRITTEN / SOURCE_CONTROLLED_POSITIVE`：唯一 Tools 源码编译 exit0。正式资源完整 `GameSession28` 八组 X205/208 × Y−10/−5/0/5 各10tick 中，X205/Y−5 目标链接连续10tick、源 frame182 连续10tick，目标规则 X205→178；X205/Y−10仅1tick链接且无搬运，其余六组0。原 CSV/摘要 `artifacts/diagnostics/NTSD28-336B44-Q07-D024-PLATFORM-REACH-001/source-run-01/` 保留；这只是受控初态，尚无根 EXE LFR、原 Unity Scene 或自然输入。下一在同一诊断脚本加阳性 LFR 记录并以根正式 EXE 回放，另存 source-run-02，不改旧输出。该补充在修改脚本前已写入 Task 与本 Record。
2026-10-03 `VERIFIED / CONTROLLED_REACH_ONLY`：唯一 Tools 诊断源码第二次编译 exit0，source-run-02 新增9982字节阳性 LFR，八案 CSV/摘要与 source-run-01 各逐 SHA 一致；根正式 EXE 回放 exit0、`passed=true`、报告 `nativeParityClaim=false`，共有初态+10tick两实体九字段99/99、首差无。正式 EXE SHA仍为336B44，正式/Unity OID56 DAT同 SHA `27F0B91F...C010F75D`。原场景由独立 Scene Change 验收，不能从本 Tools 包推断自然按键。无正式源、EXE、DAT或Unity生产改动。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-PLATFORM-REACH-001/REPORT.md)。
