# Task Contract — NTSD28-B0-AUTHORITY-SOURCE-CAPTURE-001

> 状态：`FOCUSED_TEST_PASS / SOURCE-MODEL-CAPTURE-READY / UNITY-EXPORTER-NEXT`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B0`  
> 建立日期：2026-09-02

## 1. 目标

在本仓库工作区构建一个只读取 NTSD 2.8-Logan 对应 playable 源码的诊断 capture runner，按
`NTSD28-B0-ENTITY-FIELD-SCHEMA-001` 冻结的 47 个核心实体字段输出 raw JSONL。正式 EXE 身份、
source producer 身份和 legacy ScenarioLoader reference hash 必须分开记录，不得把 source-model
capture 表述为正式 EXE runtime trace。

## 2. Authority 与证据边界

- 正式 authority EXE SHA：`1277B70B...DAF75`。
- 对应源码的 playable build 明确编译 `battle_world.cpp`、`simulation_tick_driver.cpp`、
  `game_session.cpp`、`scenario28.cpp`。
- 既有 ScenarioLoader legacy reference SHA：`5EDA5144...19D86B`；只作为既有 scenario 合同字段，
  不替代正式 EXE 身份。
- 既有 `trace_json28` 为 `ntsd28-trace/1.0` 且字段不足，因此本包创建 workspace-owned raw capture。

## 3. 允许文件

- `Tools/NTSD28AuthorityTrace/Build-AuthoritySourceCapture.ps1`
- `Tools/NTSD28AuthorityTrace/authority_source_capture_main.cpp`
- `Tools/NTSD28AuthorityTrace/Scenarios/neutral-two-entity.json`
- `Tools/NTSD28AuthorityTrace/README.md`
- `Tools/NTSD28Parity/AuthorityCaptureValidator.cs`
- `Tools/NTSD28Parity/Program.cs`
- `Tools/NTSD28Parity/TraceComparator.cs`（仅复用 entity-array validation seam）
- `Tools/NTSD28Parity/TraceContractSelfTest.cs`
- 本 Task、同 ID Change Record、Ledger、STATE、handoff、总表

## 4. 强制安全合同

- authority root 只读；build/output 必须在本仓库 `Temp` 或用户明确给出的非 authority 目录。
- build 脚本解析并验证 output 不在 authority root 内，正式 EXE hash 不符即 fail closed。
- 只编译未修改 authority source 与本仓库 runner main；不复制、patch 或覆盖 authority 文件。
- capture header 分别记录 formal EXE SHA、source manifest SHA、scenario legacy reference SHA、
  scenario data SHA、slot capacity、tick range。
- raw capture 是 `SOURCE_MODEL_DIAGNOSTIC_ONLY / certificateEligible=false`。

## 5. 验收

1. PowerShell parser 与 C++ `-Wall -Wextra -Wpedantic` build 通过。
2. authority source manifest 在两次构建中稳定，正式 EXE SHA 校验通过。
3. neutral scenario 实际运行并产生连续 completed-tick raw JSONL。
4. C# validator 对 header、身份、47-field exact entity schema、tick 连续性和 extra/truncated fail closed。
5. `NTSD28Parity` build/self-test、format、Ledger 和 diff check 通过。
6. authority 目录没有任何写入或 build output。

## 6. 回滚

删除本仓库新增 runner/validator，并把本 Change 标记 `ROLLED_BACK`；保留 Task/Record 与失败证据，
不删除 authority、用户文件或此前 B0 包。

## 7. 实际结果

- C++ final build 0 warning / 0 error；PowerShell parse PASS。
- formal EXE SHA verified；authority source manifest `C59BD8D3...F2D75` 两次稳定。
- real capture `3 ticks / 6 entity snapshots / 47 fields / valid`；certificate false。
- .NET build 0 warning / 0 error；self-test 21/21；authority-output guard PASS；Ledger PASS。
- binary 因链接器元数据非确定，故 header 精确记录每次实际 binary SHA，未与稳定 source SHA
  混为同一身份。
