# NTSD28-336B44-Q07-RASENGAN-ATTACK-CURRENT-SCENE-001

状态：`RUNTIME_PENDING / FIRST_WINDOW_SCOPED_PASS / OTHER_TWO_WINDOWS_DEFERRED`。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`，G1/BATCH-04/Q07。当前正式根 336B44 三条自然防→前→跳→后续 Attack 55 tick 的源码/根选定字段 990/990 已一致；新版原 Unity Battle Scene 首253通过，次253与254后保留条件门。

原状：既有 `NTSD28UserRasenganPhysicalPlayProbeEditor.NaturalProbe` 能从原 Battle Scene 站立鸣人、InputSystem 物理键状态和生产 LocalFreeRun Driver 跑三窗口。2026-10-04 首窗口在原 Editor `initialTick=2`、首253=35、Attack=36/phase0、action301=36 通过；第二次启动时 Editor 失焦，排队 L 键 30 tick 均未进 `FrameInputSet`，探针八次脉冲耗尽。该失败是诊断输入注入前置，不是战斗规则首差。

声明代码路径：仅修改 `Assets/NTSD/Scripts/Test/Editor/NTSD28UserRasenganPhysicalPlayProbeEditor.cs` 的既有 `NaturalProbe.Queue`，在排入合成键盘状态后显式执行一次 InputSystem 更新，让后台 Editor 的同一物理设备动作映射稳定收到事件。保留既有三菜单、自然起手、逐 tick 记录、单 tick 生产 Driver 和唯一结果文件；不改 `FrameInputSet`、生产输入路由、DAT、图、音频、Scene、Prefab、配置和非战斗逻辑。

验收：生成 Editor 工程编译 0 error，原 Editor 导入新程序集；原 Battle Scene 干净初态逐项复验首253、次253、254后三个窗口，与当前正式根的动作/MP/相位逐 tick 比较。结果必须区分合成设备事件、真实人手键和可见画面；失败原件保留。退出 Play 后检查单一 Scene clean、Battle/Menu/配置哈希和 World/pool 残留；`Tools/Validate-ChangeLedger.ps1` 通过。不跑无关全量测试。

风险与回滚：显式 InputSystem 更新只在 Editor 诊断中发生，可能改变事件到达的渲染帧，故每个窗口必须以实际 FrameInputSet、native proxy 采样相位及正式根 tick 为裁决；若出现首差，先查测试相位和共用路由，生产改动另立 Change。回滚仅撤销这一处测试脚本变更；任何删除/覆盖原件按文件操作审计合同处理。

执行结果（2026-10-04）：生成Editor工程0 error/332 warning，ChangeLedger validator exit0。原Editor新程序集已导入；首253当前探针原Battle Scene自然键PASS，初始tick4后tick37→38转301。严格源/Unity首相对tick动作0/1不等；从防御消费tick2起4字段132/132同。再启第二253时日志监测器漏掉bootstrap标记，未执行菜单；不把它记为生产FAIL。按总表工作量停止线暂停重复Play，次253/254后待真实可见提示/按键首差或Q12最终矩阵触发。两次退出Scene clean、Battle SHA稳定。详[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-RASENGAN-ATTACK-CURRENT-SCENE-001/REPORT.md)。
