# R8-WP01C-02 — pickup / held / throw / landing runtime evidence

> 日期：2026-08-23
> 结论：`Unity S4 PASS / S5 BLOCKED`
> Change ID：`R8-HOLDPLAY-001`

## 1. Evidence boundary

- 场景：`Assets/NTSD/Scene/NTSD_Battle.unity`；
- Unity：国际版 2022.3.62f3；production `SimulationTickDriver` / `SimulationWorld`；
- 表现后端保持现有 CentralOnly 边界，probe不裁决像素、图片、UV、排序或挂点；
- C++ authority只读，source合同来自`collision.cpp:996-1081`、`game_tick.cpp:1527-1640,1924-2006`、
  `physics.cpp:228-320`；没有运行/构建/写入C++；
- R1-WP02 full trace继续BLOCKED，因此本证据只把WP01C-02提升到Unity S4。

## 2. Final Play result

结果文件：`Temp/NTSD_R8_WP01C_02_HeldWeaponLifecycle.result.json`，2026-08-23 09:37:31 PASS。

- start/end tick：1→1；dedicated worker active；
- baseline/final：object 4→4、claimed slot 2→2、render pool active 2→2、logic pool active 2→2；
- cleanup：PASS，无cleanup error；
- live probe slot：holder=2、weapon=50；每类完成后回收并复用相同最低空槽。

| OID / type | pickup | held pose | throw | landing |
|---|---|---|---|---|
| 120 / type1 | frame115，link 1/-1，pickup count1 | delay7，cover0，XYZ=126/-16/201 | frame40，V=12/-4/-3，delay9，spawner=holder2，picker sentinel保留 | input Vy5→frame70，Vx4/Vy0，flight97，attacking0 |
| 150 / type2 | frame116，link 2/-2，pickup count1 | delay7，cover1，XYZ=126/-14/199 | random frame4（合法0..5），V=12/-4/-3，delay9，spawner sentinel8765保留，picker保留 | input Vy5→frame20，Vx4/Vy0，flight96，attacking0 |
| 121 / type4 | frame115，link 4/-4，pickup count1 | delay7，cover0，XYZ=126/-16/201 | frame40，V=12/-4/-3，delay9，spawner=holder2，picker保留 | input Vy12→bounce frame0，Vx5.6/Vy-8.4，flight97，attacking1保留 |
| 122 / type6 | frame115，link 6/-6，pickup count1 | delay7，cover1，XYZ=126/-14/199 | frame40，V=12/-4/-3，delay9，spawner=holder2，picker保留 | input Vy5→frame70，Vx5.6/Vy0，flight97，attacking0 |

所有投掷均清 holder/weapon link 和 holder held slot，同时保留 C++ 分支未清的 holder target、weapon holder/copy。

额外 overlap witness：weapon landing 前后 target HP 333→333、HPBound 444→444、frame0→0，证明
`LF2Weapon.OnLanded` 没有恢复 Unity-only immediate target scan/hit。

## 3. Failed probe attempts retained

1. tick623：误用只支持shared Character-DAT shell的resolver，type1 pickup被探针自身前置拒绝；
   cleanup object9/claimed7/render2/logic7全部恢复。改用真实`LF2CharacterInteractionResolver`和地面态前置。
2. tick0 empty world：type1/type2已过，type4 attacking sentinel失败。只读确认sentinel写在
   `ImmediateFrame(40)`之前被测试初始化清零；landing raw writer没有清零。同时该次world/worker尚未就绪。
   修正为frame初始化后写sentinel，并先等待tick>0、world/claimed非0后才暂停采基线。

两次失败都属于probe设计，不是production gameplay first-difference；均保留在Change Record和STATE。

## 4. Regression evidence

- source 09:36:23 < `Assembly-CSharp-Editor.dll` 09:36:40；Unity Console compile error=0；
- focused EditMode job `36440d545fe64659ae3c73ff1febf03c`：
  `PooledEntityReuseAllocationEditorTests` 23/23 PASS；
- full `BattleRuntimeSelfCheck`：2026-08-23 09:38:54 `PASS`；自检中的两条registration rollback/rest-binding
  Error是既有负向夹具预期日志，随后清空Console后error/warning=0；
- `Validate-ChangeLedger.ps1`：60 Records / 60 governed code files PASS；
- tracked scoped diff check与新probe no-index diff check均PASS。

## 5. Not proven

- C++ executable/full trace或S5；
- 真实玩家手动从地面拾取到具体攻击动作的手感；
- 图片内容、UV/slice、可见挂点/前后层级（`D-RENDER-006 / R8-WP01D`）；
- WP01C-03 grab/CPoint/link及后续04～07；
- T8、Android、1000实体和Player。
