# Q07/C011 正式 DAT 传送相位根见证

最新revision3共同条件：原Scene第一轮显示Z650经投影后超出项目walkregion，被限制为481。正式BG23/650的v2根证据仍有效但不是该Scene同态。为对照不触及两边边界，v3只将诊断初态改Z400、正式background1（San375..575）；Unity地图/边界保持。v3新编译四例12tick仍全部rootexit0/PASS、864字段相同；目录`*-v3`及artifact-hashes-v3.json独立保存。下方BG23/Z650属于v1/v2历史条件，不能与v3混用。Scenecounter也应读取AttackingCounter，不是旧AnimSub，已另包更正。

状态：`VERIFIED_SCOPED_ROOT_TRACE / UNITY_FORMAL_CONTENT_PENDING`。冻结336B44根正式EXE，当前playable源码四例12tick18字段共864/864一致，四回放exit0/passedtrue。只证明选定传送链的时序与声明字段；nativeParityClaim=false，原Scene正式内容与完整World/物理键仍是独立出口。

同seed682973786、源X500/目标800、Z650、HP/base/MP500、中性人类输入，两个类型分组正确：LeeOID7(state400)与NarutoOID2是team1/2，SakuraOID1(state401)与Naruto是team1/1。Formal root初始action/MP显式命令行覆盖，位置/team/baseHP由真实LFR承载。固定背景23仅在根诊断，不部署Unity背景DAT。

| case | 正式 DAT 初态及后继 | 正式可观察结果 |
| --- | --- | --- |
| lee-enter-v2 | action350→242/state400 | tick1仍350/X500；tick2尾部进入242、tick3保持X500；tick4当前242相位0传送X680/Z651，尾部推进243 |
| lee-skip-v2 | 初始242/state400 | tick1相位1跳过，X500；tick2传送X680/Z651，尾部进入243 |
| sakura-enter-v2 | action96→97/state401 | tick1仍96/X500；tick2尾部进入97、tick3保持X500；tick4传送X740/Z651，尾部进入98 |
| sakura-skip-v2 | 初始97/state401 | tick1跳过，X500；tick2传送X740/Z651，尾部进入98 |

传送在frame motion后、physics前，读取的是尾部帧推进**之前**的当前动作；不能用tick末state代替执行时state。目标碰撞Y复制、Z+1、水平120/60偏移符合正式当前调用链。每例12tick×18字段216/216，整数严格相等，preciseXYZ与motionXYZ因根JSON格式按绝对误差1e-6比较（comparison.json明确记录）；没有首次差异。各目录保留root argv/process/report/trace、sourceCSV/LFR和comparison。

第一版四例8tick的576字段也全部相同。只有lee-enter最终header真实失败46：正式播放器额外EOF tick9执行后继攻击，root inputScoreTotal348由0变65/targetHP500→435，而recordedtick8尚无此命中。该错误原件保留，不算回放PASS。第二版同条件延至12tick，包含source与root的tick9自然命中，四例全部PASS。没有改正式载体、EXE或DAT以使报告通过。

两个g++诊断版本均按已声明playable闭包编译exit0；编译argv/output/source快照留证。[哈希](artifact-hashes.json)确认根EXE336B44、currentprobe等于v2编译快照、Lee/Sakura/Naruto三正式staged DAT各自同SHA及四Scene/config保护值。这个工具只建立正式输入载体与观测，未改任何生产规则。
