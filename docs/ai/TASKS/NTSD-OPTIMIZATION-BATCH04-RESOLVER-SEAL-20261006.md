# 第四批 H-11 资源解析器封口 Task Contract

Task NTSD-OPTIMIZATION-BATCH04-RESOLVER-SEAL-20261006 / RUNTIME_PENDING；Change NTSD-OPT-H11-RESOLVER-SEAL-004 / RUNTIME_PENDING。
用户授权：开始执行下一批的任务，沿已批准优化文档与统一总表推进。

原状本次重扫：BattleCatalogCentralResourceResolver已有Prepare/Seal/Unseal与bounded cache skip；
PrepareCapacity本身未查capacitySealed，仍会EnsureCapacity/PrepareCapacity并提高prepared limits。
缓存满skip仅拒绝memoization，解析资源照常返回，不能误改为拒绝draw/submission。
目标：仅在参数检查后、任何缓存增长或limit写入前拒绝sealed PrepareCapacity。
生产文件仅Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderTypes.cs；
符号BattleCatalogCentralResourceResolver.PrepareCapacity。新测试BattleResolverCapacitySealEditorTests.cs。
不新增模拟字段/缓存/queue/owner；原Reset/Configure清理与EndBattleCapacitySeal解除仍承接现有生命周期。

红线：正式336B44/D023、33ms/3ms/2interval、checksum/RNG/pass、publication排序/first-visible/latency、
正常资源解析/颜色/UV/绑定/segment、CPU lease与GPU完成、fail-closed及11阶段关闭均不改变。
不读取Q06活跃方法体，不改Scene/资源/ProjectSettings/PERF/ATLAS/MONO/EXT-1或Server。
EXT-1仍PROPOSED / MODIFY_REQUIRED，无专项M0、instancing、bank/预算/格式选择。

验证：WindowsEditor Unity2022.3.62f3，原PID19040/TCP6402、Menu clean非Play，现有URP。
原状可编译test-first：sealed Prepare(零/相同/更大)拒绝、拒绝前存储/limits/配置/颜色保持、
Unseal后允许准备；central两resolver沿原owner封口/解除；逻辑缓存0/1/17满时仍解析全部新资源且不增长。
严格/Prepared Resolve、Configure no-op与材质配置切换，隔离测试框架分配的局部0B；
必要resolver/common绑定/前三批具名回归，原Editorcompile/无CSerror/场景clean与保护SHA、Ledger/diff。
不把局部0B或编译晋升完整物化/上传/录制/提交0GC，未测CPU/native/GPU/1000AI/Android，父H-11仍开放。

八文件精确原状、授权与恢复见[Operation](../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH04-RESOLVER-SEAL-20261006/RECORD.md)。
恢复另获批准仅逆向本hunk，保留前三批dirty及新鲜精确备份，不reset/checkout/clean/stash/delete。
实际结果追加到[报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH04-RESOLVER-SEAL-20261006/REPORT.md)及统一总表。

实施前范围补充：既有Animation/Rendering/Editor/BattleCatalogCentralResourceResolverEditorTests.cs
的CreateTrustedCommandWithIdentity旧27签名需按当前28/MotionAnchor适配，具名负例先确认；
只两个测试helper位置，不改预期/生产/Q06。第九个existing原件先addendum/备份/SHA再写入。

实际限定交付：原Editor PID19040/TCP6401，生产/Editor程序集19:50:48/50、idle/CS error0。
GREEN新15/15+适配旧1/1；旧resolver/三批/mesh/LatestFrame/motion/辅助/common绑定105/105，去重120项。
0/1/17缓存×strict/Prepared，32资源64轮Configure no-op+Resolve局部0B；完整central路径未测。
初两次新fixture失败与连接错误均保存，生产原状有效RED5 guard失败/10控制通过；
旧签名1失败另证并先addendum/备份后适配。无Scene/Play/GPU/M0/设备证据，父H-11继续开放。
