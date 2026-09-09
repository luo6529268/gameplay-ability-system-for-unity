# NTSD28-B6-CPOINT-THROW-ENVIRONMENT-VZ-PRODUCTION-001

<!-- CHANGE-RECORD
id: NTSD28-B6-CPOINT-THROW-ENVIRONMENT-VZ-PRODUCTION-001
status: VERIFIED
change-kind: TEST_FIRST
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCpointWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6CpointThrowAtomicProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleGrabCpointLinkPlayModeProbeEditor.cs
authority: NTSD 2.8-Logan BattleWorld28::advance_catch_relations kind-1 throw tail; EXE B1E13AE1, closure 39DDDA15.
evidence: FOCUSED-8-OF-8 / RELATED-17-OF-17 / TARGETED-PLAY-PASS / BUILDS-0-ERROR / SELFCHECK-BLOCKED-UNRELATED-HELD-ACCOUNTING / CONSOLE-0-ERROR / SCENE-CURRENT-BASELINE-UNCHANGED / FULL-RESOURCE-DEFERRED / ILLEGAL-CATEGORY-RETIRED
-->

> 状态：`VERIFIED / FOCUSED_8_OF_8 / RELATED_17_OF_17 / TARGETED_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_HELD_ACCOUNTING / CONSOLE_0_ERROR / SCENE_CURRENT_BASELINE_UNCHANGED / FULL_RESOURCE_DEFERRED / ILLEGAL_CATEGORY_RETIRED`

## 改前/继承状态

- 旧 Unity production误写WeaponCount并无条件清Vz。
- superseded包已临时写入正确environment/Vz，但也错误提前连接完整 MP transaction；本 Change
  必须先移除后者。
- SelfCheck与grab probe的旧错误断言已改到environment/WeaponCount/Vz目标；新 focused test已写，
  但Unity Licensing IPC阻止测试框架启动。

## 验证记录

- `BattleCpointWriter.ApplyThrow()` 现在把`world`传入throw tail；正 injury 先解析resource attacker，
  解析成功时复用display-step helper，然后写caught `EnvironmentState320`与self
  `EnvironmentSourceSlot160`。不再写`WeaponCount`。
- Vz只在`depthUp != depthDown`时覆盖；same-false与same-true都保留入场Vz。
- superseded `ResolveNativeHitResourceInjury`/`ApplyNativeHitResourceTransaction`调用已完全移除，
  `MPMax`不再伪装Authority baseMax；完整MP resource仍标记B7/B8/B11/H后置。
- 新focused test覆盖8个case：positive display/environment/WeaponCount/Vz、四种depth组合、
  nonpositive 0/-2、missing resource owner的display skip/environment continue；SelfCheck与grab probe
  旧错误断言已同步。
- isolated build：Runtime 47 warnings / 0 errors；Editor（临时纳入新test后）104 warnings / 0 errors。
  生成csproj的临时Compile行已反向移除，`git diff`无残留。
- source contract：world传递、no WeaponCount write、no MP transaction、environment两字段、XOR、
  no Vz clear共7/7；`git diff --check`无错误。
- Unity batch尝试4次；前两次在Test Runner启动前因`LicenseClient-Logan` IPC拒绝、
  return199退出。第三次确认stale lock记录的PID 7192已不存在后启动；Unity成功
  拉起LicensingClient PID 53168，但等待`LicenseClient-Logan` 60.01秒后仍无IPC channel，
  再次return199。第四次仅加入本机Hub已使用的无密钥`-useHub -hubIPC -licensingIpc`
  参数；Editor能启动versioned LicensingClient并完成handshake，但因当前沙箱进程无Hub
  access token而返回无有效license/exit1，仍未进入Test Runner。随后只读调用Hub help也因
  AppData `UnityHub/user-settings` 原子写EPERM在bootstrap阶段退出。本轮未读取、传入或
  回显任何敏感token，不继续尝试。四次均没有test XML。额外.NET managed harness
  触发Unity native ECall限制，明确不计为测试证据。
- Scene未加载/保存/进入Play；磁盘SHA仍为
  `50FD4D8FAF2CC5C630886CEFD4742A5FD068E1F7AADEE89809AD19B7B85AFF3C`。
- `Tools/Validate-ChangeLedger.ps1`：355 records / 307 governed code files，PASS。

## 未验证与恢复入口

Unity许可通道恢复后必须补跑focused、B6/CPoint related、B5、NTSD28 broad、fresh SelfCheck、grab
Play probe与Console/Scene dirty检查；在此之前不得写成`FOCUSED_TEST_PASS`或`VERIFIED`。
第三次日志：`Temp/NTSD28-B6-CpointThrow-focused-retry.log`；第四次日志：
`Temp/NTSD28-B6-CpointThrow-focused-hubipc.log`；两个预期XML都未生成。

## CORPUS CORRECTION（2026-09-08）

Direction-B current有58条throwvx与48条positive throwinjury。已写精确子集无需回退，但测试覆盖扩大且
Unity runtime仍未运行；状态继续RUNTIME_PENDING。

## 2026-09-09 runtime验收与夹具纠正

- 首次真实focused job `0e530ff8294141a9a7432f9e574539a4`执行到fixture，但10个NUnit test
  node全部在OneTimeSetUp因category `NTSD28-B6`含非法连字符失败；没有执行production断言，因此不把它
  记录为behavioral RED。
- 同一test文件将category改为`NTSD28_B6`后，job
  `b78fa2a696504d87bb9944afbf7910b4`为`8/8`；category复跑job
  `36f73c6b48da43698cb7404a93e4609c`仍为`8/8`。
- 7-class related job `b93bfe236b5b4401a3d82144c962bcf5`为`17/17`，覆盖CPoint resolved
  hurt、formal CPoint、HitPlan、pre-interaction与B5 exact-credit邻接面。
- `BattleRuntimeSelfCheck.CheckCpointThrowRawAndTransformMatrix`漏改的旧断言现按Authority在无独占depth
  input时保留entering Vz；full SelfCheck已跨过该检查，下一首差为独立held-CPoint injury accounting
  (`BattleRuntimeSelfCheck.cs:11399`)。
- `BattleGrabCpointLinkPlayModeProbeEditor`在真实`NTSD_Battle`返回`PASS`；mismatch throw与link residue
  cleanup通过。退出Play后Console error=0。
- 运行窗口Scene baseline前后均为
  `89AA621640A3818F2C7CD307836C50D72CAB84B6D4F2B6DB333FF7CBF525A673` / `216762` bytes /
  `2026-09-09T01:58:58.6144489Z`，Editor `isDirty=false`。该baseline开始前已有并行用户Scene修改，
  本Change未回退或认领。
- fresh `dotnet build Assembly-CSharp.csproj --no-restore`与Editor对应命令均0 error；既有warning分别
  47/104。
- 保持`FULL_RESOURCE_DEFERRED_B7_B8_B11_H`；没有重新加入MPMax近似或改变HitPlan/schema/content。
