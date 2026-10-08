# SetPass 优先修复：显式视觉资源输入补充证据
状态：PARTIAL_STATIC_EVIDENCE / IMPLEMENTATION_NOT_AUTHORIZED / PERFORMANCE_UNCHANGED。
本轮为78取证的有界补充，不新开优化候选或增加已执行优化子批计数。57执行、六项4/6、H07/H11 OPEN；没有代码修改、Unity控制、测试/测量或Goal状态变更。人类尚未确认视觉准备链修复范围，自动Goal续跑不等于授权。

## 本轮新增事实
正式内容根：J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan/resources/runtime。
BattleContentSource.cs:32–36定义catalog.csv / decoded_dat / vfs；先前data目录查找失败仅是查询路径错误，不是正式资源缺失。77固定千人案例根OID1为Sakura（正式catalog）；本轮只为该案例取证，不构建全角色方案。

以OID1 DAT中全部原始oid整数token进行保守递归，得到13个注册对象、22张去重file(...)身体/技能sheet，无未注册目标。13 DAT的实际SHA分别与catalog.plain_dat_sha256相等；完整边、路径、SHA和PNG头尺寸在Operation/before-manifest-01.json。对象IDs：1、120、121、123、150、151、202、204、208、700、706、900、999。
该正则摘录只是源证据，不是生产DAT parser或语义依赖闭包：不能裁定所有frame/action可达，也不覆盖隐式规则、融合、模式生成或共同shadow/spark/words/UI等输入。不得用“13对象”直接实现生产过滤或宣称资源齐全。

| 输入 | 本次真实尺寸/字段 | 对现有准备方式的含义 |
|---|---|---|
| vfs/c/saku/saku.png | PNG头799×1119；decoded_dat/c/saku/saku.dat:6引用 | 身体sheet满足现有2046最大content尺寸，单角色本身不是本案例的尺寸阻塞。 |
| vfs/c/saku/rage.png | PNG头683×247；同DAT:7引用 | 同样满足尺寸条件。 |
| OID900的vfs/a/cla/fla.png | PNG头3000×800；decoded_dat/c/saku/saku.dat:2016声明oid900/action100；decoded_dat/a/cla/cla.dat:6声明该sheet | 该显式引用对象拥有超大sheet；保守整对象资源选择必须保留此声明。未证明本窗口/action100会显示该sheet，不能说它导致75高SetPass。 |
| 22张sheet合计 | 源RGBA估算26153356 bytes，约24.9418MiB；21张满足尺寸、1张超大 | 仅w*h*4估算，未解码、未运行planner、不含共同资源/菜单/临时/旧新resident；不是atlas或完整双预算通过证明。 |

BattleAtlasLayoutPlanner.cs:95–101：PageSize=2048、ExtrusionPadding=1、MaximumContentSize=2046；CharacterAnimtorManager.cs:3198–3205分类，3215–3221若超过设备MaxTextureSize则拒绝，3224–3228设备可承受的超大sheet继续SourceTexture2D。该守卫应保留，不能强制全部array、缩图、裁切/丢资源或抬预算绕过它。

## 对下一动作的影响
“本局完整视觉依赖＋预算内共享纹理绑定”的候选需保留合理SourceTexture2D例外；不能承诺整局所有命令都具有同一binding。本案高频身体＋阴影交替仍是75已经确认的断点，超大特效是否出现/占多少draw未测，不另造阻断或假定所有角色无法优化。
完整生产依赖的冷准备接线、预算、所有权/lease与适用验证仍属于待批准实施范围；本次不冻结bank/格式/预算，不改原分段/透明顺序。数值留到被批准候选的既有必要验证，不追加可选的全角色/GPU证书。
75/77实测复用：中央快照2024segment（相邻2023仅纹理身份），77 Combat1000平均SetPass1995；本轮没有新的下降、FPS或0GC结果。原始失败、候选默认OFF和H11迟发事件待办保持。

## 留痕与验证
Operation/Task：NTSD-OPTIMIZATION-SETPASS-VISUAL-INPUT-AUDIT-20261008；48只读输入指纹、4当前dirty治理文档准确副本在docs/ai/FILE-OPERATIONS/NTSD-OPTIMIZATION-SETPASS-VISUAL-INPUT-AUDIT-20261008/。只追加唯一总表、STATE、handoff、INDEX顶部与新增本包文件，旧正文保留；不新建进度总表。
最终字节/SHA/历史604guard/HEAD/staged/diff/ChangeLedger结果在final-audit-01.json及Operation实际交付段。本报告不以文档操作通过代替生产验收。
