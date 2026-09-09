# NTSD28-B0-UNITY-RAW-SCENARIO-EXPORTER-001 — Unity completed-tick raw scenario exporter

<!-- CHANGE-RECORD
id: NTSD28-B0-UNITY-RAW-SCENARIO-EXPORTER-001
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs
authority: User-directed NTSD28-UNITY-BATTLE-REALIGNMENT-001 B0; NTSD28-B0-ENTITY-FIELD-SCHEMA-001; NTSD28-B0-UNITY-ENTITY-PROJECTION-001; NTSD 2.8-Logan Stage 23 decoded background bounds.
evidence: UNITY-COMPILE-0-ERROR / EDITMODE-3-OF-3-PASS / REAL-UNITY-3-TICKS-6-ENTITIES / DETERMINISTIC-BYTE-EQUAL-RERUN / OID-99-FAIL-CLOSED / TEMP-DRIVER-AND-DATA-SINGLETON-CLEAN / COMMON-AUTHORITY-CAPTURE-VALID / GLOBAL-LEDGER-PASS / NO-DAT-SCENE-AUTHORITY-WRITE
-->

> 状态：`FOCUSED_TEST_PASS / UNITY-COMPILE-0-ERROR / EDITMODE-3-OF-3-PASS`

## 修改前事实

- Unity 47-field projection 已通过 3/3 focused，但没有 scenario、tick hook 或文件 writer。
- authority source-model 已真实输出原 OID 99/7 的 3 tick；OID 99 存在于新权威
  `resources/runtime/decoded_dat/data/data.txt`，不在 Unity `Assets/NTSD/Config/data.txt`。
- OID 2 与 OID 7 在两侧都存在，因此选为不掩盖内容差异的公共 runtime 基线。
- Unity `data.txt` 没有 background catalog；新权威 Stage 23 decoded background 明示
  `width:1330 zboundary:542 712`。
- 既有 `BattleParityTraceEditor` 是旧 schema/旧流程的历史 runner；本包不修改它，也不把它的
  旧输出标为 2.8 证据。

## 预期实现

- 新增 2.8 专用 request/menu/test runner，读取公共 scenario，临时装载 Unity 当前 DAT，创建真实
  `LF2Character`，以 Manual driver 连续 `StepOneTick`。
- header 与 tick raw JSONL 全部为 diagnostic only；tick 实体由既有 47-field projection 生成。
- cache、singleton 和临时 GameObject 必须在 finally/dispose 恢复。

## 回滚

删除本 ID 新增 exporter/test/meta/common scenario，并移除本 ID 的 Ledger/STATE/handoff/总表状态；
不触碰原 OID 99 场景、既有 projection、旧 parity runner、DAT、Scene 或 authority。

## 实际实现与验证

- 新增 2.8 专用 Editor request/menu/test runner、公共 OID2/7 scenario 与 3 个 focused tests。
- 使用 Unity 当前 `data.txt`、Decryptor、ParserV2、Converter 和真实 `LF2Character`；仅加载请求的
  OID，缓存与新建 singleton 在 dispose 恢复/销毁。
- Manual driver 直接提交每 tick 的空 `FrameInputSet`，调用 `StepOneTick(...,
  buildPresentation:false)`；completed tick 后调用既有 47-field projection。
- 首次 compile 失败：header 末尾多一个右括号，Unity 报 CS1002/CS1513；单字符修正后 fresh
  compile 0 error。未把首次失败包装成通过。
- focused job `3fe84b44ff2345f4850ae6cdaac612f0`：3/3 PASS，duration 2.2622294s。
- OID99 原场景明确报 `Unity current content is missing required object ids: 99`，且不创建输出。
- 公共场景真实输出 tick 1/2/3，每 tick slot0=OID2、slot1=OID7；两次 canonical raw 逐字节相同；
  临时 driver、GameDataManager、CharacterAnimtorManager 数量前后相同。
- Unity raw SHA：`5293992E43015074AA719F2260576BB2C733BC797A24EA06D14EB9366DD5D9C7`。
- 同场景 authority source-model capture exit 0，validator 为 valid/3 ticks/6 entity snapshots，raw SHA
  `954D3F77BD7F955253C83532EEB77CE706523C313074C1B472A3DA5B8DD80566`。
- 首次字段盘点共 105 个 tick-slot difference rows、18 个 unique fields：14 个既定 missing=null；
  另 4 个为 allocationEpoch、frameCounter、baseMaxMp、environmentState。尚未做语义修正或 v2 adapter。
- 全局 Ledger：69 records / 13 governed code diffs / PASS；diff check 只有既有换行 warning。
