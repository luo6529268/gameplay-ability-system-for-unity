# R8-WP01G-R08-R04 — production DAT self-check fixture catalog path

> 建立日期：2026-08-24  
> 状态：`VERIFIED / FULL SELF-CHECK PASS`  
> Change ID：`R8-DATFIXTUREPATH-001`  
> Blocker：`B-R8-SELFCHK-DATPATH-01`

## Goal

使`BattleRuntimeSelfCheck`的production DAT夹具通过当前`data.txt`/`GameDataManager`正式catalog定位DAT，移除
R8 type0资源迁移后已经不存在的`AnimationConfig/*`和`FrameConfig/naruto_clone.dat`硬编码测试路径；不改变DAT
内容、生产加载器或任何战斗逻辑。

## Scope

### 允许

1. 只读确认`data.txt`中OID1/2/3/11/33/120/204/205/214的正式当前路径；
2. 在`BattleRuntimeSelfCheck.cs`为`LoadProductionDatWrapper`增加按objectId读取`ObjectDefinition.file`的test helper；
3. 把现有production DAT测试调用改为catalog路径，不手写新Character目录映射；
4. 保留decrypt/parser/`BuildCharacterDataFromDat`与所有字段断言；
5. fresh compile、完整self-check、相关DAT资源测试与ledger validator。

### 禁止

- 不修改data.txt、DAT、BMP、parser、GameDataManager或CharacterAnimtorManager production实现；
- 不增加旧AnimationConfig fallback，不复制或恢复已删除旧目录；
- 不改变movement、Naruto DDJ、sprite range或weapon字段预期；
- 不扩大到角色技能、AI、collision、render、T8、Android、IL2CPP或服务器；
- 不把后续self-check新失败混入本包。

## Authority / Evidence

- `R8-CHARASSET-001`已批准并验证type0统一迁移到`Assets/NTSD/Config/Character/`；
- 当前`data.txt`：OID2 naruto、OID1 sakura、OID3 kakashi、OID11 sasuke、OID33 naruto_clone均位于Character；
  OID120 weapon4与OID204/205/214 specialattack仍保持各自正式路径；
- `CheckMovementDatLoadingContracts`最新失败发生在几何检查之后，错误为旧
  `Assets/NTSD/Config/AnimationConfig/Mingren/naruto.dat`不存在；
- 同文件`CheckNarutoDdjSixCloneProductionChain`还保留Naruto旧AnimationConfig和clone旧FrameConfig路径，需同一
  test fixture path包统一处理，避免下一次逐条失败。

## Files likely involved

- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`；
- 本Change Record、Ledger、STATE、主计划与handoff。

## Unknowns

1. 路径修正后full self-check是否暴露更后的独立失败；若出现立即停止并另记；
2. `ObjectDefinition.file`字段是否始终为project-relative Assets路径；当前data.txt与生产loader显示是，仍由focused断言确认。

## Deliverables / Verification

1. 所有production DAT fixture由objectId→current ObjectDefinition.file解析且文件存在；
2. 旧AnimationConfig/FrameConfig硬编码在self-check中清零；
3. 原movement/DDJ/sprite/weapon字段断言不变并通过；
4. fresh compile0、full self-check实际继续、CharacterAssetDeployment focused PASS、validator PASS；
5. 后续独立first difference如实记录。

## Stop conditions

- 需要修改production catalog/loader或资源文件；
- 同OID存在多个不明确正式路径；
- 字段断言因DAT内容变化失败，而不是路径解析失败；
- 用户改变范围。

## Out of scope

R1-WP02 full trace、T8、AI、render、collision、性能、Android、IL2CPP、服务器。

## Authorization

用户已于2026-08-24明确批准执行`R8-WP01G-R08-R04 / R8-DATFIXTUREPATH-001`并恢复目标。授权只覆盖
本Task中的test helper/callsite修正和既定验证；production catalog、loader、资源与gameplay继续禁止修改。

## Implementation update（2026-08-24）

- 新增`LoadProductionDatWrapper(animatorManager, objectId)`test overload，通过
  `GameDataManager.GetObjectById(objectId).file`和production `ResolveObjectFilePath`取得当前路径；
- Naruto/clone/wind/poison、flash、Naruto/Kakashi/Sakura/Sasuke movement、weapon4共11个production DAT callsite
  均改为objectId catalog overload；
- self-check中的旧AnimationConfig与FrameConfig Naruto clone literal已清零；
- decrypt/parser/private converter和全部行为字段断言保持不变；production与资源0改动；
- 当前`CODE_WRITTEN`，compile/focused/full self-check待执行。

## Verification result（2026-08-24）

- fresh force-all compile：`Assembly-CSharp.dll 02:25:59Z`晚于source，compiler error=0；
- CharacterAssetDeployment focused job `75849d918dec46d88b01f1253cecec63`：1/1 PASS；
- full `BattleRuntimeSelfCheck`结果`Temp/NTSD_BattleRuntimeSelfCheck.result`于`02:27:38Z`写入`PASS`；
- 旧AnimationConfig/FrameConfig literals在self-check中为0；movement、Naruto DDJ、sprite range、weapon字段断言全部
  原样通过；
- self-check期间两条registration rollback/rest-binding负向fixture按预期写error日志；结果PASS后已清Console，最终error=0；
- production catalog/loader、data.txt、DAT/BMP与gameplay0改动。本包VERIFIED，无后续first difference。
