# Q07/C051 effect23 护甲 Unity 完整 Driver 首差

状态：VERIFIED / SCOPED_DRIVER_PASS；依赖 `NTSD28-336B44-Q07-C051-ARMOR-REACH-001` 正式源/根阳性。父 C051/Q07 开放。仅诊断 Unity 当前战斗 runtime，不改生产战斗行为、DAT、Scene、资源或非战斗代码。

正式依据：OID78/action466→467自然生成OID447/action58→54，tick3 的 kind0/effect23 ITR 对 OID97/action0/type1 armor 为 applied+armor applies、HP500→495；对 OID2 无甲为 applied、HP500→450/action180。当前根两 LFR 与源码各80tick×11字段880/880零差，seed682973786、mode0、角色X500/550、Z400、actor面右、target面左。正式DAT静态effect22无ITR不作本包入口。

只允许在既有 `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs` 的 `Q07C051Effect23ScenarioSchema` 精确参数验证中增补这两组正式可达初态；schema的旧 OID20→888 左右案例、校验版本/seed/tick/正式内容门保持。新建两份唯一场景JSON和各自全新输出路径，不覆盖既有 raw 或结果。优先用原Editor MCP 的短小 `execute_code` 调用既有只读 Raw Capture 测试入口；若受工具编译限制，依文件操作合同先记录请求临时文件生命周期，再用现有请求机制。不得手动改生成项目、开第二个 Unity Editor 或使用 computer-use。

验收：原Editor导入编译0 error；两案例当前正式内容索引绑定与完整Driver运行成功；同tick与当前根至少核父/目标动作、HP、MP、护甲HP、速度、子体存在/动作/X，导出首差；原Editor回到idle，Menu/Battle/GameConfig/Mode Asset四SHA前后相同，LoganRuntime Git无差异。若出现首差，先查正式初态和比例域，再按独立最小生产 Task/Change 修复；若零差也只记限定Driver证据，护甲原Scene自然按键和整个C051/Q07不自动关闭。回滚仅本包测试脚本新增校验分支，受文件操作合同约束，保留原件。

实测：原Editor导入脚本并实际运行两组正式内容完整Driver各12tick，当前336B44根选定11字段各132/132零差。护甲OID97 tick3 HP495/action0；无甲OID2 HP450/action180。四SHA/LoganRuntime稳，原Editor idle/非Play；临时请求文件的自动清理事前登记并已核验。动态 `execute_code` 编译器报路径过长，未产生结果；随后按已记录请求入口完成，两条历史分开留证。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C051-ARMOR-UNITY-DRIVER-001/REPORT.md)。原Scene Play/自然键/内部逐hit待，父C051/Q07不关闭。
