using UnityEngine;
using System.Collections.Generic;
using MoreMountains.TopDownEngine;
using NTSD.Animation;
using UnityEngine.Rendering;

namespace BeatEmUpTemplate2D {

    public enum UNITTYPE { PLAYER = 0, ENEMY = 10, NPC = 20 }

    [System.Serializable]
    public class UnitSettings : MonoBehaviour, ICharacterModule {

        public UNITTYPE unitType = UNITTYPE.PLAYER;

       //LINKED OBJECTS - 关联对象
        [Tooltip("阴影预制体")] public GameObject shadowPrefab; //shadow prefab - 阴影预制体
        [Tooltip("跟随此单位的阴影")] public GameObject shadow; //shadow that follows this unit - 跟随此单位的阴影
        [Tooltip("武器附加物的位置")] public GameObject weaponBone; //position for weapon attachments - 武器附加物的位置
        [Tooltip("击中物体时播放的效果")] public GameObject hitEffect; //effect that gets played when we've hit something - 击中物体时播放的效果
        [Tooltip("用于碰撞检测的精灵边界框")] public SpriteRenderer hitBox; //sprite bounding box used for hit collision - 用于碰撞检测的精灵边界框
        [Tooltip("此单位的精灵渲染器")] public SpriteRenderer spriteRenderer; //this unit's sprite renderer - 此单位的精灵渲染器
        
        //MOVEMENT SETTINGS - 移动设置
        [Tooltip("初始方向")] public DIRECTION startDirection = DIRECTION.RIGHT; //start direction - 初始方向
        [Tooltip("地面上的移动速度")] public float moveSpeed = 4; //move speed while on the ground - 地面上的移动速度
        [Tooltip("空中的移动速度")] public float moveSpeedAir = 4; //moving speed while in the air - 空中的移动速度
        [Tooltip("为true时使用加速移动，为false时瞬间移动")] public bool useAcceleration = false; //use acceleration over time if true, or move instantly when false - 为true时使用加速移动，为false时瞬间移动
        
        //ACCELERATION / DECELERATION - 加速/减速设置
        [Tooltip("获得速度的快慢（加速度）")] public float moveAcceleration = 25f; //how fast we gain speed - 获得速度的快慢（加速度）
        [Tooltip("失去速度的快慢（减速度）")] public float moveDeceleration = 10f; //how fast we lose speed - 失去速度的快慢（减速度）
        
        //JUMP SETTINGS - 跳跃设置
        [Tooltip("单位能跳多高")] public float jumpHeight = 4; //how high this unit can jump - 单位能跳多高
        [Tooltip("跳跃模拟的速度")] public float jumpSpeed = 3.5f; //how fast the jump is simulated - 跳跃模拟的速度
        [Tooltip("向下的力（重力）")] public float jumpGravity = 5f; //the downward force - 向下的力（重力）
        [Tooltip("垂直移动速度")] public float verticalMoveSpeed = 3f; // 垂直移动速度
        [Tooltip("最小地面高度")] public float minGroundHeight = -5f; // 最小地面高度
        [Tooltip("最大地面高度")] public float maxGroundHeight = 5f; // 最大地面高度



        //ATTACK DATA - 攻击数据区域
        [Space(10)]  // 在Unity编辑器中创建10像素的垂直间距，用于界面布局优化
        [Help("* Only PUNCH and KICK Attack Types can be used in combos")]  // 在Unity编辑器中显示帮助信息
        // 提示：只有拳(PUNCH)和踢(KICK)攻击类型才能用于连招系统
        public List<Combo> comboData = new List<Combo>();  // 存储所有连招数据的列表
        // 每个Combo对象包含一个完整的连招序列信息
        public float comboResetTime = .55f;  // 连招重置时间
        // 当超过这个时间(0.55秒)没有输入新的攻击指令时，连招序列将会被重置
        public bool continueComboOnHit;  // 是否只在命中目标时继续连招
        // 如果为true，只有当前一个攻击命中目标时才能继续连招；
        // 如果为false，则可以连续执行连招动作而不需要考虑是否命中

        [Space(10)]
        public AttackData jumpPunch;
        public AttackData jumpKick;
        [Space(10)]
        public AttackData grabPunch;
        public AttackData grabKick;
        public AttackData grabThrow;
        [Space(10)]
        public AttackData groundPunch;
        public AttackData groundKick;

        //敌人攻击数据
        /* 存储敌人所有攻击信息的列表 */
        public List<AttackData> enemyAttackList = new List<AttackData>(); //敌人攻击列表

        //击倒设置
        public bool canBeKnockedDown = true; //该单位是否可以被击倒
        public float knockDownHeight = 3; //击倒时单位在空中飞行的高度
        public float knockDownDistance = 3; //水平移动距离
        public float knockDownSpeed = 3; //击倒动画的模拟速度
        public float knockDownFloorTime = 1; //单位倒地后站立前在地面上停留的时间
        public bool hitOtherEnemiesWhenFalling = false; //倒下时是否可以撞击其他敌人

        //投掷设置
        public float throwHeight = 3; //被投掷时单位飞行的高度
        public float throwDistance = 5; //被投掷的距离
        public bool hitOtherEnemiesWhenThrown = true; //被玩家投掷时是否可以撞击其他敌人

        //防御设置
        /* 以下是关于敌人防御机制的相关参数 */
        public float defendChance; //敌人防御攻击的几率（百分比）
        public float defendDuration; //敌人保持防御状态的持续时间
        public bool canChangeDirWhileDefending; //是否允许在防御时改变方向
        public bool rearDefenseEnabled; //防御时是否能够防御来自背后的攻击

    
        //================= 抓取设置 =================
        public bool canBeGrabbed = true;              // 是否允许被抓取
        public string grabAnimation = "Grab";         // 抓取时播放的动画名称
        public Vector2 grabPosition = new Vector2(0.93f, 0);  // 抓取位置坐标
        public float grabDuration = 3f;               // 抓取持续时间

        //================= 装备武器设置 =================
        public bool loseWeaponWhenHit = true;         // 被击中时是否掉落武器
        public bool loseWeaponWhenKnockedDown = true; // 被击倒时是否掉落武器

        /*================= 单位名称和肖像设置 =================
          此部分定义了单位的显示名称、名称样式、肖像图片以及
          是否从预设列表中随机加载名称等功能*/
        public string unitName = "";                  // 单位名称
        public bool showNameInAllCaps;                // 是否全大写显示名称
        public Sprite unitPortrait;                   // 单位肖像（显示在血条旁的小图片）
        public bool loadRandomNameFromList;           // 是否从文本文件随机加载名称
        public TextAsset unitNamesList;               // 名称列表（.txt文件）

        //================= 敌人设置 =================
        public float enemyPauseBeforeAttack = .3f;    // 敌人攻击前的等待时间

        /*================= 视野设置 =================
          此部分定义了单位的视野系统，包括视野距离、角度、
          位置偏移以及是否在编辑器中显示视野范围等参数*/
        public bool enableFOV;                        // 是否启用视野系统（不启用时默认始终可见目标）
        public float viewDistance = 5f;               // 最大可视距离
        public float viewAngle = 45f;                 // 视野角度
        public Vector2 viewPosOffset;                 // 视野锥形位置偏移（眼睛高度）
        public bool showFOVCone;                      // 是否在Unity编辑器中显示视野锥形
        [ReadOnlyProperty] public bool targetInSight; // 目标是否在视野范围内（只读属性）

        // Step 4: 缓存 Character hub（替代 UnitActions）
        private Character _character;

        public void ModuleSetup(Character character)
        {
            _character = character;
        }

        public void ModuleInitialize()
        {
            Start_Legacy();
        }

        public void ModuleBind() { }

        public void ModuleUnbind() { }

        private void Awake() { }

        private void Start() { }

        private void Start_Legacy() {

            /**
             * 初始化单位组件的设置部分
             * 包括创建阴影、隐藏碰撞箱、检查精灵渲染器和加载名称等功能
             */

            // 创建阴影对象
            // 检查阴影对象不存在且阴影预制体存在
            if(!shadow && shadowPrefab) 
            {
                shadow = GameObject.Instantiate(shadowPrefab, transform.parent) as GameObject;
                Debug.Log($"[UnitSettings] Shadow created: {shadow?.name}, parent={transform.parent?.name}");
            }
            else
            {
                Debug.Log($"[UnitSettings] Shadow skip: shadow={shadow != null}, shadowPrefab={shadowPrefab != null}");
            }

            // 在开始时隐藏碰撞箱
            if(hitBox) 
                // 如果存在碰撞箱，将其颜色设置为透明
                hitBox.color = Color.clear;
            else 
                // 如果不存在碰撞箱，输出错误信息提示用户分配碰撞箱
                Debug.LogError("Please assign a HitBox to GameObject "+ gameObject.name + " in UnitSettings/Linked Components");
            
            // 检查精灵渲染器组件
            if(spriteRenderer == null) 
                // 如果精灵渲染器未分配，输出日志提示用户分配
                Debug.Log("Please assign a SpriteRenderer to GameObject "+ gameObject.name + " in UnitSettings/Linked Components");

            // 加载单位名称
            if(loadRandomNameFromList) 
                // 如果设置为从列表中随机加载名称，则调用方法获取随机名称
                unitName = GetRandomName();

        }

        void Update() {

            // 在Unity编辑器中显示碰撞框的调试信息
            // 使用红色矩形绘制当前单位的碰撞框边界，便于在场景视图中观察和调试
            if(hitBox && hitBox.gameObject.activeSelf) MathUtilities.DrawRectGizmo(hitBox.bounds.center, hitBox.bounds.size, Color.red, Time.deltaTime);
        
            // Step 4: 控制阴影跟随单位移动（使用 Character.GroundWorldY 替代 unitActions.groundPos）
            if(shadow){
                float groundY = _character != null ? _character.GroundWorldY : transform.position.y;
                shadow.transform.position = new Vector3(transform.position.x, groundY, 0);
                
                // Step 4: 优先使用 SortingGroup，fallback 到 spriteRenderer
                int baseSortingOrder = 0;
                if (_character != null)
                {
                    var sortingGroup = _character.GetComponent<SortingGroup>();
                    if (sortingGroup != null)
                    {
                        baseSortingOrder = sortingGroup.sortingOrder;
                    }
                    else if (spriteRenderer != null)
                    {
                        baseSortingOrder = spriteRenderer.sortingOrder;
                    }
                }
                else if (spriteRenderer != null)
                {
                    baseSortingOrder = spriteRenderer.sortingOrder;
                }
                
                var shadowSr = shadow.GetComponent<SpriteRenderer>();
                if (shadowSr != null)
                {
                    shadowSr.sortingOrder = baseSortingOrder - 1;
                }
            }
        
        }

        /// <summary>
        /// 从预设的名称列表中随机获取一个名称
        /// 该方法会从unitNamesList文本文件中读取名称列表，并随机返回一个名称
        /// </summary>
        /// <returns>返回随机选择的名称字符串，如果出错则返回空字符串</returns>
	    string GetRandomName(){
		    // 检查是否存在名称列表文件
		    if(unitNamesList == null) {
			    // 如果未找到名称列表，输出错误日志
			    Debug.Log("no list of unit names was found, please create a .txt file with names on each line, and link it in the unitSettings component.");
			    return "";
		    }

		    // 将文本文件的内容转换为字符串
		    string data = unitNamesList.ToString();
		    // 定义可能的换行符组合（处理不同操作系统的换行符）
		    string cReturns = System.Environment.NewLine + "\n" + "\r"; 
		    // 根据换行符分割文本内容为字符串数组
		    string[] lines = data.Split(cReturns.ToCharArray());

		    // 初始化名称变量和计数器
		    string name = "";
		    int cnt = 0;
		    // 尝试最多100次获取一个非空名称
		    while(name.Length == 0 && cnt < 100){
			    // 生成一个随机索引
			    int rand = Random.Range(0, lines.Length);
			    // 从数组中获取随机名称
			    name = lines[rand];
			    cnt += 1;
		    }
		    return name;
	    }

        /// <summary>
        /// 在Unity编辑器中验证组件时调用的方法
        /// 用于在编辑器中实时显示起始方向的变化
        /// </summary>
        private void OnValidate() {
             // 根据起始方向设置对象的旋转
             // 如果起始方向是向左，则旋转180度；否则保持默认方向
             transform.localRotation = (startDirection == DIRECTION.LEFT)? Quaternion.Euler(0,180,0) : Quaternion.identity;
        }

    }
}
