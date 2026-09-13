<!-- CHANGE-RECORD
id: NTSD28-Q05-JOINT-SNAPSHOT-RESTORE-REPLAY-VALIDATION-001
status: VERIFIED
change-kind: LOGAN_SNAPSHOT_REPLAY_VALIDATION
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q05LoganReplayEditorTests.cs
authority: Approved Q05 step5 / Q03 snapshot identity contract; actual Logan content and existing runtime restore/replay/lifecycle contracts.
evidence: artifacts/diagnostics/NTSD28-Q05-JOINT-SNAPSHOT-RESTORE-REPLAY-VALIDATION-001/REPORT.md; final-82-results.xml; SelfCheck-final.result; play3-pass.json; play4-reentry-pass.json
-->

# Q05 真实Logan同版本恢复与回放验收

IN_PROGRESS / VALIDATION_IMPLEMENTATION。准确两个Editor脚本，无生产规则改动。既有Raw exporter只支持冻结3tick capture，继续保留该限制。新增验证专用callback入口复用原ValidateScenario/BuildFrameInputs/临时DataScope/ConfigureWorldAndRoster，用原3tick输入后显式neutral tail扩展到24tick；不改变原Scenario文件或生产输入。使用既有TryConfigureEmptyDiagnosticWorld选择Authority400/MobileExtended，绑定测试World自己的既有BattleLogicReferencePool并启用logic-only materialization，均在实体注册前，不增加正式Runtime manager/关闭阶段。

真实source=当前Logan runtime；内容身份使用catalog.ContentIdentity.CreateLocalValidationSessionIdentity，seed来自scenario、player槽来自实际combatants，stage/fixture身份按scenario文件SHA经明确NTSD28_Q05_SCENARIO_FIXTURE_V1域投影。callback得到driver、规范FrameInputSet数组与identity。两profile/输入移动与攻击场景，在tick2 checkpoint继续至24，改变World后TryRestoreAndReplay逐tick验证retained checksum，再验证最终full checksum与claimed/raw独立2F8等值。不改已知MP200/201规则差异。

对象生命周期用实际OID124(type4)及其DAT action40，通过真实LogicEntityFactory/OPoint任务建立logic-only对象；独立空pool验证真正同一instance回收、槽generation更换和2F8/owner重置，按现有ClearLocalEntityShellsForTransfer测试较早生命周期恢复而不偷改generation。两条OPoint非空拒绝已有guard证据仍复验，不为capture Flush/丢任务。记录实际对象动作/坐标变化/RNG/hash，不以空World数字拷贝替代。

复用现有temporary driver有序shutdown/data后恢复，回调结束后检查旧World对象/claimed slot/logic pool ActiveCount归零；保持用户原GameConfig与源config。先运行有意义的验证，用失败确定实际缺口；若生产恢复有bug，先独立准确Record再修，不能在此验证Record下修改生产。现有旧数据真实Play进出/重进另用已声明probe接线，不用computer-use，不冒称物理键/正式330视觉通过。

验收两profile逐tick replay、新旧identity/版本拒绝、slot/pool复用和局部重入、相关snapshot tests、完整SelfCheck、必要Play和Scene/protected hash。scope内helper/new tests变更允许，Gen/Plugins/Scene/InputActions/资源/外部Server/Unity-GAS架构/非战斗行为保持。新测试对象依附当前World，停止接单和释放遵守既有11阶段，不新造队列；回滚须批准仅精确增量。总目标未完成，Q05未正式关闭，失败/未覆盖项必须记录。

## 初始验证实现

两Editor脚本已写callback bootstrap和6项两profile验证；原3tick exporter限制保持，验证入口显式neutral延伸24tick。初编译发现只读slot view没有Handle便利属性，已按实际RuntimeSlot/Generation创建RuntimeEntityHandle，未修改生产类型。待实际回放与复用结果。

## 首轮实际发现与入口修订

6项中移动场景2profile均PASS；攻击2项在LF2ObjectPool.Get因无Renderer prefab失败。已确认原因是public driver.StepOneTick主动StopDedicatedSimulationWorker并在未seal时重置logic-only materialization，因此不是只设置World flag就能把renderer host当纯逻辑executor。保持production host语义，攻击验证改用既有BattleWorldSimulationTickExecutor.Execute/World restore并逐tick对照，不通过私有反射/逐tick重设flag绕过Host；移动场景继续真实BattleLockstepSession.TryRestoreAndReplay。pool2项恢复checksum/旧generation成功，但active4而World3，退出无法归零；独立SNAPSHOT-RETIRED-SHELL-POOL-RETURN已建立准确Record。父scope仍2Editor脚本，不能把两失败当已验出口。

## 第二轮与证据补强

job 67b4cb29b18a4c3fbac53a33563e1a3d 实际9/9通过（本包6项、pool必要修复3项）；两profile移动/攻击均完成tick2到24恢复后22tick比较，slot generation1→3复用/恢复及退出pool0通过。攻击阶段SpawnCount=1但最大对象数2；源码确认SpawnCount在实际materializer之前计数，因此只证明到达生成入口，不能证明对象成功出生。现复用已有BattleParityStructuralEventBuffer记录事件及RegisterCount差值，补实该边界，尚不把OPoint生命周期写成通过。正在跑恢复/关闭/版本/双队列边界相关回归。禁止computer-use，真实Play和完整SelfCheck待本轮结果。

## 事前补充：同一已声明测试脚本的真实Play探针

在NTSD28Q05LoganReplayEditorTests.cs内新增明确请求文件驱动的Editor探针，默认不运行。观察当前真实战斗Scene tick>=5，暂停后原地capture/改动/restore，验证full checksum、原Renderer引用及对象/pool计数保持；随后调用既有App shutdown owner（或既有driver+BattleBootstrap map owner）完成11阶段，等待两个Unity帧确认Stopped、World/slot/logic pool/renderer pool归零，才退出Play。关闭失败保留Stopping/暂停Scene并报告，不继续Destroy。重复两次Play覆盖重入；此Play明确使用当前Unity旧内容，不认证正式Logan图片、物理输入或全技能。没有新增生产manager、队列或关闭阶段，不改Scene/Prefab。另在真实Logan pool用例中加入不同catalog identity拒绝且World checksum不变，仍使用原两个脚本。

事件探针首次编译遗漏SetStructuralEventSinkForDiagnostics必填tick/pass，已修；45项回归实际使用旧程序集，保留pre-instrumentation-assembly-45-results.xml，不作为新增事件验证。新程序集CS0后重新执行。

## 真实Play发现及第二个必要修复

真实tick5/对象4场景两轮capture成功、restore因WorldConfigurationMismatch拒绝。诊断报告明确core.ObjectCount=4、claimed=2，其余capacity/profile/broadphase/pending前置一致。确认额外两个是LF2ObjectRenderer.OnEnable注册的ISimObject。独立SNAPSHOT-RENDERER-REGISTRY-RETENTION-001已声明三脚本修复活动数/claimed混用与全bucket清空，禁止移除Renderer绕过门槛。当前新鲜82/82回归PASS（新增5含温热零分配），两必要生产修复都在独立Record，父本身仍仅两个Editor脚本。完整SelfCheck及真实Play复验继续，Q05尚未关闭。

首次Play失败后使用现有BattleRuntimeEditorShutdownBridge的ExitingPlayMode→App/Driver有序owner路径退出，未修改Scene或强杀Editor。桥接execute_code因CodeDom命令长度和Roslyn未安装不可用，未安装依赖/改插件；后续诊断使用已声明Editor探针。


## 最终限定出口（2026-09-13）

VERIFIED / SNAPSHOT_REPLAY_SCOPE_ONLY。最终82/82 focused（job6400c6d4e1fd432cbe7fe837231dad20）、完整SelfCheck、两次真实Scene tick5恢复4→4/有序关闭全0/两帧Stopped及重入PASS，Scene旧SHA/root14/dirtyfalse保持，Console0error。完整证据统一在 artifacts/diagnostics/NTSD28-Q05-JOINT-SNAPSHOT-RESTORE-REPLAY-VALIDATION-001/REPORT.md；失败及RED保留，不以旧程序集45项代替最终结果。独立pool归还与Renderer注册保留两个修复按各自Record集成；Q05来源/schema/restore出口满足，BATCH-02限定交付。Q06消费者、Q07正式资源、六MISSING/MP首差、后续视听与整场终验保持未完成。禁止computer-use，总目标ACTIVE。
