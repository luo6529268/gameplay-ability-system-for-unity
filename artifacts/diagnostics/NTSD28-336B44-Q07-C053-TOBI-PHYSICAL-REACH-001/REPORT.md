# Q07/C053 Tobi普通输入到OID251有限候选（2026-10-04）

**v6 `next:0`语义纠正，覆盖下文“自然action0待找”和旧静态`51→2→3→0`预期。** 正式`a/fir/firz.dat`的51→2→3之后，frame3声明`next:0`；当前336B44 playable `FrameMachine28::step()`把0解释为`stayed`，不跳到action0。原Y0/-80案的`hit_Fa:7`在整数Y>-25时由`native_ai.cpp`行为7切到action60；新增受控OID0/action212/Y-130案把OID251出生提高50像素，仍在tick13～21保持action3，tick22转60，40tick无action0。[run-f逐tick](run-f/controlled_212_high-ticks.csv)、[摘要](run-f/summary.csv)。该案40tick LFR由336B44根EXE独立回放[PASS](root-f-controlled_212_high/report.json)；动作/位置/输入相位六字段×40tick[240/240零差](source-root-high-comparison.json)，报告`nativeParityClaim=false`，不能冒称全World或Unity同态。原两案run-e/run-f LFR SHA各自相同。Unity生产`LF2Entity.RunCommonFrameTickFromTransistor()`对rawNext0亦是保留当前帧，`RunNonCharacterHitFa7FrameLogic()`有相同阈值分支；这是源码静态对应，原Battle Scene新程序集与运行时未验。不能把受控action0双Uj子门写成Tobi自然action0；也不能据三例断言OID251在所有其它受击/交互路径永远不可达action0。C053/Q07仍开放，但不再以提高出生高度搜索这个无效静态帧链。

**v5生产Spawn事件补证。** 唯一Tools探针新增`step->spawns.events`的OID251事件计数、status、slot和source_line；当前336B44 playable闭包编译exit0/无诊断。[run-e摘要](run-e/summary.csv)与[普通案逐tick](run-e/jump_from_zero-ticks.csv)、[受控案逐tick](run-e/controlled_212-ticks.csv)显示：普通案tick11、受控案tick5各有且仅有一条`spawned(0)`事件，slot50、source_line1917，与子体首次可见同tick。两案run-e LFR逐文件SHA分别与run-d完全相同（`C36C6326DEAEE9DE10B6E80BEBCF2FEFF6763CF540E31DA826DDCBFA9C9FBDE3`、`F009A8C03A73D8A0DFF05CDFB6160CBEF3001E036FF39BD686E80CB84F56F82D`），故正式根已通过的两次相同字节回放仍适用；没有重复运行根EXE。这里证明的是正式源生产Spawn pass事件与实体出生同tick，仍不证明Unity原Battle Scene。

**2026-10-04 身份纠正后的当前结论：`VERIFIED_SCOPED_SOURCE_ROOT_PRODUCER / UNITY_RUNTIME_PENDING`。** 正式`data/data.txt`中OID0=`c/tobi/tobi.dat`，OID53=`c/tobi/ttobi.dat`；只有OID0的212帧含`hit_Fa:510`，510→512链中512帧生成OID251。先前两条OID53阴性是错误角色前置，不是输入消费者或Parser首差；旧原件全部保留在run-a/b/c。修正工具在初始化时核验加载的frame212`hit_Fa=510`，以当前336B44 playable完整GameSession同两条输入、各40tick重跑，v4编译exit0/无诊断：[run-d摘要](run-d/summary.csv)。普通action0跳跃案tick8经`hit_Fa`进入510、tick11自然生成slot50/OID251/action51；受控212案tick2进入510、tick5生成相同子体。两案均产生LFR，336B44根正式EXE回放[普通案](root-d-jump_from_zero/report.json)和[受控案](root-d-controlled_212/report.json)均`passed=true/failureCode=0`、40声明tick；根trace有同出生，选定8字段×40tick×2案[640/640零首差](source-root-comparison.json)。根报告`nativeParityClaim=false`，本比较没有把LFR未编码的CRT初种/RNG或全World作为同态。

**v4历史判断，已由v6更正`next:0`语义：** 本两位置下，正式源观察到OID251在出生51后依次经过2、3、60、61、62、63，然后消失，40tick内没有进入action0；地面转换和DAT静态`3 next:0`不能相互替代。自然出生子门已通过，**自然OID251/action0与同目标双Uj、Unity原Battle Scene及玩家物理键整链仍待**；C053/Q07/总目标开放。正式EXE SHA-256已复核为`336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。仅改Tools探针与证据文档，DAT、图片、Unity生产、Scene和非战斗未改。

以下为身份纠正前OID53的历史结果，不能裁决OID0的自然可达性。原记录状态：`VERIFIED_NEGATIVE_FOR_TWO_INPUT_PROFILES / INPUT_FIELD_CONSUMER_UNRESOLVED`。工具仅使用当前336B44对应playable源码、正式`resources/runtime`与`GameSession28`完整tick；未改正式源码/EXE、DAT、Unity生产、Scene或非战斗。编译当前闭包两次均exit0、无诊断；原件为[首轮](run-a/summary.csv)与[补充轮](run-b/summary.csv)，各例40tick。

| 例 | 输入与抽样 | 可观察首差 | OID251 |
|---|---|---|---|
| `jump_from_zero` | Tobi OID53/action0/Y0，tick1-4跳；tick7末进入212，tick8防+右+攻以phase0抽样 | tick8 `combo_state[0]=4`，无`InputActionAttempt`；tick9实际只记录`native state-4 airborne attack`请求80，tick尾动作83 | 40tick未出生 |
| `controlled_212` | Tobi受控action212/Y-80，tick1-3防+右+攻；tick2以phase0抽样 | tick2 `combo_state[0]=4`，记录普通空中攻击请求80，tick尾动作83；没有`hit_Fa`尝试记录 | 40tick未出生 |

逐tick输入mask、抽样mask/phase、帧事件与`InputActionAttempt`见[普通跳跃CSV](run-b/jump_from_zero-ticks.csv)和[受控212 CSV](run-b/controlled_212-ticks.csv)。`combo_state[0]=4`说明本工具观察到组合状态，**不证明**`hit_Fa:510`在实际输入消费点已被读取；为何没有产生其action尝试仍未知，须追当前playable调用链的真实当前帧/字段与后续覆盖顺序。静态DAT的frame512 OPoint因此未获得自然出生证据，也不能据这两条输入判OID251全局不可达或认定Unity存在差异。

两例均无阳性LFR，所以没有执行正式根EXE回放；原Unity Editor仍未完成新程序集编译，原Battle Scene Play未运行。C053已证的受控OID251/action0双Uj源/根结果保持受控范围；C053/Q07/总目标仍开放。下一只诊断本输入首差，不扩成其它角色或无界按键矩阵。Battle/Menu Scene和GameConfig/ProjectBattleModeConfig四保护文件未被本工具触碰。
