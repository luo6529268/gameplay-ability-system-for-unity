# C053 OID808 完整战斗 tick 可达性（2026-10-01）

**2026-10-01 更正：** 下文将根回放 failure46 归因于“headless 只重建两人”已被原始trace否定。根tick0有slot2/OID875，但action0；源码夹具要求action55，tick1该对象消失，故HP和1500/1000不符。正式CLI仅有slot0/1初始动作覆盖。保留本报告的历史采集叙述，当前载体归因与边界以[只读更正](ROOT-CARRIER-CORRECTION-20261001.md)为准，C053/Q07仍开放。

权威根 EXE SHA-256 为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。诊断脚本只链接该发行版对应 playable 源码，消费正式 `resources/runtime` DAT；没有改正式 DAT、Unity 生产代码或 Scene。初态为**受控正式帧**：OID702（另有 OID63 对照）从 action553 进入 554，目标 OID2、种子 `682973786`、mode0、中性输入；三人例另设 OID875/action55/team2，其位置分别为 X580/620/650/700。受控帧与第三对象不等于玩家按键自然选招。

| 证据层 | 结果 | 限定 |
|---|---|---|
| v1 旧夹具 | 直接设 action554 的五组 120 tick 均未生成 OID808。 | 源码 `materialize_native_frame_zero_entry` 要求帧计数为零；该初态绕过了正式前驱，故不是生产阴性。原输出保留。 |
| v2 完整 Driver | 改为正式前驱 553 后，OID702 的 X550/700/1200、OID63 的 X550/700 五组均在 tick1 生成 OID808/slot50；tick6→153、tick7→155、tick8→156，子体命中数为零。 | 证明生成与接地链可达，未证明 C053 Uj。 |
| v3 两人源/根 | OID702/X700 两次源 CSV、RNG、LFR 各逐字节一致；正式根回放 `passed=true/failureCode=0/completedTicks=121`，声明的 16 字段×120 tick＝1920/1920 无首差。 | 根报告 `nativeParityClaim=false`；仅声明字段，无 Uj 命中。见 `o702-x700-source-root-comparison.json`。 |
| v4 三人完整 Driver | 四个第三对象 X 均在 tick7 产生一次 OID808 标准命中；`effect=2`、applied=1、Uj响应 action156。X620 的 CSV tick7 记录命中前 action153/latch153，命中后 action156/latch156，tick snapshot155；子体出生 tick1，153 tick6。 | 此路径进入泛型 Uj，但当前动作帧与锁存动作帧所给 Uj 都是 156，**不能区分 C053 修复前后**。四组各单次运行；三人阳性尚未做独立双跑。 |
| 根 EXE 三人 LFR | X620 回放 `passed=false/failureCode=46`，源首行 HP 总和1500、根仅两人1000。 | 现用 headless 回放 CLI 只重建两名战斗者；这是载体不等价，不能据此裁决战斗规则或声称三人正式根同态。见 `o702-t700-a620-root-01-report.json`。 |

v4 的 X620/第三对象单命中已于后续原位重新运行到新目录 `o702-t700-a620-04b`，120 tick 的 CSV、RNG、LFR 各自与原件 SHA-256 相同；表中“四组各单次运行”的历史说明仅描述首次采集时点。

## v5：同一 tick 双攻击者形成分读鉴别条件

脚本修改前在本 ID 的 Task/Change 中登记了可选第四战斗对象；正式 DAT、source、Unity 生产和 Scene 均未改。当前正式28 Core+3 playable 源链接 v5 编译 exit0、日志0字节，可执行 SHA-256 `E25F5FBA35D9FCC349FB62516BB66EF4DC46F7398E44D5163E4C00EA02BAC6D1`。使用 OID702/action553、OID2/X700，加两名正式OID875/action55/team2（槽2、3），中性输入，完整 GameSession 120 tick：

| 攻击者 X（槽2/3） | tick7 的 OID808 子体命中序列 | 结果 |
|---|---|---|
| 620/620 | `2:0:2:156;3:0:2:156` | 2次 `applied`，2次 Uj |
| 580/620 | `2:0:2:156;3:0:2:156` | 同上 |
| 620/650 | `2:0:2:156;3:0:2:156` | 同上 |
| 620/700 | `2:0:2:156;3:0:2:156` | 同上 |

序列字段依次为攻击者槽、标准命中状态（0=`applied`）、ITR effect、type3 响应动作。X620/620 的 tick6 子体动作为153；tick7 前动作/锁存均153，tick7 两次响应均156，tick后动作/锁存均156，快照155。正式 `battle_world.cpp` 的非零类型攻击者按槽序消费；泛型 type3 Uj 消费读 `action_latch`，只写 `frame.action`，首次命中后本 tick 尚未将锁存改为156。正式 `a/kat/kat.dat` frame153 的 `hit_Uj=156`，frame156 没有 `hit_Uj`，若第二次误读当前156会取默认20。因此**依据源码消费顺序推断**第二次命中是分读鉴别阳性，且完整 Driver 记录响应156；记录本身没有逐命中导出锁存值，不能把推断写成观测字段。

X620/620 独立双跑 `05a`/`05b` 的完整 CSV、RNG、LFR 各逐字节相同：SHA-256 分别为 `27673FFC384195C22807E28796D2587E92F13D784EB9CA314A265C2325D17998`、`00ED0D2A09C1AF6D1AE17F512E089311D364E99C3F2793C5BF61650A0D61D952`、`A19BA2456A1900E0753B105B2D98F5B662C935DC754924A60A337DC2EDBA693C`。四人初态仍为受控正式内容，并非玩家按键自然选招；现有根 headless 回放只载两人，不能宣称根四人同态。下一需在**原 Unity Battle Scene**用相同两个攻击者和 OID808 出生条件做完整 Driver tick7 对照；若原 Editor 入口不足，至少给出清楚的静态/聚焦证据边界，不能据 v5 单独关闭 C053/Q07。

当前 336B44 `battle_world.cpp::resolve_confirmed_unarmored_standard_hit` 约 7034～7049 行，普通 Fj 读当前动作、Uj 读 `action_latch`。v4 单命中仍不可区分，已由上节 v5 同 tick 双命中形成正式源码可区分条件；下一步是原 Battle Scene 同条件验证。根四人回放载体仍不足，不要为本样本改 DAT。C053 父项保持 `FOCUSED_TEST_PASS / RUNTIME_PENDING`，Q07 与总目标开放。

**2026-10-01 后续续证：** 上句的原Scene待验已由独立包完成：8tick源/Unity 104/104字段相同，tick7双rest0→10，Play退出clean；池借用、根四人、物理键自然仍待。见 [原Scene报告](../NTSD28-336B44-Q07-C053-DOUBLE-UJ-SCENE-001/REPORT.md)。本报告保留源侧原始结论与当时待验状态。

X620 v4 原件 SHA-256：`source-ticks.csv` `E1C0ECC107EBAB300E712A1914B4D43B532C0C4E00277D47E595B9DDA49F62C3`；`source-rng.csv` `A39C948F5F4461330B32E6BED15259348751F2F9300D1351732885EBA8C55CE7`；`source-packets.lfr` `DF8AC9A398C6279A17612B500A4200860ACF9C77AC0BD1648103DA171E9F3FD5`。其余 v1～v4 原件留在本目录，不覆盖失败结果。
