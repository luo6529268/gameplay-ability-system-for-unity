# HANDOFF — R1-SOURCE-004 candidate / collision / hit / grab / weapon

> 日期：2026-08-21  
> Work Package：R1-SOURCE-004  
> 状态：COMPLETED（静态 source contract；未做 runtime / trace / Play Mode 验收）  
> 唯一行为 authority：J:\QQFile\NTSD2.4\ntsd_release 的 release live source。

## 1. 完成的工作

- 以 game_tick T10、T11、T13 为入口，静态闭合了 collision_collect、collision、hit、
  entity_collision 的 candidate 创建、pair 顺序、vrest、candidate carrier、Loop1/Loop2
  consume 和 kind dispatch。
- 建立 Unity snapshot、pair-vRest、BruteForceSceneQuery、candidate range、data-oriented /
  fallback consume、InteractionWriter、DamageWriter 的 source crosswalk。
- 确认 optimized/data-oriented 与 fallback 均最终调用 BattleHitCandidateSequenceRunner；
  Unity 的 pass partition 也按当前 DAT type==0 / >0，而非 MonoBehaviour 类型分流。
- 确认 C++ HIT_CANDIDATE_MAX=20 与 Unity BattleEcsHitExecutionPlan 每 slot 20 entries
  静态一致。
- 对 D-MOV-002 的 C03/C07 read set 完成静态判断：Frame.PN / AttackingCounter 不是
  candidate/hit gate 的直接读取；后续 frame progression 影响仍保持待 fixture。

## 2. 已登记的静态差异

| ID | 已确认的 C++ → Unity 差异 | 后续 owner |
|---|---|---|
| D-COL-001 | C++ hit_confirm2 对 character candidate 执行 entire-attacker abort；Unity unified runner 未读 attacker HitConfirm2。 | R2，先用 Loop1 weapon/special → Loop2 object fixture。 |
| D-COL-002 | C++ caught cpoint/hurtable gate 位于所有 kind dispatch 前；Unity unified runner 未统一执行。 | R1-SOURCE-005 → R2。 |
| D-COL-003 | C++ effect21 + target current state18/19 终止 entire attacker sequence；Unity仅有 collect-time previous-state filter。 | R2。 |
| D-COL-004 | Unity IsPureTransitionSmoke 多出 oid999 全局 candidate gate；C++当前 collect source无同等 gate。 | R1-SOURCE-005，DAT 可达性 fixture。 |
| D-COL-005 | C++ kind1 没有与 kind3 相同的 character-only target gate；Unity统一 InteractionWriter 将二者均限制为 character。 | R1-SOURCE-005，multi-type kind1 fixture。 |
| D-HIT-001 | C++ type3 normal kind0 写公共 HP/HP max/combo/damage stat；Unity special writer漏写 vital/stat。 | R2，type3 durability/death fixture。 |
| D-HIT-002 | C++ kind10/11、kind16、normal weapon hit 直接写 frame；Unity部分使用 Immediate/SetFrame helper并附带 PN/attacking/transistor 副作用。 | R2，raw-writer subset fixture。 |
| D-HIT-003 | C++ type1/2/4 normal kind0 仍写公共 vital/stat；Unity weapon writer仅写 WeaponFlightCounter 等耐久反应字段。 | R2 + R1-SOURCE-005，weapon vital/durability fixture。 |

## 3. 已映射但不是自动缺陷

- separate collision snapshot / pair-vRest pass；
- role-aware / loose broadphase；
- candidate list + generation metadata；
- candidate max=20；
- Loop1/Loop2 participant partition；
- kind4/5/9 runtime ITR conversion；
- alternate hurt selector；
- oid300 redirect；
- dormant SuppressCollisionCandidateUntilTick；
- EndCollisionCandidateConsumption；
- kind0 hit record owner/anchor formula。

这些均仍需要未来 joint fixture 或 trace 时再提升运行时证据；不能以“静态结构类似”宣称已对齐。

## 4. 明确移交给 R1-SOURCE-005 / 006 的未知项

- kind1/2/3/7 在 CPoint、held、link、holder、target、opoint 与 pool lifecycle 中的后续
  consumer；
- type3 Karasu identity replacement、held relationship、pool/reuse 的完整 field reset；
- candidate carrier clear 与 Unity EndCollisionCandidateConsumption 之间尚未覆盖的
  cpoint/held/pool consumer；
- hit record 的全局 RNG call count 与中央表现 handoff；
- weapon / special / shadow 的 render可见边界与排序。

## 5. 未执行项与边界

- 未修改 Assets/NTSD/Scripts/、C++ source/build/binary/资源/配置、DAT、scene 或 renderer；
- 未运行 C++ executable、Unity compile、BattleRuntimeSelfCheck、Play Mode、trace、
  performance 或 1000 AI；
- C++ authority directory 未写入；
- T8 stage.dat 默认部署仍暂缓；
- CentralOnly/中央 Mesh/Texture2DArray/URP、容量 profile、30Hz、FrameInputSet、SoA/ECS、
  pool、worker 和零 GC 目标保持不可回退。

## 6. 下一步

推荐立即开始 R1-SOURCE-005。进入点必须是 C++ game_tick 的 T09/T14/T15/T16，
再追 cpoint.cpp、weapon.cpp、frame_advance.cpp、opoint/lifecycle helper；首先关闭
D-SCHED-001～004、D-INP-001、D-MOV-002～004、D-COL-002/004/005 与 D-HIT-002/003
的 relation、newborn、reset 和 consumer 合同。
