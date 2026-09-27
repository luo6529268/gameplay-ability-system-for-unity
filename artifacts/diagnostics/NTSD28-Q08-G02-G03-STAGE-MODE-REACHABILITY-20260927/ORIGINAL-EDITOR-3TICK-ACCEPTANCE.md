# Q08 G-02/G-03 鸣人分身 stage gate 三 tick 限定见证

2026-09-27。状态：`FOCUSED_TEST_PASS / PAIRED_SOURCE_VS_ORIGINAL_EDITOR_FIRST_DIFFERENCE`。本报告不关闭 Q07、Q08 或 BATCH-04，也不是正式根 EXE 的该边缘输入回放证书。

权威链：正式根 `NTSD2.8-Logan.exe` SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`；其普通战斗 HP3 LFR trace 已独立观察到 selected-mode stage gate 1。配对 playable 构建闭包的 `GameSession28::step` / `SimulationTickDriver28::step` / `BattleWorld28::settle_ordinary_stage_bounds` 在 gate 1 或 3 时将**全部** type0 slot 按角色边界裁剪，gate 0 时 slot >=20 允许左界 -100。另以正式 runtime `decoded_dat/data/data.txt` SHA-256 `3ED7DE4918AA7B5E94FE73A2B7D9B43DED9D10575DD182B9CA2B647EAE29A8F0` 与 `catalog.csv` SHA-256 `0AAB4A0FFEE70F17D31DDF952254C798DFEF185FEDD55CACEC28C7A66181E6FE` 固定内容。配对源码诊断 binary SHA-256 `A129837B524250387C4D5D14B98023E84F1E571639831BE3BB33BEDCA48DEA77`，只作为源码调用链见证，不冒称正式根 EXE。

同初态：Stage23 宽1330、Z542..712，seed682973786、mode0、difficulty1、无后续输入；slot0 鸣人 OID2/team1/X0/Z650/action121，slot1 李 OID7/team2/X1200/Z650/action0，二者 HP/baseHP/MP 500。原 Editor 仅使用既有 Editor raw exporter 新增的严格三 tick schema；其临时角色夹具按生产 `AppManager` 出生合同同时初始化物理与源规则坐标。所有生产脚本、DAT、Scene、模式 Asset 和非战斗入口未改。原 Unity Editor 原项目连接的程序集已在 `Assets/Refresh` 后重新生成；精确请求第 5 次 PASS；相邻旧 `input-common-two-entity` 三 tick 请求 PASS。

| 完成 tick | 配对 playable gate1 OID33 type0 slot50 X | 原 Editor 完整 Driver 源规则 X | 原 Editor 物理 X | 判断 |
|---:|---:|---:|---:|---|
| 1 | 未出生 | 未出生 | 未出生 | 同值 |
| 2 | -42，且本 tick 帧240→241 | -42，已初始化 | -42 | 同值 |
| 3 | 0，帧241→242 | -49，已初始化 | -49 | 首差，Unity 走 slot>=20 的普通界而非 gate1 角色界 |

gate0 对照源码同初态 tick3 为 -49，恰与当前 Unity 分支吻合。由此可将 G-02/G-03 的实现候选收窄到所选模式 stage gate 生产载体与 type0 PreFrame 边界 writer；不允许把这个差异推广到 D-025 非角色离开项目可行走区十秒例外，也不允许修改 DAT 值。物理 X 的比较只说明本夹具 ratio=1 的同态；D-024 固定全景相机下的比例位移另按原合同验证。

诊断更正：首次请求误把 `data.txt` SHA 写入 Unity exporter 所要求的 `catalog.csv` SHA 字段，被身份门拒绝；第二次请求因诊断预期错误（以为分身从 tick3 才参与帧 pass）在 tick2 计数3被拒绝。配对源码事件证明分身在出生 tick2 即参与帧 pass，修正后不放宽其他 schema。第四次请求发现临时夹具未初始化源规则坐标；这**不是生产缺陷证据**。第五次按生产出生合同补齐初始源坐标后才得到上表首差。旧 neutral 案例因当前旧内容缺 OID99 在资源门拒绝，随后改用现存旧 input-common 案例并 PASS。所有失败记录保留，不能用最终 PASS 覆盖历史原因。

原始证据：`naruto-clone-stage-gate1-action121.trace.jsonl` 与 gate0 同名对照；`unity-naruto-clone-stage-gate1.scenario.json`；`unity-naruto-clone-stage-gate1-5.raw.jsonl` 及 `.stage-source.jsonl`（SHA-256 分别为 `5998169B174D23921433815C122962DAEF1694B46B0E9409905DC24B94C78F01` / `A09E114316323C7661FB4335CE849AD06B2528D344D3E766EEA29F5F7CC6B765`）；`unity-naruto-clone-stage-gate1-5.result` 和 `adjacent-input-common-three-tick.result`。Editor 导出器最终 SHA-256 `BC6077465EF685605759037A771D9E412947B8BA804F5B41E216788829887DB6`。`dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -p:UseSharedCompilation=false` 在早一版诊断脚本上 0 error；最终版已在原 Editor 编译并执行上述 PASS，不能把 dotnet 检查混称最终版 Unity 编译。`Tools/Validate-ChangeLedger.ps1` PASSED，`git diff --check` 0 退出。Battle/Menu Scene 与模式 Asset SHA-256 分别保持 `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`、`785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`、`88E10D43B047952FD3053A87B7E5F60D0F87A6CD1A23313F37EA1503C686F55C`。

下一门：先取得正式**根 EXE**同边缘输入的直接 LFR/可见见证，或明确其正式可达入口局限；然后独立 Task/Change 接入项目 `ProjectBattleModeConfig` 中所选模式的等价 stage gate 到生产边界 writer，针对 gate1/gate0 正反与另一 type0 slot 做聚焦 RED→GREEN，并在原 Editor 完整 Driver 复测。不因单个测试诊断擅改模式 Asset、排除的 mode DAT 或非战斗选择流程。Q09 画面、Q10 音频、Q12 整场另按各自出口。
