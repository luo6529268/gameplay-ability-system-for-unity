# Q09/P-12 香燐 Legacy 诊断：中央出生后切换模式的前提失败

状态：`DIAGNOSTIC_FAIL / LEGACY_RUNTIME_PENDING`，父级 Q09/P-12、Q07/BATCH-04 和总目标均未关闭。

原项目唯一交互 Editor PID11944 在空闲、非 Play、Battle Scene `isDirty=false` 时通过 MCP `refresh_unity` 导入新增探针；生成 `Assembly-CSharp-Editor.csproj --no-restore` 构建 0 error/195 warning，原 Editor `Assembly-CSharp-Editor.dll` 写入时间晚于源码，Editor.log 记载 Tundra build success。唯一 opt-in 请求 `karin-x500-state9997-legacy-01` 复用正式 LoganRuntime、项目 etc-mode1 和原 Battle Scene。请求旧字节 SHA-256 `408A4EAB9B877003019CD591187998FAB13533D08788153B6ACE3EE50713B107`，提交字节 `F0771ADFA2501112084EBCB6FAE3208E86F9719B0AC2AC62D37BB099C30FFA24`，两份均在本目录；旧结果没有覆盖。

完整 Driver 第3步再次自然生 OID314/action50/state9997/owner8，逻辑左向、中央冻结左向而提交的 body 命令右向，复现了已过的中央限定门。随后新增 Legacy 观测立即失败：`child.Renderer == null`，因此没有 SpriteRenderer flip/位置读数，`backendRestored=false` 仅因尚未切换，并非发生了恢复失败。原始结果在既有探针结果目录的 `karin-x500-state9997-legacy-01.json`，SHA-256 `C7792D174A81064FCF0CE1437203EA3ABDB0B1B024AFD0D700B66E4998236D15`。不能把 `legacyFlipX=false` 默认字段当作真实读数。

源码路径解释这一前提：正式 Battle 的 `_presentationBackendMode == CentralOnly` 且目录就绪时，`SimulationTickDriver.PrepareBattleRuntimeServices` 开启 logic-only materialization；`LF2ObjectPointFactory.MaterializeObjectForStructuralWriter` 因而调用 `BattleLogicEntityFactory.Create`，后者 `entity.Init(task, null)`，不会在出生时分配 Legacy Renderer。事后调用 `world.SetBattlePresentationBackend(LegacyOnly)` 只变更表现协调器和现有 renderer 的抑制状态，不为已出生的纯逻辑子体补 renderer。这是当前 Unity 可执行路径事实，不能据此说 Legacy 投影值有首差。下一有效验收入口必须从**战斗准备前**选择 Legacy backend，在对应物化路径自然出生 OID314 后再测 SpriteRenderer；须先审查 dedicated worker/logic-only 开关和现有设置入口，不能在正在运行的 Worker 下直接翻转 materialization 标志。

失败后的 `Finish()` 回收了子体与夹具，World 对象/槽/池借用均从4/2/2回到4/2/2，暂停状态恢复；Editor 已回到 idle/non-Play，Battle Scene `isDirty=false`。Battle/Menu Scene SHA-256 分别为 `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A` / `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`，均与运行前相同。没有生产、DAT、图片、Scene、相机、模式 Asset 或非战斗改动。Change Ledger validator exit0、定向 diff check exit0；没有重跑全量案例。
