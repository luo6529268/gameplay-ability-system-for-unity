# 原函数武器碎片见证结果

VERIFIED / SOURCE_MODEL_DIAGNOSTIC_ONLY。正式 playable BattleWorld28::materialize_weapon_piece_fragments 和 resolve_pending_lifecycle 直接运行；诊断程序未晋升为正式 EXE。

构建命令：`Tools/NTSD28AuthorityTrace/Build-AuthoritySourceCapture.ps1 -OutputDirectory Temp/NTSD28Q06WeaponPiece -RunnerSource Tools/NTSD28AuthorityTrace/weapon_piece_witness.cpp -ExecutableName weapon_piece_witness.exe`，退出0。参数与输出见 runner、native-build.log、build-manifest.json。157个构造输入生成762片，正式Logan三个定义生成50片；重复执行stdout字节相同。validation.json记录逐项核验与SHA。

正式OID151生成15内置+7 DAT片、150生成13+7、124生成3+5，同步调用分别136/126/58；未消费CRT。所有立即出生片HP/bound/base/MP均500，即使stats包含ohp25/omp50；max_mp保留metadata值，包括0/-1。内置默认owner -1/group0/facing false；DAT按字段继承owner/group/facing。位置以source整数位置加随机，不继承小数。四个动作镜像一致，counter0，sound/opoint latch -1。无空槽双variant场景仍有2次选择调用，证明选择早于容量判断。

正式EXE SHA B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033；75源码/header manifest 07CD47A0623F23D2C439E0E85EABF2ED10F8EAE8FC7D70DDB8396C704B3D778F。运行器SHA39B3E06879042C84DE219F844F32F34755E097234733E46F55126FBDAF14EC19，binary SHA FE39034FDD7044DADD9479C268CB8A87A70AED1C43F5ABABED5BD544715EF271。

限制：此结果证明当前源码两个端点，不是正式EXE屏幕验收，也不是完整driver高低slot当tick参与。Unity producer未实现；SelfCheck旧失败没有被覆盖，不能声明战斗完整对齐。用户已确认HUDBg横坐标变化由本人/其他任务产生，必须保留。禁止computer-use。
