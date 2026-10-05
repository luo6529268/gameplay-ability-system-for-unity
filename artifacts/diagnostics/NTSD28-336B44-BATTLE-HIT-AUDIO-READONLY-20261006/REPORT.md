# 当前 Unity 与 Logan 336B44：战斗逻辑和受击音效只读复核

日期：2026-10-06。状态：`READ_ONLY_AUDIT_COMPLETE / SOUND_RESOURCE_AND_ROUTING_GAPS_CONFIRMED_STATIC / FORMAL_EXE_AUDIO_RUNTIME_UNVERIFIED`。

用户确认比较对象是当前 Unity 项目与 Logan 正式版。正式根 EXE 本轮重算 SHA-256 为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。旧对齐目标已经用户接受限定范围收尾；本报告是新问题的只读诊断，不重开旧目标、不创建自动执行队列。

**结论：不能宣布全部战斗场景逻辑和全部受击音效一致。现有代表战斗链的限定通过仍有效；受击声音存在当前源码、配置与磁盘可共同确认的缺口。** 尚未运行的新场景不补写成失败或通过。

## 1. 战斗逻辑：已覆盖的同态与边界

本轮重新读取现行主 tick、角色命中路由、声音队列/发布/消费以及已有收尾证据。当前正常/快速间隔仍为33/3ms；`NTSDBattleTickSystem` 维护帧运动、物理、候选、角色/对象命中、关系、帧推进与表现清理的明确阶段。普通角色命中当前实际路径为：

`BattleHitCandidateSequenceRunner` → interaction consumer → `BattleDamageWriter.TryApplyCurrentDatTargetHit` → `LF2Character.Hit` → `LF2CharacterDatHitResolver.ResolveHit` → `BattleDamageWriter.ApplyStandardCharacterDamage`。

这是生产路径定位，不能由方法存在推导所有分支对齐。既有鸣人持续窗口后续攻击、武器/抓取/对象链、胜负事件、D-024比例映射与有序退出/重进等具名样本保留各自同初态、同输入和同tick证据；[旧目标收尾原件](../NTSD28-UNITY-BATTLE-REALIGNMENT-CLOSURE-20261006/REPORT.md)明确没有声称所有角色/操作/模式/声音穷尽一致。本次未进行新的非音频逐tick运行，故不声称已排除其它未覆盖规则差异。

用户已经接受并继续保留的差异：项目背景/地图/可走区、固定完整背景相机、D-024按正式视口比例统一映射位移和碰撞、现有1.5倍实体显示、D-025非角色出可走区10秒逻辑时间、项目模式Asset、原背景及两类mode DAT排除、普通HUD/名字等表现边界。这些不成为本次修复任务。DAT数值不得改。

## 2. 已确认的静态播放断点：13个内建 SFX 别名

当前 Battle Scene 的 AudioController 已启用，但 `AudioList: []`（`Assets/NTSD/Scene/NTSD_Battle.unity:3035`）。`Assets/NTSD/Sound` 的直接子目录只有Battle、Click、data。当前生产初始化和消费链中，未发现把下列别名转换到正式WAV路径的写者：

```text
SFX_001 SFX_002 SFX_004 SFX_006 SFX_010 SFX_011 SFX_017
SFX_032 SFX_033 SFX_039 SFX_065 SFX_066 SFX_068
```

`SimulationTickDriver`717/732行原样发布和派发cue；`NTSDSoundPlayer.PresentSound`244行原样传cue；307行找不到配置时786行生成 `streamingFolder=soundId`。无扩展名别名进入目录模式，成为 `Assets/NTSD/Sound/SFX_001` 一类缺失目录，不能进入355行只针对单文件的正式VFS查找。

`NTSDResourceLoader`540行缺目录返回空数组；播放器724/752行无配置clip则保留空数组，同时748行标记Loaded；417行PickClip得到null便在420行返回，尚未获取voice或调用Play。因此，按当前保存配置进行新鲜初始化，这13个别名不能播放。**待播事件存在、派发计数增加、预热完成都不能证明声音已响。** 本轮未读取当前Editor内存中的临时注入/缓存，亦未新测扬声器，结论层级是明确的静态生产可达性断点。

DAT直接声明的 `data/xxx.wav` 和角色WAV、已通过的 `data/016.wav` 自然落地voice不受这个无扩展名别名断点自动支配；不能扩大为全部战斗声音都不响。

## 3. 受击相关资源：两套路径查找后的实际磁盘比较

[逐路径结果](resource-audit.csv)、[完整SHA/PCM/字段owner](resource-audit.json)、[角色帧引用原件](hurt-state-frame-sound-references.json)。

从正式data.txt的object区段读取330个对象DAT，全部在位。BMP中三类 `weapon_hit_sound`、`weapon_drop_sound`、`weapon_broken_sound` 各169条，共507条声明，去重24个路径；加sound.dat的18个内建路径，得到33个路径。

另读取158个type0索引项，按防御/破防/被抓/受伤/倒地/冻结/燃烧等状态 `{7,8,10,11,12,13,14,16,18,19,70,75}` 提取帧头sound，共446条引用、125个路径。严格排除itr等嵌套块的sound字段。这个集合是受击相关状态的词法声明清单，不等于所有路径已证明自然可达，也不保证覆盖自定义状态或所有技能诱发的受击帧。

比较优先级与当前播放器相同：单文件战斗cue优先 `Assets/NTSD/Content/LoganRuntime/vfs`，缺失回退 `Assets/NTSD/Sound`。

| 声明集合 | 唯一路径 | 正式VFS在位 | 当前有效查找根缺失 | 回退后PCM/格式不同 | PCM相同、容器不同 | 正式文件逐字节相同 |
|---|---:|---:|---:|---:|---:|---:|
| 内建＋对象三个声音字段 | 33 | 33 | 6 | 12 | 13 | 2 |
| 所选type0状态帧声音 | 125 | 125 | 106 | 6 | 10 | 3 |
| 两组去重并集 | 150 | 150 | 111 | 17 | 18 | 4 |

两组有交集，不能把各列直接相加。150是本次所选声明范围，不是“全部音频”或150个必须单独跑的任务；111缺路径也不能表述为已观察111种自然战斗静音。

33路径中的6个缺文件：`c/shino/w/e1.wav`、`data/0.wav`、`data/001.wav`、`data/006.wav`、`data/032.wav`、`data/033.wav`。应按实际消费者判断用途和自然可达性，不能因声明就假定均应有可听声音。

33路径中有效回退资源的12个PCM/格式差异：`data/002.wav`、`035.wav`、`036.wav`、`037.wav`、`040.wav`、`065.wav`、`066.wav`、`068.wav`、`079.wav`、`084.wav`、`085.wav`、`103.wav`（后11条同在data目录）。020/021已暂存正式文件，其旧Sound版本差异不列为当前有效资源差异。

不是只比较WAV整文件hash：解析RIFF fmt/data、声道/频率/位深/帧数、PCM hash；对8/16位PCM另用共同Q15整数幅度做诊断。18个容器不同但PCM一致的文件不列替换理由；其它17个的规范化Q15/形状也不同。不过数据差异不等于已证明玩家能听出不同，实际混音和设备听感未验。

具体例子：正式与当前暂存的鸣人/李/鼬DAT分别逐字节一致，但其受击帧声音路径缺失：鸣人frame180/186的 `c/nar/w/p3.wav`；李frame221的 `c/lee/w/p3.wav`；鼬frame221的 `c/ita/w/p2.wav`。缺口位于音频资源查找，而非需要改DAT。正式data/002为mono32000Hz、8位9257帧，旧Unity为16位11789帧；data/103正式mono、旧Unity stereo。这些字段差异已有磁盘证据。

## 4. 当前 C++ 与 Unity 受击声音生成分支的静态差异候选

当前README_SOURCE已说明源树持续开发，根正式EXE不随每批覆盖；本轮不把候选/source新变化晋升为336B44规则。下列是真实的当前代码分支对比，尚未补正式根EXE逐声音输出或新Unity同态命中验证，故状态保持候选。

| 分支 | 当前C++源码 | 当前Unity生产路由 | 本轮裁决 |
|---|---|---|---|
| 普通type0非减伤、effect0、非80档 | battle_world.cpp7199：一次基础channel0，定位受击者X | BattleDamageWriter1166先SFX_001，1195再基础SFX_001；两条均可定位攻击者X | 事件数及定位静态不同，正式根音频运行未证 |
| 普通effect1 | 基础0/2 → 032/033 → 再基础0/2，全在受击者 | 除上述序列外还先RecordDamageEffectSound的SFX_002；非knockback基础位置在攻击者 | 固定effect前置与定位候选 |
| 防御/护甲减伤 | battle_world.cpp723：按受击前state选目标DAT hit/drop，空则channel1；type3攻击者用其broken字段但定位目标 | BattleDamageWriter2644按受击者OID37/6选SFX_017，否则SFX_002；type3 broken定位攻击者 | 通用DAT选择及位置静态不同 |

不得凭这些候选立即改所有声音、加角色特判或改DAT；应先选同状态可比命中，排除帧声音叠加和地图例外，记录第一条事件差异，再决定最小共享修复。

帧声音本身允许与命中声音同tick存在。当前两边都按action latch与声明顺序处理最多20条；帧去重不能错误扩展为全tick音效去重。武器破碎、state12/18严格触地016、type1/2/4/6反弹011等是独立触发源，不能合成一条固定受击声音。

## 5. 已有声音证据不作废，也不扩大

既有078、我爱罗j4/043自然clip→voice、C032自然016两次voice和隔离软件PCM等局部证据仍有效。本轮重读C032原件复核报告、078与生产声像报告；它们均明确自己的事件/clip/voice/PCM边界。C032的016容器hash不同但PCM一致，不引出替换。当前空AudioList的minTime/random配置分支静态不起作用，不为它们另造修复任务。主菜单、结果设置/重赛、普通HUD不进入本次。

## 6. 最小后续范围与本轮验证

若用户随后指定修复，本次事实对应的优先顺序是：

1. 在现有战斗播放器的共用入口解决内建cue到实际WAV的解析；验证一条普通命中和一条相邻不同内建cue确实产生clip/voice。不能仅验待播队列，也不能改DAT或加13套目录/角色分支。
2. 对实际受击消费者所需的缺失/不同WAV按逐文件记录补齐正式资源；角色呻吟与物体声合并使用同一查找合同。PCM已等价项保留；不自动整包搬全部技能/BGM/菜单音频。
3. 对普通命中、防御/护甲两条通用分支取得正式同态事件对照，再处理重复、定义owner、空间位置与顺序。无需全角色/全场景矩阵。

这是建议归口，不是已授权的新目标或自动执行排期。

实际运行：Get-Content、rg、git status、正式EXE SHA256、330 DAT声音字段解析、158 type0状态帧声音解析、150路径存在性及WAV/PCM比较；12个当前脚本/Scene/配置SHA在本轮原件快照后再次核对，全部保持。新建的只有本目录诊断报告/JSON/CSV；没有修改项目脚本、DAT、WAV、Scene、Prefab、ProjectSettings或旧总表；未删除、移动、恢复、提交或push任何文件。

未运行：Unity编译、SelfCheck、EditMode/PlayMode、正式根音频回放/录音、AudioRenderer、硬件设备。已有报告里的历史PASS不是本轮新运行。当前Editor缓存或临时AudioList注入仍未知。
