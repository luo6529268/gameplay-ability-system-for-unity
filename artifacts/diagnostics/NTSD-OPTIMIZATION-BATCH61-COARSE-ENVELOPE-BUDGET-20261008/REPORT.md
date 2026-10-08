# 第61批：包络候选 CPU 数组预算检查

结论：SCOPED_OWNER_CAPACITY_PASS / BYTE_BUDGET_UNKNOWN / NOT_ADMITTED。完成到计数器可靠性边界，不将 UNKNOWN 冒充字节预算通过，不重复已失效 API；H07/H11仍OPEN，阶段4/6、22—61共40已执行，Goal active。

## 本次实际证据

- 初始两新检查实际RED：XML2/2缺probe、0skip，5313B/SHA CD058DB01012EC5153E8F3DCD180290FA828EDCA1BAAC2428F996835CF7E509D。job426d365fe1d64a72a1e0a386fa9fc7a0管理器报init timeout/result null，但原callback XML有完整start/end与两case，不因job错误抹掉已执行事实；未重启/重发RED。
- 第一次预算/旧回归job728a0db4296a40859855b11c3d690f93实际4/5、0skip，已知存活4096B正例计数为0。empty控制先通过；因此没有进入32/1050/2048数组计量。失败XML8748B/SHA14920BC2A3EF53F9E80BC7558A78794B1EA96A37B5F32F5A441952259F2D9F6C与完整当时源E24212CD...F541均保存，不能用该0推断无分配。
- 此结论仅针对原Editor本次Test执行上下文的CurrentThread字节API，具体失效原因UNKNOWN，不推及其它平台，也不撤回60独立GC.Alloc recorder前后正例校准/零事件证据；不同工具/scope不能相互代替。
- 新单owner检查在该次实际PASS；owner-budget-82d12ac16858402a8bbacb94aabd2e0c.json/SHA149346F92773CE6BC03A1C5DDE5422DBCFE0BE25D65BFBDCE5BC3E90C8732007。
- 停止采用未通过的自动计量断言/未执行helper，改本批冷计数gate：空控制非0或正控制不足4096即COUNTER_UNAVAILABLE；可靠控制也仅CALIBRATED_MEASUREMENT_REQUIRED，stride/steady/transition值仍null、不认证budget。
- 最后jobf2694ee984b34937af44523e00cf9314实际guard3＋原默认/request3共6/6、0skip、2.7679717s；6772B原XML/SHA0D9BCDABF737609C33D97B588EE09AE2643CAC5BE3FE7F45E2E4B5463E47CBFC及freshcopy同。此轮不再调用counter，不复测owner。不是7个新测量通过，不是9789全树测试，也不是预算/FPS/0GC证书。

## 稳态 / 过渡记账

| 项目 | 结果 / 限制 |
|---|---|
| owner | 当前实际Query一处participant buffer owner，buffer一处items数组 |
| 准备容量 | 初始128，经原EnsureCapacity(1050)得到1050records；直接冷owner路径，非Battle实景readback |
| 复用 | ON/OFF、相同capacity再次Ensure、1000条构建后缩到900条，items仍同数组；默认flag最终false |
| steady | 记1050records对应当前数组；stride/header/bytes UNKNOWN，不以字段payload17B推总驻留 |
| transition | 至少记录本次resize旧128＋新1050=1178records的共存模型；非进程峰值实测，释放后旧数组仍可能等GC回收 |
| 其它所有权 | 此owner无GPU/lease/staging；不等于整个renderer/World没有这些成本。Entity/Frame引用目标不重复作为该数组私有内容记账；全World/其它cache/GC堆/OS驻留不在此测量中 |
| 历史增量 | 旧编译类型没有实测，旧stride与新旧delta UNKNOWN；未造mirror |

同组60原report runtimeSlotCapacity1050/pressure0按保护SHA复用；本冷owner测试没有sealed战斗域、未宣称新0GC或全热路径容量证明。生产Buffer本身EnsureCapacity仍可增长，本批只验证已准备数量内复用，不修改其seal/failclosed规则。ATLAS bank/设备预算/资源格式、renderer GPU预算未改。

## 实际改动 / 保护

唯一C#为既有BattleBruteProductionAdmissionEditorTests.cs，相对准确dirty副本最终128增/0删，SHA05547FD2284635CD28D3FE6028798AACB4B971CD7448505DBB33DD70BFC4343F。所有原方法/断言保持，新增只冷gate和owner检查；试验数组helper未采用的完整版本已保留，非用户内容回退/文件删除。
UTC2026-10-07T23:36:29.2032487Z：101guards/八事前copy/HEAD45bbed41c64e0599601fa0df4028072d9f83303a同；runtime Query/Buffer/Suite、60报告、Q06 SHA及Scene资源保护同。公共最终XML保持。
未Play/Profiler/FrameDebugger/GPU/M0/新实景/正式1800/全SelfCheck/native trace；仅原PID19040/6402 EditMode，不重启或第二Editor，无Scene/Prefab/资源/settings/Gen/Plugins/Server写入或破坏性Git。技能unity-cli用于原Editor安全接入，无Unity6 Pipeline安装。
工具限制保留：外部EXE/Server git status报outside repository，外部Git状态UNKNOWN但SHA成功；一次rg旗标置于--后错误后按正确参数读取；首次patch重复INDEX操作被验证拒绝后零写入，改每路径单操作；reload短暂TCP拒绝只观察原handle，管理器init timeout与XML实际执行事实分开。git diff --no-index exit1是有差异，不是构建失败。

## 后继

60 performance仍FAIL：ON logic P95126.166/123.513ms、drop712/702、collector平均43.172/45.820ms；约4.16/4.21显示间隔倒数只是旧60数据，没有本批FPS收益。H11旧camera严格FAIL不被本冷检查或60 logic0覆盖。
下一[62既有候选组合Driver资格](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH62-ENVELOPE-BINDING-DRIVER-20261008.md)仅READY：60包络拒绝调用PreserveBruteRejectedBinding，48已有复用入口正接在该处，需针对新组合核必要Driver逐tick合同，不假设IsBound已被证成主要耗时或必有大收益。仅同普通Brute已批准范围、无新runtime owner/字段；不推广默认或切collector，不重复失效counter/owner/旧cost/60四窗。字节预算未知保持，不因次数或UNKNOWN停止Goal。
最终validator、diff、表行/Editor状态另附，不能把focused pass升级为预算或性能完成。

最终核验UTC2026-10-07T23:39:30.7808403Z：validator实际exit0/PASSED、1357records/20governed/4252历史WARNING/0ERROR；diff-check0（26 CRLF提示）。101guards/八事前copy/HEAD/C#及publicXML同；总表34unique、六项各3处表行/40已执行；原Editorfresh idle/noPlay/noCompile/noTest，Menu单Scene clean8roots。见validation-final-01.json，历史warning未逐条重审。无新的Source/测量修改；62仅READY，Goal active。
methods-and-writes-after-01.json进一步证明所有原Admission方法/断言文本未变，owner方法与实际PASS版本文本相同，八写域after SHA/bytes已记录；不以源码文本同扩大为全项目运行证明。get_goal本次实际返回active，本轮无update_goal/新Goal操作。
