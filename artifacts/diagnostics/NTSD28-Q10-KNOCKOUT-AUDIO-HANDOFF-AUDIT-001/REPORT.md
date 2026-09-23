# Q10 击倒提示音频交接审计（2026-09-23）

状态：`STATIC_FIRST_DIFFERENCE_CONFIRMED / IMPLEMENTATION_AND_RUNTIME_PENDING`。本报告只读正式 playable 调用链与当前 Unity 原项目脚本；未改音频、脚本、Scene 或资源，未运行 Unity/正式 EXE 同条件音频测试。

## 正式行为

- 当前正式 `NTSD2.8-Logan.exe` SHA-256 为 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。`source/ntsd28_playable/src/game_session.cpp:3248-3251,3292-3304` 在模拟 tick 前记下击倒事件数量，tick/结果尾部后仅遍历当 tick 新增事件，按顺序调用 `native_knockout_feed_audio_event`，把有效音频追加到本 tick `audio_events`，随后裁剪击倒尾部。
- `source/ntsd28_core/src/rendering/render_snapshot.cpp:858-892` 的音频门槛为 battle mode `1`、`runtime_display_enabled`、victim slot 仍存在且 battle group 为 `1` 或 `5`，并有有效 channel 或非空 resource path。音频不检查图文 `bound`、mode/id draw lists。事件取 victim 的 world X；有 path 时 source 为 `mode_sound`，否则为 `builtin_channel`。
- 正式选定 `decoded_dat/data/mode/ntsd.dat:81-82` 的 `sound1`/`sound2` 分别为 `data\\m_ok.wav` 与 `data\\m_join.wav`；项目暂存的对应 DAT 已包含同字段。需在最终事件见证中核对这两个 path 与 victim 组别。

## 当前 Unity 对应与首差

- `LoganModeKnockoutFeedInput` 已从正式 mode 子表捕获 `StageTeam1DeathSoundPath`/`StageTeam5DeathSoundPath`，测试断言了选定文本值；`SimulationWorld.RecordNativeKnockout` 已记录逐次击倒事件；`NTSDBattleTickSystem.RunReleaseTick` 在 `BattleResultsFlow` 后裁剪尾部。
- 当前脚本搜索显示上述两个声音字段只有解析赋值和测试读取，没有生产调用者。`NTSDBattleTickSystem.cs:549-558` 在裁剪前没有遍历本 tick 新增击倒事件并入队音频。`SimulationTickDriver.cs:638-642,688-723` 只会发布 World 中已有的 `PendingSounds` 并交给 `NTSDSoundPlayer`，故该正式 mode 音频事件目前缺生产入口。这是源码级首差，不能据此宣称已经测得静音。
- 实施时先固定 tick 前事件数量，在现有 tick/结果尾部后、裁剪前按事件原顺序筛选并入队；遵守 mode/display/存活 victim/组别/path/channel 门槛，不沿用图文 `bound` 和 draw-list 筛选。需处理现有 `PendingSoundEvent` 只承载 cue/worldX/tick、未承载 native channel/source 的差异，并确认内存快照、锁步、停止接单和音频发布顺序。不要重做 Q06 已闭合的 frame-sound producer。

## 验证与依赖

当前仅为静态差异。下一独立 Q10 Task/Change 应先用当前正式 playable 见证 group 1/5、非 mode 1、display off、无效 victim 和同 tick 多事件的音频 event 结构与顺序，再写 Unity 聚焦测试和最小生产接线。之后以原项目 Editor 验证同 tick 事件、clip 解析/播放、退出重进，并与正式 EXE 同条件可听结果对照。Q10 现有 976 cue 内容清单、905 本地同路径缺失及 WAV 部署边界仍由 `NTSD28-Q10-FRAME-SOUND-ENTRY-READINESS-001/REPORT.md` 跟踪；D-023 不自动授权整体替换 WAV。

审计时原项目没有运行中的 Unity Editor。运行中的 `I:\\UnityPreject\\test` 是独立项目，不能作为原项目编译、NUnit 或 Play 证据。
