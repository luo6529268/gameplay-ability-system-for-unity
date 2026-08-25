# R1-SOURCE-004 — C++ candidate、collision、hit、grab 与 weapon consume 源码合同

> 状态：COMPLETED（静态 source contract；runtime / joint fixture 待后续阶段）。
> Authority：J:\QQFile\NTSD2.4\ntsd_release 中参与 ntsd_new.exe release 构建的 live source。  

本文件只记录静态 C++ source contract；未运行 executable、未取得 trace、未运行 Unity。

## 1. 主链与冻结边界

| checkpoint | C++ release source | 静态合同 |
|---|---|---|
| C00 | game_tick.cpp:2083-2087, 310-350；include/game_world.h:202-214 | 每 tick 末尾的 postframe tail 对每个 active entity 清 candidate carrier：mp=0、mp2/mp3/mp4=1000、hit_confirm2=0。下一 tick 的 candidate collect 从该清空状态开始。 |
| C01 | game_tick.cpp:1645-1655 | 在 Loop1/Loop2 之前，先按升序全 slot 扫描 active entity，写 prev_frame2=frame；随后才开始 candidate collect。 |
| C02 | collision_collect.cpp:363-372；entity_collision.cpp:92-96 | candidate collect 固定 i=0..MAX_OBJECTS-1、j=i+1..MAX_OBJECTS-1。每个 active 且有 char_data 的无序 pair 先对称递减 s_vrest[i][j] / s_vrest[j][i]，再依次 collect i→j、j→i。 |
| C03 | collision_collect.cpp:104-120, 242-360 | pair 先检查 active、char_data、attacker attack_exempt、vrest、oid205→oid9 特例；随后要求 attacker current frame 有 itr、target current frame 有 bdy，几何使用双方 prev_frame2。 |
| C04 | collision_collect.cpp:123-240；include/game_world.h:13-15,153-158 | record candidate 处理 state12/fall、nearest path、nearest RNG tie、kind1 target-nearest、kind2/7 fresh jump；HIT_CANDIDATE_MAX 固定为 20；写 attacker mp、candidate slot、itr index。 |
| C05 | collision_collect.cpp:266-359 | 每个 prev2 ITR 在写入前依次经过 oid、kind、state3005、hit_stop、same team、kind5、effect、z width 和 itr/bdy exact overlap 筛选。kind3 / kind8 明确只接受 character target；kind1 没有同类 target-type gate。candidate collect 不做消费侧 kind5 runtime replacement。 |
| C06 | collision.cpp:32-1259；game_tick.cpp:1648-1655,1818-1822 | T11 Loop1 只消费 current obj_type==0 的 attacker；T13 Loop2 只消费 current obj_type>0 的 attacker。二者均按升序 slot，再按 attacker mp 中冻结顺序消费。 |
| C07 | collision.cpp:57-80, 86-194 | 消费每条 candidate 时，先重新检查 vrest；hit_confirm2 对 character target 会结束整个 attacker；被抓 cpoint 保护会跳过当前 candidate；kind5/4/9 等 runtime ITR replacement 发生在消费侧；effect21 对 target current state 18/19 会结束整个 attacker。 |
| C08 | collision.cpp:493-1247；hit.cpp:81-793 | kind 0/1/2/3/6/7/8/9/10/11/14/15/16 分别写 damage、frame、velocity、relation、vrest/arest、hit confirmation、sound/hit record 等字段。kind1/3 直接写 frame，再做抓取对位和关系写入；kind2/7 写 pickup/link。 |

## 2. C++ candidate 关键不变量

1. slot 顺序不是抽象的 entity enumeration：C02 使用固定 MAX_OBJECTS 的 i/j 升序遍历。
2. prev_frame2 是 C01 冻结的碰撞几何帧；current frame 仍参与 active itr/body、state 和个别 filter。
3. pair vrest 的递减发生在每个无序 pair 的 collect 前，而不是 hit consume 后。
4. nearest 和 kind1 tie 都消耗 C++ RNG；改写候选发现或排序不得改变 tie 发生顺序。
5. candidate carrier 在 T18 后才被 C00 清空。因此 T11 对 object 写入的 hit_confirm2 在同 tick T13 仍可读取。
6. Loop1/Loop2 的分界按 consume 当刻 attacker 的 current obj_type 判定，不按 candidate collect 时的 object type 预先固定。

## 3. 消费前的 C++ 可观察 gate

### C07-A — hit_confirm2

collision.cpp:65 在每条 candidate 开头检查：attacker.hit_confirm2 非零且当前 target 为 character DAT 时，直接转到 next_attacker。它不是单条 candidate skip，而是停止该 attacker 本次 loop 剩余 candidate 的消费。

### C07-B — caught cpoint 保护

collision.cpp:69-79 读取 target 的 prev_frame2 cpoint。若 target 的 cpoint.kind==2，且其 catcher 仍 active、catcher.caught_idx 等于当前 attacker slot、catcher 的 prev_frame2 cpoint.hurtable 为零，则跳过当前 candidate。该 gate 位于 kind dispatch 之前，对所有 kind 生效。

### C07-C — effect 21 current-state abort

collision.cpp:188-194 在消费侧检查 runtime ITR：kind==0、effect==21 且 target 当前 state 为 18 或 19 时，转到 next_attacker。candidate collect 的 prev-frame effect filter 不能替代此 gate，因为 target current state 可在 collect 与 consume 之间发生变化。

## 4. 已读 hit / interaction 核心字段合同

| 子流程 | C++ source | 必须保留的写入边界 |
|---|---|---|
| normal / alternate damage | hit.cpp:81-489, 515-629 | 普通 apply_hurt 对 type0/1/2/3/4 均写公共 HP、HP max bound、combo、damage stat、fall、hit count/state count、frame delay、arest/vrest、holder frame delay、state1002/3000 tail；type6 明确走 reaction-only，不走 vital/stat write。 |
| kind1 / kind3 grab | collision.cpp:921-994,1084-1143 | Vx 清零、facing、raw catching/caught frame、integer-based grab alignment、双方 slot relation、caught duration=300、victim fall=0；其中 kind3 在 collect 侧明确 character-only，kind1 无对应 target-type gate。 |
| kind2 / kind7 pickup | collision.cpp:996-1081 | raw pick frame（kind2）、link sign、team/holder/target/held slot、pickup count、attacking reset。 |
| kind6 / kind8 / kind14 | collision.cpp:1146-1155,1240-1242；hit.cpp:639-662 | hit confirmation、heal timer / attacker frame / XZ、direction block。 |
| kind9 / kind10/11 / kind15/16 | collision.cpp:1157-1247；hit.cpp:664-793 | object break / attacker HP zero、flute force/stat、wind/freeze damage、vrest、held release。 |

### C08-D — non-type6 normal kind0 进入 common vital/stat path

collision.cpp:561-585 对 type6 以外的 target 调用 apply_hurt；hit.cpp:104-155 的 standard
damage write 没有按 obj_type==1/2/3/4 排除。因此 type1、type2、type3 和 type4 target 的
kind0 normal damage 都会写公共 HP、hp_max、combo_count_vic，以及满足 unk_344 条件时的
global damage stat；只有 type6 在该 callsite 明确走 reaction-only。

其中 type3 的 HP 随后是 game tick late update 中 type3 death/lifecycle 的输入，不能把
type3 仅当作无耐久视觉效果。type1/2/4 的同一公共 HP 字段也是真实 Entity 字段，尽管它们
另有 weapon_count / unk_31C 相关耐久与生命周期分支。

## 5. 尚未闭合的 C++ 读取

- kind0 normal damage 的所有 type1/type2/type3/type4/type6 branch 与 Unity writer 的
  remaining per-field / lifecycle-consumer 对照；
- kind9 和 type3 Karasu identity replacement 的 Unity source contract；
- C++ hit record 的 owner choice、RNG 点位和 Unity central presentation descriptor 的映射；
- C++ grab/pickup 后 T14 CPoint / WeaponSync 与 T16 held/link 之间的联合可见边界；
- C++ candidate carrier clear 之后的 late-tail read set。当前只确认 cpoint.cpp、weapon.cpp、frame_advance.cpp 未读 candidate carrier；完整 lifecycle 仍交给 R1-SOURCE-005。

## 6. Evidence 分级

- VERIFIED（source）：本文件表格中列出的 C++ file/line control flow 已被静态读取。
- INFERRED：由 C++ field name 与 Unity field mapping 得出的同义关系，必须在 Unity crosswalk 中标出。
- UNKNOWN：任何需要 executable trace、DAT reachability、Unity runtime 或 CPoint/held/lifecycle 联合证据的结论。

本包不授权修改 C++、Unity gameplay、测试、pass 顺序或 Unity rendering/capacity 架构。
