# R8-WP01C-05 death / respawn / AI / integer runtime evidence — 2026-08-23

## Verdict

`PASS / VERIFIED`，仅限当前Unity production Play S4。C++ authority只读source合同已闭合；R1-WP02
full C++ trace仍BLOCKED，因此本证据不是C++ executable逐tick等价证明。

## Environment

- Scene：`NTSD_Battle`；国际版Unity 2022.3.62f3；
- live driver tick：1；worker本轮inactive；
- probe：`BattleDeathRespawnAiIntegerPlayModeProbeEditor`；
- production gameplay、AI、scheduler、DAT/scene、render与C++：0改动。

## State sequence

| checkpoint | frame/state | HP | HitStun | input | integer position | result |
|---|---:|---:|---:|---|---|---|
| death | 14/14 | 0 | 0 | KeyJump1/Prev0 | (40,0,5) | eligible |
| after AI | 14/14 | 0 | 0 | KeyJump0/Prev1 | unchanged | pre-cleanup input executed |
| arm | 14/14 | 0 | 30 | attacking0 | unchanged | PASS |
| gate ready | 14/14 | 0 | 4 | rolled | unchanged | 26 decrements |
| no-count | 212/4 | 180 | 20 | preserved | (147,-300,39) | HP2Orig3→2、PP500 |
| stored | 219/0 | 80 | 3 | n/a | (77,-12,19) | relation1、delay10、offset140 |
| free | slot6 released | 0 | n/a | n/a | n/a | query null、slot=-1 |

No-count allies的stale integers是(100,40)/(160,20)，live doubles故意改为(1100,1040)/(2160,2020)。
production respawn读取average=(130,30)，两次RNG预测/实际均为(147,39)，call delta=2。No-count分支未额外
改写relation/link/holder/target；stored分支只把relation改为1，其余三字段保持source sentinel。

Stored-count分支使用production factory生成OID998：slot50、frame/action6、(77,-12,20)、relation1、
SpawnerEntityIndex=stored slot。

## Cleanup and verification

- baseline/final：objects4→4、claimed2→2、render pool2→2、logic pool2→2；
- RNG state/call count、pending sounds、driver pause恢复；Play Console error0；
- AI focused job `bfe625591b59498faf28aa29a7a65a86`：85/85；
- W05 exact job `f7323ae3265c40f89786ad26f73580ae`：1/1；isolated class job
  `b87c9db826b244f1965aee87147cf29e`：8/8；
- full self-check：2026-08-23 13:52:04 PASS；ledger validator：70 records / 69 code files PASS。

AI+W05组合运行曾令W05B因跨group静态状态污染失败；随后exact与isolated class均通过，未修改production。
该失败保留为测试隔离事实，不当作C++ gameplay first-difference。

## Boundaries

- WP01C-04已提供lethal damage S4；本包从权威state14/HP<=0 checkpoint继续，不重复裁决damage writer；
- worker-active、target-bearing full AI policy、所有direct-position writers与C++ runtime trace未由本包关闭；
- T8默认stage.dat、Android、render与服务器均不在范围。
