# Type5 matched-pair 源见证

SOURCE_MODEL_ONLY / PASS。最终138行，独立validator 78,933项检查通过；两次执行均exit0，stdout各2,294,742 bytes逐字节一致，SHA-256 `f68655babcbb106920ec9fc9fa29a69160692acd1be27209c969715041b2c8be`。未运行Unity、正式EXE物理输入或Play，不据此声明Unity或整个type5领域已对齐。

写入脚本仅 `Tools/NTSD28AuthorityTrace/type5_matched_pair_witness.cpp` 与 `Tools/NTSD28AuthorityTrace/validate_type5_matched_pair_witness.py`；符号为 `MatchedCase`、`matched_dat`、`matched_state`、`emit_matched`、`wmain`，以及Python `main`、`check/equal`、`frame_state`、`rest_write`。原weapon/type5 witness、正式source、Unity生产与治理文档未由本worker修改。

## Authority 与构建

正式EXE SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。源manifest SHA `07CD47A0623F23D2C439E0E85EABF2ED10F8EAE8FC7D70DDB8396C704B3D778F`，runner SHA `06C13102D8C0A8664A90DDE244A11009A1D69F906BD39BBFCEDECEBF32B9F636`，binary SHA `9562B843B958EB9AFFC771923F8F560FEC67E6FF32BF5409D7F9DB8C85A50AF0`。完整构建闭包与正式身份在 `build-manifest.json`；最终构建输出 `build-138.log`。

规则来自当前playable `battle_world.cpp`：`resolve_confirmed_unarmored_hit` 的selected armor/live rest/current first-body/初始非角色matched门，`apply_standard_hit_rest`，`reset_native_matched_projectile_pair`，`release_native_attacker_motion_hold` / `mirror_native_negative_relation_hit_stop`，`resolve_unarmored_reaction`；普通运动/finalizer来自 `hit_response.cpp`，CRT独立递推来自 `native_random.cpp`。

## 覆盖

| 分类 | 行数 |
|---|---:|
| 两同态3005/3006 × 三种latch/reset × hold(-5,0,5) × relation(0,1,2,3) | 72 |
| attacker自身latch→71、target自身latch→900，两个同态 | 2 |
| 负link有效但不reciprocal的parent2，两个同态 | 2 |
| recover0下standard-rest先覆写hold，两个同态 × 三hold × 四relation | 24 |
| 3005/3006交叉负例 | 2 |
| 当前frame900读取非零matched state | 2 |
| 当前state0、target previous/snapshot3005或3006，不应matched | 2 |
| 两同态 × armor无/0 × liveRest0/5 × current first-body0/1033 | 16 |
| 普通非match，previous13/snapshot12/不强制 × Y(-5,5) × reference(-10,10) | 12 |
| 普通非match，target正link的reciprocal/不匹配child × fall0/60 | 4 |

实际分支104 matched、20普通、2 first-body、4 live-rest拒绝、8 type0 armor feedback。8 feedback发生在rest/body/matched之前，按既有Spark合同消耗两个CRT并发一次spark；它们不属于“matched早返零RNG/零spark”的断言范围。

关键新鲜结果：reset保留contribution_count、瞬时velocity、latch/previous/snapshot、HP/MP/Fall/Bdefend/stats/status/links/team；只raw action、counter0、pending XYZ0。latch900→71与reset900(state451)按各自definition读取。有效parent即使自身link0/child-1也接受hold镜像；不存在parent9不fallback。

特别注意rest顺序：recover3隔离hold路由；recover0先将attacker非负hold改为3、target改为-3，再进行reset/release。negative-link有效parent接收的是rest之后的attacker hold；非正hold复制但不取反，attacker保留。

## 稳定输入格式与重建

准确机器可读合同见 `shape.json`，真实行见 `first.jsonl`。每行参数含 `params.dat[3]` 三份**实际**传给C++ parser的完整DAT文本，三槽objectId/type来自 `before.raw[slot].identity`，避免Unity手工重造DAT。`latchUj`是attacker/third值，`targetLatchUj`是target值。reset动作20/71/900均显式定义，900有state451（current900行则是本例matched state）。

初始spawn三槽frame0，位置100/110/10000、Y0、Z200；先设attacker current91，再snapshot/rebuild，断言唯一候选；之后按before输入设置最终current/latch/previous/snapshot、位置、motion/pending/count、hold、stats/status、links/rest。`candidateAttackerAction:91`不可省略：eligibility检查candidate ITR source-line身份。设置before后不得重新rebuild。

状态块是 `raw[3] / extra[3] / rest[3][3] / sparks[3] / rng`。extra包含count/XYZ、hpConsumed/mpConsumed/score、link/parent/child、kind4SourceCount/weak/incomingScale、statusDx/Dy/Dz/Gain/Facing/Picked/Picking。`extra.yResolved`仅作**source-only diagnostic**，当前无已确认Unitycarrier，不要求Unity发明字段。raw/extra是显式合成fixture输入，不证明默认spawn或完整driver可达性。

after录入writer结果后，三槽motionHoldTimer显式置0，再调用finalize；全部三槽raw/extra/rest/spark/RNG均再次保存和检查。该步骤验证消费count/XYZ与清零语义，不是正常hold倒计时或完整tick证明。

## 命令、失败与边界

最终实际命令：

```powershell
& Tools/NTSD28AuthorityTrace/Build-AuthoritySourceCapture.ps1 -OutputDirectory Build/NTSD28Type5Matched -RunnerSource Tools/NTSD28AuthorityTrace/type5_matched_pair_witness.cpp -ExecutableName type5_matched_pair_witness.exe
& Build/NTSD28Type5Matched/type5_matched_pair_witness.exe
& Build/NTSD28Type5Matched/type5_matched_pair_witness.exe
& D:/anaconda3/python.exe -X utf8 Tools/NTSD28AuthorityTrace/validate_type5_matched_pair_witness.py
& Tools/Validate-ChangeLedger.ps1
```

执行器用Python subprocess捕获原始stdout bytes为first/repeat JSONL，返回码与字节数见 `execution.json`。Python AST解析通过。Change Ledger退出0/PASSED（575 records、23 governed diff paths，当时共享工作区状态），仓库既存warning保留在 `change-ledger-validation.log`。脚本未跟踪，`git diff --check -- <two paths>`退出0只作为命令记录，不能单独宣称检查了未跟踪内容。

首次134例因冻结frame0后改attacker snapshot91而全部status2，validator在case0失败；原始trace `rejected-candidate-first.jsonl`、对应manifest与 `validation-first.log`保留。修正candidate身份后源分支成立。初稿有编译warning但exit0；现最终build日志无warning/error。中间136例77,791项PASS另保留 `validation-136.json` 与manifest，最终以138例为准。后续补例为提高owner/parent验证，未改正式source以让测试变绿。

未覆盖：非type5目标、非零armor与broken fallback、复杂special effect、missing latch definition、非法/越界action全集、完整spark几何重验、其他resource/status组合、默认生产DAT、正式EXE物理输入、Unity编译/测试/Play。本validator对本fixture普通/早返的raw/extra/rest独立预测，对RNG完整scalar独立递推；spark仅检查零/单次与owner，几何仍依赖前序Spark任务。根代理仍需自身Unity RED、实现与全部验收，不能将源PASS直接替换Unity证据。
