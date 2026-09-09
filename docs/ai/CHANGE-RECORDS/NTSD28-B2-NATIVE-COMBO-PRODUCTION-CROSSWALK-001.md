# NTSD28-B2-NATIVE-COMBO-PRODUCTION-CROSSWALK-001 — native combo production crosswalk

<!-- CHANGE-RECORD
id: NTSD28-B2-NATIVE-COMBO-PRODUCTION-CROSSWALK-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: NONE
authority: NTSD 2.8-Logan input_state.h, input_routing.cpp, simulation_tick_driver.cpp, native_ai.cpp and locked input/skill corpus tests; current Unity input/AI/frame/parser/action owners.
evidence: AUTHORITY-SAMPLE-FIRST / PROXY-BEFORE-EDGE-AND-COMBO / COMBO10-PRIORITY-CLOSED / UNITY-PRODUCER-EARLY-EDGE-COMBO / UNITY-HIT-AJ-AD-JD-MISSING / AUTHORITY-HIT-AJ-1110-IN-96 / AUTHORITY-HIT-AD-1089-IN-96 / AUTHORITY-HIT-JD-99-IN-99 / UNITY-CONFIG-0-0-0 / IMPLEMENTATION-SPLIT-4 / AUTHORITY-READ-ONLY / NO-SOURCE-CHANGE
-->

> 状态：`VERIFIED / CROSSWALK_CLOSED / IMPLEMENTATION_SPLIT_DEFINED / GOVERNANCE_ONLY`

## 已观察事实

- 2.8 `simulation_tick_driver.cpp:351-454`第一遍只完成non-character `hit_Fa`、AI producer、
  human/replay按phase sample、`previous/current`推进与slot0..7 current记录。
- `simulation_tick_driver.cpp:455-508`第二遍先按counter/enabled/source/live/type0门复制完整0x21 block，
  随后才调用`InputRouter28::step_sampled`。
- `input_routing.cpp:1511-1574`在proxy之后依次执行dead type0 clear、current remap、edge/cooldown递减、
  rising-edge history、combo10推进、combo字段、三按钮字段、方向字段和type0 built-in。
- combo字段优先级固定为`hit_Fa/Fj/Ua/Uj/Da/Dj/aj/ad/ja/jd`；任一字段被尝试后才执行
  `clear_combo_attempt`。横向字段为0时不得转向，`hit_ja`保留独立special-family gate。
- Unity human `NTSDInputStateModule.UpdateFromBuffer`当前在producer采样内就执行`DecrementCooldowns`和
  `ApplyNewPressEdges`；Unity AI `AiDecisionKernel.ApplyInputEdges`也在producer内更新旧edge/history，且
  多处分支直接写旧9-combo状态。现有resolver只消费旧9-combo。
- `NTSD28InputTwoPassModule.FreezeProducerState`目前把旧edge/combo投影进exact block；这是显式迁移桥，
  不是native第二遍真值。
- Unity `LF2FrameData`与`Lf2DatConverter`没有`hit_aj/hit_ad/hit_jd`。只读统计：新权威decoded DAT
  分别为1110处/96文件、1089处/96文件、99处/99文件；Unity正式Config三者均0处。
- `native_ai.cpp`个别注释把combo index 2称为`hit_aj`，但正式router和skill corpus均明确index2=
  `hit_Ua`、index6=`hit_aj`；实现必须按实际路由与locked corpus，不按该注释机械映射。

## 实施拆分

1. `B2-NATIVE-COMBO-ROUTER-FIELDS`：先补frame/parser三个缺失字段与exact combo优先级/attempt/clear
   resolver；只用synthetic fixture验证，不修改`Assets/NTSD/Config`。
2. `B2-NATIVE-INPUT-PRODUCER-MIGRATION`：把human/AI producer限制为pending/current/previous与AI直接
   决策写入；proxy后统一调用native edge/combo machine，再投影兼容镜像，禁止双推进。
3. `B2-RNG-CALLSITE-MIGRATION`：按83 expression/80 synchronized IDs及2个direct CRT另包迁移，避免
   与input action重构混在一起。
4. `B2-INPUT-JOINT-TRACE`：同seed/input/tick验证phase、recording、0x21 proxy、history、combo/action和
   两条RNG stream；first-difference为零后才可把B2标aligned。

## 边界

- 本包只读并只更新治理文档；权威目录未写入。
- 不修改DAT、Unity正式Config、Scene、ProjectSettings、AI规则、RNG consumer或action行为。
- 三个新字段在Unity正式Config中当前无值；字段/解析能力不等于B11内容迁移，不能借此提前选择内容策略。
