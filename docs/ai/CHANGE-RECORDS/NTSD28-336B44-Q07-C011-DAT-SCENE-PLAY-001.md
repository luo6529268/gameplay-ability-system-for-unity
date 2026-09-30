<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C011-DAT-SCENE-PLAY-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07TeleportDatBattlePlayProbeEditor.cs
authority: selected formal 336B44 root current-frame phase teleport and unchanged formal Lee/Sakura DAT
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C011-DAT-SCENE-PLAY-001.md
-->

# NTSD28-336B44-Q07-C011-DAT-SCENE-PLAY-001

Created beforecode. Diagnostic only: two unique formal-content scenePlay cases extend existing syntheticphase proof, with allcontrolledsetup insidePlayclone. Root fourcases alreadyPASS/864declaredfields. Ownedpath, invariants, risks, current/previousstate distinction, acceptance and rollback are inTask. No role-specific production behavior.

Actual newcode follows the existing originalEditor request/bootstrap/pause/Driver/exit pattern with a validated caseName, rosterLee/Naruto orSakura/Naruto, controlled teams andsharedseed/phase,12neutralDriver ticks. It recordspost-tail actoraction/state/counter/sourceXYZ/HP andtargetaction/sourceXYZ/HP, plus viewcoordinates andphase. It never directlyteleports. OriginalEditor Assets/Refresh imported/compiled Assembly-CSharp-Editor with Tundra success4.00s; existingwarnings only. Domainreload pending before uniquePlay requests. OriginalScene/runtime proof is not inferred from compilation.

FirstuniqueLee PlayDIFFERENCE/DONE/exit/clean,12samples: Xteleport tick4exactly680, actor/targetZ clamped650→481 fromtick1 by project's ownwalkregion (viewZmax760), and probe incorrectly sampled AnimSub instead of canonical AttackingCounter (+0x088: RunNativeC25FrameTransaction increments it).27declaredfielddifferences only counter/Z, no X/timing/HP/action difference. Preserve JSON/comparison. Correct diagnostic only: sourceZ400/commonwalkinterior, formaldiagnosticbackground1(San z375..575) instead of23; Unityprojectmap unchanged. Capture AttackingCounter. Also fix thisprobe UTCdeadline DateTime.Parse(...).ToUniversalTime and10minute startup allowance, because real originalEditor initialization is6–7minutes and default Parse convertsUTC toLocal. This is testcarrier timing only, not battlecadence. Both affecteddiagnostics are alreadyowned; originalroot background23v2proof retained, newBG1/v3same-state replay required before comparison.

Correctedprobe originalEditorAssets/Refresh compiledsuccess/Tundra9.28s, domainreloadpending. MatchingBG1/Z400 rootsourcev3four12tickcasesexit0/PASS/864fields equal. NoSceneorproduction changes; firstfailedScene12samples remainunchanged. NextuniqueLee02thenSakura01afterfullsceneexit, noautomaticrelaunchonobservationaltimeouts.

Lee02PASS/DONE/exitedPlay/clean:12samples, source/currentroot12declaredfields144/144 equal, correctcounter/Z400insidewalkregion, tick4X680/Z401 andlaterhitmatch. OriginalEditoridle/nonPlay confirmed before uniqueSakura01 request usingalreadycompiledsameprobe. Sakura12tickgate remainsopen; RecordRUNTIME_PENDING despiteLee-scopedpass. Artifacts/diagnostics/NTSD28-336B44-Q07-C011-DAT-SCENE-PLAY-001/REPORT.md recordsallversions/failures.


2026-09-30 C011限定关闭：336B44正式根v3四例12tick/864声明字段PASS；原Battle Scene Lee02与Sakura01各12tick/144字段严格一致，合计288/288、首差0。李tick4 X680/Z401，小樱tick4 X740/Z401；两轮均PASS/DONE/exitedPlay/sceneCleanAfter，MCP确认原Editor idle/nonPlay。两Scene、GameConfig、ProjectBattleModeConfig四SHA保持，DAT与生产脚本未因补证修改。初次Lee01计数字段/地图边界夹具失败和根v1 EOF46原记录保留。该出口证明受控初态后的正式DAT自然帧链与相位，不声称物理键选招、全World/全画面或Q07整组完成。下一G1/C012。

Closure governance checkpoint: Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location).Path exit0/PASS,1056 records/16 governed code files; git -c core.safecrlf=false diff --check exit0. Existing historical record warnings retained. These checks are change tracking/whitespace only, not additional behavior proof.
