# Q07/D-024 项目地图舞台双坐标域原 Battle 首 tick RED

日期：2026-09-29。结论：`VERIFIED_DIAGNOSTIC_RED / PRODUCTION_FIX_PENDING`。本包只扩现有 Editor Play 探针的独立单 tick 请求，直接在原项目 `NTSD_Battle.unity` 与项目自有 Sunagakure 地图复现。原版背景/模式 DAT 未进入 Unity，正式 DAT 值及角色图片不改。两个起点故意在项目多边形外，仅用于边界规则诊断；不把它们称为合法随机出生点或完整战斗同态。

| 原 Battle 独立运行 | 项目 Stage 物理边界 | 出生源Z → 实际Z | 完成 Driver tick6 后源Z → 实际Z | 按项目物理边界应达到 |
|---|---|---|---|---|
| `q07-project-stage-edge-near-z100-20260929-a.json` | 237..760 | 100 → 157.808219178082 | 237 → 374.005479452055 | 实际近界237；源≈150.182291666667 |
| `q07-project-stage-edge-far-z600-20260929-a.json` | 237..760 | 600 → 946.849315068493 | 600 → 946.849315068493 | 实际远界760；源≈481.597222222222 |

两例均从正式角色内容生成韩、李，使用 World 唯一 `BattleSpatialProjection` 的当前深度比 `1152/730` 和共享零锚点映射出生，保留原 Scene 的 `Stage.ZMin=237/ZMax=760`、地图与固定相机，且只调用一次完整 `SimulationTickDriver.StepOneTick`。双方角色同样表现。当前 `NTSDEntityRuntime.ClampStageZ` 把 237..760 同时当作 `SourceRuleZ` 的钳制边界和项目 `Z` 的实际边界；近例源100被误钳到237，再乘倍率把实际157.808推到374.005；远例源600落在错误的数字区间内，故实际946.849未被约束。此为生产路径实测，不只是静态演算。

显式正式源边界控制：原 Editor 的 `NTSD.Test.Editor.NTSD28SourceStageDepthEditorTests` EditMode组，第二次作业 `525ee953c2b346a891ab0bb1128c17b1` 为8/8通过，覆盖 DataOriented/Legacy/Shadow Stage-Z、PreFrame、无源载体和非角色 margin。第一次作业 `8f9b8a1e99c6481bb7310ccdab6c377f` 在测试初始化阶段超时，0例启动，未计作断言失败；第二次加 `initTimeout=180000` 后通过。这证明现有显式源夹具仍可用，不证明项目物理快照已正确。

原 Editor PID11944 的 `refresh_unity` 完成 Tundra build success，0 C# error；两次 Play均记录 `stopped=true`、`borrowersAfter=0`、Scene磁盘前后SHA相同，且脚本结束后Editor返回非Play/idle。五保护文件 SHA-256：Battle Scene `3A089236328ACAE1510F8A831B77D4895CC34028DDCDEBE542BEF0DA8EC235ED`、Menu Scene `DD6A48A37FB8CEA9CD8A1F7738964A719E007A42F0FBB54FB48BBD0B723B9DC3`、SunagakureMap `F7B5E4A44CAC05480D1CA6F67ABF623531264C1C96725D7FDD23DA50C8E60C08`、GameConfig `0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`、ProjectBattleModeConfig `B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`。目标 `git diff --check` 和 `Validate-ChangeLedger.ps1` 退出0。

诊断出口已达，生产修复未写：项目地图物理来源必须与显式正式源夹具分开标记；`Stage.ZMin/ZMax`和可走多边形继续保存物理边界，源规则范围由同一个 World 投影推导，且其来源要在 worker、lockstep snapshot/restore 和 checksum中存活。Stage-Z与PreFrame的 Legacy/DataOriented/Shadow及无源实体要分别验证；AI与波次/state405是独立读者，不由这个首 tick RED 自动闭合。下一个生产 Change 必须按 [父Task](../../../docs/ai/TASKS/NTSD28-Q07-D024-PROJECT-STAGE-DOMAIN-001.md) 精确登记，不得整体改DAT、背景、地图、相机或非战斗框架。Q07/BATCH-04和总目标仍开放。
