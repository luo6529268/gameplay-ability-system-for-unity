# Unity type5普通无护甲受击

VERIFIED / DECLARED_NO_ARMOR_ORDINARY_TYPE5_SCOPE。准确两生产文件：DamageWriter type5分流和NativeType5HurtTail，HitPlan type5独立CanProject/预测；没有修改type3生产路径，weapon的共用机械helper只加原规则timer80稳定化guard，已有weapon2100已回归通过。

源585/14048，SHA c164b073b4ee789df121f18dcffe8253d0771c21c37703eca35b2e73d4acf30e。首Unity四组before0，direct各2277差异/Shadow各2862，失败保留red/。修正后22/22 PASS：type5四组585完整tuple0差异、Bdefend四组256通过（原各16 writer观察缺口已清）、weapon十四项含2100与回放/旧oracle通过。

修正当前native40/20/0分档、保留80、参考平面判断、raw动作/counter/latch、水平稳定化/整数X方向/累计、vertical默认-7不夹紧、rest后的单次native0xEE攻击者post、仅attacker type3 broken音频。复用原HP/status、原火花owner和有序生命周期。Shadow根据命中前状态独立预测，cursor不commit；没有增加persistent schema。

本轮应用astra-orchestrator技能：worker只读提出Shadow patch，根代理审查并重写集成；reviewer只读核对DamageWriter，在声明无armor范围未见阻断性错误，未据静态审查声称运行通过。

尚未完成：新14个local replay场景、Play2340、完整SelfCheck。完整984的非角色reduced108仍在父任务。新增TYPE5-MATCHED-PAIR-EARLY-001为必须回访：native所有非角色有初始matched3005/3006早返，Unitytype5未接入且旧reset getter857不支持latch/action900；不能用本普通585关闭整个type5领域。prev13/snapshot12、非零reference、复杂held、特殊effect等未被585全部动态覆盖；破甲selected route不能仅用runtimeArmorHp=-1推断，保持父上下文依赖。

原GUI Editor曾退出导致MCP job无终态；随后两个独立2022 batch分别exit2并保留详细RED。用户后来重新打开本项目GUI2022，预检阻止再起同项目实例，后续恢复现有Editor MCP。未使用computer-use，未操作另一个FPSTest项目。Scene hash bcd1047b…/HUDBg30保持，DAT图片迁移Q07尚未部署。

## 最终验证

- 首合并22/22 PASS（c6502f38b24b4ac19d8162985d280a13）：type5四585、Bdefend四256、weapon14（含2100/回放/旧oracle）。
- 扩展50项46PASS/4FAIL（295b4adc9fdb4c4aa8bfa80aeced93cf）：type5四585和14本地回放场景/28replayed ticks、原34、684早期及其回放均PASS；4FAIL仅既有完整984的108 reduced，每组846差异、before0、Shadow额外0，原失败完整保留。
- 完整SelfCheck 2026-09-14 14:21:38Z PASS，未为本type5包修改旧SelfCheck生产断言。
- 真实NTSD_Battle Play14:24:37Z两factory×direct/Shadow×585=2340 PASS，Scene checksum保持、Renderer借用2→2。14:24:58Z关闭PASS：原位restore4→4，World/slots/两pool全0，连续两帧Stopped，已退出Play。
- Scene dirty=false、root14、SHA bcd1047b…保持；生产hash在测试/Play前后相同。脚本编译0 error。未改Scene/资源/非战斗/框架/Server，未提交推送。

下一唯一Task TYPE5-MATCHED-PAIR-EARLY-001。普通样本以外的初始matched3005/3006、native高帧900读写及prev/snapshot/reference/held边界继续取证；随后NONCHARACTER-REDUCED-HIT-TRANSACTION-001的108例。Q07正式DAT/角色图片仍未部署，physical input/visual parity和整体目标未完成。
