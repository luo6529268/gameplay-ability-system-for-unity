# Q08 项目 stage gate 生产接线限定验收

状态：`VERIFIED / SCOPED_PROJECT_STAGE_GATE`。对应 `NTSD28-Q08-PROJECT-STAGE-GATE-001`；Q08、BATCH-04 和总对齐目标仍开放。

正式依据：根 `NTSD2.8-Logan.exe` SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033` 已直接输出普通 mode0/gate1 鸣人分身 slot50 X 为未生/-42/0；配对 playable `GameSession28::step` → `SimulationTickDriver28::step` → `BattleWorld28::settle_ordinary_stage_bounds` 确认 gate1/3 的 type0 全槽角色边界，其他 gate 的高槽临时边界。正式根录制与 LFR 回放的 CRT RNG 初态不同，本报告仅判该有界分身结果，不声称全状态同态。

生产改动：项目自有 `ProjectBattleModeConfig.asset` 显式选中 gate1，经冻结快照/内容指纹在首次 battle seal 前发布到 Client runtime；DataOriented 与 Legacy 的 type0 物理 X 和源规则 X 共用正式边界公式。新增标量进入 reset、core/aggregate snapshot、restore、runtime checksum 和 extended/lockstep parity；旧 aggregate 版本被拒绝。tick0 恢复后的再次准备不得覆盖已恢复 gate，下一场配置重置和有序关闭时清除发布标记。原版背景/模式 DAT 不作为生产输入，DAT 值未改；D-024 比例移动及 D-025 非角色离可行走区域 304 tick 清除不改。

原 Editor 首轮聚焦 RED：15/15 因缺配置字段/标量失败，job `6fbb7d6526d7430c8bad3cb01fa36f94`。生产接线后 26/26 GREEN，job `1cb830e5eb6c43d4843bd45677f10314`；相邻 D-024 非角色边界、D-025 304 tick 和模式发布 18/18，job `2920fdbdd64a4869930d6a7eaf4dad1d`。tick0 快照恢复后再次发布先 RED（期望 gate3、实际 gate1，job `4cb1954aa0764447a6be0b0299361126`），生命周期修复后 3/3 GREEN，job `26ac636f266a4761894961f8396f4523`。原始 job JSON 均在本目录，记录历史失败而非抹去。

原项目原 Editor 完整 Driver 三 tick，正式 `catalog.csv` 内容身份门下：生产 gate1 分身 slot50 的源规则 X/物理 X 为未生/-42/0，导出 `unity-naruto-clone-stage-gate1-production-2.raw.jsonl`、`.stage-source.jsonl`，结果 PASS。测试专用显式 gate0 运行时覆盖，不改真实 Asset；同场景为未生/-42/-49，导出 `unity-naruto-clone-stage-gate0-production-1.raw.jsonl`、`.stage-source.jsonl`，结果 PASS。后两份源坐标 sidecar SHA-256 分别为 `43B2B8A9E003356DEAD592FA12EABEF7608075E4954F534CDBE93743A5BCFF4F` 与 `3AC2E3D7DF5D1B288B275B4FCCABC22896DBA5E84B82F22DEE62FDFD62C6048A`。首次请求文件直接写入与 Editor 轮询相撞，日志出现 sharing violation；改为先写临时文件再原子发布，后两次正式请求均 PASS，首次不计行为失败。

保护核对：`NTSD_Battle.unity` SHA-256 `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`；`NTSD_Menu.unity` SHA-256 `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`，均与本包前相同。模式 Asset 为本包声明内变更，当前 SHA-256 `0AD22411DC42C899094A69F3D13BE9503E2524F6286B636E02C9DF95ABCD7061`。未启动第二 Unity 项目或 Editor；没有使用 computer-use。

`git diff --check` 对本包代码/Asset 路径 0 退出；`Tools/Validate-ChangeLedger.ps1` 经 `pwsh` 且显式传入本仓库根目录后 PASS（共享工作树 30 个受治理 code diff）。首次用 Windows PowerShell 且不传根目录时，脚本默认参数在 `$PSScriptRoot` 上报空路径；这不是代码验证失败，实际通过的命令和限定范围见 Change Record。

真实原 Battle Scene：独立请求文件探针在 Runtime `Running`、tick2 捕获 `observedGate=1`、项目 Asset `projectGate=1`、activeSlotCount=2、正式内容目录已就绪，写出 `PASS_CAPTURED` 并请求退出。随后原 Editor MCP 状态实见 idle/non-Play、Battle Scene 仍为活动场景，Battle/Menu SHA 均与本包前一致。归档 `project-stage-gate-original-battle-play.json` SHA-256 `3EE62E2F1C1D50359032DA6B3BD5808FC91B9966FACFC74A50153332434BB080`。此项连同三tick完整Driver正反例关闭**所选项目模式 stage gate 生产边界**，不证明其他mode、全RNG同态或整场战斗。

新增探针后的原 Editor 编译/导入成功，`BattleLockstepChecksumEditorTests.SourceRuleJsonProjectionRetainsExtendedSelfCheckContract` 调用受影响的 `BattleRuntimeSelfCheck.CheckExtendedChecksumContracts`，定向 1/1 PASS（job `fc6e057da3dd4b71bb939839280f6aaf`、原始 JSON `project-stage-gate-extended-selfcheck-job.json`）。最终 Ledger validator PASS，当前共享差异中受治理脚本31个；本包 `git diff --check` 0 退出。未执行整套8583项 EditMode 或所有角色/场景重测。

旧双轮自然KO TestRunner job `20b6cc8dd0cf4094ae7b46c1bbb1c458` 跨域后停在 `running` / 0 completed，原 Editor 空闲后用官方 `clear_stuck` 标为 `failed / Job cleared manually (stuck or orphaned)`，原始末态在 `project-stage-gate-play-job-final.json`；它不是生产行为失败，也不替代上述独立探针。正式根 LFR 的完整 RNG 同态、模式/结果其余 Q08 子门、Q07 有限出口、Q09/Q10/Q12 仍按总表各自验收，不重跑已过角色矩阵。
