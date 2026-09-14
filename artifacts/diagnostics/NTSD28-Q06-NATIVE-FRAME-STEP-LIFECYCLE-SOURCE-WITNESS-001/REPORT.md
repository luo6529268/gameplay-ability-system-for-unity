# 原函数帧推进/生命周期见证结果

VERIFIED / SOURCE_MODEL_DIAGNOSTIC_ONLY。唯一新增脚本 Tools/NTSD28AuthorityTrace/frame_step_lifecycle_witness.cpp；本轮没有改Unity生产、测试脚本或资源。

正式EXE SHA B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033；构建闭包75-source/header manifest 07CD47A0623F23D2C439E0E85EABF2ED10F8EAE8FC7D70DDB8396C704B3D778F。build-manifest.json含runner/binary SHA，正式EXE及源文件未修改。

## 实际执行

1. Build-AuthoritySourceCapture.ps1 -OutputDirectory Temp/NTSD28Q06FrameStep -RunnerSource Tools/NTSD28AuthorityTrace/frame_step_lifecycle_witness.cpp -ExecutableName frame_step_lifecycle_witness.exe。第一次编译InputButtons28只读operator[]赋值失败；根据input_state.h改用set接口，原失败log保留。最终构建exit0。
2. 运行Temp/NTSD28Q06FrameStep/frame_step_lifecycle_witness.exe，exit0，frame-step.tsv保存2676行输出。每例DAT.ok与合法spawn、单frame event均有runner断言。分组：lookup15、next180、counter24、cost1944、modifiers160、gates36、terminal14计72、type3计240、already212计5。
3. Python subprocess直接再次执行同一EXE，stdout与保存文件逐字节一致，SHA e046a7fc4da329f9679503f979751fcc069c170c4b045fdcde0820d4a72ef783。validation.json保存分组统计和11个重点样例，正负999/Yref、缺失998、声明999、成本相等/MP失败HP继续、encoded reset等事实断言通过。
4. 本脚本仅供给输入并调用原BattleWorld28::step_frame_slot与resolve_pending_lifecycle，没有复制规则算法。没有执行中间完整driver的OPoint、state18 particles、previous078提交和weapon pieces。源码顺序已另读CALLER-MAP.md；原函数两个端点输出不是完整driver或正式EXE可观察场景证书。

## 已测事实及实际差异

详情见CALLER-MAP.md和validation.json。Unity当前自动负成本读取hit_d并以PP与负mp比较、漏负HP事务；原版读取destination.next、HP/MP严格小于cost才fallback，始终保留原destination，先MP后HP、HP支付减max/3、写独立消费统计。负999翻面后action0，正999才可能按YReference进入212。11xx/12xx重置保留action latch并清collision snapshot：普通next1101保留旧latch0，成本fallback1101保留已写latch1101。隐式998存活，声明999/next0仍删除。

新增待处理项：source orphan cpoint-kind2仅抑制type3 drain，当前Unity整个framebody早退；已在212的next212不是advanced，不能再次初始化跳跃速度；独立sound_action_latch和完整列表/成本前两个采样点在Unity尚未定位到等价载体；生命周期与previous078/particle排序需完整driver补证。均为未解决差异，未标为Unity已对齐。

## 状态和下一步

本源码见证任务限定完成，父reader迁移和Q06仍IN_PROGRESS。下一唯一Task NTSD28-Q06-NATIVE-FRAME-TRANSACTION-INTEGRATION-001 / READY_FOR_EXACT_PRECHANGE_RECORD。先按CALLER-MAP把数据前置（特别sound latch/快照）和frame/direct/cost/lifecycle划定准确Record，再Unity同输入RED与成组实现；不能批量替换857或把未修成本/生命周期当作可跳过部分。2676行是后继Unity对照输入/预期，不是2676项Unity测试。

前快照子包的244/SelfCheck/Play是历史已验成果，本轮未重跑Unity编译/SelfCheck/Play；不得包装成新生产验证。全部三脚本hash与用户Scene bcd1047b…保留，详见protection.json。禁止computer-use，未启动另一Unity Editor，未改非战斗/框架/正式资源，未执行Git提交/推送/删除。最终ChangeLedger结果见ledger-final.txt。
