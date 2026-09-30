# Q09/P-08 等当前/基础 HP 的正式根 LFR 限定入口

状态：`VERIFIED_SCOPED_ROOT_TRACE_ENTRY / P-08_OPEN`。本包只关闭同 HP 建局、自然受击与正式根 LFR 所列字段对照入口；**不关闭**正式根出血标记像素、Unity 同 World/同视口比较、P-08/Q09/BATCH-05 或总目标。正式根 `NTSD2.8-Logan.exe` 本轮前后 SHA-256 为 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。

新增且仅新增 `Tools/NTSD28Q09Diagnostics/ita_equal_hp_root_lfr_probe.cpp`。它链接正式 playable 的 core 28 源、`GameSession28`、`GameSessionLfr28`、LFR 编码器与所声明的原生资源；不修改发行 EXE、源码或资源。受控初态：mode0、正式背景 ID23 只供正式侧诊断、seed2833；鸣人 OID2 slot0 X500/Z650/HP500/基础HP500，鼬 OID9 slot1 X540/Z650/当前HP与基础HP均30、双方动作0、对立队伍。仅 tick1–2 对鸣人提交普通 Attack，此后无输入；建局后不写 HP、动作或帧。

配对 `GameSession28::step()` 完整 tick 的新探针于 tick8 自然令鼬 HP30→10，且存活；tick22 鼬回动作0，在 `RenderSnapshot28` 出现唯一 `bleed_mark`，阈值10、尺寸1×3、红 `0x00ff0000`。探针在所选 tick 将同一 Session 的22个输入/状态包录成9993字节 LFR；[原始逐 tick CSV](ita_equal_hp_source_ticks.csv) 与 [运行日志](run-01.log) 保留。加入 RNG 列后的第二份独立运行仍在 tick8/22达到相同状态，LFR SHA 与第一次完全相同：`ABD2B83A72E7DC7496228CE56BD2310E3AC69C67BECCC69354A7FA50416C5C55`，见 `run-02/`。

不改正式根 EXE，以该 LFR 和正式 `resources/runtime` 作为两个 VFS 根执行 headless playback；显式恢复两人的动作0、朝向0/1及 MP500。根进程退出0，[报告](formal-root-replay-report-01.json) 为 `passed=true / failureCode=0 / declaredTicks=22 / completedTicks=23 / nativeParityClaim=false`；tick23 是包装器附加终步，不计入声明的22 tick。根 trace 的 tick0 鼬当前HP/基础HP均30。机器[字段对照](source-root-field-comparison-01.json) 在声明的22 tick 比较鸣人动作/X及鼬动作/X/HP/基础HP，共132/132相同、0差异；包含 tick8 HP10、tick22 动作0/HP10。根 trace **没有出血命令或红点像素字段**，故只证明根角色状态到达同一条件；配对侧的标记由配对快照直接观察，不能转写为根 GPU 证据。

完整同初态仍有明确限制：配对侧 tick1 原生 CRT state=`1671198313`，根 LFR 回放为 `3374725112`，虽然已比字段在22 tick内相同，`nativeParityClaim=false` 也不能被报告的 `passed` 覆盖。根与配对的同 RNG、全 World 及与 Unity 的同视口对照仍待。旧 HP180/基础HP500 的三像素 WARP 案例保持独立，不与此 HP30 案例混成一场。

构建和失败留证：首次复用旧 WARP 链接参数时，缺 `-lz` 且误带 `-municode`，在链接阶段失败，原始 [`compile.log`](compile.log) 保留；一次 PowerShell 调用误把首个编译参数当可执行文件，未启动编译器、未生成 `compile-02.log`。精确修正为普通 `main` 和 zlib 链接后，[最终参数](compile-argv-04.txt) 对应 `compile-04.log` 为空、编译退出0；两次探针运行退出0，根回放退出0。没有启动第二 Unity Editor，也未运行 Unity 测试；本包不修改 Unity 生产，原 Unity 自然受击/Legacy 3 像素证据不例行重跑。

五个保护文件 SHA 未变：正式根 EXE 同首段；Battle Scene `3A089236328ACAE1510F8A831B77D4895CC34028DDCDEBE542BEF0DA8EC235ED`、Menu Scene `DD6A48A37FB8CEA9CD8A1F7738964A719E007A42F0FBB54FB48BBD0B723B9DC3`、GameConfig `0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`、ProjectBattleModeConfig `B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`。`Tools/Validate-ChangeLedger.ps1` exit0，日志首行 `Change ledger validation PASSED` 且本新Tools脚本有精确 `COVERED` 行；历史已声明路径不在当前diff的警告保留。[日志](ledger-validator.log)。`git -c core.safecrlf=false diff --check` exit0，新建源码/验收/Record 的尾随空白独立检查PASS。未修改 DAT、图片、Scene、Asset、Unity 脚本或非战斗代码。
