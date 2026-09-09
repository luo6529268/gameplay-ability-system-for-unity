# Task Contract — NTSD28-B2-NATIVE-TYPE0-BUILTIN-DATA-SEAMS-001

> 状态：`FOCUSED_TEST_PASS / DATA_SEAMS_READY / CONSUMERS_UNCONNECTED`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2 / NATIVE-TYPE0-BUILTINS`  
> 建立日期：2026-09-04

## 目标

在迁移native type-0/common built-ins之前，补齐其正式live path实际读取、但Unity当前定义模型丢失的
数据契约：`walking_frame/running_frame/heavy_walking_frame/heavy_running_frame`有序动作序列，
以及linked `<stats>`中的9个动作选择字段。只建立parser→immutable character data seam与focused
证据；不在本包连接built-in consumer，不修改正式Config/DAT。

## Authority 与当前事实

- 正式playable build闭包中的`input_routing.cpp:630-653`：`movement_action`优先读取BMP命名动作
  sequence，缺失/空时才使用built-in cyclic fallback；sequence按声明顺序和rate推进。
- `input_routing.cpp:808-870,1063-1065,1232-1235,1283-1307,1433-1438,1464-1482`：
  linked definition的`normal_attack1/2`、`light_throw`、`weapon_drink`、`heavy_throw`、
  `run_heavy_throw`、`run_attack`、`jump_attack`、`sky_light_throw`为正式动作选择器；字段0或
  linked definition缺失时使用各调用点默认动作。
- `input_routing_tests.cpp`的custom running、linked stats、heavy walk/run、air/dash selector测试均由
  `main`调用；formal authority 46/46及fresh binary已在前包证明通过。
- 权威decoded runtime存在单行和跨行sequence（例如Yagura `running_frame: 6 9 10 11 519 520 521`、
  Suigetsu四组跨行sequence）。Unity tokenizer当前能看到token，但V2 parser只把sequence count当
  普通property并丢弃后续动作；`LF2CharacterData`也没有sequence和9个stats字段。
- 既有`HitConfirmEa`已承担authority `kind6_input_timer_0ea`的命中确认/timer语义，本包不新增重复
  runtime scalar；该绑定将在ground built-ins focused中复验。

## 允许修改

- `Assets/NTSD/Scripts/Animation/LF2CharacterData.cs`
- `Assets/NTSD/Scripts/DatParser/Runtime/Models/Lf2BmpSection.cs`
- `Assets/NTSD/Scripts/DatParser/Runtime/Parsing/Lf2DatParserV2.cs`
- `Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs`
- 新增`Assets/NTSD/Scripts/Test/Editor/NTSD28NativeType0BuiltinDataSeamsEditorTests.cs`及`.meta`
- 本Task、Change Record、Ledger、STATE、handoff与总表

禁止修改authority、`Assets/NTSD/Config`、Scene/Prefab、ProjectSettings、Packages、runtime input
consumer、RNG cursor、frame/physics/hit/spawn、B11内容值或资源。不得把新seam写成built-ins已对齐。

## 不变量

- sequence保留声明顺序与重复值；count只限定本sequence读取边界，不成为动作、不吞掉后续tag/property。
- inline与multiline token布局产生同一normalized sequence；缺失sequence保持空，并由未来consumer走
  built-in fallback。
- 9个stats字段保持`int / absent→0`；本包不把0替换成调用点默认值，fallback属于consumer职责。
- converter重复应用同一definition时必须替换sequence，不能append旧值；不同
  `LF2CharacterData`实例不得共享可变列表。
- parser/converter只在加载阶段分配；未来hot route只读取已构建List，不查询Mono singleton。
- Direction B内容权威不变；本包只让现有输入文本可被忠实表示，不改任何内容文件。

## 验收

- test-first红灯覆盖缺失sequence model、4组carrier、9个stats carrier；
- focused覆盖inline/multiline、顺序/重复、count边界、缺失默认0/empty、重复convert替换、实例无alias；
- Unity compile0、相关parser/carrier回归、full SelfCheck、Console0、diff与Ledger PASS；
- production built-in caller保持未连接，ground/air/joint trace另包。

## 回滚

移除4个sequence和9个stats carrier、BMP sequence model/parser分支与converter复制，并删除focused test；
不触碰已通过的native combo/direct/hold/direction生产接线。

## Test-first 证据

- 2026-09-04：新增6项focused contract后，最后一次Unity编译段得到37条唯一C#错误；全部仅来自
  新测试引用尚未实现的`Lf2BmpFrameSequence/FrameSequences`、4组movement List和9个stats字段，
  当前编译段的新测试之外错误为0，Tundra按预期失败。
- production data seam已写入合同列出的4个加载/模型文件；runtime built-in consumer仍未连接。
- Unity compile0；new6/6、相关parser/carrier20/20、B2 broad247/247；15:18:00 full SelfCheck
  `PASS`，预期7条rest-binding负向日志清除后Console error0；Ledger134/89与diff check通过。
- ground、air/dash/redirect consumer及joint trace仍待，不得把本状态表述为built-ins对齐。
