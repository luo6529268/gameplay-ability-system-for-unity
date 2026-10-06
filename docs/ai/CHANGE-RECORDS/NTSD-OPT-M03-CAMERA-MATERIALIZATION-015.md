<!-- CHANGE-RECORD
id: NTSD-OPT-M03-CAMERA-MATERIALIZATION-015
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs
code-path: Assets/NTSD/Scripts/Simulation/Stage/SimulationStageRenderModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleCentralProductionWindowSceneProbeEditor.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleCentralPresentationDeferralEditorTests.cs
authority: user next optimization batch; presentation-only redundant materialization; formal336B44 and timing unchanged
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH15-CAMERA-MATERIALIZATION-20261007/REPORT.md
-->

# M-03 warm CentralOnly host/camera 物化消重

最终审计（2026-10-07）：Tools/Validate-ChangeLedger.ps1 exit0，1311 Records/9 governed script覆盖/0error；4260全库历史warning，本Change/新fixture0warning，四宽过滤warning为旧Record声明路径而非本包缺项。514事前保护/10准确备份/三末期补保护0drift；175本地链接0missing、git diff --check exit0、34=高12/中14/低8、编译identity0drift、HEAD与staged不变。原Menu clean/8roots/单Scene/非Play。依据已回链REPORT；生产和父项继续RUNTIME_PENDING。

2026-10-07最终限定通过：149具名去重Passed；before132camera/264Build，after143/143及118/118，各96tick。entity vertex bytes/camera1408→704、Build/camera2→1（所记录工作50%，不是整场/GPU/FPS50%）。三窗segment4/recordedCPUdraw5、stride44/隔离/finite bounds/两slot保持，0growth/failed/rejected，各分配记录0B；Editor硬门false。两次after关闭objects/slots/borrowers0/Scene同SHA，原Menu clean/8roots/nonPlay恢复。after2一次camera tick95→97，latest显示未丢当前publication，类别95非模拟降频。静态仅helper+一调用，生产DLL/最终Editor指纹冻结，10备份/514事前保护及三末期补保护；最终validator/冻结审计回链REPORT。metadata保持RUNTIME_PENDING，动态/首可见/透明/GPU像素/完整链/1000AI/Android与父项不晋升；下方IN_PROGRESS/RED/首窗事实为历史。无Scene/资源/Settings/Q06body/专项门或破坏性Git改动。

after cycle01原Scene PASS：143camera/143Build(pub96+alpha47)、100672bytes；按camera为2→1 Build、1408→704bytes（该实体vertex stream50%），非整场CPU/GPU/FPS50%。原2实体/4command/4segment/1chunk/RenderPass5 recordedCPUdraw保持、两slot/有限bounds/stride44/publication隔离、0growth/failed/rejected、记录分配0B、关闭三残留0/Scene同SHA。第二显式重进已启动；metadata RUNTIME_PENDING保留动态实体/首可见/像素/完整链/高负载/设备门，不晋升父M03/H11。

相关回归 b17ba0b5f4694ebc860d10cdbe1c396b：135/135 Passed/5.7365096s（101前述+辅助容量15/Resolver15/Shutdown4），包括whole-frame capacity拒绝/保留good submission、三pure motion、计数窗口、submesh及关闭。与新14共149去重具名Passed。未执行两个历史孤儿UnityTest；不覆盖其旧trace，不冒充已通过。准备原Scene after第一自然窗口，第二显式重进待验。

GREEN 257cf5f7012a49a5990ee691d1476d2d：14/14 Passed/1.2190235s，完整case输出已保存；原三RED转GREEN，warm host64次0B、同tick空Worldchecksum不变、pending旧lease拒绝及camera最新可取、fallback/显式Flush保持。生产DLL A49C69CE...、最终Editor D27CD497...，probe/source指纹及两生产文件已冻；相关回归与after两自然窗口仍待验。

COMPILE_PASS：生产DLL A49C69CE... / 4215296，Editor515475D7... / 6181376；原Editor idle/CS0。fixture自审将lease.Dispose移到assert之前，确保预期RED/意外失败也释放实际取到的struct lease；不改生产生命周期。RED原件保留，before自然运行有Domain Reload且两slot正常；最终fixture重新compile后才计GREEN。

生产CODE_WRITTEN：Central新增20行host helper，Stage只替换一处调用并显式interactive/non-batch。Queue后有效warm plan/原renderer proof才保留ResolveDisplayAlpha并返回；其它仍原Flush。未改相机入口、原clock公式、Build/segment/capacity/lease/sort/Shutdown。新warm测试补同一tick checksum不变（当前空Worldfixture限定）。before原Scene PASS：132camera/264Build/pub96+alpha168，185856bytes，0growth/分配记录0B、关闭三残留0/Scene同SHA。真实GPU/1000AI/像素/设备尚未验证。

测试先行RED已观察：543f78077ae348c4bea53f2f6bd61d26，14 completed，三个预期首差（pending旧lease已可取、warm host提前消费版本、warm geometry替换）。failed job result为空，仅以实际failures列表/14 completed报告，不猜补通过案例。Editor DLL已AF01297D...，生产DLL和两文件before SHA不变；准备原Scene改前自然窗口。

首次test编译四诊断：既有SelfCheck lease API需CameraType且lease是struct。修新fixture调用/Dispose，不改生产API；EditMode alpha返回1不初始化Play clock，改用既有lastResolved值断言，真实Play时钟由自然窗口验证。此为fixture编译修正，非性能RED证据。

CODE_WRITTEN（仅test/probe）：新14个具名/参数case待RED，probe追加batch15独立before/after菜单与Session输出根；生产两文件保持before SHA。尚未compile/test，不把future helper当已接入。

脚本编辑前PLANNED。[Task](../TASKS/NTSD-OPTIMIZATION-BATCH15-CAMERA-MATERIALIZATION-20261007.md)；[Operation](../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH15-CAMERA-MATERIALIZATION-20261007/RECORD.md)。
原状：子批14同显示帧stage force Flush及camera各Build一次；中央mode旧Renderer/overlay/spark已绕过。
拟新增host helper仅warm有效交互路径Queue与原时钟取样，不做geometry；camera保持既有采样/materialize/acquire。
旧Flush、冷启动、失败、world切换、<=30/batch/Edit/legacy路径保持，pending版本拒绝旧lease保持。
准确4脚本/一个新增meta及七治理进度文档；无新Runtime owner，existing Reset/11阶段关闭回收pending/clock/submission。
副作用、验收、风险、未验证门与恢复方式完整见Task。逻辑33ms/checksum/input/RNG/pass、publication只读、
Q06 activebody/EXT1/MONO/ATLAS/Scene/资源边界不动。不承诺完整0GC/1000AI/120FPS/Android。
