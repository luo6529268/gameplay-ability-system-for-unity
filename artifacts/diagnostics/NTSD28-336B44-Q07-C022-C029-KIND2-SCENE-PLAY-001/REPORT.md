# C022/C029 originalScene kind2 natural catch

Status: `VERIFIED_SCOPED`. Latestclosedexit isScene02; initialdiagnosticfailurebelowretained.

Pre-edit firstdifferencecorrection: uniqueScene01 DIFFERENCE/DONE/exitedPlay/clean,32samples/33declaredfielddifferences. Firsttick2 targetaction110/state7 versusformal210/state4; nativeJumpCurrent[5] never1. Existing CharacterInputModule.CaptureHeldSimulationButtons explicitly maps physical defending→SimulationInputButtons.Attack, attacking→Jump, jumping→Defend under crossednativepacket contract. This diagnostic incorrectly usedAttackforjump. Changeonly ownedprobe input flag Attack→Defend; keepnativeJumpindex5 (confirmedformal InputKey28::jump=5) andallformalinitialstate/32tickassertions. Firstcapture9/counter1 andcatchcoordinates alreadymatch; velocitytestunmetbecausefixturedefendedinstead ofjumping. PreserveScene01/33diffs, recompile andrerunoneuniqueScene02 afteridle. No productioninput/framework/DATedit. InitialTaskmappingline isobsolete andsupersededhere, notcurrentAPItruth.

[OriginalScene01](kind2-catch-scene-01.json),[33-field differences](source-unity-comparison.json),[protectedhashes](protected-hashes-after.json). Existing runtimeinput mapping remainsunchanged; initialsame-statefixture waswrong.


2026-09-30 C022/C029限定关闭：336B44根四自然/控制案例各32tick/960声明字段，合计3840/3840、exit0/PASS；原Scene修正输入后的kind2-catch-scene-02 PASS/DONE，32tick/960字段一致、首差0、速度差值实际0。OID52真实跳跃，tick9自然kind3抓取到130/state1700/primarykind2/counter1/Vy-12.899999，后继持有tick保持计数/Vy，挂点正常改位置，motionhold0/interaction0排除其他物理门。原Editoridle/nonPlay、Scene clean、四保护SHA稳定。首轮Scene01探针误用Attack产生防御而非跳跃，33字段差异保留；仅诊断flag按既有CharacterInputModule改Defend，无生产输入/战斗/DAT/Scene变更。当前正式parser20武器860帧无primarykind2：武器正例列条件性当前内容不可用，非全武器或全World证书。两父门限定通过，Q07/总目标仍开。

[Scene02](kind2-catch-scene-02.json),[960-fieldcomparison](source-unity-comparison-scene02.json),[protectedhashes](protected-hashes-after-scene02.json),[compiledprobe](compiled-probe-snapshot-scene02.txt). Tundracompile8.36s/zeroerrors, originalEditorreload/MCPidlebeforeuniquerequest, noentiretestsuite rerun.

最终关闭检查（2026-09-30）：`Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location).Path` 通过，1060 Records / 20 governed code files；完整输出保存于 Scene 证据目录的 ledger-check.txt。`git -c core.safecrlf=false diff --check` 退出 0。仅当前闭合记录与进度文档更新；本轮未改生产战斗脚本、DAT、Scene、配置 Asset 或非战斗逻辑。Q07及总目标仍 ACTIVE，下一出口 C023/C024。
