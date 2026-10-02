<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C040-BEE-NATURAL-ENTRY-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/c040_bee_natural_attack_probe.cpp
authority: 336B44 playable Bee OID75 ordinary attack re-press route to action70/73
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C040-BEE-NATURAL-ENTRY-001.md
-->

# Q07/C040 奇拉比普通输入到护甲攻击动作入口

Created before the new diagnostic script. Current 336B44 C040 triad evidence uses Bee at controlled initial action70; Kakuzu already starts from action0. Bee DAT declares the ordinary punch follow-up `hit_a:66` at frame63 and `hit_a:70` at frame69, then frames70～73 progress by `next`. The input phase and accepted re-press windows have not been measured in the full host.

Pre-change state: no Bee OID75/action0 re-press matrix with same formal content and seed exists in this package. Expected effects: one opt-in C++ diagnostic executable and unique output files, no runtime behavior or resource changes. Existing Unity/Editor/Menu work is protected.

Acceptance: declare a finite two-repress timing matrix, complete GameSession per case, record input phase/action and first 63/66/69/70/73, repeat any positive byte-for-byte and replay matching LFR in root 336B44 if generated. Root PASS is not whole-state parity; compare selected same-tick fields. Avoid misclassifying action70 reached by controlled initial action as natural. Run scoped Ledger and diff checks. Rollback by forward correction of this new diagnostic; no deletion/reset.

Actual code: added only `Tools/NTSD28Q07Diagnostics/c040_bee_natural_attack_probe.cpp`. It creates Bee OID75/action0 and a far OID97 opponent, runs one initial-attack-only control plus 120 bounded attack re-press schedules (second start7～14, third16～30) for 40 complete GameSession ticks each. It records input phase, attack edge, actor action, first 65/63/66/69/70/73, positions/HP and RNG, and emits one representative LFR only if action73 is reached. No action, relation, hold, damage or DAT field is injected. No formal/Unity production, resource, Scene, Menu or noncombat file changed.

Final validation: g++ exit0/empty output; two 121-case source runs each find eight action70/73 positive schedules, and their full tick, RNG, summary and representative LFR SHA values match pairwise. Earliest positive second press7/third16 reaches action65/63/66/69/70/73 at ticks2/8/9/14/16/19; initial-only and late-second-press controls do not reach70/73 in the declared window. Formal root 336B44 EXE replay of representative LFR exits0/PASS, and independently compared 40 ticks x12 selected input/action/entity/RNG fields =480/480, firstDifference=null. Root report says `nativeParityClaim=false`; near-contact three-actor consequences and Unity Scene remain pending. Full raw commands, identity, hashes and limitations are in `artifacts/diagnostics/NTSD28-336B44-Q07-C040-BEE-NATURAL-ENTRY-001/REPORT.md`. Scoped/full Ledger and diff-check outputs are attached separately; rollback remains forward correction of this diagnostic without deletion.
