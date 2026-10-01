# C051 无甲方向修复的限定验证

状态：`SCOPED_DRIVER_PASS / SCENE_PLAY_PENDING`。Q07、C051整项和总目标未关闭。

当前根正式EXE已再次核对SHA-256为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。沿用原Task中seed682973786、mode0、OID20/action288/X500，经正式OPoint OID888/action35→40在tick9命中OID2；右X550双方朝右，左X350双方朝左。原项目Editor PID105896、Unity2022.3.62f3，未启动第二实例，未进入Battle Scene Play。

生产修正仅共用 `ResolveStandardDamageKnockbackX` 删除无甲effect22/23的相对X例外，按攻击者朝向乘完整dvx；减伤路径保持原逻辑。原Editor方向测试job `ef7051f449b34105ac5c1235a0e3256e` 六参数6/6 PASS，包括effect22/23左右与effect0对照。第一次使用不完整筛选名称的job `98c0e59374f1492eb08e2cf1d61c7750` 运行0项，原件保留，不计为通过。

原Editor左右正式内容完整Driver新raw各12tick PASS，分别核对父体、目标及子体的动作/HP/Vx各99项，身份OID各33项；当前根右v1、左v2同初态的对应字段全部一致。tick9目标均action180/HP435，tick12右Vx-10、左Vx+10，旧方向首差消除。[比较结果](c051-direction-v3-comparison.json)。初次v2比较误选历史左根v1（只有slot0翻向）的不匹配初态，四动作差不能归为新生产首差；v2原件保留，v3按两角色朝向核对后更正。原根报告已有该历史初态更正，未修改权威EXE或trace。

相邻原Editorjob `9dba91085c074d668bbcf049bc521d95` 执行17项时三项HitPlan因旧反射参数数量错误失败。独立 `NTSD28-336B44-Q07-C051-REDUCED-TEST-CALL-001` 仅补当前接口可选默认参数；job `0d854bf7635541899e8118c2956706f9` 新17/17 PASS，原断言及生产均保持。MCP客户端在若干会话关闭时报BrokenResourceError，工具结果已收到并落盘；客户端退出码1不能写成命令整体成功，也不推翻已保存的Unity测试结果。

生成工程 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly` exit0、0 error/266 warning。原Editor新测试程序集时间16:34:15（+08:00），晚于测试helper改动，重跑使用新编译版本。四项Scene/Asset保护SHA未变，LoganRuntime Git状态无差异。恢复后部分输入字节身份受Git换行处理影响，与旧raw的fusion/kind/mode semantic SHA仍相同；不把字节fingerprint变化抹去或声称全部DAT逐字节同正式根。

临时请求的两次消费后清理已预先登记 [Operation Record](../../../docs/ai/FILE-OPERATIONS/NTSD28-C051-RAW-REQUEST-LIFECYCLE-20261001-001/RECORD.md)，payload、哈希和新结果保留，请求最后不存在；无已有资源删除。

边界：raw metadata仍为certificateEligible=false，仅声明上述字段、tick和初态。effect22尚仅聚焦测试，护甲正式同条件、原Battle Scene Play/自然物理输入、全World与整场门均待验证；Q07不以这些局部PASS计为100%。
