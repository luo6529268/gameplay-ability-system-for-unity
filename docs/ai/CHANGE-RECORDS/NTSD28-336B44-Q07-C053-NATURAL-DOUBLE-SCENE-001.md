<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C053-NATURAL-DOUBLE-SCENE-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C053NaturalDoubleBattlePlayProbeEditor.cs
authority: selected 336B44 playable GameSession natural double OPoint witness
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C053-NATURAL-DOUBLE-SCENE-001.md
-->

# C053 自然双 Uj 原 Battle Scene 探针

脚本前记录。Unity 原场景已经证实两名角色自然单 Uj 的所选 184 字段同态；当前正式源码三名受控初始角色 9/9 位置组合均在 tick7 自然双 applied/effect2/Uj156。原 Unity 受控双攻击者案例手动创建攻击体，尚不能证明两个正式 OPoint producer 在三人原场景中同 tick 双命中。

本包仅增加一个请求式 Editor 测试探针及 `.meta`；准确代码路径见 metadata。前置、字段、范围、风险、四保护 SHA、验收和回滚见 Task。预期副作用仅为 Play 副本中的临时 World 状态和唯一诊断 JSON；不改生产代码、资源、DAT、场景及非战斗功能。首差与失败原件必须保留，不得悄悄修改生产规则。本包不重复全量测试，按三人受控源样本做聚焦验证。

待代码写入后记录实际文件/符号、编译与场景结果、未验证项，并如实更新状态。

2026-10-02 实际文件：新增 metadata 中的测试 Editor `.cs` 和唯一 GUID `9a98f5e4974446ec912a4908470209a4` `.meta`。新探针只扩展既有 C053 请求式 Play 探针为三人 roster、两自然攻击者 owner/rest 的 12 tick 采集；生产职责不变，DAT/Scene/非战斗不动。生成工程 `dotnet build` 0 error/293 warning；原 Editor 强制 refresh 后 DLL 晚于脚本且含新类型，12 tick 原 Scene Play 实际运行。源码/Unity 选定 11 字段×12 tick=132/132 零差、tick7 两 attacker rest10/目标 HP450，结果 `SCOPED_PASS / DONE`；Play 退出、Scene clean、四保护 SHA 稳，原 Editor idle。内部逐 hit/物理键/完整 World 仍待，C053/Q07不关。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-NATURAL-DOUBLE-SCENE-001/REPORT.md)。回滚仅审阅本新增探针/meta/记录，不覆盖已有 dirty 文件；删除须另行留痕并获批准。

交付检查：`Tools/Validate-ChangeLedger.ps1` exit0/PASSED（1118 Records、64 当前差异代码文件），原件见[日志](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-NATURAL-DOUBLE-SCENE-001/ledger-validation.txt)；相关跟踪文档 `git diff --check` exit0。没有执行普通本地 git add/commit/push，也没有删除或覆盖任何已有文件。
