# Type5普通受击源见证

VERIFIED / SOURCE_MODEL_ONLY。585向量，独立验证14048项PASS；两遍exit0、4,112,355 bytes完全一致，SHA256 c164b073b4ee789df121f18dcffe8253d0771c21c37703eca35b2e73d4acf30e。原程序在Build/NTSD28Type5Witness，复用正式playable source闭包，由Build-AuthoritySourceCapture.ps1校验正式EXE身份并构建；manifest已归档。

基础108、水平324、攻击者post18、资源48、双方左向9、垂直18、tier60。输入固定frozen candidate，再设置位置/面向/累计impulse/计数，初始reaction29，type5；有符号fall覆盖负数及20/40/60/80临界。当前源resolve_unarmored_reaction阈值是>60/>40/>20/>0，top80保留；高于碰撞参考平面的行为按所处tier区分。HP/HPBound为signed injury/scale/weak，type5不扣weapon durability。普通source位移、单次攻击者post、raw counters保持、native0xEE及CRT、source音频/火花和强制释放hold后finalizer都纳入。

准确调用：battle_world.cpp resolve_ordinary_unarmored_standard_hit→resolve_confirmed_unarmored_hit→resolve_unarmored_reaction→hit_response.cpp horizontal/vertical→standard rest→apply_native_unarmored_attacker_post_hit→非type0/3目标末尾spark。C++源码文件参与性由build manifest记录。源码诊断不等于正式EXE物理输入验收，未部署DAT/图片；复杂armor/held/effect/前帧状态另有父依赖。

命令：Tools/NTSD28AuthorityTrace/Build-AuthoritySourceCapture.ps1 -OutputDirectory Build/NTSD28Type5Witness -RunnerSource Tools/NTSD28AuthorityTrace/type5_unarmored_witness.cpp -ExecutableName type5_unarmored_witness.exe；执行该EXE两遍；D:/anaconda3/python.exe -X utf8 Tools/NTSD28AuthorityTrace/validate_type5_unarmored_witness.py。

发现对下一步的影响：当前Unity ApplySpecialObjectHurtTail仍为>50/>30/>10并清零80，且有旧音频和攻击者post。不能只补Shadow以复制它；必须先TYPE5-UNARMORED-UNITY-001真实全tuple RED，再准确生产实现。
