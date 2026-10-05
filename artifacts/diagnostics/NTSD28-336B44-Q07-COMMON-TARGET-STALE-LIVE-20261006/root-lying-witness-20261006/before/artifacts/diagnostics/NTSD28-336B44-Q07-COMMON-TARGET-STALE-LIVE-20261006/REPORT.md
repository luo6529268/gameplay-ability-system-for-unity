# Q07 重扫失败后继续消费活跃旧目标：限定修复结果

状态：RUNTIME_PENDING / SCOPED_STALE_LIVE_TARGET_DRIVER_PASS。已确认的共用规则首差已经修复，原 Editor 的七项限定检查通过；Q07、Q09、Q12与总目标保持开放。

正式 native_ai.cpp 的缓存块在旧目标 HP0、state14、phase或组无效时触发重扫；重扫失败仍保留3F8，只有最终3F8为-1才清本体HP。随后本体HP门与world.entity查询决定是否继续行为1/3等。旧槽仍active时可继续消费，已经不active的槽不凭raw字段造运动。Unity原来保留数字却以bestSlot查询，失败返回null；Fa1还额外检查目标死血并清本体HP。

## 实际首差与最小修复

正式内容 OID99的frame230为state14，OID902/frame0为type3/hit_Fa1。三项direct消费者先用实际AI从-1取得50，再显式控制目标倒地/HP0与替代组、重置subject位置和速度；一个完整Driver受控缓存50并执行真实生产tick。它们不是自然玩家击杀/倒地输入。

原Editor四RED完成，三个预期Vx+.85与0首差失败，替代目标控制未报失败；failed job未发布summary，不能虚构RED通过/跳过数。完整tick初态13项相同，tick1首差是sourceX500.85/500、sourceY-98.8/-100、integerY-98/-100、Vx.85/0；RED声明字段22/26同。

生产只有两处共用增量：扫描尾按最终3F8查询active实体，删除Fa1目标HP0清subject HP的额外门。扫描筛选、排名、主体HP、-1 sentinel、inactive语义、2F8与其它分支保持。测试新增两个具名方法/四案例，222行插入，全部旧行保留；此前已有dirty未回退。

## 实际验证

- fresh native v2编译exit0，运行exit0；当前声明28 Core CPP、C++17/O2无fastmath，70输入在前后及收尾复核无变化。v2是当前Core实际消费者，不是正式根EXE/完整Host/GPU运行。
- 生成工程 dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q -clp:ErrorsOnly：RED0错误301警告；GREEN0错误334警告，GREEN工具报告27.87秒。
- 原Unity Editor两批各一次MCP Refresh，Library运行时与Editor程序集均比对应源文件新鲜。RED job508d5e1656fd4d03ad2f0ced9f935d05完成4项；GREEN job9d69c11e3abd48b282ea1c5b675725c9为7/7 PASS，0失败0跳过，101.9008333秒。每批首查询MCP30秒处理超时原件保留，续查同job，没有因此重启。
- 三direct的5个声明字段共15/15同；完整Driver初态与tick1各13项共26/26同，统一view X投影残差0。实际配对见declared-field-comparison.json。
- 七项只包含本包四例、前包缓存type两个控制、一个既有共用局部SelfCheck；没有跑全套或角色矩阵。正常GREEN direct零对象尾断言和完整wrapper清理断言通过；不称RED抛错后尾部通过。
- 最终原Editor idle/nonPlay/noncompiling、无测试运行，Battle clean/root11，Console0error；四保护文件与九before备份稳，三DAT正式/暂存两端原SHA保持且raw相等。独立只读审阅无阻断，生产SHA837623…008。

## 身份漂移与证据边界

首native编译前guard发现外部build.ps1从5F400变5E85，发生于编译启动前；已保存前后字节与delta，当前28Core CPP声明相同，但完整playable闭包未比较。根EXE336B44与三个规则body保持。

native v1虽compile/run0，尾部70输入guard发现battle_world.h BC03变22D5，其余69项稳。该header为未跟踪文件，HEAD不能读取旧字节；保留失败与after内容，执行者未知，没有恢复或写外部源码。v1结果不接纳为修复证据。一次fresh v2以已保存22D5重新编译与运行，70项身份稳定才写生产；不能回溯宣称v1或全部原before权威稳。

native subject槽51、Unity槽1，初态构造/World/seed/载体并非严格全世界同态；只比较声明subject字段。Unity旧scenario schema/Stage23只是夹具，内容仍正式LoganRuntime和项目mode Asset。自然链、正式根同初态、Host与GPU未覆盖，只按实际非例外首差或相关改动回访。本包没有改DAT/图片/Scene/InputActions/非战斗流程，没有删除、移动或Git丢弃。

本必要ONE完成并改为REUSE。当前229份同名Record中68份未关闭，REUSE54/TRIGGER14/P0=DEP=ONE=0；记录未关闭不等于68项必做。Q12六代表子门已有各自限定证据，不重复旧样本；整场完全一致仍未证明。

2026-10-05T16:55:28.900628+00:00 最终身份更正：此前scope-pre-final-check时当前70输入稳，native v2编译/运行前后70稳定证书仍有效；随后最终guard失败实际仅README_SOURCE.md C0BA→DE936，不是新header/CPP变化。已保存当前全文post-green-current-0-README_SOURCE.md.txt。其现声明源目录为持续开发的锦标赛候选，不能把当前源快照与旧根发行EXE视为逐字节对应。根336B44/三个规则body/当前其余69输入稳，但今后不能把当前源树自动当336B44对应权威；下一先只读核冻结源码及根EXE对应关系，不用候选定新规则，不重跑七项或重编仅为README变更。已通过7/7、Core26/26/direct15/15证据保留，RUNTIME_PENDING/父Q与目标开放。最终不称70输入全稳或已对齐根EXE。
