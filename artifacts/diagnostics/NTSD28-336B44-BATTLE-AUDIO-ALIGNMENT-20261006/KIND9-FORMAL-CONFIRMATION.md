# kind9 正式声音事件确认（2026-10-06）

状态：FORMAL_KIND9_EVENT_CONFIRMED。Unity 修复的检查结果由父REPORT最新追加记录裁决。

直接运行原336B44 EXE，原runtime DAT，新增受控LFR/CLI初态；GDB只读实际Driver返回向量，不重建Core/EXE、不注入游戏状态、不改DAT。正常软件断点仅用于运行观测。当前header仅用于布局假设，audio offset1816、event stride48、string offset16已由实际正声音的source/path/worldX及完整向量边界验证；不把该header当作正式战斗规则。

| 实际对象/场景 | 正式交互结果 | 四tick声音事件 |
|---|---|---|
| OID810/frame107 → OID120/frame0，近距 | kind9/type1拒绝，共4次候选消费 | frame data005三次；无额外effect/broken |
| OID810/frame107 → OID206/frame0，近距 | kind9/type3转移action30；同tick帧30→11并spawn OID211，随后生命周期移除 | frame data005三次；spawn帧data020一次；无额外effect/broken |
| OID810/frame107 → OID906/frame53/state3005，近距 | kind9选择action40 | frame data005三次；无额外effect/broken |
| OID810/frame107 → OID206/frame0，远距 | 无候选关系消费 | frame data005三次 |

四份root-report均passed=true/completedTicks4，16个返回tick序列1–4，capture error0，正式进程退出0，前后EXE SHA336B44不变。所有捕获声音source=1（frame），builtin/definition_weapon_broken均0。转移例中020是实际frame声音，不能把它误删为broken。零交互的远距正声音与近距005相同，证明观察器能够读取声音，而非漏记录导致的静音。

旧attempt01在LFR容器断言前停止；02硬地址未处理ASLR；03默认断点跳过prologue；04返回断点重复采样及旧LFR尾部header不匹配。原件全部保留，不称通过；05修正观察器/新夹具元数据后才使用上述结论。夹具尾部期望字段不是生产规则修改。

冻结源只读检索未找到可归属336B44的battle_world.cpp快照：pre_update对应B1E13、ZIP EXE为1277B70B。当前开发源码也不能默认对应336B44。此限制不阻止本次正式EXE实际观测；不再用当前Core候选推断kind9。

准确原件：formal-kind9-attempt-05/summary.json、四子目录observation.json/root-report.json/root-trace.jsonl/debugger-stdout.log、kind9-formal-evidence-checked.json。观察脚本 Tools/NTSD28Q10Diagnostics/battle_audio_formal_kind9.py。文件操作/备份 docs/ai/FILE-OPERATIONS/NTSD28-336B44-BATTLE-AUDIO-KIND9-FORMAL-CONFIRM-20261006。

这只确认实际三类对象分支及对照的逻辑声音事件；没有设备听感、自然完整技能输入或全角色整场音效的证据。旧总目标仍USER_ACCEPTED_SCOPED_CLOSURE。
