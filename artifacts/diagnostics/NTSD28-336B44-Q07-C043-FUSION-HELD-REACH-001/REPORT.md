# 336B44 Q07/C043 融合伙伴持有子体的正式可达性

状态：`SCOPED_FORMAL_SOURCE_ROOT_REACH_PASS / UNITY_SCENE_PENDING`。父 C043/Q07/总目标继续开放。本包只新增正式源码诊断脚本与独立原件，没有改正式 C++ 源码、正式/Unity DAT 数值、Unity 战斗生产代码、场景或非战斗功能。

正式权威：根 `NTSD2.8-Logan.exe` 本轮重新计算 SHA-256 为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。正式 `fusion.dat` 是7+8→51；OID7 李、OID8 小樱、OID420 子体正式 DAT 同项目 `LoganRuntime` 对应文件逐字节相同。`fusion.dat` 原始字节 SHA 不同，仅换行规范化后正文相同，不编辑 DAT。

正式 playable 完整 `GameSession28::step` 诊断：双方同组、HP/baseHP100、seed682973786、李初始action0/X304、小樱初始action256/X300/面左、Z400、mode0，tick23～24按右、25～26松开、27起持续按右。小樱经正式 DAT 256→257 于tick6自然 OPoint kind2 生成OID420/slot50，子体 `interaction_state=-1/linked_parent_slot=1` 持续至tick27；双方双次方向按下后tick28合体7+8→51，小樱槽位休眠。tick28首轮held-refill仍见子体1项且success=true；融合后的第二轮held-refill见同一子体1项、success=false，报告 `linked child 50 has no reciprocal parent link`，将子体关系-1→0，保留linked_parent_slot=1；tick29不再报此错。此处 pass 的success=false是已按正式规则诊断失效关系，不等于战斗失败。

双跑 `run-07/08` 的三profile各60tick CSV及LFR逐字节SHA相同；两负控制 `late_hold`/`early_hold`都有正式OID420持有而未合体，仅排除该输入时点。初版直接设frame257导致wait0先推进、没有子体；原件 `run-01/02` 保留。修正前驱frame256但HP/baseHP100/500的源码阳性在 `run-03/04`，不能用其LFR做根同初态；首次录包忘记 `capture_after_step` 的 `run-05/06` 和先前缺 `-municode`、`-lz` 的编译日志均保留。最终 C++17 源码诊断使用28个正式Core+`game_session.cpp`/`selection_flow.cpp`/`scenario28.cpp`+正式LFR两文件，`g++ -O1 -Wall -Wextra -Wpedantic -municode -lz` exit0、0诊断，最终参数与日志为 `compile-argv-v6.txt`/`compile-output-v6.txt`。

正式根程序以 `run-07`、`run-08` 各自的LFR独立回放，使用原 `--character 7 --enemy 8 --background 23 --p2-human --lfr-slot1-action 256 --lfr-slot1-facing 1`，两次进程exit0、报告passed=true/failureCode0/declaredTicks60。两次根trace逐字节同SHA；正式根tick6也有slot50/OID420/关系-1，tick28李变OID51/action290、slot1休眠、OID420关系0/父槽1并报告相同held-refill失效诊断，tick29诊断为空。[选定逐tick对照](source-root-selected-comparison.json)覆盖tick1～60、slot0/1/50每tick14字段共840/840差异0；根报告另有回放载体尾步tick61，未纳入声明的60tick比较。此证据不代表根程序与重建源码逐hit/完整World/checksum全部同态。

当前正式可达前置条件已从静态候选升为正式源+根程序阳性。下一在原Unity Battle Scene的运行副本按相同正式DAT、双角色同组与相同逐tick方向键流执行生产Driver，观察tick6持有、tick28合体/第二轮失效尾以及次tick不重复报错，并按相同14字段配对；不手工破坏关系、不改DAT或场景序列化。Unity原Scene、完整SelfCheck、全World与最终画面未在本包验证，因此C043仍`RUNTIME_PENDING`，Q07仍`IN_PROGRESS`。

审计：Change Ledger validator exit0/PASSED，1126 Records/5个代码文件且本探针由本ID覆盖；`git diff --check` exit0。Battle/Menu场景、LoganRuntime、GameConfig和ProjectBattleModeConfig的窄Git状态无修改。未启动第二Unity，也未使用computer-use；本包未运行Unity Editor/Play。
