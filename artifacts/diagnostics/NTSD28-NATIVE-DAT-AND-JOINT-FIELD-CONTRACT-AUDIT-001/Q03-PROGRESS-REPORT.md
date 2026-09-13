# Q03 字段合同审计：已测量差异与未关闭项

> 最终状态：DELIVERED_CONTRACT_ONLY。以下按阶段保留审计过程；当前出口、完整联合版本更正和下一Q04-A入口以同目录Q03-EXIT-REPORT.md为准。numeric37见证已完成；旧Oscillate reader/base-shell遗漏已补入Q04/Q05，base-shell1→2。实现与运行时对齐仍未完成。

日期：2026-09-13。状态：IN_PROGRESS / DAT_WITNESS_AND_DECODER_SHAPE_CAPTURED / FULL_READER_AND_JOINT_MATRIX_PENDING。

本报告属于 `docs/ai/TASKS/NTSD28-NATIVE-DAT-AND-JOINT-FIELD-CONTRACT-AUDIT-001.md`。Q02加载基础已交付；Q03仍未到出口。这里的双端数据是源码链接的诊断程序输出，不是正式EXE与Unity真实战斗场景trace。本轮只读生产代码、运行既有诊断工具并写文档/诊断数据，没有修改脚本、正式资源或schema。

## 身份与实际验证

- 正式EXE SHA-256复核为 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。
- 既有native capture executable与其9个源文件、31个header共41个身份检查无漂移，见 `native-tool-identity.json`。诊断exe没有晋升为正式权威。
- 输入为六份未改写的正式DAT副本和六份最小合成DAT，路径/hash见 `input-manifest.json`。
- 实际运行 `Temp/NTSD28ContentAudit/native/AuthorityContentCapture.exe --input-root <repo>/Temp/Q03FieldAudit/inputs --output <本目录>/native-focused.jsonl`：12文件语法解析通过，0 warning/error。
- 实际运行 `dotnet build Tools/NTSD28ContentAudit/UnityContentCapture.csproj --no-restore`：0 warning/error，见 `unity-capture-build.log`。
- 实际运行同工程 `dotnet run --no-build -- --input-root <repo>/Temp/Q03FieldAudit/inputs --input-mode plaintext --output <repo>/Temp/NTSD28ContentAudit/unity/q03-focused.jsonl`：12文件语法解析通过，8文件Converter失败（6正式、2合成）。首次尝试直接输出本目录被工具路径限制拒绝；改用工具允许的Temp诊断目录，未修改工具。
- `focused-comparison.json`保留9个正式失败frame及6个合成用例的两端raw keys、normalized值及错误；`decoder-contracts.json`记录27/9/24字段形状，不能将其中SHAPE_PRESENT状态理解为consumer已对齐。
- 本Q03轮没有新增Unity Editor编译、SelfCheck或Play证据；Q02的40/40、完整SelfCheck和加载Play证据仍只关闭其原范围。

## 六文件九frame的完整根因

相对路径以正式 `resources/runtime/decoded_dat` 为根。行号来自这次冻结输入。

| DAT | frame / line | 两端差异 |
|---|---|---|
| c/ank/ank.dat | 319 / 1448 | WPoint effect:1；native保留raw但9字段decoder忽略，Unity严格adapter拒绝 |
| c/ank/ssnk.dat | 428 / 487 | WPoint 7:0；native lexer不承认以数字开始的字段名，Unity生成名为7的属性后拒绝 |
| c/hir/hir.dat | 40 / 280；52 / 321 | WPoint dircontrol:1；native不将其作为WPoint语义字段，Unity拒绝 |
| c/hir/hir.dat | 414 / 1847 | CPoint drain:600；native解码且resource事务实际消费，Unity19字段合同缺失 |
| c/min/min.dat | 414 / 1974 | 同上，CPoint drain:600 |
| c/min/sag.dat | 414 / 1931 | 同上，CPoint drain:600 |
| c/nar/nar.dat | 30 / 196；52 / 322 | WPoint dircontrol:1；同hir的WPoint差异 |

纠正Q01摘要的解读：hir有三处失败，第三处是CPoint，不能按文件首个错误把三处都归为dircontrol。所有这些块均可在单行内声明；以整行起止标签扫描会漏计。

WPoint的出口不是新增effect/dircontrol/7三个DTO字段。需要在明确的Logan来源解析/转换路径实现原版字段准入，保留用于审计的raw文本；Converter、formal adapter和loader应遵守同一合同，不能只绕过一层异常。

## CPoint：27字段不仅是数量差异

权威：`source/ntsd28_core/include/ntsd28/combat_records.h`、`src/simulation/combat_records.cpp::CombatRecordDecoder28::catch_point`；实际reader为 `battle_world.cpp` 中catch advance/settlement/placement路径。当前playable build.ps1包含这些源。

27项为kind、x、y、injury、cover、vaction、aaction、jaction、daction、taction、faction、baction、uzaction、dzaction、throwvx、throwvy、hurtable、fronthurtact、backhurtact、decrease、dircontrol、throwinjury、throwvz、z、recover、drain、gain。三个throw速度为float32，其余24项int32；省略/非法输入按相应native decoder归零，同名精确大小写key采用最后一项。first_catch_point使用第一个块；缺块与存在全零块须区分，关系建立的inline helper有自己的零值策略。

Unity现为19个int。缺少faction/baction/uzaction/dzaction/z/recover/drain/gain，三个速度类型也不符。Converter另将fronthurtact写入injury、backhurtact写入cover；这不符合native四个独立字段。

| 实际合成输入 | native输出 | 当前Unity输出 |
|---|---|---|
| injury3 cover4 fronthurtact12 backhurtact13 | 3 / 4 / 12 / 13 | 12 / 13 / 12 / 13 |
| throwvx1.5 throwvy-2.25 throwvz-842150451 | 1.5 / -2.25 / -842150464（float32舍入） | 1 / -2 / -842150451 |
| int字段+5、12junk、1.5、2147483648 | 四项均0 | 5 / 12 / 1 / 2147483647 |
| cover重复1后3 | 3 | 3 |

native整数用from_chars且要求消费全部文本；Unity ParseInt接受前缀并饱和溢出。native float使用strtof、有限值和完整结尾检查。不得为-842150451添加未经消费链证明的“哨兵删除”；原版实际float值就是舍入后的-842150464。后续还需冻结float的canonical bit编码、正负零和解析边界测试，不能仅更换C#字段类型。

已追到的reader：

- battle_world.cpp:5846起，decrease调catch timeout；5885～5924按既定输入窗口/顺序决定A/T/D/UZ/DZ/F/B/J action。
- :5951只有throw_vx非零进入throw；正throwinjury先执行drain/gain资源事务，再写environment_state_320。随后速度由float转double，不做小数截断。
- :6090 injury非零且catcher frame_counter为0时，先资源事务再伤害统计；cover排除计时器逻辑独立。
- :6174起X/Y挂点；z为0采用cover分支，非零按z偏移；cover高位决定显示相位。
- CPoint recover/fronthurtact/backhurtact目前在已检索的playable simulation中只确认解码，未找到实际战斗reader；保留内容身份，不发明效果。battle_world.cpp:2815附近相似hurt字段属于FusionRecord，不属于CPoint。

旧CPoint27审计可复用，但旧Direction B hold已被D-023替代，Goal17～20已退休的kind2伤害等consumer不得重做。旧“33个CPoint块”计数已经其后续多行修正废止，不得复用。

## OPoint：24字段与实际生成链

权威 `object_spawning.cpp::ObjectSpawnPlanner28::decode/plan_frame` 解码24个int，默认0。Unity Value/Converter/ToLegacyTask当前只有kind/x/y/action/dvx/dvy/oid/facing八项，其余16项不会完整到达materializer。

新增范围：z、dvz、hp、mp、team、reserve、effect、pic、centerx、centery、centerz、framea、attacking、join、join_reserve、join_pic。

已确认：plan_frame处理facing>10的数量及朝向余数；team0继承，否则使用显式值；Z为parentZ+pointZ+1，kind1才取baseDVZ。battle_world materializer先检查kind1/2及definition/action，hp/mp正值优先、否则按对象默认，再应用stats比例；kind1依次消费centerx/centery/centerz/framea随机偏移。effect>0只在kind1改render_phase。

特别注意：reserve/join/join_reserve/join_pic的实际条件是child type0或5，且parent ordinary_credit_gate_2f4不为2；此处没有额外kind1条件，附近注释不能替代代码。分别写revive_lives_30c、revive_next_hp_314、revive_next_lives_310、revive_visual_id_184。framea与hp在kind2持有路径还有额外reader（:7810、:7813），不能仅按kind1随机项解释。

OPoint pic/attacking目前只找到decoder写入，无已确认playable规则reader，仍需内容保留。Unity逻辑路径 `BattleLogicObjectPointRuntime.ProcessOneLateOpoint/ConfigureLateOpointPosition` 当前固定parent team、dvz0、Z+1；另一presentation materializer、task复制/复用、random调用顺序及完整kind2尾部矩阵尚未闭合。

## BDY / ITR需要继续核验，不能只比DTO

native `battle_world.cpp:4356 -> HitCandidateBuilder28::append_pair_geometry -> CollisionGeometry28::project/overlaps_depth` 的live调用已确认。ITR raw zwidth0只在几何投影时变15；BDY raw zwidth0保持0，BDY z字段不加到中心。native深度使用半径之和及包含端点的比较，2D使用严格重叠。held weapon还可由所选strength行覆盖ITR zwidth/z。

Unity `InteractionArea.zwidth`默认15；Converter的Body只存X/Y/W/H。BruteForceSceneQuery若干候选路径使用`zwidth > 0 ? zwidth : 15`并排除等于边界的深度点。**这是需定向对照的新候选差异；目前未完成全部fast/fallback/cached caller和正式DAT可达性核验，不声称已测量到实际游戏命中差异。** PhysicsState可视体积helper不能自动代表生产命中路径。

ITR caughtact/catchingact在native取有效最后字段中的首个整数；Unity存数组但多个生产reader使用[0]。数组形状不同本身不证明行为错。缺省/重复/packed pair/非法输入与pickedact/pickingact的全reader还需一起冻结。

## Q03剩余出口及顺序

1. 补完整CPoint/OPoint字段到所有生产reader的矩阵，含两条materializer、copy/reset、canonical float与来源准入策略。
2. 对BDY/ITR候选深度边界、负宽度/省略、strength override、packed action执行最窄同输入双端验证；不直接重开已关闭B5。
3. 完成 `JOINT-FIELD-MATRIX.md` 的+2F8、mass、五类reserved、shell/ECS/checksum/trace所有读写与版本影响；对残余引用区分兼容API、diagnostic和实际行为。
4. Q03全部合同冻结后才进入Q04剩余行为退休。Q05同一协调窗口实施12→13、20→21、23→24及实际必要shell版本；Q06按字段producer/consumer接线，Q07再正式迁移内容。若BDY/ITR新增字段影响窗口，先纳入Q03，不另开第二次破坏兼容窗口。

本报告没有将Q03、B6、B7或B11标记完成；总目标保持ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

## 本轮收尾检查

`Tools/Validate-ChangeLedger.ps1 -RepositoryRoot $PWD.Path` 实际PASS，470 records / 40 governed code files，输出见 `ledger-validation.txt`；这些脚本diff属于既有Q01/Q02声明改动。本轮未新增script Change。

3059文件保护基线复核：3047不变，12项仍是前批声明的生产脚本修改，0缺失，详见 `workspace-protection-progress.json`。正式DAT/图片未部署、未清理用户资源。`git diff --check`通过，只有工作区既有LF/CRLF提示。读取状态另存 `audit-source-identities.json`；完整名称引用清单 `joint-field-reference-inventory.txt` 待逐条区分test/diagnostic/生产reader。

### 后续几何见证追加（不改写上文只读轮的历史）

同日新增独立Task/Change `NTSD28-Q03-COLLISION-GEOMETRY-WITNESS-001`，只写诊断工具/Editor测试，实际同DAT native14/Unity42比较15相同、27不同。上文BDY/ITR候选差异的普通type0边界部分已有实测，见该包REPORT.md和comparison.json；held strength与dense/cache/parallel路径仍未覆盖。CS0/Scene clean/Ledger471-43PASS，原50个读取状态hash无变化。父Q03继续IN_PROGRESS，下一完成其余consumer与联合字段合同。
