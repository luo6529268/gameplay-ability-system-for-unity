# NTSD-OPTIMIZATION-BATCH80-SETPASS-MATCH-BINDINGS-20261008
状态 RUNTIME_PENDING / SCOPED_RENDER_RUNTIME_PASS：本声明千人SetPass实测1995→5；正常App/多角色/设备完整视觉未验，H07/H11继续OPEN，非Goal停止边界。
授权：人类明确“批准，按上述脚本范围优先修复 SetPass”，本局完整视觉依赖＋既有预算内共享纹理绑定，保留完整逻辑配置。
## 准确写域
1. CharacterAnimtorManager：Preparing/空World/未seal/无读lease门控；完整已加载配置中选中角色全帧OPoint和现有state变身递归候选＋共同资源；全部catalog恢复有效源绑定后复用现有统一atlas builder，非候选/超尺寸保留Source2D；仅原子更新central bindings与owned atlas资源，不重新提交逻辑/UI/content key。
2. AppManager.InitializeBattleAsync：ApplyMatchConfig/map之后，pool服务与实体出生前以use玩家ID调用。
3. ProductionEntityStressHarness：仅Setup冷接线，recreated Preparing World内以实际selectedCharacterOid调用同一API；不改请求schema/tick/AI/碰撞/统计。属于对应聚焦验收。
4. BattleCommonAtlasBindingEditorTests：同文件新增依赖、源身份、完整catalog、共享纹理/预算fallback/冷边界/所有权测试，先RED。
5. BattleOptimizationWindowsAiSuiteEditor：仅新增本80固定root两Combat1000 OFF/ON准备开关与原central segment readback复用，17参数/旧请求/统计/cleanup不改；120warm＋180sample，同seed/actual1000，既有四碰撞开关ON/独立候选OFF。
无需新增Central/Driver写域；确需时先精确补Task/Operation，不顺手扩大。

冷像素来源补充：正式body/SPARK/WORDS源Texture已Apply(false,true)，不能GetPixels32。候选必须重用BMPLoader.LoadVerifiedImageData＋原PrepareBattleSheetPixels/ApplyNativeSparkBlackKey，从已发布candidate原SHA解码，仅CPU像素、不创建新Sprite/源Texture、不改importer；未知clamped/COM不可读资源保留有效Source2D。静态builder允许测试提供冷像素provider。各源dimension必须同原descriptor；content变更/解码失败拒绝整候选。
## 不变量与所有权
336B44/D023、精确33ms/3ms/max2/checksum/RNG/publication只读/透明排序/segment/fail-closed/11stage关闭保持。Q06方法体不读。不得改PNG/DAT/Scene/Prefab/meta/importer/ProjectSettings/格式/预算/bank/EXT1/Mono；无Instancing/EXT1专项M0。
完整catalog/logic/UI不裁剪，逐项保留Sprite/源Texture/rect/pivot/key/Count，未知隐式spawn仍有效Source2D；资源coverage完整不等于全部隐式spawn已optimized。构建失败不部分发布。Preparing/空World/未seal/无lease硬门；只旧atlas资源退休，源纹理和Sprite所有权保持。旧/新/在途atlas预算不得提高；冷像素/staging单独记录，不能声称全驻留内存测量通过。源不可读显式fallback，无GPU读回/改importer。热路径无新工作/扩容。
## 验证和退出边界
新测试有效RED→新＋旧common/冷边界GREEN，Editor0CS；两实际Combat1000 OFF/ON记录segment/DrawMesh和全帧SetPass，不能当GPUcapture批数；同请求除output、末tick/checksum/RNG、完整catalog/config、三容量、unresolved/stale/关闭0、Scene字节保持。H07P95<33ms/drop0、H11完整0GC仍各自OPEN，收益不能complete总Goal；不因批数停止。79PLANNED/DEFERRED保持。
## 操作和回滚
先Operation的12当前SHA/Git/blob/副本，再C#修改；公共Temp XML/result覆盖前备份，新证据CreateNew。不删旧失败/dirty，不reset/restore/clean/stash/commit/push。回滚另获批准，仅定位本hunk。当前Editor74724/6402/2022.3.62f3/Menu sole clean8/idle已核，每次控制前再核；仅正常saved scene load、Play、原owner退出回Menu，不保存dirty、不第二Editor、不清console/Profiler。

