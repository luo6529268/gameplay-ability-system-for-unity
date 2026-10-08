# 第55批 retained raw 调用点证据

状态：CALLSITES_IDENTIFIED / RETAINED_RAW_RECOVERED；原live capture PARTIAL保留，不把恢复当原8frame实时导出成功。

只执行一次恢复菜单，追加帧1865—1937，共73个valid Main Thread frame/144个完整 Driver marker 子树，scanComplete=true。当前history原1565—1864的300帧未清空，恢复前独立保存35,640,372B；SHA与原prior-history.raw相同。Profiler十设置before/after逐字段相同，未进入Play/未新采样。原raw1,040,390,105B保持。

Driver子树内7个GC.Alloc均有metadata和非空地址栈：

| Profiler frame（不是逻辑tick） | sample | metadata bytes | 已解析最上层调用点 |
|---|---:|---:|---|
| 1865 | 35123 | 46 | String.Concat → BattleKnockoutFeedRowProjection.BuildName |
| 1865 | 35125 | 46 | 同上 |
| 1867 | 24893 | 30 | BattleCharacterActionWriter.RouteNativeRunning；更上层地址未解析 |
| 1929 | 36358 | 46 | String.Concat → BattleKnockoutFeedRowProjection.BuildName |
| 1929 | 36360 | 46 | 同上 |
| 1932 | 36440 | 46 | 同上 |
| 1932 | 36442 | 46 | 同上 |

7事件metadata合计306B，仅本instrumented retained片段，非54校准counter字节数、RSS或完整300tick总分配。6拼接事件276B；1奔跑入口30B。marker包含observer Begin/End，但这7栈均经过真实StepOneTick，不是仅observer方法。没有绝对tick映射，不宣称完全解释54六事件或55八事件。outsideDriver2,858,883事件来自本片段主线程其余scope，未扫栈/不归因生产。

本次重扫非活跃文件：BattleKnockoutFeedRowProjection.cs:266—280 以slot/OID/append为key，cache miss时拼接并Add；目前dictionary32、没有独立冷态全名预热。实际stack映射到方法起始265，实际拼接位于271/276。这是可进入后继限定修复的已证分配源，需先确定冷加载/目录有效期/容量和显示等价，不改Q06调用体。

BattleCharacterActionWriter.cs:1202—1235入口的ldstr/延迟字面量等只是推断；30B栈没有解析到更上层分配函数，不能因尺寸吻合就认定left是根因。不凭此改规则；后继以必要最小局部证据裁定。

Q06文件只由已有raw栈引用，未读取方法体。没有改变Scene/资源/33ms/pass/RNG/publication/segment/关闭或collector/defaults。H07主耗时仍54有效窗口PairExactLoop约51—54ms、P95112.550/114.910ms；本恢复不是性能改善，H07/H11继续OPEN、阶段4/6/34已执行。

验证：17新增有效缺接口RED→原Editor66/66 GREEN、0skip；恢复state无error、settingsRestored=true，原Menu clean8roots/idle。同批精确11dirty backups/69guards事前核同。原始完整解析输出 windows-01/logic-gc-callstacks-recovered.json，状态 windows-01/logic-gc-recovery-state.json；最终指纹/validator追加于REPORT。

官方API子树边界依据：[Unity2022.3 GetSampleChildrenCountRecursive](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Profiling.RawFrameDataView.GetSampleChildrenCountRecursive.html)，没有借时间重叠猜父子归属。
