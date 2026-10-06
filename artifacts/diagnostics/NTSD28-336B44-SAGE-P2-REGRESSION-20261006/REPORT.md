# 本批最新结果（2026-10-06）

- **P2 连续击飞不同步：已修复并通过原场景复验。** 实际角色X边界分别钳制原版坐标与项目坐标；统一入口后，三次真实上勾拳含撞墙和自然落地，最大误差从1.85446361590402px降至1.5916157281026244e-12px。555完整Driver tick，最终P2 HP200，run04整体PASS。
- **受击框调试绘制：已修复并验收。** 读实际Prev2碰撞帧/统一投影，不再依赖旧Sprite原点；bdy/itr/pickup共用生产几何，2/2与同场景连续运动检查通过。此处只改绘图，实际碰撞修复归上项。
- **仙人变身：P1成功、P2在mode0失败与正式版一致，未改生产规则。** P1真实InputAction与正式自然191链到OID99一致、中央可见；P2克隆准备/召回后不变身，原Editor单例1/1、完整191tick六字段1146/1146同正式336B44。用户失败时P1/P2待确认；若P1，本輪测过条件未复现，不能声明报告已修复。
- **保护与边界：** 已退出owned Play，Battle Scene clean/Scene和配置SHA保持，十一阶段关闭完成，World/slot/borrower0；681保护路径和EXE保持。Unity编译errorCS0。没有DAT、图片、Scene、InputAction或非战斗修改，没有删除/移动/Git回退。旧总目标保持收尾。

证据：scene-e2c9120d493a42568ec2db3a526f6d98.json（修后PASS）；scene-4eeb5e1d154542429c9a7eb5edce3bef.json（修前首差）；boundary-green-job-final.json（8/8）；gizmo-green-result01.json（2/2）；sage-p2-job-final01.json（1/1）；natural-p2-191-focal-comparison.json（1146/1146）；protected-final.json。

仅验收变身、连续击飞、角色边界和调试碰撞几何共用链路；没有在已核查链路中确认其它新战斗差异，不等于全角色/全World/GPU画面完全一致。实际按键Play采用Manual完整Driver tick，不新宣称Host cadence/worker路径验收。正式LFR旧最终header失败原件保留、整体不称PASS，只所列逐tick观察用于对照。

以下为按时间保留的过程记录；待回收及旧普通运动结论以最新结果为准。

# 鸣人仙人模式与 P2 连续击飞回归核查

Task: NTSD28-336B44-SAGE-P2-REGRESSION-20261006
状态：IN_PROGRESS；原 Battle Scene run03 验证中。

## 已确认结果

- 用户确认：直接打开 Battle Scene Play；分身已生成并准备完成，召回后未变身。原场景 P1/P2 都配置 OID2 鸣人，强制走路/奔跑关闭。
- 正式根 EXE SHA-256 仍为336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3；候选 source 不用于定义未观察到的正式规则。
- Unity 完整 Driver：成熟分身受控两例通过；自然准备链在原比例与2048×1152项目投影下，两例均第191tick到 OID99。原 Editor natural job b569108006e2486eabe0e06285d3130e 为2/2 PASS。
- 实际正式 EXE 同按键自然191tick：9生成分身、167完成准备、174为鸣人401/分身334、191为仙人99/action0。Unity 所选六字段 oid/action/state/counter/owner/hp 各1146/1146同；四个正式LFR角色与两个Unity夹具角色是声明的初态差异，不称完整World一致。
- 正式回放沿用的尾断言仍期待旧OID2，进程exit46/report passed=false，错误明确为最终header expected2/got99。完整191tick原件保留，只将观察到的变身链作为证据，不称整个回放PASS。
- 受击框旧适配已确认：NTSDHitboxGizmos读Frame.D和旧Sprite原点缓存，而实际碰撞使用Prev2/Runtime/统一投影。通用修复已让bdy/itr/pickup绘制复用生产几何与深度；central、实际伤害和运动规则未改。原Editor两项几何检查2/2 PASS，独立只读审阅未发现逻辑状态写入或生产体积提取的语义改变。

## 原场景出口与诊断修订

run01在tick0按键采集None/rawAttack不符，属于未聚焦Editor的探针输入问题；探针还遗漏关闭阶段11，因此仅由root停止其拥有的Play并读取生产fallback退出结果。Scene clean、Scene/config哈希保持，对象/槽/借用者0。不能将此失败解释为仙人游戏故障。

run02使用临时InputSettings副本和真实InputAction回调，tick2已到防御110；tick3探针查找字符串up失败，因为P1实际名称为Up。新探针采用OrdinalIgnoreCase，不改InputAction资产。run02已自动完成十一阶段关闭、对象/槽/借用者0、Scene clean且哈希保持。run03是同一出口修订后的重跑，没有扩大角色矩阵。

公开InputSystem.Update不支持typed overload的CS1501，以及初次helper namespace CS0103、base Controller CS1061、无新asset导入的zero-case job、错误NUnit category和错误诊断按键时点，均原件保留于本目录与两个Change Record。后续实际原Editor编译errorCS0；新脚本完整生成工程编译301warnings/0errors。

原191字段对照v1误将正式current_mp与独立Unity Runtime.MP比较；该旧结果保留，v2依据既有current_mp→Unity PP合同取消错误字段比较。这里没有捕获PP，所以不新宣称资源扣费逐tick一致，也不因这个诊断错误扩大重测。

## 其它共用入口检查

- 本轮共享击飞路径源码尚未发现累计source/view漂移写者；是否是实际命中还是纯绘图问题，仍以run03三次自然落地恢复数据为准。
- 其余旧Sprite原点使用已定位：LF2SpecialAttack的两个私有旧点函数没有调用者，不据此新开任务；持有武器PS.sz写入是已声明共享挂点路径，不作为已证差异。
- 直接Battle world mode0和菜单配置mode1的路径差异已记录。用户明确本例直接Battle，所以它不解释本例；正式mode1仙人行为未观察，不擅改respond4规则或菜单配置。
- 681个既有保护路径在run02后哈希一致。本任务不改DAT、角色图片、Scene、InputAction或非战斗代码，既有音效修改保留。

## 未完成边界

原场景run03的实际按键、中央可见仙人身份、P2三次击飞恢复和关闭证据待回收；中央快照/绘图几何不自动等同于GPU像素或设备听感。未找到可审计的新Sage生产规则缺陷，因此尚未声称已修复变身失败。旧总目标USER_ACCEPTED_SCOPED_CLOSURE保持，本报告为用户新问题的独立归口。

Change Records:

- docs/ai/CHANGE-RECORDS/NTSD28-336B44-SAGE-KNOCKBACK-PROBE-001.md
- docs/ai/CHANGE-RECORDS/NTSD28-336B44-HITBOX-GIZMO-PROJECTION-001.md

Operation: docs/ai/FILE-OPERATIONS/NTSD28-336B44-SAGE-P2-REGRESSION-EDIT-20261006/RECORD.md


## run03 实测与边界修复更正

上文“未发现source/view漂移写者”“run03待回收”是修复前快照；run03原件scene-4eeb5e1d154542429c9a7eb5edce3bef.json实际复现第三次P2受击后t448撞右墙，源规则坐标与项目坐标相差1.854463615904px。此前t446/447误差仅约1.6e-12；不是普通击飞每tick累积差，而是共用PreFrame角色X边界出口独立钳制两域。P1自然按键Sage191tick与中央仙人可见身份均通过。run03整体FAIL正确保留，十一阶段关闭、对象/slot/borrower0、Scene clean与文件哈希保持。

新Change NTSD28-336B44-PROJECTED-CHARACTER-BOUNDARY-SYNC-001已先登记四清洁脚本before；只新增同一Runtime角色X边界出口并接exact/legacy路径，投影坐标实际钳制后反算source与整数，未钳制保持原源坐标位模式，identity/缺carrier与数值边界保留。原Editor RED8项4fail/4pass（左0旧实现已满足），同8 GREEN全部通过；编译errorCS0，独立只读审阅通过。原场景run04已启动，待同自然Sage/三次P2上勾拳复验，不重跑其它旧矩阵。


最终留痕检查：Tools/Validate-ChangeLedger.ps1实际PASS，1287 Records、工作树14个受治理代码路径（含既有音效改动，不均属于本轮）；日志change-ledger-final-eac0c94744544d99b35eaf5927064df0.log。git diff --check实际exit0，仅既有LF/CRLF提示，无空白错误。所有原失败和修订原件保持。文档写入命令的Python3.9 newline参数/管道编码失败发生在写入前，随后改用UTF8 base64直接解码写入，未丢弃内容。
