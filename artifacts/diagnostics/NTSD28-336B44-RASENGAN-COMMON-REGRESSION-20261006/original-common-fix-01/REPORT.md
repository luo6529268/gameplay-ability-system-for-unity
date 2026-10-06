# NTSD28-336B44-RASENGAN-COMMON-REGRESSION-20261006 original shared fix
Status SCOPED_COMMON_FIX_VERIFIED. Useractualreference rootNTSD.exe5EDA confirmed. Two behaviorreports use originalobservable/staticDirectSound evidence. No global authoritychange; root336 observedbugs retained as history, not completion. Changes planned before scripts, operation NTSD28-ORIGINAL-COMMON-AUDIO-CATCH-FIX-20261006.

## 已实施的共用修复

1. `NTSDSoundPlayer` 按原始 cue 身份复用同一个战斗 voice，每次触发 Stop/rewind/Play；同 tick 的相同 cue 汇总后播放一次，不同 tick 不合并。内置 SFX 和 DAT 动态声音仍为独立身份，非战斗一次性声音保持原路径。正式声音 PCM 不改。
2. 原 DirectSound mono 声音在中间位置向左右声道各发送原 PCM。Unity mono 的平衡方式且 volume 上限为 1，因此在既有预热中建立左右相同 PCM 的 stereo 播放副本，保持原 AudioClip/WAV。副本由声音组件所有，catalog 更换和销毁时释放；异步 continuation 的世代检查防止销毁后重建，部分准备失败回收本批副本。
3. `BattleCpointWriter` 在抓取对象消失或失去 cpoint2、仍有互相对应的持有特殊攻击、且 DAT 已写有有限收尾链时，保留攻击者当前帧并继续既有帧推进。既有 WPoint 与持有物尾部执行释放/消除。没有角色、OID 或鸣人帧号判断，不直接消除技能，不改 DAT。普通武器、有效抓取、没有消耗出口及无效关系保持原分支。

## 权威和证据边界

用户本次明确要求按实际原版 `NTSD.exe` 的两项可观察表现修复，SHA-256 `5EDA51440039099069D041E5BD13FDE8BE9FD8C7B20983985B1712A59719D86B`。其它战斗规则仍按正式 336B44；不晋升任何候选 EXE。

原版静态声音路径包含每个身份的同批汇总及共享声音 Stop/rewind/Play。原版 cpoint 失效分支也会写攻击者 0，最终如何消除手持技能的原版写入点尚未证明。本批抓取修复是用户要求的、由 DAT 收尾链限定的 Unity 适配，不能称逐行复刻原版算法或消除 tick 精确一致。

## 当前验证

原项目 Editor 定向 job `7420cacebc784cf6982d8643dd111c24` 实际选择 **32 项，32 PASS，0 FAIL，0 SKIP**，运行 131.509 秒；结果 `common-green-result05.json`。发现库 total8993 不是本次执行数量。

- 抓取收尾 11 项：两个不同收尾序列、对象消失、有效抓取、普通武器、循环/缺出口、无效关系，以及当前 Logan DAT 的完整 Driver 敌方影分身/本体两种坐标各例。
- 共用声音 10 项：053、nar-a7、saku-tra 重触发，同 tick 汇总/后 tick 重启，不同 cue 与内置/raw 隔离，voice 回收，volume 上限和 mono PCM 副本，组件所有权及非战斗声音隔离。
- 环境伤害/落地声音 10 项，保持伤害、状态及原 builtin016 声音身份。
- 053 原 PCM 检查 1 项：5746 样本、11025 Hz、源 mono；解码最大误差 0，播放副本 11492 个左右声道值均为源样本，同 cue 两次触发只占一个 voice。

完整 Driver 中两种坐标均在 35 tick 抓到分身，随后沿 122/123/.../262 收尾，45 tick 不再有 434；保留了初始 RED、断言修正和历次结果。原 saved Battle Scene 的定向 Play 尚在运行，设备最终混音波形未测量。

## 原 Battle Scene 实际 Play

`battle-scene-01.json` 已 PASS：运行 87 个逻辑 tick，P1 通过生产 Manual Driver 的离散 D/F/J 输入生成/持有螺旋丸，P2 的真实 DAT272 生成影分身。第 71 tick 攻击、第 77 tick 抓到 owner=P2/slot54 的影分身、第 87 tick 螺旋丸实体移除且当前中央渲染快照无可见434。

持续053有44次实际声音事件，始终使用同一 AudioSource（ID -1599224），最多一个同 cue voice，实际 isPlaying 为 true、播放 clip 两声道；拒绝/voice丢弃均0。本轮有965个组件所有的播放PCM副本，退出 Play 后副本及 voice均0。11阶段关闭完成，World对象/slot/pool borrower均0；原 Scene clean且SHA前后相同 `253B2EBAD322AA6EC18488DFBBF555BB93AD897940A342E4130E77CE778F9010`。

这是真实原 Battle Play 和生产模拟/音效出口验证，使用诊断初态（Play内Naruto/Naruto roster、P2真实OPoint生成、仅一个影分身放近，其余停放、AI关闭）；输入是离散FrameInputSet，不是本次物理InputAction验收。没有采集GPU像素或设备最终混音波形，也没有重新测量原NTSD.exe消除tick。场景与DAT不改。

## 其他角色及边界

声音入口统一消费已发布声音，预热的 `CollectBattleSoundIds` 遍历全部 `TotalCharacterFrameConfig` 的 hit/drop/broken及各帧声音。本轮直接用053、nar-a7、saku-tra验证同一共用播放逻辑，不扩展成全角色逐技能人工重跑。

只读 catalog/DAT 结构审查找到以下五类“type0角色 + kind2持有type3 + catchingact + child终端帧”的候选，沿 catchingact 的next走到首个非cpoint1帧后：

| 角色 | 持有特殊对象 | 实际收尾数据 | 本次适配可达性 |
| --- | --- | --- | --- |
| 鸣人 | 434 ras | 120→122…128→260→261→262；WPoint kind3/act399，ras399 next1000 | 满足；完整Driver与Scene通过 |
| 千代 | 420 pup | 321→334；WPoint kind1/act428 | 不满足，不改变 |
| 雏田 | 444 bya | 120→344 kind1/act35；360→361无消耗终端 | 不满足，不改变 |
| 宁次 | 444/445 bya/bya2 | 120/295等→344/379；kind1/act35 | 不满足，不改变 |
| 蝎 | 213 pup | 120→237；kind1/act25 | 不满足，不改变 |

以上是静态数据可达性结论，不代表每个角色都已运行实测。生产代码没有角色常量；另用两个不同toy收尾帧证明规则由数据决定。普通抓取、普通武器、缺出口、循环及关系无效负例保持原行为。

只读审查提出 WeaponAct>=1000 未限制 Kind；现有 `BattleHeldObjectWriter` 的终端处理本来就不限制Kind，因此保持相同出口合同。另加 CaughtSlotIndex<0 时不进入适配，避免没有抓取目标的技能被误当作失去目标；这只是收窄条件，不改变已通过Scene中有实际抓取slot54的分支。该 guard 和修正后的旧诊断验收标准，正在跑唯一追加的7项检查。

**上述额外索引 guard 已撤销。** 追加7项中的两个完整Driver分身案例证明：真实命中之后，分身回收会清空 `CaughtSlotIndex`，所以不能用负索引拒绝技能收尾。未交付的 guard 候选失败文件保留；移除其两行后 `BattleCpointWriter` 字节恢复到原Scene PASS与32 PASS所验证的版本。新增一项“目标索引已清空仍按已有有限收尾链继续”的边界检查；旧held-clone诊断改用用户本次明确的消耗验收，本体仍对照原formal336。只复验这7项。

## 保护及留痕

681项受保护文件当前SHA全部稳定，Operation保留原字节/原dirty及最后guard前备份。本批没有删除/移动文件、修改DAT/角色图/WAV/Scene/InputActions或非战斗代码。Change Ledger用现有pwsh执行通过；Windows PowerShell直接执行时的两次host/行尾warning异常已保留日志，没有修改validator或Git设置。

最终资源审计 `final-protected-resource-audit.json`：681保护文件、978战斗WAV均0差异，18份操作前备份哈希一致，原版5EDA及正式336 EXE身份保持。`git diff --check` 全部工作区diff返回0。

播放适配的资源成本：965个mono WAV的stereo float32 PCM副本按音频头样本数估算为220188696字节（约209.99 MiB），见 `runtime-pcm-static-estimate.json`，不是Unity驻留内存/峰值实测。原Scene退出Play确实观察到副本计数0，仍需将这项预热内存成本作为交付边界记录；本批没有授权或实施另一个声音资源架构/移动端优化任务。

Final guard candidate rejected before delivery: jobc14eb47875be4843961c5496405bc59f selected7, two complete-Driver clone cases failed, other5 passed. Actual lifecycle clears CaughtSlotIndex after a real caught clone disappears, so negative index cannot distinguish no-hit from completed hit. Remove only proposed two-line CaughtSlotIndex guard, retain original data-authored finite chain; new boundary test instead requires continued completion after target index is cleared. Existing Scene PASS87 and32PASS correspond to unguarded production. Historical candidate failure retained. 2026-10-06T06:19:16.922105+00:00

## 最终状态

2026-10-06 scoped common restoration VERIFIED: NTSD28-ORIGINAL-COMMON-CUE-RETRIGGER-001 / VERIFIED; NTSD28-ORIGINAL-CATCH-SKILL-COMPLETION-001 / VERIFIED. Original Editor32/32 PASS (7420cacebc784cf6982d8643dd111c24), final affected7/7 PASS (2469f52667044d4f979530394ea10d2e); original saved Battle Scene01 PASS87: actual P1 DFJ, enemy owned33 caught77, 434 consumed/invisible87, 05344events one actual voice; no rejected/dropped. Ordered11-stage shutdown objects/slot/borrower0, audio copies/voices0, Scene clean/SHA unchanged. Proposed negative-CaughtSlot guard rejected by prior7 run(twofail), removed; final Cpoint bytes equal Scene/32 version. All evidence retained. Other-role audio sink common; five held-skill candidate DAT families audited, only Naruto actual chain qualifies; not full roster/runtime/GPU/native exact tick or final device waveform parity. PCM stereo payload estimate210MiB, not measured RSS. Protected681/WAV978/backup18/formal336/original5EDA stable. Task NTSD28-336B44-RASENGAN-COMMON-REGRESSION-20261006 / SCOPED_COMMON_FIX_VERIFIED. No DAT/Scene/resource/InputAction/nonbattle edits, file deletion/move, Git discard/commit/push, extra Editor or old goal reopening. Report: artifacts/diagnostics/NTSD28-336B44-RASENGAN-COMMON-REGRESSION-20261006/original-common-fix-01/REPORT.md.

最后追加7项实际 PASS7/FAIL0/SKIP0，127.610秒；依据 final-cleared-target-result02.json，保留失败候选与超时原件。最终没有负索引guard，新增目标回收后继续收尾正例通过。实际执行数量共32加追加7，包含重复对照，不称39个独立场景。
