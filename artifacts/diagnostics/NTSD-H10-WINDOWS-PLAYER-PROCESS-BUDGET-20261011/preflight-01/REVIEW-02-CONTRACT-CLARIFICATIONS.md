# PRE-FLIGHT-01 review-02 合同澄清（新原件）

继续PLANNED；本件追加澄清，不覆盖POSTPROCESSOR-COMPATIBILITY-PROPOSAL.md或已冻结旧原件。主会话接受该提案作为最小候选，但旧processor第四写域仍待直接人类确认；即使该路径获批，也不等于CASE/raw UI/输入/tee已锁定或已经允许实施/build/Player。具体派发由主会话统一整合。

## 兼容分支固定约束

精确H10输出在active上下文缺失/已消费/域重载失去时明确拒绝，不能TryGet=false后走旧清单；由newbuilder常量＋请求精确身份识别保留输出，禁前缀/磁盘marker激活。非H10未激活继续旧1373/46927691与原CSV逐字；非Windows仍return。

旧processor候选改域只含OnPostprocessBuild合同选择/完成通知/日志、ReadManifest参数化同算法和最小internal只读资格入口。VerifySourceSet/VerifyDestinationSet/ResolveWithin/逐文件SHA/不可覆盖copy/SPARK算法保持；无ignore/allowExtra。generic417绝不写入formal根。

改变backend/Build前冻结immutable合同，绑定请求SHA、nonce、manifestSHA、exact output、两Scene、target/options与同一owner；callback读冻结副本并核磁盘manifest及真实源漂移。Prepared→CallbackEntered→Completed最多一次，finally无论成败清上下文并按已批准合同恢复backend。internal入口无任意setter/可变List外泄；不重build。实际diff实施后另审。

主会话独立审计（与执行会话先前自审分列）：formal CSV SHA47efe8bdebe4c7a39e970fb3a6e1eef0300cf6a865ae36201f1f875289f24877，2334/80126190B逐项源SHA/长度/集合/目标0差；旧共有1356无漂移。known-domain manifest SHA4f171d95fe838015d8ccae944b97e7299c59d95a03362f8ed0b60d1dde617915，2752/96956488B逐项/containment/unique0差；processor SHA079bb670fdd08c5a20785bddfc725362ddc41ed60130fee97c7851b8d439fc48、build不存在。均是只读清单证据，不是raw UI闭包、部署或Player实测。

## tee返回与所有权更正

主会话独立读Driver733–758，sink选择diagnostic override??App.SoundPlayer，PresentSounds正常返回后才增dispatch计数，finally清原List。执行会话未新读该方法体，仅引用具名供证。

因此tee观察失败、满缓冲、未知cue、校准失效**均不得在PresentSounds里抛新异常**，即使原player已经forward完也不得抛，以免改变Driver的计数/消费。tee只置固定状态，原列表同步forward一次；由外层冷controller下一安全边界停止。生产player自身异常仍原样传播，观察的catch/finally不得遮蔽它。原List只在当次调用借用；历史只保存Cue字符串引用与Tick/WorldX数值的自有副本，不拿可变数组/List当历史快照。

只允许原override=null，或精确ReferenceEquals captured NTSDSoundPlayer owner；其它未知override归属直接在安装前拒绝，不替换或链入用户observer。已允许冷只读字段Driver277取原override、安装/恢复均用现有public setter。_battleSoundPlayer绝不写；若需要冷只读字段292核owner引用，仍列入下面请求，不能将已有允许扩大成setter。

容量采用CASE-SAMPLING-RESTORE-V2的明确固定表：phase256、cue1024、别名索引2048、目标事件/voice各4096每轮、未知32、输入转换32每进程、输入观察8192每进程、voice64、owned2048每轮/最多6144。first unknown/overflow马上使整轮验收FAIL，不等缓冲满再判断，不扩容/截生产转发。GC资格含正控/empty/实际tee、异常/满容量exactly-once及原载荷顺序。新tee未实现，旧63scope不能替它认证。

安装/恢复表仍按V2：ordinary ready后warm前，sample完App-close前恢复；combat StartRun同步Configure后首次Update前安装，runner终态回来核同driver/owner/sink再恢复，之后真实App map11。身份变化则不覆盖他人状态，不伪造恢复PASS。主会话未授新执行额度。

## GameModeConfig身份更正

执行会话只读声明定位得到`Assets/NTSD/Scripts/App/GameModeConfig.cs:6`是无Unity基类的sealed class，:8 gameModeId、:9 battleGameModeId（初值1）。故前稿“GameMode对象asset GUID/instanceID”不能成立。本对象本身按实际两字段、同build及真实VS选择来源核对；如果有拥有它的序列化配置资源，只能记录真实存在的那份资源身份，不能发明GameModeConfig GUID。当前允许的currentGameMode冷只读仍只取得引用，不写值；P1模板保存值，P2比值及来源，不能比较跨进程对象实例身份。

已定位后续最小只读需求（尚未展开正文，供主审供证/授权）：

| 项 | 精确范围 | 决定 |
|---|---|---|
| raw UI后继 | `Animation/Manager/CharacterAnimtorManager.cs::LoadLoganContentWithScopeAsync`313–396，仅直接File/path/资源调用边；新callee先具名再扩 | R4A已到达此实际formal加载，需确认额外raw UI是否仅SPARK或有其它精确文件；不读legacy GameData或完整解析器。 |
| mode来源 | `App/GameModeConfig.cs`仅完整数据字段；`UI/SelectGameModeController.cs::HandleOptionConfirmed`40到下一声明、`OnVSMode`53–59 | 确认实际VS产生的两字段/选择来源，替换错误“对象asset GUID”说法；不要展开其他模式/Q06。 |
| owner冷读 | Driver292声明_battleSoundPlayer；NTSDSoundPlayer78/82/83声明oneShotVoices/oneShotVoiceStartSequences/battleVoiceIdentities | 只冷读精确引用/数组形状，安装恢复不写owner/数组；原source64数组是运行时借用而不是历史副本。 |
| 已消费packet数据 | 已声明Client UPM `com.ntsd.battle-kernel`实际路径`I:/GitHub/Unity_GAS/NTSD_Server/packages/com.ntsd.battle-kernel/Runtime/Abstractions/FrameInputSet.cs`，`SimulationPlayerInput`22–38纯数据定义、`FrameInputSet`51–67构造/Players属性声明 | FrameInputSet是class，不能把LastApplied引用保存成历史。只需按索引复制P1/最多8实际玩家的值字段到fixed缓冲，不读GetCanonicalHash/Server业务/Q06，不修改kernel。当前仅rg定位声明，没有打开这些正文。 |

核模式声明时一个旧猜测名ModeSelectController.cs不存在，随后rg --files找到真实SelectGameModeController.cs；没有构造替代文件。`FrameInputSet`在Client UPM声明的本地kernel中，只定位文件/公开声明，不借该指针扩大Server工作域。

以上不是给H10增加全角色技能穷举或新生产合同，只是把新诊断方案中真实依赖和数据类型错误纠正到可实施程度。当前处理仍仅文档/清单。
