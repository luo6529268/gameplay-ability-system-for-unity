using UnityEngine;

namespace BeatEmUpTemplate2D
{

    /**
     * 敌人移动到目标位置的状态类
     * 继承自StateNode基类，用于控制敌人向指定目标位置移动的行为
     */
    public class EnemyMoveTo : StateNode
    {

        public override string animationName => "Run";    // 移动时播放的动画名称
        private Vector2 destination;             // 目标位置坐标

        /**
         * 构造函数
         * @param pos 目标位置的Vector2坐标
         */
        public EnemyMoveTo(Vector2 pos)
        {
            destination = pos;
        }

        /**
         * 固定更新方法，每帧调用一次用于处理移动逻辑
         * 包含以下功能：
         * 1. 计算移动方向
         * 2. 检测前方是否有墙壁
         * 3. 执行移动和播放动画
         * 4. 检查是否到达目标位置
         */
        public override void FixedUpdate()
        {
            // 获取单位当前位置
            Vector2 unitPos = unit.GetComponent<UnitActions>().currentPosition;
            // 计算朝向目标位置的标准化方向向量
            Vector2 moveDir = (destination - unitPos).normalized;

            // 检测墙壁碰撞距离
            Vector2 wallDistanceCheck =
                Vector2.one * .3f;
            /* 
             * 墙壁检测距离计算说明：
             * 除以1.8f是为了让检测距离略大于碰撞体大小
             * 否则可能永远不会检测到墙壁碰撞
             */

            // 如果检测到前方有墙壁，切换到空闲状态
            if (unit.WallDetected(moveDir * wallDistanceCheck))
            {
                unit.stateMachine.SetState(new EnemyIdle());
                return;
            }

            // 执行移动并播放跑步动画
            unit.animator.Play(animationName);

            // 如果到达目标位置（距离小于0.1），切换到空闲状态
            if (Vector2.Distance(unitPos, destination) < .1f)
                unit.stateMachine.SetState(new EnemyIdle());
        }
    }

}