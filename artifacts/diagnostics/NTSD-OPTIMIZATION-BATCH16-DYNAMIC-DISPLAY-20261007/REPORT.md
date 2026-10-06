# 第16批 M-03 动态显示验证报告

最终审计：Validate-ChangeLedger exit0/1312 Records/9 governed scripts覆盖/0error、本Change0warning（全库4260历史warning）；8/8 before备份与582保护SHA零漂移。34项=高12/中14/低8；2907本地链接全扫20旧错误均事前已存在，本批新增错误0，不扩范围改历史链接。git diff --check exit0、HEAD/staged不变，原EditorPID19040/6401，formal336身份保持。

结论：SCOPED_DYNAMIC_PUBLICATION_PASS。Task NTSD-OPTIMIZATION-BATCH16-DYNAMIC-DISPLAY-20261007 限定出口通过；
Change NTSD-OPT-M03-DYNAMIC-DISPLAY-016 / RUNTIME_PENDING，父M03/H11保持OPEN。
本批只有Editor探针扩展，没有改生产代码；不是新增性能收益或Android证书。

## 改动与环境

唯一脚本：Assets/NTSD/Scripts/Test/Editor/BattleCentralProductionWindowSceneProbeEditor.cs。
原探针复用启动/自然tick/camera观察/有序关闭，只增加batch16独立菜单、240tick动态输入、
128身份与2048相机样本硬容量、快照身份/帧号/可见字段比对和覆盖记录。
旧14/15入口保持；新记录CreateNew，旧原件不覆盖。
技能输入经现有InputSystem/当前InputActions，不强制step、不设置角色位置/帧/HP/MP。
输入事件/params数组在Editor Poll而非camera观察作用域分配，JSON/结果导出在热观察之后。
没有新runtime manager/worker/缓存；退出先释放测试键，再既有11阶段正常关闭。

原Editor6401，Unity2022.3.62f3/URP14/WindowsEditor。
从原Menu clean/8roots/idle进入saved Battle；结束恢复原Menu clean/8roots/nonPlay。
技能序列正常进入240/241/253/301等实际动作，没有用受控改帧制造技能。
source X实际521.1890197694302→841.1890197694302，Z452.4479166666667→477.4479166666667；
两段变化含移动输入和技能运动，不能把总320px归因于12tick单纯移动。

## 实际运行结果

| 证据 | 结果 | 边界 |
|---|---|---|
| compile | 最初两Editor诊断修后error CS0 | 缺namespace/uint诊断字段，未改生产API |
| focused | 50/50具名Passed，2.2119742s | Deferral14/Latest13/Capacity16/pure motion3/Shutdown4 |
| 原Battle动态窗 | tick8→248/240自然tick，308camera样本 | current default roster，非1000AI |
| motion | 21相机样本实际actor source位置改变 | 非像素跟手/全场latency证明 |
| publication比对 | 1183次实体身份/帧号/可见字段均同 | captured为独立副本，未物化source publication |
| 新/离开身份 | 18新增、16离开publication，最多8实体 | 只所观测camera，不等于全部生成/逻辑销毁 |
| body command | 每条Entity command属于当前publication | 未测GPU残影/透明重叠 |
| 物化/上传 | 307Build=238publication+69alpha，397408entity vertex bytes | 308request一个sameUnityFrame旧gate，非收益A/B |
| segment/CPU draw | 4～12物理segment，recordedCPUdraw5～13 | 不能推导真实GPU batch |
| 热路径记录 | camera envelope/observer各0B，0growth/failed/rejected | Editor两硬门false；输入/其他线程/native/完整链未认证 |
| 关闭 | objects/slots/borrowers均0，Scene clean且SHA同 | 正常有Domain Reload退出，非所有关闭组合 |

focused job：8b5e5c239f6e414faea04a1129ef9f42，[完整具名结果](focused-result.json)。
[运行原件](run-01/production-window-01.json) / [计数窗口](run-01/materialization-window-01.json) /
[汇总](dynamic-summary.json) / [恢复Editor](restored-editor-state.json)。

相邻camera逻辑tick差：0共70、1共235、2共2；Unity frame差0共1、1共306。
每个样本logic/publication/display tick一致；没有以238publication Build反推降频或tick丢失。
已有sameUnityFrame gate使307而非308Build，既有语义不改，不称所有相机必然各新Build一次。

11个有body的身份，首个可见publication与首body command都在同一**观测tick**；
9个oid518记录firstBodyCommandTick=-1，当前证据不能把EntityVisible旗标等同body资格，
不判缺图/first-visible故障、不猜补其具体非视觉规则。
slot50/51/55实际generation复用且stableId分离；每个相机样本body拒绝已离开publication的旧身份。
camera无法证明漏过中间tick的短生命周期实体首可见；排序/Q06内部/正式EXE first-visible、
绝对latency、GPU painter order与像素残留仍未验收。

## 不变量、审计与下一个出口

生产Central、DynamicMeshBackend、Stage SHA与事前相同；生产DLL保持A49C69CE...。
只核Q06文件身份/既有调用引用，不读活跃方法体。
33ms/3ms/max2/逻辑/checksum/input/RNG/pass/插值/segment/failclosed均未修改；
本批未跑正式EXE/全World checksum A/B，不把生产未改当实际正式回放验收。
EXT1 PROPOSED/MODIFY_REQUIRED、无专项M0/instancing；MONO USER_HOLD；
ATLAS bank/预算/格式与PERF/ATLAS正文/Scene/资源/Settings不动。
unity-cli技能使验证复用原Editor/MCP兼容路线；pixel-perfect技能限定既有URP/UV/44stride，
未采用其通用60Hz物理、相机/importer配置建议。

事前8当前工作树备份/582保护，[Operation](../../../docs/ai/FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH16-DYNAMIC-DISPLAY-20261007/RECORD.md)；
未删除/移动/丢弃Git/创建第二Editor。最终Ledger/after/范围外SHA另附收尾记录。
下一M03必要出口：同显示样本的透明像素/alpha取样/延迟对照，再真实高负载/完整链/设备。
不因本批限定PASS解冻EXT1/MONO/ATLAS，父34项不冒充关闭。

[Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH16-DYNAMIC-DISPLAY-20261007.md) / [Change](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-DYNAMIC-DISPLAY-016.md)。

