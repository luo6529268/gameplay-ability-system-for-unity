# Unity state9996 direct出生接线审计

2026-09-21，只读实施前审计；源12/813已通过，Unity RED fixture正在准备，生产未改。

## 独立复核确认的最小路径

- OPointCreateTask：新增专用一次性nativeState9996CloneSpawn标记并Clear=false；不借用nativeWeaponPieceSpawn。后者另控制OID0准入，clone217/218不需要放宽OID0。
- BattleLateEntityLifecycleModule.SpawnState9996Children：native查definition在随机数前；tuple与native frame准入后再查free slot；失败continue，不能提前break阻断后续RNG。legacy兼容分支保持旧准入。producer只设置native clone标记，移除native HP/MP10后写；保留owner/group/arest/velocity/reset cooldown各自已确认职责。
- BattleLogicEntityFactory.Create与LF2ObjectPointFactory.ProcessCreateObject：两即时路径都在分配前做native初始frame准入；clone绕过普通PostInitLiving，进入共享direct出生初始化。
- BattleNativeWeaponPieceWriter.InitializeBirth已有spawn_at式500/500/maxMP/lives/display/nativeframe初始化，可将完整通用body及frame admission提取到准确新writer，原piece入口委派。不能只复用HP值或让clone冒充piece；不得更改piece Materialize/RNG/scheduling。
- 共享新writer精确路径及所有上述生产路径须在Record追加后才写，目前尚未授权生产改动。

## Task传递及生命周期

单任务队列传同一对象引用，CreateObjectImmediate进入相同ProcessCreateObject；当前clone即时caller不进入multi-object复制路径。那些路径按普通OPoint解释，不应自动继承clone标记。新标记无实体持久字段、无schema变化；必须测Clear与归还后再用于普通OPoint，防止错误跳过普通算法。若实际实施改变多对象/延后入口，先独立声明与检查，不静默扩大scope。

原owner的try/finally Recycle保留；不新增队列/manager/worker或关闭阶段，遵守现有停止接单/任务丢弃合同。任务池或实体池拒绝不应令native剩余attempt跳过规定RNG，也不允许用无界分配掩盖容量。

## 容量及证据边界

正式EngineProfile是1000槽，dynamic50～999；Unity当前Authority400=400、Mobile=1050，dynamic起点50。保留既有profile，不把300/400当source常量。source满容量950 filler+source=951 active，Unity同可用空槽上下文有不同filler数。容量测试必须分别报告context，只精确对照创建slot/实体tuple与随机序列，不声称完整世界occupancy相等或修改容量配置。

Source-final正常案例在独立materializer之后又调用完整driver，源counter没有人为推进，因此following再生成5个。它不是“第一次已执行完整tick”的自然下一tick；Unity后继必须复现同一调用实验并注明AI controls，不能设置counter2或跳过生成来凑相等。

## 源证据

source-final/first.jsonl与repeat.jsonl均163239 bytes，SHA090B9773FCDB0C755D0E458CC719F97246702A9DFC006E2F4426D53AAA1D09F6；独立813 checks/root复跑PASS。type0/3、explicit/implicit均5+34calls；缺217为1+6，缺218为4+28；partial2/full0都34；guards0，encoded8M仍unresolved。HP/MP500忽略ohp20/omp30，maxMP731、weapon37、armor23/recover17、alias34/drop2、display500、incomingScale0（defend140未在spawn_at应用）。

当前仅诊断源码及新测试路径已声明；clone出生、完整显示父任务、Q06仍未完成，Q07未迁移。
