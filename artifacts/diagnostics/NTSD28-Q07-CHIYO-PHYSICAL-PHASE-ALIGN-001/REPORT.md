# Q07 千代物理输入与显式中立步混用的阴性单例

状态：`DIAGNOSTIC_COMBINATION_FAILED / SCRIPT_HUNK_ROLLED_BACK`。原Battle Editor探针更正物理按键映射后的单例 `chiyo-physical-correctmap-20260928-01.json` 已证明P1按钮、按下、释放各120/120，并自然到傀儡407；但首测输出phase0，正式及直接canonical例首测phase1。本包曾尝试在物理例测量前复用直接canonical例的显式 `FrameInputSet` 中立完整Driver步，只改探针 `Poll()` 一处条件。

原Editor编译后的唯一新Play报告为 [chiyo-physical-phasealigned-20260928-01.json](../NTSD28-Q07-CHIYO-CONTROL-NATURAL-PLAY-001/chiyo-physical-phasealigned-20260928-01.json)：测量前 `startTick=6`、`initialInputPhase=0`，首测tick7输出phase1。报告状态 `OBSERVED_DIFFERENCE`，首差为相对tick1物理L对应的canonical `None`；120个测量tick的P1 canonical均为None，OID419/854/千代420/傀儡400/407均未到达。此轮只证明“显式中立步+该物理注入路径”组合失效，不能当作正式版与Unity千代战斗规则首差；静态阅读Driver重载不足以确定是何处抑制了动作回调。

原始JSON SHA-256：`0B542A5E68E829A2ECFC813EEC804A5746315603CD1C196DA0807C074B1D789C`。独立读取120行：`canonicalButtons`、`canonicalPressed`、`canonicalReleased` 各120/120为None。旧成功物理例仍为`chiyo-physical-correctmap-20260928-01.json` SHA-256 `4369EBEFFE4DD86EE50866C6CFB3802D549406D78C200E4BB3349962B5981ABF`；不能以这份阴性覆盖它。

探针遵守十一阶段有序退场：`stopped/detached=true`，World objects/runtime slots/pool borrowers均0，键盘neutral、退出Play，Battle Scene前后同SHA且clean。该唯一脚本改动已原位撤回，原物理P1成功报告、直接canonical相位对照及所有旧阴性报告均保留。未改生产战斗逻辑、DAT、图片、Scene、Input Actions或非战斗功能；Q07/BATCH-04、D-024及总目标仍开放。后续若确需物理同相位证书，应先增加动作状态/回调/采样边界的可观测字段并确定精确原因，而不是重复长时间Play。

撤回后原Editor的`Assembly-CSharp-Editor.dll`时间晚于恢复源码；MCP `read_console`按该探针名筛选 error 为0条，`manage_scene/get_active`读回唯一`NTSD_Battle`场景`isDirty=false`。四个保护文件最终SHA-256：Battle `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`、Menu `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`、GameConfig `0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`、项目模式Asset `B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`，与本包前的已记录基线一致。本轮不重复旧成功用例。
