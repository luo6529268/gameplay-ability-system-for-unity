# 第86批直接 Battle 共享绑定冷接线
结论：SCOPED_DIRECT_BINDINGS_ACCEPTANCE_PASS / H11_SELECTED_CPU_SCOPE_PASS；Change VERIFIED（本Task）。六项阶段5/6，剩余H07性能门未过，34父项关闭0，65已执行，Goal active；不是全域/Android/120FPS或GPU完成证书。

## 实际改动与因果边界
本次重扫确认菜单AppManager与ProductionEntityStressHarness已有PrepareBattleCentralBindings；BattleTestBootstrap首次直接启动/原地重开遗漏。补齐两处实体创建/PrepareBattleRuntimeServices之前的冷接线，压力Suppress返回仍先发生，空/负ID/null/重复与原SetupTestCharacters语义一致。完整逻辑配置不裁剪，视觉依赖、atlas决策/设备预算/Source回退、资源owner、排序/segment/fail-closed均复用80。只3C#：Bootstrap32新增行、Common fixture新增13用例、probe两个冷menu wrapper；移除本包wrapper后原probe normalized全文同before，无热scope变化。
对应[Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH86-DIRECT-BATTLE-SHARED-BINDINGS-20261009.md)、[Change](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-H11-DIRECT-BATTLE-SHARED-BINDINGS-086.md)、[Operation](../../../docs/ai/FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH86-DIRECT-BATTLE-SHARED-BINDINGS-20261009/RECORD.md)。

## 编译和聚焦的实际结果
- 新RED job7da3739e189b429ba4485fbb2eca6966实际13FAIL；其中首次接线guard错误使用非static EnsureAppManager签名，修正仅新fixture后job95592eb8c8f94f6db7b388aed4ddded2实际1FAIL（缺hook）。两者合起来证明新13有效RED，失败XML保留。
- ff60bc72acbc4717b4c89141fa410100在编译期间提交，终态failed/0实际用例/init timeout，仅工具准备失败，不算GREEN或有效RED；原job保存green-01-not-started/job.json。
- 编译完成后job78c2eb6416bf46afbc3049efaf7d2187实际79/79、0skip；包含新13与既有Common/Foot/observer literal/stages。green-02/tests.xml SHA B36908D0D306361E6231168051E224CC455F7B4BEAFAE4BB204AFDAEDCB7A1D4。最后Console error CS0，不把catalog10314当执行数。

## 原地重开运行证明（与Scene重进分开）
原fixture NTSD.Test.NTSD28Q08DirectBattleResultHostPlayModeTests.DirectBattleSceneUsesRematchOwnerWithoutMenuScene，实际EnterPlay/直接Battle owner成立、旧World换新World、有对象且Running、未加载Menu。原fixture/断言没有改。
第一次job96b745bfa4164e7a9a60d28fbe004856最终XML1FAIL/end2026-10-08 21:39:04Z，错误为MCP NetworkStream disposed的Unhandled log；桥接后自然failed/Job orphaned after domain reload，非生产异常证明或本代理强清。rematch-01/tests.xml SHA7383EC3CEC555B790DA3AC3F47EC103DB2D37BDF903578D811A1F86C38B599F0保留。
声明有记录干扰后仅一次协议修订：第二次job5f3d6a7b1966455aae84e3997c0a51f8，提交后至最终XML仅外部文件/PID观察、无MCP/源码编辑。最终XML2026-10-08 21:57:59Z准确1/1 PASS，rematch-02/tests.xml SHA825294BBB3F8D2F1E9074A6066A8FD4998159AB5C6E6607980953805E70BBE57。当前桥接仍残留running/0completed，但Editor实际非Play/noTest、最终RunFinished XML已终态；因此只引用准确XML作运行证据，不称桥接job succeeded、不强清/重启。两次协议差异是已记录干扰控制，不据此证明某个连接是唯一因果，不忽略任何日志或降低断言。

## 同冻结源的两次完整1800camera
原saved Battle、原StartCalibratedGc(root,true,true,true)，各冷菜单只调用一次；无ProfilerSettings/历史/栈capture/人工注入，运行中无MCP、C#编辑/import。相机编号1起全部1800纳入原完整物化—上传—录制—提交范围，独立Begin/End observer原scope和子范围保持。

| 窗口 | camera/observer scopes | camera/observer events | 两slot | 实际CPU draw录制=执行 | min Foot/Health | growth/max CPUlease | 关闭objects/slots/borrowers |
|---|---|---|---|---|---|---|---|
| camera-01 | 1800 / 3600 | 0 / 0 | 2 | 5840 = 5840 | 2 / 2 | 0 / 0 | 0 / 0 / 0 |
| camera-02退出后重进 | 1800 / 3600 | 0 / 0 | 2 | 5840 = 5840 | 2 / 2 | 0 / 0 | 0 / 0 / 0 |

两窗phaseDONE/statusPASS、pre/post camera与observer stage正负校准全PASS、invalid camera/observer/stage0、unattributed0、所有sample绑定catalog。两窗start tick8、end1034/1035；这是既有首个合格camera边界，不新增首帧豁免。当前每帧actual Texture2DArray segment1—2、SourceTexture segment最多1、AtlasPage0；证明共享绑定实际生效，不称全部资源单array/零Source回退。
configuredRenderFps120是配置，不是实测120FPS。5840/1800约3.244中央CPU绘制命令/观察camera，不是全帧SetPass、GPU batch或SetPass实测。旧43 draw11536/11570仅历史不同版/环境对照，不作为受控性能A/B或旧12event调用点已定位的证明。
两生产window SHA：33317F2D35D561BAC50A140105EFECDE7A92B83698A966EAC54B76FF38341F36 / 53B73EAEB60B01C1201CC40DF27BCEA9B27C8B1DB9A87A5A0432C7D8C9A4D45C；对应materialization-window原件保留。每窗十一阶段orderedShutdown true，Scene clean/双磁盘SHA不变，恢复原Menu sole clean8/idle/noCS。
GC.Alloc原值单位TimeNanoseconds；校准有效零事件支持所选当前主线程CPU域零分配，不把raw value当bytes，不覆盖其他线程、native/GPU、GPUconsumer完成、1000AI或全PlayerLoop。

## 阶段收口与下一动作
H11本阶段限定容量/显示/完整可靠CPU 0GC/正常关闭重进及本次affected直接rematch通过；复用M03、H06、M13、M14已结束限定交付，不重新扩域。旧43自然12event未知/FAIL和79未采样原件保留，本次fresh代码/绑定状态的两完整PASS不是历史失败被删除或已归因。父H11保持OPEN（GPU/native/device域未认证）。
H07仍未达：85最新OFF logic mean42.478598/42.985574ms、P9547.443065/51.347850ms、全run dropped397/258，仍>33ms；本包不是千人性能测量。生产Role默认未改，已发出准确异步授权询问（未答复不视为批准），H07同范围安全优化继续、目标不停止；不重跑本包已过scope或H06/其它4项，不解冻EXT1/ATLAS/Mono/Burst/Grid。
审计最终SHA/范围外保护/备份/validator/diff见Operation after-01.json；正式336、33ms/3ms/max2、逻辑结果/输入/RNG/排序/segment/预算保持。无Scene/Prefab/资源/meta/设置/Gen/Plugins/Server改动、删除/移动/Git discard/提交/push/新Editor。
