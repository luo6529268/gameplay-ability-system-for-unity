# H06 Role 诊断前置问题：已定位

Task NTSD-H06-ROLE-PREREQUISITE-DIAG-20261010 / SCOPED_DIAGNOSIS_COMPLETED。
Change NTSD-H06-ROLE-PREREQUISITE-DIAG-001 / VERIFIED，仅Editor诊断实现与本报告证据；原完整Driver0GC/旧状态门仍失败，七段成本未运行，不是优化或父项通过。

结论：两个独立前置问题已被当前证据区分。113次状态失败来自诊断把惰性对象计数误当真实回退；两次稳态分配在当前原Editor的Mono.JIT里，各36B/30B。不能据此重写碰撞、AI或伤害生产逻辑。后继只修Editor诊断资格与必要冷JIT准备，再交回结论。

## 本轮实景与一致性
一次combat1000，Sakura1000，Role/DataOrientedCanonical，seed1314149188，120warm+180steady。原配置、原聚合判断与失败停止均保留；record七段/旧六分支/输入细分/producer/95/84/89/92/94 OFF。原Editor39108/TCP6402复用，无第二实例。
300个完成tick与原失败OFF的完整RuntimeChecksum、输入数、双RNG状态及计数/同步表字段逐项完全相同；14字段共300行0差异，原聚合失败序列也相同。不是仅末态相同。见same-fixture-parity.json。

## 组合状态门
300tick有效观察，113false：仅bit12（fallback participant必须0），其他14项全部通过；mask与原聚合0分歧。失败tick104–134、180–191、206–275，warm17/steady96。
每个tick fallback participant == inert participant，失败时1/2/3对象分别62/32/19tick；真实fallback attack/body ordinal数所有300tick均0，实际collector始终Role，collection aborted所有tick0。
现有CountRoleAwareFormalFallbackParticipants明示包含既无HasAttackItr又无HasBody的对象；这种对象不能生成candidate，但仍记入诊断。原零总数断言把它们误判为回退。修复需用准确的真实回退列表+惰性对账，仍拒绝真实fallback、abort、配置错与观测缺口；不是删除或放宽真实回退安全门。

## 完整Driver分配
校准前后PASS，300 begun/ended/accepted，180steady，invalid/rejected/sequenceErrors0。稳态事件在tick130与162，各1；预热另140事件不与稳态混写。Recorder raw346300单位TimeNanoseconds，不是bytes或GC停顿。
已保存raw重新导入后在Main Thread唯一tick marker及完整Driver父marker内部各取到1个GC.Alloc：
- tick130，36B，父Mono.JIT，首已解析managed frame BattleDamageWriter.RecordAlternateLeadSound（BattleDamageWriter.cs:2643），所在CharacterHitConsumePostInteraction。
- tick162，30B，父Mono.JIT，首已解析managed frame BattleCharacterActionWriter.RouteNativeRunning（BattleCharacterActionWriter.cs:1210），所在CharacterInput/EntityInputPass。
元数据合计66B是本次两个精确事件的大小，不是Recorder时间换算。Mono.JIT嵌套是直接观察；没有证明它们是每tick重复创建或具体对象类型，不猜foreach/装箱/字符串处理。没有证据把95或旧ABBA历史事件追溯为同一原因。
正确后继是Editor冷阶段准备这两个既有方法的JIT（只准备不执行业务），再用不启用Profiler的原完整Driver范围验证。保留全部事件，不滤Mono.JIT、不移动计时/GC边界、不增加warmtick掩盖、不称Android修复。若当前Mono不能无副作用准备或仍有新分配，准确停下交回，不自动重写生产方法或追加实验。

## 原始捕获的边界
原计划复用八帧实时回调，但该回调收到0帧；binary仍保存到窗口结束，故capture/cpu-gc-state.json为PARTIAL、原samples空。原件保留，不能冒称实时八帧成功。没有追加实景；idle导入同一raw并仅导出含tick130/162的2帧（Main Thread2帧有效）。现有全线程扫描达到128线程上限，不能声称全线程完整；本Task要求的同步主线程两事件有完整嵌套/调用栈且数量与Recorder一致。见diagnosis-summary.json、allocation-callsites.json及capture/cpu-gc-recovery.json。
原窗口开始/结束Profiler设置相等；解析前后设置相等；导入使用append，不清旧历史。导入原件保留，未删除历史或raw。

## 修改/验证/保护
只改Suite、GC observer、新Editor helper三脚本及新meta。原聚合与失败停止未修；只读completed scope、固定300行、冷预建tick markers、复用原capture和独占GC observer，解析复用原导出/设置恢复。
原Editor最终编译CS errors0；focused4/4 PASS（两新fixture/真实scope保留检查+两既有owner检查，job4ff1921248c24d499497bca12fad14f0）；后加idle解析路径另编译并实际解析成功，未机械重跑未受影响测试。
原Suite按旧硬门停止，PARTIAL/有限诊断门失败原件保留。300tick容量门180steady PASS、reject/drop/growth0；orderedShutdown及teardown恢复PASS、World object/entity0，原NTSD_Menu8roots clean，Battle/Menu磁盘SHA不变。三个取证源文件开窗/收窗前指纹相同（helper用解析添加前真实备份核对）；2512其它保护文件0漂移。
首次scripts-only编译未导入新增文件CS0103，full refresh后修复；中途只读动态进度查询因旧CodeDOM命令行过长/Roslyn未安装失败，未安装组件/改桥接，改用现成只读组件资源核对。准备时额外Temp副本4个锁定/cache文件未能复制； intended paths及共享终端备份完整，无Temp清理或Editor启动。
Validate-ChangeLedger与diff检查结论见本报告末尾后续附记。

## 后继范围
只把明确的Editor诊断门修正和两方法冷JIT准备交执行会话；最窄正确性/不执行业务证明，一次同fixture120+180 OFF/noProfiler完整Driver校准0GC+逐tick一致性/容量/关闭。该后继不运行七段ON，不自动解冻其它候选，不改Query/World/Host/AI/伤害生产逻辑，失败保留并停止。H06父/H07精确33ms/drop0/backlog/Android仍开放，producerUSER_DEFERRED/OFF、95OFF、旧Goal取消。

最终留痕：Tools/Validate-ChangeLedger.ps1 PASSED（1399 records，既有WARNING保留）；git diff --check exit0，仅既有LF/CRLF提示。最终原Editor idle/nonPlay/noncompile/nontest；NTSD_Menu8roots clean。后继具名指令见execution-handoff.txt，派发结果另存。
