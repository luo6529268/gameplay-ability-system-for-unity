# Q08 revive-lives living-group focused acceptance — 2026-09-22

Status: `FOCUSED_TEST_PASS_ISOLATED / ORIGINAL_EDITOR_AND_REAL_BATTLE_PENDING`. This closes only the HP0/remaining-native-lives eligibility sub-branch, not the Q08 result-flow or full alignment exit.

Authority: formal root EXE SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`; matching playable `GameSession28::step` calls `BattleFlow28::step`, whose `classify` includes a HP0 character when `revive_lives_30c>1`. Fresh source `battle_flow_tests.exe` previously passed its exact remaining-life test. Unity already maps that carrier to `LF2Entity.HP2Orig`.

The project gained one focused Editor fixture and its meta, and `BattleResultsOutcomeHostWriter` now excludes a character only when `HP<=0 && HP2Orig<=1`. No state field/schema, Scene, DAT/image, menu, GAS or nonbattle logic changed under this Change ID. The same test and writer files in the isolated Unity project copy matched the original project by SHA-256 before the post-change run; the copy used a separate Library because the original Editor still had an external-Scene-change modal.

Validation with Unity 2022.3.62f3 EditMode in the isolated copy:

| Evidence | Result | Scope |
|---|---|---|
| [THREE-CASE-RED.xml](THREE-CASE-RED.xml) | 1 pass, 2 intended assertion failures | Old writer: HP0/one-life boundary passes, HP0/two-life direct and full tick fail |
| [THREE-CASE-PASS.xml](THREE-CASE-PASS.xml) | 3/3 pass | New writer: direct, `RunReleaseTick` tick 2, one-life boundary |
| [OUTCOME-SEAM.xml](OUTCOME-SEAM.xml) | 2/2 pass | Existing result writer ownership and active-result gate |
| [SCENE-HOST-SEAM.xml](SCENE-HOST-SEAM.xml) | 3/3 pass | Existing after-world result-page input ownership |

The focused Unity runs compiled and executed, with no compilation errors. The original Editor reload, `BattleRuntimeSelfCheck`, real battle terminal/continue, full source-matched native-driver tick, and Q08's broader group/timer/transition/mode-4 reserve semantics remain pending. The approved result-page visual exception remains. Do not promote this focused evidence to Q08 completion or Q06 reopening.

Final local checks after the script edit: `Tools/Validate-ChangeLedger.ps1` PASS (657 Records, 24 governed code files in diff); `git diff --check` exit 0. The original Battle Scene SHA-256 remains `9E7B8A91ADD396D8A3674915BB5AC9A03B8D1A2EC817BA3B12F135D03EBA0AC0`. No computer-use was used.
