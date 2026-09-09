# NTSD28-B2-INPUT-JOINT-TRACE-001 — B2 exit joint trace

<!-- CHANGE-RECORD
id: NTSD28-B2-INPUT-JOINT-TRACE-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: NONE
authority: Formal NTSD2.8-Logan.exe SHA 1277B70BA030A1F33B625EEA20B43834325B280CEC555650BF43CD90A64DAF75 plus playable source closure; same-scenario completed-tick input joint trace exit requirement.
evidence: TASK-CONTRACT-CREATED / AUTHORITY-CAPTURE-REBUILT / DOMAIN-INPUT-SLOTS-LIFECYCLE-EQUAL / SLOT-CAPACITY-EXCEPTION-APPLIED / UNITY-TRACE-PHYSICAL-BUTTON-TRANSLATOR-DEFECT-CORRECTED / JOINT-ACTION-STATE-COUNTER-EQUAL / ENTITY-EQUAL-37-DIFFERENCE-10 / FIRST-DIFFERENCE-BASEMAXMP-B11 / EXACT-INPUT-NATIVE-DUAL-RNG-INPUT-COMMON-3-TICKS-6-PAIRS-EQUAL / HUMAN-DIRECT-PER-CALL-INPUT-COMMON-AND-STANDING-ATTACK-EQUAL / AI-RNG-PER-CALL-6-7-8-EQUAL / AI-TICKS1-2-EXACT-AND-RNG-EQUAL / AI-NEXT-FIRST-DIFFERENCE-TICK3-CURRENT-MASK-B11-CONTENT-DOWNSTREAM / ACTION-CONTENT-B11 / FORMAL-EXE-HUMAN-AND-AI-900-TICK-HEADLESS-EXIT0-PASS / FORMAL-EXE-INPUT-RESET-OBSERVED / FORMAL-EXACT-INTERNALS-NOT-EXPOSED / SOURCE-MODEL-DIAGNOSTIC-ONLY / FORMAL-PER-CALL-CERTIFICATE-NOT-CLAIMED / NONAI-50-PLUS-2-ROUTED / FUNCTION-KEY-REAL-PLAY-PASS / B2-SCOPE-JOINT-CLOSED / AUTHORITY-READ-ONLY
-->

> 状态：`VERIFIED / B2-SCOPE-JOINT-CLOSED / HUMAN-SCENARIOS-EQUAL / AI-TICKS1-2-EXACT-AND-RNG-EQUAL / FORMAL-BEHAVIOR-PASS / DOWNSTREAM-FIRST-DIFFERENCE-B11`

## 改前事实

- B2输入producer、proxy、combo/action、双RNG commit与ground/air production已分别通过focused/部分Play，
  但尚无同seed/input/tick的统一双端first-difference证据。
- 现有authority runner可输出47-field entity raw与B0 domain raw；Unity exporter可跑共同2实体3tick场景。
- 这些工具仍明确是source-model/B0诊断，不是正式EXE certificate；现有domain schema也可能缺exact
  proxy/history/combo10/per-call synchronized log。

## 本阶段职责

- 不修改代码，先重建并运行现有双端capture/validator/comparator，冻结新鲜首差和schema缺口。
- 若缺口阻断B2 exit，后续先更新治理范围再写工具或exporter；不得在本GOVERNANCE_ONLY记录下偷改脚本。

## 验证记录

- Task Contract在所有capture前建立；production与authority未写入。
- `Build-AuthoritySourceCapture.ps1`成功，identity为formal EXE SHA `1277B70B...DAF75`、source manifest
  `C59BD8D...F2D75`、runner source `05E352F5...B09DC`、capture binary `9BBF3D90...756D2`；
  只标`SOURCE_MODEL_DIAGNOSTIC_ONLY`。
- 同一input-common scenario的authority与Unity旧译码raw/domain均valid；entity raw分别为
  `EFF8D693...1A238`/`A8B6DD05...3F50`，domain raw分别为`A3C3372F...C39A6`/
  `C28C478B...CB08`。
- domain comparator `FD4C0C60...0E9CD`：input、slots、lifecycle相等；capacity1000/400按用户例外；
  Unity旧schema只报告`unityDeterministic`，未表达current native dual RNG，故topology different仍待扩展。
- entity comparator `DA6DE8D7...C6CE`：首差tick2 slot1 action110/210，随后action latch/previous/
  tick snapshot与state7/4分歧；baseMaxMp500/200是B11内容差异，9个missing为后续runtime/schema项。
- 输入链复核：物理J/K/L在正式Unity合同中以FrameInput Jump/Defend/Attack交叉承载；旧trace parser却直译为
  Attack/Jump/Defend，使物理K+L误成native J+K。production bridge不改，已另立
  `NTSD28-B2-UNITY-TRACE-PHYSICAL-BUTTON-MAPPING-001`测试/诊断修正包，修正后再取真实首差。
- translator修正后的Unity entity SHA `6AFABA89...D63DD8`；comparator `AD963C73...E003EA`把
  action/latch/previous/tick snapshot/state/counter列为equal，37 equal/10 differences，首差为B11
  baseMaxMp500/200，其余9 missing。B2动作主路径的这个3tick共享场景已闭合，但不扩大为完整B2。
- 已选择`NTSD28-B2-INPUT-RNG-JOINT-RAW-SCHEMA-001`建立独立exact input/native dual RNG输出，避免破坏
  B0 v1 schema或被B11字段阻断；per-call RNG log/formal EXE certificate仍另待证据。
- direct-battle RNG pre-draw与defend re-entry exact mirror两个后续生产包完成后，同一input-common fixture的
  exact input/native dual RNG comparator为3 ticks/6 entity pairs全equal、`firstDifference=null`；这仍是
  source-model diagnostic，未补per-call log或formal EXE certificate，B2保持IN_PROGRESS。
- v2后续包已使input-common及standing-attack的human/direct completed-tick calls均可观察并compare equal；
  standing-attack在tick2精确为site0x82/bound2/result1。AI cursor/accepted commit per-call及formal EXE仍待。
- v3 accepted-only AI cursor、first-tick readiness、native history pre-bind与host pending投影包完成后，AI场景
  ticks1/2/3的synchronized calls 6/7/8逐次equal；tick1 exact及tick2 previous/run均闭合。当前AI首差为
  tick2 slot1 `keyHistory[3]` authority4 / Unity-1，归后续exact-input store roundtrip包；run action650/9为
  Direction B内容差异，归B11。formal EXE certificate仍未获得，B2未退出。
- exact-input store roundtrip包关闭tick2 history后，AI comparator完成ticks1—2全部entity/input/RNG比较且相等；
  新首差为tick3 slot1 `currentMask` authority18 / Unity3。由于tick2一般entity已先发生authority action650 /
  Unity action9，tick3输入是该B11内容差异改变战斗状态后的AI下游，不再归B2输入实现。formal EXE仍待。
- 根formal EXE SHA锁定后，p2-human/p2-ai两条headless smoke均exit0、900tick passed；human双人七键、动作、
  no-input divergence和双方reset clean均成立，AI正式配置smoke通过，authority清单前后不变。该schema不含
  exact/history/combo10/per-call RNG，故只关闭正式EXE可观察行为边界，内部字段继续以playable source joint
  trace为证；不声明formal per-call certificate。下一步审计B2剩余I/R/A条目后决定阶段退出。

## 退出证据补记

- B2 exit audit已逐项路由I/R/A；NONAI 50+2调用均交给B3+ single-owner阶段。
- F1～F12 crosswalk、pure route、Session carrier、production integration与real Play physical probe均完成；Play tick0→5、
  九组PASS、Scene不变。
- 因此本joint record在B2职责内关闭；B11内容分叉和B3+ producer/effect不反向计为B2失败。
