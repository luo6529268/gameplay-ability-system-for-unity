<!-- CHANGE-RECORD
id: NTSD28-Q05-MASS-OSCILLATE-SHELL-CARRIER-RETIREMENT-001
status: FOCUSED_TEST_PASS
change-kind: RETIRED_MASS_OSCILLATE_SHELL_AND_CONTEXT_REMOVAL
code-path: Assets/NTSD/Scripts/Animation/Character/CharacterMechanics.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2LivingObject.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDSpec.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterFrameAdvancePass.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldCharacterShellSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldEntityBaseShellSnapshot.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleWorldCharacterShellSnapshotEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleWorldEntityBaseShellSnapshotEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/FrameAdvanceRuntimeSnapshotEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4State1218ContactActionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4Type0PhysicsCoreEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6NtsdSpecDeadFluteApiRetirementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B9NtsdSpecOscillateProducerRetirementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q04MassFrictionGateEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q04MassFrictionPlayProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q04OscillateConsumerEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q04OscillatePlayProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q05MassOscillateCarrierEditorTests.cs
authority: Q03 frozen JOINT-FIELD-MATRIX and VERSION-IDENTITY-AND-CAPTURE-CONTRACT; Q04 verified mass friction gate and Oscillate producer/reader retirement against current formal Logan B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 live path.
evidence: RED_13_FAIL_1_PASS / FOCUSED_890_PASS / SELFCHECK_PASS / SCOPED_PLAY_PASS / JOINT_SCHEMA_PENDING
-->

# Q05 Mass/Oscillate载体和shell退休

准确21脚本：8生产、12既有测试/探针、1新测试。父Q05步骤2，不重做已验证的行为退休。所有现有未提交变化保留；无文件删除/移动、资源、Scene、UI、Gen/Plugins/外部Server改动。

原状及完整调用：CharacterMechanicsContext只保存mass，Step已不读取；LF2Character._mass及MassForFrameAdvance用于初始化/构造/ECS/shell capture-restore，LF2Entity另造NTSDGlobal mass作同参数。20处new Context调用（3生产/17测试）应统一移除第4参，保留runtime/frame/width/minSpeed/gravity。NTSDSpec.GetMassOrDefault是唯一生产NTSDSpec查询；全Assets/Tools及外部Server packages搜索未见其他目标接口消费。删除SpecEntry Mass/Oscillate、构造参数和对应查询/表中数值，仅这些字段；其余历史兼容表/API及NTSDGlobal独立常量保持，不删除文件或制造空类。

LF2EffectState.Oscillate/OscillateDirection及Reset、EntityBaseShell capture/属性与LF2Entity.restore删除；其余Num/Dvx/Dvy/Stuck/Blink/Super/TimeIn/TimeOut/BlinkCounter与Sprite偏移、真实TrackerParent等保持。CharacterShell仅删除Mass，HeldWeaponHandle/DeadBlink/OPoint flags不变。

测试迁移：先新absence/Context shape RED。旧CharacterShell/BaseShell assertions移除退休字段并保留其余完整快照/关系/不可变断言。Q04 mass见证8条保留，新的无mass构造；真实Character/ECS和Play probe改用0/100/150三个对象ID以验证原静态表不再影响运动，不重复三个已不存在的mass输入。旧Oscillate测试用Sprite初始偏移（7/-3/0）覆盖偏移保持及Blink/Timeout/DeferredMotion，不再写人工振幅；旧producer测试保留effect优先级、Num/速度/计时等有效payload，去除只重复不同退休sentinel的用例，改用字段不存在证明。旧历史artifact不改，当前probe输出明确已无mass/振幅注入。

验收：新字段不存在与五参Context、有效Effect Reset；Q04原版physics.tsv及Character/ECS行为、effect producer/consumer及旧DeadFlute其它API退休、两shell/capture-restore和warm no allocation；Unity compile/完整SelfCheck/准确ledger；必要Play probe通过桥接执行并保留限制。CurrentSchemaVersion数字本包暂保持1/1，与父步骤4统一2/2及entity13/aggregate21/checksum24，禁止发布中间baseline/跨版本恢复或Q07资源部署。旧版本拒绝/trace/新回放/Play最终门槛不在本包假关闭。

风险：构造参数位置错位、shell有效字段误删、把旧测试保留carrier的断言当正式行为。逐调用记录参数移除、focused与完整自检验证，行为规则本身不改。无新增runtime owner/queue/worker，十一阶段关闭保持。Q06各新consumer与五reserved清理/identity/双OPoint guard后继保持。

用户禁止computer-use，只桥接/日志/结果/进程。保持Unity/GAS与非战斗、33ms/3ms、stage.dat USER_HOLD、所有例外、Foot任务外18删除和blue/red/yellow目录及Scene旧SHA/精度差异。回滚须批准，仅准确差量，不恢复其他用户内容。

## 实施已写

RED14：13FAIL/1PASS，absence/五参构造缺失，真实Effect Reset已有PASS。准确21脚本已写，20处Context调用逐项删除mass第4参（constructor-argument-removals.json）；清Mass/Oscillate保存/初始化/shell/copy/reset及Spec字段/查询。历史compat表其它API保持。Q04当前movement测试为1 native8行+6对象ID/两caller，Oscillate测试改用真实偏移；Producer重复退休sentinel消除，效果Num/速度/优先级/计时断言保持。旧Q04 Play probe已适配但未新运行，规则核心函数将核对与preimages一致。版本未发布，compile/focused/SelfCheck待。

## focused与Play探针边界更正

联合890/890 PASS，三核心规则方法主体与事前文本一致；完整SelfCheck09:16:30Z PASS。Play效果探针PASS（CentralOnly，实际production snapshot验证，未绑定SpriteRenderer所以不声称组件绘制检查），cleanup通过。首轮运动Play探针ID100整tick X506而预期505，Vx4一致；纯Dynamics多ID6例已通过，当前不能把整tick差异归因于mass删除。保留play-first-id-specific.json，未改production；只把已声明Play probe改为同ID0的forward/reverse/stationary三实际速度向量，避免新引入跨ID整tick相等假设，随后相同landing输入比较。待重载重进Play验证。

## 限定出口

FOCUSED_TEST_PASS / SCOPED_PLAY_PASS / JOINT_SCHEMA_PENDING。890定向/SelfCheck/实际driver运动和CentralOnly效果snapshot probe均通过；实际检查及未执行组件/自然输入范围见artifact REPORT。生产目标引用仅保留无消费的NTSDGlobal Mass常量，三核心行为函数主体未改。Scene旧SHA/Foot任务外状态保持。下一NTSD28-Q05-FIVE-RESERVED-CARRIER-RETIREMENT-001；同Q05联合版本尚未发布。

最终Tools/Validate-ChangeLedger.ps1 -RepositoryRoot $PWD.Path：PASSED，491 Records / 124 governed code files，ledger-final.txt已保存。
