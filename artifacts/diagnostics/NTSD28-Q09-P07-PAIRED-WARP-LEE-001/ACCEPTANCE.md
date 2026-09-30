# Q09/P-07 李 OID204 配对 D3D11 WARP 同帧像素归因

状态：`VERIFIED_SCOPED_PAIRED_WARP_PIXEL_ATTRIBUTION`。本包只验证正式配对 playable 源码的 GPU 消费，不关闭 P-07、Q09、BATCH-05 或总目标。

正式根 `NTSD2.8-Logan.exe` 的 SHA-256 为 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。诊断源 `Tools/NTSD28Q09Diagnostics/lee_shadow_warp_probe.cpp`（SHA-256 `8745B98E92498C0D10EC1835B2888441FA225FC64730AC0DC4368E657EA9A671`）以既有 `NTSD28-Q09-P07-FORMAL-LEE-SNAPSHOT-001` 的 seed、配置和李 J@2/L@3–4 输入推进正式配对 `GameSession28` 六 tick。编译沿用 playable `offscreen_gate` 的 core/playable/D3D11 源与链接闭包，只替换诊断 main；完整参数见 `compile-argv.txt`，编译退出码 0，诊断 EXE SHA-256 `C784C953B4F9C32F875D8B77E81998AB3098A14693D1B2DCEFFB2A5036EDAD3A`。

同一 tick6 快照中有 owner0 的五个 OID204 子体、五个本体 sprite 命令、零个子体 shadow 命令，另有三个普通 shadow 对照。WARP 1333×730 离屏先渲染原快照，再仅从快照副本移除五个子体 sprite 命令后渲染；两次之间没有推进战斗 tick 或更改 World/DAT。运行退出码 0，见 `run.log`、`run-exit.txt`。

独立 PNG 逐像素比较：两图都是 1333×730，差异 590 个 RGBA 像素，左上原点包围框 `[402,630,444,651]`。`tick6-full.png` SHA-256 为 `CCF0D55DF9D7DE0D3C7303E82839323872DD5751FC50A19B50264753932CA353`；`tick6-without-children.png` 为 `F9B9809B752E434F5CB596406A4AC5322E56237280CFDBA0C58AF8A972516AE8`。机器计数见 `pixel-analysis.json`，两张 PNG 已目视核对。重复写同一输出路径被拒绝（退出码 3），两 PNG 哈希不变，见 `no-overwrite-result.txt`。

受保护 Scene 磁盘 SHA-256：Battle `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`，Menu `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`，均与包前一致。未改 Unity 生产脚本、DAT、图片、Scene、背景或模式配置，因此本包不重跑 Unity 编译/Play。

账本校验 `Tools/Validate-ChangeLedger.ps1` 退出码 0（完整输出 `ledger-validator.log`；既有历史 Record 的非当前 diff 警告仍在）；`git -c core.safecrlf=false diff --check` 退出码 0。

**范围限制：** WARP 图来自配对 playable 源码，不是根正式 EXE 自身 GPU 截图；既有原 Battle Scene 自然 Play 与这里并非相同场景、视口或同 tick 世界状态。因此这 590 像素只能证明配对渲染器对五个本体命令的可见消费，不能声称正式 EXE/Unity 像素完全对齐。P-07 还需根正式 EXE 与 Unity 的可比较场景/画面出口；只有出现新的可达首差才重开 mode/state/object 聚焦门。
