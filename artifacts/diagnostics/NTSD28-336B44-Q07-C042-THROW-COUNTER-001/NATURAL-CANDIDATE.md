# 正式内容自然投掷候选（只读）

2026-10-01 只读检查正式 `resources/runtime/decoded_dat/c/bee/bee.dat`：OID75 的 action355 有 kind-3 抓取 ITR，成功时 `catchingact:358`、`caughtact:130`；action358～374 均有 kind-1 CPOINT，逐帧 `next` 连到375；action375 的 CPOINT 有 `throwvx:25 / throwvy:-10 / vaction:181`，其后 next376。另一路 action378 kind-3 可进入381，最终 action78 也有投掷 CPOINT。

这使“从正式可载入初始 action355 开始，靠正式碰撞建立关系，再自然推进到375投掷”成为最窄的运行候选。尚未证明给定双方位置能命中、关系能维持到375、被投者投掷前计数非零，亦未取得正式根或 Unity 原 Battle Scene 同态。下一步先做当前源码完整 Driver 正反位置控制，并记录每 tick 双方 action/frameCounter、关系、HP、速度和 action375 前后；仅阳性时才录同一初态 LFR 到 336B44 根，再做原 Scene 定向 Play。不能将 DAT `next` 链静态可达当作运行时 PASS。
