# D-024 持有武器显示挂点

2026-10-05 后继状态：`GREEN_GEOMETRY_SCOPED / ORIGINAL_SCENE_RENDER_PENDING`。下方RED及修复前计划是历史事实，原件保留。唯一生产改动为共用`LF2ObjectRenderer.ResolveHeldVisualAttachmentOffsetPixels`，在非恒等投影时用实际已投影两端位置差补偿保留1.5倍图像的本地WPoint差；中央/旧呈现共用，不改DAT、源规则、Scene或非战斗。

原Editor MCP刷新后同两参数job `c4e6daee940d483bbc32735e7242d0ce` **2/2 PASS**、46.7945989秒：[终态](green-test-result.json)、[退出状态](green-editor-poststate.json)、[CSV比较](green-comparison.json)。恒等0/0维持且整行不变；项目视野从X6/Y4.04656982421875改为0/0，仅offset_x/y、weapon_point_x/y和difference_x/y改变，其余所有记录的源/frame/view字段相同。生成Editor334warnings/0errors/12.35秒，原EditorConsole0error，Battle Scene clean/nonPlay/root11/SHA `253B2EBAD322AA6EC18488DFBBF555BB93AD897940A342E4130E77CE778F9010` 保持。一次观察超时后重取同job，无再次执行；progress.total8839是发现数，实际仅2项。

探针`FOCUSED_TEST_PASS`，生产`RUNTIME_PENDING`；原Scene真实拾取画面/GPU、左朝向等未覆盖。这两项记录纳入证据复用，不为抬状态扩矩阵。Q07/Q09/Q12/总目标开放。

2026-10-05 当前状态：`RED_GEOMETRY_CONFIRMED / PRODUCTION_FIX_PENDING`。本例使用当前正式非排除DAT、OID2/OID120自然拾取的生产完整Driver前2tick，恒等与固定2048×1152视野各一个原Editor参数例；本体图像尺寸保持用户批准的1.5倍。来源为当前336B44 `BattleWorld28::settle_held_refill_objects` 的两端WPoint源位置对齐、`render_snapshot.cpp` 的源屏幕公式，以及当前对应源码[已取得的24tick定位样本](../NTSD28-336B44-Q07-D024-WPOINT-SPATIAL-AUDIT-20261002/native-336b44-24.csv)。这不把诊断程序升级为正式根EXE运行或GPU像素证书。

原Editor job `23310520aeec408b9a11737b5f720400` 精确指定新增方法，只执行2项。终态`failed`，progress.completed=2，失败项为配置视野`True`的X接触断言，actual6；工具的progress.total=8839是发现的全项目数量，不是执行了8839测试，终态result=null因此不捏造完整汇总计数。[启动](red-test-start.json)、[终态](red-test-result.json)、[退出状态](red-editor-poststate.json)。一次观察请求25秒超时，后续重取同job终态，没有重启测试。

| tick2字段 | 恒等视野 | 项目固定视野 |
| --- | ---: | ---: |
| holder/weapon action | 115/24 | 115/24 |
| 源X | 201/214 | 201/214 |
| 源Z，与对应源码CSV同态 | 542/543 | 542/543 |
| 源Y | 0/7 | 0/7 |
| 实际视图X整数 | 201/214 | 201/220 |
| 实际视图Z整数 | 542/543 | 542/543 |
| 旧本地补偿X/Y | 6.5/4 | 6.5/4 |
| 图片WPoint画面差X | 0 | 6 |
| 图片WPoint画面差Y | 0 | 4.04656982421875 |

原件：[identity CSV](pickup-anchor-identity-20261005T103932030-077b16d5ade14ed7b49beea05880f151.csv)、[project CSV](pickup-anchor-project-20261005T103954612-be030c277fac4edf947634bf95f32ec9.csv)，含正式frame center/WPoint、原生源X、源Y、实际显示X/Z、共用pivot/offset及最终两端点。图片尺寸在WPoint相对pivot的计算中抵消；没有加载PNG或生成GPU截图。结果是实际生产几何函数的有界首差，不声称实屏像素已取得。

原因：两端本体的实际相对位置已按世界投影改变，旧挂点补偿`(visualScale-1)*(holderLocalPoint-heldLocalPoint)`仍假设逻辑距离为1:1。正确补偿须以双方图片上的本地WPoint差乘保留的1.5倍，再减去双方**已投影且取整的实际屏幕位置差**；仅在表现出口读字段，不反写逻辑/碰撞，也不改变武器跟随位移倍率。恒等分支保留现有行为，既有“关系未贴齐”的历史夹具不被顺手重构。

RED后原Editor非Play/idle/无测试，Battle Scene clean/root11、磁盘SHA仍 `253B2EBAD322AA6EC18488DFBBF555BB93AD897940A342E4130E77CE778F9010`，Console0error。生成Editor0错/301warnings、Ledger1268records/19dirtycodepaths通过。生产尚未改；下一只改中央与Legacy共用的`ResolveHeldVisualAttachmentOffsetPixels`，另立准确Record后原两例GREEN。原Scene实际持有/GPU、其它方向/武器/动作及整个D-024仍不能据此宣称完全一致。
