# Q05 BMP/stats数据来源限定交付

状态 FOCUSED_TEST_PASS / VERIFIED_BMP_STATS_SOURCE_ONLY。Q05、父metadata Task与总目标继续ACTIVE/FULL_ALIGNMENT_INCOMPLETE，正式资源未部署。

新版逐行路由现在同时处理BMP、全局inline stats、既有frame与weapon_strength，保留literal key、声明顺序、last值、bare movement字段及重复BMP合并；stats只读取同行结束标记前内容，保持原context。BMP动作列表按原版formatted int行语法、计数和关闭条件验收，包含正号/相邻负整数/最后溢出和EOF行为。sprite声明范围及同一行尺寸取值由同一路径投影，不跨物理行借尺寸。

Lf2DatFile.LoganStats保存全局合并字段；LF2CharacterData.NativeMetadata在实际Logan manager加载时绑定不可变Bmp/Stats集合。准确binary64/optional/原始非空stats已到达真实330 catalog数据模型。Native兼容速度槽使用准确值转float，缺省0，walking/running rate缺省1；精确数值保留在NativeMetadata，Q06尚需迁移运动读取者。weapon参数仅BMP来源，input参数采用literal/strict int与merged stats；旧Parse/旧source保持原行为。

BMP shadow/bound、stats.max_mp等原始值和有效性已承载，不代表Q06/Q09消费者已接通。stats.y平台取景、普通HUD、角色选择及其他批准例外保持。Armor及weapon_piece仍后继；不能把本包视为全DAT语法或完整definition闭合。

## 验证

- 原版28语法夹具capture双跑一致；正式405 DAT薄投影来自同SHA的native完整capture。10个原版失败输入（7夹具+3非游戏文档）检查拒绝；有效语料检查BMP/stats原始有序字段、全部动作序列与sheet声明；330实际catalog构建后Metadata/raw/来源缺省及已映射参数核验。
- RED434：431失败/3通过。首轮1362：1360通过/2失败，原始证据保留。诊断工具用-1代替null造成两例错误预期，原版optional helper对5个缺省/无效尺寸补valid=false证据后只修正薄投影，不更改原始capture。再次运行发现缓存旧oracle与旧Q02全405无条件成功测试，分别修复test cache及独立test-only Record（见下）。
- 最终462/462通过，job6486f670f52f43229f83ab465f425ca3；和首轮928个frame/strength/typed通过共1390不同测试。test-evidence-union.json按实际XML fullname去重。旧sprite测试仍访问405、完整比较402有效定义并断言3正确拒绝。
- dotnet build Tools/NTSD28ContentAudit/UnityContentCapture.csproj --nologo -v quiet：0warning/0error。Unity实际编译重载并运行测试，最终error CS查询0。
- 完整SelfCheck请求2026-09-13T08:08:22.0905858Z，结果2026-09-13T08:09:08Z为PASS；mtime晚于请求，旧结果另存NOT-CURRENT。自检执行时bridge一次查询超时，完成后重查成功，不把超时当测试失败或沿用旧结果。
- dotnet run ... --input-root Assets/NTSD/Config --input-mode unity --output Temp/NTSD28ContentAudit/unity/bmp-stats-after.jsonl：旧138全部解析转换成功，与前typedframe轮排除source身份后完全相同。旧投影不是本包所有新增字段的identity证明。
- 63个fixture/optional原版源头身份0漂移。保护3059：3025相同/34已存在或已声明差异/0missing；较前包仅新增LF2CharacterData准确声明差异。Scene SHA a96e11064f1bd054d9d5547fe8f02c702d971b3972d55754886d75b2dce9d28f保持，isDirty=false/root14；旧UI精度差异来源仍pending，不报告与HEAD相同。
- 无新Play、正式资源迁移、schema发布、非战斗逻辑修改或Unity/GAS重构。精确九脚本加source-linked csproj；test-only修正独立记录。最终ledger结果回填Record。

## 后续出口

下一唯一Task NTSD28-Q05-NATIVE-ARMOR-WEAPON-PIECE-SOURCE-INTEGRATION-001（先准确Record），继续父metadata AST/source闭合。不得重做已交付BMP/stats/numeric/frame/strength/PNG。护甲沿用现模型，核验literal/strict/list/结束边界；武器碎片需要原版root/group/variant及amount/piece_end归属和实际definition载体。

全部metadata/raw/profile/新字段随后进入同一Q05内容identity和联合版本窗口；Q06运动、PP/HP与碎片消费、Q09非例外shadow/bound、Q10音频与整场Play按依赖回访。现16速度兼容float仍不能证明原版double运行结果。stage.dat USER_HOLD、音频/其他图片部署边界、所有用户例外、33ms/3ms、十一阶段关闭合同保持。

最终变更账本验证：PASSED，487 Records / 98 governed code files。
