# NTSD-OPTIMIZATION-BATCH21-FOOT-AUTHORING-20261007
状态 SCOPED_FOOT_AUTHORING_DIAGNOSIS_PASS；Change NTSD-OPT-M03-FOOT-AUTHORING-021 / VERIFIED（仅Editor诊断，不是生产修复或父项关闭）。以下事前范围保留，实际结果追加。
用户要求开始下一批；依总表第20批指定下一步，只定位生产Foot覆盖前置，不擅自修复配置或降低门槛。

## 原状、范围和不变量
第20批原Battle1800相机帧Foot均0，严格window FAIL/replay NOT_RUN；静态链要求loaded BattleCentralEditorPreview，savedBattle无该GUID，GameConfig已有六帧引用。仅线索，运行时原因未闭合。
原6401 Editor2022.3.62f3/Menu clean8roots/idle/nonPlay/errorCS0。HEAD2cccd597dbf6cb32826eb0b6e98d7462c1eac48d，staged空；既有dirty保护。
唯一正式336B44 authority保持。33ms/3ms/max2、输入/pass/RNG/checksum/World、有序11阶段关闭不改；不读活跃Q06 body；排序/Self生成/first-visible语义不裁决。
EXT1 PROPOSED/MODIFY_REQUIRED无专项M0/instancing；MONO USER_HOLD、PERF/ATLAS正文、bank/budget/format/segment/fail-closed不改。Scene/Prefab/PNG/meta/importer/Settings/InputActions/Server不改。

## 准确文件与方法
- 修改 Assets/NTSD/Scripts/Test/Editor/BattleCentralProductionWindowSceneProbeEditor.cs：新增Batch21入口与report diagnostic字段；启动时只读loaded authoring、GameConfig引用、runtimeFoot字段；已物化command的Self标志逐camera计数。64个distinct WorldCamera Unity帧自然观察，不注入输入，不强制tick，不运行catalog replay。复用旧关闭/SceneSHA/新文件CreateNew；旧所有入口/验收条件不变。
- 新增 Assets/NTSD/Scripts/Test/Editor/BattleFootCoverageDiagnosticEditorTests.cs/.meta：具名纯分类policy测试，authoring/资源/Self/实际Foot分别报告，0Foot不得分类为活动覆盖通过。
- 修改七治理docs：总表、风险表、M03、handoff、STATE、Ledger、FileOperationsIndex；准确8现存文件先备份，733既有保护，前批冻结741路径已验证。
- 新Task/Change/Operation及 artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH21-FOOT-AUTHORING-20261007/。不改第20批Record或失败原件。

## 风险、验收与关闭
Editor诊断启动Resources/反射/AssetDatabase会分配，置于观察前；观察后只读计数不回写状态。不是性能收益测量，不把64低roster窗口替代1800/高负载/0GC证书。
即使诊断PASS也只表示证据采集与关闭合格；明确Foot实际覆盖未满足。若Self标志0，不读Q06 body补全原因；如需Scene或新生产fallback，先声明另立准确授权，不能为了测试通过创建Preview/改开关。
复用原有序关闭，worker Join hard gate和三残留0后退出；失败停Stopping不强行退Play。原Menu干净才开savedBattle；不保存Scene，结束恢复Menu。
原Editorcompile、具名纯分类+旧61回归，实际64distinct camera、loadedPreview/runtimeEnabled/Sprite/frames/config/Self/Foot证据、两slot/CPUlease及SceneSHA/关闭。Ledger/diffcheck/链接/733保护/8备份/HEAD/staged核验。
新模块仅Editor观察容器，不接正式runtime queue/worker/cache；结束解除临时reader/引用，不销毁共享项目资源。

## 回滚
仅用户批准后恢复本Operation准确8份before；新增文件另行具名审计。不reset/clean/restore/删除其它用户工作。

## 实际结果
新测试实际9 RED失败；首次未导入调度0 selected不算PASS。完整刷新后新9＋旧61=70/70，原Editor CS错误0。原savedBattle64 distinct camera/tick8→63、每帧Self2/Foot0/Health2、loadedPreview0、authoring false、runtimeEnabled false/Sprite空/frames0；GameConfig实际六帧Sprite/texture有效、80ms。因配置读取链未接通而无Foot输出，不是资源丢失或Self标志未观察到；只读诊断范围闭合，不读取Q06 body解释其生成/排序。
64 entityBuild/45056 entity vertex bytes、320实际CPU DrawMesh，两slot/CPUlease0。camera/observer各0B，但startup/end诊断会分配、Editor两fullGC硬门false，仅2实体短窗，不能推为fullchain0GC/性能收益。11阶段关闭objects/slots/borrowers0，Scene clean/SHA同，原PID19040/6401/Menu8roots idle已恢复。
无生产fallback/配置开关/Scene组件修复，第20批原严格FAIL和NOT_RUN保持；下一须独立明确生产authoring入口/开关/样式归属，不为测试通过自动补组件。
执行期HEAD2cccd597→8107196b发生外部提交（含本批中间文件）；本任务没有执行add/commit/push。733保护SHA和8备份均保持，staged空；不把HEAD写成未变，不归因特定执行者、不回退并发工作。最终冻结详见REPORT及Operation after。
