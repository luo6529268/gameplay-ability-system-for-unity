# R3-COMBO-001 — staggered combo-state persistence

<!-- CHANGE-RECORD
id: R3-COMBO-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleCharacterInputActionResolver.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/CharacterInputLiveSlotLoopEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleComboPlayModeProbeEditor.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\include\input_handler.h:9-16; src/input/input_handler.cpp:1555-1609,2758-2859; Makefile:35
evidence: C++ by-reference combo fields vs Unity local transaction early-return discard; user staggered combo failure
-->

## 1. Authority / requirement source

C++ release `advance_combo/run_combo`直接接收entity九个combo字段引用。用户当前Play Mode报告组合键无法释放
技能；source对照已定位Unity局部事务未写回这一确定差异。

## 2. Unity original state

Unity把九个combo字段复制为局部变量，只在DJA极窄fallthrough写回。普通未完成wrapper、valid/failed DJA、
guard、Unk328等return均丢弃局部进度。现有self-check主动要求该错误行为，并把分tickNaruto L/S/K写成
negative expectation。

## 3. Planned paths / symbols

- `BattleCharacterInputActionResolver.ApplyComboFrameInput`
- `BattleRuntimeSelfCheck.CheckComboLocalShadowCommitContracts`
- `BattleRuntimeSelfCheck.CheckStaggeredNarutoDefendDownJumpInput`
- `CharacterInputLiveSlotLoopEditorTests.RegisteredAiActionResolver_ConsumesAndCommitsRuntimeProgressDirectly`
- `BattleComboPlayModeProbeEditor`（Editor-only、显式菜单触发、真实场景InputBuffer probe）
- focused Editor combo persistence fixture（若新增）

## 4. Intended before / after responsibility

- before：local transaction决定是否commit九combo字段；
- after：每个wrapper直接拥有对应`input.Combo*`字段的即时source-equivalent mutation；
- physical input、cooldown edge生产、frame packet、worker和DAT保持不变。

## 5. Non-regression / protected boundaries

- wrapper调用顺序、direct `hit_a/hit_d/hit_j`顺序、DJA guard/Unk328字段副作用不变；
- CentralOnly、1.5 scale、30Hz、FrameInputSet、worker、SoA/ECS、pool、0-GC不回退；
- 不修改C++ authority、DAT、scene、T8或Android；
- 保留脏工作树所有其他用户/R2～R8改动。

## 6. Acceptance criteria

- 九combo跨tick1→2→3和interrupt矩阵与C++ source一致；
- Naruto L→S→K synthetic + real Play Mode能触发对应DAT frame；
- same-tick组合保持；
- guard/missing/Unk328/valid DJA状态按C++ ref mutation保持；
- compile/self-check/EditMode/Play Mode/validator分别通过。

## 7. Rollback

失败时只回退本Change ID的resolver/test增量并保留失败证据；不得恢复陈旧测试来掩盖C++差异，也不得
回退其他工作树内容。

## 8. Current state

`CODE_WRITTEN`。`B-R3-COMBO-001-01`已于2026-08-23由用户明确回复“同意修改，继续处理”解除。

实际代码增量：

- `BattleCharacterInputActionResolver.ApplyComboFrameInput`删除九字段局部事务，八个`RunCombo`与DJA
  `AdvanceCombo`直接接收`ref input.Combo*`；所有early-return自然保留当时字段状态；
- `CheckComboLocalShadowCommitContracts`按C++ source重算fresh defend、interrupt、missing target、Unk328和
  normal DJA期望；
- `CheckStaggeredNarutoDefendDownJumpInput`改为物理L→S→K跨tick 1→2→触发frame105并清零DDJ。

编译证据：Unity 2022.3.62f3于2026-08-23执行forced refresh + script compilation；
`Assembly-CSharp.dll`更新时间晚于两个目标源码，Editor log记录`Tundra build success (4.72 seconds)`，目标
编译0 error。日志仍有既存`TimeWheel` nullable及两个unused-field warnings，不属于本Change。

首次full self-check于08:08:18真实失败在`CheckOid5152DjaReleaseTriggersSameTickSplit`：旧fixture仍要求
missing/valid target DJA early return丢弃local clear。按C++ `input_handler.cpp:2847-2858`，trigger path在
`do_frame_jump`后无条件清零`combo_DJA`；现已把missing/valid target两条断言改为private/runtime均为0。
追加测试修改已由fresh Tundra 2.28s重新编译，目标0 error，DLL时间晚于源码。full self-check复跑与
Play Mode仍待；当前不能报告为行为通过或已对齐。

第二次full self-check于08:10:17真实失败在`CheckOid6DjaGuardComboHold`的旧transaction-discard断言。
C++ `input_handler.cpp:2826-2846`顺序要求前八wrapper先将ordinary 1因fresh attack打断为0，DJA 2推进为3；
guard直接退出而不清DJA。已把guard期望改为ordinary0/DJA3，并同步把同函数successful release按
C++ trigger path改为ordinary0/DJA0。该追加修改已由fresh Tundra 2.47s、目标0 error重新编译；full
self-check再次复跑待验。

第三次full self-check于08:12:09真实失败在`CheckFrameSnapshotHumanInputPolling`右侧partial组合旧断言，
实际`frame102/comboDrj1/cooldowns0`符合C++ wrapper先写step1、随后direct hit_d清cooldown的顺序。静态搜索
确认同组仅right/left两条仍含“must not commit/DJA fallthrough”陈旧措辞；两条均已改为combo1，后续tick的
fresh jump会按C++ interrupt规则清掉未完成组合，原`frame != 240`断言保留。需重新编译复跑。

right/left partial修正后fresh Tundra 2.06s、目标0 error、DLL晚于源码；full self-check再次复跑待验。

第四次full self-check于2026-08-23 08:14:02返回`PASS`。该fresh结果晚于resolver与最终测试源码以及
2.06s Tundra DLL，实际执行本包的W01、missing/valid target、Unk328、oid6 guard/release、right/left
partial、same-tick与Naruto L→S→K断言。状态提升为`FOCUSED_TEST_PASS`；相关EditMode input regression与
真实Play Mode仍待，C++ full trace仍BLOCKED。

随后EditMode输入回归job `ab3e2977fee04f888730e1f44464c443`完成47个目标测试并有1个FAIL：
`RegisteredAiActionResolver_ConsumesAndCommitsRuntimeProgressDirectly`仍期望`ComboDra=2`，实际为3。该fixture
中CdAttack=5把DRA step2推进到3，随后direct hit_a跳frame1并清cooldown，但不得回滚已推进DRA；已把断言
改为3。该追加测试修改已由fresh Tundra 1.49s重新编译到`Assembly-CSharp-Editor.dll`，目标0 error；
同一EditMode矩阵复跑job `135495e273a646539f7b42eca9b8611b`为47/47 PASS、0 failed/skipped；
覆盖`CharacterInputLiveSlotLoopEditorTests`、`StrictDelayedInputBufferEditorTests`与
`LocalFrameInputProviderEditorTests`，包含crossed physical mapping、canonical packet、resolver commit与
warmed no-allocation回归。真实Play Mode仍待。

Play Mode preflight已确认`NTSD_Battle`实际启动、bootstrap完成并生成两个character id2；UnityMCP动态
`execute_code`因CodeDom引用命令行过长且Roslyn不可用，不能读取纯C#实体状态。为获得可审计的真实场景
tick证据，已在修改前将Editor-only `BattleComboPlayModeProbeEditor.cs`纳入本Record；该探针只在显式菜单
触发时向第一个真实场景角色现有InputBuffer排入L→S→K并写Temp结果，不进入生产热路径或修改gameplay。

Play probe代码已写入：显式菜单`NTSD/验证/运行组合键PlayMode探针`在Play中反射读取bootstrap已创建的
first player，按未来连续逻辑tick排入internal att/down/def（物理L/S/K语义），记录DDJ、cooldown、frame与
world object count到`Temp/NTSD_R3_COMBO_PLAY.result.json`。探针未自动运行、不会进入正式tick pass；
fresh Tundra 3.62s已成功编译到`Assembly-CSharp-Editor.dll`，目标0 error、DLL晚于源码；尚未运行。

首次real-scene probe于08:28:20返回FAIL：预排tick314/315/316期间DDJ及所有cooldown始终为0，证明直接
写`SimInputBuffer`的探针事件没有进入当前canonical `FrameInputSet`。`LocalSimulationFrameInputProvider`
以controller held state构造packet并在BeforeSimTick丢弃direct tick，因此这是probe绕过生产输入源的测试
方法错误，不是gameplay回归证据。下一版改为向Input System `Keyboard`排L/S/K设备状态，让现有action
callback→held state→FrameInputSet→InputBuffer→resolver完整生产链消费。

probe已改为Input System device-state状态机：先排`Key.L`，观察DDJ=1后排`Key.S`，观察DDJ=2后排
`Key.K`，观察authored `hit_Dj` frame后释放全部键并继续记录18 tick。直接SimInputBuffer预排逻辑已删除；
fresh Tundra 1.75s已重新编译到Assembly-CSharp-Editor，目标0 error；尚未运行device probe。

第二次real-scene device probe于08:32:12返回PASS，并经过完整Input System/action/canonical packet链：

- tick613：physical L → DDJ1 / CdDefend5；
- tick614：physical S → DDJ2 / CdDown5；
- tick615：physical K → DDJ3 / CdJump5；
- tick626：进入当前Naruto DAT authored `hit_Dj=271`并清DDJ；
- 后续271→272→273→274，world object count从8升至peak20，证明opoint/lifecycle继续执行。

证据文件：`Temp/NTSD_R3_COMBO_PLAY.result.json`。该probe通过是real-scene synthetic Input System device
event，不冒充用户手指按实体键盘，但已覆盖production callback至技能帧链。当前继续把同一probe泛化并补跑
physical L→facing direction→J，以关闭八方向ordinary wrapper的Play证据；尚未改脚本。

probe泛化已写：保留原L/S/K menu/result，并新增`运行组合键PlayMode探针-防前攻`与独立forward-attack
result；第二步按角色当前朝向选择A/D，第三步排physical J，观察DLA/DRA与current frame authored
`hit_Fa`。trace同时记录DDJ/DRA/DLA及action/direction cooldown。尚未重新编译/运行。

generic forward probe fresh Tundra 1.18s、目标0 error、Assembly-CSharp-Editor晚于源码；尚未运行。

forward real-scene probe于08:35:39返回PASS：

- 当前朝向right，device sequence为physical L→D→J；
- tick496/497/498为DRA1/2/3与CdDefend5/CdRight5/CdAttack5；
- tick509进入current Naruto DAT authored `hit_Fa=263`并清DRA；
- 后续263→264→283→284，object count 7→8。

证据文件：`Temp/NTSD_R3_COMBO_PLAY.forward-attack.result.json`。两组production Input System real-scene probe
均通过；状态暂为`RUNTIME_PENDING`，等待final fresh self-check、ledger validator和scoped diff后再裁决本包。

final evidence：

- final full `BattleRuntimeSelfCheck`：2026-08-23 08:37:09 `PASS`；
- input EditMode regression：job `135495e273a646539f7b42eca9b8611b`，47/47 PASS；
- resolver/test/probe fresh compile：Tundra 0 error（final Editor probe 1.18s）；
- DDJ production-chain Play：L/S/K→DDJ1/2/3→frame271，objects8→20；
- DRA production-chain Play：L/D/J→DRA1/2/3→frame263，objects7→8；
- Change ledger validator：PASS，58 Records / 58 governed code files；
- scoped `git diff --check`：PASS（仅LF/CRLF提示）。

`R3-COMBO-001`状态为`VERIFIED`，仅裁决D-INP-010的C++ source-derived by-ref combo persistence与Unity
production-chain行为。R1-WP02 full C++ runtime trace继续BLOCKED；用户实体键盘/窗口焦点physical edge仍归
D-INP-006/R8人工复核，不能据此扩大为全部输入或全部战斗逻辑已对齐。
