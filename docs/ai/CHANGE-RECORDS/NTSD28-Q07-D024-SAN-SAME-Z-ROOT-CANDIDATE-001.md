<!-- CHANGE-RECORD
id: NTSD28-Q07-D024-SAN-SAME-Z-ROOT-CANDIDATE-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q09Diagnostics/han_natural_earthquake_lfr.cpp
authority: formal root EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and playable background ID1 plus user project-map exception
evidence: docs/ai/TASKS/NTSD28-Q07-D024-SAN-SAME-Z-ROOT-CANDIDATE-001.md
-->

# NTSD28-Q07-D024-SAN-SAME-Z-ROOT-CANDIDATE-001

Status: `VERIFIED / SAME_SOURCE_Z_FIRST_CANDIDATE_ONLY`. Exact scope, acceptance and rollback are in the Task.

Before: the existing optional Han/Lee LFR tool supports default formal background23 at source Z650, and `z400-jump3` at formal background23 whose stage lower bound542 clamps source Z400 at tick1. Unity's in-map Z400 near/far candidate reports are already saved, but not same absolute stage state with that background. Formal background ID1's Z375..575 makes the same Z400 legal in the formal root and the project's own map after the user-approved projection.

Plan: add only an opt-in background1/Z400 selector with unique output names; existing cases unchanged. The released playable source determines rules, native BG1 data is formal-only diagnostic input. Expected production side effect: none. Risk: BG1's selected mode/stage effects may change the Han action chain; retain any RED and compare first candidate rather than adjusting formal data or Unity state to force a pass. No script edit yet.

Actual edit (2026-09-29): the single optional diagnostic source now accepts `san-z400-jump3`; it selects only sourceZ400, formal background ID1 and the existing paired X520/X580 jump3 inputs. Its output stems append `-bg1-z400`; existing background23 selectors and stems are unchanged. Summary JSON now includes `backgroundId` for all cases. No Unity production, formal source/EXE, DAT, Scene/map/camera or nonbattle file was edited. Build, formal root replay, output overwrite guard and protected hashes pending.

Verification: g++17/O2 build from 28 formal playable core sources plus four playable Session/LFR sources exited0. Fresh optional mode two cases exited0, first action146 at formal tick10 near1/far0, source Z at tick1 remained400 in both. Both LFRs replayed through formal root EXE SHA `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033` with `passed=true`/failureCode0; eight selected CSV/root fields each400/400. A repeat run refused overwrite with exit4 and unchanged CSV SHA. Original Unity in-map reports were reused, candidate1/0 and first146 source X/Z exact; nine selected cross-end rows match52/54 fields per case, with Lee action at formal ticks4/8 one-tick transient mismatch. Could be target age/registration phase, not yet classified. No Scene/map/asset/script production edit and protected five SHA remain. The prior pending sentence is historical. [Acceptance](../../../artifacts/diagnostics/NTSD28-Q07-D024-SAN-SAME-Z-ROOT-CANDIDATE-001/ACCEPTANCE.md). Scope is only the diagnostic and same-source candidate branch; no Q07 aggregate completion or stage-domain claim.

Correction after read-only trace phase audit: the 52/54 cross-end figure mixed Han's input-response offset with Lee's birth-age clock. Formal `SimulationTickDriver28::step` first advances input phase0→1 and holds physical input until phase0 at tick2; Unity probe directly submits `FrameInputSet` and Han responds at relative1. Recomputed Han action/source X/Z formal tick2–10 vs Unity relative1–9 at 27/27, and no-input Lee action/source X/Z formal tick1–8 vs Unity relative1–8 at 24/24, each in both near/far cases. The two Lee action differences are comparison-phase artifacts, not confirmed production bugs. Same-tick full World and production input entry remain unverified. `git diff --check` and `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot $PWD.Path` returned exit0 after code/evidence update; no additional script edit or Unity run.
