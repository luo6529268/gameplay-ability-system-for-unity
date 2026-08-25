# R8-WP01G-R08 — OID7/8→51 merge / dormant / split central runtime certification

> 建立日期：2026-08-23  
> 状态：`VERIFIED / UNITY S4 / B-R8-R08-03 CLOSED`  
> D-ID：`D-LIFE-001`

## Goal

在真实Unity production world与完整30Hz tick中，以正式OID7、8、51 DAT证明：低槽OID7/8满足C++条件后
由maintenance合体成OID51，partner进入dormant但保留原slot/generation且不产生ghost command/pixel；冷却或
正式DJA释放后，OID51按记录slot恢复原OID pair、formal reset字段、half HP/HPBound、位置、速度、朝向与team。

## Scope

### 允许

1. 只读复核C++ release `game_tick.cpp:1008-1154`、Unity OID maintenance和正式DAT；
2. 使用`data.txt`正式OID7、8、51 wrapper与production factory/pool；
3. 在测试初始边界配置roster slot、team、位置、HP/HPBound和输入控制权；
4. 通过authored running frame或正式physical input形成`state==2`前置；
5. 只调用完整tick/公开maintenance pass，观察它自己写OID51、Unk32C/330/334/338和dormant；
6. 优先通过真实DJA输入走early release；若当前OID51 DAT无有效DJA target，则通过完整固定tick推进4500
   cooldown，不得直接把Unk338写0；
7. 在merge前、merge后、split gate内、split后采集slot/generation/query/ECS/ObjectCount/central command/
   isolated pixel和完整runtime字段；
8. fresh compile、focused OID5152 tests、self-check、Play、Console0、ledger validator。

### 禁止

- 不修改、运行、构建或写入C++ authority；
- 不直接写`OidMergeDormant`、merged OID51 identity、Unk328/32C/330/334、ObjectCount或split结果；
- 不调用`TryMergeOid7Or8Into51`、`TrySplitOid51BackToPair`或反射私有helper；
- 不直接写Unk338=0绕过cooldown/DJA；
- 不新增或修改DAT，不给OID7/8/51增加专项production分支；
- 不释放dormant partner slot或推进其generation；
- 不修改CentralOnly、pool/capacity、pass order、30Hz、FrameInputSet、worker、SoA/ECS或0GC边界；
- 不处理AI、P1/P2通用输入差异、R07A/B/C、T8、IL2CPP、Android或服务器。

## Authority / Evidence

- C++ `src/entity/game_tick.cpp:1008-1154`；
- Unity `SimulationWorld.Passes.partial.cs::Oid5152RuntimeMaintenanceAll`、merge/split helpers；
- Unity `LF2States.Running == 2`；
- `Assets/NTSD/Config/data.txt`正式OID7/8/51；
- existing七组OID5152 full self-check与focused 32/32；
- `R8-LIFE-001` source/adapter closure与R5-LIFE-01B slot/generation/presentation证据。

## Initial fixture contract

允许配置的是“战斗开始前状态”，不是预期结果：

- OID7 self：slot0..9之一；
- OID8 partner：slot10..19之一，避免both-low X tie rule；
- 同RelationTeam、存活、Unk338默认0、HP gate通过、距离`abs(dx)<50`且`abs(dz)<8`；
- self通过authored running frame或正式方向输入进入state2；partner保持可接受的grounded/running状态；
- 不得在初始fixture中写OID51、dormant或merge metadata。

## Acceptance

1. maintenance前OID7/8均active、记录production fixture完成后的Unity ObjectCount、slot/generation有效；
2. maintenance成功后self为OID51/frame290、PP500、merged HP/HPBound与midpoint正确，Unk338=4500；
3. partner由production maintenance写`OidMergeDormant=true`，Unity ObjectCount相对post-fixture精确减1，退出active query/ECS/central
   command/pixel，但原slot/generation/stable id/rest仍保留；
4. opoint/stage/dynamic allocator在dormant期间不占用该low slot；
5. physical DJA若有效，按C++ pass顺序在下一maintenance前解除cooldown；无有效DJA时完整4500 tick推进；
6. split gate frame9..260期间不得过早拆分；离开gate且cooldown0后由maintenance拆分；
7. split pass写OID7/8原slot与frame112、当前HP/HPBound各半、PP0、position/velocity/facing/team及formal reset
   defaults；完整tick末按正式DAT `wait0→next113`观察frame113/state8，并要求局部ObjectCount `+1`、claimed不变；
8. old dormant handle未被错误generation替换，central只显示恢复后的current generation，无ghost/双画；
9. cleanup恢复world/slot/pool/driver/central状态；fresh compile/focused/Play/self-check/Console0/ledger PASS。

## Files likely involved

- 如确有必要：一个新的Editor-only OID5152 Play probe及`.meta`；
- existing OID5152/self-check/lifecycle/central diagnostics；
- Task/Research/Change Record/Ledger/STATE/register/main plan/handoff；
- production gameplay默认不应修改。

## Unknowns

- 正式OID51 DAT是否有当前输入合同可达的DJA target；执行时先只读查询loaded frame，再决定DJA或4500 tick；
- authored running frame能否通过physical double-tap在可重复时间内形成；若Editor InputSystem边沿不稳定，允许
  在初始fixture选择正式DAT的running frame，但不得预写merge结果；
- 4500完整tick的Editor耗时未知；可用Manual/no-publication推进中间tick，但merge/split观察tick必须构建
  presentation，且不改变每tick规则或dt。

## Verification

1. source/DAT/reachability crosswalk；
2. existing OID5152 focused 32/32与七组self-check；
3. actual production factory→maintenance merge→central dormant→split Play；
4. slot/generation/rest/formal reset/central pixel结构化报告；
5. full self-check与Change Ledger validator。

## Stop conditions

- 正式OID7/8/51 wrapper/resource无法加载；
- 必须直接写merge/dormant/split结果才能继续；
- production first difference出现；
- 需要改变allocator、slot/generation、pass order或受保护adapter；
- 4500 tick不可在合理Editor时间完成且DJA不可达；
- 只剩C++ full trace且R1-WP02仍BLOCKED。

## Out of scope

AI parity、P1/P2通用修复、R07A/B/C、其他OID lifecycle、T8、IL2CPP、Android、服务器、C++ executable/full trace。

## Authorization

用户已于2026-08-23明确批准执行R08并恢复总目标。已在任何脚本写入前建立
`R8-MERGESPLIT-001 / IN_PROGRESS / TEST-ONLY`；批准只覆盖本Task Contract，不授权production gameplay
修复、R09或范围外架构变更。

## Execution blocker（2026-08-23）

只读reachability审计确认当前Unity项目缺少`data.txt`所声明的三个正式角色DAT：

- `Assets/NTSD/Config/chars/rock_lee.dat`（OID7）；
- `Assets/NTSD/Config/chars/chiyo.dat`（OID8）；
- `Assets/NTSD/Config/chars/sasori.dat`（OID51）。

loader没有其他正式fallback；缺失文件会被跳过，无法生成production wrapper。相邻`ntsd_proto`目录虽存在
同名加密DAT，但本合同禁止新增/修改DAT，且其Unity适配来源未确认，未擅自复制。已触发stop condition
“正式OID7/8/51 wrapper/resource无法加载”，故`B-R8-R08-01`成立；probe尚未创建，专项Play未运行。

恢复条件：用户恢复上述Unity DAT，或明确确认允许使用且符合当前Unity DAT适配流程的资产来源。

## Resource deployment authorization（2026-08-24）

用户已明确指定`J:\QQFile\NTSD 2.4.1\chars`及`...\sprite`为本次Unity type0资源恢复源，并批准将缺失
type0 DAT/BMP部署到`Config/Character`及`Sprite/Character/<dat-basename>`，同步改data.txt与DAT bmp_begin
资源路径。该资源工作拆为`R8-CHARASSET-001`；完成其预检、复制、路径适配与wrapper验证后，才可解除
`B-R8-R08-01`并继续R08。此项不授权改DAT战斗字段或production gameplay。

## B-R8-R08-01 closure（2026-08-24）

资源 blocker 已关闭，而不是把旧缺失事实删除：用户批准的恢复已部署37个缺失type0 DAT与182个去重BMP；42个
type0 `data.txt`映射均为Character catalog，227个DAT BMP引用均指向存在的`Assets/NTSD/Sprite/Character/...`
文件。Unity全资源导入compiler error=0；资源契约用例
`CharacterAssetDeploymentEditorTests.TypeZeroCharacterCatalogDecryptsParsesAndResolvesDeclaredBitmaps`实测1/1 PASS；
正常`NTSD_Battle` Play 20秒Console error/warning=0。

因此OID7/8/51 wrapper资源前置已满足，`R8-MERGESPLIT-001`可在用户下一次明确恢复R08执行时从`BLOCKED`调整为
`IN_PROGRESS`。本次没有创建R08 probe、没有运行merge/dormant/split、没有修改任何production gameplay。

## Execution blocker B-R8-R08-02（2026-08-24）

用户恢复目标后，R08 Editor-only probe已写并fresh compile0；正式Play在fixture创建前由角色资源预热异常阻塞：
`Duplicate battle sprite key (56,112)`。OID56 DAT的106-120与112-200范围重叠；C++ renderer采用DAT声明顺序
first-range-wins，Unity当前异步sprite写入与catalog builder不支持且可能受任务完成顺序影响。已触发production
first-difference stop condition，拆出`R8-WP01G-R08-R01 / R8-SPRITERANGE-001 / APPROVAL PENDING`。

补充全目录只读审计：使用项目正式DAT解密密钥与减法算法解析`data.txt`全部137个对象，137/137成功、共347条
sprite范围，只有OID56的112-120重叠。由此排除“还有大量同类DAT尚未发现”的当前证据风险，但不改变通用修复
要求，也不授权在未批准前写production/test repair代码。

在该repair获批、实现并验证前，R08不得继续运行或宣称merge/dormant/split已验收。本first difference与OID7/8/51
gameplay无关，不允许通过删除OID56、修改DAT范围或在probe里跳过资源预热绕过。

## Resumed probe correction（2026-08-24）

- `B-R8-R08-02`已由获批的通用first-declared-range ownership修复解除；R08已重新进入真实production完整tick；
- post-fixture `ObjectCount - 1`合同已通过，OID51/frame290、dormant、HP/HPBound/PP/metadata均已观察到正确值；
- 当前FAIL仅因probe把Z夹具固定为340/344，而当前production `Runtime.Stage`由场景BoundaryWall换算后的Z下界高于
  该值；C++与Unity都会在merge之后的同一完整tick执行StageBounds，因此最终Z被合法夹取到376；
- 只允许将Editor-only夹具改为从当前production stage范围选择合法Z，继续严格验证merge中点；不得修改StageBounds、
  场景、production merge writer或把该测试失败写成gameplay差异。
- 合法Z夹具实测进一步闭合完整tick顺序：merge writer写midpoint后，C++/Unity都在同tick的frame advance physics
  使用未被merge清零的self.vz推进1px，再由地面摩擦归零；因此验收读取tick末快照时必须断言`midpoint + 1`，
  而不是merge pass瞬间的midpoint。此为source-backed probe correction，不是放宽gameplay结果。
- 修正后merge与Central dormant/visible合同均已通过；进入DJA时发现probe把物理顺序写成attack→defend→jump。
  C++与Unity authority crosswalk都要求defend→jump→attack，因此只修Editor-only投递顺序并继续完整tick验证。
- correction：Unity物理键到C++ cooldown语义的正式映射为`att→defend`、`def→jump`、`jump→attack`，所以物理
  `att→def→jump`才是canonical DJA。实际阻塞是Local provider生成的complete FrameInputSet覆盖并令同tick普通buffer
  event失效。probe将改用公开`StepOneTick(FrameInputSet)`投递该roster slot的正式complete frame；不得直写combo。
- canonical输入实测到达DJA step1/2，但OID51 frame290正式`hit_ja=0`，C++不会进入Unk328 cooldown-clear标签；
  现有self-check也禁止missing-target fallthrough。因此R08改走合同原有的4500完整tick fallback：分批无表现推进，
  最终`Unk338==1`tick构建presentation并由maintenance自然split。split后的Unity ObjectCount必须恢复post-fixture
  logic+shell值，不使用C++逻辑对象数或旧`baseline+2`误算。

## Production first difference B-R8-R08-03（2026-08-24）

- 4500 fixed ticks已真实推进到split final maintenance；merge runtime、OID51 Central body与dormant suppression已通过；
- `TrySplitOid51BackToPair`在`partner.Reset()`期间通过relation/link setter发布字段，AI unified publisher对该dormant
  slot执行current-row验证并抛`stale slot generation after commit`；
- 异常发生于production脚本而非probe断言，并中断split/cleanup；R08不能标PASS；
- 已拆`R8-WP01G-R08-R02 / R8-AIROWGEN-001 / PLANNED / APPROVAL PENDING`；批准前不改production。

## R02 repair observation / R08 acceptance continuation（2026-08-24）

- 获批的`R8-AIROWGEN-001`已写入通用row-membership invalidation并通过专项回归；
- 修复后R08已真实推进完4500 tick，旧stale-row异常不再出现；
- 当前探针在split后用一个联合断言同时验证OID、dormant和旧绝对ObjectCount，失败前没有保存分项证据，无法裁决
  production split还是长时运行中的并发lifecycle计数；
- 下一步仍属于本Task和`R8-MERGESPLIT-001`的test-only验收：在final tick前后采集两实体、ObjectCount、claimed
  slots与structural writer delta，拆分断言后重跑。若OID/dormant/slot/generation本身失败，立即作为新的production
  first difference停止；若只有旧绝对计数受并发lifecycle影响，只能依据局部split delta修正probe，不得放宽对象恢复。
- 分项重跑证据已经裁决为probe口径问题：split身份/dormant/slot/generation均正确，局部ObjectCount严格`14→15`且
  claimed slots不变；全局旧fixture绝对值因4500 tick期间其他production lifecycle而不可作为final tick局部合同。
  同时，C++按split当前HP/HPMax除2，且写frame112后同tick继续frame advance；正式DAT的112为`wait0→113`。因此
  probe应断言pre-split动态HP/HPBound的一半与tick末frame113/state8，不能把merge初值或pass瞬时frame写死。
- 上述修正后runtime与Central split已经全部通过；当前只剩probe cleanup。因为测试真实推进4500个production tick，
  期间产生的post-baseline对象不会被仅释放OID7/8夹具的旧cleanup覆盖。Task允许的最小清理改动是：baseline保存
  generation-safe runtime handles；结束时只释放当前快照中不属于baseline的实体并flush，继续要求四项count/pool与
  RNG/sound恢复。运行前实体一律保留，production生成/销毁实现不改。
