# 第二批容量封口证据报告

Task NTSD-OPTIMIZATION-BATCH02-CAPACITY-SEAL-20261006；Change NTSD-OPT-H11-CAPACITY-SEAL-002 / RUNTIME_PENDING。
本子批容量封口已写入，原Editor编译通过，新16项与旧30项全部通过；父H-11未关闭。
pre-change已确认原Editor TCP6402/Menu clean/idle/nonPlay/errorCS0；11个精确文件已备份SHA匹配。
之前只读范围查询两次错误路径（Frame实际在Q06容器文件）及一次
PowerShell范围数组类型失败不涉及写入；改用既有外部public消费，不读Q06活跃body。
本批不测GPU/设备、不改33ms/规则或segment；父H-11完整0GC尚未闭合。

## Test-first与实际编译

- 原状[RED](red-result.json)：16执行/16预期失败，14个封口API缺失，2个缺少提前容量拒绝。
  test树total9044不是实际运行数；原件保留。
- 四生产文件补完后，原Editor Assembly-CSharp/Editor编译时间19:03:08/09，idle/errorCS0。
- [GREEN](green-result.json)：新16/16实际通过，0 skipped/failed。
- [回归请求](regression-start.json)/[实际结果](regression-result.json)：旧预热7、mesh/bounds8、
  LatestFrame13、两具名motion正例，共30/30通过、0 skipped/failed；只EditMode，不进入Play。

实现：每slot entity/hit/command/motion count在CopyFrom前预检；mesh command逻辑限在
mutation/geometry前预检；motion prior/current slot在generation/lookup写入前预检。
生产超限用常量原因复用既有fail-closed，整份新submission拒绝，旧有效提交/read lease保留；
无last-good则无central提交，仍压制Legacy，不输出部分帧。
封口后PrepareCapacity禁止，原EndBattleCapacitySeal解除，缓存仍归原owner且不清空消费中数据。
直接误用API的异常检查不是生产0GC正例；CPU lease与GPU完成证明不等价，未修改GPU生命周期。
局部varying alpha 64次DisplayMotion.Prepare（含本实例预检）测得0B；public count恶意边界
用既有私有setter制造，仅本地fixture。未读/写Q06活跃body，不新增GPU API或改变pixel/segment。

## 验证范围与剩余门

本次是原Unity Editor的编译和46项具名EditMode证据，不是Play、全BattleRuntimeSelfCheck、
正式EXE同输入trace或Android认证。Unity CLI技能要求复用原Editor；已有连接完成请求，
未安装/升级工具或启动第二Editor。2d-pixel-perfect技能使本批先核URP并保持既有采样合同；
无相机、过滤、插值时刻、fixedDeltaTime或shader修改。

生产接受路径增加prior/current motion槽位预检；CPU成本尚未测量，不宣称吞吐收益。
MaterializeCommands内部其它缓存、native/GPU Mesh存储、完整copy/物化/上传/录制/提交0GC，
真实Battle enter/exit/re-enter和1000 AI/设备性能仍待闭合；不可由局部0B推出全路径0GC。
原EndBattleCapacitySeal解除仅修改准入元数据，不清空在途消费者的数据；GPU完成/fence未实施或测量。
EXT-1仍PROPOSED / MODIFY_REQUIRED，无专项M0；MONO/ATLAS资源格式与预算等独立门保留。

原状16项预期失败、修后16/16、旧30/30原件均保留，不抹除失败证据。
Ledger校验、最终静态检查和Editor状态各存同目录JSON；文件备份/前后SHA见
[Operation](../../../docs/ai/FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH02-CAPACITY-SEAL-20261006/RECORD.md)。

2026-10-06 19:11静态收尾：[static-validation.json](static-validation.json)记录11/11备份与
8/8保护SHA不变、34唯一条目12高/14中/8低、94个本地链接无缺失、scoped diff-check exit0。
[editor-post.json](editor-post.json)记录原Menu isDirty=false、nonPlay、idle与error CS0；
[compile-state.json](compile-state.json)保留编译后的Editor与既有URP信息。
最终[change-ledger-validation.json](change-ledger-validation.json)保留实际validator结果，
完整文件前后清单存Operation after.json；两份无关JSONL与第一批原件仍保留。
