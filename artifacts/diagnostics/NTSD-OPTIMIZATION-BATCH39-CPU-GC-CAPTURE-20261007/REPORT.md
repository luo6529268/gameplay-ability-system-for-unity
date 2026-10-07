# 第39批CPU/GC采集：已得到热点证据，尚未改善生产 FPS

结论：当前受控千人重现的首要热点是普通 BruteForce 碰撞候选收集，不是渲染线程持续进行 GPU 工作。此次只做诊断，四候选仍关闭，H07/H11 未完成，Goal 保持 active。

## 实际范围与结果

原 PID19040 Editor 已在干净 Menu/非 Play，不是用户截图仍在运行的现场。复用原 Combat1000 请求一次：1000 真实 AI、120 warm + 180 sample、seed 0x4E545344、完整显示/main-thread/Brute，只有输出路径变化；原 33ms/3ms/max2、pass、输入、checksum 与关闭边界不改。

suite-result 为 DONE / MEASUREMENTS_COMPLETED；观测实际 AI/base roster 下限 1000，StoppedCleanly / harnessValidity=true。orderedShutdown 完成，objects/slots/borrowers=0，Battle/Menu 磁盘 SHA 和 clean 恢复门通过。终态第300tick的20个扩展/lockstep hash及完整 snapshot SHA与37 Combat全同（E4916ACA…6B12）；这仅是末tick证据，不是逐tick/native准入。

## CPU 证据（采集有扰动，不是无 Profiler 性能基线）

| 口径 | 本次结果 | 解释 |
|---|---|---|
| 完整180 sample逻辑 tick | mean 475.685ms / P95 823.006ms | 仍远超33ms |
| CandidateCollect / PairExactLoop | 439.602 / 438.925ms mean | collector占tick约92.414%；不能把父/子耗时相加 |
| 显示帧间隔 | mean1105.438ms，150帧 | 约0.90 FPS的受控重现，非新的优化收益 |
| 8个恢复主线程帧 | 每帧2 tick，Driver1.400–1.829s / collector1.277–1.696s | 原调用树直接定位普通实体对扫描 |
| 同8帧中央物化 / ExecuteCommandBuffer | 物化9.565–21.202ms / Execute0.061–0.200ms | 仍有优化空间，但不是这次秒级卡顿的主成本 |
| 同8帧 Render Thread | Gfx.WaitForGfxCommandsFromMainThread1.479–1.926s，占该线程帧墙钟>99% | render thread高显示值主要为等主线程；不是GPU耗时 |
| FrameTiming已接受窗口 | render CPU mean0.743ms/89帧；GPU counter mean1.198ms/85帧 | 有效计数器样本，非GPU capture、平台认证或全场景证明 |
| CPU中央命令 / segment / 全帧SetPass | mean1991.49 / 1990.49 / 1995 | 三口径分开；不推导真实GPU batch，也不解冻EXT1/ATLAS |

CPU细表：cpu-eight-frame-summary.json；完整调用树与GC地址/方法：windows-01/cpu-gc-samples-recovered.json；不将同一调用树的父/子inclusive值累加成占比。

## 采集工具失败与已有 raw 恢复（全部原件保留）

首次自动帧回调0次，45秒保护截止；binary写出127133652B，但原state为PARTIAL/capturedFrames0，原samples为空。这没有通过“自动8帧截止”验收。随后仅在原Editor idle且Profiler history为空时LoadProfile同一raw（keepExisting=true），跳过首帧导出1..8帧，没有第二次录制或千人重跑。

恢复后主线程8/8有效、Render Thread8/8有数据；扫描128线程上限触顶，恢复state仍PARTIAL，未扫描线程完整性UNKNOWN。它不否定已经取得的主/render证据，亦不能写成全线程或自动截止PASS。恢复JSON573432964B；旧raw、旧state和旧samples未覆盖/删除。raw前后SHA一致8DE275686D399C23595D95155D73547182026B19CC596DBA99E40BBF815B09F5。

原采集与恢复的完整ProfilerSettings前/后逐字段一致（enabled/driver/binary/logFile/stack/CPU/profileEditor/deep/memoryMode），再读manage_profiler所有areas同预检：CPU/GPU false、enabled/recording/stack false。加载的本批history保留，未ClearAllFrames；普通Editor仍Menu idle。

## GC专项发现

8帧Main Thread每帧约6254–6271 GC.Alloc、约490106–490446B，包含大量Editor工具轮询，不能全部归为战斗runtime。按Assembly-CSharp.dll运行时调用栈分开后，已确认：

- BattleNativeDirectSpawnWriter.IsInitialActionAdmitted：235次/4700B，每次20B；调用链为late OPoint structural birth。当前源9–14行为frames.Exists(frame => frame.frameId == action)，捕获lambda是对应的分配嫌疑；尚未改此规则入口或做IL/修复验证。
- BattleDamageWriter.RecordAlternateLeadSound：1次/36B，位于替代命中声音调用链，精确叶子及修复尚待查。
- 大量分配来自现有Assembly-CSharp-Editor轮询（示例gc-frame01-method-groups.json），不是上述运行时两项，也不是GPU或中央合批问题。

上述两项不是35批camera的2事件归因；后者准确调用点仍UNKNOWN，不凭新场景替代原H11验收。原stress raw gate true仅保留原读数，不能对抗此次实际分配或恢复完整0GC证书。

## 编译、聚焦与安全检查

新10有效RED（缺API）→新10+旧Suite67共77/77 GREEN（4.7720017s）；加入raw恢复后的同77项再次77/77（4.0342941s），0失败/跳过。原Editor编译/程序集重载成功，CLI无Pipeline与execute_code CodeDom失败原件保留，不伪装成功。

Tools/Validate-ChangeLedger.ps1 exit0：1335records/14 governed code files covered，4247既有warnings（非0warning）。15保护与8dirty备份/HEAD保持、声明diff检查exit0；最终post-audit.json为准。只改本批三个Editor源码，不修改普通战斗算法或默认，没有启动第二Editor、破坏性Git、Scene保存、资源/ProjectSettings/Server/Q06排序方法体改动。
最后独立git diff --check仍exit0；比较Suite与fresh dirty备份的git diff --no-index --stat为37insert/4delete、exit1仅表示存在预期差异，不是检查失败。最终Editor get_editor_state为原Menu/idle/非Play/无测试/不编译；原始响应及validator/diff实际命令结果见final-validation.json。

## 下一步（已经授权，不重复索取本次范围）

直接进入已有32/34/36/37 Brute候选的限定生产准入：保持同collector、原pair/方向/候选/RNG/rest副作用与容量回退，补必要逐tick一致性并复用适用当前权威证据，通过后按用户授权接入普通战斗。不是开新空间backend/重开历史全量对齐，也不以继续微候选、更多同版profile或末tick替代准入。39没有FPS改善或生产推广；38仍PLANNED，EXT1/ATLAS/Mono专项门保持，Goal不停止/不complete。
