# R8-WP01C-07 — production combat object certification synthesis

> 日期：2026-08-23  
> 状态：`COMPLETE`  
> 范围：仅R8-WP01C-01～06的Unity production Play S4证据汇总

## Verdict

WP01C-01～06均为`PASS / VERIFIED（Unity S4限定范围）`；07 synthesis完成。没有任何子包发现最终成立的
production first-difference，也没有为认证修改production gameplay。这个结论不等于整个战斗系统、所有DAT
可达分支或C++ runtime full-trace已完整对齐。

## Package matrix

| WP | 结果 | 主要producer→consumer证据 | Change / evidence |
|---|---|---|---|
| 01 opoint/lifecycle | PASS | character/weapon/special/other birth、Prev2、same/next-pass、generation reuse | `R8-OPLIFE-001` / `R8-WP01C-01-opoint-lifecycle-runtime-evidence-20260823.md` |
| 02 held/throw/landing | PASS | type1/2/4/6 pickup→held wpoint→throw→landing，no-immediate-hit | `R8-HOLDPLAY-001` / `R8-WP01C-02-pickup-held-throw-landing-runtime-evidence-20260823.md` |
| 03 grab/CPoint/link | PASS | valid grab/injury/stat、mismatch throw、escape、positive/negative residue | `R8-GRABPLAY-001` / `R8-WP01C-03-grab-cpoint-link-held-injury-runtime-evidence-20260823.md` |
| 04 collision/hit | PASS | 10 frozen candidates、三类positive、HitConfirm2/caught/effect21、raw frame | `R8-HITPLAY-001` / `R8-WP01C-04-collision-hit-damage-runtime-evidence-20260823.md` |
| 05 death/respawn | PASS | HP=0 AI、state14 30→4、no-count/stored/free、integer/RNG、OID998 | `R8-DEATHPLAY-001` / `R8-WP01C-05-death-respawn-ai-integer-runtime-evidence-20260823.md` |
| 06 random/late/effect | PASS | natural random、live五子、full chain、authority400 exhaustion | `R8-LATEPLAY-001` / `R8-WP01C-06-random-weapon-late-effect-runtime-evidence-20260823.md` |

## Highest evidence by domain

- `D-OP-001`、`D-HOLD-001～003`、`D-CPT-001～005`、`D-LINK-001～002`：对应实际Play子集已到S4；
- `D-COL-001～003`、`D-COL-004B`、`D-COL-005A`与`D-HIT-001～004`的明确matrix子集：S4；
- `D-INP-002`、`D-MOV-003`、`D-LATE-001`：S4；
- 所有S4均有fresh compile、focused/相关回归、clean Play cleanup、full self-check和ledger证据；
- S5 C++ executable full trace：`BLOCKED / R1-WP02`。

## Explicitly not closed

1. `D-COL-004` oid999正式有效geometry的真实production可达Play仍pending；
2. `D-COL-005B` non-character kind1正式DAT producer可达性仍UNKNOWN；
3. `D-HIT-002/003/004`中未进入本轮live matrix的kind11/16、其余weapon type分支只到focused/self-check；
4. `D-HIT-005` CLR shell/current-DAT dispatch可达性仍UNKNOWN；
5. `D-LIFE-001` oid7/8→51 dormant partner真实Play仍pending；
6. WP05只联合证明明确HP=0 AI input/respawn边界，不扩大为所有target-bearing AI policy；
7. 中央像素、阴影、透明排序、挂点和authored state8000视觉样本归WP01D；
8. 1000实体/0 GC归WP01E，Player/IL2CPP归WP01F，R8全量汇总归WP01G；
9. T8默认stage.dat、Android、服务器均保持排除/暂缓。

## Failure history discipline

各包曾出现的菜单编码、报告取样、数组槽、mode切换、跨group静态污染、依赖ready、probe API和authority
candidate预测问题均已保留在各Change Record。它们最终都被证明是probe/test问题并通过独立复跑，未被包装成
production修复，也没有删除失败事实。

`B-R8-WP01C-06-TEARDOWN-01`仍明确保留：WP06 probe结束前runtime cleanup与Console通过，但退出Play后
新增回收renderer触发两个AutoCreated manager未清理warning；无probe对照不复现。它不属于战斗tick字段首差，
但说明该probe的post-stop Editor teardown不是零告警。

## Final verification snapshot

- latest fresh compile：Editor DLL 14:06:36，C# error0；
- latest focused：late-tail 14/14；此前各包focused详见各evidence；
- latest clean Play：WP06 14:07:23 PASS，worker active，runtime cleanup complete；post-stop warning见上；
- latest full self-check：2026-08-23 14:08:15 PASS；
- latest ledger validator：71 records / 70 governed code files PASS；
- production gameplay modifications from WP01C certification probes：0。

## Next boundary

WP01C已完成，不自动说明R8完成。父任务下一顺序由`R8-WP01D/E/F/G`及未关闭D-ID决定；本synthesis不
授权修复它们。
