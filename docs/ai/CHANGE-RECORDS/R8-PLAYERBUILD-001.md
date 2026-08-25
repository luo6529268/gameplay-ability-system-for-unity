# R8-PLAYERBUILD-001 — dual-backend Windows diagnostic Player build entry

<!-- CHANGE-RECORD
id: R8-PLAYERBUILD-001
status: ABANDONED
code-path: Assets/NTSD/Scripts/Test/Editor/ProductionEntityStressPlayerBuild.cs
authority: USER R8 Windows Mono/IL2CPP certification requirement; existing Player bootstrap contract; no C++ gameplay change
evidence: WP01E Editor current-build VERIFIED; WindowsStandaloneSupport and IL2CPP module present; current build tool Mono-only
-->

> 创建日期：2026-08-23  
> 状态：`ABANDONED`  
> 所属：`R8-WP01F-01`

## 1. 修改前状态

- 现有build tool只暴露旧U9 Mono菜单；
- 输出固定`Temp/U9-Windows-Player/NTSD-U9-Windows-Mono.exe`；
- 共享raw dependency复制和finally恢复backend/frame timing/run-in-background/Burst逻辑已存在；
- 没有IL2CPP入口或两后端独立artifact identity；
- 当前Standalone默认backend=IL2CPP，所需国际版模块已安装。

## 2. 允许改动

- 仅在现有Editor build tool提取共享helper；
- 新增Mono/IL2CPP菜单、backend spec和独立Temp输出路径；
- 保留单scene Development build、raw dependency copy和finally恢复；
- 构建日志明确backend、output和BuildReport结果。

## 3. 禁止与保护

- 不改production runtime/bootstrap/harness/scene/ProjectSettings默认值；
- 不删除旧U9 artifacts或未知目录；
- 不改Burst实际默认内容，只允许build期间临时关闭并按原始bytes恢复；
- 不部署默认stage.dat，不触碰Android/C++。

## 4. 验收与回滚

- fresh compile0；Mono和IL2CPP BuildReport均Succeeded；
- 每次build后backend/frame timing/runInBackground/Burst bytes恢复；
- 两Player run gate和cross-runtime hash由WP01F-02裁决；
- 回滚只撤销本build tool change并保留build/error artifacts与Record。

## 5. 实际改动

用户已于2026-08-23明确批准`R8-WP01F / R8-PLAYERBUILD-001`并恢复目标。已只修改已登记的
`ProductionEntityStressPlayerBuild.cs`：

- 新增R8 Windows Mono与IL2CPP两个菜单入口；
- 输出隔离为`Temp/R8-Windows-Player/Mono`和`Temp/R8-Windows-Player/IL2CPP`；
- 提取共享`BuildWindowsPlayer(...)`，由显式backend、输出目录和可执行名称驱动；
- 保留旧U9 Mono菜单作为兼容别名，但实际委托新Mono入口；
- 构建日志新增backend、output、BuildReport size与duration；
- 未修改runtime、scene、ProjectSettings默认值、raw dependency范围或C++。

## 6. 验证进度

- UnityMCP force scripts refresh在一次可恢复断连后返回Editor ready；随后Console error为0；
- 目标脚本与留痕文档的scoped `git diff --check`通过；
- `Tools/Validate-ChangeLedger.ps1`直接执行通过：73 records、73 governed code files；
- 构建前Burst与ProjectSettings SHA256分别为
  `72601656F7F4B74E53D13C65A01CF8A26A450257D75A3F145A0D5D6EBF5D1296`和
  `25475E98DDF5AE903C1CE4685353715F843D004485D088E0BED33BEA98792338`。

当前状态仅为`COMPILE_PASS`；尚未取得任一BuildReport或Player运行证据。

## 7. Mono build / run中间证据

- Mono BuildReport：`Succeeded`，`195302909` bytes，`00:00:43.6326710`；
- build后ProjectSettings/Burst SHA256与构建前完全相同；
- 首次以隐藏窗口启动时，1000实体、180 tick、Player hard 0 B与teardown均通过，但SRP world-camera不提交，
  Bootstrap按中央draw=0正确以exit 3失败；该次保留为负控制；
- 可见D3D11窗口正式运行exit code 0：1000实体、30 warmup、180 sampled、MobileExtended、
  DataOrientedCanonical、每帧最多1 tick、Player hard tick/driver/presentation/envelope均0 B、Gen0/1/2=0、
  capacity critical0、CentralOnly draw=1、submitted-pixel frames=179、teardown restored；
- Mono报告：`Temp/R8-Windows-Player/Mono/Combat1000.report.json`；
- Mono executable SHA256：
  `7C3239CDA74699A565D2F70972D2A59DD5629CE5A17A7986568C93E3B2BDC7AF`；
- Mono report SHA256：
  `63046D253C001161DA4191E3E24F2379370DE5290BB618F921FA6C591B337EAC`。

更正：上一句是Mono完成时的中间状态。其后IL2CPP build曾在Codex沙箱中执行，但用户已明确要求不处理、
不诊断且不将其结果作为项目差异；完整双runtime验收未取得，WP01F不得标`VERIFIED`。

## 8. 用户停止决定

用户随后明确指示`IL2CPP Player 不会有任何问题，不要做相关处理`。本Change现标`ABANDONED`：

- 不再执行任何IL2CPP构建、运行、诊断或修复；
- 不把Codex沙箱Player结果作为gameplay变更依据；
- 已写的双backend Editor build helper不回退、不删除，除非用户另行明确授权；
- 本Change未达到原双runtime验收条件，不能标`VERIFIED`。
