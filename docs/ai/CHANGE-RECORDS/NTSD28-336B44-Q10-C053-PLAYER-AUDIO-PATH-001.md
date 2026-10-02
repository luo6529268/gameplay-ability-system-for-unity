<!-- CHANGE-RECORD
id: NTSD28-336B44-Q10-C053-PLAYER-AUDIO-PATH-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07WindowsContentBuildProcessor.cs
code-path: Assets/NTSD/Scripts/App/NTSDSoundPlayer.cs
code-path: Assets/NTSD/Scripts/Test/Editor/SoundPresentationDispatchEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/NTSD28Q07WindowsPlayerRuntimeProbe.cs
authority: 336B44 formal battle 020/067 WAV; D-023 formal content; current GameConfig formal root; Windows Player content closure
evidence: docs/ai/TASKS/NTSD28-336B44-Q10-C053-PLAYER-AUDIO-PATH-001.md
-->

# Q10/C053 Windows Player 正式音频根与清单

脚本修改前建立。原状、唯一权威、首次差异、精确路径与符号、预期副作用、不可回退边界、验收和回滚见同 ID [Task](../TASKS/NTSD28-336B44-Q10-C053-PLAYER-AUDIO-PATH-001.md) 和[首差报告](../../../artifacts/diagnostics/NTSD28-336B44-Q10-C053-PLAYER-AUDIO-PATH-001/FIRST-DIFFERENCE.md)。只为当前已部署的正式两 WAV 修复 Windows Player 内容闭包与正式根解析；旧 Sound、非战斗与 DAT 数据不改。实际文件、构建、Player 和未验项待填。

脚本修改前的 v3 清单已新建，SHA `53FD2E7BDDDF4B4A7FDFAD66E7F82AE58313381962DE78D4A3D0A249CCA15B6E`：1373 行、46,927,691 字节；1348 文件与正式原始字节同，25 DAT 严格仅 CRLF/LF 不同，v2 SHA `5D3D9C302726541D3AFA477BC347F857E247FB047ECDB6ACAFF3857AA5EBE73F` 保持。准备进入声明的脚本改动。

**2026-10-02 实际改动与验收：** Windows postbuild 仅换 v3 清单路径、1373 件/46,927,691 字节常量；`NTSDSoundPlayer.ResolveFormalBattleSoundSourcePath` 从 `GameConfig.BattleContentRuntimeRoot` 的项目根解析 `vfs`，修正 Player `<EXE>_Data` 与 EXE 同级 sidecar 的差异。两项正式 WAV Editor 测试添加临时 GameConfig scope，使用后恢复原静态引用；Q07 Development Player probe 只在显式 Q10 参数时增加当前 catalog 身份一致性、020/067 路径/帧数和两次 `PresentSound` 的播放检查，默认固定指纹门不变。未改 DAT、场景、非战斗播放路径。

生成 Editor 工程编译 0 error/284 警告；原 Editor 具名 2/2 PASS（job `3612308b286645fc861173ccf619169a`）。首次 Player 构建成功、1373 文件逐 SHA 通过，但旧 Q07 指纹常量先拦截，`player-01.json` FAIL 原件保留。当前项目 mode Asset 纳入复合身份后 Player 指纹 `868070…`，三处发布键相同；按本包显式 Q10 probe 改为验证发布 catalog 复合身份和三键一致后，第二构建成功，打包审计 1373/1373、0 差。`player-02.json` PASS：正式 sidecar 020/067 的 `AudioClip.samples=16413/31170`，合成战斗事件实际播放计数 +2；Battle World=4，有序关闭 `RuntimeMapCleared`、池活跃借用0、发布资源存活0/30872。6保护文件 SHA 稳，旧 v2 manifest SHA 未变。完整命令、路径、限制和原始证据见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q10-C053-PLAYER-AUDIO-PATH-001/REPORT.md)。

本 Change 精确 Windows Player 内容与战斗音频路径出口 `VERIFIED`；未证设备出声、声像/音量、其它 cue、其它平台或 Player 自然 C053 命中，不将本包升格为 Q10 整组完成。回滚须按本包精确差异审查并遵循批准规则；未执行删除/回退。

交付前 `Tools/Validate-ChangeLedger.ps1` exit 0、`Change ledger validation PASSED`（其它历史 Record 无当前 diff 的警告保持）；`git diff --check` exit 0。Player 构建命令通过原 Editor 的 `Temp/NTSD28_Q07_WindowsBuild.request.json` 触发，实际两次输出为 `Q10C053PlayerAudio336B44-20261002-01/02`；运行 `Start-Process -WindowStyle Hidden` 并带 `-ntsd-q07-serialized-root-probe -ntsd-q10-formal-audio-probe`，未用 computer-use。

**复核后追加修正，脚本修改前重开 `IN_PROGRESS`：** 第一版把 Q10 新字段直接加到 Q07 基础 Report，即使未启用 Q10 也会改变原 Q07 JSON 结构；Task 明确要求默认 Q07 输出不变。只在同一已声明的 `NTSD28Q07WindowsPlayerRuntimeProbe.cs` 中把 Q10 字段移到显式 opt-in 子报告，并使用独立 runId 重跑 Player。原 01/02 结果保留，前述 `VERIFIED` 是修正前快照，待复核后恢复。

**03 复核出口：** `Q10AudioReport : Report` 只由显式 Q10 参数创建，基础 Report 与 Git HEAD 的15个序列化字段按名称和顺序相同，见 `default-report-schema-audit.json`。03 Windows Mono Development 构建0 error；1373/1373文件、46,927,691字节逐SHA无差；隐藏窗口 Player 03 `PASS`，正式020/067分别16413/31170采样帧、两次合成战斗事件实际播放+2、有序关闭借用0、发布资源存活0。六保护SHA在03后仍相同。默认Q07路径未另运行；其旧固定指纹门当前内容下不成立，报告结构不变仅有静态证据。至此本包 Windows Q10 内容/路径限定出口恢复 `VERIFIED`，Q10整体及其它平台待。
