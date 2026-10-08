# 第45批：普通 Brute 低频分支计时资格

最终限定SCOPED_SAMPLED_DIAGNOSTIC_VERIFIED：221/221与四真实1000AI短窗已完成；约1/64coverage/末hash/配置恢复/关闭保护通过，OFF/ON差2.656/3.540ms仍有扰动，不宣称纯cost/FPS收益。H07 P95/drop/可靠0GC仍未达，正式长窗未过；本包诊断出口闭合，转38固定夹具候选资格，不再同构计时细化。下方PLANNED是事前，不表示正在等待本包首次执行。
状态：PLANNED。用户来源：有限首阶段合同当前0—8节；第44批collector OFF/ON实测差35.295/11.034ms证明需要限定仪器干扰。本批仅诊断方式，不新增碰撞算法，不切Role-aware/default，不重复44全pair窗口。

准确C#写域：
- Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs：既有四phase计时增加stride=1默认兼容44；stride只接受1—4096的2次幂。诊断开启/recorder有效/geometryFirst才按逐collection旋转offset、方向ordinal有界采样；8long覆盖字段（eligible/timed direction及reject/pairAllowed/exact各visit/timed），Last/Total两个值struct，附两int配置/offset，约136B字段payload下界，不是全内存。只采样clock，不采样或跳过规则；四原分支仍各执行一次、不动RNG/rest/顺序。默认OFF不计数，Last每collection清零，Total仅diagnostic累计。
- Assets/NTSD/Scripts/Animation/Rendering/Editor/ProductionEntityStressEditorTests.cs：反射API先RED；stride范围、64offset完整覆盖、OFF/ON候选RNG、拒绝分支、无recorder/geometry关闭不计数、准备后严格128collection无GC。
- Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs：新45菜单/root四OFF/stride64 ON，request65字段同40仅output；用既有bruteBranchTimingOnly模式，不新增owner。两设置在tick前保存/配置，normal和abort恢复bool+stride后清引用；RunState完整覆盖struct含warm+sample明确记账，不外推cost。

普通原world/query持有标量/值结构；无新worker/buffer/lease/异步queue，owner生命周期/十一阶段顺序不变。Q06只hash；不改Scene/Prefab/资源/Settings/Input/Gen/Plugins/Server/renderer/ATLAS预算格式/segment。33/3ms/max2/正式336/checksum/publication等红线不变。

验证：19具名新增case先有效RED→GREEN，相关44 branch/原candidate/detail/suite/formal必要门；不全项目重跑。只在实际编译/新旧测试通过、原Editor idle和指纹冻结后才新四120+180真实1000AI窗口；本次若未启动如实留WINDOWS_READY，不冒充测量。0GC是此clock路径局部聚焦，不覆盖H11 camera FAIL；计时数据须提供各branch sampled/visited数，未经代表性/扰动资格不乘stride外推纯成本。默认stride1不改旧菜单44结果。

验收：固定64个offset周期的每方向/分支恰被采样一次（固定fixture），Last重置/Total可核、0GC；配置恢复false/true和原stride，request同40，窗末snapshot/hash/flag/四默认/teardown/Scene保持；OFF/ON tax尚待真实判断，不预设收益/干扰阈值、不过H07正式门不收口。

回滚：Operation先记逐路径现状SHA/Git并Copy-Item至全新backup，C#仅apply_patch。不得用HEAD覆盖dirty；恢复须新授权与Operation。原历史/失败产物保留，不删除共享request；existing shared terminal属于44才可既有runner更新，新目录CreateNew。

出口：SCOPED_LOW_FREQUENCY_DIAGNOSTIC资格及instrumentation影响结论，不把19case/诊断报表称性能成功。H07/H11未完成；44已执行22、38仅PLANNED累计23，45仅准备不计执行；有代码/实际门后才计第23已执行。

