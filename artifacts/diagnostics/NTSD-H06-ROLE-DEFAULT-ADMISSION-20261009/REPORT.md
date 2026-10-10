# H06 Role 限定生产准入与默认接线

结论：ROLE_SCOPED_ADMISSION_COMPLETED。现有Role已成为空配置生产默认；H06父项/H07/Android仍OPEN，未运行1800正式证书。
Task/Operation NTSD-H06-ROLE-DEFAULT-ADMISSION-20261009；窗口Change NTSD-H06-ROLE-ADMISSION-WINDOWS-001；生产Change NTSD-H06-ROLE-PRODUCTION-DEFAULT-001。

## 改动与适用范围

BattleRuntimeProfile.cs::CollisionBroadphaseBackendResolver.Resolve 在原合法override→config解析后，仅为两者均null/空字符串返回LooseQuadtree；非空非法值（包括空白）继续Brute安全回退。GameConfig.cs仅collision说明更新，资产未改。显式Brute、显式Role及命令行优先级不变，SimulationWorld测试默认构造仍Brute。无新runtime状态/缓存/owner/容量，无十一阶段关闭职责变化。
Suite仅增加具名冷入口/新输出根/一枚持久模式字段；原观察器/GC校准/窗口标准/owner关闭复用。Editor状态容量1，由原Suite终态清Session、冷标记finally清零；无新战斗模块。新增测试覆盖实际ProductionSource→Driver.CreateProductionWorld/RecreateWorld→World→query，不只检查字符串。

## 当前同配置实测

原Editor2022.3.62f3/PID74724/MCP6402，一次菜单 NTSD/Validation/Optimization/H06 Scoped Role Admission。两布局各Brute/Role、每窗120warm+180sample，Sakura OID1，actual AI/base roster均1000；GameConfig实际DataOrientedCanonical、collision空，与窗口AI一致，没有推广AI模式。共享绑定与其它flags固定，配对request仅formalCollectorMode/outputPath不同，源码期间SHA冻结不变。

| 布局/collector | tick均值ms | tick P95ms | collector均值ms | dropped debt |
|---|---:|---:|---:|---:|
| 00-dispersed1000-brute | 85.959984 | 105.608110 | 55.391590 | 811 |
| 01-dispersed1000-role | 46.550398 | 54.416385 | 15.658797 | 308 |
| 02-combat1000-brute | 91.312252 | 113.179990 | 59.901228 | 781 |
| 03-combat1000-role | 49.844037 | 94.255555 | 17.485331 | 328 |

Role两布局collector和整tick均有明确收益；混战Role P95有较大波动，单轮顺序窗口不能承诺固定收益/全部均值差因果，也没有把均值换算为P95或相加父子耗时。P95与drop仍未达原H07最终门，因此不开1800刷证书。D1=A允许此限定准入，不以未33ms阻止本项。

四窗完整同步主线程StepOneTick前后正分配/空scope校准有效：accepted300、steady180、nonsteady120、invalid/rejected/sequence0、steady events0、coverageValid/zeroAllocationPassed true。非稳态存在事件，不能声称全300tick或camera/GPU/worker0GC。容量criticalDelta0、capacityPressure通过。实际collector采样观察1/3符合配置、无mismatch；Role保守fallback峰值4/3、aborted ticks0，不宣称全程无fallback或一直走tree。

同布局末tick300完整checksum文件字节相同，SHA分散F1A2CAC863D1AA995D49BEF7DA2D5C1B30337465FA846DB0CEFAF86D8AD7B6AA、混战E4916ACA8C5BE675B8192D5A6C1429029FD94DB6F838CF6C82E532B030836B12。末态不代替逐tick证明；四场景32tick逐tick输入/候选序/双RNG/checksum使用既有具名证据，依赖指纹核对详reuse-audit.json。旧25与82不互充全域。FindEffect局部与千人修复复用，不重做；91/93与95证据各守scope。

四窗StoppedCleanly、workloadValid、teardownRestored，objects/entities/slots/active pool/reference borrowers及cleanup exceptions零；suite MEASUREMENTS_COMPLETED、completed4、error空、orderedShutdown、双Scene恢复/SHA通过。settings前后显式测试一致。Role/Brute/Role实际生产重建三轮也验证完整关闭及停止后禁止tick。

## 实际命令与测试

- MCP manage_asset import准确Suite、新H06RoleDefaultAdmissionEditorTests；确认原Editor DLL晚于源码/reload完成。
- run_tests EditMode，groups H06RoleDefaultAdmissionEditorTests 与 BattleOptimizationWindowsAiSuiteRequestTests.PostBindingsRole_*，job00a6a92fae494861bd155a963ebec486，24/24 PASS，qualification-tests.xml。
- AdmissionEditorSettings("before") 首jobd62800c6d3154b0e8adbb1ec6a4e095f FAIL（EditMode runner临时场景使sceneCount=2）；未启动窗口，改为测试内核原Menu loaded/clean/roots，运行外依旧检查sole clean8。jobd3e0431710c942f1a85d6762a4e6de42 1/1 PASS。失败XML保留editor-before-tests.xml，成功为editor-before-settings-pass.xml。
- execute_menu_item(menu_path="NTSD/Validation/Optimization/H06 Scoped Role Admission") 唯一一次，原始四窗均在windows，终态suite-result.json。
- AdmissionEditorSettings("after") job8958ed43108f4a2eb80ffd2f0ef7eca7，1/1 PASS；editor-before.json和editor-after.json逐项相同，editor-after-settings-pass.xml。
- 门通过后再独立登记生产Change、保存promotion-before字节，manage_asset导入Profile/GameConfig说明/具名两测试文件。
- run_tests EditMode groups: H06RoleDefaultAdmissionEditorTests.Production*；BattleBruteProductionAdmissionEditorTests.(H06RoleAdmission_EditorReleasedAndDefaultUsesRole|RoleDriver_DefaultBruteAndExplicitQualificationOnly)；BattleRuntimeOrderedShutdownEditorTests.DriverShutdown_ClosesRuntimeStagesBeforeStoppedAndIsIdempotent。job5ce5b09255b54e09b5f9b4c862da9710，最终XML13/13 PASS，2.591339s。get_test_job查询通信超时，未重复派发；XML与production-route.json提供终态证据。
- 新测试开发期两次CS0246为缺类型namespace（ProductionEntityStressRequest、BattlePresentationBackendMode），已修正；修正前源备份和真实失败保留，最终原Editor两DLL编译且测试执行成功。

production-route.json明确cycle0 expected backend1/actual mode3、cycle1 backend0/mode1、cycle2 backend1/mode3。不是新实景长窗，关闭重进为原Production创建链的聚焦覆盖，未声称正常App物理按键或全角色视觉认证。

## 未完成边界

本次只完成既有Role限定准入；没有关闭H06父项、H07、Android，没有正式EXE新全域trace、Android设备/GPU/Player证书。正常33ms、无持续backlog、正常drop0等最终门不变。95继续OFF且失败原因不因本项被抹去。
下一仅已批准的第二项生产者Runtime快照复用，固定本次Role配置，先同配置候选评估、必要门通过才独立生产启用；不继续第三项/旧Goal。

交付校验：pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity，exit0；1395记录/5个当前dirty代码路径覆盖，warnings=4417。保护2353既有文件仅登记的GameConfig说明、Profile、旧默认测试及Suite变化，详preservation-audit.json；新增测试/meta单列，HEAD/staged保持。

13/13生产测试XML完成后，get_test_job与get_editor_state连续通信超时（6402仍由原PID74724监听，进程Responding=True、标题Menu）。没有重启/重复派发。四窗结束时的Scene/设置恢复已实测，生产测试结束后的最新Editor安全状态暂不可查询；第二项暂仅只读准备，已向用户询问是否有弹窗/他人操作。此通信未知不能写成第二项获准运行。

用户后续确认：原Editor存在弹窗；用户点击按钮后Editor退出，随后用户手动重启。弹窗标题/正文未取得，不能归因于本次代码或特定故障。原PID74724已退出，新PID39108已出现，初查窗口Completing Domain，等待加载/桥接及场景安全核验。不是代理重启，不抹去此前13/13/XML与四窗证据。
