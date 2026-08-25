# R8-WP01C-06 random weapon / late effect runtime evidence — 2026-08-23

## Verdict

`PASS / VERIFIED`，仅限Unity production Play S4。C++ release authority只读source已闭合；R1-WP02 full
C++ trace继续BLOCKED。

## Final Play matrix

### Natural random weapon — current sealed catalog

- weapon count before：0；lowest free slot：50；
- C++ range/source-order candidates：`100,101,120,121,122,150,151,123,124`；
- selected：index4 / OID122；
- actual：slot50、frame0、position `(1314,-500,509)`、velocity0、HP200/HPBound500/HP3 500/PP500；
- RNG calls：8；candidate special gates、selection和四坐标calls与同一LCG预测一致。

### Live state9996 — current OID217/218 resources

- spawner slot2；children slots50～54；OID=`217,217,217,217,218`；
- RNG calls=34；每子generation非0、SpawnerEntityIndex=2、AttackExempt6、team/relation0、holder99；
- position/velocity/frame/facing逐字段与C++顺序预测一致；final worker active。

### Explicit full-chain fixture

- source state9995 → OID50 state4900 → OID900 state8901 → OID901 state9996；
- final frame0、RenderPicOffset140、attacking1；同调用生成5 child、34 RNG；
- logic-only isolated world，不修改live catalog或scene。

### Exhaustion fixture

- Authority400 slots50..399共350动态槽全部占用；
- natural random：gate hit后1 RNG、0 spawn；
- state9996：0 RNG、0 spawn；
- 证明free-slot检查先于candidate/field RNG。

## Verification and cleanup

- final Editor DLL 14:06:36，C# error0；
- focused job `faa6cee5653347b69fd31410832e1fcb`：14/14；
- clean Play报告：`Temp/NTSD_R8_WP01C_06_RandomWeaponLateEffect.result.json`；
- baseline/final objects4→4、claimed2→2、render pool2→2、logic pool2→2；RNG/sounds/pause恢复；Console0；
- full self-check 2026-08-23 14:08:15 PASS；validator 71 records/70 code files PASS；
- production gameplay/allocator/pool/RNG/DAT/scene/render/C++ 0改动。

Final probe退出Play后出现`LF2ObjectPointFactory_AutoCreated`/`LF2ObjectPool_AutoCreated`未清理warning；
probe结束前runtime基线已恢复且Console0。随后无probe clean Play→Stop为0 error，故登记
`B-R8-WP01C-06-TEARDOWN-01`为Editor-only probe teardown hygiene。它不否定tick/field matrix，但不能把
post-stop Console写成clean；未通过反射或production修改清理pool内部队列。

首次tick0依赖未ready和第二次probe authority-range预测缺失均为probe-only失败，均完整cleanup并已留痕；
final运行未发现production first-difference。

## Boundaries

- D-RENDER-006 authored state8000真实资源样本仍独立，不由此logic fixture关闭；
- C++ executable trace、T8默认stage.dat、Android、1000实体和Player不在本包。
