# HANDOFF — R8-WP01G-R08 OID7/8→51 merge / dormant / split central runtime

> 日期：2026-08-23  
> 状态：`VERIFIED / UNITY S4 / B-R8-R08-03 CLOSED`

## Preflight result

- `LF2States.Running == 2`，C++ merge的self state2要求已经闭合；
- `data.txt`正式包含OID7、8、51；可用low slot0..19和production factory构造真实前置；
- merge结果必须由完整tick的OID maintenance产生，禁止直接写OID51/dormant/metadata或调用private helper；
- split优先走正式DJA；若当前DAT不可达，就完整推进4500 fixed ticks，禁止直接写Unk338=0；
- Unity保留dormant partner原slot/generation是已批准adapter，本包只验证真实Play中无占槽/ghost并正确恢复。

## Next action after approval

先建立test-only Change Record，查询loaded OID7/8/51 authored running/DJA frames，再决定复用现有probe还是
新增一个Editor-only联合Play probe。发现production first difference时停止并另建repair包。

## Execution start

- 用户已于2026-08-23明确批准R08并恢复总目标；
- C++ `game_tick.cpp:1008-1154`与Unity maintenance再次只读复核，未发现执行前必须修改production的差异；
- 正式`data.txt`确认OID51=`sasori.dat`、OID7=`rock_lee.dat`、OID8=`chiyo.dat`；
- 已在脚本写入前建立`R8-MERGESPLIT-001 / IN_PROGRESS / TEST-ONLY`；
- 下一步只读检查loaded/authored running与DJA，再实现probe；production gameplay默认0改动。

## Blocker

- `Assets/NTSD/Config/data.txt`声明OID7/8/51，但`Config/chars`缺少`rock_lee.dat`、`chiyo.dat`、
  `sasori.dat`；loader会跳过对应wrapper；
- 相邻`I:\GitHub\Unity_GAS\ntsd_proto\ntsd_assets\chars`存在同名加密DAT，但它不是已确认的当前Unity
  适配资产来源，且Task禁止新增/修改DAT，未复制或解密；后续在实际运行根目录
  `J:\QQFile\NTSD2.4\chars`找到正式runtime DAT，三份长度/hash均与`ntsd_proto`不同，证明不能混用；
- `B-R8-R08-01`触发Task stop condition。probe未创建、专项Play未运行、production与C++均0改动；
- 恢复条件：用户恢复三份Unity DAT，或明确确认合法Unity资产来源和允许的部署方式。

## Blocker closure / resume point（2026-08-24）

- 用户已指定并批准资源源：`J:\QQFile\NTSD 2.4.1\chars`与`J:\QQFile\NTSD 2.4.1\sprite`；
- 已恢复37份缺失type0 DAT和182个去重BMP；`data.txt`的42项type0均指向Character DAT catalog；DAT内227条
  `head/small/file(...)` BMP引用均为存在的Unity Character sprite路径；
- Unity全资源导入后compiler error=0；针对真实`data.txt`/decryptor/parser的EditMode job
  `34a8a483ff314b82b65e9df5f4aaaf0e`为1/1 PASS；正常`NTSD_Battle` Play 20秒Console error/warning=0；
- `B-R8-R08-01`关闭。`R8-MERGESPLIT-001`此前未写probe或production代码；恢复R08时从“查询OID7/8/51 authored
  running/DJA frame可达性”开始，并继续遵守禁止直接写merge/dormant/split结果的原Task Contract；
- 本次仍未运行R08 merge/dormant/split，未改C++、Unity gameplay、T8、AI、IL2CPP、Android或服务器。

## Protected boundaries

C++只读；不改DAT；不直接写结果；不回退CentralOnly/extended capacity/pool/worker/SoA/ECS/0GC；固定
30Hz、FrameInputSet、1.5×scale和fixed camera保持。

## Execution resumed（2026-08-24）

- 用户明确恢复目标；`R8-MERGESPLIT-001`由BLOCKED恢复为IN_PROGRESS；
- OID7/8 authored state2可达；OID51 frame290真实存在；C++/Unity DJA的`Unk328==1` cooldown-release分支闭合；
- 下一步新增登记中的Editor-only probe，使用正式production catalog/factory与完整tick产生merge、DJA release、split，
  不直接写任何预期结果字段；production first difference出现时立即停止并拆repair Change。

## New production first difference / stop（2026-08-24）

- R08 probe已写、probe-only compile错误已修、fresh compiler error=0；
- 正式Play在fixture前被OID56重叠file range导致的catalog duplicate key(56,112)异常阻塞；
- C++为first-declared-range-wins；Unity当前结果可能受异步sheet完成顺序影响并最终抛duplicate；
- 已按正式DAT解密合同只读审计`data.txt`全部137个对象：137/137成功、347条range、仅OID56的112-120重叠；
- 已建立`R8-WP01G-R08-R01`与`R8-SPRITERANGE-001 / PLANNED / APPROVAL PENDING`；
- 当前停止。未运行merge/dormant/DJA/split；不得在R08 test-only probe中修production或修改DAT绕过。

## R08-R01 repair / resumed execution（2026-08-24）

- `R8-SPRITERANGE-001`已获批并写入通用first-declared ownership；fresh compile0、focused2/2、atlas/catalog29/29、
  normal Play 25秒Console0，旧OID56 duplicate blocker已解除；
- R08实际完整tick已证明OID7→51/frame290、HP150/bound190/PP500/metadata与OID8 dormant；cleanup完整恢复；
- probe先修复跨Editor poller的pause竞争，再纠正Unity logic+shell ObjectCount口径：现在要求post-fixture count精确减1；
- 该最新版已fresh compile0。当前唯一执行阻塞是existing Unity Editor处于高CPU Play，ping可用但主线程工具命令
  30秒timeout；未强杀/未启动第二Editor。恢复点：用户或Editor正常退出Play后，直接重跑R08，不重做资源/代码定位。

## B-R8-R08-03 production split blocker（2026-08-24）

- 用户退出Play后R08已恢复并多轮重跑；probe-only纠正均有Change Record追加留痕；
- merge结果、post-fixture ObjectCount精确-1、merged Central command与dormant suppression已实际通过；
- OID51 frame290正式`hit_ja=0`，C++与self-check都禁止missing-target fallthrough，因此按合同完成4500 tick fallback；
- final maintenance在`TrySplitOid51BackToPair → partner.Reset → RelationTeam setter → relation/link store → unified row`
  抛`stale slot generation after commit`；异常中断split与cleanup；
- 已退出Play；未修改C++、DAT或production repair代码；
- 新建`R8-WP01G-R08-R02-dormant-split-ai-row-generation.md`与`R8-AIROWGEN-001 / PLANNED /
  APPROVAL PENDING`。下一步只有获得用户批准后，先写focused reproduction，再做最小lifecycle修复并重跑R08。

### R02只读预检恢复点

- current slot generation与relation/frame/vital/input store binding都仍有效；stale字样实际表示partner不在current
  Included row，并非generation已递增；
- publisher在CharacterInput激活后持续到maintenance，现有occupancy invalidation可安全EndPass并令next tick full rebuild；
- 推荐新增通用row-membership invalidation语义，并在merge进入dormant前、split `partner.Reset()`前各调用；
- focused必须覆盖：merge next-tick不包含dormant、split不抛异常/handle不变、再下一tickfull rebuild重新包含partner；
- production code仍0新增改动；等待用户批准R02后才写focused/production。

## R02 approved / execution resumed（2026-08-24）

- 用户明确批准`R8-WP01G-R08-R02 / R8-AIROWGEN-001`并恢复目标；
- Change现为`IN_PROGRESS / PRE-CODE`；
- 执行顺序固定为focused旧实现复现→通用row-membership invalidation→compile/focused/self-check→R08 4500 tick；
- 不得改generation、allocator、AI策略、ValidateRow或用OID专项静默绕过。

## R02 production exception closed / R08 probe pending（2026-08-24）

- `R8-AIROWGEN-001`生产修复已通过merge/split focused 2/2、unified authority 21/21和live-slot/0-GC 37/37；
- fresh compile为0 error；full self-check在更早的独立`R-HC-01` geometry审计处BLOCKED，未到OID5152；
- 修复后R08 Play已完成4500 tick，旧`stale slot generation after commit`异常消失，merge/Central/dormant前半段继续通过；
- 当前失败是`R8-MERGESPLIT-001`探针把OID恢复、dormant恢复与全局ObjectCount旧绝对基线合并断言，失败前没有保存
  split分项状态。恢复点：先做test-only诊断采样和断言拆分，再重跑；在得到分项证据前禁止继续改production。
- 分项证据现已获得：self/partner恢复7/8、dormant=false、slot/generation保持，final tick `ObjectCount 14→15`、
  claimed `8→8`且无spawn/register；旧fixture绝对值6不适合作为4500 tick后的全局断言。pre-split当前HP/HPBound
  为190/190，C++要求当前值各半，实测双方95/95；frame112的正式DAT为wait0/next113，完整tick末双方113/state8。
  恢复点：只修probe为局部+1/claimed不变、动态half health与tick末113，然后重跑Central split/cleanup。
- 该轮重跑已得到`mergeSplitPassed=true`与`splitBodiesSubmitted=true`；唯一失败是probe cleanup没有清除4500 tick中
  新生成的非基线对象。下一步在同一test-only Change中记录baseline handles，并只回收post-baseline handles，之后
  继续严格检查world/claimed/object pool/logic pool/RNG/sounds全部回到基线；禁止清理运行前实体或改production生成链。

## Final R08 closure（2026-08-24）

- generation-safe baseline cleanup已实现为test-only：只释放当前handle集合中不属于运行前baseline的实体；
- fresh compile0；最终R08 result于`01:48:32Z`写入PASS；4500 tick、merge/dormant/split、原slot/generation、
  current-half HP/HPBound、tick末frame113/state8、Central merged/dormant/split均通过；
- cleanup释放5个post-baseline handles，最终world/claimed/object pool/logic pool=`2/1/1/1`、RNG恢复、无cleanup error；
- `R8-AIROWGEN-001`和`R8-MERGESPLIT-001`均VERIFIED，`B-R8-R08-03`关闭；
- full self-check仍在更早的独立`R-HC-01`处BLOCKED，Position38既有AI fixture与R1-WP02 full trace仍独立；退出Play
  产生1条Unity scene-close warning，未宣称warning0。后续不得把本R08局部闭合夸大为C++ executable full-trace认证。
- final `git diff --check`无错误；Change Ledger validator PASS（91 records / 111 governed files）。
