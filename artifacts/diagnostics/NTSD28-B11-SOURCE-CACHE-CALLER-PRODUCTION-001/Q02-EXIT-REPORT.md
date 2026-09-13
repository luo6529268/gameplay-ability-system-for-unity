# Q02 加载基础限定出口

2026-09-13，DELIVERED / VERIFIED_LOAD_INFRASTRUCTURE_ONLY。BATCH-02仍进行中，总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

## 逐项出口证据

| Q02要求 | 已获得证据 | 明确保留的边界 |
|---|---|---|
| 明确runtime/DAT/VFS路径 | CONTENT-SOURCE-PATH-CONTRACT-001：真实Unity24/24，显式Logan/旧Unity路径区分 | 不以路径helper存在证明全内容可用 |
| 后台PNG与旧BMP兼容 | PNG-WORKER-DECODE-001：正式1255/1255尺寸/RGBA hash；Unity13/13及最大图1/1 | 解码与最终显示分别验证 |
| native sprite effective range | NATIVE-SPRITE-RANGE-CONTRACT-001：405DAT/773sheet对照，Unity14/14 | declared信息保留，正式内容转换缺口另列 |
| PNG alpha及现有显示管线 | PNG-SHEET-ALPHA-CONTRACT-001：Unity21/21，现有D3D11/URP/Gamma下10个GPU样点 | 不是所有角色/技能完整画面对照 |
| 正式对象catalog/index及完整候选失败门槛 | LOGAN-CATALOG-CONFIG-CANDIDATE-001：source-linked native330/330目录对照、Unity39/39；正式6DAT失败拒绝partial | 不吞帧、不假默认；完整正式候选仍待Q03 |
| 同源config/sprite/UI整体发布与退休 | SOURCE-ATOMIC-PUBLICATION-001：focused27/27，完整事务、输入hash、取消、同ID换源、旧图引用和ownership检查；后继E3最终回归继续覆盖14项 | 保留Unity-native owner与非战斗操作，不热切active battle |
| 准备途中关闭 | PREPARING-SHUTDOWN-OWNER-CAPTURE-001：真实RED→focused26/26/完整SelfCheck；Preparing2/Running2/App1实际关闭0残留、两帧仍Stopped | 只关闭该回访子条件，不撤销未来新模块R16责任 |
| source cache与三个实际caller | SOURCE-CACHE-CALLER-PRODUCTION-001：final40/40，完整SelfCheck最终PASS；native direct/app/menu与menu重进共4次Play均三key一致、World4、关闭0借用/46资源零残留；menu cache hit1并Ready | 数据为合法隔离内容，不冒充正式330对象部署 |
| 默认旧入口回归 | e3-legacy-regression-1真实App启动/关闭通过，E2四项/113资源回收通过 | 默认GameConfig root空，原asset与内容未替换 |
| 文件和框架保护 | CS0；NTSD_Battle dirtyfalse/root14；3059保护3047不变、12声明既有脚本变化、零缺失/范围外变化；Ledger470/40PASS | 无提交/push/资源删除；无Gen/Plugins/asmdef/schema/33ms/顶层关闭重排 |

各前置证据目录为artifacts/diagnostics下对应Change ID；当前包的完整实际命令、失败记录和最新结果链接在IMPLEMENTATION-REPORT.md。测试集合有重叠，不将上表数字相加冒充独立总覆盖数。

## 回访和下一入口

R17返回已完成的路径、raw像素、range、alpha、候选/发布/cache/caller加载子条件，状态PARTIAL_RETURN；正式内容/GUID/引用迁移与全音视条件继续由Q07/Q09/Q10承担。R16本次输入失效、pool after-yield、Preparing owner与关闭检查已返回；未来新增queue/entity/renderer仍必须声明owner和验证。R15 schema/正式内容fingerprint触发未发生，保留Q05/Q07责任。

下一是Q03的NTSD28-NATIVE-DAT-AND-JOINT-FIELD-CONTRACT-AUDIT-001：六DAT九frame拒绝、CPoint27/float32、OPoint24、WPoint tokenizer/default/alias、+2F8、mass/reserved及runtime/shell/ECS/copy/reset/hash联合矩阵。Q04退休和Q05一次协调版本窗口不可提前跳过；当前schema12/20/23保持。

正式DAT/角色图片分批迁移仍为Q07，旧资源精确处置仍待对应清单与授权规则；默认stage.dat暂停及既有表现例外不变。B11完整内容与B12全场景/全角色技能/视听/长跑/最终同版本trace没有完成，本出口不改变这些事实。
