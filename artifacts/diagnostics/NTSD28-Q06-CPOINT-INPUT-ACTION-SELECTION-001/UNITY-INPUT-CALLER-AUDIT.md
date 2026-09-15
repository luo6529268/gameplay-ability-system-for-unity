# CPoint input selection caller audit

IN_PROGRESS / READ_ONLY。真实source advance_catch_relations()5751起，选择约5870..5945。Unity RunCharacterCpointStep10→Match.CpointWriter.RunKind1→RunActionSelection→ApplyAction→ApplySignedCpointFrame→SetFrameTickDirect。现代码只A/T/J并逐次Apply，缺D/F/B/Uz/Dz；BattleCatchPointValue和formal adapter已有全部字段，不能误报parser尚未支持。

精确输入映射（NTSD28NativeComboStateMachine）：Current/Previous为Up0,Down1,Left2,Right3,Attack4,Jump5,Defend6；EdgeWindow为Attack0,Jump1,Defend2,Right3,Left4,Up5,Down6。旧KeyJump=CurrentAttack、KeyDefend=CurrentJump，CdAttack/CdJump来自对应edge。名称不定义按键语义。

源码selector顺序：A（current attack且edge>0，且无当前水平或T==0）、T（current attack/edge且任一当前方向并T!=0）、D（current defend/edge）、Uz/Dz（previous depth及对应edge）、F/B（previous水平，facing为true先rightF再leftB，否则先leftF再rightB）、J（current jump/edge）。每次赋值包含0；最后requested非0才执行一次native_relation_action。后匹配0会取消先前非0，不得各分支自行跳过0或逐项应用。

本分支active_window仅>0，不使用其它输入helper的128阈值。负requested翻向并取负值；捕获范围需区分raw999与输入999归零语义，不能复用NormalizeInputAction。selected native frame无/无cpoint取victimAction0，否则取selected Cpoint.Vaction；清双方counter，保留latch/previous/snapshot等除后续正式pass明确写入。

RunKind1随后用原catcherFrame/CPoint处理throw及dircontrol；本包source主向量throwvx0隔离选择，不重做已验throw392，仍记录完整后继tick。旧setter/public接口的其它caller保持；源证据之后才限定修RunActionSelection/ApplyAction。
