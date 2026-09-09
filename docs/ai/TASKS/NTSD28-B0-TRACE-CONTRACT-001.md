# Task Contract — NTSD28-B0-TRACE-CONTRACT-001

> 状态：`FOCUSED_TEST_PASS / TOOL_CONTRACT_READY / BUILD_PASS / SELF_TEST_13_13 / GLOBAL_LEDGER_PASS`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B0`  
> 建立日期：2026-09-02

## 1. 目标

建立不依赖旧 NTSD 2.4 C# authority 的独立 NTSD 2.8 trace 基础合同：

- 固定正式 EXE identity、completed-tick 边界、双 RNG、world、entities、relations、rests、events 和 presentation 域；
- 固定当前用户批准例外、用户排除项、内容策略状态和 Slot 容量不参与 header equality 的边界；
- 提供 JSONL trace validator、流式 first-difference comparator 和 malicious/self-test；
- 为后续 C++/Unity exporter 提供同一份版本化消费者合同，但本包不实现 exporter。

## 2. 允许文件

- `Tools/NTSD28Parity/NTSD28Parity.csproj`（独立 .NET 10；无外部包依赖）
- `Tools/NTSD28Parity/Program.cs`
- `Tools/NTSD28Parity/TraceContract.cs`
- `Tools/NTSD28Parity/TraceComparator.cs`
- `Tools/NTSD28Parity/TraceContractSelfTest.cs`
- `Tools/NTSD28Parity/README.md`
- 本 Task、同 ID Change Record、Ledger、STATE、当前 handoff 和对齐总表。

任何其他 C#、Unity Scene、Prefab、DAT、资源、ProjectSettings、旧 `Tools/NTSDParity` 或权威目录文件均不属于本包。

## 3. Authority 与合同

- 权威：`docs/ai/CURRENT-AUTHORITY.md` 固定 SHA 的正式 `NTSD2.8-Logan.exe`，以及 `source/README_SOURCE.md` 声明对应且进入 playable build closure 的源码。
- 主 tick：`SimulationTickDriver28::step(...)`；本包将一行 tick 定义为该 step 完成后的 completed state，不把 host/render frame 当逻辑 tick。
- 双 RNG：CRT stream 与 synchronized 3000-byte table stream 必须分别记录 state/calls；同步流还必须记录 call-site 序列或其完整 canonical body。
- Slot 容量：两端 header 可记录各自容量，但按用户决定不要求容量相等；实体 identity、slot 顺序和生灭事件仍严格比较。
- 内容：两端必须记录内容 fingerprint；fingerprint 不同且 H 策略未决定时只能返回 `content-strategy-pending`，不能签发 parity certificate。

## 4. 明确不做

- 不修改旧 `Tools/NTSDParity`；它继续标记为 NTSD 2.4 历史工具。
- 不实现 C++ exporter、Unity exporter、自动启动正式 EXE、注入、hook、patch 或 instrumentation。
- 不修改 33 ms/3 ms、主 pass、输入、RNG、AI、碰撞、命中、生命周期或表现。
- 不修改 Slot 容量模型、用户批准例外或用户排除项。
- 不修改 Direction B 内容、DAT、PNG、WAV、Prefab、Scene 或 importer。
- 不把 synthetic self-test 或离线 comparator 写成运行时对齐证书。

## 5. Schema 最小要求

Header 至少包含：

- schema、kind、producer、authority identity；
- scenario id/version、first completed tick、expected tick count；
- normal/fast logic interval；
- producer slot capacity（只记录，不要求跨端相等）；
- content policy 与 content manifest SHA-256；
- 当前批准例外和排除项；
- domain order。

每个 completed tick 至少包含：

- `completedTick`；
- `input`；
- `rng`，且含 CRT 与 synchronized 两个子流；
- `world`；
- `entities`；
- `relations`；
- `rests`；
- `events`；
- `presentation`；
- 每域 SHA-256 与 overall SHA-256。

比较顺序固定为：

```text
input → rng → world → entities → relations → rests → events → presentation → overall
```

## 6. 验收

1. `dotnet build Tools/NTSD28Parity/NTSD28Parity.csproj -c Release` 为 0 error；
2. `self-test` 覆盖 valid equal、input first-difference、RNG call-site first-difference、skipped tick、forged hash、Slot 容量不同但允许、内容 fingerprint 未决、例外漂移、排除项漂移和额外 tick；
3. `contract` 输出稳定 schema descriptor；
4. `compare` 和 `validate` 对 malformed/forged/truncated/extra trace fail closed；
5. 所有相等结果仍为 `certificateEligible=false`，直到后续正式双端 exporter 和 runtime evidence 建立；
6. `Tools/Validate-ChangeLedger.ps1` 覆盖本包全部 `.cs`；
7. 不产生 Unity/C++/资源行为变更。

## 7. 回滚

删除 `Tools/NTSD28Parity` 新目录，并把本 Change 标为 `ROLLED_BACK`；保留 Task/Change 及失败证据。不得删除或回退用户文件、旧 parity 工具或本轮对齐总表。

## 8. 实际结果

- Release build：`0 warning / 0 error`。
- Self-test：`13/13 passed`，覆盖合同、合法 trace、不同容量、input/RNG 首差、跳 tick、伪造 hash、slot 越界、内容策略未决、例外/排除漂移、额外 tick 和 malformed 必需 tick。
- Contract 连续两次输出 SHA-256：`5B5E4ABFF9842714D3DBDD30DE08C8261E35C11FE771AFF48E0B7E4CB56DED5C`。
- `dotnet format --verify-no-changes`：通过。
- Global Ledger：本包 generated `.g.cs` 已通过本地 `.gitignore` 排除；独立治理修复包关闭旧 governance-only Record 的元数据阻塞后，全局 validator 通过。本包没有未记录 authored `.cs`。
- 未运行 Unity/C++ runtime exporter；不宣称 runtime parity。
