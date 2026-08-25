# HANDOFF — R3-COMBO-01 staggered combo-state persistence

> 日期：2026-08-23
> Change ID：`R3-COMBO-001`
> 状态：`VERIFIED / SOURCE + UNITY RUNTIME`

## Confirmed first difference

- C++ P1的L/J/K交叉internal mapping是authority设计，Unity物理映射方向正确；
- C++九combo字段按引用即时写回；
- Unity local transaction在大多数return前不写回，导致跨tick组合丢进度；
- self-check把该缺陷写成“staggered Naruto L/S/K must not complete”，属于陈旧错误oracle。

## Planned minimal implementation

只让resolver直接`ref`写入九个`input.Combo*`并修正source-conflicting tests；不改physical input、worker、
FrameInputSet、DAT、技能/opoint或render。focused→self-check→input regression→Play Mode分层验证。

## Stop point

所有脚本前置留痕已建立。用户已于2026-08-23明确批准实施；`B-R3-COMBO-001-01`已解除。当前从resolver
最小diff与stale oracle修正开始，不重做source定位，不扩大到physical input、worker、DAT、技能/opoint或render。

代码现已写入：九combo wrapper直接修改`input.Combo*`，两组陈旧self-check已按C++ by-ref语义修正。尚无
compile/focused/self-check/Play Mode证据；下一步从scoped diff与Unity fresh compile开始。

fresh Unity编译已通过：Tundra 4.72s、目标0 error、Assembly-CSharp晚于源码。既存nullable/unused warnings
不属于本包。下一步运行full self-check并以首个失败反算具体branch期望。

08:08:18首次full self-check真实FAIL于OID51 missing-target旧断言；C++ source确认trigger path无条件清零
combo_DJA。missing/valid target两条source-conflicting断言已改为private/runtime 0；需要fresh重编译和复跑。

修正后fresh Tundra 2.28s重新编译通过，目标0 error、DLL晚于源码；下一步复跑full self-check。

08:10:17第二次full self-check真实FAIL于oid6 guard旧transactional-discard断言。source顺序确认guard应
ordinary0/DJA3，opened release应ordinary0/DJA0；两条均已修正，需重新编译复跑。

修正后fresh Tundra 2.47s、目标0 error、DLL晚于源码；下一步再次复跑full self-check。

08:12:09第三次full self-check在held-right partial旧断言FAIL；实际frame102/combo1/cooldowns0符合source。
同组right/left两条已改为step1持久化；后续fresh jump interrupt仍应保持不触发240。需重新编译复跑。

修正后fresh Tundra 2.06s、目标0 error、DLL晚于源码；下一步再次复跑full self-check。

fresh full self-check于08:14:02 PASS；实际执行跨tick、early branches、oid6、OID51、same-tick与Naruto
L→S→K合同。下一步跑input相关EditMode regression，再做真实Play Mode；C++ full trace仍BLOCKED。

EditMode job `ab3e2977fee04f888730e1f44464c443`完成47项/1 FAIL：AI resolver fixture仍expected
`ComboDra=2`、actual3。该测试脚本已在修改前纳入Record，断言现已改为3；下一步重编译并复跑同一矩阵。

Editor fixture修正后fresh Tundra 1.49s、目标0 error、Assembly-CSharp-Editor已更新；下一步复跑同一矩阵。

复跑job `135495e273a646539f7b42eca9b8611b`为47/47 PASS、0 failed/skipped；下一步仅剩真实
`NTSD_Battle` Naruto L→S→K与至少一组L→方向→J Play Mode验收。

Play preflight已生成两个id2角色；动态execute_code受CodeDom长命令行/Roslyn缺失阻塞。已在脚本修改前
将Editor-only显式菜单Play probe纳入Record，尚未写代码；它只能提供真实场景InputBuffer/tick证据，不冒充
桌面物理键注入，physical edge仍由D-INP-006与用户实键复核独立关闭。

probe现已写入：在Play中对bootstrap first player排future-tick att/down/def并写DDJ/frame/object-count Temp
JSON；fresh Tundra 3.62s、目标0 error，尚未运行。该证据明确标为real-scene buffer probe，不冒充桌面
物理键注入。

08:28:20首跑FAIL：direct SimInputBuffer事件被canonical FrameInputSet边界丢弃，目标tick cooldown均0。
这是probe层级错误，不是gameplay verdict。下一版改用Input System Keyboard状态事件走action callback与
canonical packet完整链。

device-state probe现已写：L触发后等DDJ1再排S，等DDJ2再排K，命中authored hit_Dj后释放全部键；直接
buffer预排已删除。fresh Tundra 1.75s、目标0 error；尚未运行。

08:32:12 real-scene InputSystem probe PASS：613 L/DDJ1、614 S/DDJ2、615 K/DDJ3、626 frame271并清零；
后续272→273→274，objects 8→20。当前只继续泛化同一Editor probe并补跑L→facing direction→J；不改gameplay。

probe已泛化并新增forward-attack menu/result：按Runtime.Dir选择physical A/D，第三步physical J，观察
DLA/DRA及authored hit_Fa。尚未重编译/运行。

generic forward probe fresh Tundra 1.18s、目标0 error；真实场景运行待。

08:35:39 forward probe PASS：496 L/DRA1、497 D/DRA2、498 J/DRA3、509 frame263并清零；后续
264→283→284，objects 7→8。两组Play production-chain probe均通过，final self-check/validator/diff待。

final 08:37:09 full self-check PASS；input EditMode 47/47；compile 0 error；validator 58/58与scoped diff PASS。
`R3-COMBO-001 / D-INP-010`已关闭。full C++ trace仍BLOCKED，用户实体键盘/窗口焦点edge仍归D-INP-006，
不扩大为全部输入或全部战斗逻辑对齐。
