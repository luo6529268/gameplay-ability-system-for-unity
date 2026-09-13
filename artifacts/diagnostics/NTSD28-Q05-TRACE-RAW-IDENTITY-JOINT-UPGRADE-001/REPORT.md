# Q05 Trace / Raw / 内容身份升级限定出口

FOCUSED_TEST_PASS / SAME_CONTENT_CAPTURE_PASS / RAW_PARITY_DIFFERENT / Q05_REPLAY_PENDING。准确19脚本，最终SHA见final-code-scope.json。canonical trace/descriptor/comparison/validation/selftest已v3；Unity raw header/tick/comparison/selftest、authority source wrapper/validation已v2；native build manifest2.0。实体50字段、44已绑定、原6MISSING保持。13/21/24/2/2仍在同一未发布Q05窗口；不代表整场对齐或Q07资源已部署。

## 实际改动

- 新增combat.objectAiExcludedGroupSourceSlot，native与Unity均读各自真实独立2F8，未借用owner/spawner。生产2F8写入/AI消费仍属Q06。
- 内容头绑定policy/scope/profile/rawDefinitionSha256/decodeContract/semanticSha256/catalogFingerprint64/schemas；采用既有V2 byte contract。只声明catalog对象DAT定义的身份，不扩大到所有图像/音频/背景。原版与Unity自身源码/assembly SHA作为分别的来源证据。
- Unity Editor捕获可选实际Logan root，使用已有catalog/完整native configs；默认旧内容入口有独立legacy profile和实际文件hash，退休旧常量manifest。临时scope保存/恢复config、objects、registry、frame configs与publication身份，并在构造后半途失败时恢复。正式场景/资源/global入口未切换。
- 原版runner用系统BCrypt对实际catalog/DAT读到的bytes计算相同BinaryWriter/SHA身份，初始化前后/模拟后复核。hash对象buffer寿命覆盖BCryptDestroyHash。源树只读，候选工具仅建于Temp，不覆盖正式EXE。
- 内容不匹配、伪语义摘要/投影、旧schema在字段比较前拒绝；canonical CLI不再把content-strategy-pending当成功。Raw FirstDifference现在按最早tick、slot稳定选择，保留字段顺序作为tie；各字段差异统计仍完整。
- 实际同内容捕获揭示旧Editor bootstrap把scenario当前MP传给Initialize(maxHp,maxMp)的错误。仅Logan诊断路径按native battle_world.cpp current_mp=request.mp、base_max_mp=stats.max_mp/fallback映射，既有生产Initialize/资源规则和默认legacy诊断保持。没有用修改输出字段来掩盖差异。

## 新鲜验证

1. Tools初始25项中4个新RED失败；集成时Program旧状态consumer编译遗漏、sealed异常继承尝试均已纠正，源码/Record留痕。新50字段补齐synthetic fixture；共享contract异常转为invalid结果，未放宽输入。
2. 最终.NET工具共88/88：trace51、raw14、B0-domain12、B0 comparator6、B2 input/RNG5。包括旧tag、缺key、错raw/tag/SHA/LE/schema/type/profile、不同合法内容、最早tick反例、missing2F8拒绝。B0/B2独立schema保持。
3. Unity初始4 RED全失败；首轮46=45PASS/1旧C25版本断言已按当前13/21/24修正。MP bootstrap新增RED证明200误当500；最终49/49（含真实Logan root、恢复原配置引用）与独立构造失败恢复1/1，共50个不同测试通过。validation-counts.json准确记录49+1，不冒称单轮50。
4. 真实当前正式Logan输入：source runner3tick/6entity、Unity同一neutral-common-two-entity（seed682973786、stage23、OID2/7、无按键）。两端完整content对象相同：raw4EFE1D2A6A51C20742EA839CC5EAC2BA0D09EE9E4A5888E77C8AC35D4AA0C58C；semanticDB579550BCEC0039383BB421B0F62FB9C741FA2BFD30212059F329B8CADA4407；projection3900ECBC509557DB。native复跑字节级相同，实际header/源码/binary SHA已归档。
5. 独立native binding unit包含当前实际write_entity，输出2F8=-1/0/37且owner19，和Unity非默认测试共同证明字段接线。该unit不是游戏运行或正式EXE trace。
6. 实际旧Unity capture与正式native不同内容：raw CLI exit1/reason content-identity-mismatch/ticks0。旧authority v1负夹具exit1拒绝；由现有selftest实际builder导出的synthetic canonical不同内容也CLI exit1/ticks0，详见canonical-cli-exit-evidence，非运行时证据。
7. 完整SelfCheck：请求2026-09-13T13:34:49.1829552Z、结果2026-09-13T13:35:48.861458Z PASS。Unity最后error CS查询0；native/工具均真实编译。
8. 现有旧Unity内容的真实NTSD_Battle Play：tick5、对象4→4，两OPoint队列pending拒绝/不消费、空闲capture成功，正常退出；无dedicated worker，非正式330资源的完整Play或物理技能验收。退出后bridge状态文件有一次写入中的空JSON读取失败，重读同Editor成功，无重启/第二实例。
9. Scene isDirty=false/root14，旧SHA a96e11064f1bd054d9d5547fe8f02c702d971b3972d55754886d75b2dce9d28f保持。3059保护项2930同/111既有或声明变化/18旧缺失，无新增缺失。任务外用户performance roadmap未触碰；用户在工作中自行提交至ac09a622，本任务未提交/推送。最终脚本scope由preimages/SHA核对，不能用当前git diff数量代替完整scope。

## 必须保留的实际差异

same-content-comparison.json为different，不是parity PASS：300字段出现38处差异、7类差异、43类一致。严格最早首差tick1/slot0 combat.runtimeStateCode是MISSING；另5个原MISSING同样保留。已绑定的最早数值首差为tick3/slots0与1 vitals.currentMp，native200/Unity201。最大MP初始错位已在诊断入口修正，不再归因于正式内容不同；剩余MP增量的具体规则/时点原因尚待Q06追踪实际consumer，不在本包修改生产战斗逻辑。初始8类差异与旧排序结果均另存，不覆盖旧事实。

## 下一出口

同窗口步骤4的trace/raw身份与字段工具现已可用；下一NTSD28-Q05-JOINT-SNAPSHOT-RESTORE-REPLAY-VALIDATION-001，使用当前内容身份/版本完成有意义的capture→修改→restore→同seed/input重放、claimed/raw/slot/pool复用与关闭重入验证，再关闭Q05。不得以本次3tick raw diagnostic、6MISSING或单场Play声明总目标完成，也不得提前Q07。Q06继承上述MP差异和所有已有consumer待办。

命令使用dotnet run --project Tools/NTSD28Parity各self-test/validate/compare、Build-AuthoritySourceCapture.ps1（正常runner和独立unit）、已归档native-run-command.json、Python -X utf8 Temp/Goal13_bridge.py的Unity tests/Play/Console，以及现有SelfCheck request/result。全部未使用computer-use，未改非战斗、Unity/GAS架构、Scene/InputActions、正式资源、Gen/Plugins或外部Server。

最终审计：Change Ledger PASS，503 Records/当前0脚本diff（用户已自行提交脚本）；19脚本最终SHA及实际exporter源码SHA复核一致，diff --check通过。
