# Q05 护甲与武器碎片数据来源限定交付

状态 FOCUSED_TEST_PASS / VERIFIED_ARMOR_PIECE_SOURCE_ONLY。Q05步骤1的native内容模型/来源门槛已闭合到声明范围；Q05整体、身份/版本、实际新规则消费和总目标仍ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

## 实际实现

同一物理行router新增LoganDefinitionBlockReader，直接产生LoganArmors和LoganWeaponPiece结构。护甲当前pointer与major context独立，opening/closing行及nested覆盖行为遵循原版；piece的root/group/variant原始字段顺序、同piece归组、amount归属、piece_end、至多5组/每组5variant及关闭/重复错误与原版parseSuccess相同。原版接受的隐式variant写法保留；无效piece数字不会被擅自变成piece零。

护甲沿用LF2ArmorData，native source专用decoder改为literal/strict last int，无效type回退有效ptype，列表按formatted stream读取前1/2整数，保留重复顺序与optional sound。旧共享入口默认false以及原双参数LoganDefinitionMetadata构造入口保持。

NativeMetadata现在有不可变Armors raw fieldsets和WeaponPiece。整块/组/变体字段均复用既有LoganDefinitionFieldSet提供预解码值与有效性，manager在实际Logan候选构建时深复制解析数据；原AST后续清空不影响运行定义。列表不可修改，group amount不会混进variant。没有新runtime manager/队列或shutdown阶段。

## 验证与限制

- 正式权威EXE、dat_parser/combat_records/battle_world等源码及build.ps1参与性与hash已冻结；完整source-linked原版capture保存39语法夹具双跑相同、405语料结构。fixture14拒绝、正式3非游戏文档拒绝；其余完整比较有序raw、opening/closing、group/variant结构和normalized armor。18正式armor保持匹配，三indexed piece的全部3组/4variant结构进入实际330 catalog metadata。
- RED446：443FAIL/3PASS，原始XML已保存。生产后首轮1829：1817PASS/12FAIL，其中1是原双参数反射构造入口被optional扩展签名取代，11是Unity重启后Temp native catalog probe缺失。修复原构造入口，使用既有Build-NativeCatalogProbe.ps1重新构建探针（330/330、源/头身份稳定），不修改测试期待或正式源码。
- 最终job6dc23afb7dbc414f97a5044c6b7a985f：467/467 PASS（本包447+fieldset4+catalog16）。与前轮其余通过用例按XML fullname去重共1829不同测试通过；BMP/stats434、frame AST444、strength45、typed439均保留本轮通过证据。447包含39夹具+405文档+2非法catalog拒绝+1嵌套不可变隔离。
- dotnet build Tools/NTSD28ContentAudit/UnityContentCapture.csproj --nologo -v quiet：0warning/0error；现有Unity实际编译重载/测试通过，最终error CS为0。
- 完整SelfCheck请求2026-09-13T08:31:58.3854131Z，结果08:32:42Z PASS，mtime晚于请求。没有新增整场Play/碎片生成/护甲交互行为的对照证明。
- 旧138全部解析与转换成功。Unity重启清空Temp后上轮完整输出不在，但先前已冻结完整输出SHA。将本轮输出仅linkedProductionSourceSha256恢复为事前准确hash并移除本轮新增source项，完整字节SHA精确等于上轮05ddfa8900b777cfc12aee17ac9208a462ad8b110cdea4b61f4d05a3d22b5890；证明差异仅源码身份。重建字节不冒充旧实际运行，本轮normalized全文另存artifact，便于后续不依赖Temp。
- 405正式DAT及39fixture/原版源等无内容漂移；临时AuthorityContentCapture.exe因Unity重启清理而缺失，原始capture/fixture身份及工具源码证据仍保存。这与正式EXE缺失或规则漂移不同。
- 保护3059：3007相同/34既有或已声明差异/18缺失。18是任务外旧Foot图片与meta删除，同时出现blue/red/yellow新目录；本包没有资源删除/移动/替换操作，保留现状，不能报告全资源未变。Scene SHA仍a96e11064f1bd054d9d5547fe8f02c702d971b3972d55754886d75b2dce9d28f，isDirty=false/root14；旧精度差异来源pending保持。
- 用户明确禁止computer-use，后续所有检测只使用桥接/日志/结果/进程。启动无界面test时新的NTSD普通实例出现，Unity拒绝第二实例，CLI COMMAND_FAILED无测试结论；未删锁/未重复启动，改接当前58092实例验证。

## 下一执行入口

NTSD28-Q05-RETIRED-CARRIER-AND-2F8-MIGRATION-001，按父Q05步骤2继续。先已有冻结矩阵与115候选inventory收敛准确Record，然后清理已退休carrier（含reset/copy/ECS/hash/shell/diagnostic），建立独立+2F8 int32/-1载体。保留有效Spawner/Owner/TrackerParent等合同，不重做Q04行为退休，不提前接Q06新producer。

随后父步骤3完成raw/semantic identity和native candidate/cache/publication身份、双OPoint队列capture guard；步骤4同窗口entity13/aggregate21/checksum24/character2/base2，当前仍12/20/23/1/1。步骤5再旧版本拒绝/新capture-restore-replay/pool/Play。R13/R15须等相应carrier/schema证据，R08碎片和R09护甲须等Q06/Q07等真实触发；本包不提前关闭这些回访。

本批只证明数据来源与构建；Q06运动/PP/HP/碎片随机与生命周期、Q09非例外表现、Q10音频及Q07正式DAT/图片部署仍待。Unity/GAS、非战斗、33ms/3ms、十一阶段、stage.dat USER_HOLD和全部批准例外保持。所有新增raw/presence/ordered armor/piece与frame字段列入后续semantic identity，不能仅哈希旧字段。

最终Tools/Validate-ChangeLedger.ps1 -RepositoryRoot $PWD.Path：PASSED，488 Records / 102 governed code files，ledger-final.txt已保存。
