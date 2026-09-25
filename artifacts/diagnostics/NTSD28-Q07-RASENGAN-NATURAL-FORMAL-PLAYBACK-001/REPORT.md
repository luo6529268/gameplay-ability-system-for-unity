# Q07 鸣人自然组合键窗口：正式发行受控回放（2026-09-25）

范围：只验证明确初态与 L/D/K/J 相对 tick 输入的正式 playable 源码及根目录正式 EXE 回放。正式 EXE SHA-256 为 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。诊断用正式 `resources/runtime` 的 catalog、decoded DAT 和完整 VFS；不写原版目录，不改 DAT。自建 C++ 只编译 playable 构建清单中的 28 个 core `.cpp`、`game_session.cpp`、`selection_flow.cpp`、`lfr_recorder.cpp`、`game_session_lfr.cpp` 及新增诊断文件；MinGW GCC 15.1.0 编译 exit0、`compile.log` 空。

源桥初态为 Naruto OID2/action0/HP500/MP500、OID7/action0/HP500/MP500，分队1/2，X500/1200、Z650，背景23、固定BGM选择2、seed682973786。每例前2tick持续防御、3–4tick方向右、5–6tick跳跃，7tick起松键；首253、次253、254后用例分别在tick34、35、36起持续攻击2tick。该离散安排取自原Editor `NTSD28-Q07-RASENGAN-NATURAL-COMBO-PLAY-001` 的相对物理设备事件与2tu相位：formal tick2/4/6对应Unity已观测的防/方向/技能进入，formal tick33/34/35对应首253/次253/254。两端位置、对手、RNG初态未做同世界归一，故只对照这条输入/动作/资源子链。

| 输入例 | 正式源码桥里程碑 | 根正式EXE回放 |
|---|---|---|
| 首253后攻击 | tick33 action253/phase1；tick34 phase0采J→action301，MP350→250 | report PASS、exit0、55tick；逐tick所比字段0差 |
| 次253后攻击 | tick34 action253/phase0；tick35 action254/phase1到J，tick36 phase0才采J→仍254；MP350 | report PASS、exit0、55tick；逐tick所比字段0差 |
| 254后攻击 | tick35 action254/phase1；tick36 phase0采J→仍254；MP350 | report PASS、exit0、55tick；逐tick所比字段0差 |

三条正式EXE报告均 `passed=true/failureCode=0/declaredTicks=55/traceRows=57`。源码CSV和发行trace对 `tick、inputUpdatePhase、slot0 action、slot0 MP、slot0 current attack、slot0 previous attack` 共 `3×55×6=990` 个字段逐项比较，`990/990` 相等。未把 `pending` 纳入此等价计数：源码 `set_input` 每tick写 pending；LFR只序列化2tu采样流，正式回放在第二例tick35和37的 pending 位与源码桥不同，但 sampled `current/previous`、相位、动作和MP相同。第二例和254后例的 LFR SHA相同，说明两者在2tu相位0形成同一采样流；根EXE无法从这份LFR独立区分两次物理按键时点。第二例“phase1收到、phase0才采”的证据来自正式 playable 源码运行和 Unity Play，发行回放证明的是其采样后行为。`nativeParityClaim=false`，不是独立原版键盘/像素证明。

资源字段边界更正：前一 Unity 自然探针记载的 `mp=500` 实际读取 `actor.Runtime.MP`，而当前 Unity 输入资源写者 `BattleCharacterActionWriter` 扣的是 `character.Health.PP`（绑定 `NTSDEntityRuntime.PP`）。因此不能拿那列与正式 EXE `EntityState28.current_mp` 比较，也不能由此宣称 Unity 技能不扣资源。需用同一原Editor自然输入补采 `Health.PP` 和 `InputMpConsumedTotal350` 才能判断这一字段。此前探针的动作/输入相位结论不受这项字段标注影响。

关键原始文件及 SHA-256：`first253.lfr` `B26D37E9D131A0165C283B84612E1D6C016F2031E9808D8D99BBE6AED1145139`，`second253.lfr` 与 `after254.lfr` 均 `684107D01200F697801C5922F3083AA23713CCD61F2ED1F861F16FC804A10152`；三份 `*-source.csv`、`*-release-report.json`、`*-release-trace.jsonl` 在本目录，互不覆盖。新增诊断 EXE SHA `520603A117C4AA598390A99C7A402A50BAFD4CC0F475387C42317D3398829EFB`。完整文件可用 `Get-FileHash` 复核。

本包没有修改 Unity 生产、DAT、图片、Scene、Prefab 或 ProjectSettings。下一精确动作是补 Unity 自然 Play 的正式资源字段并对本报告的 tick7 MP350、首253转换tick34 MP250 作同相位限定对照；正式EXE独立物理按键与像素、Q07/R18及总目标继续开放。
