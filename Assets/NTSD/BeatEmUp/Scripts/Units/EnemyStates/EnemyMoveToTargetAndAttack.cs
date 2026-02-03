using UnityEngine;

namespace BeatEmUpTemplate2D
{

    /**
     * 敌人移动到目标并攻击的状态类
     * 该状态控制敌人移动到目标位置，并在适当距离发起攻击
     */
    public class EnemyMoveToTargetAndAttack : StateNode
    {

        public override string animationName => "Run"; //移动动画名称
        private Vector2 maxAttackRange = new Vector2(1.2f, .1f); //可以攻击目标的最大距离范围
        private float attackDistance = 1f; //与目标保持的理想攻击距离
        private AttackData attack; //该单位将要执行的攻击数据
        private float pauseBeforeAttack; //攻击前的暂停时间

        /**
         * 构造函数
         * @param attack 攻击数据
         */
        public EnemyMoveToTargetAndAttack(AttackData attack)
        {
            this.attack = attack;
        }

        /**
         * 进入状态时的初始化操作
         */
        public override void Enter()
        {
            if (!unit.target) unit.stateMachine.SetState(new EnemyIdle()); //如果没有目标，切换到待机状态
            pauseBeforeAttack = unit.settings.enemyPauseBeforeAttack; //初始化攻击前暂停时间
            unit.TurnToTarget(); //转向目标
        }

        /**
         * 每帧更新逻辑
         */
        public override void Update()
        {

            //当目标在攻击范围内时
            if (targetInRange())
            {

                //停止移动
                unit.StopMoving();
                unit.animator.Play("Idle");

                //攻击前暂停
                if (pauseBeforeAttack > 0)
                {
                    pauseBeforeAttack -= Time.deltaTime;
                    return;
                }

            }
        }

        /**
         * 固定时间更新逻辑，主要用于物理相关的计算
         */
        public override void FixedUpdate()
        {

            //当超出攻击范围时移动到目标
            bool targetIsGrounded = unit.target.GetComponent<UnitActions>().isGrounded;
            if ((unit.distanceToTarget().y > maxAttackRange.y && targetIsGrounded) || unit.distanceToTarget().x > maxAttackRange.x)
            {

                Vector2 idealPos = getIdealAttackPos(); //获取理想攻击位置
                Vector2 dirToPos = (idealPos - (Vector2)unit.transform.position).normalized; //获取到攻击位置的方向向量

                //如果前方有墙壁，进入待机状态
                Vector2 wallDistanceCheck = Vector2.one * .3f; //除以1.8f是因为距离检测需要比碰撞体稍大（否则我们永远不会遇到墙壁）
                if (unit.WallDetected(dirToPos * wallDistanceCheck))
                {
                    unit.stateMachine.SetState(new EnemyIdle());
                    return;
                }

                //移动并播放'Run'动画
                unit.MoveToVector(dirToPos, unit.settings.moveSpeed);
                unit.animator.Play(animationName);
            }
        }

        /**
         * 获取理想攻击位置
         * @return 理想的攻击位置坐标
         */
        Vector2 getIdealAttackPos()
        {
            Vector2 XDirToTarget = (unit.target.transform.position.x > unit.transform.position.x) ? Vector2.right : Vector2.left; //检查目标是在左侧还是右侧
            return unit.target.GetComponent<UnitActions>().currentPosition - XDirToTarget * attackDistance; //返回攻击目标的理想位置
        }

        /**
         * 检查目标是否在攻击范围内
         * @return 如果目标在攻击范围内返回true，否则返回false
         */
        bool targetInRange()
        {
            return (unit.distanceToTarget().x < maxAttackRange.x && unit.distanceToTarget().y < maxAttackRange.y);
        }
    }

}