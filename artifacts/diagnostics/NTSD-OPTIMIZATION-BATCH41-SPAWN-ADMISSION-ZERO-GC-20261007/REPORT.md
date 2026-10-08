# 第41批：生成准入捕获分配源限定修复

结论：SCOPED_ADMISSION_ALLOCATION_SOURCE_FIX_PASS / FOCUSED_TEST_PASS。生产只改BattleNativeDirectSpawnWriter.IsInitialActionAdmitted，去捕获lambda；20新case＋6受影响旧生成case均通过。没有新千人/FPS/Profiler或完整0GC测量，H07/H11仍未完成，Goal实际active且本批不操作停止/完成状态。

## 因果证据与改变

39批已恢复调用树记录235次/4700B（20B/次）。本批原编译IL为newobj / ldftn / newobj，证明原方法存在捕获闭包/委托构造；显式guard后仅999索引扫frames，编译IL直接managed构造/委托指令0。0..998/999/1000边界仍取NativeMax1000，不误用857；null/异常/短路、首次命中与动态增删保持。InitializeBirth、调用者、所有World/RNG/cadence/presentation/collector/关闭owner和资源未改，无新增buffer/cache/capacity。

IL门只证明本方法直接构造来源消除，不替代实际warmup后整个logic allocation0B/tick，也不定位H11相机两次事件或证明FPS提升。正常List读取路径无新增构造；未再次测235调用对应字节。

## 实际执行

| 阶段 | 原件 | 实际结果 | 解释 |
|---|---|---|---|
| RED | [red-01.xml](red-01.xml) / [Editor](red-01-editor.json) | 20case，19PASS/1FAIL | 唯一失败为新IL禁止构造门，旧表达式正控制与行为控制全PASS，有效test-first |
| 首次复验 | [green-01.xml](green-01.xml) / [Editor](green-01-editor.json) | 20case，19PASS/1FAIL | STALE_COMPILED_ARTIFACT；17:41:16已结束，runtime DLL17:41:21才重编译，实际仍旧IL，不计新实现结果；原件保留 |
| GREEN | [green-02.xml](green-02.xml) / [Editor](green-02-editor.json) | 20/20 PASS | loaded IL冷准入后实际新编译方法；direct construction0，legacy正控制通过，-1..1000五wrapper共5010布尔对照及异常/动态门PASS |
| 受影响生成 | [affected-01.xml](affected-01.xml) / [Editor](affected-01-editor.json) | 6/6 PASS | C25 clone34调用/容量/动态槽两参数、ordinary recycle、weapon flag；只旧synthetic/既有只读witness，不晋升正式EXE全World |

同一个原Editor19040 / Unity2022.3.62f3；四次EditMode实际66case（含RED/旧产物无效复验），候选GREEN及受影响有效验收26/26，不混合计数。未运行Play、SelfCheck全套、全角色/native历史矩阵或设备测量。当前编译Console筛选error CS为0，历史MCP权限错误不清除/隐藏，不写Console全部0error。

## 工具异常、恢复和命令

原MCP TCP6401重载后启动Socket AccessDenied；只读查端口确认既有droid PID57212绑定，不能kill该用户进程。按现有PortManager.GetStoredPortConfig读JSON的真实契约，把两属于本项目的连接配置6401→空闲6402，准确backup/操作清单见tool-config-before.json；原Editor自动ready。未改OS权限、插件、Package/ProjectSettings或另开Editor。不把工具恢复当游戏性能收益。

仅新Editor测试文件增加仿既有请求式ICallbacks；fixed red/green/affected filter，无任意代码/路径输入。实际IsRunActive、编译/更新/nonPlay、原单Menu saved clean门，SessionState防重复，输出CreateNew、request不删除。green-01旧domain抢先消费后补loaded IL前置，green-02有效，不增加矩阵。该入口热Play即return，非生产模块。

实际验证调用：
- 原桥get_editor_state / read_console(action=get,types=error,filterText=error CS)，不clear。
- refresh_unity(scope=all,compile=request,wait_for_ready=false)；同原实例。
- 四具名request/Unity TestRunner EditMode（记录上述XML），不MCP重复dispatch。
- pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity：exit0，1337 Records/6当前code diff覆盖、4297既有non-diff warnings，无error；最终复核见validation-final.json。
- git diff --check：exit0（CRLF提醒不当错误）；git status与逐文件SHA/backup核对。

## 保护与真实未完成项

六现状backup保持、17非写域（包括已有菜单Scene/字体dirty、Brute/Suite/旧tests、Q06 ShadowBuild、PERF/ATLAS/Mono/EXT1/合同/版本与包清单）SHA已核。正式根EXE仍336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3；HEAD45bbed41保持。Q06只hash不读活跃body。实际最终指纹/Editor见after.json与editor-after.json；无reset/checkout/clean/stash/删除/移动/覆盖用户字节/commit/push。

H07最后性能数字仍是40批：短窗logic P95 107.065/113.849ms、显示估算3.7—3.9FPS，不因本批修复改报。H11完整camera2事件仍UNKNOWN/严格FAIL，范围不缩小。限定产物5/6、首阶段条件通过4/6、34父关闭0保持；已执行22—37/39/40/41共19子批，38仍PLANNED，累计20只复盘不停止。H06/M13/M14已有评估不重开，M03限定交付复用；EXT1/ATLAS/Mono和Role-aware默认切换权限不变。

下一继续范围内有据任务：H11两个实际分配点必要归因/修复与H07已测collector残余成本。这个直接构造点局部门已经闭合，不再追加可选确认测试；未获生产推广权限的方向仅拦对应动作，不让其它技术待办停止Goal。
