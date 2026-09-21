<!-- CHANGE-RECORD
id: NTSD28-Q06-CPOINT-INPUT-ACTION-SELECTION-001
status: VERIFIED
change-kind: CPOINT_INPUT_SELECTION_SOURCE_WITNESS_FIRST
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCpointWriter.cs
code-path: Tools/NTSD28AuthorityTrace/cpoint_input_action_selection_witness.cpp
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06CpointInputSelectionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06CpointSelectionPlayProbe.cs
authority: Current formal playable battle_world.cpp native relation action and caught-object input selection; default formal EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033.
evidence: Root read-only caller audit confirms live RunKind1/RunActionSelection and missing selectors despite Q05 data fields; no runtime first-difference claim yet.
-->

# NTSD28-Q06-CPOINT-INPUT-ACTION-SELECTION-001

PLANNED / SOURCE_WITNESS_FIRST.

当前正式BattleWorld28::advance_catch_relations()（5751起）中的attack/throw/defend/previous-depth/facing-dependent front/back/jump选择链，约battle_world.cpp5877..5945；native_relation_action处理负动作翻向；最后非0才写catcher action、按selected native cpoint读victim action并清双方counter。当前Unity BattleCpointWriter.RunKind1 -> RunActionSelection只处理A/T/J且逐项ApplyAction，ApplySignedCpointFrame使用旧SetFrameTickDirect。BattleCatchPointValue已有Daction/Faction/Baction/Uzaction/Dzaction合同，不新增字段或schema。

第一写域仅Tools/NTSD28AuthorityTrace/cpoint_input_action_selection_witness.cpp（新source诊断runner）。确认当前formal EXE/source closure，使用未修改playable源码。覆盖current vs previous输入、edge窗口、左右朝向、8选择入口、同时满足的最终优先级、后续0覆盖取消、正负低/高/隐式/declared999动作和selected victim action、初始及更新CPoint差异。完整before/立即/following raw/B2/counter/latch/snapshot/link/RNG，先独立source模型再Unity RED。

源场景使用throwvx0隔离选择事务，保留其他flags/selector组合；不重做已验CpointThrow392、kind2validation212或已关闭字段退休。捕获same initial relation并保留失败/invalid行，不用Unity定义源规则。当前Unity生产不改；具体测试/生产符号在源证据后另准确追加Record。

验收：源build+双跑+独立检查、Unity两profile完整初态匹配和立即/后继tick、局部replay及真实Play/关闭/新SelfCheck；只达到各层才推进状态。风险：选择顺序、旧输入映射、0取消语义、selected descriptor/vaction与cached initial CPoint后续消费。保持Unity/GAS/33ms/有序关闭和非战斗/资源/Scene边界。回滚仅人工撤销同ID准确diff，保留用户和其它Task修改及失败证据，不自动删除。

恢复确认：源诊断370双跑一致SHA9ed8699d6af26c12f1c6e6c3776955a0aabc58c90e41048c7999efcbbe63adde；根独立selector/catcher action-facing-counter-latch-snapshot2590检查0失败，仅部分oracle，非全事务/Unity验证。formal330 primarykind1=1709且八selector非零值0（原始405字段另查同为0）。本项保持IN_PROGRESS / SOURCE_CAPTURE_ONLY，生产优先级让位kind8；不标整个selector对齐，也不从静态零域取消总目标要求。

Priority return after platform fulltick3 PASS: source370 double bytes/hash and manifest rechecked; current RunActionSelection still A/T/J sequential. See artifacts/diagnostics/NTSD28-Q06-CPOINT-INPUT-ACTION-SELECTION-001/RETURN-AUDIT.md. No new script edit/test in this audit. Next exact Unity fixture declaration; source partial oracle is not full transaction proof.

Pre-change Unity RED declaration: new NTSD28Q06CpointInputSelectionEditorTests.cs only. SelectionPassMatchesSource(profile), MakeWorld/RestoreBefore/Compare/Definition helpers. Pinned370 source rows, both existing profiles, actual RunCpointAdvanceStep10 slot loop, canonical proxy explicit key mapping; initial raw47 plus descriptors/catch relation verified before reporting post differences. Use established test-only raw comparator and factory/shutdown. Immediate projection first; following/native input/RNG/replay remain explicit pending, not certified by immediate pass. No production edit. Independent world no new runtime owner; preserve resources/Scene/schema and old tests.

Initial test job e46360d6 two profiles failed after raw initial projection matched. Fixture had initialized canonical input but omitted production ProjectExactStateToLegacy alias publication; therefore these initial differences are not clean production RED. Archive initial-proxy-only-e46360d6. Before fixing test, declare MakeWorld call existing NTSD28NativeComboStateMachine.ProjectExactStateToLegacy after samples; no production edits/expected changes.

Production pre-change declaration after corrected RED cc8e4b0b: BattleCpointWriter.cs only RunActionSelection and ApplyAction. Both profiles370 initial raw+descriptor projections zero differences; actual pass differs in legacy implicit descriptor and missing ordered selectors. Select once in native order A/T/D/Uz/Dz/F/B/J using canonical proxy current/previous/edge, with matching zero overwrites. Apply final nonzero signed action once (negative flips only once), use existing SetCpointRawFramePreserveWait native binding then selected native primary CPoint vaction or0; victim raw write and both counters0. Do not change global ApplySignedCpointFrame/public setters, caller snapshot/cached original CPoint, throw/dircontrol/settlement, pass order, input producer or schema. No new owner; rollback only declared delta with user authorization. Focused370x2 then affected old throw regression; following/replay/Play remain required.
Pre-edit input seam refinement: consume existing published runtime aliases (KeyJump=attack, KeyDefend=jump, KeyAttack=defend; Prev directions and Cd windows) produced by ProjectExactStateToLegacy. This preserves both existing input entry profiles and does not require changing input ownership; canonical proxy values and aliases are identical at the declared pass boundary.

Production two-method implementation and corrected fixture immediate validation complete: jobac32c386 actual4/4PASS,370x2 selection before/after zero diff plus existing throw profilesPASS. IMMEDIATE-RESULT.md documents scope/RED/fixture correction. Following/full input/RNG/replay/Play/SelfCheck still pending; notVERIFIED.
Pre-change test extension: existing NTSD28Q06CpointInputSelectionEditorTests.cs only. Refactor shared RunSelectionMatrix(profile, following); add FollowingSelectionMatchesSource, CompareState and recursive CompareJson; existing projection helpers via reflection for full B2/RNG, relation link/parent/child plus lifecycle presence. Restore explicit source input history/remap/run fields and relation initial state; reset FunctionKeys and set source stage bounds. Compare before/immediate/full following tick; no production changes or replay claim yet. Null entities handled as source lifetime. Source output unchanged.

Expanded test actual jobc4d57852 two profiles failed following tick, each370 before0/after1194 (input886/other308); immediate evidence preserved. Source nonAI sample_pending vs Unity active-roster/controller sampling gate requires equivalent-input-boundary audit before any producer changes. FOLLOWING-FIRST-DIFFERENCE.md records exact path and next step; no further production edit. NotVERIFIED.
Pre-change fixture binding correction: same declared test file adds EmptyController and BindHumanInput helper; actual LF2Character Controller/SimInputBuffer and Runtime.Roster slots bind both factory entities, InputState.SyncFromRuntime publishes authored initial held state before any tick. No test callback injecting during pass; existing PostCooldownHumanInputAll/NTSDInputStateModule performs sample roll. Equivalent source controls native_ai=false and pending=current. Keep unbound failed evidence and source expectations; test whether first difference disappears without production input changes.

Actual human roster/controller fixture job0fccc80a two profile tests370 each allPASS including full B2/RNG/relations and following raw47. This resolves unbound synthetic input mismatch without production input edits; archived following-pass-0fccc80a. Pre-change next same test adds SelectionSnapshotReplayRepresentative(index) for source rows1/4/247/260/331/369: capture before CPoint, actual slot loop+complete tick, restore original snapshot then repeat, compare source states and per-stage fullruntimechecksums. Existing controller/roster restored through formal snapshot contract, no manual after-restore repair permitted. No new owner/production edit.

Following job0fccc80a370x2PASS after actual roster/controller fixture binding; prior unbound sampling failure resolved without production input changes. Snapshot replayjob49f1368c6/6PASS with fullstate/source projections and immediate/following checksums, no manual after-restore repair. Evidence FOLLOWING-REPLAY-RESULT.md. Next stable SelfCheck/realRenderer/ordered closure, notVERIFIED.

Pre-change realRenderer acceptance: existing test MakeWorld optional renderer flag routes actual LF2ObjectPointFactory.MaterializeObjectForStructuralWriter and asserts Renderer; helper Shutdown frees active renderer fixtures before World shutdown, including failure construction path. VerifyRendererForPlay(index) compares before/immediate/following for source247/331 through actual bound human input. New NTSD28Q06CpointSelectionPlayProbe.cs uses existing request mechanism/paused real scene, two isolated representative Worlds, preserves scenechecksum and borrower count, triggers existing Q05 replay/ordered shutdown. No new production owner or Scene/assets, no physical key/visual parity claim. One stable SelfCheck after compile.

Scoped acceptance VERIFIED: SelfCheck12:57:55Z PASS; real pooledRenderer two representativesPASS, scenechecksum/borrowers2->2; Q05 restore4->4 and World/slots/pools0/Stopped2framesPASS; CS0/Scenehash unchanged/Ledger632-19PASS. ACCEPTANCE.md defines source/field/content/visual boundaries and retained failed evidence. Next Q06 state13/action200 live-source audit, Q07 not migrated.
