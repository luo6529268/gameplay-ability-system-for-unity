# R8-WP01C-06 — random weapon / late special / effect chain execution

> 日期：2026-08-23  
> 状态：`VERIFIED / UNITY PRODUCTION PLAY S4`  
> Change ID：`R8-LATEPLAY-001`

## Goal

认证 natural random weapon、state9995→4000→8000→9996、state9996 4×217+1×218 的最低空槽、
RNG、字段初始化与exhaustion顺序；只使用当前场景catalog或明确test fixture，不部署默认stage.dat。

## Scope

- Editor-only Play probe；
- live production world：当前sealed runtime catalog的natural random drop，完整预测candidate/RNG/slot/position；
- live production world：通用state9996 character shell调用正式writer，使用当前OID217/218资源生成五个对象；
- Play进程内logic-only fixture：完整9995→4000→8000→9996同调用chain和五子；
- Play进程内authority400 fixture：natural random与state9996在50..399耗尽时分别验证1 RNG/0 RNG和0 spawn；
- 恢复live RNG、sounds、objects/slots/pools/pause。

## Authority / Evidence

- C++只读 `src/entity/game_tick.cpp`：natural random位于character consume与object consume之间；weapon<4才
  消费1/200 gate；先找最低空slot50+，按loaded_oid_order与122/123 gate建候选，再选择、四次坐标RNG并reset；
  late chain按9995、4000、8000三个独立if逐段reload，8000写`unk_318=140`；9996+character+attacking1
  逐轮重新找最低空slot，成功4×217+1×218，完整5子34 RNG；
- Unity `R7-LATE-001`曾修复chain/writer，但其旧HitStun140证据已由`R8-SPRITEMAP-001`纠正为
  `RenderPicOffset=140`；本包只按纠正后的production验收；
- GT-03/GT-11已有self-check；缺当前Play进程和live pool/catalog联合证据。

## Required matrix

1. natural random positive：candidate source order、122/123 gate、selected OID、lowest slot、四坐标RNG、
   y=-500、zero velocity、HP fields、总calls；
2. live state9996：五个child的OID、slot、generation、spawner、position、velocity、frame/facing、
   AttackExempt与reset relation defaults；34 RNG；
3. synthetic full chain：9995→OID50(state4900)→OID900(state8901)→OID901(state9996)，frame0 reload、
   render offset140、attacking preserved并同调用生成五子；
4. exhaustion：authority slots50..399满时natural gate只耗1 call且不建候选；state9996耗0 call/0 child；
5. cleanup与first-difference-friendly报告。

## Verification

- fresh compile 0 error；GT-03/GT-11相关focused/self-check；
- clean Play required matrix、Console0 error、live baseline恢复；
- full self-check、ledger validator、diff check。

## Stop conditions

- live candidate/RNG/slot/field顺序出现production first-difference；
- 当前OID217/218或随机武器资源缺失且需要改DAT/scene/default stage.dat；
- 需要修改production gameplay、allocator/pool、RNG、pass order、render或approved adapter；
- 需要运行/构建/修改/写入C++ authority。

## Out of scope

修复首差、D-RENDER-006剩余authored state8000视觉样本、1000实体、Player、T8、Android、服务器、C++ trace。

## Authorization

用户已连续授权执行WP01C-05→06→07，无需逐包批准。

## Result

- fresh all-scope compile0；focused late-tail 14/14；
- final clean Play在worker active下PASS：natural candidates按C++范围/source order为
  `100,101,120,121,122,150,151,123,124`，选OID122、slot50、position(1314,-500,509)、8 RNG；
- live state9996生成slot50～54的4×217+1×218、34 RNG，所有position/velocity/frame/facing/
  generation/spawner/reset字段匹配；
- logic-only完整chain到OID901/state9996/RenderPicOffset140并同调用生成5 child；
- authority400动态槽350个全满：natural 1 RNG、late 0 RNG、0 spawn；
- objects4→4、claimed2→2、pools2→2，RNG/sounds/pause恢复，Console0；
- 14:08:15 full self-check与71 records/70 code files validator PASS；production0改动。

`B-R8-WP01C-06-TEARDOWN-01`：final probe在退出Play后，Unity报告
`LF2ObjectPointFactory_AutoCreated`与`LF2ObjectPool_AutoCreated`未清理；probe结束前active objects/slots/
pools均已恢复且Console0。随后“不运行probe”的clean Play→Stop为0 error，证明该warning来自probe新增并回收的
inactive renderer在Editor场景销毁顺序中触发singleton再创建。它不改变本包tick/字段S4结论，但post-stop
Editor teardown不得写成零告警；不允许为认证反射清理pool队列或修改production。

报告：`Temp/NTSD_R8_WP01C_06_RandomWeaponLateEffect.result.json`；persistent evidence：
`RESEARCH/R8-WP01C-06-random-weapon-late-effect-runtime-evidence-20260823.md`。
本Task只关闭06 Unity S4；C++ full trace继续BLOCKED。
