# Task Contract — NTSD28-B0-UNITY-RAW-SCENARIO-EXPORTER-001

> 状态：`FOCUSED_TEST_PASS / UNITY-COMPILE-0-ERROR / EDITMODE-3-OF-3-PASS / REAL-3-TICKS-6-ENTITIES`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B0`  
> 建立日期：2026-09-02

## 目标

为 Unity 建立独立的 NTSD 2.8 completed-tick raw scenario exporter。公共基线使用新权威与
Unity 正式内容均具备的 OID 2/7，在固定 seed、位置、HP/MP、朝向、Stage 23 边界和无输入条件下，
通过真实 `LF2Character` DAT 配置、`SimulationTickDriver` Manual 模式与 `StepOneTick` 连续输出
3 个 completed tick；每 tick 实体投影必须调用
`NTSD28UnityEntityRawCapture.CaptureTickJson(...)`。

原 OID 99/7 场景不得删除或静默换角：OID 99 在新权威存在、Unity
`Assets/NTSD/Config/data.txt` 不存在，必须作为 Direction-B 内容策略未决下的独立内容前置差异。

## 允许文件

- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs`
- 对应 `.meta`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs`
- 对应 `.meta`
- `Tools/NTSD28AuthorityTrace/Scenarios/neutral-common-two-entity.json`
- 本 Task、同 ID Change、Ledger、STATE、handoff、总表

如 exporter 验证暴露 projection API 的最小缺口，可在同一包只扩展
`Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs` 的只读 header/writer
辅助；不得改变既有 47-field 绑定语义。

## 不变量

- 不修改 DAT、PNG、WAV、Prefab、Scene、importer 或新权威目录。
- 不修改 tick/pass/输入/RNG/战斗行为；不复用旧 NTSD 2.4 parity 结论。
- Unity 内容只从当前 `Assets/NTSD/Config` 只读加载，运行后恢复 singleton cache。
- 输出只写 `Temp/NTSD28UnityTrace`，`certificateEligible=false`。
- Stage 23 使用新权威背景的只读已观测边界 `width=1330, z=542..712`，避免初始 z=650
  被 Unity 默认无背景边界错误夹取。
- 公共场景与 OID 99 内容缺失证据分开；不得用公共场景宣称内容已一致。

## 验收

- Unity compile 0 error。
- focused EditMode 覆盖场景校验、Unity OID 99 fail-closed、真实 OID 2/7 三 tick 文件输出。
- 输出 header 明示 Unity producer、Direction-B raw manifest、scenario identity、47-field
  21/12/14 binding 状态及非证书属性。
- tick 必须是 1、2、3，实体按 runtime slot 排序，每 tick 两个真实实体。
- exporter 连跑两次得到相同 canonical tick 行；运行后没有遗留临时 driver 或 cache 污染。
- Ledger validator 与 diff check 通过。

本包不完成 v2 full-domain adapter、不关闭 candidate/missing binding，也不宣称双端 parity。

## 当前证据

- 首次 Unity compile 捕获 header 构造末尾多一个右括号（CS1002/CS1513）；单字符修正后
  fresh compile 0 error，仅既有 warning。
- focused job `3fe84b44ff2345f4850ae6cdaac612f0`：3 total / 3 passed / 0 failed /
  0 skipped，duration 2.2622294s。
- 真实 Unity raw：3 completed ticks / 每 tick 2 个 OID2/7 实体；两次输出逐字节一致，且临时
  driver、GameDataManager、CharacterAnimtorManager 数量运行前后相同。
- Unity raw SHA-256：`5293992E43015074AA719F2260576BB2C733BC797A24EA06D14EB9366DD5D9C7`。
- 同场景 authority source-model raw SHA-256：
  `954D3F77BD7F955253C83532EEB77CE706523C313074C1B472A3DA5B8DD80566`；既有 validator
  确认 3 ticks / 6 entity snapshots / valid / certificate false。
- 首次只读逐字段盘点：47 字段中 29 个相等、18 个差异；14 个是既定 missing=null，另 4 个是
  `allocationEpoch`、`frame.frameCounter`、`vitals.baseMaxMp`、`combat.environmentState`。
- 全局 Ledger：69 records / 13 governed code diffs / PASS；diff check 无 error，仅换行 warning。
