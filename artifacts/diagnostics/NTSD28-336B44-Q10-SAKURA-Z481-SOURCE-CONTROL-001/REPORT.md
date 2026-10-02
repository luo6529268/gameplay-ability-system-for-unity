# Q10 Sakura 自然双声道 cue：正式 Z481 对照

2026-10-02。状态：`VERIFIED_SCOPED_SOURCE_AND_ROOT_CONTROL / UNITY_NATURAL_PLAY_PENDING`。原 Unity Battle Scene 的项目地图会把此前诊断请求的源规则 Z650 钳位到 Z481；本包仅检验这一初始纵深变化是否改变当前 336B44 正式版的小樱自然选招与发声。结论是所选输入链的动作和发声事件未变。不能由此判定 Unity 已实际播放声音。

权威根 `NTSD2.8-Logan.exe` 前后 SHA-256 为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。只在 `Tools/NTSD28Q10Diagnostics/sakura_stereo_natural_reach_probe.cpp` 增加可选初始源 Z 参数，取值 0..1000，省略时仍为 650；两名角色使用相同初始 Z。没有修改正式源码、DAT、Unity 生产代码、地图、场景或相机。按先前 28 Core + playable live path 编译参数生成诊断 EXE，`g++` 退出 0，产物 4,818,221 字节。最初的 PowerShell 包装器在编译成功后读取不存在的空 compiler-output 文件而返回 1；它不是编译器错误。

输入保持小樱 OID1、HP100、action0、X500，李 OID7、X1200，防御2 tick→纵深上2 tick→攻击2 tick→action172 后下一 tick 新按跳跃。正式资源根均为当前 `resources/runtime`。Z481 两次独立运行 [b](z481-b/source-events.csv)、[c](z481-c/source-events.csv) 均退出0，55 tick 的 `timing-grid.csv` SHA 均 `084644387B3E962831E08DFC9D07263EFE0D4CFF69A88FE877E68725DF5F03A3`，`source-events.csv` SHA 均 `DCBB853A5634D5A7F8A57B391D0BB439A304B2648E9261662022FF2879FDEA77`，LFR SHA 均 `B6FCC51E94C2853A4B814D6F3DA30A717D5F1E6B66A7335CCFA0AFDE8CF3BDC9`。省略参数的 Z650 控制运行，其 grid/CSV 与 Z481 全同，LFR SHA `F1805AAD31EC3BEC8ADD07BE299BC3EEEC8F0FBC1B8A155A1AB487BB5190FEED` 也与原 Z650 样本一致；LFR 因初始位置不同而与 Z481 字节不同。

两种初始 Z 均在 tick6 进入 action240/MP150，tick23 为 action172，tick24 新跳跃进入 action340，同时正式 `GameSession28::last_tick().audio_events` 有 `c/saku/w/tra.wav`、source1/frame_sound、规则 X500。Z481 录制再由根目录正式 EXE 以 `--resource-root` 和 `--complete-vfs-root` 同指当前 runtime 回放：[报告](root-z481-02-report.json) `passed=true/failureCode=0`、进程退出0、55个声明tick/56个宿主完成tick；[逐 tick 对照](root-z481-02-comparison.json) 在声明的55tick上比较小樱动作、MP、相机X，共165/165同值、首差0。[根trace](root-z481-02-trace.jsonl) SHA `ECC13E963948ADCABA9C5F3744AB1C216014F5BFFB78BF60122B3B282E7319C6`。第56个宿主末步不计入同态。根报告的 `nativeParityClaim=false` 为内建边界，根trace不导出逐条音效事件；音效身份和时点直接证据来自对应 playable live source。

失败原件保留：[z481-a](z481-a/) 首次误把 `decoded_dat`/`vfs` 子目录分别当完整 runtime 根，诊断退出5；首次根 [失败报告](root-z481-report.json) 漏传资源根，初始化报告 failureCode45/BGM selection mismatch，完成0tick。更正参数后使用新的 02 文件名，无覆盖或删除。原 Unity Editor 的程序集时间仍早于更正后的 Q10 物理键探针；全量误启动测试是否真正停稳尚未由新程序集证明。本包未运行原 Unity Scene，下一步在原 Editor 空闲并重编后仅以正确 P1 `L`防御、`W`上、`J`攻击、`K`跳跃执行已登记的定向自然 Play，并核待播 cue、voice、双声道输出和退出清理。Q07、Q10、Q12 和总目标继续开放。

交付检查：`Tools/Validate-ChangeLedger.ps1` 退出0/PASSED，新改 Tools 文件由本 Change ID 覆盖；`git diff --check` 退出0。保护基线中的 Battle/Menu Scene、ProjectBattleModeConfig Asset 和正式 Sakura WAV 四项 SHA-256 均未变。[完整审计输出](change-ledger-validation.txt)留存；其中既有其他 Record 的非当前 diff 警告不改变本包结果。
