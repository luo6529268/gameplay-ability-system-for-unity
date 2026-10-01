<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C044-IMPULSE-PASS-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6CatchExactConsumerAndAdvanceOrderProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCpointWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleGrabCpointLinkPlayModeProbeEditor.cs
authority: selected 336B44 playable catch relation and horizontal impulse passes, formal OID16/OID2 controlled source witness
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C044-IMPULSE-PASS-001.md
-->

# C044 Unity pending 冲量与动作计数时序

代码修改前创建。当前正式源受控跨零：抓取pass把双方pending contribution count设1、受害者total X/Y设±4/-3，动作计数保留、motion暂不变；finalizer才写motion。Unity共用writer立即写速度/将动作计数改1、没有写pending count，现有两个测试把旧实现写作预期。只修声明的战斗writer和两个同范围夹具；不改source/DAT、场景、GAS、菜单、结果页或用户比例。影响是后续同tick碰撞读取、后处理结算、记录帧计数；以现有帧后处理pass在Legacy/DataOriented中的共用合同承接，不新增平行结算。

流程：先更正定向测试取得RED；再只改writer跨零分支；最后更新既有Play探针断言、跑目标与邻近测试。验证与限制详Task。回滚仅在审阅当前差量后精确恢复本包修改，不触碰原有C042同文件修改。当前状态 PLANNED；结果另行追加。

2026-10-01 测试先行进展：只改已声明 B6 Editor 定向测试为两个帧后处理 profile × 左右方向四例，加入抓取 pass 前/后字段断言；同时修同文件 Play runner 对旧方法名的直接调用并更新案例数。原 Editor 第一次 Refresh 暴露旧调用 CS1061，未把旧程序集16/16冒充RED；修调用后 Editor.log 显示 Assembly-CSharp-Editor Tundra build success、0 error，当前 domain reload 尚未结束，MCP status reloading=true。生产writer和另一份旧Play探针尚未改；RED与修复待Editor恢复。

2026-10-01：原Editor job cbc96e7e8d5d4126942dd6bd7e27d664 定向19例中新增4例如期RED，其余15 PASS；4例首差均为抓取者动作计数期望2、旧实现1。此前第一次旧程序集16/16及测试初始化超时job 9a1e7a04934e46dfafa4f4670a507b2b 均不计RED；旧Play runner旧方法名CS1061已修且重新编译0错。之后只在声明的 BattleCpointWriter.RunKind1 跨零分支将双方 AttackingCounter=1 改为 HitCount=1、删除即时 Runtime.Vx/Vy 写入；保持受害者 Knockback X/Y、动作、关系及同文件既有C042投掷修正。另一声明的旧Play探针已把立即pending与后处理速度断言改为当前源时序。修后编译/测试/Play仍待，当前状态 CODE_WRITTEN。

2026-10-01 修后验证：原Editor Tundra build success、0 error。job a4813146a2884a6c894c40382f30f108 抓取类19/19 PASS（新四例两profile×双向及相邻15例），job b6deff00b29c4209aab80d92fc547c6f 帧后处理对照2/2 PASS；无全量重跑。Tools/Validate-ChangeLedger.ps1 exit0，1075 Records/15 code文件覆盖；git diff --check exit0。Battle/Menu Scene、GameConfig、ProjectBattleModeConfig 四SHA与保护基线相同。Play与正式根自然跨零待，状态 RUNTIME_PENDING / FOCUSED_TEST_PASS。

2026-10-01 原Battle Scene既有 R8 抓取 Play 探针 PASS，结果SHA 2B048397A330AF1B9810F350934729F19E798062A6D471C499F48264C2AFE80E；tick1707受控跨零即时受害者HitCount1/Knockback4/-3，后处理HitCount0/Runtime速度4/-3，动作、timeout、关系/帧等待断言均通过。全探针对象4→4/slot2→2/池2→2、统计还原/cleanup true。已退出Play，Editor idle/nonPlay/0编译；四保护SHA保持，git diff --check/账本通过。[验收](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C044-IMPULSE-PASS-001/ACCEPTANCE.md)。本 Change 仅受控 Unity 逐pass出口 VERIFIED；正式根自然跨零无同态，父C044/Q07/总目标继续开放。
