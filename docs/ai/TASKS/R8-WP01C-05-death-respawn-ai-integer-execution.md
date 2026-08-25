# R8-WP01C-05 — death / respawn / integer state / AI boundary execution

> 日期：2026-08-23  
> 状态：`VERIFIED / UNITY PRODUCTION PLAY S4`  
> Change ID：`R8-DEATHPLAY-001`

## Goal

在真实 `NTSD_Battle` Play world 中，以通用 source-derived fixture 联合认证 C++ Release 已静态闭合的
HP<=0 AI input 边界、state14 death hit-stop、no-count/stored-count/free 三条 cleanup/respawn 分支、
integer-coordinate/RNG 与复活后字段状态。

## Scope

- 只新增一个 Editor-only 显式 Play probe；
- 在 driver paused、worker idle 的 live production world 注册自有逻辑实体；
- AI HP=0 时调用既有 production character input 链，证明不会在 cleanup 前整体跳过；
- 使用真实 character frame-tick 把 state14 hit-stop 从0武装到30并递减到 cleanup gate；
- 调用真实 `PostFrameAdvanceDeathCleanupAll`，验证 no-count respawn 从 stale integer snapshots 求平均并消费
  两次 RNG；
- 验证 stored-count 字段、relation reset、frame219/delay/render offset 与 production OID998 effect；
- 验证 HP2Orig<2 的 free/unregister/slot release；
- 输出逐 tick/pass 表并恢复 RNG、实体、slot、pool、pending sound 与 pause 基线。

## Authority / Evidence

- C++只读 source：
  - `src/core/main.cpp` 与 `src/input/input_handler.cpp`：active character-DAT AI 在 death cleanup 前仍进入
    `prepare_ai_input → apply_input`；
  - `src/entity/frame_advance.cpp`：state14+HP<=0 的 hit-stop arm 与 attacking clear；
  - `src/entity/game_tick.cpp:1280-1421`：state9998后执行 respawn gate；no-count、stored-count、free、
    integer average、RNG、frame/HP/PP/relation与OID998字段顺序；
  - release Makefile 包含上述 translation units；authority只读，不运行、构建或写入；
- Unity既有 `R3-AI-LIFE-001`、`R3-SYNC-RESP-001` 和 respawn self-check 已有 source/compile/fixture证据，
  当前缺 live joint S4；
- WP01C-04 的 lethal damage production S4 已通过，本包从权威 state14/HP<=0 death checkpoint继续，
  不重复实现或伪造另一套伤害 writer。

## Required matrix

1. AI HP=0：current key滚入previous并清current，不能被self-HP gate整体跳过；
2. state14 arm：HitStun 0→30、Attacking→0；逐frame-tick递减到4；
3. no-count respawn：HP2Orig 3→2、stale integer average+two RNG、HP/PP/frame212/Y/Vy/HitStun；
4. stored-count respawn：HP overlay、PP、HP/HPBound/HP3、RespawnCount、relation、frame219、delay、
   render offset、OID998/action6/x/y/z+1；
5. free：HP2Orig<2实体从world/slot移除；
6. relation/link/holder/target字段按C++明确writer验证：no-count不额外清理；stored-count只重写relation，
   未被该C++分支写入的link/holder/target保持；free后实体slot不可再查询；
7. 逐边界状态表和完整cleanup恢复。

## Verification

1. fresh Unity compile 0 error；
2. AI input与respawn相关focused suites；
3. clean Play probe required matrix和Console；
4. world object/claimed、object/reference pool、RNG、sounds与pause恢复；
5. full `BattleRuntimeSelfCheck`、ledger validator和diff check。

## Stop conditions

- production input、frame-tick、respawn、RNG、slot或effect出现first-difference；
- OID998需要部署默认stage.dat或修改资源/scene；
- 需要修改 gameplay、AI policy、scheduler、pool、render或approved adapter；
- 需要运行、构建、修改、复制或写入C++ authority。

发现后记录最短复现与独立repair WP，本认证Record不顺手修复。

## Out of scope

- 重复裁决WP04伤害writer；
- random weapon/late special（06）、synthesis（07）；
- render、1000实体、Player、T8默认stage.dat、Android、服务器与C++ full trace。

## Authorization

用户于2026-08-23明确授权连续推进`R8-WP01C-05→06→07`，无需逐包批准。

## Result

- fresh all-scope Unity compile 0 error；
- AI focused 85/85、W05 exact 1/1与isolated 8/8；组合运行曾出现W05B跨组静态状态污染，
  随后独立复核均PASS，未修改production；
- clean Play完成HP=0 AI、state14 0→30→4、no-count/stored/free矩阵；
- stale integer average=(130,30)，两次RNG后预期/实际respawn=(147,39)；
- stored-count生成slot50的OID998/action6，位置(77,-12,20)、relation1、spawner正确；
- objects4→4、claimed2→2、render pool2→2、logic pool2→2，RNG/sounds/pause恢复，Console0 error；
- 2026-08-23 13:52:04 full self-check PASS，validator 70 records/69 governed files PASS；
- production gameplay、AI、scheduler、DAT/scene、render与C++均0改动。

报告：`Temp/NTSD_R8_WP01C_05_DeathRespawnAiInteger.result.json`；persistent evidence：
`RESEARCH/R8-WP01C-05-death-respawn-ai-integer-runtime-evidence-20260823.md`。
本Task只关闭WP01C-05的Unity S4；worker本轮inactive，C++ full trace继续BLOCKED。
