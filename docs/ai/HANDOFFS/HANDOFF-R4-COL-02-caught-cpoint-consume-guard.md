# HANDOFF — R4-COL-02 C++ caught-cpoint `hurtable` current-candidate skip

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING`  
> Change ID：`R4-COL-002`  
> 唯一 authority：`J:\QQFile\NTSD2.4\ntsd_release` release live source；未运行、修改、构建或写入 C++ authority。

## 已完成

- 只读复核 C++ `collision.cpp:69-79,1253-1258`：在 vrest 与 C07-A attacker abort 后，target prev2
  cpoint.kind=2 / active catcher / caught-slot关系匹配 / catcher prev2 `hurtable`缺失或为0时，跳转
  `next_pair_outer`，即只跳过当前 candidate。
- 复核 Unity `BruteForceSceneQuery.IsReleaseConsumerPairBlocked` 已有等价 helper及 active-only catcher查询，
  但 shared `BattleHitCandidateSequenceRunner` 未使用它。
- 仅在 runner C07-A后、runtime ITR replacement前复用该 helper；blocked返回 `false`，沿用 sequence runner的
  current-candidate skip语义，不改 helper、candidate collection、CPoint writer、scheduler或 held/link。
- 新增 `CheckCaughtCpointConsumeGuardContracts`：exact/shared kind0 first-skip / second-hit continuation、
  `hurtable=1`正向对照以及kind6 blocked不写 `HitConfirmCounter`。

## 验证证据

| 检查 | 结果 |
|---|---|
| Unity scripts refresh / compile | 现有 Unity Editor / UnityMCP port 6401；refresh触发 domain reload时 TCP 连接预期关闭，随后 Console `error CS` 查询为 0。 |
| full `BattleRuntimeSelfCheck` | 通过菜单请求；`Temp/NTSD_BattleRuntimeSelfCheck.result=PASS`，最后写入 2026-08-22 04:01:04 +08:00。 |
| Change Ledger | `Tools/Validate-ChangeLedger.ps1` PASS：16 records / 13 governed code files。 |
| diff hygiene | `git diff --check` exit 0；仅已有 LF/CRLF warning。 |

## 未关闭 / 不得夸大

- 未做真实 cpoint / caught Play Mode；
- R1-WP02 C++ runtime trace仍 `BLOCKED`，不得运行 C++ executable；
- 本包不是完整 R4，也不是完整 battle alignment。

## 连续下一步（D-009）

不等待逐包确认，进入 `D-COL-003` 的只读 source preflight：闭合 C++ `effect=21` + target current state
18/19 的 entire-attacker abort范围、Unity collect-time prev-state filter与实际 consume-time gate的差异，以及最小
two-candidate fixture设计。先建立 Task Contract / Change Record，再决定是否需要最小脚本改动；不得合并
`D-COL-004` oid999、`D-COL-005` kind1或 R5 held/link。
