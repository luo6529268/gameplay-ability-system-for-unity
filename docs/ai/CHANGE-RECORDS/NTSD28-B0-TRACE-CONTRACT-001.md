# NTSD28-B0-TRACE-CONTRACT-001 — NTSD 2.8 trace schema 与 first-difference 基线

<!-- CHANGE-RECORD
id: NTSD28-B0-TRACE-CONTRACT-001
status: FOCUSED_TEST_PASS
code-path: Tools/NTSD28Parity/Program.cs
code-path: Tools/NTSD28Parity/TraceContract.cs
code-path: Tools/NTSD28Parity/TraceComparator.cs
code-path: Tools/NTSD28Parity/TraceContractSelfTest.cs
authority: User instruction on 2026-09-02 to execute NTSD28-UNITY-BATTLE-REALIGNMENT-001; fixed-SHA NTSD 2.8-Logan executable and corresponding playable source in docs/ai/CURRENT-AUTHORITY.md.
evidence: TOOL-CONTRACT-READY / RELEASE-BUILD-0-WARN-0-ERROR / SELF-TEST-13-OF-13-PASS / CONTRACT-SHA256-5B5E4ABFF9842714D3DBDD30DE08C8261E35C11FE771AFF48E0B7E4CB56DED5C-STABLE / DOTNET-FORMAT-PASS / GLOBAL-LEDGER-PASS / NO-UNITY-OR-AUTHORITY-RUNTIME-CHANGE
-->

> 创建日期：2026-09-02  
> 最后更新：2026-09-02  
> 类型：tool / test / battle trace governance

## 1. 状态与范围

- 当前状态：`FOCUSED_TEST_PASS / TOOL_CONTRACT_READY / GLOBAL_LEDGER_PASS / EXPORTERS_NOT_STARTED`
- 所属 Work Package：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B0`
- 目标：建立独立 2.8 trace contract、validator、streaming comparator 与 self-test。
- 不属于本包：任何 exporter、Unity/C++ runtime、33 ms/3 ms、pass、输入、RNG 生产写入、Scene、资源和内容迁移。
- 关联：`Assets/NTSD/Docs/ntsd28-logan-vs-unity-battle-alignment.md`。

## 2. Authority / 需求依据

- 正式 EXE：`NTSD2.8-Logan.exe`，SHA-256 `1277B70BA030A1F33B625EEA20B43834325B280CEC555650BF43CD90A64DAF75`。
- 正式源码入口：`SimulationTickDriver28::step(...)`、`BattleWorld28`、`native_random.cpp`、`render_snapshot.cpp`。
- 用户明确要求：开始按 NTSD 2.8 完整对齐总表实施。
- Evidence 等级：authority identity/source `VERIFIED`；双端 runtime trace `UNKNOWN/PENDING`。

## 3. Unity / 工具原状与已确认差异

- 旧 `Tools/NTSDParity` 的 csproj、README、schema 和 runner 绑定 NTSD 2.4 C#、400 slot、Authority400 和 v3/v4 旧结构。
- 它不能成为 NTSD 2.8 exporter/comparator，也不能通过修改默认路径获得新权威资格。
- Unity 已有 checksum/snapshot/witness 能力，但没有 2.8 双 RNG、completed-tick、用户例外和内容策略合同。
- 新工具必须完全独立，正常情况下不引用 Unity assemblies 或旧 C# authority assembly。

## 4. 计划改动

| 文件 | 目标职责 |
|---|---|
| `Tools/NTSD28Parity/NTSD28Parity.csproj` | 独立 net8.0 console 工程，无旧 authority reference。 |
| `Program.cs` | `contract`、`validate`、`compare`、`self-test` 命令入口。 |
| `TraceContract.cs` | 固定 schema、authority identity、域顺序、例外/排除集合、canonical SHA-256。 |
| `TraceComparator.cs` | JSONL header/tick validation、hash verification、streaming first difference。 |
| `TraceContractSelfTest.cs` | synthetic/malicious trace regression。 |
| `README.md` | 范围、命令、非证书边界和后续 exporter 接入。 |

## 5. 不可回退边界

- 不修改 `Tools/NTSDParity` 或将其旧结果重命名为 2.8 证据。
- Slot 容量使用 Unity；header capacity 只记录不判等。
- 33 ms/3 ms 仅作为 header contract，不在本包修改 runtime。
- 用户批准例外和排除项必须以固定顺序进入 header，漂移即 fail closed。
- 内容 manifest 不同时，在 H 策略未决定阶段不得返回 certificate eligible。
- 所有 `equal` 结果仍是 structure/self-test 等级，不是双端 runtime parity。

## 6. 实际改动

| 文件 | 实际改动 | 预期副作用 |
|---|---|---|
| `Tools/NTSD28Parity/NTSD28Parity.csproj` | 新建无旧 authority/Unity 引用的 net10.0 console 工程，warnings as errors；关闭 NuGet audit 并忽略不可用外部源。 | 仅离线 build output。 |
| `Program.cs` | 新增 `contract`、`validate`、`compare`、`self-test` 命令和稳定 JSON report 写入。 | 只写用户指定的输出路径。 |
| `TraceContract.cs` | 固定 authority SHA、33/3 ms、domain order、用户例外/排除、canonical JSON/SHA-256 和最小 entity/RNG 合同。 | 不接 runtime。 |
| `TraceComparator.cs` | 流式验证 header/completed tick、双 RNG、entity identity、域/overall hash、内容策略和 first difference。 | malformed/forged/truncated/extra 输入 fail closed。 |
| `TraceContractSelfTest.cs` | 13 个 contract/valid/equal/difference/malicious synthetic cases。 | synthetic only，不产生 parity certificate。 |
| `README.md` | 记录命令、范围和非证书边界。 | 文档。 |
| `.gitignore` | 忽略本工具 `bin/`、`obj/`，避免 generated `.g.cs` 被治理校验器误判为 authored script。 | 不影响源码或其他目录。 |

未修改旧 `Tools/NTSDParity`、Unity/C++ runtime、Scene、DAT 或资源。

## 7. 验收与证据

| 层级 | 命令 / 场景 | 实际结果 | 状态 |
|---|---|---|---|
| Build | `dotnet build Tools/NTSD28Parity/NTSD28Parity.csproj -c Release --no-restore` | 0 warning / 0 error | `PASS` |
| Self-test | `dotnet run --no-build ... -- self-test --output Temp/NTSD28Parity/self-test-final.json` | 13/13 passed / 0 failed | `PASS` |
| Contract | 连续两次 `contract` 输出 | SHA-256 均为 `5B5E4ABFF9842714D3DBDD30DE08C8261E35C11FE771AFF48E0B7E4CB56DED5C` | `PASS` |
| Format | `dotnet format ... --verify-no-changes --no-restore` | 无输出、exit 0 | `PASS` |
| Ledger | `pwsh ... Validate-ChangeLedger.ps1 -RepositoryRoot ...` | 64 Records；本包 4 个 authored `.cs` 全部由本 Change 覆盖；全局通过 | `PASS` |
| Unity/C++ runtime | 本包不运行 | exporter 尚未建立 | `NOT_IN_SCOPE` |

首次 `dotnet build` 未进入 C# 编译：NuGet.org TLS/凭据失败。随后 `--ignore-failed-sources` 在 warnings-as-errors 下仍被 `NU1801` 拒绝；关闭该次 restore 的 warnings-as-errors 后，SDK 10 又为 `net8.0` 请求本机未安装的 8.0.30 targeting pack。机器已安装 .NET 10.0.11 targeting pack，故本独立工具改为 net10.0。首次真正源码编译发现 `TraceComparator.cs` 缺 `System.Text.Json`，产生 2 个 `CS0103`；补充唯一 `using` 后最终 build 通过。首次 self-test 为 11/12：malformed JSON 被放在声明范围外，正确先判成 extra tick；fixture 改为替换必需 tick 后最终 12/12。随后增加 producer 自身 slot-capacity 内部校验，最终为 13/13。所有中间失败均保留，未包装为一次成功。

## 8. 风险、回滚与未关闭项

- 风险：过早冻结不完整字段；因此 v1 只固定 envelope/domain/必备双 RNG 和 first-difference 顺序，具体 entity 子字段由后续 B0 字段包扩展并版本化。
- 风险：synthetic equal 被误报为 parity；工具必须硬编码 `certificateEligible=false`。
- 风险：用户批准 exception 被静默归一化后掩盖其他漂移；v1 只记录 exception，不自动 normalize。后续 exporter 必须选择未激活 gameplay 例外的场景，或另建版本化 projection。
- 未关闭：C++ exporter、Unity exporter、真实 scenario、字段全量映射、H 内容策略。
- 原治理阻塞已由独立 `CHANGE-LEDGER-GOVERNANCE-ONLY-METADATA-001` 真实建模并关闭；本包没有借机修改其外部范围。
- 回滚：仅删除本包新目录；保留记录和失败证据，不触碰旧工具/用户文件。

## 9. Git / 交接

- 修改前分支：`NTSD_2.8_C++`。
- 修改前 HEAD：`d12cb9749c8255fe0bfa399ed1f36cd5f5e9a3be`。
- 修改前已有本轮文档迁移 diff 和用户未跟踪 `.claude/`；均不回退、不清理。
- 提交：未请求，不提交、不 push。
- 实际 diff：仅本 Task/Record/直接治理文档与新 `Tools/NTSD28Parity`；旧 parity、Unity/C++ runtime、Scene、资源均未改。
