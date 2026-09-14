<!-- CHANGE-RECORD
id: NTSD28-Q06-DEAD-CHARACTER-FRAME-AND-HELD-SOURCE-WITNESS-001
status: VERIFIED
change-kind: DEAD_CHARACTER_FRAME_HELD_ORIGINAL_WITNESS
code-path: Tools/NTSD28AuthorityTrace/dead_character_frame_held_witness.cpp
authority: Formal SimulationTickDriver28::step; BattleWorld28::step_frame_slot/resolve_pending_lifecycle/apply_held_refills and physical/hit reaction callers.
evidence: State9998 original full driver versus Unity type0 HP0 48 differences, confirmed Unity extra pre-OPoint drop/bounce method.
-->

# 死亡角色帧/持有关系原函数见证

IN_PROGRESS。唯一新C++ runner，完整driver和frame/lifecycle两端点分别输出，禁止把后者当完整C25。矩阵覆盖旧Unity bounce谓词边界动作0/5/11/12/110/111/180/184/185/186/189/190/212/214/215，state0/12/14、HP-1/0/500、ground/air/landing、无持有/轻武器/重武器/明确WPoint释放、render phase0/3。初始base HP500后设当前HP模拟已有角色变化；持有字段按原kind2/重武器关系合同建立双向link，输出父子raw+relation+pending hit impulse与源消息/RNG，分开比较frame子调用与完整physics/hit/WPoint时序。无碰撞ITR的完整tick不是一次新命中的复现；hit_response源码仅定位保留职责，不伪称已跑新hit。

在原authority构建器重验EXE/75源码manifest后编译，重复stdout字节一致、frame/lifecycle错误检查、归纳保持/释放/物理反应分支。输出repo Temp/artifacts，不改正式源/EXE/Unity。后续任何Unity修改另立准确Record，不能按此计划直接移除额外方法或真实physics/hit职责。无新增runtime服务/关闭阶段，回滚仅该新runner且需批准。

VERIFIED / SOURCE_MODEL_DIAGNOSTIC_ONLY，6480两类范围分别输出、复跑字节一致，见同名REPORT。
