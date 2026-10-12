# M12 手机优先有限预检交付

结论：PRE_FLIGHT_READY_FOR_REVIEW / IMPLEMENTATION_NOT_DISPATCHED。Task NTSD-M12-ANDROID-DECODE-UPLOAD-BACKPRESSURE-20261011、Change NTSD-M12-ANDROID-DECODE-UPLOAD-BACKPRESSURE-001保持PLANNED；M12父项OPEN。本轮把必要性、准确两生产代码候选、字节参数、取消/归还与有限验证表集中到同一PLAN，没有写脚本或启动Unity。

## 主审直接使用的三项决定

1. **接受最小native PNG body域。** manager加入operation字节准入/引用收尾，BMPLoader加入stream核验的冷descriptor与guarded内部入口；旧public合同、PNG decoder、atlas算法不改。共享ON和显式OFF均覆盖，legacy/非PNG/UI/common明确在限定byte门之外。新增一Editor测试与两份既有小fixture必要入口，不复制H10录制器。
2. **接受待审参数／超大策略。** 普通128MiB工程权重；超限FIFO等待reserved=0后单张独占；固定当前权重上界439,799,644B，超过即解码前拒绝完整candidate、保留旧publication，不抬上限、不跳sheet。不是Android RAM或whole-process预算。若改成全部>128MiB拒绝，会阻断当前9条资源，因此不能静默推默认。
3. **核定准确关闭接口并接受有限验证表。** 原阶段1取消、阶段2dedicated Join、阶段5清staging保持；图片线程池任务的真实drain不能由head回调旧用例冒充。主审只需核SimulationTickDriver.ShutdownBattleRuntime里stage1/2/5调用及后置条件；本轮未读这个方法体，也未读Q06。如果硬合同确需sourceworker在stage5前Join且当前接口做不到，必须给出准确owner/异步关闭写域由用户决定，不能改顺序。其他源码方案已经收敛，无新泛化读域请求。

全部细节见[PLAN.md](PLAN.md)，不另设子包。已有M12启动授权继续有效，主审独立核定后派同Task实施；本预检不自动暂停/关闭父项，不恢复旧Goal或队列。

## 有据发现与模型界限

CPU4/upload1的现有顺序允许完成像素积压；同一回收额度若保持到atlas消费会与WhenAll形成等待环。计划在decode前取byte额度、再CPU；plan完成后丢本worker float像素引用，先释放CPU再等upload；上传移交后归还byte额度，retained源与clamp另计，禁止只转账就称总峰值下降。

本地新检查703个实际PNG头与旧inventory尺寸全同、总压缩28,102,250B。完整源SHA0差复用主审具名供证，本轮没有重复读703份完整像素源或decode。旧inventory SHA c6ab501ea8b79481ab8afc83ba7ee2a9e72fe5066a9f4b6d9b8c1e5f9afc7154；manager SHA6990b2534ca6c832ab50c5fd692e3275ca335b2e2c77c1502bcc85eaa394ea34匹配主审。

基本24*px+4*fileBytes+8*width+65536模型>64/128/256MiB仍34/8/2。补计最大cell Color32与HashNativeClampedCellPixels的8+4*cellPx副本后变38/9/2，最大439,799,644B（419.4256MiB）。模型只调度已声明大payload；Rects/字典、native texture/atlas、未回收垃圾及全进程不是该权重的已证明上界。最大源两数组350,895,360B与全703源Color32加总2,003,918,132B说明本M12不能单独完成手机内存适配。未裁资源/改规则/开atlas工程。

## 有限下一阶段验证，尚未执行

PLAN给出准确新测试名称、现有小fixture复用和固定阈值：descriptor实际字节一致；cap+1/溢出/坏图/失败；FIFO独占与exact-once归还；真实等待取消／旧generation迟到／重入；全像素、rect、pivot、UV、catalog与共享身份；同owner/public入口和ordered shutdown。

最多一次小fixture同输入A-B-B-A组，六图256²/384²/512²重复、8MiB测试权重／5帧慢upload，实际未上传payload峰下降至少25%、B两次总加载耗时不超过A总和120%、retained最终量相同。普通128MiB和最大E用纯admission整数/小data覆盖，不解码最大资源，不全330，不Player/Android。任何先行硬门失败保留首差交回，不重试同版成本组。冷加载不称0GC；未改battle热路径证据按原scope复用，不生成H07/Android证书。

## 实际执行与审计

只读命令：git status --short --branch --untracked-files=no；git status --porcelain=v1 --untracked-files=all；git rev-parse HEAD；git diff --cached --name-only；rg定位具名符号、Get-Content读取已授权窄段；内联Python读取inventory/703个33字节头、文件长度并计算静态模型与SHA。源worker、PNG、关闭测试都只读，没有调用它们。初始一次沟通误引用已完成FindEffect，核当前派发后已更正；没有因此操作旧任务或Editor。

治理文件通过inline Python CreateNew备份／新件、SHA guard逐文件前置追加，真实执行原件为before-manifest.json、registration-after.json、governance-write-intent.json、mutation-after.json。10份原治理dirty字节完整保留，不从HEAD恢复。没有删除/清理/移动、Git写操作或跨线程发信。

实际适用校验命令：
pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity

结果exit0/PASSED，1424 records、4539 warnings、0 errors。本PLANNED含5条代码路径尚不在diff的预期警告，完整见validator.json/change-ledger.log，未篡改历史或清警告。git diff --check -- [manifest内10条精确治理路径] exit0。

protection.json：46项具名源码／设置／Scene／Input／旧证据指纹0漂移，10份真实备份SHA通过、旧正文尾字节完整，HEAD与空暂存不变。原13项tracked dirty全部保留，本次仅额外令已声明DEPENDENCIES成为dirty（14），意外新增0；原638项未跟踪Git记录仍在。该计数不是所有未跟踪文件逐字节保护证明。新测试.cs/.meta不存在。后续完成记录仅追加Operation/INDEX并另存自身前件。

编译、EditMode、Play、Profiler、性能采样、build、Player、Android及任何Settings/资源修改：全部NOT_RUN/0。M12本轮是设计与代码前治理准备，无性能或运行验收PASS。H08正常默认共享、H10 CLOSED/Player ABANDONED、其余hold/原失败/33ms和十一阶段不变。
