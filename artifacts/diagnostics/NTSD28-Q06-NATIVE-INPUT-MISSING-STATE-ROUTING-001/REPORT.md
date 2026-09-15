# Q06 Native input optional-state reader

IN_PROGRESS / FOCUSED_PASS_RUNTIME_PENDING。

当前正式EXE SHA B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033；source manifest 07CD47A0623F23D2C439E0E85EABF2ED10F8EAE8FC7D70DDB8396C704B3D778F。源模型构建没有修改发行目录，不等同于正式EXE观察。

源调用链为InputRouter28::step_sampled → route_type0_builtins：当前frame及action215后重读state采用FieldBag::integer(...).value_or(-1)，且非null frame在分支之前递减run accumulator。源FieldBag严格完整Int32解析；缺失、bad、+0、1.5和溢出不是声明state0。raw capture和其它pass的默认state0不变。

## Evidence

- source/first.jsonl与repeat.jsonl：432例、各1522298 bytes、SHA7178ec5e534bc710575838e2a610d477c23b46228c30b9cc5dbdc22965d20898。源定向3552检查通过，实际边界在validation-summary.json；不是所有输入分支独立证明。
- Unity RED job3c5a4eda8c764ec8b6781962e28af954：两profile各432 before0/after1193，production-red保留完整JSON/XML。
- 两生产文件修复后job5cec09d0c7c540d0be3c74c154ba7509联合28项27PASS/1FAIL：432两profile全部0差异，父CPoint392立即与完整下一tick四项全部PASS，旧Ground及其它AirDash通过。
- 剩余失败为旧AirDash unhandled state3计数17保持断言，与source任何非null frame公共前置衰减冲突。仅改该断言为16并保留返回false、非type0调用及4096循环零分配检查。jobefd5176c830f45e28536f776a1101d36本类10/10 PASS，airdash-10-pass.xml。
- 首次编译漏EditorTools namespace导致CS0246和一次0-test作业；已纠正并明确不作为通过证据。最终测试前编译CS0。

## Change and remaining gates

BCAW只在native输入读取state时使用rawProperties与严格decoder；旧typed frame入口保持。Ground已消费前置则不会进入fallback；AirDash fallback在无效state早退前消费公共计数，null frame不消费。LF2Entity原生unchecked输入写入绑定Native descriptor，保留raw写号/latch/counter规则。

本矩阵单实体type0、attack开关、相同current/previous、非零run accumulator；包含高帧、未声明/非法frame/state、action110/215/182/188，但没有方向/Jump/Defend触发的全部交叉前置，也不证明double-tap递归或全部type输入分派。不得扩大为完整B2/B6/Q06对齐。

新SelfCheck、父两factory真实Play、同World回放与有序关闭仍待完成；父原28隐式99+attack差异已在完整下一tick对照清零。Q07未迁移，非战斗/GAS/Mono/Scene/资源/Server未改。

## 最新限定出口
VERIFIED / DECLARED_OPTIONAL_INPUT_STATE_AND_DESCRIPTOR。Fresh SelfCheck18:13:58Z PASS；真实Play18:15:16Z投掷/后继tick1568+输入864全部PASS，Renderer2→2、Scene checksum保持；关闭18:16:07Z恢复4→4、World/slots/logic/render全0、两帧Stopped。before-throw本地回放112场景224重放tick已PASS。最终Scene dirtyfalse/root14/文件SHA不变，Editor idle/notPlaying。先前失败保留，Q06与总目标未关闭。下一准确Task为NATIVE-INPUT-ACTION-COST-FRAME-READERS-001。
