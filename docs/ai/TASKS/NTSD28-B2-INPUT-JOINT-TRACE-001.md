# Task Contract — NTSD28-B2-INPUT-JOINT-TRACE-001

> 状态：`VERIFIED / B2-SCOPE-JOINT-CLOSED / HUMAN-SCENARIOS-EQUAL / AI-TICKS1-2-EXACT-AND-RNG-EQUAL / FORMAL-BEHAVIOR-PASS / DOWNSTREAM-FIRST-DIFFERENCE-B11`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2 EXIT`  
> 建立日期：2026-09-04

## 目标

用同一scenario、seed、applied input与completed tick闭合B2输入联合trace：phase、proxy、history、combo10、
action/motion以及authority CRT+synchronized与Unity双流证据。先复用现有workspace-owned authority source
capture、Unity raw exporter和independent comparator获得新鲜首差；若schema不能表达B2字段，先形成精确
缺口清单，再另立代码包扩展。

## 证据等级

- 根目录正式`NTSD2.8-Logan.exe` SHA仍为
  `1277B70BA030A1F33B625EEA20B43834325B280CEC555650BF43CD90A64DAF75`。
- `Tools/NTSD28AuthorityTrace`重编译的是对应playable source closure，输出必须标为
  `SOURCE_MODEL_DIAGNOSTIC_ONLY`，不能冒充正式EXE runtime trace或certificate。
- 现有scenario内`referenceExeSha256`是ScenarioLoader历史身份；capture header必须另列当前formal EXE SHA、
  source manifest SHA、runner source SHA和capture binary SHA，禁止混同。
- Unity现有B0 domain raw只保证applied input、RNG stream topology、slot/epoch/lifecycle；47-field entity raw包含
  action/motion等，但未确认包含exact input proxy/history/combo10。未表达字段不得以0或legacy projection代替。

## 当前允许操作

- 运行现有`Tools/NTSD28AuthorityTrace/Build-AuthoritySourceCapture.ps1`，输出仅写workspace `Temp/`。
- 运行既有authority source capture、Unity raw capture测试/请求与`Tools/NTSD28Parity`验证/比较命令。
- 修改本Task、Change Record、Ledger、STATE、handoff与总表。
- `code-path: NONE`；本阶段禁止修改C#/C++/PowerShell/tool source、Config/DAT、Scene/Prefab、ProjectSettings、
  Packages或authority。若现有schema不足，必须先更新Task/Record允许路径或另立独立实现包。

## 验收

- authority capture重新构建，manifest hash与当前formal EXE/source manifest身份被记录；
- 同一`input-common-two-entity`双端raw/domain capture均通过各自validator；
- comparator输出首差，明确区分共享域相等、RNG topology差异、content strategy差异和entity/action差异；
- 建立B2 exact字段覆盖矩阵，不能表达的proxy/history/combo/call log标missing；
- 只有正式EXE可观察证据、source-model trace、Unity trace和必要运行时场景均闭合后才允许B2 exit；否则保持
  diagnostic/pending。

## 回滚

本阶段只生成`Temp/`诊断文件和治理记录；删除Temp输出即可，不触碰生产代码或authority。

## 当前证据

- authority source capture已重建：formal EXE SHA `1277B70B...DAF75`、source manifest
  `C59BD8D...F2D75`、runner source `05E352F5...B09DC`、capture binary `9BBF3D90...756D2`；
  evidence明确为`SOURCE_MODEL_DIAGNOSTIC_ONLY`。
- input-common双端3tick/2entity raw与domain均通过各自validator。authority entity/domain SHA分别为
  `EFF8D693...1A238`/`A3C3372F...C39A6`；Unity旧译码entity/domain SHA分别为
  `A8B6DD05...3F50`/`C28C478B...CB08`。
- domain共享input/slots/lifecycle相等，capacity1000/400按用户例外；旧exporter只发布
  `unityDeterministic`，尚未表达已实现的Unity native CRT+synchronized双流，故RNG topology仍是schema缺口。
- entity首个value difference为tick2 slot1 action110/210、state7/4。调用链复核证明production crossed
  FrameInput合同正确，真正错误是测试exporter把物理J/K/L直译为Attack/Jump/Defend，令物理K+L误成native
  J+K。已建立`NTSD28-B2-UNITY-TRACE-PHYSICAL-BUTTON-MAPPING-001`先修诊断翻译，再重跑同场景。
- baseMaxMp 500/200属于B11内容策略；9个raw missing及exact proxy/history/combo/call-log schema仍待后续分类，
  不以默认0或legacy projection伪造。
- translator修正后同场景action/latch/previous/tick snapshot/state/counter全部equal；entity首差转为B11
  baseMaxMp，其余9 missing。为避免修改B0 schema或让B11阻断B2，已建立
  `NTSD28-B2-INPUT-RNG-JOINT-RAW-SCHEMA-001`输出独立exact input/native RNG诊断格式；per-call RNG log仍会
  诚实标missing，后续另包。
- direct-battle synchronized pre-draw和frame110/114 exact defend cooldown mirror完成后，input-common独立B2
  comparator得到3 ticks/6 entity pairs全equal、`firstDifference=null`。当前仍缺per-call RNG与formal EXE
  可观察证据，故B2不退出。
- completed-tick direct RNG v2现覆盖input-common空调用序列与standing-attack tick2单次
  site0x82/bound2/result1，两组均3 ticks/6 pairs equal。AI cursor per-call与formal EXE仍未关闭。
- v3 accepted AI trace与后续first-tick/history/host/store修复现证明AI ticks1—2 exact input及6/7次逐次
  synchronized calls全部相等；tick3首差是tick2 action650/9 B11内容分叉的下游。
- hash锁定根formal EXE已分别以p2-human/p2-ai完成900tick headless smoke，均exit0/passed；human双人七键、
  动作、无输入对照与双方reset clean闭合，authority清单未改变。formal smoke不导出exact/history/combo10/
  per-call RNG，因此正式可观察行为已补，内部字段继续由对应playable source joint trace提供，不伪称formal
  per-call certificate。B2是否退出需另做剩余I/R/A条目审计。

## B2退出结论

- 后续exact/per-call、AI roundtrip、formal headless、NONAI RNG crosswalk与F1～F12 real Play证据均已闭合到各专项Record。
- human common/standing与AI ticks1～2的B2 exact input/native RNG无未路由首差；AI tick3首差由B11 action内容分叉传导。
- formal EXE内部per-call字段仍不在smoke schema，故只声明source live path内部joint + formal可观察行为，不声明
  formal per-call certificate。该边界不再阻断B2 scope退出。
