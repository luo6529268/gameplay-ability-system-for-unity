# Q07 佐助修后自然物理键 Play 回访（2026-09-25）

原项目已打开的 `NTSD_Battle` Scene 中，首次唯一请求 `q07-sasuke-state15-postfix-20260925` 在任何按键前由旧诊断预检拒绝，结果 `FAIL`（SHA-256 `A2E7926A39ED95C607CF5020944F1A1A42005C375A8A2E72CAF7D98DF0881461`）。旧探针只给佐助比较不带项目Mode Asset的过时语义指纹，失败JSON也没有列出各门实值。这不是已观察到的战斗出招失败。保留该结果，另用 `NTSD28-Q07-SASUKE-NATURAL-PROBE-IDENTITY-001` 只把佐助诊断身份门切到当前项目Mode指纹，并在失败消息中打印正式根、指纹、OID、`hit_Fa` 和 visual-key 是否存在；其它生产或探针门未动。

重编译后第二唯一请求 `q07-sasuke-state15-postfix-currentmode-20260925` 结果 `PASS`（SHA-256 `B9C10FF639BF9DC26F816EAE34CDEBBC0CCEA2B74F2C83C79B681447E13783A0`）。保存场景的真实物理 L/D/J 在逻辑 tick2/4/6 登记，PP500→400，frame264、四个 OID440／四个 stable ID及正式 `chi.png` pic0 中央绑定均被探针观察；`formalRootVerified=true`。探针结束后原 Editor 已回到 Edit Mode/idle。Battle/Menu Scene SHA-256仍为 `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`／`785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`，GameConfig asset仍为 `0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`。

这个现成探针的自然 Play 结果只记录出招、子体出生和图片绑定，没有记录 OID440 在 tick24 的 Vx。该具体速度修复由同初态26tick Manual RED→PASS与正式发行逐字段对照证明；不能把此次自然 Play 里程碑进一步说成自然场景逐 tick 速度、碰撞、完整寿命、像素或音频一致。探针 `initialState` 未填且 `continuousTickCapture=false`，不能把它与发行 trace 当作同世界证书。Renderer借用数和完整退出零残留也未由此探针采集。Q07/R18仍开放。
