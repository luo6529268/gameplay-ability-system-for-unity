# NTSD28-B2-NATIVE-COMBO-ACTION-TRANSACTION-001 — native combo action transaction

<!-- CHANGE-RECORD
id: NTSD28-B2-NATIVE-COMBO-ACTION-TRANSACTION-001
status: FOCUSED_TEST_PASS
change-kind: CODE_AND_TEST
code-path: Assets/NTSD/Scripts/Simulation/Input/NTSD28InputTwoPassModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Input/NTSD28NativeInputPreprocessor.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCharacterActionWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Core/BattleCharacterInputActionResolver.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeComboActionTransactionEditorTests.cs
authority: NTSD 2.8-Logan input_routing.cpp current-remap/bound preprocessor, route_combo_fields and FUN_0040D900 action transaction; input_routing_tests.cpp called-by-main matrix.
evidence: TASK-CONTRACT-CREATED / SOURCE-CHAIN-CLOSED / PLAYABLE-BUILD-CLOSURE-CONFIRMED / AUTHORITY-INPUT-ROUTING-46-DEFINITIONS-46-MAIN-CALLS-NO-MISSING-OR-EXTRA / AUTHORITY-INPUT-ROUTING-FRESH-BINARY-PASS / CARRIER-DEPENDENCY-PASS / TEST-FIRST-COMPILE-RED-65-EXPECTED / PREPROCESSOR-WRITTEN / GENERIC-ACTION-TRANSACTION-WRITTEN / EXACT-COMBO-PRODUCTION-CALLER-WRITTEN / ROSLYN-GENERATED-PROJECT-COMPILE-0 / UNITY-EDITOR-COMPILE-0 / FOCUSED-1AD4B0-18-OF-18 / B2-BROAD-21FD6D-261-OF-261 / B2-RAW-CAPTURE-SUPERSET-BA2674-267-OF-267 / SNAPSHOT-CHECKSUM-ADB87C-31-OF-31 / FULL-SELFCHECK-2026-09-04T14-31-34-PASS / EXPECTED-NEGATIVE-REST-LOGS-7-CLEARED / CONSOLE-ERROR-0 / DIFF-CHECK-PASS / LEDGER-132-84-PASS / DIRECT-HOLD-DIRECTION-DEFERRED / TYPE0-BUILTINS-DEFERRED / CONFIG-DAT-SCENE-UNCHANGED-BY-PACKAGE / JOINT-TRACE-PENDING
-->

> 状态：`FOCUSED_TEST_PASS / COMBO_ACTION_TRANSACTION_READY / PRODUCTION_CONNECTED / JOINT_TRACE_PENDING`

## 改前事实

- production second pass已推进exact edge/history/combo并投影legacy，但selector没有production caller。
- current remap与bound jump carrier存在，consumer尚未接入edge之前。
- compatibility frame jump只实现部分signed/999与旧PP费用，缺source-state redirect、完整MP/HP、fallback、
  last-action、exact attempt和`hit_ja`特殊族合同。

## 预期改后职责

- native preprocessor在edge前无分配执行current remap与bound jump suppression。
- `BattleCharacterActionWriter`成为generic native input action transaction唯一Unity mutation boundary。
- exact combo只在第二遍路由一次，成功/拒绝/零field/特殊族的consume与counter语义均可观测。
- 后续three-button/hold/direction与type-0 built-ins仍保持显式未实现状态。

## 验证记录

- Task Contract已创建；authority source、46-test归属与carrier依赖已闭合。
- test-first新增14项contract；Unity refresh后Console捕获65条预期编译错误，均为preprocessor、
  action attempt/result或writer API尚不存在。该红灯在production修改前取得。
- 已写无分配current remap/bound preprocessor；`BattleCharacterActionWriter`新增锁、signed/999、
  encoded-state redirect、source-cost、recmp/mode/double/waiver/F6、fallback、last-action与facing事务。
- exact combo production caller已接到DataOriented第二遍的edge/combo之后、legacy projection之前；
  horizontal facing、reapply counter、attempt clear和`hit_ja` 300/linked/gate328尾已写。Legacy不接。
- 尚待Unity compile、focused/broad/SelfCheck与治理验证；direct/hold/direction、built-ins和joint trace仍待。
- 首轮实现focused job `3634e136008d47569c790aecde282e46`执行14项、12项通过、2项失败：
  `FrameCache.GetFrameDataById`对不存在ID返回fallback frame，导致missing source未拒绝且未定义fallback
  未把`Frame.D`置空。修正为先以`HasFrame`判定真实定义；这是Unity adapter存在性修正，不改native规则。
- 修正后job `7da96172579445dcb233a5878521e416`为14/14。生产串联审阅随后新增native
  non-consumption不得被legacy combo重入的第15项；job `1fa8745e3ec2486ab2ec789d65d3f7f2`
  捕获1/15预期失败：special reject后legacy Dja仍跳到360。现只在native selector已选择terminal时清
  legacy combo投影，exact未消费状态保持不变，防止双resolver所有权。
- 上述“仅清runtime legacy投影”实现后job `5bd9eae12c5a44b79d97b3b0c6bb549f`仍为
  14/15，证明实际问题是`LF2Character.ComboUpdate`继续调用legacy resolver，而不是runtime镜像本身。
  最终修正改为DataOriented在共享`BattleCharacterInputActionResolver`跳过legacy combo owner、继续
  direct/release；撤销临时清镜像方案，使legacy mirror仍可观察但无生产权。
- 2026-09-04重新逐行核对正式playable build闭包中的`input_routing.cpp:317-428,446-522,1511-1574`：
  source-frame存在性、encoded-state重定向但保留source费用、`requested<1`朝向翻转、combo attempt清理、
  remap/bound与combo→three-button→direction→built-ins顺序均支持当前实现边界；本包仍不声明后三域完成。
- 最新所有权修正后运行`dotnet build Assembly-CSharp-Editor.csproj --no-restore --verbosity:minimal`：
  exit 0、0 error。初次增量输出22个既有程序集版本冲突warning；新增coverage hardening触发更广重编后
  输出94个既有程序集版本/fixture DTO `CS0649` warning，目标测试文件无新增诊断。该结果只记为Unity
  生成`.csproj`的Roslyn编译通过，
  不冒充Unity Editor脚本编译或focused test。
- gameplay项目当前没有活动Unity Editor：其`Library/EditorInstance.json`仍指向已失效PID 29796；当前
  PID 53488明确属于`I:\UnityPreject\SkillEditor`，不得借用。先前gameplay batchmode因sandbox身份无法
  连接用户会话中的Unity Licensing IPC，60秒后return 199且未生成test XML；离线reflection runner又
  在NUnit/Unity runtime启动前失败，二者都不计测试失败或通过。最新18/18、B2 broad、action/lockstep、
  SelfCheck与Console0继续等待gameplay项目由桌面用户身份打开并退出Play。
- fresh authority test：以正式`ntsd28_core` source与build脚本相同的27个translation units/flags编译
  `input_routing_tests.cpp`，唯一输出位于工作区
  `Temp/NTSD28AuthorityInputRoutingEvidence/input_routing_tests.exe`，SHA-256
  `C238A7F4958568B1E3810587ACBE469798D1D634750CBB557103608B3C3F98F7`。静态计数为46 definitions、
  46 main calls、unique 46/46、missing 0、extra 0。首次从Unity仓库cwd运行因相对路径
  `reextracted_v2_需要保留`不可见而报`locked input-trace catalog load failed`；随后只把工作目录切到
  该只读夹具的父目录重新运行，exit 0、`input_routing_tests: PASS`。两次均未向authority目录写入；
  首次结果属于夹具cwd前置条件失败，不是规则断言失败。
- Unity focused从15项加固到18项：新增duplicate remap后写覆盖、undefined redirect raw target/source cost、
  `mp==0`时hp-only不收费与HP等于费用时严格拒绝；既有reject测试新增counter保留断言，`hit_ja`
  special-family新增requested 0且`Unk328==1`的消费/清timer/counter尾。尚未在Unity Editor执行，
  只获得上述Roslyn 0 error，不能计为18/18通过。
- 后续gameplay Unity由桌面身份以PID 51752打开；`EditorInstance.json`、Unity command line与active scene
  `Assets/NTSD/Scene/NTSD_Battle.unity`三方确认目标实例，独立bridge为127.0.0.1:6400（SkillEditor
  仍为6402）。初始Asset Refresh后Tundra build success，两个项目脚本assembly均生成且无C# error。
- final focused job `1ad4b0b855d348cc8626a89378b4675e`：18 total / 18 passed / 0 failed /
  0 skipped，duration 0.7187824s；覆盖新增hardening与native non-consumption不回落legacy combo。
- broad过滤纠正留痕：`991fc6d7b04e4e4b8bb9df04fd13f3b9`只覆盖`NTSD.Test`及四个相邻类，
  180/180通过但不计完整B2 broad；补入`NTSD.Simulation.Tests`以及B0 raw-capture namespace的
  超集job `ba267403a2db4c2da7226853dcbcba20`为267/267；最终移除不属于B2的9项raw-capture，
  精确六组job `21fd6d75fdc74ac585ba076411d61661`为261/261 PASS，0 failed/skipped。
- snapshot/checksum/lockstep job `adb87cbf590f4deb9481fc1372a0679e`：31/31 PASS，0 failed/skipped，
  覆盖runtime snapshot、restore、checksum与两个lockstep ring。
- full `BattleRuntimeSelfCheck`请求由Editor消费，结果文件2026-09-04 14:31:34为`PASS`；随后Console
  error/exception/assert读取到7条已知负向rest-binding防御日志，清除后fresh读取为0项。
- `git diff --check`与`Tools/Validate-ChangeLedger.ps1`最终通过（132 records / 84 governed code files）。
  本包未修改Config/DAT、Scene/Prefab、ProjectSettings或authority；工作树中的Scene差异为既有用户状态。
  本包只闭合combo/generic action transaction，不声称direct/hold/direction、type-0 built-ins、B2 joint
  trace或全角色action parity完成。
