# 优化第一批：表现缓存预热与统一进度

Task ID：NTSD-OPTIMIZATION-BATCH01-CAPACITY-20261006
状态：RUNTIME_PENDING（子批代码/编译/聚焦通过；父H-11开放）
Change：[NTSD-OPT-H11-CACHE-PREWARM-001](../CHANGE-RECORDS/NTSD-OPT-H11-CACHE-PREWARM-001.md)
进度：[34项总表](../../../Assets/NTSD/Docs/battle-optimization-progress-tracker.md)
操作：[精确before清单](../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH01-CAPACITY-20261006/RECORD.md)

用户已批准按优化文档开始；本包是H-11的最小预热子批，不是34项同时开工。
先建立RED测试证实中央预热未覆盖DisplayMotion及chunk描述缓存，再接入现有预热。
槽位单位沿用runtime entityCapacity，描述容量按成功quad计数的保守上界：
每chunk=min(QuadsPerChunk, commandCapacity-chunkIndex*QuadsPerChunk)。
未解析命令不会增加成功quad总数；每quad最多一个物理segment，尾chunk因此可用剩余额度。
不更改publication/采样/排序/UV/segment/33ms或模拟真值。无新Runtime owner，
缓存依旧central static owner/chunk Dispose；既有11阶段关闭顺序不变。

首批验证：新精确EditMode测试+旧submesh/bounds回归、源码/编译、ChangeLedger、文档和保护哈希。
只读确认原Editor空闲后才测试，不另开Editor，不切换/保存Scene。
若原Editor不可达，静态/隔离编译单列，不能报告Unity运行验收通过。
完整0GC、硬上限/seal/overflow、原Battle退出重进及Player/Android仍父H-11后续门。
EXT-1/MONO/资源写入专项仍原门；不改ATLAS bank/预算/格式，不开EXT专项M0。

回滚：用户另批准后用固定commit/before SHA与当前自己的hunk生成逆向补丁，
不动其他任务内容，不reset/checkout/clean。

2026-10-06 子批交付：两runtime精确预热hunk已写，test-first RED 6失败/7执行；
修复后新7/7与旧8/8实际原Editor EditMode PASS。未切换Scene/进入Play。
局部display lookup0B不扩大成全路径0GC；剩余门见父H-11与总表，资源/专项门保持。
