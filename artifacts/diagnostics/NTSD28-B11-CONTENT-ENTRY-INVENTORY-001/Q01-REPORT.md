# Q01 正式内容接入清单与兼容性审计

总目标：`NTSD28-UNITY-BATTLE-REALIGNMENT-001`；Task/Change：`NTSD28-B11-CONTENT-ENTRY-INVENTORY-001`。
日期：2026-09-13。证据类别：离线源码模型和资源静态审计。Q01 出口不代表 B11 全部完成，不代表资源已迁移或战斗已对齐。

## 1. 实际清单

| 项目 | 当前 NTSD 2.8-Logan | 迁移前 Unity |
|---|---:|---:|
| registry 对象记录 | 330 | 137 |
| registry 背景记录 | 24 | 0 |
| 本次全目录 DAT 捕获 | 405 | 138 |
| indexed 对象声明涉及的独立图片 | 1,010 PNG | 383 BMP |
| indexed 对象图片引用记录（含不同角色/用途重复） | 2,005 | 496 |

正式 data.txt 与 catalog.csv 的 section/ID/type/source path 完全对账。按 section/ID 对应，125 个对象可与旧版比较，205 个为新增正式对象，12 个仅在旧 Unity registry 中存在；另有 24 个背景记录。全部 registry DAT 路径存在。对象 ID 对应不是角色意义、技能或可达性相同的证明。完整catalog元数据另存catalog-full-review.csv，hidden/random/classification等尚非runtime可达性结论。

全 runtime 目录另有 1,255 PNG；不能把其中所有图片都归入用户批准的角色图片替换范围。辅助 DAT/背景图片声明另列 242 条 owner-review 记录。上述清单只覆盖可静态导出的引用，不是完整动态 reachability 证书。

菜单面板另有98份原生DAT、405个layer图片引用，已单列`menu-face-image-references-review.csv`为NON_BATTLE_MENU_SELECTION_OWNER_REVIEW，不混入上述角色/战斗图片计数。Unity真实ParserV2 AST未保留这些layer，明确UNAVAILABLE_IN_PRODUCTION_AST；没有用自写原文parser伪造支持，也没有将菜单面板纳入自动迁移范围。

旧资源处置清单共 695 项：138 DAT、383 张被旧 indexed 内容引用的图片、174 张未进入已证明替换集合的图片。**全部保持 deleteAuthorized=false**。后者含地图/UI等保留内容；前者也必须等待新内容可用和引用复核后才决定旧文件处置。

证据：`report/object-mapping.csv`、`indexed-image-references.csv`、`native-dat-image-manifest.csv`、`old-asset-disposition-review.csv`。例如旧 `Zuozhu/sasuke_0.bmp` 仍被 `NTSD_Battle.unity` 的 inactive editor preview 引用；不能因为游戏运行时未使用就直接删除。动态路径引用不受 GUID 扫描完整覆盖。

## 2. 同一份新版 DAT 的解析与转换结果

- Unity 实际 ParserV2 解析新版 405/405、旧版 138/138；旧版经过生产相同 key 的实际 Decryptor。语法成功不等于 Converter 或 runtime 成功。
- 新版有 **6 个 indexed 对象 DAT、9 个 frame 被真实 Unity Converter 拒绝**；隔离后 9 个 subblock 也明确拒绝，失败投影均为 null。旧 Unity 138 文件没有此次 Converter 拒绝。
- 原生 generic DatParser 捕获 405 文件，402 成功，3 个非 indexed object 格式产生错误：INKHUD、INKHUD2、resource。它们有专门生产消费路径；这里不能将 generic parser 的报错写成正式游戏故障。

| 新版路径 | 拒绝 frame 数 | 本次首个拒绝原因 |
|---|---:|---|
| c/ank/ank.dat | 1 | WPoint effect 不在当前 Unity formal contract |
| c/ank/ssnk.dat | 1 | WPoint 被解析出字段名 7，需复核原文与两端 tokenizer |
| c/hir/hir.dat | 3 | WPoint dircontrol 不在当前 Unity formal contract |
| c/min/min.dat | 1 | CPoint drain 不在当前 Unity formal contract |
| c/min/sag.dat | 1 | CPoint drain 不在当前 Unity formal contract |
| c/nar/nar.dat | 2 | WPoint dircontrol 不在当前 Unity formal contract |

不能为使这些文件通过就盲目扩展 WPoint DTO。正式原生 decoder 对未知 token、重复字段和格式的处理必须逐例核实，然后选择保留原文/忽略/消费的正确策略。

规范化差异共 47 个分组：28 个 DTO 字段缺失、11 个 typed 值差异、6 个 body 原始字段审查、2 个表示形状审查。它们是**接入诊断**，不是 47 个已经证明的战斗行为 bug。CPoint 是原生 27 字段对当前 Unity 19 字段，OPoint 是 24 对 8；缺失字段即使当前默认值为 0 也不伪造成已支持。

注意以下边界：native BDY 导出的是 DatDocument typed field bag，并非 CollisionGeometry 最终 box；因此 body 差异只标 `PARSED_BODY_FIELD_REVIEW`。ITR caughtact/catchingact 的标量与数组/null 差异只标 `REPRESENTATION_SHAPE_REVIEW`，需在 Q03 追到实际 consumer。CPoint throwvz 哨兵、ITR zwidth 默认值及 alias 差异同样必须按消费语义核实，不能以 JSON 数值不同直接宣布战斗不同。

追加 consumer 审阅：原生 `combat_records.cpp:first_integer_or_zero` 取首个整数，Unity `BattleInteractionWriter` 与 `LF2CharacterInteractionResolver` 也取数组首项、缺省0。本次各比较19,438个 catchingact/caughtact 值，23个无法按相同subblock位置配对的ITR保留跳过，实际只剩 `c/rai/rait.dat` frame723/724 的 caughtact `397 vs 232` 两项首差，交Q03追tokenizer；不重做已正确的数组消费。证据 `itr-consumer-projection-review.json`。CPoint throwvx/throwvy/throwvz 的原生合同是float32，Unity目前为int；throwvz原文-842150451在原生float32中为-842150464，属于真实类型语义差异，Q03须同时覆盖小数与哨兵，不能只改一个常量。

同输入 sprite layout 有 **31 文件、55 个字段差异**，主要涉及 declared range 与原生按 row×col 累积的 effective pic range。当前导出来自 parser AST，未运行 Unity 的最终 sprite/catalog publication；Q02 必须沿最终 consumer 确认并测试，不能只把声明数字改得一样。

证据：`report/parser-compatibility-by-file.csv`、`typed-projection-gaps.csv`、`same-input-sprite-layout-differences.csv`，以及两端完整 JSONL。顶层/frame字段、专门 block 和整体 CharacterData 的全部消费语义不在本次 typed 比较覆盖内，保留给 Q03/Q06/Q07 的精确合同与验证。

## 3. 后继最小包及原差异回链

| 顺序 / owner | 具体工作 | 主要现有生产接缝 | 原差异 / 回访 |
|---|---|---|---|
| Q02-A | 冻结 index、decoded DAT、VFS root 的 source 合同；旧默认入口保持；识别 singleton 先载旧 registry 后拒绝 reload 的时点 | GameDataManager.InitializeSingleton/LoadDataFile/ResolveObjectFilePath；CharacterAnimtorManager.ParseCharacterFrameConfigs/ResolveSpritePath/GetDatFileDirectory | D-02、D-06、D-09 / R17 |
| Q02-B | 在既有后台像素解码路径补 PNG，独立验证 RGBA/透明、朝向、格式失败和原 BMP 行为 | Animation/Runtime/BMPLoader.cs；CharacterAnimtorManager 的两个后台加载 caller | D-07 / R17 |
| Q02-C | 实际 sheet pic range、head/small source key 和 catalog/source identity；只接通加载前置，Q07 才切正式内容 | ParserV2 的 file declaration；CharacterAnimtorManager.BuildCharacterDataFromDat/BuildBattleSpriteCatalog；现有 BattleSpriteCatalog | D-04、D-06、D-07 / R17 |
| Q03-A | 从本次 6 文件/9 frame 拒绝和 CPoint27/OPoint24 差异冻结 token/default/alias/presence 与数据契约；逐个区分 decoder 容忍与真实字段消费 | DatParser/Runtime、DataContracts/CatchPoint、ObjectPoint、WeaponPoint；对应 playable decoder/resolver | D-04、D-08 及原 B6/B7 字段行 / R13、R15 |
| Q03-B | 把 body/ITR 表示差异与 +2F8/mass/reserved runtime/shell/ECS/copy/hash 列为联合窗口合同，不现在升版 | 既有 Simulation 字段与 D-022 声明路径，须逐符号审计后写准确 Change code-path | D-05、D-08 / R13、R15 |

以上是 Q01 冻结的后继工作边界，**不是以表格授权整目录修改**。每个源码包开始前还须创建独立 Task/Change 并列实际文件、符号、测试和回滚。当前确切下一步为 Q02-A 的现有 source/cache/caller 合同审计与最小方案，不能直接整体替换 Config/Sprite。Q03 可在无重叠写范围时独立准备。

共用非战斗 caller 已记录在 `scope-and-consumers.md`：菜单角色头像、Editor preview、raw capture、内容 patcher、测试场景等。保持这些功能和既有框架；不能通过全局改默认 root 或热 reload 影响它们。Q02 加载支持先用隔离来源验证，Q07 再按正式 migration contract 切换。

## 4. R15 本次回访

本次满足 R15 的 Q01 身份与字段可用性子条件。正式 EXE SHA-256 为 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`，两端 capture 身份、registry/catalog、完整 manifest 和比较结果哈希在 `report/summary.json` 与 `output-hashes.json`；native 编译源/compiler身份在 `native/build-manifest.json`，Unity 每份捕获保存 26 个链接源 hash 与 stub 边界。

状态只能为 `PARTIAL_RETURN / Q01_IDENTITY_AND_FIELD_AVAILABILITY_RECORDED`。Q05 schema变化后还需旧 snapshot 拒绝与重放，Q07 每次 content fingerprint 变化仍需重建同版本 trace 身份；这些条件尚未发生。本次没有补零、没有混用旧版 checksum，也没有产生正式 EXE 战斗 trace。

## 5. 验证与范围保持

原生捕获器编译成功，真实 parser/decoder fixture 断言通过；两次 405 文件输出 hash 相同，405 输入 DAT 前后 hash 相同，9 个参与编译源的 hash 复核相同。Unity source-linked 诊断工程编译 0 warning/0 error，自检通过；新版/旧版各自双跑稳定。Python 清单工具自检覆盖 registry 注释/重复、capture缺漏、输出隔离、typed缺失零字段、表示差异、PNG元数据与sprite range。

本批保护基线中 3,059 个文件结束检查无变化（`workspace-protection-end.json`）。本批只新增诊断工具/工件并更新治理文档，没有修改 Unity production、非战斗逻辑、DAT、图片、Scene、Prefab 或 ProjectSettings；没有运行 Unity 编译、SelfCheck、Play 或正式 EXE 战斗，不声明这些门槛通过。最终清单重跑与 ChangeLedger 结果以本包 Change Record 的最新追加证据为准。

最终出口：身份gate实际通过405输入、9源文件+31保守header、Unity26生产链接源和每文件input root/hash；Python12项PASS，清单20工件双跑hash完全一致，ChangeLedger462记录/6脚本PASS，diff检查PASS。状态为Q01/BATCH-01 DELIVERED、Change VERIFIED_OFFLINE_AUDIT_ONLY。后继精确Task：`docs/ai/TASKS/NTSD28-B11-SOURCE-ROOT-AND-CACHE-CONTRACT-AUDIT-001.md`，当前PLANNED；无资源或运行时变更，整个对齐总目标保持未完成。
