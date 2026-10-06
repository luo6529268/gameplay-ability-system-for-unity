# 第19批：真实项目纹理高命令数提交验证

状态 SCOPED_REAL_TEXTURE_CPU_BRIDGE_PASS；来源：用户“开始执行下一批的任务”；条目 M-03/H-11 限定验证，父项仍 OPEN/RUNTIME_PENDING。

2026-10-07限定结果：六workload10800接受samples各当前线程0B/0growth/0CPUlease；初7completed1MCP网络日志failed留痕，仅affected＋54回归55/55通过，不写初7/7。原Menu clean8roots/idle/nonPlay/error CS0、7备份/679保护、production未改；报告 ../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH19-REAL-TEXTURE-SUBMISSION-20261007/REPORT.md。下面是事前准确范围与未覆盖项，未因本限定通过自动关闭其它门。
平台：原 Unity2022.3.62f3 Editor、现有URP/D3D11实例；先确认 idle/nonPlay/Scene clean，使用原Editor EditMode Runner，不启动第二实例，不主动切Scene/Play。
唯一新增代码路径 Assets/NTSD/Scripts/Test/Editor/BattleRealTextureSubmissionEditorTests.cs 及同名.meta；只新增测试，不改production/Q06活跃方法体/Scene/Prefab/资源/ProjectSettings/InputAction/Server/Gen/Plugins。
现存修改范围：七治理/进度文档，逐路径与当前字节备份见Operation before.json；以当前未提交内容为基线，不用HEAD恢复。
Workload：项目现存Naruto/Sasuke BMP经既有Unity importer的Texture2D资产；只加载不修改，记录源/meta SHA及实际texture身份/尺寸。预排序合成publication命令与相邻motion样本，100/500/1000 command连续/交错/Strict及4097跨chunk；不是实际DAT decoder/catalog或AI模拟/正式source闭包。
两独立submission slot预备+seal；受控 snapshot CaptureFrame→显示motion Prepare/Apply→backend Build/upload→Foot/Health禁用空构建→submission Publish→read lease→CommandBufferPool/Get/DrawMesh→Graphics.ExecuteCommandBuffer→record/lease release/Retire。
正常33ms/3ms/输入/RNG/checksum/World真值不运行或改变。Q06排序器body不读，命令既定顺序仅检查backend分段；first-visible/透明全场未知保持。纹理不重烘焙、不换bank/格式/budget/segment/failclosed。
每组64warm+1800同步样本，固定array与标量；startup reflection/delegate/JIT、资产加载、断言/readback/JSON在热窗外。检查全受控CPU桥当前线程0B、0capacityGrowth、draw=有效physical segment、两slot frozenFrame隔离、mesh/stride稳定、源motion/command不被改写、整份overflow/held lease拒绝。不把CPU Execute返回/lease0视GPU完成；无GPU性能capture/fence证明。
明确范围：Graphics.ExecuteCommandBuffer是测试桥，不冒充生产RenderPass/ScriptableRenderContext.ExecuteCommandBuffer/自然publication/全部辅助活动/PlayerLoop/1800真实帧或Android0GC；总CPU时长包含提交可能等待，不归因为GPU成本/真实batch/收益A-B。
验证：compile→新具名6规模/分段+容量lease测试→相关旧聚焦→ledger/diff/保护SHA与原Menu状态。0B断言如失败保留原件/定位，不调整0GC硬门或改生产无证据规则。
输出 artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH19-REAL-TEXTURE-SUBMISSION-20261007/ 下CreateNew run-GUID报告，不覆盖旧文件。
新测试没有接入runtime owner/worker/queue；临时submission先lease释放→Retire→dispose三个backend，CommandBuffer归池、仅销毁自建material/RT；项目texture/shared shader不销毁。无关闭11阶段顺序变化。
EXT-1 PROPOSED/MODIFY_REQUIRED无专项M0；MONO USER_HOLD；ATLAS合同不变。回滚仅经批准按新增fixture和本批文档hunk定向处理，准确当前backup保留；不删除/清理既有内容。
