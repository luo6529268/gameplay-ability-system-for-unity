using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using GAS.Runtime;
using MoreMountains.TopDownEngine;
using NTSD.Animation;
using NTSD.Simulation;
using NTSD.TimeWheel;
using MoreMountains.Tools;





#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BeatEmUpTemplate2D
{

    /**
     * UnitActions 类 - 单位动作系统
     * 这个类管理游戏单位（如玩家和敌人）的基本动作和行为。
     * 它处理移动、攻击、防御、跳跃等功能，并提供了一系列辅助方法来处理游戏中的各种交互。
     */
    public class UnitActions : MMMonoBehaviour
    {

        [HideInInspector] public GameObject target; // 当前目标对象

        // 单位在地面上的当前位置
        public float groundPos;
        [HideInInspector] public Vector2 currentPosition => new Vector2(transform.position.x, groundPos); // 当前在地面上的位置
        [HideInInspector] public float lastAttackTime = 0; // 上次攻击的时间
        [HideInInspector] public ATTACKTYPE lastAttackType; // 上次攻击的类型
        [HideInInspector] public float yForce = 0; // 用于跳跃计算的垂直力
        [HideInInspector] public bool isGrounded = true; // 单位是否在地面上
        [HideInInspector] public WeaponPickup weapon; // 单位当前持有的武器
        [HideInInspector] public bool targetSpotted; // 是否至少发现过一次目标
        [HideInInspector] public List<ATTACKTYPE> attackList = new List<ATTACKTYPE>(); // 攻击类型列表

        // 组件引用
        public Animator animator => GetComponentInChildren<Animator>(); // 动画控制器
        public LF2CharacterAnimator _LF2CharacterAnimator => GetComponentInChildren<LF2CharacterAnimator>(); // LF2角色动画播放器
        public StateMachine stateMachine => GetComponent<StateMachine>(); // 状态机
        public Character _Character => GetComponent<Character>(); // 能力系统组件
        public UnitSettings settings => GetComponent<UnitSettings>(); // 单位设置
        public bool isPlayer => settings?.unitType == UNITTYPE.PLAYER; // 是否为玩家
        public bool isEnemy => settings?.unitType == UNITTYPE.ENEMY; // 是否为敌人

        // 私有变量
        private bool onApplicationQuit; // 应用程序是否正在退出
        private float currentSpeed = 0f; // 当前速度
        private float animDuration; // 动画持续时间

        // 方向相关属性
        public DIRECTION dir => transform.localRotation == Quaternion.Euler(Vector3.zero) ? DIRECTION.RIGHT : DIRECTION.LEFT; // 当前朝向
        public DIRECTION invertedDir => (DIRECTION)((int)dir * -1); // 相反方向

        // 事件委托
        public delegate void OnUnitDealDamage(GameObject recipient, AttackData attackData);
        public static event OnUnitDealDamage onUnitDealDamage; // 造成伤害事件

        /**
         * OnDestroy - 对象销毁时调用
         * 当单位被销毁时，同时销毁其阴影
         */
        void OnDestroy()
        {
            if (settings?.shadow && !onApplicationQuit) Destroy(settings.shadow);
        }

        /**
         * findClosestPlayer - 查找最近的玩家
         * @return 返回距离最近的玩家对象，如果没有玩家则返回null
         */
        public GameObject findClosestPlayer()
        {
            List<GameObject> allPlayers = GameObject.FindGameObjectsWithTag("Player").ToList();
            allPlayers = allPlayers.OrderBy(player => Vector2.Distance(this.transform.position, player.transform.position)).ToList();
            if (allPlayers.Count > 0) return allPlayers[0];
            return null;
        }

        /**
         * distanceToTarget - 计算到目标的距离
         * @return 返回到目标的x和y距离向量，如果没有目标则返回正无穷
         */
        public Vector2 distanceToTarget()
        {
            if (!target) return Vector2.positiveInfinity;
            return new Vector2(Mathf.Abs(target.transform.position.x - transform.position.x), Mathf.Abs(target.transform.position.y - transform.position.y));
        }

        /**
         * TurnToTarget - 朝向目标
         * 使单位转向面对当前目标
         */
        public void TurnToTarget()
        {
            if (!target) return;
            transform.localRotation = (target.transform.position.x < transform.position.x) ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
        }

        /**
         * TurnToDir - 朝向指定方向
         * @param dir 要转向的方向
         */
        public void TurnToDir(DIRECTION dir)
        {
            transform.localRotation = (dir == DIRECTION.LEFT) ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
        }

        /**
         * TurnToFloatDir - 根据浮点数值转向
         * @param x 正数向右，负数向左，0时不转向
         */
        public void TurnToFloatDir(float x)
        {
            if (x == 0) return;
            if (x > 0) TurnToDir(DIRECTION.RIGHT);
            else if (x < 0) TurnToDir(DIRECTION.LEFT);
        }

        /// <summary>
        /// 检查攻击是否命中目标，并处理命中后的所有效果
        /// </summary>
        /// <param name="attackData">包含攻击数据的对象，包括伤害、击倒效果等信息</param>
        /// <returns>返回是否成功造成伤害</returns>
        public bool CheckForHit(AttackData attackData)
        {
            bool damageDealt = false; // 标记是否造成伤害

            // 如果攻击来源为空，则设置为当前游戏对象
            if (attackData.inflictor == null) attackData.inflictor = gameObject;

            // 检查攻击碰撞框是否激活
            if (HitBoxActive())
            {
                // 遍历所有被击中的对象
                foreach (GameObject obj in GetObjectsHit(attackData))
                {
                    // 获取目标单位的UnitActions组件
                    UnitActions targetUnit = obj.GetComponent<UnitActions>();

                    // 如果目标正在防御，则造成伤害并跳过后续处理
                    if (targetUnit?.IsDefending(invertedDir) == true) { damageDealt = true; continue; }

                    // 检查目标是否正在被击倒的过程中



                    // 在命中位置显示击中特效
                    ShowHitEffectAtPosition(settings.hitBox.transform.position + (Vector3.right * Random.Range(0, .5f)));

                    // 获取目标的生命值系统并造成伤害
                    HealthSystem targetHealthSystem = obj.GetComponent<HealthSystem>();
                    if (attackData != null) targetHealthSystem?.SubstractHealth(attackData.damage);

                    // 播放攻击音效
                    if (attackData.sfx.Length > 0) AudioController.PlaySFX(attackData.sfx);

                    // 触发造成伤害事件
                    if (onUnitDealDamage != null) onUnitDealDamage(obj, attackData);

                    // 如果目标单位存在且在地面上
                    if (targetUnit != null && targetUnit.isGrounded)
                    {
                        // 如果目标已死亡，触发死亡状态
                        if (targetHealthSystem.isDead)
                        {
                        }
                        else
                        {
                            // 判断是否需要击倒
                            bool doAKnockdown = (attackData.knockdown && targetUnit.settings.canBeKnockedDown);
                            if (!doAKnockdown)
                            {
                                // 不需要击倒，触发受击状态

                            }
                            else
                            {
                                // 需要击倒，设置击倒力度并触发击倒状态
                                Vector2 knockDownForce = new Vector2(targetUnit.settings.knockDownDistance, targetUnit.settings.knockDownHeight);

                            }
                        }
                    }
                    damageDealt = true; // 标记已造成伤害
                }
            }
            return damageDealt; // 返回是否造成伤害
        }


        /**
         * GetObjectsHit - 获取被击中的对象列表
         * @param attackData 攻击数据
         * @return 被击中的游戏对象列表
         */
        public List<GameObject> GetObjectsHit(AttackData attackData)
        {
            // 创建可被击中对象的列表
            List<GameObject> hitableObjects = new List<GameObject>();
            // 创建最终被击中对象的列表
            List<GameObject> ObjectsHit = new List<GameObject>();

            // 如果是玩家角色
            if (isPlayer)
            {
                // 获取所有带有"Enemy"标签的对象
                hitableObjects = GameObject.FindGameObjectsWithTag("Enemy").ToList();
                // 添加所有带有"Object"标签的对象
                hitableObjects.AddRange(GameObject.FindGameObjectsWithTag("Object").ToList());
            }

            // 如果是敌人角色
            if (isEnemy)
            {
                // 判断是否处于投掷状态
                bool enemyIsBeingThrown = (attackData.attackType == ATTACKTYPE.GRABTHROW);
                // 判断是否在倒地时能对其他敌人造成伤害
                bool enemyDoesFallDamage = settings.hitOtherEnemiesWhenFalling;

                // 如果不是投掷状态，获取玩家对象
                if (!enemyIsBeingThrown) hitableObjects = GameObject.FindGameObjectsWithTag("Player").ToList();

                // 如果是投掷状态或倒地伤害开启
                if (enemyIsBeingThrown || enemyDoesFallDamage)
                {
                    // 遍历所有敌人
                    foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
                    {
                        // 判断是否是自己
                        bool enemyIsMyself = (enemy.gameObject == this.gameObject);
                        // 判断敌人是否处于倒地状态

                        // 如果不是自己且不在倒地状态，添加到可击中列表

                    }
                }
            }

            // 移除已经死亡的对象
            for (int i = hitableObjects.Count - 1; i >= 0; i--)
            {
                if (hitableObjects[i].GetComponent<HealthSystem>()?.isDead == true) hitableObjects.RemoveAt(i);
            }

            // 移除正处于受击状态的对象
            for (int i = hitableObjects.Count - 1; i >= 0; i--)
            {

            }

            // 按距离排序可击中对象列表
            hitableObjects = hitableObjects.OrderBy(obj => Vector2.Distance(transform.position, obj.transform.position)).ToList();

            // 检查每个对象是否在攻击范围内
            foreach (GameObject obj in hitableObjects)
            {
                // 获取对象的精灵渲染器
                SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                // 如果精灵渲染器存在，且与攻击盒相交，且在Z轴范围内，则添加到被击中列表
                if (sr != null && settings.hitBox.bounds.Intersects(sr.bounds) && targetInZRange(obj.gameObject, .5f)) ObjectsHit.Add(sr.gameObject);
            }

            // 返回最终被击中的对象列表
            return ObjectsHit;
        }


        /**
         * GetClosestPickup - 获取最近的拾取物
         * @param pickupRange 拾取范围
         * @return 返回在范围内的拾取物对象，如果没有则返回null
         */
        public GameObject GetClosestPickup(Vector2 pickupRange)
        {
            List<GameObject> allPickups = GameObject.FindGameObjectsWithTag("Pickup").ToList().OrderBy(pickup => Vector2.Distance(transform.position, pickup.transform.position)).ToList();
            if (allPickups.Count == 0) return null;

            foreach (var pickup in allPickups)
            {
                if (Vector2.Distance(transform.position, pickup.transform.position) <= pickupRange.magnitude) return pickup;
            }
            return null;
        }

        /**
         * targetInZRange - 检查目标是否在Z轴范围内
         * @param target 目标对象
         * @param zRange Z轴范围
         * @return 如果在范围内返回true，否则返回false
         */
        bool targetInZRange(GameObject target, float zRange)
        {
            return (Mathf.Abs(target.transform.position.y - groundPos) < zRange);
        }

        /**
         * MoveToVector - 向指定方向移动
         *
         * ⚠️ 已弃用！请直接修改 character._LF2CharacterAnimator.ps.vx/vz
         *
         * 旧版实现直接设置 Rigidbody2D.velocity，违反了 FLF 的分离原则。
         * 新架构：状态处理器设置 ps.vx/vz → ApplyDynamics() 应用到位置
         *
         * @param moveDir 移动方向（离散值 -1/0/1，不需要归一化）
         * @param speedX X轴速度（水平移动速度，FLF单位：像素/帧）
         * @param speedZ Z轴速度（纵深移动速度，FLF单位：像素/帧），默认为 speedX * 0.7f
         */
        [System.Obsolete("请直接修改 character._LF2CharacterAnimator.ps.vx/vz，而不是调用 MoveToVector。参考 CharacterStates.cs 的 Standing 状态实现。")]
        public void MoveToVector(Vector2 moveDir, float speedX, float speedZ = -1f)
        {
            // FLF 常量（用于速度单位转换）
            const float FLF_FRAMERATE = 30f;           // FLF 运行在 30fps
            const float PIXELS_PER_UNIT = 100f;        // Unity PPU 设置
            const float SPEED_CONVERSION = FLF_FRAMERATE / PIXELS_PER_UNIT;  // = 0.3

            if (isGrounded) groundPos = transform.position.y;

            // 如果未指定 speedZ，默认为 speedX 的 70%（FLF 原版常见比例）
            if (speedZ < 0) speedZ = speedX * 0.7f;

            // FLF 行为：使用离散方向（dx/dz）
            int dx = Mathf.Abs(moveDir.x) > 0.1f ? (moveDir.x > 0f ? 1 : -1) : 0;
            int dz = Mathf.Abs(moveDir.y) > 0.1f ? (moveDir.y > 0f ? 1 : -1) : 0;

            // 对角移动时降低 x 速度（FLF：2/7）
            // character.js Line 367-368: var xfactor = 1 - (dz ? 1 : 0) * (2/7);
            float xFactor = (dz != 0) ? (1f - (2f / 7f)) : 1f;

            // FLF 逻辑：立即设置速度（不使用加速度系统）
            // character.js Line 377-378:
            // $.ps.vx = xfactor * dx * $.data.bmp.walking_speed;（像素/帧）
            // $.ps.vz = dz * $.data.bmp.walking_speedz;
            float vx_flf = dx * speedX * xFactor;  // FLF 速度：像素/帧
            float vz_flf = dz * speedZ;

            // ✅ 转换为 Unity 速度（单位/秒）
            float vx_unity = vx_flf * SPEED_CONVERSION;
            float vz_unity = vz_flf * SPEED_CONVERSION;
          
            // 更新朝向
            if (dx != 0)
            {
                TurnToDir((moveDir.x > 0) ? DIRECTION.RIGHT : DIRECTION.LEFT);
                //_Character._CharacterDirection = (moveDir.x > 0) ? DIRECTION.RIGHT : DIRECTION.LEFT;
            }
        }

        /**
         * WallDetected - 检测墙壁
         * @param dir 检测方向
         * @return 如果检测到墙壁返回true，否则返回false
         */
        public bool WallDetected(Vector2 dir)
        {
            RaycastHit2D hit = Physics2D.Linecast(currentPosition, currentPosition + dir, 1 << LayerMask.NameToLayer("Environment"));
            Debug.DrawRay(currentPosition, dir, Color.yellow, Time.deltaTime);
            return (hit.collider != null);
        }

        /**
         * AddForce - 添加冲击力
         * @param force 冲击力大小
         */
        public void AddForce(float force)
        {
            StartCoroutine(AddForceRoutine(force, .25f));
        }

        /**
         * AddForceRoutine - 添加冲击力协程
         * @param force 冲击力大小
         * @param duration 持续时间
         */
        private IEnumerator AddForceRoutine(float force, float duration)
        {
            Vector2 startPos = transform.position;
            Vector2 endPos = startPos + Vector2.right * (int)dir * force;
            float t = 0;
            while (t < 1)
            {
                transform.position = Vector2.Lerp(startPos, endPos, MathUtilities.Sinerp(t));
                t += Time.deltaTime / duration;
                yield return 0;
            }
            transform.position = endPos;
        }

        /**
         * JumpSequence - 跳跃序列
         * 处理跳跃过程中的移动逻辑
         */
        public void JumpSequence(Vector2 direction = default)
        {
            Vector2 moveVector = transform.position;

            float inputVectorX = direction.Equals(default) ? InputManager.GetInputVector(_Character._CharacterInput.MoveAction).x : direction.x;
            float inputVectorY = direction.Equals(default) ? InputManager.GetInputVector(_Character._CharacterInput.MoveAction).y : direction.y;

            Vector2 inputVector = direction.Equals(default) ? InputManager.GetInputVector(_Character._CharacterInput.MoveAction).normalized : direction.normalized;

            Debug.LogFormat("JumpSequence: {0}", inputVector);

            if (inputVectorX != 0) TurnToDir(inputVectorX > 0 ? DIRECTION.RIGHT : DIRECTION.LEFT);
            moveVector.x = transform.position.x + (inputVectorX * settings.moveSpeedAir * Time.fixedDeltaTime);

            bool CheckWall = false;
            Vector2 wallDistanceCheck = Vector2.one * .3f; // 除以1.6是为了使检测距离略大于碰撞体，否则无法检测到墙壁
            CheckWall = WallDetected(inputVector * wallDistanceCheck);

            // 添加垂直移动控制
            if (inputVectorY != 0 && !CheckWall)
            {
                // 计算新的地面位置
                float newGroundPos = groundPos + (inputVectorY * settings.verticalMoveSpeed * Time.fixedDeltaTime);

                // 限制地面位置在允许范围内
                newGroundPos = Mathf.Clamp(newGroundPos, settings.minGroundHeight, settings.maxGroundHeight);

                // 更新地面位置
                groundPos = newGroundPos;
            }

            moveVector.y += yForce * Time.fixedDeltaTime * settings.jumpSpeed;
            yForce -= settings.jumpGravity * Time.fixedDeltaTime * settings.jumpSpeed;

            transform.position = moveVector;
        }

        /**
         * StopMoving - 停止移动
         * @param stopInstantly 是否立即停止
         */
        public void StopMoving(bool stopInstantly = true)
        {
            if (isGrounded) groundPos = transform.position.y;
            if (!settings.useAcceleration) stopInstantly = true;

            if (stopInstantly)
            {
                currentSpeed = 0;
                return;
            }

           
        }

        /**
         * IsDefending - 检查是否正在防御
         * @param attackDir 攻击方向
         * @return 如果正在防御返回true，否则返回false
         */
        public bool IsDefending(DIRECTION attackDir)
        {
            if (settings.rearDefenseEnabled) return true;
            if (dir == attackDir) return true;
            return false;
        }

        /**
         * HitBoxActive - 检查攻击框是否激活
         * @return 如果攻击框激活返回true，否则返回false
         */
        public bool HitBoxActive()
        {
            if (settings.hitBox == null) return false;
            return settings.hitBox.gameObject.activeSelf;
        }

        /**
         * ShowHitEffectAtPosition - 在指定位置显示击中效果
         * @param pos 显示位置
         */
        public void ShowHitEffectAtPosition(Vector2 pos)
        {
            if (!settings.hitEffect) return;
            GameObject effect = GameObject.Instantiate(settings.hitEffect, (Vector3)pos, Quaternion.identity) as GameObject;
        }

        /**
         * PlaySFX - 播放音效
         * @param sfx 音效名称
         */
        public void PlaySFX(string sfx)
        {
            BeatEmUpTemplate2D.AudioController.PlaySFX(sfx, transform.position);
        }

        /**
         * Footstep - 播放脚步声
         * 根据单位所在地面类型播放相应的脚步声音效
         */
        public void Footstep()
        {
            Collider2D[] overlappedColliders = Physics2D.OverlapPointAll(transform.position);
            foreach (Collider2D col2D in overlappedColliders)
            {
                Surface surface = col2D.GetComponent<Surface>();
                if (surface && surface.footstepSFX.Length > 0)
                {
                    AudioController.PlaySFX(surface.footstepSFX, transform.position);
                    return;
                }
            }
            AudioController.PlaySFX("FootstepDefault", transform.position);
        }

        /**
         * ShowEffect - 显示特效
         * @param effectName 特效名称（从Resources文件夹加载）
         */
        public void ShowEffect(string effectName)
        {
            GameObject effect = GameObject.Instantiate(Resources.Load(effectName), transform.position, Quaternion.identity) as GameObject;

            ParticleSystem[] allParticleSystems = effect?.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in allParticleSystems) ObjectSorting.Sort(ps?.GetComponent<Renderer>(), new Vector2(transform.position.x, transform.position.y));

            if (effect) Destroy(effect, 3f);
        }

        /**
         * SpawnProjectile - 生成投射物
         * @param objName 投射物对象名称（从Resources文件夹加载）
         */
        public void SpawnProjectile(string objName)
        {
            WeaponAttachment wa = GetComponentInChildren<WeaponAttachment>();
            Vector2 spawnPos = (wa != null) ? wa.transform.position : transform.position;

            GameObject projectile = GameObject.Instantiate(Resources.Load(objName), spawnPos, Quaternion.identity) as GameObject;
            if (!projectile) return;

            Projectile projectileComponent = projectile.GetComponent<Projectile>();
            if (projectileComponent == null) return;
            projectileComponent.dir = dir;
        }

        /**
         * CamShake - 触发相机震动效果
         */
        public void CamShake()
        {
            Camera.main?.GetComponent<CameraShake>()?.ShowCamShake();
        }

        /**
         * targetInSight - 检查目标是否在视野内
         * @return 如果目标在视野内返回true，否则返回false
         */
        public bool targetInSight()
        {
            if (!target || !settings) return false;
            if (!settings.enableFOV) { targetSpotted = true; return true; }

            Vector2 directionToTarget = target.transform.position - transform.position + (Vector3)settings.viewPosOffset;
            float distanceToTarget = directionToTarget.magnitude;
            if (distanceToTarget > settings.viewDistance) return false;

            SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
            if (sr == null) return false;

            Bounds spriteBounds = sr.bounds;
            Vector3[] corners = {
            spriteBounds.min,
            spriteBounds.max,
            new Vector3(spriteBounds.min.x, spriteBounds.max.y),
            new Vector3(spriteBounds.max.x, spriteBounds.min.y)
        };

            foreach (Vector3 corner in corners)
            {
                Vector2 directionToCorner = corner - (transform.position + (Vector3)settings.viewPosOffset);
                float distanceToCorner = directionToCorner.magnitude;
                if (distanceToCorner <= settings.viewDistance)
                {
                    float angleToCorner = Vector2.Angle(transform.right, directionToCorner);
                    if (angleToCorner <= settings.viewAngle / 2)
                    {
                        targetSpotted = true;
                        return true;
                    }
                }
            }
            return false;
        }

        /**
         * OnDrawGizmos - 在编辑器中绘制Gizmos
         * 当选中此GameObject时，在场景视图中绘制敌人的视野锥
         */
        void OnDrawGizmos()
        {
            if (settings == null || !settings.showFOVCone || settings.viewDistance <= 0) return;

            int lineSegments = settings.viewAngle > 180 ? 40 : 20;
            Gizmos.color = Color.red;
            Vector3 viewOffset = new Vector3(settings.viewPosOffset.x * (int)dir, settings.viewPosOffset.y, 0);

            Vector3 forward = transform.right;
            Vector3 leftBoundary = Quaternion.Euler(0, 0, settings.viewAngle / 2) * forward * settings.viewDistance + viewOffset;
            Vector3 rightBoundary = Quaternion.Euler(0, 0, -settings.viewAngle / 2) * forward * settings.viewDistance + viewOffset;

            Vector3 previousPoint = transform.position + rightBoundary;
            for (int i = 0; i <= lineSegments; i++)
            {
                float angle = -settings.viewAngle / 2 + (settings.viewAngle / lineSegments) * i;
                Vector3 nextPoint = Quaternion.Euler(0, 0, angle) * forward * settings.viewDistance;
                nextPoint = transform.position + nextPoint + viewOffset;

                Gizmos.DrawLine(previousPoint, nextPoint);
                previousPoint = nextPoint;
            }

            Gizmos.DrawLine(transform.position + viewOffset, transform.position + leftBoundary);
            Gizmos.DrawLine(transform.position + viewOffset, transform.position + rightBoundary);
        }

        /**
         * OnApplicationQuit - 应用程序退出时调用
         */
        void OnApplicationQuit()
        {
            onApplicationQuit = true;
        }
    }

    /**
     * DIRECTION 枚举 - 方向
     * 定义了游戏中使用的两个基本方向：左和右
     */
    public enum DIRECTION
    {
        None = 0,  // 无方向
        LEFT = -1,  // 左方向
        RIGHT = 1,  // 右方向
    }

}
