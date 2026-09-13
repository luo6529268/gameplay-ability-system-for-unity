# Q05 Mass/Oscillate shell载体退休

状态 FOCUSED_TEST_PASS / SCOPED_PLAY_PASS / JOINT_SCHEMA_PENDING。父NTSD28-Q05-RETIRED-CARRIER-AND-2F8-MIGRATION-001步骤2继续，独立+2F8与raw恢复已有46/SelfCheck证据，不重做。

使用上一包artifact Q05MassRefs/Q05ShellFullRefs及当前rg核对准确路径，事前Record覆盖全部生产与测试。Q04已退休Mass摩擦gate和Oscillate reader，当前只清carrier/context/构造调用、初始化/恢复/copy/shell、相关旧fixture与Play probe。不得恢复旧行为去让旧测试通过。

Mass重点：CharacterMechanicsContext.mass及各构造者、LF2Character._mass/MassForFrameAdvance/初始化查询、CharacterShell.Mass与restore、ECS frameadvance参数。NTSDSpec.GetMass是当前唯一生产查询；只在完整引用证明后处理其Mass/Oscillate静态数据/死API，保留其他有边界的API及NTSDGlobal等无关常量，不把历史表重新当权威。

Oscillate重点：ObjectEffects.Oscillate/OscillateDirection、Reset、EntityBaseShell两字段/capture与LF2Entity.restore，Q04/旧producer测试和probe中人工sentinel需要改为字段退休后的等价核心行为断言。其他真实Effect震动/恢复/sound/blink/stuck等状态保留，不移除真实TrackerParent或扩展渲染重构。

CharacterShell1→2和EntityBaseShell1→2必须同父Q05步骤4与entity13/aggregate21/checksum24协调；当前步骤2可编译中间态但不发布baseline/跨版本交换。后继五reserved字段清理、identity/双OPoint guard及旧版本拒绝/回放/真实Play不能遗漏。

验证：先字段不存在/构造与snapshot形状RED，保留Q04 native运动见证/真实Effect行为与相关snapshot/restore/no-allocation/完整SelfCheck。记录所有使用过的旧字段测试为什么改变，原历史证据不改。任何新增文件前准确Record，无删除现有文件授权，不用空壳或批量删除绕过审计。

Unity/GAS、非战斗、Scene/InputActions/Gen/Plugins/外部Server、33ms/3ms、十一阶段、stage.dat USER_HOLD及批准例外保持；禁止computer-use，只桥接/日志/结果/进程。Foot任务外18删除/新blue-red-yellow目录与Scene旧精度差异保留。回滚须批准，仅准确差量。

最新出口：NTSD28-Q05-MASS-OSCILLATE-SHELL-CARRIER-RETIREMENT-001已FOCUSED_TEST_PASS/SCOPED_PLAY_PASS（890/SelfCheck/两probe），Mass/Oscillate存储删除，规则核心主体保持。下一唯一NTSD28-Q05-FIVE-RESERVED-CARRIER-RETIREMENT-001；五reserved、identity/guard/版本/回放仍待，当前不发布中间payload。禁止computer-use。
