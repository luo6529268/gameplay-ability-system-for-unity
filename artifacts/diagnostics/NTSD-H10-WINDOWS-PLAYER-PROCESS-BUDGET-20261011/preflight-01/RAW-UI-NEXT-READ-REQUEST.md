# PRE-FLIGHT-01 review-03：raw UI 最小后继读域

状态：READ_REQUEST_ONLY。当前执行会话只对下列文件做声明/已命名调用行定位，没有展开这些方法体；主审可在已有授权内具名供证或明确允许窄读。本件不授权继续解析器、atlas、writer、producer、factory、StageWave或Q06读域。

文件 `Assets/NTSD/Scripts/Animation/LoganVisualContentCandidate.cs`，当前SHA-256 `35ac697cb8b5a43bc9ae76413f59dd3498218b6c188a06a02e00e99b01d99f9f`。

| 精确范围 | 仅需确认的边与决定 |
|---|---|
| `ImageInput` 19–38 | 保存的路径与SHA是否就是传给loader的身份；missing body标志仅判定现有行为，不改可缺规则。 |
| `NativeWordsInput`构造49–73与`Capture`75–118 | resource DAT真实路径、固定索引16起6张的路径解析和不可用分支；不展开`FirstNativeResourceTable`120起解析算法。 |
| `NativeSparkInput`构造155–185与`Capture`187–201 | resource表43索引、image路径和独立raw SPARK/配置路径；不读`ReadDimension`203起。 |
| `NativeKillIconInput`构造229–259与`Capture`261–268 | feed提供的七类路径如何解析、缺图及null的既有含义；不反向展开feed producer。 |
| `LoganVisualContentCandidate.Capture`321–361 | catalog wrapper body/head/small哪些真实已解析路径纳入Images、哪些只允许body缺失；确认这是否提供完整直接图像集合。 |
| `CopyCharacterConfigs`407–412与`GetImageSha256`414–420 | Manager已核调用的路径/配置身份对应，不展开AssertInputsCurrent其他catalog校验。 |

已由主审给出的直接消费链：Manager `LoadNativeUISpriteAsync`397–420将同一path与Candidate.GetImageSha256给BMPLoader；common读取SparkInput，Words/Kill存在时构造对应publication；native准备并验证后commit并return，legacy LoadAllCharacterUISpritesAsync只在native==null。该证据不能自动保证目录中的所有raw UI都可达，也不能据此复制legacy GameData。

得到本表后先核真实路径集合与现有known-domain manifest的交集/缺口；如新callee仍有必要，仅再报告其路径/符号/最小段，不自动扩读。已有2752条manifest只证已知域，不称完整raw UI部署闭包；本轮无build/Player/资源复制。
