# C044 Unity 负 decrease 跨零 pending 冲量验收

权威：当前正式根 EXE SHA-256 336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3；对应 playable Core 抓取与水平冲量结算 pass；正式OID16/OID2 DAT 的低 timeout2/3/4 [受控源见证](../NTSD28-336B44-Q07-C044-CONTROLLED-SOURCE-001/REPORT.md)。自然四例负帧 timeout38→35，未跨零，不把此受控分支写成自然根证书。

生产首差：Unity BattleCpointWriter.RunKind1 旧分支立即写受害者 Runtime.Vx/Vy，把双方 AttackingCounter 改1，未写双方 HitCount。当前源跨零则保留动作计数，双方 pending contribution count=1，受害者冲量X/Y=±4/-3，当前速度仍为0，后续 finalizer 才结算速度。已只在共用writer此分支改为双方 HitCount=1、保留受害者 Knockback X/Y、删除即时速度写入；未改任何 DAT、Scene、用户比例、GAS或非战斗逻辑。同文件既有C042投掷计数差量保持。

验证：

- 原 Editor 新程序集定向测试：先编译失败于旧Play runner对改名方法的调用（CS1061），已在同文件修复并编译0错；旧程序集16/16不计证书。RED job cbc96e7e8d5d4126942dd6bd7e27d664：19例中新增四例均首差动作计数2/1，余15例PASS。首次新测试初始化 job 9a1e7a04934e46dfafa4f4670a507b2b 超时且执行0例，保留为失败尝试。
- 修后原 Editor Tundra build success、0 error；GREEN job a4813146a2884a6c894c40382f30f108：抓取类19/19 PASS，含Legacy/DataOriented×左右方向四例；相邻帧后处理 job b6deff00b29c4209aab80d92fc547c6f 2/2 PASS。只跑定向与相邻，未跑全量。
- 原项目原 Battle Scene 真实Play中运行已有 R8 抓取/CPOINT/关系探针，原文件 Temp/NTSD_R8_WP01C_03_GrabCpointLink.result.json，复制见 [证据](original-editor-grab-play-v1.json)，SHA-256 2B048397A330AF1B9810F350934729F19E798062A6D471C499F48264C2AFE80E。结果PASS、start/end tick1707、worker inactive；跨零即时受害者HitCount1、Knockback=4/-3，后处理HitCount0、Runtime速度4/-3，动作0/181、timeout-3、关系/帧等待保持；全探针对象数4→4、claimed slot2→2、池占用2→2、统计恢复、cleanupCompleted=true。
- 已退出Play，原 Editor idle/nonPlay/noncompiling；Battle/Menu Scene、GameConfig、ProjectBattleModeConfig 四SHA-256分别保持 3A089236...235ED、DD6A48A...23B9DC3、0527D737...B8EA7、B57CFEF...85B82。git diff --check exit0；Tools/Validate-ChangeLedger.ps1 exit0，1075 Records、15 governed code files。

出口限定：本包只证明正式源受控低timeout规则、Unity两postprocess profile及原Battle Scene中受控关系的生产World逐pass表现。正式根EXE同条件LFR不承载低timeout抓取关系，尚无自然跨零同态；Q07和总目标继续开放。下一按当前总表处理可达首差，C044自然低timeout若不能从发行输入到达保持条件性待证，不能用更多合成样本冒充根运行时。
